'use strict';

angular.module('xproxy.search', [])

.controller('Search', ['$scope', '$location', 'xproxy.logexplorer.service', function ($scope, $location, logexplorer) {

    $scope.query = {
        text: ""
    };

    logexplorer.onSearch(function (data) {
        $scope.query.text = data.queryText;
    });

    $scope.searchGo = function () {
        $location.path('logs/' + $scope.query.text);
    };
}]);