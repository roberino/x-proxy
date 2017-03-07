'use strict';

angular.module('xproxy.logs.compare', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/compare/:originHost1/:originHost2/:sourcePath', {
      templateUrl: 'compare-view/comparison.html',
      controller: 'CompareCtl'
    });
}])

.controller('CompareCtl', ['$scope', '$routeParams', 'xproxy.logexplorer.service', function ($scope, $routeParams, logexplorer) {    
    
    var iterobj = function (obj) {
        var results = [];

        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                results.push({
                    key: key,
                    value: obj[key]
                });
            }
        }

        return results;
    };

    var loadFunc = function (host1, host2, path) {
        
        $scope.loading = true;
        $scope.compareStatus = "(Loading)";

        var flatten = $scope.flatten;

        logexplorer.loadHostComparison(host1, host2, path, function (data) {
            if ($scope.flatten) {
                $scope.comparison = { request: iterobj(data.properties) };
            }
            else {
                $scope.comparison = {
                    request: iterobj(data.children.request.properties),
                    response: iterobj(data.children.response.properties),
                    responseBody: data.children.response.children.body ? iterobj(data.children.response.children.body.properties) : []
                };
            }

            $scope.compareStatus = "";
            $scope.loading = false;
        }, $scope.flatten);
    };

    $scope.source1 = {
        originHost: $routeParams.originHost1,
        path: window.atob($routeParams.sourcePath)
    };

    $scope.source2 = {
        originHost: $routeParams.originHost2,
        path: $scope.source1.path
    };

    $scope.flatten = true;

    loadFunc($scope.source1.originHost, $scope.source2.originHost, $scope.source1.path);
}]);