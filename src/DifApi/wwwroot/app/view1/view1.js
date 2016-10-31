'use strict';

angular.module('xproxy.view1', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
  $routeProvider.when('/view1', {
    templateUrl: 'view1/view1.html',
    controller: 'View1Ctrl'
  });
}])

.controller('View1Ctrl', ['$scope', 'xproxy.logexplorer.factory', function ($scope, logexplorer) {
    $scope.tree = [];

    logexplorer.loadTree(function (data) {
        $scope.tree = [translateNode(data)];
    });

    $scope.treeSelect = function (branch) {
        $scope.currentNode = branch;
    };
    
    function translateNode(node) {

        // if (node.isLeaf) return node.path;

        var treeNode = {
            data: {
                averageSizeKb: node.averageSizeKb,
                requestCount: node.requestCount,
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
                treeNode.children.push(translateNode(child));
            }
        }

        return treeNode;
    }
}]);