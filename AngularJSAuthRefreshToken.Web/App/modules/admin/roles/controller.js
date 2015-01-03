/* Controllers */
(function (angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('RolesCtrl', ['$scope', 'bk.utils', 'ea.resources', 'ea.data', 'ea.dialogs', 'ea.toastr',
          function ($scope, utils, resources, data, dialogs, toastr) {
              var ctrl = this;

              this.roles = [];
              this.load = function () {
                  return data.role.query().then(function (data) {
                      ctrl.roles = data;
                  });
              };

              this.create = {
                  form: null,
                  model: {},
                  messages: {
                      common: resources.validation.common
                  },
                  submitForm: function () {
                      var self = this;
                      if (self.form.$valid) {
                          data.role.save(this.model)
                            .then(function (role) {
                                self.model.name = undefined;
                                self.form.$setPristine();
                                ctrl.addRole(role);
                            },
                            function (context) {
                                toastr.errorFromHttpContext(context, {
                                    ctrl: {
                                        form: self.form,
                                        messages: self.messages
                                    }
                                });
                            });
                      }
                  }
              };

              this.addRole = function (role) {
                  this.roles.push(role);
                  utils.arraySort(this.roles, 'name');
              }

              this.remove = function (row) {
                  var message = utils.format(resources.roles.confirmRemove, row);
                  dialogs.standard.confirm({ message: message })
                      .then(function () {
                          data.role.delete({ id: row.id })
                              .then(function (data) {
                                  utils.arrayRemove(ctrl.roles, row);
                                  toastr.success(utils.format(resources.roles.successRemove, row));
                              },
                              function (context) {
                                  toastr.errorRemoveItem(context, {
                                      ctrl: {
                                          messages: resources.roles,
                                          data: row
                                      }
                                  });
                              });
                      });
              }
          }]);

})(window.angular);