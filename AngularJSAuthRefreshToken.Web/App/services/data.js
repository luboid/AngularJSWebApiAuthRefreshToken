(function (angular) {
    'use strict';

    angular.module('ea.services')
        .factory('ea.data', ['$http', '$log', 'transformRequestAsFormPost', 'webApi', function ($http, $log, transformRequestAsFormPost, webApi) {
            var tokenRequestUrl = {
                "/token": true
            };

            var api = {
                utils: {
                    isTokenRequest: function (config) {
                        return !!tokenRequestUrl[config.url];
                    }
                },
                account: webApi('/api/account', {}, {
                    logIn: { url: '/token', transformRequest: transformRequestAsFormPost },
                    register: true,
                    forgotPassword: true,
                    resetPassword: true,
                    logOut: true,
                    externalLogins: 'GET',
                    registerExternal: {
                        transformRequest: function transformRequest(data, headers) {
                            if (data.access_token) {
                                data = angular.copy(data);
                                headers = headers();
                                headers["Authorization"] = 'Bearer ' + data.access_token;
                                delete data['access_token'];
                            }
                            return angular.toJson(data);
                        }
                    },
                    addExternalLogin: true,
                    confirmEmail: true,
                    profile: 'GET',
                    removeLogin: true,
                    setPassword: true,
                    changePassword: true
                }, {
                    actions: null
                }),
                test: webApi('/api/test', { id: 'testid' }),
                user: webApi('/api/user', {}, {
                    role: [':id/role', undefined/*paramDefaults*/, undefined/*actions*/, { defaultParam: ':name' }]
                }),
                role: webApi('/api/role')
            };

            return api;
        }])
        .factory("transformRequestAsFormPost", ['bk.utils', function (utils) {
            return function transformRequest(data, headers) {
                var headers = headers();
                headers["Content-type"] = "application/x-www-form-urlencoded; charset=utf-8";
                return utils.encodeToUrl(data);
            };
        }]);

})(window.angular);