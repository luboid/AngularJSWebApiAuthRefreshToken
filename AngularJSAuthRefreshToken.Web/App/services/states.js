(function (window, document, angular) {
    'use strict';

    angular.module('ea')
        .constant('ea.urlFactory', {
            version: (new Date()).valueOf(),//in production change to something meaningful
            templateUrl: function () {
                var url = '/app/modules';
                if (arguments.length) {
                    for (var i = 0, l = arguments.length; i < l; i++) {
                        url += '/' + arguments[i];
                    }
                }
                return url + '/template.html?version=' + this.version;
            },
            partialUrl: function (partial) {
                return '/app/partials/' + partial + '.html?version=' + this.version;
            },
            hostUrl: function () {
                return (window.location.protocol + '//' + window.location.host);
            },
            applicationLocationUrl: function () {
                var url = this.hostUrl(),
                    base = angular.element(document).find('base');

                if (base.length) {
                    url += base.attr('href');
                }
                return url;
            },
            externalLoginCallbackUrl: function () {
                return this.hostUrl() + '/externallogin.html';
            },
            confirmEmailUrl: function () {//emailConfirmationUrl
                return this.applicationLocationUrl() + 'account/confirmemail?uid={0}&cid={1}';
            },
            resetPasswordUrl: function () {//passwordConfirmationUrl
                return this.applicationLocationUrl() + 'account/resetpassword?uid={0}&cid={1}';
            }
        })
        .config(['$stateProvider', '$urlRouterProvider', '$locationProvider', 'ea.urlFactory',
            function ($stateProvider, $urlRouterProvider, $locationProvider, urlFactory) {

                // Load application navigation paths 
                $locationProvider.html5Mode(true);
                $locationProvider.hashPrefix('!');

                //ако не е дефиниран някой път да отиде на началния екран
                $urlRouterProvider.otherwise('/');

                $stateProvider
                    .state("home", {
                        url: "/?congratulation",
                        templateUrl: urlFactory.templateUrl('home'),
                        controllerAs: 'ctrl',
                        controller: 'HomeCtrl',
                        data: {
                            auth: false
                        }
                    })
                    .state("unauthorized", {
                        url: "/unauthorized",
                        templateUrl: urlFactory.partialUrl('unauthorized'),
                        data: {
                            auth: false
                        }
                    })
                    //#region account states/routes
                    .state('account', {
                        abstract: true,
                        url: '/account',
                        controller: ['$scope', function ($scope) {
                            //$scope.inherit ={} value inherited into down scopes -> account/login
                        }],
                        // Note: abstract still needs a ui-view for its children to populate.
                        // You can simply add it inline here.
                        template: '<ui-view/>',
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
                        templateUrl: urlFactory.templateUrl('account','signup'),
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
                        url: "/confirmemail?uid&cid",
                        templateUrl: urlFactory.templateUrl('account', 'confirmemail'),
                        controller: 'ConfirmeMailCtrl as ctrl',
                        resolve: {
                            confirmEmail: ['$stateParams', 'ea.data', function ($stateParams, data) {
                                return data.account.confirmEmail({
                                    userId: $stateParams.uid,
                                    code: $stateParams.cid
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
                        url: "/resetpassword?uid&cid",
                        templateUrl: urlFactory.templateUrl('account', 'resetpassword'),
                        controller: 'ResetPasswordCtrl as ctrl',
                        data: {
                            auth: false
                        }
                    })
                    .state("account.profile", {
                        url: "/profile",
                        templateUrl: urlFactory.templateUrl('account', 'profile'),
                        controller: 'ProfileCtrl as ctrl'
                    })
                    //#endregion account states/routes
                    .state("test", {
                        url: "/test",
                        templateUrl: urlFactory.templateUrl('test'),
                        controller: 'TestCtrl as ctrl',
                        data: {
                            auth: true
                        }
                    });
        }]);

})(window, document, window.angular);