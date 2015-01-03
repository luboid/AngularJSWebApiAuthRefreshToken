/* Services */
(function (angular) {
    'use strict';

    angular.module('ea.resources', [])
        .constant('ea.resources', {
            applicationName: 'Sample app ...',
            validation: {
                common: {
                    required: 'Required.'
                }
            },
            common: {
                saveSucceed: 'Data was successfully saved.',
                saveError: '<strong>Important!</strong> Error while trying to save data.',
                loadError: '<strong>Important!</strong> Error while trying to load data from server.'
            },
            authentication: {
                common: 'App can\'t recognize you.'
            },
            roles: {
                notExistsError: 'Role {name} don\'t exists.',
                cantDeleteError: 'Can\'t remove role {name}.',
                successRemove: 'Remove of {name} succeed.',
                confirmRemove: '<h5>Confirm delete of {name} role.<br />\
                      <small>\
                        <strong>Important!&nbsp;Role will be removed from all users.</strong>\
                      </small>\
                    </h5>'
            },
            datepickerPopupConfig: {
                currentText: 'Today',
                clearText: 'Clear',
                closeText: 'Close'
            }
        });

})(window.angular);