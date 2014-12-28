(function (window, document, angular) {
    'use strict';

    angular.module('ea.services')
        .service('ea.authentication', ['$rootScope', '$timeout', '$q', '$log', 'bk.utils', 'ea.config', 'ea.urlFactory', 'ea.data', 'ea.resources',
            function ($rootScope, $timeout, $q, $log, utils, config, urlFactory, data, messages) {
                var currentExternalLogin = null,
                    authenticationLoginDefered = null,
                    state = {
                        isAuthenticated: false,
                        token: {}
                    };

                // we here need access to others modules from app to verify access_token
                window.externalLogin = function (token) {
                    removeAuthIFrame();
                    if (token && -1 < token.indexOf('access_token=')) {
                        token = utils.decodeFromUrl(token.substr(1));
                        if (token.provider === 'Local') {
                            currentExternalLogin = null;
                            assignToken(token)
                        }
                        else {
                            $rootScope.$broadcast('ea:authentication:externallogin', token);
                        }
                    }
                    else {
                        if (token && -1 < token.indexOf('error=')) {
                            token = utils.decodeFromUrl(token.substr(1));
                        }
                        else {
                            $log.error('Invalid external token.', token);
                            token = { error: 'Invalid external token.' };
                        }
                        $rootScope.$broadcast('ea:authentication:externallogin:error', token);
                    }
                };

                function resolveAuthenticationLoginDefered() {
                    if (authenticationLoginDefered) {//exists defer initilized in process of login dialog
                        if (service.isAuthenticated()) {
                            authenticationLoginDefered.resolve(true);//who care about value when all is Ok
                        }
                        else {
                            authenticationLoginDefered.reject({ error: 'login_dialog_dismiss', error_description: 'Login dialog fail ...' });
                        }
                        authenticationLoginDefered = null;
                    }
                }

                function raiseEvent() {
                    resolveAuthenticationLoginDefered();
                    $rootScope.$broadcast('ea:authentication:token', service);
                }

                function hashRoles(roles) {
                    var i, hash = {};
                    if (roles && angular.isString(roles)) {
                        roles = roles.split(',');
                        for (i in roles) {
                            hash[roles[i]] = true;
                        }
                    	return hash;
                    }
		    else {
			return roles;
		    }
                }

                function assignToken(token) {
                    state.token = token || {};
                    state.token.roles = hashRoles(state.token.roles);
                    state.isAuthenticated = !!state.token.access_token;

                    if (state.isAuthenticated) {
                        window.localStorage['token'] = angular.toJson(state.token)
                    }
                    else {
                        window.localStorage.removeItem('token');
                    }

                    raiseEvent();
                }

                function initialize() {
                    var token = window.localStorage['token'];
                    if (token && (token = angular.fromJson(token))) {
                        assignToken(token);
                    }
                    else {
                        assignToken();
                    }
                }

                function logIn(cridentials) {
                    return data.account.logIn(cridentials)
                        .then(function (token) {
                            // fatch userName, roles and etc.
                            // this need to be done without any problems
                            assignToken(token);
                        }, function (error) {
                            if (error.data.error === 'invalid_grant' && cridentials.grant_type === 'refresh_token') {
                                authenticationLoginDefered = $q.defer();
                                $rootScope.$broadcast('ea:authentication:login', authenticationLoginDefered);
                                return authenticationLoginDefered.promise;
                            }
                            else {
                                state.isAuthenticated = false;
                                if (error.data.error) {
                                    throw error.data;
                                }
                                else {
                                    throw { error: 'client_app', error_description: messages.authentication.common };
                                }
                            }
                        });
                }

                var service = {
                    isAuthenticated: function () {
                        return state.isAuthenticated;
                    },
                    token: function () {
                        return state.token.access_token;
                    },
                    userName: function () {
                        return state.token.userName;
                    },
                    isInRole: function () {
                        var roles = Array.prototype.slice.call(arguments, 0);
                        if (roles.length !== 0 && state.isAuthenticated) {
                            roles = roles.filter(function (role) {
                                if (angular.isArray(role)) {
                                    return this.isInRole.apply(this,role);
                                }
                                else {
                                    return !!state.token.roles[role];
                                }
                            });
                            return roles.length != 0;
                        }
                        return false;
                    },
                    isTokenRequest: function (config) {
                        return data.utils.isTokenRequest(config);
                    },
                    logIn: function (cridentials) {
                        var cridentials = angular.copy(cridentials, {});
                        if (cridentials.grant_type === undefined) {
                            cridentials.grant_type = 'password';
                        }
                        if (cridentials.client_id === undefined) {
                            cridentials.client_id = config.client_id;
                        }
                        if (cridentials.client_secret === undefined) {
                            cridentials.client_secret = config.client_secret;
                        }

                        var lCallback = angular.bind(null, logIn, cridentials);
                        if (state.token && state.token.refresh_token) {
                            //we need to drop old refresh token, before login again
                            return this.logOut().then(lCallback, lCallback);
                        }
                        else {
                            return lCallback();
                        }
                    },
                    refresh: function () {
                        var cridentials = {
                            grant_type: 'refresh_token',
                            refresh_token: state.token && state.token.refresh_token,
                            client_id: config.client_id,
                            client_secret: config.client_secret
                        };
                        return logIn(cridentials);
                    },
                    logOut: function () {
                        return data.account.logOut({ refreshToken: state.token.refresh_token })
                            .then(function () {
                                assignToken();
                                return true;
                            }, function (error) {
                                assignToken();
                                $log.error(error);
                                return true;
                            });
                    },
                    externalLogins: function (params) {
                        params = params || {};
                        if (params.generateState === undefined) {
                            params.generateState = true;
                        }
                        if (params.returnUrl === undefined) {
                            params.returnUrl = urlFactory.externalLoginCallbackUrl();
                        }
                        if (params.client_id === undefined) {
                            params.client_id = config.client_id;
                        }
                        return data.account.externalLogins(params);
                    },
                    externalLogin: function (external) {
                        currentExternalLogin = external;

                        var win = window.open(external.url, '__externalLogin', 'width=1024,height=620,status=0,personalbar=0,toolbar=0,menubar=0');
                        if (win.focus) {
                            win.focus();
                        }
                    },
                    registerExternal: function (token, params, onSuccessLogin) {
                        if (onSuccessLogin === undefined) {
                            onSuccessLogin = true;
                        }

                        return data.account.registerExternal(token, params).then(function (emailConfirmation) {
                            if (onSuccessLogin && !emailConfirmation) {
                                loadAuthIFrame(currentExternalLogin.url);
                            }
                            return emailConfirmation;
                        });
                    }
                };

                initialize();

                return service;
            }]);

    function removeAuthIFrame() {
        var frame = angular.element('#auth-frame');
        if (0 !== frame.length) {
            frame.remove();
        }
    }

    function loadAuthIFrame(url) {
        var frame = angular.element('#auth-frame');
        if (0 === frame.length) {
            var element = document.createElement("iframe");
            element.setAttribute('id', 'auth-frame');
            element.setAttribute('style', 'display:none');
            document.body.appendChild(element);
            frame = angular.element(element);
        }
        frame.attr('src', url);
    }

})(window, document, window.angular);