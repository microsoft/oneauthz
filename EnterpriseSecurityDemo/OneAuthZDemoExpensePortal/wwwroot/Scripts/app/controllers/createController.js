angular.module('app')
    .controller('createController', ['$scope', 'itemService', 'apiService', 'uiService',
        function ($scope, itemService, apiService, uiService) {
            $scope.expense = {};

            $scope.submit = function () {
                if (!$scope.expense.name) {
                    uiService.notify('Please enter a name.', 'warning');
                }
                else if (!$scope.expense.org || ($scope.expense.org === "Other" && !$scope.expense.otherOrg)) {
                    uiService.notify('Please select/enter an Organization.', 'warning');
                }
                else if (!$scope.expense.category || ($scope.expense.category === "Other" && !$scope.expense.other)) {
                    uiService.notify('Please select/enter an Category.', 'warning');
                }
                else if (isNaN(parseInt($scope.expense.amount)) || parseInt($scope.expense.amount) <= 0) {
                    uiService.notify('Please enter a positive amount.', 'warning');
                }
                else {
                    $scope.notifyPendingRequests(1);
                    itemService.create({
                        name: $scope.expense.name,
                        category: $scope.expense.category !== "Other" ? $scope.expense.category : $scope.expense.other,
                        owner: $scope.currentUser,
                        org: $scope.expense.org !== "Other" ? $scope.expense.org : $scope.expense.otherOrg,
                        amount: parseInt($scope.expense.amount)
                    }, $scope.isImpersonating ? $scope.currentUser : undefined).then(function () {
                        uiService.notify($scope.expense.name + ' has been created.', 'info');
                    }, function (error) {
                        uiService.handleHttpError(error);
                    }).finally(function () {
                        $scope.notifyPendingRequests(-1);
                    });
                }
            };

            var refreshAttributeValueList = function () {
                apiService.getAttributeCatalog().then(function successCallback(result) {
                    $scope.scopeValueList = result.data["Categories"];
                    $scope.orgValueList = result.data["Org"];
                }, function errorCallback(error) {
                    handleHttpError(error);
                });
            };

            refreshAttributeValueList();
        }]);