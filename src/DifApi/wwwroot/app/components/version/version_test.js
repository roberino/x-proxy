'use strict';

describe('xproxy.version module', function() {
  beforeEach(module('xproxy.version'));

  describe('version service', function() {
    it('should return current version', inject(function(version) {
      expect(version).toEqual('0.1');
    }));
  });
});
