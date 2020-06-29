angular.module('app')
    .controller('completedController', ['$scope', 'itemService', 'uiService',
        function ($scope, itemService, uiService) {
            $scope.refreshCompletedList = function () {
                $scope.refreshCurrentuser();

                $scope.notifyPendingRequests(1);
                $scope.completedItemList = null;
                itemService.refreshCompletedItemList($scope.isImpersonating ? $scope.currentUser : undefined).then(function (response) {
                    $scope.completedItemList = response.data;
                }, function (error) {
                    uiService.handleHttpError(error);
                }).finally(function () {
                    $scope.notifyPendingRequests(-1);
                });
            };

            $scope.refreshCompletedList();
        }]);