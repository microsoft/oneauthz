angular.module('app')
    .controller('pendingController', ['$scope', 'itemService', 'uiService',
        function ($scope, itemService, uiService) {
            $scope.refreshPendingList = function () {
                $scope.refreshCurrentuser();

                $scope.notifyPendingRequests(1);
                $scope.pendingItemList = null;
                itemService.refreshPendingItemList($scope.isImpersonating ? $scope.currentUser : undefined).then(function (response) {
                    $scope.pendingItemList = response.data;
                }, function (error) {
                    uiService.handleHttpError(error);
                }).finally(function () {
                    $scope.notifyPendingRequests(-1);
                });
            };

            $scope.approve = function (item) {
                $scope.notifyPendingRequests(1);
                
                itemService.approve(item, $scope.isImpersonating ? $scope.currentUser : undefined).then(function (response) {
                    var index = $scope.pendingItemList.indexOf(item);
                    if (index > -1) {
                        $scope.pendingItemList.splice(index, 1);
                    }
                    var result = response.data;
                    if (result) {
                        if ($scope.completedItemList === undefined)
                            $scope.completedItemList = [];
                        $scope.completedItemList.push(result);
                    }
                    uiService.notify('An expense: ' + item.name + ' has been aproved.', 'info');
                }, function (error) {
                    uiService.handleHttpError(error);
                }).finally(function () {
                    $scope.notifyPendingRequests(-1);
                });
            };

            $scope.reject = function (item) {
                $scope.notifyPendingRequests(1);
                itemService.reject(item, $scope.isImpersonating ? $scope.currentUser : undefined).then(function (response) {
                    var index = $scope.pendingItemList.indexOf(item);
                    if (index > -1) {
                        $scope.pendingItemList.splice(index, 1);
                    }
                    var result = response.data;
                    if (result) {
                        if ($scope.completedItemList === undefined)
                            $scope.completedItemList = [];
                        $scope.completedItemList.push(result);
                    }
                    uiService.notify('An expense: ' + item.name + ' has been rejected.', 'info');
                }, function (error) {
                    uiService.handleHttpError(error);
                }).finally(function () {
                    $scope.notifyPendingRequests(-1);
                });
            };

            $scope.refreshPendingList();
        }]);