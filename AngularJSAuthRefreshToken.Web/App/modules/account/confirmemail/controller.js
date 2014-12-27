(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('ConfirmeMailCtrl', ['$state', 'bk.utils', 'confirmEmail', function ($state, utils, confirmEmail) {
          var ctrl = this;

          utils.errorContext.init(this);// initialize error showing context
          
          this.success = typeof (confirmEmail) == 'boolean';
          if (!this.success) {
              utils.errorContext.setFailHttpContext(ctrl, confirmEmail);
          }

      }]);

})(window, window.angular);