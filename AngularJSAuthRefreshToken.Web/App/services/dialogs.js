(function (angular) {
    'use strict';

    angular.module('ea.services')
        .factory('ea.dialogs', ['$modal', 'ea.urlFactory', function ($modal, urlFactory) {

            return {
                standard: {
                    error: function (options) {
                        return $modal.open({
                            templateUrl: '/app/partials/dialogs/error.html',
                            backdrop: 'static',
                            controllerAs: 'ctrl',
                            controller: ['$scope', function ($scope) {
                                $scope.title = options.title || 'Error ...';
                                $scope.message = options.message;
                                $scope.close = function () {
                                    $scope.$close();
                                };
                            }]
                        }).result;
                    },
                    confirm: function (options) {
                        return $modal.open({
                            templateUrl: '/app/partials/dialogs/confirm.html',
                            backdrop: 'static',
                            controllerAs: 'ctrl',
                            controller: ['$scope', function ($scope) {
                                $scope.title = options.title || 'Confirm ...';
                                $scope.message = options.message;
                                $scope.no = function () {
                                    $scope.$dismiss()
                                };
                                $scope.yes = function () {
                                    $scope.$close('yes');
                                };
                            }]
                        }).result;
                    },
                    alert: function (options) {
                        return $modal.open({
                            templateUrl: '/app/partials/dialogs/notify.html',
                            backdrop: 'static',
                            controllerAs: 'ctrl',
                            controller: ['$scope', function ($scope) {
                                $scope.title = options.title || 'Info ...';
                                $scope.message = options.message;
                                $scope.close = function () {
                                    $scope.$close('yes');
                                };
                            }]
                        }).result;
                    },
                    wait: function (options) {
                        throw new Error('Not implemented yet.');
                        return $modal.open({
                            templateUrl: '/app/partials/dialogs/wait.html',
                            backdrop :'static',
                            controllerAs: 'ctrl',
                            controller: ['$scope', function ($scope) {
                                $scope.title = options.title || 'Прогрес ...';
                                $scope.message = options.message;
                            }]
                        }).result;
                    }
                },
                user: {
                    info: function (id) {
                        return $modal.open({
                            templateUrl: urlFactory.templateUrl('admin', 'users', 'info'),
                            controllerAs: 'ctrl',
                            controller: 'UserInfoCtrl',
                            resolve: {
                                info: ['ea.data', function (data) {
                                    return data.user.get({ id: id });
                                }]
                            }
                        }).result;
                    },
                    roles: function (user) {
                        return $modal.open({
                            templateUrl: urlFactory.templateUrl('admin', 'users', 'roles'),
                            controllerAs: 'ctrl',
                            controller: 'UserRolesCtrl',
                            resolve: {
                                userRoles: ['ea.data', function (data) {
                                    return data.user.role.get({ id: user.id });
                                }],
                                roles: ['ea.data', function (data) {
                                    return data.role.query();
                                }],
                                user: function () {
                                    return user;
                                }
                            }
                        }).result;
                    }
                },
                authentication: {
                    logIn: function (defered) {
                        return $modal.open({
                            templateUrl: urlFactory.templateUrl('account', 'login'),
                            controllerAs: 'ctrl',
                            controller: 'LoginCtrl',
                            backdrop: 'static',
                            windowTemplateUrl: urlFactory.partialUrl('modalwindowtemplate'),
                            resolve: {
                                externalLogins: ['ea.authentication', function (authentication) {
                                    return authentication.externalLogins();
                                }]
                            }
                        }).result.finally(function () {
                            // here can not say how process is ended
                            // if dialog is closed without trying to login
                            // result from dialog is under quesstion without checking ea.authentication
                            defered && defered.reject();
                        });
                    }
                }
            };
        }])
    	// Add default templates via $templateCache
	    .run(['$templateCache', '$interpolate', function ($templateCache, $interpolate) {
	        // http://codepen.io/m-e-conroy/pen/ALsdF
	        $templateCache.put('/app/partials/dialogs/error.html', '<div class="modal-header dialog-header-error">\
    <button type="button" class="close" ng-click="close()">&times;</button>\
    <h4 class="modal-title text-danger"><span class="glyphicon glyphicon-warning-sign"></span>&nbsp;{{title}}</h4>\
</div>\
<div class="modal-body text-danger" ng-bind-html="message"></div>\
<div class="modal-footer">\
    <button type="button" class="btn btn-primary" ng-click="close()">Ok</button>\
</div>');

	        $templateCache.put('/app/partials/dialogs/wait.html', '<div class="modal-header dialog-header-wait">\
    <h4 class="modal-title"><span class="glyphicon glyphicon-time"></span>&nbsp;{{header}}</h4>\
</div>\
<div class="modal-body">\
    <p ng-bind-html="msg"></p>\
    <div class="progress progress-striped active">\
        <div class="progress-bar progress-bar-info" ng-style="getProgress()"></div>\
        <span class="sr-only">progress ...</span>\
    </div>\
</div>');

	        $templateCache.put('/app/partials/dialogs/notify.html', '<div class="modal-header dialog-header-notify">\
    <button type="button" class="close" ng-click="close()">&times;</button>\
    <h4 class="modal-title text-info">\
        <span class="glyphicon glyphicon-info-sign"></span>&nbsp;{{title}}\
    </h4>\
</div><div class="modal-body text-info" ng-bind-html="message"></div>\
<div class="modal-footer">\
    <button type="button" class="btn btn-primary" ng-click="close()">Ok</button>\
</div>');

	        $templateCache.put('/app/partials/dialogs/confirm.html', '<div class="modal-header dialog-header-confirm">\
    <button type="button" class="close" ng-click="no()">&times;</button>\
    <h4 class="modal-title">\
        <span class="glyphicon glyphicon-check"></span>&nbsp;{{title}}\
    </h4>\
</div>\
<div class="modal-body" ng-bind-html="message"></div>\
<div class="modal-footer">\
    <button type="button" class="btn btn-default" ng-click="yes()">Yes</button>\
    <button type="button" class="btn btn-primary" ng-click="no()">No</button>\
</div>');

	    }]); // end run / dialogs.main

})(window.angular);