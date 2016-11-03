'use strict';

// Declare app level module which depends on views, and components
angular.module('xproxy', [
  'ngRoute',
  'xproxy.logs.table',
  'xproxy.logs.tree',
  'xproxy.search',
  'xproxy.logexplorer',
  'xproxy.diff',
  'angularBootstrapNavTree'
]).
config(['$locationProvider', '$routeProvider', '$httpProvider', function ($locationProvider, $routeProvider, $httpProvider) {
  $locationProvider.hashPrefix('!');
  $httpProvider.defaults.useXDomain = true;
  $routeProvider.otherwise({redirectTo: '/tree'});
}]);
