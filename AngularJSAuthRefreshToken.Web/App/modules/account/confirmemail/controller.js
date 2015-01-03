(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('ConfirmeMailCtrl', ['$state', 'bk.utils', 'confirmEmail', function ($state, utils, confirmEmail) {
          var ctrl = this;

          utils.errorContext.init(this);// инициализира контекста за показване на грешка при запис
          
          this.success = typeof (confirmEmail) == 'boolean';
          if (!this.success) {
              utils.errorContext.setMessageFromHttpContext(ctrl, confirmEmail);
          }

      }]);

})(window, window.angular);