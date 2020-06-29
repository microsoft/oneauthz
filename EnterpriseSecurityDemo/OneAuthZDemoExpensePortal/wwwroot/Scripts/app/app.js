var app = angular.module('app', ['ngRoute', 'AdalAngular', "ui.grid", "ui.grid.autoResize", "ui.grid.moveColumns", "ui.grid.resizeColumns"])
    .config(['$routeProvider', '$locationProvider', '$httpProvider', 'adalAuthenticationServiceProvider', 'apiServiceProvider', function ($routeProvider, $locationProvider, $httpProvider, adalProvider, apiServiceProvider) {
        $routeProvider.when("/home", {
            templateUrl: "Views/home.html",
            requireADLogin: true
        })
        .when("/create", {
            templateUrl: "Views/create.html",
            requireADLogin: true
        })
        .when("/pending", {
            templateUrl: "Views/pending.html",
            requireADLogin: true
        })
        .when("/completed", {
            templateUrl: "Views/completed.html",
            requireADLogin: true
        })
        .when("/audit", {
            templateUrl: "Views/audit.html",
            requireADLogin: true
        });

        $locationProvider.html5Mode(false).hashPrefix('');

        var currentConfig = "Local";

        var authenticationConfig = {
            "Local": {
                instance: 'https://login.microsoftonline.com/',
                //cacheLocation: 'localStorage', // enable this for IE, as sessionStorage does not work for localhost.
                tenant: 'microsoft.onmicrosoft.com',
                clientId: 'ed4a95de-be05-4500-bb79-3bf86ff61139',
                endpoints: {
                    "api/": "ed4a95de-be05-4500-bb79-3bf86ff61139"
                }
            },
            "Service": {
                instance: 'https://login.microsoftonline.com/',
                //cacheLocation: 'localStorage', // enable this for IE, as sessionStorage does not work for localhost.
                tenant: 'microsoft.onmicrosoft.com',
                clientId: '9919edd0-6f13-491d-874c-52ab2957f86e',
                endpoints: {
                    "api/": "9919edd0-6f13-491d-874c-52ab2957f86e"
                }
            },
            "ContosoDeb": {
                instance: 'https://login.microsoftonline.com/',
                //cacheLocation: 'localStorage', // enable this for IE, as sessionStorage does not work for localhost.
                tenant: 'contosodeb.onmicrosoft.com',
                clientId: '75d1ec64-2a06-45e1-9f2b-8ed9d4386060',
                endpoints: {
                    "api/": "75d1ec64-2a06-45e1-9f2b-8ed9d4386060"
                }
            }
        };

        adalProvider.init(authenticationConfig[currentConfig], $httpProvider);
        apiServiceProvider.init(currentConfig);
    }]);
