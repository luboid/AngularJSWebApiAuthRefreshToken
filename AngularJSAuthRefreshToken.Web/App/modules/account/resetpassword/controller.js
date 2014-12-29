﻿(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('ResetPasswordCtrl', ['$state', '$location', 'ea.resources', 'bk.utils', 'ea.data', function ($state, $location, resources, utils, data) {
          var ctrl = this,
              params = utils.decodeFromUrl($location.hash());

          utils.errorContext.init(this);// инициализира контекста за показване на грешка при запис

          // here will be stored server error messages to be connected with inputs
          this.messages = {
              common: {
                  required: 'Required.',
                  email: 'Invalid email address.'
              }
          };

          this.model = {
              userId: params.uid,
              code: params.cid,
              password: undefined,
              confirmPassword: undefined
          };

          this.submitForm = function () {
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