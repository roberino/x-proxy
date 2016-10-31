
function initialiseService($http) {
    this.$http = $http;

    console.log("Me init");

    function loadTreeFromApi(callback) {
        $http({
            url: 'http://localhost:9373/tree/-1',
            method: "GET",
            withCredentials: true,
            headers: {
                "Content-Type": "application/json; charset=utf-8"
            }
        }).then(function (response) {
            callback(response.data);
        }, function (response) {
            callback({
                error: response
            });
        });
    }

    return {
        loadTree: loadTreeFromApi
    };
}

angular.module('xproxy.logexplorer', []).factory('xproxy.logexplorer.factory', ['$http', initialiseService]);