angular.module('app')
    .controller('indexController', [
        '$scope', '$location', 'adalAuthenticationService', 'apiService', 'uiService', function ($scope, $location, adalService, apiService, uiService) {
            // UI tab control
            $scope.tab = 0;

            $scope.setTab = function (newTab) {
                if ($scope.tab === newTab) {
                    return;
                }
                $scope.tab = newTab;
            };

            $scope.isSet = function (tabNum) {
                return $scope.tab === tabNum;
            };

            //Progress control
            $scope.pendingRequests = 0;

            //Model
            $scope.model = {};
            $scope.currentUser = "";
            $scope.isImpersonating = false;
            $scope.permissions = {};

            $scope.refreshTop = function () {
                $scope.refreshCurrentuser();

                $scope.notifyPendingRequests(1);
                apiService.getPermissions($scope.isImpersonating ? $scope.currentUser : undefined)
                    .then(function successCallback(response) {
                        $scope.permissions = response.data;
                        ResetTabIfneeded();
                    }, function (error) {
                        uiService.handleHttpError(error);
                        $scope.permissions = {};
                    }).finally(function () {
                        $scope.notifyPendingRequests(-1);
                    });
            };

            function ResetTabIfneeded() {
                if ($scope.tab === 1 && !$scope.permissions.submit
                    || $scope.tab === 4 && !$scope.permissions.audit) {
                    $scope.tab = 0;
                    $location.path("/home");
                }
            }

            $scope.getThumbnail = function () {
                apiService.getUserThumbnail()
                    .then(function success(result) {
                        $scope.userThumbnail = result.data && result.data.length !== 0 ? result.data.replace(/['"]+/g, '') : null; //Remove extra quotation marks
                    }, function errorCallback(error) {
                        uiService.handleHttpError(error);
                        $scope.userThumbnail = null;
                    });
            };

            $scope.resetExpenseList = function () {
                $scope.notifyPendingRequests(1);
                itemService.initializeList().then(function () {
                    uiService.notify('The list has been reset!', 'info');
                }, function (error) {
                    uiService.handleHttpError(error);
                }).finally(function () {
                    $scope.notifyPendingRequests(-1);
                });
            };

            $scope.refreshCurrentuser = function () {
                $scope.isImpersonating = $scope.model.impersonationUser && $scope.model.impersonationUser !== adalService.userInfo.userName.substring(0, adalService.userInfo.userName.indexOf('@'));
                $scope.currentUser = $scope.isImpersonating ? $scope.model.impersonationUser : adalService.userInfo.userName.substring(0, adalService.userInfo.userName.indexOf('@'));
            };

            $scope.notifyPendingRequests = function (number) {
                $scope.pendingRequests += number;
            };

            // After sucessful login
            $scope.$on('adal:loginSuccess', function (evt, data) {
                $scope.getThumbnail();
                $scope.refreshTop();
            });

            // Onload
            $scope.getThumbnail();
            $scope.refreshTop();
            $location.path("/home");
        }]);