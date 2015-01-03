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
            confirmEmailUrl: function () {
                return this.applicationLocationUrl() + 'account/confirmemail#uid={0}&cid={1}';
            },
            resetPasswordUrl: function () {
                return this.applicationLocationUrl() + 'account/resetpassword#uid={0}&cid={1}';
            }
        });

})(window, document, window.angular);