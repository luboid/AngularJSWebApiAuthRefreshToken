(function (angular) {
    'use strict';

    angular.module('ea.services')
        .service('ea.data', ['$rootScope', '$http', '$q', '$log', 'transformRequestAsFormPost', function ($rootScope, $http, $q, $log, transformRequestAsFormPost) {

            var tokenRequestUrl = {
                "/token": true
            };

            return {
                utils: {
                    isTokenRequest: function (config) {
                        return !!tokenRequestUrl[config.url];
                    }
                },
                account: {
                    apiUrl: '/api/account/',
                    logIn: function (params) {
                        return $http.post('/token', params, {
                            transformRequest: transformRequestAsFormPost
                        })
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    register: function (params) {
                        return $http.post(this.apiUrl + 'register', params)
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    forgotPassword: function (params) {
                        return $http.post(this.apiUrl + 'forgotpassword', params)
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    resetPassword: function (params) {
                        return $http.post(this.apiUrl + 'resetPassword', params)
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    logOut: function (params) {
                        return $http.post(this.apiUrl + 'logout', params)
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    externalLogins: function (params) {
                        return $http.get(this.apiUrl + 'externallogins', { params: params || {} })
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    registerExternal: function (token, params) {
                        return $http.post(this.apiUrl + 'registerexternal', params, {
                            headers: {
                                'Authorization': 'Bearer ' + token
                            }
                        })
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    confirmEmail: function (params) {
                        return $http.post(this.apiUrl + 'confirmemail', params)
                        .then(function (data) { return data.data; })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    }
                },
                test: {
                    apiUrl: '/api/test/',
                    get: function () {
                        return $http.get(this.apiUrl, {
                            params: { id: 'testid' }
                        })
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    },
                    post: function () {
                        return $http.post(this.apiUrl, { id: 'testid' })
                        .then(function (data) {
                            return data.data;
                        })
                        .catch(function (state) {
                            $log.error(state);//state -> {data, status, headers, config}
                            throw state;
                        });
                    }
        }
            };

        }])
        .factory("transformRequestAsFormPost", ['bk.utils', function (utils) {
            return function transformRequest(data, headers) {
                var headers = headers();
                headers["Content-type"] = "application/x-www-form-urlencoded; charset=utf-8";
                return utils.encodeToUrl(data);
            };
        }]);

})(window.angular);