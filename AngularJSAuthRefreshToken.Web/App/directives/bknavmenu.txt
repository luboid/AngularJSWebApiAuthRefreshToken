/* Directives */
(function (angular, document) {
    'use strict';

    angular.module('ea.directives')
        .directive('bkNavMenu', ['$compile', '$timeout', 'ea.authorization', function ($compile, $timeout, authorization) {
            var templateMenuDivider = '<li class="divider"></li>',
                templateMenuItem = '<li><a ui-sref="{{menuItem.state}}" ng-bind-html="description()"></a></li>',
                templateMenuDropdown = '<li class="dropdown" dropdown>\
    <a href="#" class="dropdown-toggle" dropdown-toggle bk-stop-propagation role="button" aria-expanded="false">\
        <span ng-bind-html="description()"></span>&nbsp;<span class="caret">\
    </span></a>\
    <ul class="dropdown-menu" role="menu" bk-nav-menu="menuItem.items" bk-nav-menu-sub="true"></ul>\
</li>';

            function elementAuthorized(authorized,mElement) {
                if (authorized) {
                    mElement.removeClass('hidden')
                }
                else {
                    mElement.addClass('hidden')
                }
            }

            return {
                restrict: 'A',
                scope: {
                    bkNavMenu: '=',
                    bkNavMenuSub: '=?'
                },
                link: function ($scope, $element, $attr, ctrls, $transclude) {
                    var mElement, master = $scope.$new();

                    if (!$scope.bkNavMenuSub) {
                        master.$on('ea:menuitem:authorize', function (event) {
                            event.stopPropagation();

                            event.currentScope.timeout && $timeout.cancel(event.currentScope.timeout);
                            event.currentScope.timeout = $timeout(angular.bind(event.currentScope, function () {
                                this.timeout = null;
                                this.authorized = (this.bkNavMenu.filter(function (i) {
                                    return i.authorized;
                                }).length != 0);

                                elementAuthorized(this.authorized, $element);
                            }), 100);
                        });
                    }

                    angular.forEach($scope.bkNavMenu, function (m, i) {
                        var mElement, scope,
                            type = m.description === 'divider' ? 0 : (!!(m.items && m.items.length) ? 2 : 1);

                        if (0 === type) {
                            mElement = templateMenuDivider;
                        }
                        else {
                            scope = master.$new();

                            mElement = angular.element(type === 2 ? templateMenuDropdown : templateMenuItem);

                            scope.menuItem = m;
                            scope.description = angular.isFunction(m.description) ?
                                m.description : function () {
                                    return m.description;
                                };

                            if (type === 2) {//dropdown
                                scope.authorized = function () {
                                    return m.authorized = (m.items.filter(function (i) {
                                        return i.authorized;
                                    }).length != 0) && (m.visible === undefined || m.visible());
                                };

                                scope.$on('ea:menuitem:authorize', function (event) {
                                    event.stopPropagation();

                                    event.currentScope.timeout && $timeout.cancel(event.currentScope.timeout);
                                    event.currentScope.timeout = $timeout(angular.bind(event.currentScope, function () {
                                        this.timeout = null;
                                        this.$parent.$emit('ea:menuitem:authorize');
                                        elementAuthorized(this.authorized(), mElement);
                                    }), 100);
                                });
                            }
                            else {
                                scope.authorized = function () {
                                    m.authorized = authorization.state(m.state) && (m.visible === undefined || m.visible());
                                    this.$emit('ea:menuitem:authorize');
                                    return m.authorized;
                                };

                                scope.$on('ea:authentication:token', function (event) {
                                    elementAuthorized(event.currentScope.authorized(), mElement);
                                });
                            }

                            scope.$on('$destroy', function () {
                                scope.description = m = null;
                                scope.authorized = angular.noop;
                                mElement.remove();
                                mElement = null;
                            });

                            $compile(mElement)(scope);
                        }

                        $element.append(mElement);
                    });

                    if (!$scope.bkNavMenuSub) {
                        master.$broadcast('ea:authentication:token');
                        // user name filp/flop when signout if I don't that need to master.watch authentication.isAuthenticated()
                        // and then to evaluate menu authurization
                    }
                }
            };
        }]);

})(window.angular, document);