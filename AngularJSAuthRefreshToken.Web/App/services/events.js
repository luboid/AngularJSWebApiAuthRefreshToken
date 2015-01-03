(function (angular) {
    'use strict';

    angular.module('ea.services')
        .factory('ea.events', [function () {

            var registerEvents = { };

            return function ($scope, unListener) {
                var scopeEvents = registerEvents[$scope.$id];

                if (!scopeEvents) {
                    scopeEvents = registerEvents[$scope.$id] = (function ($scope) {
                        var events = [];

                        $scope.$on('$destroy', function () {
                            events.forEach(function (unListener) {
                                unListener();
                            });

                            delete registerEvents[$scope.$id];
                        });

                        return events;
                    })($scope);
                }

                scopeEvents.push(unListener);
            };

        }]);

})(window.angular);