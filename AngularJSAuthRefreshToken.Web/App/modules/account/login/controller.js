(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('LoginCtrl', ['$scope', '$state', 'bk.utils', 'ea.authentication', 'ea.dialogs', 'externalLogins',
          function ($scope, $state, utils, authentication, dialogs, externalLogins) {
              var ctrl = this;

              utils.errorContext.init(this);// инициализира контекста за показване на грешка при запис

              this.modal = $scope.$dismiss !== undefined;//call as modal dialog
              this.externalLogins = externalLogins;
              this.model = {
                  userName: undefined,
                  password: undefined,
              };

              this.submitForm = function () {
                  if (this.form.$valid) {
                      authentication.logIn(this.model)
                          .then(function () {
                              if (ctrl.modal) {
                                  $scope.$close(true);
                              }
                          }, function (error) {
                              utils.errorContext.set(ctrl, error.error_description);
                          });
                  }
              };

              this.externalLogin = function (external) {
                  authentication.externalLogin(external);
              };

              $scope.$on('ea:authentication:token', function (event, service) {
                  if (service.isAuthenticated()) {
                      if (ctrl.modal) {
                          $scope.$close(true);
                      }
                      else {
                          $state.go('home', { congratulation: 'on' });
                      }
                  }
              });

              $scope.$on('ea:authentication:externallogin', function (event, token) {
                  // trying to in with public (Google,Facebook) profile but it is not paired with local user
                  var params = {
                      loginProvider: token.provider,
                      token: token.access_token,
                      userName: token.userName,
                      email: token.email
                  };

                  $state.go('account.externalloginconfirmation', params, { location: false });
              });

              $scope.$on('ea:authentication:externallogin:error', function (event, data) {
                  dialogs.authentication.error(data);
              });
          }]);

})(window, window.angular);