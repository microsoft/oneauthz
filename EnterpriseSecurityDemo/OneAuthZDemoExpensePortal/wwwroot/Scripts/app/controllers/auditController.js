app.controller("auditController", ["$scope", "apiService", "uiService",
    function ($scope, apiService, uiService) {
        var selectedColumns = {};
        var timeToUtc = function (time) {
            return time.toISOString().slice(0, -5).replace("T", " ");
        };

        var setupPDPQuery = function () {
            selectedColumns = {
                "env_time": { width: 160 },
                "RequestId": { width: 315 },
                "Component": { width: 155 },
                "Result": { width: 80 },
                "Scope": { width: 270 },
                "RemoteAuthorizationDecision": { width: 150 },
                "CheckAccessAction": { width: 190 }
            };
            $scope.queryString = "PerRequestTableIfx \n| where Call == 'PolicyDecisionPoint:CheckAccess' \n| where ApplicationId == '9919edd0-6f13-491d-874c-52ab2957f86e' \n| where env_time >= datetime(" +
                timeToUtc(new Date(Date.now() - 30 * 60 * 1000)) + ") and env_time <= datetime(" + timeToUtc(new Date()) + ")";
        };

        var setupPAPQuery = function () {
            selectedColumns = {
                "TIMESTAMP": { width: 160 },
                "CallerObjectId": { width: 315 },
                "ServiceCalled": { width: 200 },
                "Operation": { width: 70 },
                "Entity": { width: 470 },
                "CallStatus": { width: 90 }
            };
            $scope.queryString = "let startDate         = \"datetime(" + timeToUtc(new Date(Date.now() - 24 * 60 * 60 * 1000)) + ")\";\n" +
                "let endDate          = \"datetime(" + timeToUtc(new Date()) + ")\";\n" +
                "let tenantId          = \"\"; // The GUID ID of the tenant. Leave blank to show all.\n" +
                "let applicationId   = \"9919edd0-6f13-491d-874c-52ab2957f86e\";\n" +
                "let callerObjectId = \"\"; // The GUID ObjectId value of the caller to look up. Leave blank to show all\n" +
                "let entityId           = \"\"; // The GUID ID of the entity (role assignment or definition) modified. Leave blank to show all.\n" +
                "let entityType      = \"\"; // Choose from \"RoleAssignment\" (if operationType is \"Put\" or \"\", it includes onboarded role assignments) and \"RoleDefinition\". Leave blank to show all.\n" +
                "let operationType	= \"\"; // choose from \"Delete\" and \"Put\" (if entityType is \"RoleAssignment\" or \"\", it includes onboarded role assignments). Leave blank to show all.\n" +
                "AsmIfxAuditApp | where (TIMESTAMP >= todatetime(startDate) and TIMESTAMP <= todatetime(endDate))\n" +
                "| where (EventPayload contains \"PAP\" or EventPayload contains \"System\" or EventPayload contains \"OnboardApplication\")\n" +
                "| where (EventPayload contains \"Put\" or EventPayload contains \"Delete\"or EventPayload contains \"OnboardApplication\")\n" +
                "| extend eventDataSplit       = split(EventPayload, \"||\")\n" +
                "| extend CallStatus           = split(eventDataSplit[1], \"$$$$\", 1)[0]\n" +
                "| extend RequestId            = split(eventDataSplit[5], \"$$$$\", 1)[0]\n" +
                "| extend SourceIP             = split(eventDataSplit[6], \"$$$$\", 1)[0]\n" +
                "| extend CallerType           = split(eventDataSplit[7], \"$$$$\", 1)[0]\n" +
                "| extend CallerObjectId       = replace(@'_', @'', tostring(split(eventDataSplit[8], \"$$$$\", 1)[0]))\n" +
                "| extend tenantAppAndEntity   = split(tostring(split(eventDataSplit[10], \"$$$$\", 1)), \"__\")\n" +
                "| extend TenantId             = replace(@'\"]', @'', replace(@'(\\\[\")', @'', tostring(tenantAppAndEntity[0])))\n" +
                "| extend ApplicationId        = tenantAppAndEntity[1]\n" +
                "| extend EntityId             = replace(@'\"]', @'', replace(@'(\\\[\")', @'', tostring(tenantAppAndEntity[2])))\n" +
                "| extend operationName        = split(eventDataSplit[0], \"$$$$\", 1)[0]\n" +
                "| extend ServiceCalled        = split(operationName, \":\", 0)[0]\n" +
                "| extend Operation            = split(operationName, \":\", 1)[0]\n" +
                "| extend Entity               = split(eventDataSplit[2], \"$$$$\", 1)[0]\n" +
                "| where (\n" +
                "      ApplicationId   == iif(applicationId == \"\",     ApplicationId,		applicationId)\n" +
                "  and CallerObjectId	== iif(callerObjectId == \"\",	CallerObjectId,		callerObjectId)\n" +
                "and TenantId        == iif(tenantId == \"\",          TenantId,			tenantId)\n" +
                "and EntityId        == iif(entityId == \"\",          EntityId,			entityId)\n" +
                "  )\n" +
                "| where (\n" +
                "      (Operation	== iif(operationType == \"\",	Operation,	operationType)) and (ServiceCalled	contains iif(entityType == \"\",	ServiceCalled,	entityType))\n" +
                "or	(Operation	== iif(entityType == \"RoleAssignment\" and operationType == \"Put\",	\"OnboardApplication\",	\"\"))\n" +
                "  )\n" +
                "| project TIMESTAMP, CallerType, CallerObjectId, SourceIP, TenantId, ApplicationId, ServiceCalled, Operation, EntityId, Entity, CallStatus\n" +
                "| order by TIMESTAMP asc";
        };

        var getKustoLogs = function () {
            $scope.isLoading = true;
            var query = {
                db: "AADPAS",
                csl: $scope.queryString
            };
            apiService.getKustoQuery(query).then(
                function (response) {
                    $scope.gridOptions = {
                        enableSorting: true,
                        enableGridMenu: true,
                        columnDefs: [],
                        minimumColumnSize: 70,
                        data: response.data.rows
                    };
                    var i = 0;
                    angular.forEach(response.data.columns, function (column) {
                        var def = {
                            name: column.columnName,
                            field: i.toString(),
                            visible: false
                        };

                        if (selectedColumns[column.columnName] !== undefined) {
                            def.width = selectedColumns[column.columnName].width;
                            def.visible = true;
                            def.cellClass = function (grid, row, col) {
                                if (grid.getCellValue(row, col).substring(0, 4) === "True")
                                    return "green";
                                if (grid.getCellValue(row, col).substring(0, 5) === "False")
                                    return "red";
                                return null;
                            };
                            if (column.columnType === "datetime") {
                                //def.cellFilter = "kustoDatetimefilter";
                                def.sort = { direction: "desc", priority: 0 };
                            }
                        }
                        else if (column.columnName === "Message") {
                            def.visible = true;
                        }
                        i++;
                        $scope.gridOptions.columnDefs.push(def);
                    });
                }, function (error) {
                    uiService.handleHttpError(error);
                })
                .finally(function () {
                    $scope.isLoading = false;
                });
        };

        $scope.execquery = function () {
            getKustoLogs();
        };

        $scope.papQuery = function () {
            setupPAPQuery();
            getKustoLogs();
        };

        $scope.pdpQuery = function () {
            setupPDPQuery();
            getKustoLogs();
        };

        var initialize = function () {
            $scope.isLoading = false;
            $scope.gridOptions = {};

            setupPDPQuery();
            getKustoLogs();
        };

        initialize();
    }]);


app.filter("kustoDatetimefilter", function ($filter) {
    return function (value) {
        if (!value) { return ""; }
        var localTime = new Date(value + " UTC");
        var timeString = localTime.toLocaleString();
        return timeString;
    };
});