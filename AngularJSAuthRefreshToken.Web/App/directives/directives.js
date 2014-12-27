/* Directives */
(function (angular, document) {
    'use strict';

    function focusElement(e) {
        if (e && e.length) {
            e.focus();
            if ('INPUT' === e.prop('nodeName') || 'TEXTAREA' === e.prop('nodeName')) {
                e[0].select();
            }
        }
    }

    function execFocusElement(result, e) {
        if (result && (result.promise || result.finally)) {
            if (result.promise) {
                result = result.promise;
            }

            result.finally(function () {
                focusElement(e);
            });
        }
        else {
            focusElement(e);
        }
    }
    
    //#region bkErrorTitle
    var bkErrorTitle = [function () {
        return {
            require: ['^form','ngModel'],
            restrict: 'A',
            link: function ($scope, $element, $attrs, ctrls) {
                var ctrl = ctrls[0],
                    inputCtrl = ctrls[1],
                    cssClass = $attrs.bkHasErrorClass || 'has-error',
                    parent = $element.parents($attrs.bkHasErrorParentSelector || '.form-group:first'),
                    messages = ($attrs.bkMessages && $parse($attrs.bkMessages)($scope)) || {};

                if (0 === parent.length) {
                    parent = $element.parent();
                }

                function showErrorText(newValue, oldValue) {
                    if (inputCtrl.$invalid && ctrl.bkShowErrors()) {
                        var i, key, e, etext = [],
                            error = inputCtrl.$error,
                            inputMessages = messages[$attrs.bkInputName] || {};

                        for (i in error) {
                            if (error[i] && i !== 'parse') {
                                key = 'bkMsg' + angular.uppercase(i.charAt(0)) + i.substr(1);
                                e = $attrs[key];
                                if (!e) {
                                    e = inputMessages[i];
                                    if (!e && messages.common) {
                                        e = messages.common[i];
                                    }

                                    if (e && angular.isFunction(e)) {
                                        e = e(inputCtrl, ctrl);
                                    }

                                    if (!e) {
                                        e = (i + ' no message text!');
                                    }
                                }
                                etext.push(e);
                            }
                        }

                        $element.attr('title', etext.join('\r\n'));
                        parent.addClass(cssClass);
                    }
                    else {
                        $element.attr('title', '');
                        parent.removeClass(cssClass);
                    }
                };

                $scope.$watchCollection(function () {
                    return inputCtrl.$error;
                }, showErrorText);
                $scope.$watch(function () {
                    return inputCtrl.$invalid && ctrl.bkShowErrors();
                }, showErrorText);
            }
        };
    }];
    //#endregion

    //#region bkErrorText
    var bkErrorText = ['$parse', function ($parse) {
        return {
            require: '^form',
            template: '<span></span>',
            restrict: 'E',
            link: function ($scope, $element, $attrs, ctrl) {
                var inputCtrl = ctrl[$attrs.bkInputName],
                    cssClass = $attrs.bkHasErrorClass || 'has-error',
                    parent = $element.parents($attrs.bkHasErrorParentSelector || '.form-group:first'),
                    messages = ($attrs.bkMessages && $parse($attrs.bkMessages)($scope)) || {};

                if (!inputCtrl) {
                    throw 'Can\'t find input ' + $attrs.bkInputName + ' into form.';
                }

                if (0 === parent.length) {
                    parent = $element.parent();
                }

                function showErrorText(newValue, oldValue) {
                    if (inputCtrl.$invalid && ctrl.bkShowErrors()) {
                        var i, key, e, etext = [],
                            error = inputCtrl.$error,
                            inputMessages = messages[$attrs.bkInputName] || {};

                        for (i in error) {
                            if (error[i] && i !== 'parse') {
                                key = 'bkMsg' + angular.uppercase(i.charAt(0)) + i.substr(1);
                                e = $attrs[key];
                                if (!e) {
                                    e = inputMessages[i];
                                    if (!e && messages.common) {
                                        e = messages.common[i];
                                    }

                                    if (e && angular.isFunction(e)) {
                                        e = e(inputCtrl, ctrl);
                                    }

                                    if (!e) {
                                        e = (i + ' no message text!');
                                    }
                                }
                                etext.push(e);
                            }
                        }

                        $element.html(etext.join('&nbsp;'));
                        $element.removeClass('hidden');
                        parent.addClass(cssClass);
                    }
                    else {
                        parent.removeClass(cssClass);
                        if (!$element.hasClass('hidden')) {
                            $element.addClass('hidden');
                            $element.html('');
                        }
                    }
                };

                $scope.$watchCollection(function () {
                    return inputCtrl.$error;
                }, showErrorText);
                $scope.$watch(function () {
                    return inputCtrl.$invalid && ctrl.bkShowErrors();
                }, showErrorText);
            }
        };
    }];
    //#endregion

    //#region form
    var form = ['$parse', '$timeout', function ($parse, $timeout) {
        return {
            require: 'form',
            restrict: 'E',
            link: function ($scope, $element, $attrs, ctrl) {
                var form = $element, submitValid, valid, showErrors = false,
                    name = $element[0].nodeName,
                    ngForm = 'NG-FORM' === name;

                if (ngForm) {
                    form = $element.parents('form:first');
                }

                function focusFirstInvalid() {
                    $timeout(function () {
                        if (!ctrl.$valid) {
                            // find the first invalid element
                            var e = $element.find('.ng-invalid:visible:not(:disabled):first');
                            if (0 === e.length) {
                                e = $element.find('.ng-valid:visible:not(:disabled):first');
                            }
                            focusElement(e);
                        }
                    }, 0, false);
                }

                form.on('submit', focusFirstInvalid);
                $scope.$on('$destroy', function () {
                    form.off('submit', focusFirstInvalid);
                    form = null;
                });

                ctrl.bkFocus = focusFirstInvalid;
                ctrl.bkShowErrors = function () {
                    var onSubmit = (ctrl.$submitted || (ctrl.$$parentForm.$submitted !== undefined && ctrl.$$parentForm.$submitted)) &&
                        (ctrl.bkShowErrorsOnSubmit || (ctrl.$$parentForm.bkShowErrorsOnSubmit !== undefined && ctrl.$$parentForm.bkShowErrorsOnSubmit));

                    return (onSubmit || showErrors);
                };

                if ($attrs.bkShowErrors) {
                    $scope.$watch($parse($attrs.bkShowErrors), function (newValue, oldValue) {
                        showErrors = newValue === undefined ? false : newValue;
                    });
                }

                if ($attrs.bkShowErrorsOnSubmit && !ngForm) {
                    ctrl.bkShowErrorsOnSubmit = !('false' === $attrs.bkShowErrorsOnSubmit) || undefined === $attrs.bkShowErrorsOnSubmit;
                }
                else {
                    if (!ngForm) {
                        ctrl.bkShowErrorsOnSubmit = true;
                    }
                }

                if ($attrs.bkShowErrorsValid) {
                    submitValid = $parse($attrs.bkShowErrorsValid);
                    $scope.$watch(function () {
                        return ctrl.bkShowErrors() ? ctrl.$valid : undefined;
                    }, function (newValue) {
                        submitValid.assign($scope, newValue);
                    });
                }

                if ($attrs.bkValid) {
                    valid = $parse($attrs.bkValid);
                    $scope.$watch(function () {
                        return ctrl.$valid;
                    }, function (newValue) {
                        valid.assign($scope, newValue);
                    });
                }
            }
        };
    }];
    //#endregion

    //#region bkDisablePristine
    var bkDisablePristine = [function () {
        return {
            require: '^form',
            restrict: 'A',
            link: function ($scope, $element, $attrs, ctrl) {
                $scope.$watch(function () {
                    return ctrl.$pristine;
                }, function (newValue, oldValue) {
                    $element.prop('disabled', newValue);
                });
            }
        };
    }];
    //#endregion

    //#region bkAutoFocus
    var bkAutoFocus = ['$timeout', '$parse', function ($timeout, $parse) {
        return {
            restrict: 'AC',
            link: function ($scope, $element, $attrs) {
                var setFocusOn = $attrs.bkAutoFocus ? $parse($attrs.bkAutoFocus) : undefined;

                function runFocusElement() {
                    $timeout(function () {
                        focusElement($element);
                    }, 100, false);
                }

                if (!setFocusOn) {
                    runFocusElement();
                }
                else {
                    $scope.$watch(function () {
                        return setFocusOn($scope);
                    }, function (newValue) {
                        if (newValue) {
                            runFocusElement();
                        }
                    })
                }
            }
        };
    }];
    //#endregion        

    //#region bkAlert
    var bkAlert = function () {
        return {
            template: '<div class="alert" ng-class="cssClass" role="alert">\
    <button ng-show="closeable" type="button" class="close" ng-click="closeAlert()">\
        <span aria-hidden="true">&times;</span>\
        <span class="sr-only">Затвори</span>\
    </button>\
    <div ng-transclude></div>\
</div>',
            transclude: true,
            scope: {
                type: '@',
                timeout: '@',
                hide: '=?',
                close: '&'
            },
            restrict: 'EA',
            controller: ['$scope', '$attrs', '$timeout', function ($scope, $attrs, $timeout) {
                function cancelTimeout() {
                    if ($scope.timeoutPromise) {
                        $timeout.cancel($scope.timeoutPromise);
                        $scope.timeoutPromise = null;
                    }
                }
                function runTimeout() {
                    if ($scope.closeableOnTimeout && !$scope.hide) {
                        cancelTimeout();
                        $scope.timeoutPromise = $timeout($scope.closeAlert,
                            'default' === $scope.timeout ? 10 * 1000 : $scope.timeout);
                    }
                }

                if (!('hide' in $attrs)) {
                    $scope.hide = false;
                }

                $scope.closeableOnTimeout = 'timeout' in $attrs;
                $scope.closeable = 'close' in $attrs || $scope.closeableOnTimeout;

                $scope.cssClass = {};
                $scope.cssClass.hidden = true;
                $scope.cssClass['alert-' + ($attrs['type'] || 'warning')] = true;
                $scope.cssClass['alert-dismissable'] = $scope.closeable;

                $scope.closeAlert = function () {
                    cancelTimeout();
                    $scope.hide = true;
                    $scope.close();
                }

                if ($scope.closeableOnTimeout) {
                    $scope.$on('$destroy', function () {
                        cancelTimeout();
                    });
                }

                $scope.$watch('hide', function (newValue) {
                    $scope.cssClass.hidden = newValue == undefined ? false : newValue;
                    runTimeout();
                });

                runTimeout();
            }]
        };
    };
    //#endregion

    //#region bkMenu
    var bkMenu = [function () {
        return {
            require: '^dropdown',
            restrict: 'CA',
            link: function ($scope, $element, $attrs, dropdownCtrl) {

                function menuDescriptor(a) {
                    var menuHref = a.attr('href'), menuValue;
                    if (menuHref) {
                        menuValue = (menuHref.charAt(0) === '#' ? menuHref.substr(1) : '') || undefined;
                    }

                    return {
                        $href: menuHref,
                        $id: a.attr('id'),
                        $name: a.text()
                    };
                }

                function onClick(event) {
                    event.preventDefault();

                    var a = angular.element(event.target),
                        context;

                    if ('A' !== a[0].nodeName) {
                        a = a.find('a');
                    }

                    context = menuDescriptor(a);

                    $scope.$apply(function () {
                        dropdownCtrl.toggle();
                    });

                    $scope.$apply(function () {
                        $scope.bkMenu(context);
                    }, 0);
                }

                $element.on('click', onClick);

                $scope.$on('$destroy', function () {
                    $element.off('click', onClick);
                });
            },
            scope: {
                bkMenu: '&'
            }
        };
    }];
    //#endregion

    //#region bkSearchMenu
    var bkSearchMenu = [function () {
        return {
            require: '^dropdown',
            restrict: 'CA',
            link: function ($scope, $element, $attrs, dropdownCtrl) {

                function setCurrentMenu(a) {
                    var menuHref = a.attr('href'), menuValue;
                    if (menuHref) {
                        menuValue = (menuHref.charAt(0) === '#' ? menuHref.substr(1) : '') || undefined;
                    }

                    $scope.bkSearchMenuText = a.text();
                    $scope.bkSearchMenuValue = undefined;
                    if (menuValue) {
                        $scope.bkSearchMenuColumn = menuValue;
                        $scope.bkSearchMenuDisabled = false;
                    }
                    else {
                        $scope.bkSearchMenuColumn = undefined;
                        $scope.bkSearchMenuDisabled = true;
                    }

                    return {
                        $href: menuHref,
                        $id: a.attr('id'),
                        $name: $scope.bkSearchMenuText,
                        $disabled: $scope.bkSearchMenuDisabled
                    };
                }

                function onClick(event) {
                    event.preventDefault();

                    var a = angular.element(event.target),
                        context;
                    if ('A' !== a[0].nodeName) {
                        a = a.find('a');
                    }

                    context = setCurrentMenu(a);

                    $scope.$apply(function () {
                        dropdownCtrl.toggle();
                    });

                    var e = $element.parent().parent().parent().find('input:visible,select:visible');
                    focusElement(e);

                    $scope.$apply(function () {
                        $scope.bkSearchMenu(context);
                    }, 0);
                }

                setCurrentMenu(
                    $element.find('a:first'));

                $element.on('click', onClick);

                $scope.$on('$destroy', function () {
                    $element.off('click', onClick);
                });
            },
            scope: {
                bkSearchMenu: '&',
                bkSearchMenuColumn: '=?',
                bkSearchMenuValue: '=?',
                bkSearchMenuText: '=?',
                bkSearchMenuDisabled: '=?'
            }
        };
    }];
    //#endregion

    //#region bkSearchFilter
    var bkSearchFilter = ['$log', 'bk.utils', 'bk.dateParser', 'dateFilter', function ($log, utils, dateParser, dateFilter) {
        return {
            template: '<form novalidate ng-submit="executeSearch()">\
            <div class="input-group">\
                <div class="input-group-btn">\
                    <div class="btn-group dropdown" dropdown>\
                        <button type="button" class="btn btn-default dropdown-toggle" dropdown-toggle title="Избор на поле, по което да се търси...">\
                            <span ng-bind="searchText"></span>\
                            <span class="caret"></span>\
                        </button>\
                        <ul class="dropdown-menu"\
            bk-search-menu="executeSearch($disabled)"\
            bk-search-menu-column="context.searchColumn"\
            bk-search-menu-value="value"\
            bk-search-menu-text="searchText"\
            bk-search-menu-disabled="searchDisabled">\
            <li><a href="#">Без</a></li>\
            <li class="divider"></li>\
            <li ng-repeat="item in columnsSearchBy"><a ng-attr-href="#{{item.name}}">{{title(item)}}</a></li>\
        </ul>\
    </div>\
</div>\
<input type="text" class="form-control" placeholder="Филтър..." ng-model="value" ng-disabled="searchDisabled" ng-show="input" />\
<select type="text" class="form-control" ng-model="value" ng-options="obj.value as obj.text for obj in options" ng-disabled="searchDisabled" ng-show="select"></select>\
<div class="input-group-btn">\
    <button type="submit" class="btn btn-default" title="Търси..." ng-disabled="searchDisabled || !valid">\
        <span class="glyphicon glyphicon-search"></span>\
    </button>\
</div>\
</div>\
</form>',
            restrict: 'E',
            link: function ($scope, $element, $attrs, dropdownCtrl) {

                $scope.options = [];
                $scope.input = true;
                $scope.select = false;
                $scope.valid = true;
                $scope.value = undefined;
                $scope.column = undefined;
                $scope.columnsSearchBy = [];
                $scope.title = function (column) {
                    var title = '';
                    if (column.group) {
                        title = '(' + (column.group.titleShort || column.group.title) + ') ';
                    }
                    title += (column.titleShort || column.title);
                    return title;
                };

                angular.forEach($scope.items, function (item, key) {
                    if (item.searchable === undefined || item.searchable) {
                        $scope.columnsSearchBy.push(item);
                    }
                });

                $scope.executeSearch = function (disabled) {
                    if (undefined === disabled || disabled) {
                        $scope.execute();
                    }
                }

                $scope.$watch('context.searchColumn', function (newValue) {
                    var column = $scope.column = $scope.columnsSearchBy.filter(function (item) { return item.name === newValue; })[0];
                    $scope.valid = true;
                    $scope.value = undefined;
                    $scope.options.length = 0;
                    if (column && column.reference) {
                        $scope.select = true;
                        $scope.input = false;
                        $scope.options.push({ value: undefined, text: 'Филтър ...' });
                        angular.forEach(column.reference, function (item, key) {
                            $scope.options.push({ value: key, text: item });
                        });
                    }
                    else {
                        $scope.select = false;
                        $scope.input = true;
                    }
                });

                $scope.$watch('value', function (newValue, oldValue) {
                    $scope.valid = true;
                    if ($scope.column && $scope.column.searchType && newValue) {
                        newValue = newValue.split(';');
                        switch ($scope.column.searchType) {
                            case 'n':
                                for (var i in newValue) {
                                    newValue[i] = utils.parseNumber(newValue[i]);
                                    $scope.valid = (newValue[i] !== undefined) && $scope.valid;
                                }
                                $scope.valid = newValue.length <= 2 && $scope.valid;
                                break;
                            case 'd':
                                for (var i in newValue) {
                                    newValue[i] = dateParser.parse(newValue[i], 'medium') ||
                                        dateParser.parse(newValue[i], 'short') ||
                                        dateParser.parse(newValue[i], 'shortDate');

                                    if (newValue[i] !== undefined) {
                                        newValue[i] = dateFilter(newValue[i], 'yyyy-MM-ddTHH:mm:ss');
                                    }

                                    $scope.valid = (newValue[i] !== undefined) && $scope.valid;
                                }
                                $scope.valid = newValue.length <= 2 && $scope.valid;
                                break;
                        }
                    }

                    $scope.context.searchValue = ($scope.valid ? newValue : undefined);

                    $element.find('.input-group')[$scope.valid ? 'removeClass' : 'addClass']('has-error');
                });
            },
            scope: {
                items: '=?',
                context: '=?',
                execute: '&'
            }
        };
    }];
    //#endregion

    //#region bkGridPagerService
    var bkGridPagerService = ['$timeout', function ($timeout) {

        function Pager($scope) {
            var self = this;

            this.firstPageDisabled = true;
            this.previousPageDisabled = true;
            this.nextPageDisabled = true;
            this.lastPageDisabled = true;

            this.pageSizes = [{ size: 10 }, { size: 20 }, { size: 50 }, { size: 100 }];
            this.pageSizesBySize = {};

            this.firstPage = function () {
                if (!this.firstPageDisabled && $scope.bkContext.page !== 1) {
                    $scope.bkContext.page = 1;
                    this.enableButtons(true);
                }
            };

            this.previousPage = function () {
                if (!this.previousPageDisabled && $scope.bkContext.page > 1) {
                    $scope.bkContext.page -= 1;
                    this.enableButtons(true);
                }
            };

            this.refreshPage = function () {
                this.enableButtons(true);
            };

            this.nextPage = function () {
                if (!this.nextPageDisabled) {
                    $scope.bkContext.page += 1;
                    if ($scope.bkContext.lastPage < $scope.bkContext.page) {
                        $scope.bkContext.lastPage = $scope.bkContext.page;
                    }
                    this.enableButtons(true);
                }
            };

            this.lastPage = function () {
                if (!this.lastPageDisabled && $scope.bkContext.page < $scope.bkContext.lastPage) {
                    $scope.bkContext.page = $scope.bkContext.lastPage;
                    this.enableButtons(true);
                }
            };

            this.changePageSize = function (page) {
                if (page.size !== $scope.bkContext.pageSize) {
                    this.pageSizesBySize[$scope.bkContext.pageSize].active = false;
                    this.pageSizesBySize[page.size].active = true;
                    $scope.bkContext.pageSize = page.size;
                    $scope.bkContext.page = 1;
                    $scope.bkContext.lastPage = 1;
                    $scope.bkRowCount = 0;
                    this.enableButtons(true);
                }
            };

            this.enableButtons = function (call) {
                this.firstPageDisabled = $scope.bkContext.page === 1;
                this.previousPageDisabled = ($scope.bkContext.page - 1) <= 0;
                this.nextPageDisabled = (0 !== $scope.bkRowCount) && ($scope.bkRowCount < ($scope.bkContext.page * $scope.bkContext.pageSize));
                this.lastPageDisabled = $scope.bkContext.page >= $scope.bkContext.lastPage;
                if (call) {
                    $timeout(function () {
                        $scope.bkRefresh();
                    }, 0, false);
                }
            };

            this.init = function () {
                if (!$scope.bkContext.page || $scope.bkContext.page <= 0) {
                    $scope.bkContext.page = 1;
                }
                if (!$scope.bkContext.lastPage || $scope.bkContext.lastPage < $scope.bkContext.page) {
                    $scope.bkContext.lastPage = $scope.bkContext.page;
                }
                if (!$scope.bkRowCount || $scope.bkRowCount < 0) {
                    $scope.bkRowCount = 0;
                }
                if (!$scope.bkContext.pageSize) {
                    $scope.bkContext.pageSize = 10;
                }

                var i, item, active;
                for (i in this.pageSizes) {
                    item = this.pageSizes[i];
                    if (item.size === $scope.bkContext.pageSize) {
                        item.active = active = true;
                    }
                    this.pageSizesBySize[item.size] = item;
                }

                if (!active) {
                    $scope.bkContext.pageSize = this.pageSizes[0].size;
                    this.pageSizes[0].active = true;
                }

                this.enableButtons();

                $scope.$watch('bkRowCount', function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        self.enableButtons();
                    }
                });
            };

            this.init();
        }

        return function ($scope) {
            return new Pager($scope);
        }
    }];
    //#endregion

    //#region bkGridPager
    var bkGridPager = ['bkGridPagerService', function (pagerService) {
        return {
            restrict: 'AC',
            template: '<div class="col-md-6"> \
    <ul class="pagination-sm pagination"> \
        <li ng-class="{disabled:pager.firstPageDisabled}"><a title="начало" ng-click="pager.firstPage()">Първа</a></li>\
        <li ng-class="{disabled:pager.previousPageDisabled}"><a title="предходна" ng-click="pager.previousPage()">«</a></li>\
        <li class="active">\
            <a title="избери за да опресниш данните ..." ng-click="pager.refreshPage()">{{bkContext.page}}</a>\
        </li>\
        <li ng-class="{disabled:pager.nextPageDisabled}"><a title="следваща" ng-click="pager.nextPage()">»</a></li>\
        <li ng-class="{disabled:pager.lastPageDisabled}"><a title="последна достигната" ng-click="pager.lastPage()">Последна</a></li>\
    </ul>\
</div>\
<div class="col-md-6">\
    <div class="btn-group btn-group-sm bk-grid-page-sizes pull-right" title="брой редове на страница">\
        <a class="btn btn-default"\
            ng-repeat="pageSize in pager.pageSizes"\
            ng-class="{active:pageSize.active}"\
            ng-click="pager.changePageSize(pageSize)">{{pageSize.size}}</a>\
    </div>\
</div>',
            scope: {
                bkRefresh: '&',
                bkContext: '=?',
                bkRowCount: '=?'
            },
            link: function ($scope, $element, $attrs) {
                $element.addClass('row');
                $scope.pager = pagerService($scope);
            }
        };
    }];
    //#endregion

    //#region bkGridTableService
    var bkGridTableService = ['$timeout', '$compile', '$filter', '$parse', '$templateCache', '$log', function ($timeout, $compile, $filter, $parse, $templateCache, $log) {
        var templateHeaderColumns = '<tr><th bk-grid-table-header-cell scope="col" ng-repeat="column in headerColumns"></th></tr>';

        return {
            compileTemplate: function ($element, $attrs) {
                var td, template;
                if ($attrs.bkTemplateUrl) {
                    template = $templateCache.get($attrs.bkTemplateUrl);
                    if (template) {
                        $element.find('thead > tr:first').prepend('<th scope="col" ng-attr-rowspan="{{rowSpan}}">...</th>');
                        $element.find('tbody > tr:first').prepend(
                            td = angular.element('<td class="center"></td>'));

                        td.append(template);
                    }
                    else {
                        $log.log('Can\'t find template with name ' + $attrs.bkTemplateUrl + '.');
                    }
                }

                if ($attrs.bkFoot) {
                    $element.find('div:first').append('<div class="tfoot"><table class="table table-condensed table-bordered"><tfoot><tr>\
<th ng-repeat="column in bkFoot" ng-attr-colspan="{{column.colSpan}}" ng-attr-title="{{column.help}}" ng-class="utils.cssClass(column)" ng-bind-html="utils.formatFoot(this, column)"></th>\
</tr></tfoot></table></div>');
                }
            },
            title: function (column) {
                if (column.sortable !== undefined && !column.sortable) {
                    return column.title;
                }

                var sortIndicator = '';
                if (column.sortDir) {
                    sortIndicator = column.sortDir === 'asc' ? ' ▲' : ' ▼';
                }

                return column.title + sortIndicator;
            },
            sortable: function (column) {
                return (!(column.sortable !== undefined && !column.sortable) && column.colSpan === undefined);
            },
            sortBy: function (scope, column) {
                switch (column.sortDir || 'desc') {
                    case 'desc':
                        column.sortDir = 'asc';
                        break;
                    default:
                        column.sortDir = 'desc';
                        break;
                }
                scope.bkContext.sort = column.name;
                scope.bkContext.sortDir = column.sortDir;
                $timeout(function () {
                    scope.bkRefresh();
                }, 0, false);
            },
            format: function (scope, column, rowData) {
                var value, filter, args;
                if (column.compiledExpression) {
                    value = column.compiledExpression(scope);
                }
                else {
                    value = rowData[column.name];
                }

                if (column.reference) {
                    value = column.reference[value] || value;
                }
                if (column.filter) {
                    filter = $filter(column.filter.name);
                    args = [value];
                    if (column.filter.args) {
                        args.push(column.filter.args);
                    }
                    value = filter.apply(null, args)
                }
                return value;
            },
            formatFoot: function (scope, column) {
                var filter, args, value;
                if (column.compiledExpression) {
                    value = column.compiledExpression(scope);
                    if (column.reference) {
                        value = column.reference[value] || value;
                    }
                    if (column.filter) {
                        filter = $filter(column.filter.name);
                        args = [value];
                        if (column.filter.args) {
                            args.push(column.filter.args);
                        }
                        value = filter.apply(null, args)
                    }
                }
                return value;
            },
            cssClass: function (column, row) {
                return angular.isFunction(column.cssClass) ?
                    column.cssClass(column, row) :
                    column.cssClass;
            },
            noData: function (rows) {
                return !(rows && rows.length);
            },
            init: function ($scope, $element, $attrs) {
                $scope.noData = $attrs.bkNoData || '<b>Внимание!</b>&nbsp;Не са открити данни.';
                $scope.columnsByName = {};
                $scope.headerGroups = [];
                $scope.headerColumns = [];
                $scope.currentSortColumn = null;

                var tr, sortable = false;
                angular.forEach($scope.bkColumns, function (item, key) {
                    if (this.sortable(item)) {
                        sortable = true;
                    }

                    if (item.sortDir && $scope.bkContext) {
                        $scope.bkContext.sort = item.name;
                        $scope.bkContext.sortDir = item.sortDir;
                    }

                    if (item.group) {
                        if ($scope.headerGroups[$scope.headerGroups.length - 1] !== item.group) {
                            item.group.colSpan = 0;
                            $scope.headerGroups.push(item.group);
                        }
                        item.group.colSpan += 1;
                        $scope.headerColumns.push(item);
                    }
                    else {
                        $scope.headerGroups.push(item);
                    }

                    if (item.expression) {
                        item.compiledExpression = angular.isFunction(item.expression) ?
                            item.expression : $parse(item.expression);
                    }
                    else {
                        if (item.name.indexOf('.') != -1) {
                            item.compiledExpression = $parse('row.' + item.name);
                        }
                    }

                    $scope.columnsByName[item.name] = item;
                }, this);

                angular.forEach($scope.bkFoot, function (item, key) {
                    if (item.expression) {
                        item.compiledExpression = angular.isFunction(item.expression) ?
                            item.expression : $parse(item.expression);
                    }
                }, this);

                //watch sort definition to show it into column header
                if ($scope.bkContext && sortable) {
                    $scope.$watch(
                        function () {
                            return $scope.bkContext.sort + '$' + $scope.bkContext.sortDir;
                        },
                        function (newValue, oldValue) {
                            var column = $scope.columnsByName[$scope.bkContext.sort];
                            if (column) {
                                if (column !== $scope.currentSortColumn) {
                                    if ($scope.currentSortColumn) {
                                        $scope.currentSortColumn.sortDir = null;
                                    }
                                    $scope.currentSortColumn = column;
                                }
                                column.sortDir = $scope.bkContext.sortDir;
                            }
                        });
                }

                if ($scope.headerColumns.length) {
                    $scope.rowSpan = 2;
                    $element.find('thead').append(tr = angular.element(templateHeaderColumns));
                    $compile(tr.contents())($scope);
                }

                tr = $element.find('tfoot tr');
                if (tr.length) {
                    $compile(tr.contents())($scope);
                }

                if (($scope.bkLoad !== undefined || $scope.bkLoad === 'true') && $scope.bkRefresh) {
                    $timeout(function () {
                        $scope.bkRefresh();
                    }, 0, false);
                }
            }
        };
    }];
    //#endregion

    //#region bkGridTableHeader
    var bkGridTableHeaderCell = ['$compile', 'bkGridTableService', function ($compile, utils) {
        return {
            link: function ($scope, $element, $attrs) {
                var attrs = {};
                if ($scope.column.help) {
                    attrs.title = $scope.column.help;
                }
                if ($scope.column.colSpan) {
                    attrs.colSpan = $scope.column.colSpan;
                }
                else {
                    if ($scope.rowSpan && $scope.column.group === undefined) {
                        attrs.rowSpan = $scope.rowSpan;
                    }
                }
                $element.attr(attrs);
                if (utils.sortable($scope.column)) {
                    $scope.utils = utils;
                    $element.html('<a ng-click="utils.sortBy($parent, column)" ng-bind-html="utils.title(column)"></a>');
                    $compile($element.contents())($scope);
                }
                else {
                    $element.text(utils.title($scope.column));
                }
            }
        };
    }];
    //#endregion

    //#region bkGridTable
    var bkGridTable = ['bkGridTableService', function (utils) {
        return {
            restrict: 'AC',
            template: '<div class="col-md-12 table-responsive">\
    <table class="table table-striped table-hover table-condensed table-bordered">\
        <thead>\
            <tr>\
                <th bk-grid-table-header-cell scope="col" ng-repeat="column in headerGroups"></th>\
            </tr>\
        </thead>\
        <tbody>\
            <tr ng-repeat="row in bkRows" ng-init="ctrl = $parent.$parent; dir = $parent;">\
                <td ng-repeat="column in bkColumns" ng-class="utils.cssClass(column, row)" ng-bind-html="utils.format(this, column, row)">\
                </td>\
            </tr>\
        </tbody>\
    </table>\
    <div class="alert alert-warning bk-grid-nodata" ng-show="utils.noData(bkRows)" ng-bind-html="noData"></div>\
</div>',
            scope: {
                bkRefresh: '&',
                bkColumns: '=?',
                bkRows: '=?',
                bkContext: '=?',
                bkLoad: '@'
            },
            compile: function compile(tElement, tAttrs) {

                utils.compileTemplate(tElement, tAttrs);

                return {
                    post: function ($scope, $element, $attrs, $ctrls, $transclude) {
                        $scope.utils = utils;

                        if (!$element.hasClass('row')) {
                            $element.addClass('row');
                        }

                        // initialization
                        // check columns defintions for sort order, columns grouping, expression defined on column for displaying data
                        utils.init($scope, $element, $attrs);
                    }
                };
            }
        };
    }];
    //#endregion

    //#region bkInputCase
    var bkCase = function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ctrl) {

                ctrl.$render = function () {
                    //element.val(ctrl.$isEmpty(ctrl.$modelValue) ? '' : ctrl.$viewValue);
                    element.val(ctrl.$viewValue);
                };

                function changeCase(val, format, render) {
                    if (val) {
                        var u = format ? angular.uppercase(val) : angular.lowercase(val);
                        if (u !== val) {
                            val = u;
                            if (render) {
                                ctrl.$viewValue = val;
                                ctrl.$render();
                            }
                        }
                    }
                    return val;
                }

                if ('upper' === attrs.bkCase) {
                    ctrl.$parsers.unshift(function (inputValue) { return changeCase(inputValue, true, true); });
                    //ctrl.$formatters.push(function (inputValue) { return changeCase(inputValue, true, false); }); to be shown as is
                }
                else {
                    if ('lower' === attrs.bkCase) {
                        ctrl.$parsers.unshift(function (inputValue) { return changeCase(inputValue, false, true) });
                        //ctrl.$formatters.push(function (inputValue) { return changeCase(inputValue, false, false) });to be shown as is
                    }
                }
            }
        };
    };
    //#endregion

    //#region bkPreload
    var bkPreload = ['$q', '$timeout', function ($q, $timeout) {
        //credits to link below for idea
        //http://www.bennadel.com/blog/2555-preloading-data-before-executing-nginclude-in-angularjs.htm
        // Return directive configuration.
        // NOTE: ngSwitchWhen priority is 500.
        // NOTE: ngInclude priority is 0.

        var templateLoadindIndicator = '<div class="row"><div class="col-md-12"><div class="bk-progress-loading progress progress-striped active">\
                <div class="progress-bar" role="progressbar">\
                    <div class="progress-bar-label">Зареждане...</div>\
                </div>\
            </div></div></div>';

        return ({
            restrict: 'AE',
            priority: 250,
            transclude: 'element',
            link: function ($scope, element, attrs, ctrls, transclude) {
                var injectedElement,
                    loadingElement,
                    timeoutPromise,
                    timeout = parseInt(attrs.bkTimeout || attrs.timeout || 500, 10) || 500;

                function destroy() {
                    $timeout.cancel(timeoutPromise);
                    timeoutPromise = null;

                    if (loadingElement) {
                        loadingElement.remove();
                        loadingElement = null;
                    }

                    if (injectedElement) {
                        injectedElement.remove();
                        injectedElement = null;
                    }
                }

                timeoutPromise = $timeout(function () {//if exceed 500 ms to load data then show loading ...
                    element.after(loadingElement = angular.element(templateLoadindIndicator));
                }, timeout, false);

                // bkPreload - when used as attribute
                // promise - when used as element
                $q.all($scope.$eval(attrs.bkPreload || attrs.promise)).finally(function () {
                    if (!timeoutPromise) {
                        return;// called destroy
                    }
                    transclude($scope, function (copy) {
                        destroy();
                        element.after(injectedElement = copy);
                    });
                });

                $scope.$on("$destroy", destroy);
            }
        });
    }];
    //#endregion

    angular.module('bk.directives', ['bk.dateParser'])
        .directive('bkErrorTitle', bkErrorTitle)
        .directive('bkErrorText', bkErrorText)
        .directive('form', form)
        .directive('ngForm', form)
        .directive('bkDisablePristine', bkDisablePristine)
        .directive('bkAutoFocus', bkAutoFocus)
        .directive('bkAlert', bkAlert)
        .directive('bkMenu', bkMenu)
        .directive('bkSearchMenu', bkSearchMenu)
        .directive('bkSearchFilter', bkSearchFilter)
        .factory('bkGridPagerService', bkGridPagerService)
        .directive('bkGridPager', bkGridPager)
        .factory('bkGridTableService', bkGridTableService)
        .directive('bkGridTable', bkGridTable)
        .directive('bkGridTableHeaderCell', bkGridTableHeaderCell)
        .directive('bkCase', bkCase)
        .directive('bkNumber', ['$locale', 'bk.utils', 'numberFilter', function ($locale, utils, numberFilter) {
            var decimalSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP;

            function normalizeNumberSize(attrs) {
                var numberPrecision = (attrs.bkNumber || '15,2').split(',');

                if (!numberPrecision[0] || isNaN(numberPrecision[0])) {
                    numberPrecision[0] = '15';
                }
                if (!numberPrecision[1] || isNaN(numberPrecision[1])) {
                    numberPrecision[1] = '0';
                }

                numberPrecision[0] = parseInt(numberPrecision[0], 10);
                numberPrecision[1] = parseInt(numberPrecision[1], 10);
                return numberPrecision;
            }

            return {
                priority: 1,
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var numberPrecision = normalizeNumberSize(attrs);
                    ctrl.$render = function () {
                        element.val(ctrl.$isEmpty(ctrl.$viewValue) ? '' : ctrl.$viewValue);
                    };

                    ctrl.$parsers.push(function (value) {
                        if (!ctrl.$isEmpty(value)) {
                            value = utils.normalizeNumber(value);
                            value = parseFloat(value);
                            if (isNaN(value)) {
                                return undefined;
                            }
                        }
                        return value;
                    });

                    ctrl.$validators.number = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue)) {
                            var parts, normalizeVal, length = 0, length0 = 0;
                            if (isNaN(modelValue)) {
                                valid = false;
                            }
                            else {
                                normalizeVal = utils.normalizeNumber(modelValue);
                                parts = normalizeVal.split('.');
                                if (parts[0]) {//преди десетичната
                                    length0 = parts[0].length;
                                    length += length0;
                                }
                                if (parts[1]) {//след десетичната
                                    length += parts[1].length;
                                    if (numberPrecision[1] === 0 || parts[1].length > numberPrecision[1]) {
                                        valid = false;//броя символи след десетичната запетая не отговаря на допустимия
                                    }
                                }
                                if (length > numberPrecision[0]//като цяло
                                    ||
                                    length0 > (numberPrecision[0] - numberPrecision[1])//преди десетичната запетая
                                    ) {
                                    //броя цифри в числото не съответсва на това което може да се запази
                                    valid = false;
                                }
                            }
                        }
                        return valid;
                    };

                    ctrl.$formatters.push(function (value) {
                        if (!ctrl.$isEmpty(value)) {
                            if (!angular.isNumber(value)) {
                                value = utils.parseNumber(value);
                            }
                            value = numberFilter(value, numberPrecision[1]);
                        }
                        return value;
                    });

                    function keyPressCharacterFilter(a) {
                        var val = (ctrl.$viewValue || ''),
                            valLength = val.length,
                            sepIndex = val.indexOf(decimalSeparator);

                        if (!((a.keyCode === 13) ||
                          (48 <= a.keyCode && a.keyCode <= 57) ||//0-9
                          (String.fromCharCode(a.keyCode) === '-') || //????????
                          (String.fromCharCode(a.keyCode) === decimalSeparator && numberPrecision[1] != 0 && sepIndex === -1)) ||
                          (valLength >= numberPrecision[0])) {
                            a.preventDefault();
                        }
                    }

                    function blurFormat(a) {
                        if (/*ctrl.$valid && */!ctrl.$isEmpty(ctrl.$modelValue)) {
                            ctrl.$viewValue = numberFilter(ctrl.$modelValue, numberPrecision[1]);
                            ctrl.$render();
                        }
                    }

                    function focusFormat(a) {
                        if (!ctrl.$isEmpty(ctrl.$viewValue)) {
                            ctrl.$viewValue = utils.normalizeNumber(ctrl.$viewValue, false);
                            ctrl.$render();
                        }
                    }

                    element.on('keypress', keyPressCharacterFilter)
                        .on('blur', blurFormat)
                        .on('focus', focusFormat);
                    scope.$on('$destroy', function () {
                        element.off('keypress', keyPressCharacterFilter)
                            .off('blur', blurFormat)
                            .off('focus', focusFormat);
                    });

                }
            };
        }])
        .directive('bkNumberMax', ['bk.utils', function (utils) {
            return {
                priority: 2,//за да се добавят след bkNumber и да не валидират невалидни данни
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var numberMax = parseFloat(attrs.bkNumberMax || '0');
                    ctrl.$validators.numberMax = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue) && !isNaN(modelValue)) {
                            valid = modelValue < numberMax;
                        }
                        return valid;
                    };
                }
            };
        }])
        .directive('bkNumberMin', ['bk.utils', function (utils) {
            return {
                priority: 2,//за да се добавят след bkNumber и да не валидират невалидни данни
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var numberMin = parseFloat(attrs.bkNumberMin || '0');
                    ctrl.$validators.numberMin = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue) && !isNaN(modelValue)) {
                            valid = modelValue > numberMin;
                        }
                        return valid;
                    };
                }
            };
        }])
        .directive('bkNumberNotZero', [function () {
            return {
                priority: 2,//за да се добавят след bkNumber и да не валидират невалидни данни
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    ctrl.$validators.numberNotZero = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue) && !isNaN(modelValue)) {
                            valid = modelValue !== 0;
                        }
                        return valid;
                    };
                }
            };
        }])
        .directive('bkNumberOnlyZero', [function () {
            return {
                priority: 2,//за да се добавят след bkNumber и да не валидират невалидни данни
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    ctrl.$validators.numberOnlyZero = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue) && !isNaN(modelValue)) {
                            valid = modelValue === 0;
                        }
                        return valid;
                    };
                }
            };
        }])
        .directive('bkNumberRange', ['bk.utils', function (utils) {
            return {
                priority: 2,//за да се добавят след bkNumber и да не валидират невалидни данни
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var numberRange = (attrs.bkNumberRange || '0,0').split(',');
                    if (!numberRange[0] || isNaN(numberRange[0])) {
                        numberRange[0] = '0';
                    }
                    if (!numberRange[1] || isNaN(numberRange[1])) {
                        numberRange[1] = '0';
                    }
                    numberRange[0] = parseFloat(numberRange[0]);
                    numberRange[1] = parseFloat(numberRange[1]);

                    ctrl.$validators.numberRange = function (modelValue, viewValue) {
                        var valid = true;
                        if (!ctrl.$isEmpty(modelValue) && !isNaN(modelValue)) {
                            valid = numberRange[0] <= modelValue && modelValue <= numberRange[1];
                        }
                        return valid;
                    };
                }
            };
        }])
        .directive('bkValidators', [function () {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var func, validators = scope.$eval(attrs.bkValidators);
                    if (angular.isObject(validators)) {
                        for (var i in validators) {
                            func = validators[i];
                            if (angular.isFunction(func)) {
                                ctrl.$validators[i] = func;
                            }
                        }
                    }
                }
            };
        }])
        .directive('bkValidationFlags', [function () {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var validators = scope.$eval(attrs.bkValidationFlags);
                    if (angular.isObject(validators)) {
                        scope.$watchCollection(function () {
                            return validators;
                        }, function (newValue) {
                            var val;
                            for (var i in newValue) {
                                val = newValue[i];
                                ctrl.$setValidity(i, angular.isFunction(val) ? val() : val);
                            }
                        });
                    }
                }
            };
        }])
        .directive('bkServerModel', ['$parse', '$timeout', function ($parse, $timeout) {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var serverValidation = attrs.bkServerModel && $parse(attrs.bkServerModel);
                    if (serverValidation) {
                        scope.$watch(serverValidation, function (newValue) {
                            ctrl.$setValidity('serverModel', !!newValue);
                        });
                    }

                    ctrl.$validators.serverModel = function (modelValue, viewValue) {
                        return true;
                    };

                    function focusElement() {
                        $timeout(function () {
                            ctrl.$setValidity('serverModel', true);
                        }, 30 * 1000);
                    }

                    function blurElement() {
                        scope.$evalAsync(function () {
                            ctrl.$setValidity('serverModel', true);
                        })
                    }

                    element.on('focus', focusElement).on('blur', blurElement);
                    scope.$on('$destroy', function () {
                        element.off('focus', focusElement).off('blur', blurElement);
                    });
                }
            };
        }])
        .directive('bkBindDateAsIso', ['dateFilter', function (dateFilter) {
            return {
                priority: 100,
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    ctrl.$parsers.push(function (value) {
                        return value && value instanceof Date ? dateFilter(value, 'yyyy-MM-ddT00:00:00') : null;
                    });
                }
            };
        }])
        .directive('datepickerPopupWithButton', function () {
            return {
                restrict: 'AC',
                scope:{},
                link: function ($scope, $element) {
                    var input = $element.find('input'),
                        button = $element.find('button');

                    function onClick(event) {
                        event.stopPropagation();
                        event.preventDefault();
                        $scope.$apply(function () {
                            var scope = $scope.$$nextSibling;// datepickerPopup scope - hack
                            scope.isOpen = scope.isOpen === undefined ? true : !scope.isOpen;
                        });
                    }

                    if (input.length || button.length) {
                        input.on('click', onClick);
                        button.on('click', onClick);
                        $scope.$on('$destroy', function () {
                            button.off('click', onClick);
                            input.off('click', onClick);
                            button = input = null;
                        });
                    }
                }

            };
        })
        .directive('bkPreload', bkPreload)
        .directive('bkRepeatEnd', function () {
            return function (scope, element, attrs) {
                if (scope.$last && attrs.bkRepeatEnd) {
                    scope.$eval(attrs.bkRepeatEnd);
                }
            };
        })
        .directive('bkClick', ['$parse', '$timeout', function ($parse, $timeout) {
            return function (scope, element, attrs) {
                var expression, focusElement;
                function click(event) {
                    scope.$apply(function () {
                        var f, result = expression(scope);
                        if (undefined === result || result) {
                            if (!focusElement) {
                                focusElement = angular.element(attrs.bkClickFocus);
                            }

                            execFocusElement(result, focusElement);
                        }
                    });
                }

                if (attrs.bkClick && attrs.bkClickFocus) {
                    expression = $parse(attrs.bkClick);
                    element.on('click', click);
                    scope.$on('$destroy', function () {
                        element.off('click', click);
                        focusElement = null;
                    });
                }
            }
        }])
        .directive('bkClickToggle', ['$parse', function ($parse) {
            return function ($scope, $element, $attrs) {
                var flag = $parse($attrs.bkClickToggle),
                    init = flag($scope);

                if (undefined === init) {
                    flag.assign($scope, init = ($attrs.bkClickToggleInit === 'true'));
                }

                function click(event) {
                    event.stopPropagation();
                    $scope.$evalAsync(function () {
                        flag.assign($scope, !flag($scope));
                    });
                }

                function close(event) {
                    $scope.$evalAsync(function () {
                        flag.assign($scope, init);
                    });
                }

                angular.element(document).on('click', close);
                $element.on('click', click);
                $scope.$on('$destroy', function () {
                    angular.element(document).off('click', close);
                    $element.off('click', click);
                });

            }
        }])
        .directive('bkClickDirectFocus', ['$timeout', function ($timeout) {
            return function (scope, element, attrs) {

                function click(event) {
                    scope.$apply(function () {
                        var e = angular.element(attrs.bkClickDirectFocus).eq(0);
                        focusElement(e);
                    });
                }

                if (attrs.bkClickDirectFocus) {
                    element.on('click', click);
                    scope.$on('$destroy', function () {
                        element.off('click', click);
                    });
                }
            }
        }])
        .directive('bkFocus', ['$timeout', '$parse', function ($timeout, $parse) {
            return function (scope, element, attrs) {
                if (attrs.bkFocus) {
                    scope.$watch($parse(attrs.bkFocus), function (newValue) {
                        if (newValue) {
                            $timeout(function () {
                                var e = element.find(attrs.bkFocusSelector).eq(0);
                                focusElement(e);
                            }, 0, false);
                        }
                    });
                }
            }
        }])
        .directive('bkKeyUp', ['$parse', '$timeout', function ($parse, $timeout) {
            return function (scope, element, attrs) {
                var expression, keys, sysKeys = { alt: true, ctrl: true, shift: true };
                function keyUp(event) {
                    var items = [], doubleKeys = {};
                    for (var i in keys) {
                        if (keys[i].masterKey) {
                            if (event[keys[i].masterKey] && event.which === keys[i].key) {
                                doubleKeys[event.which] = true;
                                items.push(keys[i]);
                            }
                        }
                        else {
                            if (event.which === keys[i].key && !doubleKeys[event.which]) {
                                items.push(keys[i]);
                            }
                        }
                    }
                    if (items.length) {
                        scope.$apply(function () {
                            var result;
                            for(var i in items){
                                result = expression(scope, { itemId: items[i].id });
                                if (angular.isString(items[i].focus)) {
                                    items[i].focus = angular.element(items[i].focus);
                                }
                                execFocusElement(result, items[i].focus);
                            }
                        });
                    }
                }

                function init() {
                    var t;
                    if (attrs.bkKeyUp && attrs.bkKeyUpCodes) {
                        keys = attrs.bkKeyUpCodes.split(',');
                        for (var i in keys) {
                            t = keys[i].split(':');
                            if (t.length === 2) {
                                keys[i] = { id: +i, key: t[0], focus: t[1] };
                            }
                            else {
                                keys[i] = { id: +i, key: keys[i], focus: '' };
                            }
                        }
                        for (var i in keys) {
                            t = keys[i].key.split('+');
                            if (t.length === 2) {
                                if (!(t[0] in sysKeys)) {
                                    throw 'Invalid system key. Must be alt, ctrl or shift.'
                                }
                                else {
                                    t[0] = t[0] + 'Key';
                                    t[1] = parseInt(t[1], 10);
                                }
                                keys[i].masterKey = t[0];
                                keys[i].key = parseInt(t[1], 10);
                            }
                            else {
                                keys[i].key = parseInt(keys[i].key, 10);
                            }
                        }

                        expression = $parse(attrs.bkKeyUp);
                        element.on('keyup', keyUp);
                        scope.$on('$destroy', function () {
                            element.off('keyup', keyUp);
                        });
                    }
                }

                init();
            };
        }])
        .directive('bkStopPropagation', function () {
            return function ($scope, $element, $attrs) {
                var event = $attrs.bkStopPropagation || 'click';

                function stopPropagation(event) {
                    event.stopPropagation();
                }
                $element.on(event, stopPropagation);
                $scope.$on('$destroy', function () {
                    $element.off(event, stopPropagation);
                });
            }
        });


})(window.angular, document);