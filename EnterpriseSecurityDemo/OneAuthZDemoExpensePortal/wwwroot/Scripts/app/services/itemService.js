angular.module('app')
    .service('itemService', ['apiService','$q', function (apiService,$q) {
        this.initializeList = function () {
            var deferred = $q.defer();

            apiService.resetExpenseData().then(function success(response) {
                $q.all(refreshPermission()).then(function success(response) {
                    deferred.resolve();
                    }, function error(error) {
                        deferred.reject();
                    });
            }, function error(error) {
                deferred.reject();
            });

            return deferred.promise;
        };

        this.refreshPendingItemList = function (impersonationUser) {
            return apiService.getPendingExpenses(impersonationUser);
        };

        this.refreshCompletedItemList = function (impersonationUser) {
            return apiService.getCompletedExpenses(impersonationUser);
        };

        this.create = function (item, impersonationUser) {
            return apiService.submitExpense(item, impersonationUser);
        };

        this.approve = function (item, impersonationUser) {
            return apiService.approveExpense(item.id, impersonationUser);
        };

        this.reject = function (item, impersonationUser) {
            return apiService.rejectExpense(item.id, impersonationUser);
        };
    }]);