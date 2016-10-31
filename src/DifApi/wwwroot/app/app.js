'use strict';

// Declare app level module which depends on views, and components
angular.module('xproxy', [
  'ngRoute',
  'xproxy.view1',
  'xproxy.view2',
  'xproxy.version',
  'xproxy.logexplorer',
  'angularBootstrapNavTree'
]).
config(['$locationProvider', '$routeProvider', '$httpProvider', function ($locationProvider, $routeProvider, $httpProvider) {
  $locationProvider.hashPrefix('!');
  $httpProvider.defaults.useXDomain = true;
  $routeProvider.otherwise({redirectTo: '/view1'});
}]);
