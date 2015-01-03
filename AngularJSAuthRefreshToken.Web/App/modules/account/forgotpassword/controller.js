(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('ForgotPasswordCtrl', ['ea.resources', 'bk.utils', 'ea.urlFactory', 'ea.data', function (resources, utils, urlFactory, data) {
          var ctrl = this;

          utils.errorContext.init(this);
          utils.infoContext.init(this);

          // here will be stored server error messages to be connected with inputs
          this.messages = {
              common: angular.extend({}, resources.validation.common, {
                  email: 'Invalid user name (email).'
              })
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
                              utils.infoContext.set(ctrl, 'You will receive email with instructions how to change yours password.');
                          }
                          else {
                              // Don't reveal that the user does not exist or is not confirmed
                              utils.errorContext.set(ctrl, 'Can\'t execute request.');
                          }
                      }, function (context) {
                          utils.errorContext.setMessageFromHttpContext(ctrl, context, {
                              form: ctrl.form,
                              messages: ctrl.messages
                          });
                      });
              }

          };
      }]);

})(window.angular);