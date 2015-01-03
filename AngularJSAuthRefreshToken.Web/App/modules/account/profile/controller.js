(function (window, angular) {
    'use strict';

    var localProvider = 'Local';

    angular.module('ea.controllers')
      .controller('ProfileCtrl', ['$scope', 'ea.toastr', 'ea.resources', 'ea.data', 'bk.utils', 'ea.authentication', 'externalLogins', 'profile',
          function ($scope, toastr, resources, data, utils, authentication, externalLogins, profile) {
              var ctrl = this;

              this.addPasswordDisabled = false;
              this.activeLogins = null;
              this.othersLogins = null;
              this.messages = {
                  common: {
                      required: 'Required.'
                  }
              };

              this.addPassword = {
                  form: undefined,
                  model: {
                      newPassword: undefined,
                      confirmPassword: undefined
                  },
                  clear: function () {
                      this.model.newPassword = undefined;
                      this.model.confirmPassword = undefined;
                      this.form.$setPristine();
                  },
                  hide: function () {
                      this.open = false;
                  },
                  submitForm: function () {
                      var formContext = this;
                      if (formContext.form.$valid) {
                          data.account.setPassword(formContext.model)
                              .then(function () {
                                  addLocalLogin();
                                  formContext.clear();
                                  utils.infoContext.set(formContext, 'Password was set.');
                              },
                              function (context) {
                                  utils.errorContext.setMessageFromHttpContext(formContext, context, {
                                      form: formContext.form,
                                      messages: ctrl.messages
                                  });
                              });
                      }
                  },
                  initialize: function () {
                      utils.errorContext.init(this);
                      utils.infoContext.init(this);
                  }
              };

              this.changePassword = {
                  form: undefined,
                  model: {
                      oldPassword: undefined,
                      newPassword: undefined,
                      confirmPassword: undefined
                  },
                  clear: function () {
                      this.model.oldPassword = undefined;
                      this.model.newPassword = undefined;
                      this.model.confirmPassword = undefined;
                      this.form.$setPristine();
                  },
                  hide: function () {
                      this.open = false;
                  },
                  submitForm: function () {
                      var formContext = this;
                      if (formContext.form.$valid) {
                          data.account.changePassword(formContext.model)
                              .then(function () {
                                  formContext.clear();
                                  utils.infoContext.set(formContext, 'Password was successfully changed.');
                              },
                              function (context) {
                                  utils.errorContext.setMessageFromHttpContext(formContext, context, {
                                      form: formContext.form,
                                      messages: ctrl.messages
                                  });
                              });
                      }
                  },
                  initialize: function () {
                      utils.errorContext.init(this);
                      utils.infoContext.init(this);
                  }
              };

              this.removeLogin = function (login) {
                  if (profile.logins.length === 1) {
                      toastr.warning('You need to have at least one active service.');
                  }
                  else {
                        data.account.removeLogin({
                            provider: login.provider,
                            key: login.key
                        })
                        .then(function () {
                            removeLogin(login);
                        },
                        function (context) {
                            toastr.errorFromHttpContext(context);
                        });
                  }
              };

              this.addLogin = function (login) {
                  authentication.externalLogin(login, true);
              };

              function removeLogin(login) {
                  utils.arrayRemove(ctrl.activeLogins, login);
                  if (isLocal(login)) {
                      ctrl.addPasswordDisabled = false;
                  }
                  else {
                      addOthersLogin(login);
                  }
              }

              function addLocalLogin() {
                  addActiveLogin({
                      provider: localProvider,
                      key: authentication.email()
                  });
              }

              function getExternalLogin(login) {
                  return externalLogins.filter(function (externalLogin) {
                      return externalLogin.provider === login.provider;
                  })[0];
              }

              function addActiveLogin(login) {
                  if (isLocal(login)) {
                      login.caption = resources.applicationName;
                      ctrl.addPasswordDisabled = true;
                  }
                  else {
                      var externalLogin = getExternalLogin(login);

                      login.caption = externalLogin.caption;

                      utils.arrayRemove(ctrl.othersLogins, externalLogin);
                  }

                  ctrl.activeLogins.push(login);
                  ctrl.activeLogins.sort(function (a, b) {
                      if (a.caption > b.caption) {
                          return 1;
                      }
                      if (a.caption < b.caption) {
                          return -1;
                      }
                      // a must be equal to b
                      return 0;
                  });
              }

              function addOthersLogin(login) {
                  login = getExternalLogin(login);
                  ctrl.othersLogins.push(login);
                  ctrl.othersLogins.sort(function (a, b) {
                      if (a.caption > b.caption) {
                          return 1;
                      }
                      if (a.caption < b.caption) {
                          return -1;
                      }
                      // a must be equal to b
                      return 0;
                  });
              }

              function isLocal(login) {
                  return login.provider === localProvider;
              }

              function initialize() {
                  ctrl.addPassword.initialize();
                  ctrl.changePassword.initialize();
                  
                  ctrl.addPasswordDisabled = profile.logins.filter(function (item) {
                      return isLocal(item);
                  }).length != 0;

                  ctrl.othersLogins = externalLogins.filter(function (item) {
                      return profile.logins.filter(function (login) {
                          return login.provider === item.provider;
                      }).length === 0;
                  });

                  ctrl.activeLogins = profile.logins
                      .map(function (login) {
                          var ex, caption;
                          if (isLocal(login)) {
                              caption = resources.applicationName;
                          }
                          else {
                              ex = getExternalLogin(login);
                              caption = ex ? ex.caption : login.provider;
                          }

                          login.caption = caption;

                          return login;
                      });

                  $scope.$on('ea:authentication:externallogin', function (event, token) {
                      data.account.addExternalLogin({ externalAccessToken: token.access_token })
                          .then(function (login) {
                              addActiveLogin(login);
                          },
                          function (context) {
                              toastr.errorFromHttpContext(context);
                          });
                  });

                  $scope.$on('ea:authentication:externallogin:error', function (event, data) {
                      toastr.error(data.error);
                  });
              }

              initialize();
          }]);

})(window, window.angular);