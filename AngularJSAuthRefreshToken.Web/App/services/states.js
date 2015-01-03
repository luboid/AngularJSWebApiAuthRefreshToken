(function (window, document, angular) {
    'use strict';

    angular.module('ea')
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
                    .state("test", {
                        url: "/test",
                        templateUrl: urlFactory.templateUrl('test'),
                        controller: 'TestCtrl as ctrl',
                        data: {
                            auth: true
                        }
                    })
                    .state('admin', {
                        abstract: true,
                        url: '/admin',
                        template: '<ui-view />',
                        data: {
                            auth: 'Admin'
                        }
                    })
                    .state("admin.users", {
                        url: "/users",
                        templateUrl: urlFactory.templateUrl('admin', 'users'),
                        controller: 'UsersCtrl as ctrl'
                    })
                    .state("admin.roles", {
                        url: "/roles",
                        templateUrl: urlFactory.templateUrl('admin', 'roles'),
                        controller: 'RolesCtrl as ctrl'
                    });
            }]);

})(window, document, window.angular);