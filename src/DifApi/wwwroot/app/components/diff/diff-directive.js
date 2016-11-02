'use strict';

angular.module('xproxy.diff', [])
    .directive('sourceDiff', [function () {

        function run(scope, element) {
            element[0].innerHTML = "";
            if (!(scope.text1 && scope.text2)) return;
            element[0].innerHTML = "<span>Loading...</span>";
            var dmp = new diff_match_patch();
            var diffResult = dmp.diff_main(scope.text1, scope.text2);
            dmp.diff_cleanupSemantic(diffResult);
            var html = dmp.diff_prettyHtml(diffResult);
            element[0].innerHTML = html;
        }

        return {
            restrict: 'E',
            template: "<div id='{id}'></div>",
            replace: true,
            scope: {
                id: '@',
                text1: '=',
                text2: '='
            },
            link: function (scope, element, attrs) {
                scope.$watch('text1', function (newValue, oldValue) {
                    if (newValue != oldValue) {
                        run(scope, element);
                    }
                });

                scope.$watch('text2', function (newValue, oldValue) {
                    if (newValue != oldValue) {
                        run(scope, element);
                    }
                });

                run(scope, element);
            }
        }
    }]);