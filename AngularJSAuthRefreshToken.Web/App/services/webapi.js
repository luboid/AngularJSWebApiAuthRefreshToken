/**
 * @license AngularJS v1.3.7
 * (c) 2010-2014 Google, Inc. http://angularjs.org
 * License: MIT
 */
(function (window, angular, undefined) {
    'use strict';

    var $resourceMinErr = angular.$$minErr('webApi');

    // Helper functions and regex to lookup a dotted path on an object
    // stopping at undefined/null.  The path must be composed of ASCII
    // identifiers (just like $parse)
    var MEMBER_NAME_REGEX = /^(\.[a-zA-Z_$][0-9a-zA-Z_$]*)+$/;

    function isValidDottedPath(path) {
        return (path != null && path !== '' && path !== 'hasOwnProperty' &&
            MEMBER_NAME_REGEX.test('.' + path));
    }

    function lookupDottedPath(obj, path) {
        if (!isValidDottedPath(path)) {
            throw $resourceMinErr('badmember', 'Dotted member path "@{0}" is invalid.', path);
        }
        var keys = path.split('.');
        for (var i = 0, ii = keys.length; i < ii && obj !== undefined; i++) {
            var key = keys[i];
            obj = (obj !== null) ? obj[key] : undefined;
        }
        return obj;
    }

    angular.module('webApi', ['ng']).
      provider('webApi', function () {
          var provider = this,
              noCopy = { 
                  params: true, 
                  interceptor: true, 
                  url: true 
              },
              defaultActions = {
                  'get': true,
                  'save': true,
                  'update': true,
                  'query': true,
                  'remove': true,
                  'delete': true
              },
              defaultParamActions = {
                  'get': true,
                  'update': true,
                  'remove': true,
                  'delete': true
              },
              noop = angular.noop,
              forEach = angular.forEach,
              copy = angular.copy,
              isFunction = angular.isFunction;;

          this.defaults = {
              // Strip slashes by default
              stripTrailingSlashes: true,
              defaultParam: ':id',
              // Default actions configuration
              actions: {
                  'get': { method: 'GET' },
                  'save': { method: 'POST' },
                  'update': { method: 'PUT' },
                  'query': { method: 'GET' },
                  'remove': { method: 'DELETE' },
                  'delete': { method: 'DELETE' }
              },
              params: {}
          };

          function extend(dst) {
              var key, val, obj;
              for (var i = 1, ii = arguments.length; i < ii; i++) {
                  obj = arguments[i];
                  if (obj) {
                      var keys = Object.keys(obj);
                      for (var j = 0, jj = keys.length; j < jj; j++) {
                          key = keys[j];
                          val = obj[key];

                          if (angular.isArray(val)) {//!!!
                              val = copy(val);
                          }
                          else {
                              if (angular.isObject(val)) {
                                  val = extend(dst[key] || {}, val);
                              }
                          }

                          dst[key] = val;
                      }
                  }
              }

              return dst;
          }

          this.$get = ['$http', '$q', '$log', function ($http, $q, $log) {

              /**
               * We need our custom method because encodeURIComponent is too aggressive and doesn't follow
               * http://www.ietf.org/rfc/rfc3986.txt with regards to the character set
               * (pchar) allowed in path segments:
               *    segment       = *pchar
               *    pchar         = unreserved / pct-encoded / sub-delims / ":" / "@"
               *    pct-encoded   = "%" HEXDIG HEXDIG
               *    unreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~"
               *    sub-delims    = "!" / "$" / "&" / "'" / "(" / ")"
               *                     / "*" / "+" / "," / ";" / "="
               */
              function encodeUriSegment(val) {
                  return encodeUriQuery(val, true).
                    replace(/%26/gi, '&').
                    replace(/%3D/gi, '=').
                    replace(/%2B/gi, '+');
              }


              /**
               * This method is intended for encoding *key* or *value* parts of query component. We need a
               * custom method because encodeURIComponent is too aggressive and encodes stuff that doesn't
               * have to be encoded per http://tools.ietf.org/html/rfc3986:
               *    query       = *( pchar / "/" / "?" )
               *    pchar         = unreserved / pct-encoded / sub-delims / ":" / "@"
               *    unreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~"
               *    pct-encoded   = "%" HEXDIG HEXDIG
               *    sub-delims    = "!" / "$" / "&" / "'" / "(" / ")"
               *                     / "*" / "+" / "," / ";" / "="
               */
              function encodeUriQuery(val, pctEncodeSpaces) {
                  return encodeURIComponent(val).
                    replace(/%40/gi, '@').
                    replace(/%3A/gi, ':').
                    replace(/%24/g, '$').
                    replace(/%2C/gi, ',').
                    replace(/%20/g, (pctEncodeSpaces ? '%20' : '+'));
              }

              function Route(template, options) {
                  this.template = template;
                  this.options = extend({}, provider.defaults, options);
              }

              Route.prototype = {
                  url: function (actionUrl) {
                      var url = this.template;
                      if (actionUrl) {
                          var protocol = actionUrl.substr(0,8);
                          if (actionUrl.charAt(0) === '/' || protocol === 'http://' || protocol === 'https://') {
                              url = actionUrl;
                          }
                          else {
                              if (url.charAt(url.length - 1) === '/') {
                                  url += actionUrl;
                              }
                              else {
                                  url += '/' + actionUrl;
                              }
                          }
                      }
                      return url;
                  },
                  extractUrlParams: function (url) {
                      var urlParams = {};
                      forEach(url.split(/\W/), function (param) {
                          if (param === 'hasOwnProperty') {
                              throw $resourceMinErr('badname', "hasOwnProperty is not a valid parameter name.");
                          }
                          if (!(new RegExp("^\\d+$").test(param)) && param &&
                            (new RegExp("(^|[^\\\\]):" + param + "(\\W|$)").test(url))) {
                              urlParams[param] = true;
                          }
                      });
                      return urlParams;
                  },
                  setUrlParams: function (config, params, urlParams, url) {
                      var val, encodedVal, defaultParams = this.options.params || {};

                      url = url.replace(/\\:/g, ':');

                      params = params || {};
                      forEach(urlParams, function (_, urlParam) {
                          val = params.hasOwnProperty(urlParam) ? params[urlParam] : defaultParams[urlParam];
                          if (angular.isDefined(val) && val !== null) {
                              encodedVal = encodeUriSegment(val);
                              url = url.replace(new RegExp(":" + urlParam + "(\\W|$)", "g"), function (match, p1) {
                                  return encodedVal + p1;
                              });
                          } else {
                              url = url.replace(new RegExp("(\/?):" + urlParam + "(\\W|$)", "g"), function (match,
                                  leadingSlashes, tail) {
                                  if (tail.charAt(0) == '/') {
                                      return tail;
                                  } else {
                                      return leadingSlashes + tail;
                                  }
                              });
                          }
                      });

                      // strip trailing slashes and set the url (unless this behavior is specifically disabled)
                      if (this.options.stripTrailingSlashes) {
                          url = url.replace(/\/+$/, '') || '/';
                      }

                      // then replace collapse `/.` if found in the last URL path segment before the query
                      // E.g. `http://url.com/id./format?q=x` becomes `http://url.com/id.format?q=x`
                      url = url.replace(/\/\.(?=\w+($|\?))/, '.');

                      // replace escaped `/\.` with `/.`
                      config.url = url.replace(/\/\\\./, '/.');


                      // set params - delegate param encoding to $http
                      forEach(params, function (value, key) {
                          if (!urlParams[key]) {
                              config.params = config.params || {};
                              config.params[key] = value;
                          }
                      });
                  }
              };

              function extractParams(data, paramDefaults, actionParams) {
                  var ids = {};
                  actionParams = extend({}, paramDefaults, actionParams);
                  forEach(actionParams, function (value, key) {
                      if (isFunction(value)) { value = value(); }
                      ids[key] = value && value.charAt && value.charAt(0) == '@' ?
                        lookupDottedPath(data, value.substr(1)) : value;
                  });
                  return ids;
              }

              return (function resourceFactory(url, paramDefaults, actions, options) {
                  var api = {},
                      route = new Route(url, options),
                      actions = extend({}, route.options.actions, actions);

                  function createMethods(api, route, action, name) {
                      if (angular.isArray(action)) {
                          api[name] = resourceFactory(
                              route.url(action[0] || name),
                              extend({}, paramDefaults, action[1]),
                              action[2] || {}, //actions
                              extend({}, options, { defaultParam: null }, action[3]));
                      }
                      else {
                          if (angular.isString(action)) {
                              action = { method: action };
                          }
                          else {
                              if (typeof (action) === 'boolean') {
                                  if (action) {
                                      action = { method: 'POST' };
                                  }
                                  else {
                                      return;
                                  }
                              }
                          }

                          if (action.method === undefined) {
                              action.method = 'POST';
                          }

                          var hasBody = /^(POST|PUT|PATCH)$/i.test(action.method || 'POST'),
                              responseInterceptor = action.interceptor && action.interceptor.response || undefined,
                              responseErrorInterceptor = action.interceptor && action.interceptor.responseError || undefined,
                              httpConfig = {}, url = action.url, urlParams;

                          if (!url) {
                              if (defaultActions[name]) {
                                  if (defaultParamActions[name]) {
                                      url = route.options.defaultParam;
                                  }
                              }
                              else {
                                  url = name.toLowerCase();
                              }
                          }

                          url = route.url(url);

                          urlParams = route.extractUrlParams(url);

                          forEach(action, function (value, key) {
                              if (!noCopy[key]) {
                                  httpConfig[key] = copy(value);
                              }
                          });

                          api[name] = function (a1, a2) {
                              var params = {},
                                  data,
                                  callHttpConfig = {},
                                  extract;

                              /* jshint -W086 */ /* (purposefully fall through case statements) */
                              switch (arguments.length) {
                                  case 2:
                                      params = a1;
                                      data = a2;
                                      break;
                                  case 1:
                                      if (hasBody) data = a1;
                                      else params = a1;
                                      break;
                                  case 0: break;
                                  default:
                                      throw $resourceMinErr('badargs',
                                        "Expected up to 2 arguments [params, data], got {0} arguments",
                                        arguments.length);
                              }
                              /* jshint +W086 */ /* (purposefully fall through case statements) */

                              callHttpConfig = copy(httpConfig);

                              if (hasBody) {
                                  callHttpConfig.data = data;
                              }

                              extract = extractParams(data, paramDefaults, action.params || {});
                              params = extend({}, extract, params);

                              route.setUrlParams(callHttpConfig, params, urlParams, url);

                              return $http(callHttpConfig)
                                .then(function (response) {
                                    if (responseInterceptor) {
                                        return responseInterceptor(response);
                                    }
                                    else {
                                        return response.data;
                                    }
                                },
                                function (response) {
                                    if (responseErrorInterceptor) {
                                        return responseErrorInterceptor(response);
                                    }
                                    else {
                                        return $q.reject(response);
                                    }
                                });
                          };
                      }
                  }

                  forEach(actions, angular.bind(null, createMethods, api, route));

                  return api;
              });
          }];
      });


})(window, window.angular);