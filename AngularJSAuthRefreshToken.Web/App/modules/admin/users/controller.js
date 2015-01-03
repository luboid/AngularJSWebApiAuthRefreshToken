/* Controllers */
(function (angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('UsersCtrl', ['$scope', 'ea.data', 'ea.dialogs', 'bk.utils',
          function ($scope, data, dialogs, utils) {
              var ctrl = this;

              $scope.info = function (row) {
                  dialogs.user.info(row.id);
              };
              $scope.roles = function (row) {
                  dialogs.user.roles(row);
              };

              this.goFromBegining = function () {
                  utils.browserContext.clear(ctrl);
                  return ctrl.reload();
              };

              this.reload = function () {
                  return data.user.query(ctrl.browserContext)
                    .then(function (data) {
                        utils.browserContext.set(ctrl, data);
                    });
              };

              function ctrlInit() {

                  utils.browserContext.init(ctrl, { sort: 'userName', sortDir: 'desc' });

                  var groupEMail = { title: 'Email address' };
                  var groupPhone = { title: 'Phone' };

                  ctrl.columns = [
                      {
                          title: 'User name',
                          name: 'userName',
                          searchable: true,
                          sortable: true
                      },
                      {
                          group: groupEMail,
                          title: 'Email',
                          name: 'email',
                          searchable: true,
                          sortable: true
                      },
                      {
                          group: groupEMail,
                          title: 'Confirmed',
                          name: 'emailConfirmed',
                          reference: { true: 'Yes', false: 'No' },
                          sortable: false
                      },
                      {
                          group: groupPhone,
                          title: 'Number',
                          name: 'phoneNumber',
                          searchable: true,
                          sortable: false
                      },
                      {
                          group: groupPhone,
                          title: 'Confirmed',
                          name: 'phoneNumberConfirmed',
                          reference: { true: 'Да', false: 'Не' },
                          searchable: false,
                          sortable: false
                      }
                  ];
              }

              ctrlInit();
          }]);

})(window.angular);