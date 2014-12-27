/* Directives */
(function (angular) {
    'use strict';

    angular.module('ea.filters')
        .filter('authorizeMenu', ['$state', 'ea.authorization', function ($state, authorization) {
            return function (items, expresion) {
                
                return [];
            };
        }]);

})(window.angular);