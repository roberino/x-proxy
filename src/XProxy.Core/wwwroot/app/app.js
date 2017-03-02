'use strict';

// Declare app level module which depends on views, and components
angular.module('xproxy', [
  'ngRoute',
  'xproxy.logs.table',
  'xproxy.logs.tree',
  'xproxy.logs.compare',
  'xproxy.search',
  'xproxy.logexplorer',
  'xproxy.diff',
  'angularBootstrapNavTree',
  'n3-line-chart'
]).
config(['$locationProvider', '$routeProvider', '$httpProvider', function ($locationProvider, $routeProvider, $httpProvider) {
  $locationProvider.hashPrefix('!');
  $httpProvider.defaults.useXDomain = true;
  $routeProvider.otherwise({redirectTo: '/tree'});
}]);
