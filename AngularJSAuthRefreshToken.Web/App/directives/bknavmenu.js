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
    <ul class="dropdown-menu" role="menu" bk-nav-menu="menuItem.items" bk-nav-menu-item="menuItem"></ul>\
</li>';

            function elementAuthorized(authorized,mElement) {
                if (authorized) {
                    mElement.removeClass('hidden')
                }
                else {
                    mElement.addClass('hidden')
                }
            }

            function isAuthorized(votes) {
                return votes.some(function (item) {
                    return item.vote;//authorized
                });
            }

            return {
                restrict: 'A',
                scope: {
                    bkNavMenu: '=',
                    bkNavMenuItem: '=?'
                },
                link: function ($scope, $element, $attr, ctrls, $transclude) {
                    var mElement, master = !!!$scope.bkNavMenuItem;

                    angular.forEach($scope.bkNavMenu, function (m, i) {
                        var mElement, scope,
                            type = m.description === 'divider' ? 0 : (!!(m.items && m.items.length) ? 2 : 1),
                            authorizationFunc;

                        if (0 === type) {
                            mElement = templateMenuDivider;
                        }
                        else {
                            scope = $scope.$new();

                            mElement = angular.element(type === 2 ? templateMenuDropdown : templateMenuItem);

                            scope.menuItem = m;
                            scope.description = angular.isFunction(m.description) ?
                                m.description : function () {
                                    return m.description;
                                };

                            if (1 === type) {
                                scope.$on(master ? 'ea:master:menuitem:authorize' : 'ea:menuitem:authorize', function (event, votes) {
                                    var vote = authorization.state(m.state) && (m.visible === undefined || m.visible());
                                    elementAuthorized(vote, mElement);
                                    votes.push({ menuItem: m, vote: vote });
                                });
                            }
                            else {
                                if (2 === type) {
                                    scope.$on('ea:master:menuitem:authorize', function (event, votes) {
                                        var childrenVotes = [], vote;
                                        scope.$broadcast('ea:menuitem:authorize', childrenVotes);
                                        vote = isAuthorized(childrenVotes) && (m.visible === undefined || m.visible());

                                        elementAuthorized(vote, mElement);

                                        votes.push({ menuItem: m, vote: vote });
                                    });
                                }
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

                    if (master) {
                        $scope.$on('ea:authentication:token', function (event) {
                            var votes = [];
                            $scope.$broadcast('ea:master:menuitem:authorize', votes);
                            elementAuthorized(isAuthorized(votes), $element);
                        });
                        $scope.$broadcast('ea:authentication:token');//init menu
                    }
                }
            };
        }]);

})(window.angular, document);