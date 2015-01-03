(function (window, document, angular) {
    'use strict';
    /*
     * https://github.com/witoldsz/angular-http-auth/blob/master/src/http-auth-interceptor.js
     * https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS#Requests_with_credentials
     */
    angular.module('ea.authenticationInterceptor', [])
        .config(['$httpProvider', function ($httpProvider) {
            // server to make difference between calls, from where they come
            $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

            $httpProvider.interceptors.push(['$q', '$injector', function ($q, $injector) {

                var $http, $rootScope,
                    authentication,
                    authenticationRefresh;

                function ensureAuthenticationModule() {
                    if (!authentication) {
                        authentication = $injector.get('ea.authentication');
                    }
                    return authentication;
                }

                function ensureHttpModule() {
                    if (!$http) {
                        $http = $injector.get('$http');
                    }
                    return $http;
                }

                function forbidden(rejection) {
                    if (!$rootScope) {
                        $rootScope = $injector.get('$rootScope');
                    }
                    $rootScope.$broadcast('ea:forbidden', rejection)
                }

                function retryRequest(rejection) {
                    ensureHttpModule();
                    rejection.config.headers.Authorization = undefined;
                    return $http(rejection.config);
                }

                return {
                    request: function (config) {
                        ensureAuthenticationModule();
                        if (!config.headers.Authorization &&
                            !authentication.isTokenRequest(config) &&
                             authentication.isAuthenticated()) {

                            config.headers.Authorization = 'Bearer ' + authentication.token();
                        }
                        return config;
                    },
                    responseError: function (rejection) {
                        switch (rejection.status) {
                            case 401:
                                if (!authentication.isTokenRequest(rejection.config)) {
                                    // next fail request will use same authentication refresh request
                                    if (!authenticationRefresh) {
                                        // this will open login dialog if it is needed
                                        authenticationRefresh = authentication.refresh();
                                    }

                                    var defer = $q.defer();

                                    authenticationRefresh.then(function () { // if refresh is done normaly                                        
                                        retryRequest(rejection).then(function (result) {//retry succeed                                            
                                            defer.resolve(result);
                                        }, function (error) {
                                            defer.reject(error);
                                        });
                                        authenticationRefresh = null;//next will use new refresh token
                                    }, function () { // refresh fail                                        
                                        defer.reject(rejection);// need reject it...
                                        authenticationRefresh = null;//next will use new refresh token
                                    });

                                    return defer.promise;
                                }
                                break;
                            case 403:
                                forbidden(rejection);
                                break;
                        }
                        return $q.reject(rejection);
                    }
                };
            }]);
        }]);

})(window, document, window.angular);