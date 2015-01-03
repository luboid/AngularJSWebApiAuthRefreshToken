(function (window, document, angular, $) {
    'use strict';

    var push = Array.prototype.push;

    function trim(s, c) {
        s = '' + s; if (c === undefined) { c = ' '; }
        for (var i = 0, l = s.length; i < l; i += 1) {
            if (s.charAt(i) !== c) {
                s = s.substr(i);
                break;
            }
        }
        for (var i = s.length - 1; i > 0; i -= 1) {
            if (s.charAt(i) !== c) {
                s = s.substr(0, i + 1);
                break;
            }
        }
        return s;
    }

    function arrayRemove() {
        var array = arguments[0], items = Array.prototype.splice.call(arguments, 1);
        items.forEach(function (item) {
            var index = array.indexOf(item);
            if (index > -1) {
                array.splice(index, 1);
            }
        });
    }

    function arraySort(array, name, desc) {
        array.sort(function (a, b) {
            if (a[name] > b[name]) {
                return desc ? -1 : 1;
            }
            if (a[name] < b[name]) {
                return desc ? 1 : -1;
            }
            // a must be equal to b
            return 0;
        });
    }

    function arrayPush2Array(toArray, fromArray) {
        push.apply(toArray, fromArray);
        return toArray;
    }

    function array2Hash(array, property) {
        if (undefined === property) {
            property = 'id';
        }

        var hash = {}, value;
        angular.forEach(array, function (item, key) {
            value = item[property];
            if (value !== undefined) {
                hash[value] = item;
            }
        });
        return hash;
    }

    function array2Hash2(array, propertyValue, property) {
        if (undefined === propertyValue) {
            propertyValue = 'name';
        }
        if (undefined === property) {
            property = 'id';
        }

        var hash = {}, value;
        angular.forEach(array, function (item, key) {
            value = item[property];
            if (value !== undefined) {
                hash[value] = item[propertyValue];
            }
        });
        return hash;
    }

    function array2Hash3(array) {
        var hash = {}, value;
        angular.forEach(array, function (item, key) {
            hash[item] = true;
        });
        return hash;
    }

    function wrap2Functions(others, deep) {
        angular.forEach(others, function (item, key) {
            if (typeof (item) !== 'function') {
                others[key] = (function (item) { return (function () { return item; }); })(item);
                if (deep && angular.isObject(item)) {
                    wrap2Functions(item, deep);
                }
            }
        });
        return others;
    }

    function extractDataFromPromises(promises) {
        if (promises.data) {
            return promises.data;
        }
        else {
            var obj = angular.isObject(promises) ? {} : [];
            angular.forEach(promises, function (item, key) {
                if (item.data) {
                    obj[key] = item.data;
                }
            });
            return obj;
        }
    }

    function createSaveErrorMessage(messages, moduleMessages, status, data) {
        var m = messages.common.saveError;
        if (409 === status) {
            if (data.state && moduleMessages['saveState_' + data.state]) {
                m = moduleMessages['saveState_' + data.state];
            }
            else {
                m = moduleMessages.existsError;
            }
        }
        else {
            if (400 === status && data && data.message) {
                m = createHtmlMessage(data);
            }
        }
        return m;
    }

    function createRemoveErrorMessage(context, common, moduleMessages, data) {
        var m = common.saveError;
        if (404 === context.status) {
            m = moduleMessages.notExistsError;
        }
        else {
            if (409 === context.status) {
                if (context.data.state && moduleMessages['deleteState_' + context.data.state]) {
                    m = moduleMessages['deleteState_' + context.data.state];
                }
                else {
                    m = moduleMessages.cantDeleteError;
                }
            }
        }
        return data ? format(m, data) : m;
    }

    function htmlNormalize(message) {
        return message.replace(/</gi, '&lt;').replace(/>/gi, '&gt;');
    }

    function keyName(key) {
        var i = key.indexOf('.');
        if (-1 !== i) {
            key = key.substr(i + 1);
        }
        return (key.charAt(0).toLowerCase() + key.substr(1));
    }

    function createMessageFromHttpContext(context, options) {
        if (context.status !== 403) {//global handler is setup
            if (existsMessage(context.data)) {
                return createHtmlMessage(context.data, options);
            }
            else {
                return context.statusText;
            }
        }
    }

    function existsMessage(data) {
        return data && (data.modelState || (data.message && data.message !== 'The request is invalid.'));
    }

    function createHtmlMessage(data, options) {
        var label, key, i, txts, inputCtrl, inputMessages, invalidated = 0,
            message = [],
            message2 = [],
            form = options && options.form,
            formMessages = options && options.messages,
            labels = (options && options.labels) || {};

        message.push('<div>');

        if (data.message && data.message !== 'The request is invalid.') {
            message.push('<span>');
            message.push(htmlNormalize(data.message));
            message.push('</span>');
        }

        if (data.modelState) {
            txts = data.modelState[''];
            if (txts && txts.length) {
                message.push('<span>');
                message.push(htmlNormalize(txts.join('&nbsp;')));
                message.push('</span>');
            }

            message2.push('<ul>');
            for (i in data.modelState) {
                if (i === '') continue;

                txts = data.modelState[i].join('&nbsp;');
                key = keyName(i);
                if (form && (inputCtrl = form[key])) {
                    ++invalidated;
                    inputCtrl.$setValidity('serverModel', false);
                    inputMessages = formMessages[key];
                    if (!inputMessages) {
                        inputMessages = formMessages[key] = {};
                    }
                    inputMessages['serverModel'] = txts;
                    continue;
                }

                label = labels[key] || key;

                message2.push('<li>');
                message2.push('<strong>');
                message2.push(htmlNormalize(label));
                message2.push('</strong>&nbsp;');
                message2.push(htmlNormalize(txts));
                message2.push('</li>');
            }
            message2.push('</ul>');

            if (message2.length != 2) {
                arrayPush2Array(message, message2);
            }
        }

        message.push('</div>');

        if (invalidated && form.bkFocus) {
            form.bkFocus();
        }

        return 2 === message.length ? '' : message.join('');
    }

    function createHtmlMessage2(data) {
        var i, message = [];
        message.push('<div>');

        if (data.message) {
            message.push('<span>');
            message.push(htmlNormalize(data.message));
            message.push('</span>');
        }

        message.push('<ul>');
        for (i in data.messages) {
            message.push('<li>');
            message.push(htmlNormalize(data.messages[i]));
            message.push('</li>');
        }
        message.push('</ul>');

        message.push('</div>');

        return message.join('');
    }

    function format(string) {
        var args = Array.prototype.slice.call(arguments), args1 = args[1], data, locator;
        return string.replace(/\{([^\{\}]*)\}/g, function (match, group_match) {
            if (isNaN(group_match)) {
                locator = group_match.split('.'), data = args1;
                for (var i in locator) {
                    data = data[locator[i]];
                }
                return typeof data === 'undefined' || null === data ?
                    match : data.toString();
            }
            else {
                data = args[parseInt(group_match, 10) + 1];
                return typeof data === 'undefined' || null === data ?
                    match : data.toString();
            }
        });
    }

    function isEmptyString(value) {
        if (undefined === value || null === value) {
            return true;
        }
        if (angular.isString(value) && ('' === value || 0 === value.replace(/\s/gi, '').length)) {
            return true;
        }
        return false;
    }

    function isEmpty(value) {
        return undefined === value || null === value || '' === value;
    }

    function isEmptyOrZero(value) {
        return undefined === value || null === value || '' === value || 0 === value;
    }

    function checkId(id) {
        return (id || '00000000-0000-0000-0000-000000000000');
    }

    angular.module('bk.utils', [])
        .factory('bk.utils', ['$locale', '$log', '$modal', 'ea.resources', '$http', '$q', 'dateFilter', function ($locale, $log, $modal, messages, $http, $q, dateFilter) {
            var decimalSeparator = $locale.NUMBER_FORMATS.DECIMAL_SEP,
                decimalSeparatorRegExp = new RegExp('.' === $locale.NUMBER_FORMATS.DECIMAL_SEP ? '\\' + $locale.NUMBER_FORMATS.DECIMAL_SEP : $locale.NUMBER_FORMATS.DECIMAL_SEP, 'gi'),
                groupSeparatorRegExp = new RegExp($locale.NUMBER_FORMATS.GROUP_SEP, 'gi'),
                currencySymRegExp = new RegExp($locale.NUMBER_FORMATS.CURRENCY_SYM, 'gi'),
                //numberRegExp = new RegExp('^[-+]?[0-9]*[\\.|,]?[0-9]*?$'),
                numberRegExp = new RegExp('^[-+]?[0-9]*[\\.]?[0-9]*?$');

            return {
                format: format,
                isEmpty: isEmpty,
                isEmptyString: isEmptyString,
                isEmptyOrZero: isEmptyOrZero,
                sign: function (value) {
                    if (value < 0) {
                        return -1;
                    }
                    else {
                        if (value > 0) {
                            return 1;
                        }
                        else {
                            return 0;
                        }
                    }
                },
                normalizeNumber: function (value, dec) {
                    value = '' + value;
                    if (undefined === dec || dec) {
                        value = value.replace(decimalSeparatorRegExp, '.');
                    }
                    value = value.replace(groupSeparatorRegExp, '').replace(currencySymRegExp, '');
                    if (!numberRegExp.test(value)) {
                        value = undefined;
                    }
                    return value;
                },
                parseNumber: function (value) {
                    if (angular.isNumber(value)) {
                        return value;
                    }
                    else {
                        value = this.normalizeNumber(value);
                        if (undefined !== value) {
                            value = parseFloat(value);
                        }
                        return undefined === value || isNaN(value) ? undefined : value;
                    }
                },
                htmlNormalize: htmlNormalize,
                createSaveErrorMessage: function (moduleMessages, status, data) {
                    return createSaveErrorMessage(messages, moduleMessages, status, data);
                },
                createRemoveErrorMessage: function (context, moduleMessages, data) {
                    return createRemoveErrorMessage(context, messages.common, moduleMessages, data);
                },
                createHtmlMessage: createHtmlMessage,
                createHtmlMessage2: createHtmlMessage2,
                createMessageFromHttpContext:createMessageFromHttpContext,
                trim: trim,
                currentDateAsISO: function () {
                    return dateFilter(new Date(), 'yyyy-MM-dd');
                },
                wrap2Functions: wrap2Functions,
                arrayPush2Array: arrayPush2Array,
                arrayRemove: arrayRemove,
                arraySort: arraySort,
                array2Hash: array2Hash,
                array2Hash2: array2Hash2,
                array2Hash3: array2Hash3,
                extractDataFromPromises: extractDataFromPromises,
                decodeFromUrl: function (url) {
                    var i, parts, name, value, pairs = ('' + url).split('&');
                    url = {};
                    for (i in pairs) {
                        parts = pairs[i].split('=');
                        if (parts[0] || parts[1]) {
                            name = decodeURIComponent(parts[0]);
                            value = decodeURIComponent(parts[1]);

                            url[name] = value;
                        }
                    }
                    return url;
                },
                encodeToUrl: function (object) {
                    // If this is not an object, defer to native stringification.
                    if (!angular.isObject(object)) {
                        return ((object === null || object === undefined) ? "" : object.toString());
                    }

                    var name, value, buffer = [];

                    // Serialize each key in the object.
                    for (name in object) {
                        if (!object.hasOwnProperty(name)) {
                            continue;
                        }

                        value = object[name];

                        buffer.push(
                        encodeURIComponent(name) +
                        "=" +
                        ((value === null || value === undefined) ? "" : encodeURIComponent(value))
                        );
                    }

                    // Serialize the buffer and clean it up for transportation.
                    //var source = buffer.join("&")/*.replace(/%20/g, "+")*/;

                    return (buffer.join("&"));
                },
                emptyData: function () {
                    var data = [], defer = $q.defer();
                    if (arguments.length) {
                        if (arguments[0] instanceof Array) {
                            data = arguments[0];
                        }
                        else {
                            arrayPush2Array(data, arguments);
                        }
                    }
                    defer.resolve(data);
                    return defer.promise;
                },
                initDatepicker: function (datepickerPopupConfig) {
                    angular.extend(datepickerPopupConfig, messages.datepickerPopupConfig);//localization
                    datepickerPopupConfig.datepickerPopup = 'mediumDate';
                    //datepickerPopupConfig.appendToBody = true; not working into dialog
                },
                browserContext: {
                    set: function (scope, data) {
                        scope.rows = data.collection;
                        scope.rowCount = data.count;
                        scope.browserContext.sort = data.sort;
                        scope.browserContext.sortDir = 0 == data.sortDir ? 'asc' : 'desc';
                    },
                    clear: function (scope) {
                        scope.browserContext.page = 1;
                        scope.browserContext.lastPage = 1;
                        scope.rowCount = 0;
                    },
                    init: function (scope, extend) {
                        scope.rows = [];
                        scope.rowCount = 0;
                        scope.browserContext = angular.extend({
                            searchValue: undefined,
                            searchColumn: undefined,
                            page: 1,
                            lastPage: 1,
                            pageSize: 10,
                            sort: undefined,
                            sortDir: undefined
                        }, extend);
                    }
                },
                errorContext: {
                    set: function (scope, message) {
                        if (!scope.error) {
                            this.init(scope);
                        }
                        scope.error.hidden = !!!message;
                        scope.error.message = message;
                    },
                    setMessageFromHttpContext: function (scope, context, options) {
                        this.set(scope, 
                            createMessageFromHttpContext(context, options));
                    },
                    setSaveErrorMessage: function (scope, moduleMessages, context) {
                        this.set(scope,
                            createSaveErrorMessage(messages, moduleMessages, context));
                    },
                    setRemoveErrorMessage: function (scope, context, moduleMessages, data) {
                        this.set(scope,
                            createRemoveErrorMessage(context, moduleMessages, data));
                    },
                    clear: function (scope) {
                        this.init(scope);
                    },
                    init: function (scope) {
                        if (scope.error) {
                            scope.error.hidden = true;
                            scope.error.message = null;
                        }
                        else {
                            scope.error = { hidden: true, message: null };
                        }
                    }
                },
                infoContext: {
                    set: function (scope, message) {
                        if (!scope.info) {
                            this.init(scope);
                        }
                        scope.info.hidden = !!!message;
                        scope.info.message = message;
                    },
                    clear: function (scope) {
                        this.init(scope);
                    },
                    init: function (scope) {
                        if (scope.info) {
                            scope.info.hidden = true;
                            scope.info.message = null;
                        }
                        else {
                            scope.info = { hidden: true, message: null };
                        }
                    }
                }
        };
        }]);
})(window, document, window.angular, window.jQuery);