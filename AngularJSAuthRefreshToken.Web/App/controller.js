/* Controllers */
(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('EACtrl', ['$scope', '$state', '$log', 'ea.authorization', 'ea.dialogs', 'ea.toastr',
          function ($scope, $state, $log, authorization, dialogs, toastr) {

              this.leftMenu = [{
                  state: 'test',
                  description: 'Test',
              }, {
                  description: 'Admin',
                  items: [{
                      state: 'admin.users',
                      description: 'Users',
                  }, {
                      state: 'admin.roles',
                      description: 'Roles',
                  }]
              }];

              this.rightMenu = [{
                  state: 'account.login',
                  description: 'LogIn',
                  visible: function () {
                      return !authorization.authentication.isAuthenticated();
                  }
              }, {
                  description: function () {
                      return authorization.authentication.userName();
                  },
                  items: [{
                      state: 'account.profile',
                      description: 'Profile'
                  }, {
                      state: 'account.logout({ location: false })',
                      description: 'LogOut'
                  }]
              }];//

              $scope.$on('ea:authentication:login', function (event, defered) {
                  dialogs.authentication.logIn(defered);
              });

              $scope.$on('ea:forbidden', function (event, context) {
                  toastr.error((context.data && context.data.message) || context.statusText);
              });

              $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
                  if (!authorization.state(toState)) {
                      event.preventDefault();
                      $state.go('unauthorized');
                  }
              });

              $scope.$on('$stateChangeError', function (event, toState, toParams, fromState, fromParams, error) {
                  $log.error(error, toState, toParams, fromState, fromParams);
              });
          }]);

})(window.angular);