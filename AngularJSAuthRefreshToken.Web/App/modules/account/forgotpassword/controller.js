(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('ForgotPasswordCtrl', ['ea.resources', 'bk.utils', 'ea.urlFactory', 'ea.data', function (resources, utils, urlFactory, data) {
          var ctrl = this;

          utils.errorContext.init(this);// инициализира контекста за показване на грешка при запис
          utils.infoContext.init(this);

          // here will be stored server error messages to be connected with inputs
          this.messages = {
              common: {
                  required: 'Required.',
                  email: 'Invalid email address.'
              }
          };

          this.model = {
              email: undefined,
              applicationName: resources.applicationName,
              applicationLocation: urlFactory.resetPasswordUrl()
          };

          this.submitForm = function () {
              if (this.form.$valid) {
                  data.account.forgotPassword(this.model)
                      .then(function (success) {
                          if (success) {
                              ctrl.model.email = undefined;
                              ctrl.form.$setPristine();
                              utils.infoContext.set(ctrl, 'You will receive email to confirm.');
                          }
                          else {
                              // Don't reveal that the user does not exist or is not confirmed
                              utils.errorContext.set(ctrl, 'You request can\'t be executed.');
                          }
                      }, function (context) {
                          utils.errorContext.setFailHttpContext(ctrl, context, {
                              form: ctrl.form,
                              messages: ctrl.messages
                          });
                      });
              }

          };
      }]);

})(window.angular);