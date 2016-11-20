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

        if (sn % 2 === 0) {
            $scope.source1 = { url: logEntry.originUrl };
            $scope.source2 = null;
        }
        else {
            $scope.source2 = { url: logEntry.originUrl };
        }

        logexplorer.loadSource(logEntry.originHost, logEntry.originPath, logEntry.id, function (data) {
            if (sn % 2 === 0) {
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
        if (pathi === 0) {
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

    loadHistogram2();
    
    function loadHistogram2() {
        logexplorer.loadTimeHistogramByMime(function (data) {

            for (var i = 0; i < data.items.length; i++) {
                data.items[i].x = i;
            }

            $scope.data = {
                dataset0: data.items
            };

            $scope.options = {
                series: [],
                axes: { x: { key: "x" } }
            };

            var colours = ["#1f77b4", "#AE63FF", "#FFB05B", "#27B423", "#B21147", "#4FADAD", "#590DAA", "#E8E80D"];
            var i = 0;

            for (var k in data.attributes) {
                $scope.options.series.push({
                    axis: "y",
                    dataset: "dataset0",
                    key: k,
                    label: k,
                    color: colours[i++ % colours.length],
                    type: ['line', 'dot'],
                    id: 'series' + i
                });
            }
        });
    }

    function loadHistogram1() {
        logexplorer.loadTimeHistogram(function (data) {

            var ds = [];

            for (var i = 0; i < data.length; i++) {
                ds.push({
                    x: i,
                    y: data[i].length
                });
            }

            $scope.data = {
                dataset0: ds
            };

            $scope.options = {
                series: [
                  {
                      axis: "y",
                      dataset: "dataset0",
                      key: "y",
                      label: "Request histogram",
                      color: "#1f77b4",
                      type: ['column'],
                      id: 'mySeries0'
                  }
                ],
                axes: { x: { key: "x" } }
            };
        });
    }
}]);