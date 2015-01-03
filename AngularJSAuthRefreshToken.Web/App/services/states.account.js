(function (window, document, angular) {
    'use strict';

    angular.module('ea')
        .config(['$stateProvider', '$urlRouterProvider', '$locationProvider', 'ea.urlFactory',
            function ($stateProvider, $urlRouterProvider, $locationProvider, urlFactory) {
                $stateProvider.state('account', {
                    abstract: true,
                    url: '/account',
                    controller: ['$scope', function ($scope) {
                        //$scope.inherit ={} value inherited into down scopes -> account/login
                    }],
                    // Note: abstract still needs a ui-view for its children to populate.
                    // You can simply add it inline here.
                    template: '<ui-view />',
                    data: {
                        auth: true
                    }
                })
                .state("account.login", {
                    url: "/login",
                    templateUrl: urlFactory.templateUrl('account', 'login'),
                    controller: 'LoginCtrl as ctrl',
                    resolve: {
                        externalLogins: ['ea.authentication', function (authentication) {
                            return authentication.externalLogins();
                        }]
                    },
                    data: {
                        auth: false
                    }
                })
                .state("account.signup", {
                    url: "/signup",
                    templateUrl: urlFactory.templateUrl('account', 'signup'),
                    controller: 'SignupCtrl as ctrl',
                    data: {
                        auth: false
                    }
                })
                .state("account.externalloginconfirmation", {
                    url: "/externalloginconfirmation?token&loginProvider&userName&email",
                    templateUrl: urlFactory.templateUrl('account', 'externalloginconfirmation'),
                    controller: 'ExternalLoginConfirmationCtrl as ctrl',
                    data: {
                        auth: false
                    }
                })
                .state("account.logout", {
                    url: "/logout",
                    templateUrl: urlFactory.templateUrl('home'),
                    resolve: {
                        logOut: ['ea.authentication', function (authentication) {
                            return authentication.logOut();
                        }]
                    }
                })
                .state("account.forgotpassword", {
                    url: "/forgotpassword",
                    templateUrl: urlFactory.templateUrl('account', 'forgotpassword'),
                    controller: 'ForgotPasswordCtrl as ctrl',
                    data: {
                        auth: false
                    }
                })
                .state("account.confirmemail", {
                    url: "/confirmemail",
                    templateUrl: urlFactory.templateUrl('account', 'confirmemail'),
                    controller: 'ConfirmeMailCtrl as ctrl',
                    resolve: {
                        confirmEmail: ['$location', 'bk.utils', 'ea.data', function ($location, utils, data) {
                            var params = utils.decodeFromUrl($location.hash());
                            return data.account.confirmEmail({
                                userId: params.uid,
                                code: params.cid
                            }).catch(function (state) {
                                return state;//resolve promise with error state
                            });
                        }]
                    },
                    data: {
                        auth: false
                    }
                })
                .state("account.resetpassword", {
                    url: "/resetpassword",
                    templateUrl: urlFactory.templateUrl('account', 'resetpassword'),
                    controller: 'ResetPasswordCtrl as ctrl',
                    data: {
                        auth: false
                    }
                })
                .state("account.profile", {
                    url: "/profile",
                    templateUrl: urlFactory.templateUrl('account', 'profile'),
                    controller: 'ProfileCtrl as ctrl',
                    resolve: {
                        profile: ['ea.data', function (data) {
                            return data.account.profile();
                        }],
                        externalLogins: ['ea.authentication', function (authentication) {
                            return authentication.externalLogins();
                        }]
                    }
                });
            }]);

})(window, document, window.angular);