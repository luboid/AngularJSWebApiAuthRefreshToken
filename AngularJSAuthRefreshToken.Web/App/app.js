(function (window, angular) {
    'use strict';

    // тук само ги декларирам за да може нататък само  да добавям към тях
    angular.module('ea.services', []);
    angular.module('ea.controllers', []);
    angular.module('ea.directives', []);
    angular.module('ea.filters', []);

    // Declare app level module which depends on filters, and services
    angular.module('ea', [
      'ngSanitize', 'ngAnimate',
      'ui.router',
      'ui.bootstrap',
      'chieffancypants.loadingBar',
      'ea.filters',
      'ea.services',
      'ea.directives',
      'ea.controllers',
      'ea.resources',
      'ea.authenticationInterceptor',
      'bk.utils',
      'bk.directives'
    ])
    .run(['ea.params', 'datepickerPopupConfig', 'bk.utils', function (params, datepickerPopupConfig, utils) {
        // https://localhost:44300/ea/#error=access_denied google external login have cancel
        if (window.opener && 'access_denied' === params.error) {
            window.close();
        }

        // format and localization
        utils.initDatepicker(datepickerPopupConfig);
    }]);
})(window, window.angular);