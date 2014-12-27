/* Services */
(function (angular) {
    'use strict';

    angular.module('ea.resources', [])
        .constant('ea.resources', {
            applicationName: 'Sample App',
            common: {
                saveSucceed: 'Data was successfully saved.',
                saveError: '<strong>Important!</strong> Error while trying to save data.',
                loadError: '<strong>Important!</strong> Error while trying to load data from server.'
            },
            authentication: {
                common: 'App can\'t recognize you.'
            },
            datepickerPopupConfig: {
                currentText: 'Today',
                clearText: 'Clear',
                closeText: 'Close'
            }
        });

})(window.angular);