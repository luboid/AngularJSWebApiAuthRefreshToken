(function (window, angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('ProfileCtrl', ['ea.resources', 'ea.data', 'externalLogins', 'profile',
          function (resources, data, externalLogins, profile) {
              var ctrl = this;

              this.addPasswordDisabled = profile.logins.filter(function (item) {
                  return item.provider === 'Local';
              });

              this.activeExternalLogins = profile.logins
                  .map(function (login) {
                      var ex, caption;
                      if (login.provider === 'Local') {
                          caption = resources.applicationName;
                      }
                      else {
                          var ex = externalLogins.filter(function (item) {
                              return login.provider === item.provider;
                          })[0];
                          caption = ex ? ex.caption : login.provider;
                      }

                      login.caption = caption;

                      return login;
                  });


              this.externalLogins = externalLogins.filter(function (item) {
                  return profile.logins.filter(function (login) {
                      return login.provider === item.provider;
                  }).length === 0;
              });

              this.externalLoginDisabled = externalLogins.length === 0;


              this.removeExternalLogin = function (external) {
                  /*
                  debugger;
                  data.account.removeLogin({ provider: external.provider, key: external.key })
                      .then(function () {
                          debugger;
                      }, function () {
                          debugger;
                      });*/
              };

          }]);

})(window, window.angular);