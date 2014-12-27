(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('ResetPasswordCtrl', ['$state', '$stateParams', 'ea.resources', 'bk.utils', 'ea.data', function ($state, $stateParams, resources, utils, data) {
          var ctrl = this;

          utils.errorContext.init(this);// инициализира контекста за показване на грешка при запис

          // here will be stored server error messages to be connected with inputs
          this.messages = {
              common: {
                  required: 'Required.',
                  email: 'Invalid email address.'
              }
          };

          this.model = {
              userId: $stateParams.uid,
              code: $stateParams.cid,
              password: undefined,
              confirmPassword: undefined
          };

          this.submitForm = function () {
              debugger;
              if (this.form.$valid) {
                  data.account.resetPassword(this.model)
                      .then(function () {
                          $state.go('account.login');
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