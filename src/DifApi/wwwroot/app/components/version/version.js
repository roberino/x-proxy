'use strict';

angular.module('xproxy.version', [
  'xproxy.version.interpolate-filter',
  'xproxy.version.version-directive'
])

.value('version', '0.1');
