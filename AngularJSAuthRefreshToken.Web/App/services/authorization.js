/* Directives */
(function (angular) {
    'use strict';

    angular.module('ea.services')
        .service('ea.authorization', ['$state', 'ea.authentication', function ($state, authentication) {

            function find(name) {
                var idx = name.indexOf('(');
                if (idx > -1) {
                    name = name.substr(0, idx);
                }
                return $state.get(name);
            }

            return {
                authentication: authentication,

                state: function (state) {
                    if (angular.isString(state)) {
                        state = find(state);
                    }

                    var authorized = true;
                    if (state && (state.data.auth === undefined || state.data.auth)) {
                        if (!authentication.isAuthenticated() || (typeof (state.data.auth) !== 'boolean' && !authentication.isInRole(state.data.auth))) {
                            authorized = false;
                        }
                    }

                    return authorized;
                },
                isInRole: function () {
                    authentication.isInRole.apply(authentication, arguments);
                }
            };

        }]);

})(window.angular);