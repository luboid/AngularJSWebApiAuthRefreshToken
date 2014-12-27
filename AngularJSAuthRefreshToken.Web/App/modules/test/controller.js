(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('TestCtrl', ['$stateParams', 'ea.data', function ($stateParams, data) {
          var ctrl = this;
          this.data = [];
          this.call = function () {

              data.test.post({ id: 'testId' }).then(function (data) {
                  debugger;
                  ctrl.data = data;
              }, function () {
                  debugger;
              });
          }

      }]);

})(window, window.angular);