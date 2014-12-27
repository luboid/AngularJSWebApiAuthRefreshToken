(function (window, angular) {
    'use strict';

    // идеята на този модул е да запази параметрите с които е стартирана програма
    angular.module('ea.services')
        .service('ea.params', ['bk.utils', function (utils) {
            // https://localhost:44300/ea/#error=access_denied google external login have cancel
            return utils.decodeFromUrl(('' + window.location.hash).substr(1));
        }]);

})(window, window.angular);