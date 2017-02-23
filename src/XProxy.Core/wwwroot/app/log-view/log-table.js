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

    $scope.closeCompare = function () {

        $("#compare-modal").modal("hide");

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
                $scope.source1Info = logEntry;
                $scope.compareStatus = "(Waiting for second url)";
            }
            else {
                $scope.source2 = data;
                $scope.source2Info = logEntry;
                $scope.compareStatus = "";

                logexplorer.loadComparison($scope.source1Info, $scope.source2Info, function (data) {
                    $scope.comparison = {
                        request: iterobj(data.children.request.properties),
                        response: iterobj(data.children.response.properties),
                    };
                });

                $("#compare-modal").modal("show");
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

    $scope.loadHistogram = loadHistogram;

    loadHistogram();
    
    function loadHistogram(timerange) {
        $scope.chart = undefined;

        logexplorer.loadTimeHistogramByMime(function (data) {

            for (var i = 0; i < data.items.length; i++) {
                data.items[i].x = i + 1;

                for (var k in data.attributes) {
                    if (!data.items[i][k]) {
                        data.items[i][k] = 0;
                    }
                }
            }

            var chart = { data: { dataset1: data.items } };

            chart.options = {
                margin: { top: 8 },
                stacks: [
                    { axis: "y", series: [] }
                ],
                series: [],
                axes: { x: { key: "x" }, y: { min: 0 } }
            };

            var colours = ["#1f77b4", "#AE63FF", "#FFB05B", "#27B423", "#B21147", "#4FADAD", "#590DAA", "#E8E80D"];
            var i = 0;

            for (var k in data.attributes) {
                var id = "series" + i;
                chart.options.stacks[0].series.push(id);
                chart.options.series.push({
                    axis: "y",
                    interpolation: { mode: "bundle", tension: 0.7 },
                    dataset: "dataset1",
                    id: "series" + i,
                    key: k,
                    label: k,
                    color: colours[i % colours.length],
                    type: ["area", "line", "dot"]
                });
                i++;
            }

            //delete chart.options.stacks;

            $scope.chart = chart;
            $scope.chart.visible = true;

        }, timerange);
    }
}]);