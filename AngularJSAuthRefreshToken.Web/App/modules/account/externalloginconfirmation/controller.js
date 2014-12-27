(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('ExternalLoginConfirmationCtrl', ['$scope', '$state', '$stateParams', 'bk.utils', 'ea.urlFactory', 'ea.resources', 'ea.authentication',
          function ($scope, $state, $stateParams, utils, urlFactory, resources, authentication) {
              var ctrl = this;

              utils.errorContext.init(this);

              this.emailConfirmation = false;

              this.model = {
                  loginProvider: $stateParams.loginProvider,
                  email: $stateParams.email,
                  applicationName: resources.applicationName,
                  applicationLocation: utils.confirmEmailUrl()
              };

              this.submitForm = function () {
                  if (this.form.$valid) {
                      authentication.registerExternal($stateParams.token, this.model)
                          .then(function (emailConfirmation) {
                              ctrl.emailConfirmation = emailConfirmation;
                          }, function (error) {
                              utils.errorContext.setFailHttpContext(ctrl, error);
                          });
                  }
              };

              $scope.$on('ea:authentication:token', function (event, service) {
                  if (service.isAuthenticated()) {
                      $state.go('home', { congratulation: 'on' });
                  }
              });
          }]);

})(window, window.angular);