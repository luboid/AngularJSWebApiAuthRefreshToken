(function (angular) {
    'use strict';

    angular.module('ea.services')
        .service('ea.toastr', ['bk.utils', 'toastr', function (utils, toastr) {

            return {
                setDefaults: function (settings) {
                    
                },
                info: function (message, settings) {
                    toastr.info(message);
                },
                warning: function (message, settings) {
                    toastr.warning(message);
                },
                error: function (message, settings) {
                    toastr.error(message);
                },
                success: function (message, settings) {
                    toastr.success(message);
                },
                errorRemoveItem: function (context, options) {
                    options = options || {};

                    var m = utils.createRemoveErrorMessage(context,
                                    options.ctrl && options.ctrl.messages,
                                    options.ctrl && options.ctrl.data);

                    if (m) {//options.toast!!!
                        toastr.error(m);
                    }
                },
                errorFromHttpContext: function (context, options) {
                    options = options || {};

                    var m = utils.createMessageFromHttpContext(context, {
                        form: options.ctrl && options.ctrl.form,
                        messages: options.ctrl && options.ctrl.messages
                    });

                    if (m) {//options.toast!!!
                        toastr.error(m);
                    }
                }
            };

        }])
        .run(['toastrConfig', function (toastrConfig) {
            //toastr configuration
            toastrConfig.positionClass = 'toast-top-center';
            toastrConfig.allowHtml = true;
        }]);

})(window.angular);