
function initialiseService($http) {

    var searchCallbacks = [];
    var currentSearch = "";

    function corsRequest(path, callback) {
        $http({
            url: "http://localhost:9373" + path,
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

    function loadComparisonFromApi(req1, req2, callback) {
        var pathTemplate = "/source/compare?host1=$1&host2=$2&path1=$3&path2=$4&id1=$5&id2=$6";
        var url = pathTemplate
            .replace("$1", req1.originHost)
            .replace("$2", req2.originHost)
            .replace("$3", req1.originPath)
            .replace("$4", req2.originPath)
            .replace("$5", req1.id)
            .replace("$6", req2.id);

        corsRequest(url, callback);
    }

    function loadTimeHistogramFromApi(callback) {
        corsRequest("/logs/histogram/time-series/all/60000", callback);
    }

    function loadTimeHistogramByMimeFromApi(callback, timeRangeSecs) {
        corsRequest("/logs/histogram/time-series/by-mime/" + (timeRangeSecs || 2000), callback);
    }

    function loadTreeFromApi(callback) {
        corsRequest("/logs/tree/-1", callback);
    }

    function loadLogsFromApi(callback) {
        corsRequest("/logs/list/-1", callback);
    }

    function loadSourceFromApi(host, path, id, callback) {
        var url = "/source?host=" + host + "&path=" + path + "&id=" + id;
        corsRequest(url, callback);
    }

    function loadUrlFilterFromApi(path, callback) {
        var url = "/logs/url-filter/-1?path=" + path;
        corsRequest(url, callback);
    }

    function searchFromApi(query, callback) {
        var url = "/logs/search?q=" + query;
        corsRequest(url, function (data) {

            currentSearch = query;
            data.queryText = query;

            if (callback) callback(data);

            if (searchCallbacks) {
                for (var i = 0; i < searchCallbacks.length; i++) {
                    searchCallbacks[i](data);
                }
            }
        });
    }

    function onSearch(callback){
        searchCallbacks.push(callback);
    }

    return {
        loadLogs: loadLogsFromApi,
        loadComparison: loadComparisonFromApi,
        loadTree: loadTreeFromApi,
        loadTimeHistogram: loadTimeHistogramFromApi,
        loadTimeHistogramByMime: loadTimeHistogramByMimeFromApi,
        filterByPath: loadUrlFilterFromApi,
        loadSource: loadSourceFromApi,
        search: searchFromApi,
        onSearch: onSearch,
        currentSearchText: function () { return currentSearch; }
    };
}

angular.module('xproxy.logexplorer', []).factory('xproxy.logexplorer.service', ['$http', initialiseService]);