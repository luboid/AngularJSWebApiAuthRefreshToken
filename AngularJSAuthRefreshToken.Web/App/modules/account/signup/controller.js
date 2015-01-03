(function () {
    'use strict';

    angular.module('ea.controllers')
      .controller('SignupCtrl', ['$scope', '$state', 'bk.utils', 'ea.urlFactory', 'ea.resources', 'ea.data', 'ea.authentication',
          function ($scope, $state, utils, urlFactory, resources, data, authentication) {
              var ctrl = this;

              utils.errorContext.init(this);

              function showErrors(state) {
                  utils.errorContext.setMessageFromHttpContext(ctrl, state, {
                      form: ctrl.form,
                      messages: ctrl.messages
                  });
              }

              this.emailConfirmation = false;
              this.registration = true;

              this.messages = {
                  common: {
                      required: 'Required.',
                      email: 'Invalid user name (email).'
                  }
              };

              this.model = {
                  email: undefined,
                  password: undefined,
                  confirmPassword: undefined,
                  applicationName: resources.applicationName,
                  applicationLocation: urlFactory.confirmEmailUrl()
              };

              this.submitForm = function () {
                  if (this.form.$valid) {
                      data.account.register(this.model)
                          .then(function (emailConfirmation) {
                              ctrl.emailConfirmation = emailConfirmation;
                              ctrl.registration = false;

                              if (!ctrl.emailConfirmation) {
                                  authentication.logIn({
                                      userName: ctrl.model.email,
                                      password: ctrl.model.password
                                  })
                                  .catch(showErrors);
                              }

                          }, showErrors);
                  }
              };

              $scope.$on('ea:authentication:token', function (event, service) {
                  if (service.isAuthenticated()) {
                      $state.go('home', { congratulation: 'on' });
                  }
              });
          }]);

})(window.angular);