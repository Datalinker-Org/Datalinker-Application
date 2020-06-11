define(["require", "jqueryValidateUnobtrusive", "toastr", "spinner", "dialog", "error-modal"], function(require, jqV, toastr, spinner, dialog, error) {

    "use strict";
    var self = {};
    var $ui = $("#applications-ui");
    var smallSpin = '<div><div style="text-align: center;"><span class="glyphicon glyphicon-refresh icon-spin"></span> Processing...</div></div>';
    var linkToRefreshPage = window.location.href;
    var displayUrlToRefreshPage = '<a href="' + linkToRefreshPage + '">here</a>';
    var click = "click";
    var change = "change";
    var errorMsg = "Unable to process your request.";

    self.bindUIEvents = function() {

        var createApplicationBtn = ".create-new-application";
        $ui.off(click, createApplicationBtn);
        $ui.on(click, createApplicationBtn, function(event) {

            var $this = $(this);
            self.showCreateApplicationDialog(event, $(this).data("appType"), $this.data("url"));
        });

        // Service list page
        var statusCheckBox = ".service-status";
        $ui.off(change, statusCheckBox);
        $ui.on(change, statusCheckBox, function(event) {

            var checkbox = $(this);
            self.changeStatus(event, checkbox);
        });

        var editBtn = ".edit-application-details-btn";
        $ui.off(click, editBtn);
        $ui.on(click, editBtn, function(event) {

            event.preventDefault();
            var url = $(this).attr("href");
            var location = ".edit-application-details-modal-location";
            var $location = $(location);
            spinner.on();
            $.ajax({
                url: url,
                success: function (r) {
                    $location.html(r);
                    $.validator.unobtrusive.parse(location);
                    self.openEditApplicationModal();
                    spinner.off();
                },
                error: function (r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });

        // Save application details
        var saveDetailsBtn = ".save-application-details-btn";
        $ui.off(click, saveDetailsBtn);
        $ui.on(click, saveDetailsBtn, function() {
            var form = $("#edit-application-details-form").closest("form");
            form.submit();
        });

        // Create new application host
        var addHostBtn = ".save-new-application-host-btn";
        $ui.off(click, addHostBtn);
        $ui.on(click, addHostBtn, function() {

            var errorMsg = "Unable to add new host. Make sure it's a valid URL";
            var successMsg = "New host was successfully added";
            var addHostForm = "#add-new-host-form";
            var url = $(addHostForm).attr("action");
            var $form = $(addHostForm);
            if (!$form.valid()) {
                return;
            }

            spinner.on();
            $.ajax({
                url: url,
                method: "POST",
                data: $form.serializeArray(),
                success: function () {
                    toastr.success(successMsg);
                    $("#add-new-application-host").modal("toggle");
                    self.populateHosts();
                },
                error: function() {
                    error.init({ body: errorMsg });
                },
                complete: function () {
                    spinner.off();
                }
            });
        });

        // Endpoint details page
        var editEndpointBtn = ".edit-endpoint-details";
        $ui.off(click, editEndpointBtn);
        $ui.on(click, editEndpointBtn, function(event) {

            var $this = $(this);
            event.preventDefault();
            var $location = $(".edit-user-details-modal-location");
            spinner.on();
            $.ajax({
                url: $this.data("url"),
                success:function(r) {
                    spinner.off();
                    $location.html(r);
                    self.openEditEndpoint();
                },
                error: function (r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });

        // Add endpoint page
        var addEndpointBtn = ".add-new-endpoint";
        $ui.off(click, addEndpointBtn);
        $ui.on(click, addEndpointBtn, function(event) {

            var errorMsg = "Unable to process your request";
            event.preventDefault();
            var $this = $(this);
            var url = $this.data("url");
            var modalLocation = ".add-schema-endpoint-modal-location";
            var $location = $(modalLocation);
            spinner.on();
            $.ajax({
                url: url,
                success: function(response) {
                    spinner.off();
                    if ($this.data("is-all-schemas-in-use") === "True") {
                        error.init({ body: errorMsg });
                        return;
                    }
                    $location.html(response);
                    $.validator.unobtrusive.parse(modalLocation);
                    self.openAddProviderEdnpointModal();
                },
                error: function(r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });

        // Software statement
        var viewSoftwareStmtBtn = ".software-statement-btn";
        $ui.off(click, viewSoftwareStmtBtn);
        $ui.on(click, viewSoftwareStmtBtn, function(event) {

            event.preventDefault();
            var url = $(this).data("url");
            var generateUrl = $(this).data("generate-new-url");
            var $location = $(".stmt-modal-location");
            
            spinner.on();
            $.ajax({
                url: url,
                success:function(r) {
                    spinner.off();
                    $location.html(r);
                    self.bindGenerateStmt(generateUrl);
                    self.openStmtModal();
                },
                error:function(r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });

        // Authentication page
        var editAuthBtn = "#edit-service-authentication";
        $ui.off(click, editAuthBtn);
        $ui.on(click, editAuthBtn, function() {

            var url = $(this).data("url");
            var modalLocation = ".add-schema-endpoint-modal-location";
            var $location = $(modalLocation);
            spinner.on();

            $.ajax({
                url: url,
                success: function (r) {
                    spinner.off();
                    $location.html(r);
                    $.validator.unobtrusive.parse(modalLocation);
                    self.openEditAuthModal();
                },
                error: function (r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });
        
        // Industry good
        var approveIndustryGoodBtn = "#approve-industry-good";
        $ui.off(click, approveIndustryGoodBtn);
        $ui.on(click, approveIndustryGoodBtn, function() {
            var btn = $(this),
                id = btn.data("id"),
                url = btn.data("url");
            self.approveIndustryGood(id, url, btn);
        });

        var declineIndustryGoodBtn = "#decline-industry-good";
        $ui.off(click, declineIndustryGoodBtn);
        $ui.on(click, declineIndustryGoodBtn, function() {
            var btn = $(this),
                id = btn.data("id"),
                url = btn.data("url");
            self.declineIndustryGood(id, url, btn);
        });

        self.consumerRequests();
    };

    self.consumerRequests = function() {
        var errorMsg = "Unable to get count of Consumer requests";
        var btn = $(".consumer-requests-badge");
        if (btn.length > 0) {
            var url = btn.data("url");
            $.ajax({
                type: "GET",
                url: url,
                async: false,
                success: function(response) {
                    if (response != undefined && response > 0) {
                        btn.text(response);
                    }
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }
    }

    self.bindGenerateStmt = function (generateUrl) {

        var createSoftwareStmtBtn = ".update-stmt-btn";
        $ui.off(click, createSoftwareStmtBtn);
        $ui.on(click, createSoftwareStmtBtn, function () {

            var errorMsg = "Unable to update software statement.";
            var successMsg = "Software statement was successfully updated.";

            spinner.on();
            $.ajax({
                type:"POST",
                url: generateUrl,
                content: "application/json; charset=utf-8",
                success: function (r) {
                    spinner.off();
                    $("#stmt-display").val(r.newStatement);
                    toastr.success(successMsg);
                },
                error: function (r) {
                    spinner.off();
                    if (r.responseJSON != undefined) {
                        error.init({ body: r.responseJSON.msg });
                    } else {
                        error.init({ body: errorMsg });
                    }
                }
            });
        });
    };

    self.openEditEndpoint = function() {
        var location = ".edit-user-details-modal-location";
        var $location = $(location);
        $.validator.unobtrusive.parse(location);

        var saveEndpointBtn = ".save-provider-endpoint-details-btn";
        $location.off(click, saveEndpointBtn);
        $location.on(click, saveEndpointBtn, function() {
            var form = $("#provider-endpoint-form");
            form.submit();
        });
        
        $("#edit-provider-endpoint-modal").modal("toggle");
    };

    self.openStmtModal = function() {
        require(["copy"], function(cp) {
            $(".copy-stmt").off("click");
            $(".copy-stmt").on("click",function() {
                copyToClipboard("stmt-display");
                toastr.success("Statement was successfully copied");
            });
            $("#software-stmt-modal").modal("toggle");
        });
    };

    self.declineIndustryGood = function(appId, url, btn) {

        var title = "Decline application as Industry good",
            message = "Are you sure you want to decline this application as Industry Good?";

        var $dialog = dialog.init(title, message, callback);

        function callback() {
            var successMsg = "Application was declined as Industry Good";
            var errorMsg = "Unable to decline this Application";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: appId },
                success: function() {
                    toastr.info(successMsg);
                    btn.parent().html("");
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }

    };

    self.approveIndustryGood = function(appId, url, btn) {

        var title = "Approve application as Industry good",
            message = "Are you sure you want to mark this application as Industry Good?";
        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "Industry Good application was successfully approved.";
            var errorMsg = "Unable to mark this application as Industry Good";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: appId },
                success: function() {
                    toastr.success(successMsg);
                    btn.parent().html('<label class="label label-info">Industry Good</label>');
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.changeStatus = function(event, checkbox) {
        var result,
            applicationId = checkbox.data("applicationid"),
            url = checkbox.data("url"),
            isChecked = checkbox.is(":checked");

        if (isChecked) {
            result = "activate";
        } else {
            result = "deactivate";
        }

        var title = "Service status",
            message = "Are you sure you want to " + result + " this service?";
        dialog.init(title, message, yesCallback, noCallback);

        function yesCallback() {
            var successMsg = "Service was successfully " + result + "d";
            var errorMsg = "Unable to update status for this service";
            $.ajax({
                type: "GET",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: applicationId, value: isChecked },
                success: function() {
                    toastr.success(successMsg);
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }

        function noCallback() {
            checkbox.prop("checked", !isChecked);
        }
    };
    
    self.bindHostsUI = function() {

        self.bindShowHost();
        self.bindDeleteHost();
    };

    self.bindShowHost = function() {

        var showHostTokenBtn = ".show-host-token";
        $ui.off(click, showHostTokenBtn);
        $ui.on(click, showHostTokenBtn, function() {

            var $this = $(this),
                token = $this.data("hostToken"),
                host = $this.closest("tr").find("td:first").html(),
                url = $this.data("url");

            $(".displayed-host").text(host);
            $(".displayed-token").text(token);
            self.bindGenerateHost(url, $this);
            $("#host-details-modal").modal("toggle");
        });
    };

    self.bindDeleteHost = function () {

        var deleteHostBtn = ".delete-host";
        $ui.off(click, deleteHostBtn);
        $ui.on(click, deleteHostBtn, function (event) {

            event.preventDefault();
            var url = $(this).data("url"),
                title = "Remove Host",
                message = "Are you sure you want to remove API access to this service?";

            dialog.init(title, message, callback);

            function callback() {
                var successMsg = "";
                var errorMsg = "Unable to remove the API host.";
                $.ajax({
                    url: url,
                    method: "DELETE"
                }).done(function (data) {
                    var table = $("#application-host-details");

                    table.append(smallSpin);
                    $.ajax({
                        url: table.data("url"),
                        success: function (r) {
                            table.html(r);
                            self.bindHostsUI();
                        },
                        error: function (r) {
                            table.html(errorMsg);
                            if (r.responseJSON != undefined) {
                                error.init({ body: r.responseJSON.msg });
                            } else {
                                error.init({ body: errorMsg });
                            }
                        }
                    });
                }).fail(function () {
                    error.init({ body: errorMsg });
                });
            };
        });
    };

    self.bindGenerateHost = function (url, $this) {

        var generateHostTokenBtn = ".new-token-btn";
        $ui.off(click, generateHostTokenBtn);
        $ui.on(click, generateHostTokenBtn, function () {
            var successMsg = "New token was successfully generated.";
            var errorMsg = "Unable to generate a new API token.";
            $.ajax({
                url: url,
                method: "GET",
                dataType: "json",
                success: function (data) {

                    var table = $("#application-host-details");

                    $.ajax({
                        url: table.data("url"),
                        success: function (r) {
                            // refresh ui
                            table.html(r);
                            // update modal values
                            $(".displayed-token").text(data.token);
                            $this.data("hostToken", data.token);
                            url = data.url;
                            toastr.success(successMsg);
                        },
                        error: function (r) {
                            table.html(errorMsg);
                            if (r.responseJSON != undefined) {
                                error.init({ body: r.responseJSON.msg });
                            } else {
                                error.init({ body: errorMsg });
                            }
                        }
                    });
                },
                error: function () {
                    error.init({ body: errorMsg });
                }
            });
        });
    };

    self.isAuthorized = function() {
        var authUrl = $(".auth-link-keeper").data("authorization-url");
        var result = $.ajax({
            url: authUrl,
            content: "application/json; charset=utf-8",
            method: "GET",
            async: false,
            dataType: "json"
        }).responseText;
        return JSON.parse(result).value;
    };

    self.showCreateApplicationDialog = function(event, appType, url) {
        var $modalLocation = $(".create-new-application-modal-location");
        spinner.on();

        $.ajax({
            url: url,
            success: function (r) {
                spinner.off();
                $modalLocation.html(r);
                $.validator.unobtrusive.parse(".create-new-application-modal-location");
                self.openCreateApplicationDialog();
            },
            error: function (r) {
                spinner.off();
                $modalLocation.html(errorMsg);
                if (r.responseJSON != undefined) {
                    error.init({ body: r.responseJSON.msg });
                } else {
                    error.init({ body: errorMsg });
                }
            }
        });
    };

    self.openCreateApplicationDialog = function() {

        var appType = $("#create-application-modal").data("type");
        require(["rezBootstrapWizard", "tokenfield"], function (rBw,tf) {

            self.setupCreateApplicationWizard(appType);
        });
        $("#create-application-modal").modal("toggle");
    };

    self.openEditApplicationModal = function() {

        $("#edit-application-details-modal").modal("toggle");
    };

    self.setupCreateApplicationWizard = function(appType) {
        // We still want a way to close the dialog, just not by mistake (ESC key or mis-click)
        $(".bootstrap-dialog-close-button").show();

        $("#application-wizard").rezBootstrapWizard();

        $("#app-origin-fqdn").tokenfield({
            delimiter: [",", " "],
            minWidth: 180,
            createTokensOnBlur: true
        });

        var wizard = "#application-wizard";
        var $wizard = $(wizard);
        var name = 'input[name="name"]';
        var blur = "blur";
        $wizard.off(blur, name);
        $wizard.on(blur, name, function() {
            if ($(this).val().length > 0) {
                var serviceName = $(this).val();
                $(wizard + " .summary-app-name").each(function() { $(this).text(serviceName); });
            }
        });

        var industryGood = 'input[name="industryGood"]';
        $wizard.off(change, industryGood);
        $wizard.on(change, industryGood, function() {
            var industryGood = "#application-wizard .consumer-industry-good";
            if ($(this).prop("checked")) {
                $(industryGood).removeClass("hidden");
            } else {
                $(industryGood).addClass("hidden");
            }
        });

        if (appType === "consumer") {
            $wizard.bootstrapWizard("hide", 2);
        }

        self.bindWellKnownRequestBtn(wizard);
    };

    self.processLoadCallback = function($location, successCallback, response, status, xhr) {
        if (status === "error" || !self.isAuthorized()) {
            var errorDetails = " Log In required. Click " + displayUrlToRefreshPage + " to continue.";
            var errorMsg = "Error: unable to load page.(code: " + xhr.status + ")";
            if (xhr.status === 0 || xhr.status === 200) {
                errorMsg += errorDetails;
            }

            error.init({ body: errorMsg });
        } else {
            successCallback();
        }
    };

    self.bindWellKnownRequestBtn = function(selector) {

        var wellKnonwnBtn = "#refresh-well-known";
        $(selector).off(click, wellKnonwnBtn);
        $(selector).on(click, wellKnonwnBtn, function() {
            var url = $("#app-well-known").val();
            if (url.length > 0) {
                var errorMsg = "Unable to fetch configuration values.";
                $.ajax({
                    url: $(wellKnonwnBtn).data("url"),
                    cache: false,
                    data: { url: url }
                }).done(function(data) {
                    $("#app-auth-issuer").val(data.issuer);
                    $("#app-auth-jwks-uri").val(data.jwks_uri);
                    $("#app-auth-endpoint").val(data.authorization_endpoint);
                    $("#app-auth-token-endpoint").val(data.token_endpoint);
                    $("#app-auth-userinfo-endpoint").val(data.userinfo_endpoint);
                    $("#app-auth-end-session-endpoint").val(data.end_session_endpoint);
                    $("#app-auth-check-session-iframe").val(data.check_session_iframe);
                    $("#app-auth-revocation-endpoint").val(data.revocation_endpoint);
                }).fail(function() {
                    error.init({ body: errorMsg });
                });
            }
        });
    };

    self.createSwitch = function($selector, title, message, noDialogOn) {
        $selector.change(function() {
            var switcher = $(this);
            var value = switcher.is(":checked");

            if (value === noDialogOn) {
                return;
            }
            dialog.init(title, message, undefined, callback);

            function callback() {
                switcher.prop("checked", !value);
            };
        });
    };

    self.openEditAuthModal = function() {

        self.saveAuthDetails();
        var authModal = "#application-auth-modal";
        self.bindWellKnownRequestBtn(authModal);
        $(authModal).modal("toggle");
    };

    self.saveAuthDetails = function() {
        var saveAuthBtn = ".save-application-auth-btn";
        $ui.off(click, saveAuthBtn);
        $ui.on(click, saveAuthBtn, function() {
            var form = $("#application-auth-form");
            form.submit();
        });
    };

    self.populateHosts = function () {

        var $hostsTable = $("#application-host-details");
        $hostsTable.append(smallSpin);

        $.ajax({
            url: $hostsTable.data("url"),
            success: function (r) {
                $hostsTable.html(r);
                self.bindHostsUI();
            },
            error: function (r) {
                $hostsTable.html(errorMsg);
                if (r.responseJSON != undefined) {
                    error.init({ body: r.responseJSON.msg });
                } else {
                    error.init({ body: errorMsg });
                }
            }
        });
    }

    self.openAddProviderEdnpointModal = function() {

        self.addProviderEdnpoint();

        $("#add-provider-ednpoint-modal").modal("toggle");
    };

    self.addProviderEdnpoint = function() {

        var addEndpointBtn = ".add-provider-endpoint-btn";
        $ui.off(click, addEndpointBtn);
        $ui.on(click, addEndpointBtn, function() {
            var form = $("#add-provider-endpoint-form");
            form.submit();
        });
    };

    self.bindUIEvents();
    return self;
});