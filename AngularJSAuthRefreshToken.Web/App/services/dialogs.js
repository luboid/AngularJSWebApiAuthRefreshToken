(function (angular) {
    'use strict';

    angular.module('ea.services')
        .service('ea.dialogs', ['$modal', 'ea.urlFactory', function ($modal, urlFactory) {

            return {
                authentication: {
                    error: function (data) {
                        return $modal.open({
                            templateUrl: urlFactory.partialUrl('extautherror'),
                            controllerAs: 'ctrl',
                            controller: function () {
                                this.data = data;
                            }
                        }).result;
                    },
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
        }]);

})(window.angular);