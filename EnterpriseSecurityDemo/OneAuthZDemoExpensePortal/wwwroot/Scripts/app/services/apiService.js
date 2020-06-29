angular.module('app')
    .provider('apiService', function () {
        var currentConfig = undefined;

        this.init = function (config) {
            currentConfig = config;
        };

        this.$get = ['$http', 'adalAuthenticationService', '$q', function ($http, adalService, $q) {
            return {
                getUserThumbnail: function () {
                    return $http.get('/api/Authorization/GetUserThumbnail');
                },
                getPermissions: function (impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.get('/api/Authorization/GetPermissions', {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.get('/api/Authorization/GetPermissions', {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                getAttributeCatalog: function (impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.get('/api/AttributeStore/GetDomainValues', {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.get('/api/AttributeStore/GetDomainValues', {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                getKustoQuery: function (csl, impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.post("/api/AzureKusto/KustoQuery/", csl, {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.post("/api/AzureKusto/KustoQuery/", csl, {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                getPendingExpenses: function (impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.get("/api/Expenses/GetPendingExpenses", {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.get("/api/Expenses/GetPendingExpenses", {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                getCompletedExpenses: function (impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.get("/api/Expenses/GetCompletedExpenses", {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.get("/api/Expenses/GetCompletedExpenses", {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                submitExpense: function (expense, impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.put("/api/Expenses", expense, {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.put("/api/Expenses", expense, {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                approveExpense: function (id, impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.put("/api/Expenses/" + id + "/Approve", null, {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.put("/api/Expenses/" + id + "/Approve", null, {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                rejectExpense: function (id, impersonationUser) {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.put("/api/Expenses/" + id + "/Reject", null, {
                                headers: {
                                    "ImpersonationUser": impersonationUser,
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.put("/api/Expenses/" + id + "/Reject", null, {
                            headers: {
                                "ImpersonationUser": impersonationUser
                            }
                        });
                    }
                },
                // Can't be impersonated
                resetExpenseData: function () {
                    if (currentConfig === 'Local') {
                        var defer = $q.defer();
                        adalService.acquireToken('https://pas.windows.net').then(function (response) {
                            $http.delete("/api/Expenses/Reset", {
                                headers: {
                                    "AccessToken": response
                                }
                            }).then(function (response) {
                                defer.resolve(response);
                            }, function (error) {
                                defer.reject(error);
                            });
                        }, function (error) {
                            defer.reject(error);
                        });

                        return defer.promise;
                    }
                    else {
                        return $http.delete("/api/Expenses/Reset");
                    }
                }
            };
        }];        
});