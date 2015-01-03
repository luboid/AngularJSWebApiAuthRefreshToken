/* Controllers */
(function (angular) {
    'use strict';

    angular.module('ea.controllers')
      .controller('UserRolesCtrl', ['$scope', 'bk.utils', 'ea.authorization', 'ea.data', 'ea.toastr', 'user', 'userRoles', 'roles',
          function ($scope, utils, authorization, data, toastr, user, userRoles, roles) {
              userRoles = utils.array2Hash3(userRoles);

              $scope.userName = user.userName;
              $scope.disabled = authorization.isMyEmail(user.email);

              $scope.roles = roles.map(function (item) {
                  item.beforeValue = 
                      item.checked = !!userRoles[item.name];

                  var watch = angular.bind(item, function () {
                      return this.checked;
                  });

                  var callback = angular.bind(item, function (newValue, oldValue) {

                      (function (item, newValue, oldValue) {
                          if (newValue !== oldValue) {
                              if (item.beforeValue === undefined || item.beforeValue !== newValue) {
                                  var actionPromise;

                                  actionPromise = item.checked ?
                                      data.user.role.save({ id: user.id }, { name: item.name }) :
                                      data.user.role.delete({ id: user.id, name: item.name });

                                  actionPromise.then(function () {
                                      item.beforeValue = item.checked = newValue;
                                  }, function (context) {
                                      item.checked = oldValue;

                                      toastr.errorFromHttpContext(context);
                                  });
                              }
                          }
                      })(this, newValue, oldValue);

                  });

                  item.unWatch = $scope.$watch(watch, callback);

                  return item;
              });
          }]);

})(window.angular);