angular.module('app')
    .service('uiService', [function () {
        this.handleHttpError = function (error) {
            if (error === 'User login is required') {
                // Swallow for first time login
            }
            else if (error.status === 401 || error.status === -1) {
                this.notify("Token has expired. Please login again or refresh page.");
            }
            else if (error.status === 403) {
                this.notify("403 Forbidden error\n" + error.data);
            }
            else if (error.data !== undefined && error.data !== null) {
                var msg;
                if (angular.isString(error.data))
                    msg = error.data;
                else
                    msg = JSON.stringify(error.data);
                this.notify("Http Error Code (" + error.status + ")\n" + msg);
            }
            else {
                this.notify("Unknown Http Error.\n" + JSON.stringify(error));
            }
        };

        this.notify = function (message, type) {
            $.notify({
                message: message
            }, {
                    type: typeof type !== 'undefined' ? type : 'danger',
                    placement: {
                        from: "top",
                        align: "center"
                    },
                    dalay: 100,
                    offset: 60
                });
        };
    }]);