'use strict';

angular.module('xproxy.logs.events', ['ngRoute'])

.config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/events/:eventType', {
      templateUrl: 'event-view/events.html',
      controller: 'EventCtl'
    });
}])

.controller('EventCtl', ['$scope', '$routeParams', 'xproxy.logexplorer.service', function ($scope, $routeParams, logexplorer) {

    $scope.getComparisonPath = function (ev) {
        return "/compare/" + ev.data.host_0 + "/" + ev.data.host_1 + "/" + window.btoa(ev.data.path);
    };

    logexplorer.loadEvents("x", function (data) {
        $scope.eventData = data;
    });
}]);