'use strict';

angular.module('xproxy.logs.table', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/logs/:query?', {
      templateUrl: 'log-view/log-table.html',
      controller: 'LogTableCtl'
  });
}])

.controller('LogTableCtl', ['$scope', '$routeParams', 'xproxy.logexplorer.service', function ($scope, $routeParams, logexplorer) {
    $scope.logs = [];

    var query = $routeParams.query;
    var sourceNum = 0;

    $scope.closeCompare = function () {
        sourceNum = 0;
        $scope.source1 = null;
        $scope.source2 = null;
    };

    $scope.loadSource = function (logEntry) {
        var sn = sourceNum;
        sourceNum++;

        $scope.compareStatus = "(Loading)";

        if (sn % 2 == 0) {
            $scope.source1 = { url: logEntry.originUrl };
            $scope.source2 = null;
        }
        else {
            $scope.source2 = { url: logEntry.originUrl };
        }

        logexplorer.loadSource(logEntry.originHost, logEntry.originPath, logEntry.id, function (data) {
            if (sn % 2 == 0) {
                $scope.source1 = data;
                $scope.compareStatus = "(Waiting for second url)";
            }
            else {
                $scope.source2 = data;
                $scope.compareStatus = "";
            }
        });
    };

    if (query) {
        var pathi = query.indexOf("path-");
        if (pathi == 0) {
            var path = query.substr(5);
            logexplorer.filterByPath(window.atob(path), function (data) {
                $scope.logs = data;
            });
        }
        else {
            logexplorer.search(query, function (data) {
                $scope.logs = data;
            });
        }
    }
    else {
        logexplorer.loadLogs(function (data) {
            $scope.logs = data;
        });
    }
}]);