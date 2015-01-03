/* Controllers */
(function (angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('UserInfoCtrl', ['$scope','info',
          function ($scope, info) {
              $scope.info = info;
          }]);

})(window.angular);