(function () {
    /*
        // this objecct defines plugin mechanics
        \*            
        http://www.west-wind.com/weblog/posts/2009/Apr/19/Native-JSON-Parsing-What-does-it-mean
        http://my.safaribooksonline.com/book/programming/regular-expressions/9780596802837/validation-and-formatting/validate_iso_8601_dates_and_times#X2ludGVybmFsX0ZsYXNoUmVhZGVyP3htbGlkPTk3ODA1OTY4MDI4MzcvMjM0
        http://delete.me.uk/2005/03/iso8601.html
        https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Date/parse
        *\
    var reISO8601 = /^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$/;
    \* /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/; *\
    var reMsAjax = /^\/Date\((d|-|.*)\)\/$/;
    var month_names = {
        "01": 'Jan', "02": 'Feb', "03": 'Mar', "04": 'Apr', "05": 'May', "06": 'Jun',
        "07": 'Jul', "08": 'Aug', "09": 'Sep', "10": 'Oct', "11": 'Nov', "12": 'Dec'
    };

    var parseISO8601_cache = [];
    function parseISO8601Date(string, cache) {
        if (!string) {
            return null;
        }

        var d = string.match(reISO8601);
        if (!cache) {
            cache = parseISO8601_cache;
        }

        cache.length = 0;

        cache.push(month_names[d[2]]);
        cache.push(' ');
        cache.push(d[3]);
        cache.push(', ');
        cache.push(d[1]);

        cache.push(' ');
        cache.push(d[4] ? d[4] : '00');
        cache.push(':');
        cache.push(d[5] ? d[5] : '00');
        cache.push(':');
        cache.push(d[6] ? d[6] : '00');

        cache.push(' GMT');
        cache.push(d[8] ? d[8].replace(':', '') : '+0200');

        var od = new Date(cache.join(''));
        if (d[7]) {
            od.setMilliseconds(d[7]);
        }
        return od;
    }    
    */
    angular.module('bk.dateParser', [])
    .service('bk.dateParser', ['$locale', 'orderByFilter', function ($locale, orderByFilter) {

        this.parsers = {};

        var formatCodeToRegex = {
            'yyyy': {
                regex: '\\d{4}',
                apply: function (value) { this.year = +value; }
            },
            'yy': {
                regex: '\\d{2}|\\d{4}',
                apply: function (value) {
                    if (+value < 100) {
                        this.year = +value + 2000;
                    }
                    else {
                        this.year = +value;
                    }
                }
            },
            'y': {
                regex: '\\d{1,4}',
                apply: function (value) { this.year = +value; }
            },
            'MMMM': {
                regex: $locale.DATETIME_FORMATS.MONTH.join('|'),
                apply: function (value) { this.month = $locale.DATETIME_FORMATS.MONTH.indexOf(value); }
            },
            'MMM': {
                regex: $locale.DATETIME_FORMATS.SHORTMONTH.join('|'),
                apply: function (value) { this.month = $locale.DATETIME_FORMATS.SHORTMONTH.indexOf(value); }
            },
            'MM': {
                regex: '0?[1-9]|1[0-2]',
                apply: function (value) { this.month = value - 1; }
            },
            'M': {
                regex: '0?[1-9]|1[0-2]',
                apply: function (value) { this.month = value - 1; }
            },
            'dd': {
                regex: '[0-2]?[1-9]{1}|3[0-1]{1}',
                apply: function (value) { this.date = +value; }
            },
            'd': {
                regex: '[0-2]?[1-9]{1}|3[0-1]{1}',
                apply: function (value) { this.date = +value; }
            },
            'EEEE': {
                regex: $locale.DATETIME_FORMATS.DAY.join('|')
            },
            'EEE': {
                regex: $locale.DATETIME_FORMATS.SHORTDAY.join('|')
            },
            'T': {
                regex: 'T'
            },
            'HH': {
                regex: '0[0-9]|1[0-9]|2[0-3]',
                apply: function (value) { this.hours = +value; }
            },
            'H': {
                regex: '[1-9]|1[0-9]|2[0-3]',
                apply: function (value) { this.hours = +value; }
            },
            'mm': {
                regex: '[0-5][0-9]',
                apply: function (value) { this.minutes = +value; }
            },
            'm': {
                regex: '[1-5]?[0-9]',
                apply: function (value) { this.minutes = +value; }
            },
            'ss': {
                regex: '[0-5][0-9]',
                apply: function (value) { this.seconds = +value; }
            },
            's': {
                regex: '[1-5]?[0-9]',
                apply: function (value) { this.seconds = +value; }
            },
            'sss': {
                regex: '[0-9][0-9][0-9]',
                apply: function (value) { this.seconds = +value; }
            },
            ':': {
                regex: ':'
            },
            '.': {
                regex: '.'
            },
            ',': {
                regex: ','
            }
        };

        this.createParser = function (format) {
            var map = [], regex = format.split('');

            angular.forEach(formatCodeToRegex, function (data, code) {
                var index = format.indexOf(code);

                if (index > -1) {
                    format = format.split('');

                    regex[index] = '(' + data.regex + ')';
                    format[index] = '$'; // Custom symbol to define consumed part of format
                    for (var i = index + 1, n = index + code.length; i < n; i++) {
                        regex[i] = '';
                        format[i] = '$';
                    }
                    format = format.join('');

                    map.push({ index: index, apply: data.apply });
                }
            });

            return {
                regex: new RegExp('^' + regex.join('') + '$'),
                map: orderByFilter(map, 'index')
            };
        };

        this.parse = function (input, format) {
            if (!angular.isString(input)) {
                return input;
            }

            format = $locale.DATETIME_FORMATS[format] || format;

            if (!this.parsers[format]) {
                this.parsers[format] = this.createParser(format);
            }

            var parser = this.parsers[format],
                regex = parser.regex,
                map = parser.map,
                results = input.match(regex);

            if (results && results.length) {
                var fields = { year: 1900, month: 0, date: 1, hours: 0, minutes: 0, seconds: 0 }, dt;

                for (var i = 1, n = results.length; i < n; i++) {
                    var mapper = map[i - 1];
                    if (mapper.apply) {
                        mapper.apply.call(fields, results[i]);
                    }
                }

                if (isValid(fields.year, fields.month, fields.date)) {
                    dt = new Date(fields.year, fields.month, fields.date, fields.hours, fields.minutes, fields.seconds);
                }

                return dt;
            }
        };

        // Check if date is valid for specific month (and year for February).
        // Month: 0 = Jan, 1 = Feb, etc
        function isValid(year, month, date) {
            if (!(0 < date && date < 31)) {
                return false;
            }
            if (!(0 <= month && month < 11)) {
                return false;
            }

            if (month === 1 && date > 28) {
                return date === 29 && ((year % 4 === 0 && year % 100 !== 0) || year % 400 === 0);
            }

            if (month === 3 || month === 5 || month === 8 || month === 10) {
                return date < 31;
            }

            return true;
        }
    }]);

})();