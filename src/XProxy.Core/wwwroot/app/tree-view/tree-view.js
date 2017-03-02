'use strict';

angular.module('xproxy.logs.tree', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/tree', {
    templateUrl: 'tree-view/tree-view.html',
    controller: 'TreeViewCtl'
  });
}])

.controller('TreeViewCtl', ['$scope', 'xproxy.logexplorer.service', function ($scope, logexplorer) {
    $scope.tree = [];

    logexplorer.loadTree(function (data) {
        $scope.tree = [translateNode(data)];
    });

    $scope.treeSelect = function (branch) {
        $scope.currentNode = branch;
    };

    $scope.getPathRouteQuery = function (branch, prefix) {
        if (branch && branch.data)
            return (prefix || '') + window.btoa(branch.data.fullPath);
    };
    
    function translateNode(node, depth) {

        depth = depth || 0;

        var treeNode = {
            data: {
                fullPath: node.fullPath,
                averageSizeKb: node.averageSizeKb,
                requestCount: node.requestCount,
                totalRequestCount: node.totalRequestCount,
                probabilityOfFault: node.probabilityOfFault,
                maxElapsed: node.maxElapsed,
                minElapsed: node.minElapsed,
                statuses: node.statuses,
                verbs: node.verbs,
                hosts: node.hosts
            },
            label: node.path,
            children: [],
            classes: []
        };

        if (node.statuses.indexOf(404) > -1) {
            treeNode.classes.push("warning");
        }

        if (node.statuses.indexOf(500) > -1) {
            treeNode.classes.push("danger");
        }

        for (var key in node.children) {
            if (node.children.hasOwnProperty(key)) {
                var child = node.children[key];
                var tchild = translateNode(child, depth + 1);
                treeNode.children.push(tchild);
            }
        }

        return treeNode;
    }
}]);