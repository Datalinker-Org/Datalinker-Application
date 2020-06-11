define(["bootstrap", "dialog", "spinner", "toastr", "error-modal"], function(wizard, dialog, spinner, toastr, error) {

    "use strict";
    var self = {};
    var $ui = $(".account-ui");

    self.bindUIEvents = function () {

        var change = "change";
        var click = "click";

        var statusCheckBox = ".account-status";
        $ui.off(change, statusCheckBox);
        $ui.on(change, statusCheckBox, function (event) {

            var $this = $(this);
            self.changeStatus(event, $this);
        });

        var approveLoBtn = "#approve-legal-officer";
        $ui.off(click, approveLoBtn);
        $ui.on(click, approveLoBtn, function () {

            var $this = $(this),
                id = $this.data("id"),
                url = $this.data("url");
            self.approveLegalOfficer(id, url, $this);
        });

        var declineLoBtn = "#decline-legal-officer";
        $ui.off(click, declineLoBtn);
        $ui.on(click, declineLoBtn, function () {

            var $this = $(this),
                id = $this.data("id"),
                url = $this.data("url");
            self.declineLegalOfficer(id, url, $this);
        });

        var singleLoCheckBox = ".isSingleLegalOfficer";
        $ui.off(change, singleLoCheckBox);
        $ui.on(change, singleLoCheckBox, function () {

            var $this = $(this),
                isSingleLegalOfficer = $this.data("singlelegalofficer") === "True";
            self.confirmOrganizationDeactivation(isSingleLegalOfficer, $this);
        });
    }

    self.declineLegalOfficer = function(userId, url, $this) {

        var title = "Decline legal officer",
            message = "Are you sure you want to decline this user as Legal Officer?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "User was declined as Legal Officer";
            var errorMsg = "Unable to decline this Legal Officer";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: userId },
                success: function() {
                    toastr.info(successMsg);
                    $this.parent().html("");
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.approveLegalOfficer = function(userId, url, $this) {

        var title = "Approve legal officer",
            message = "Are you sure you want to approve this user as Legal Officer?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "Legal officer was successfully approved";
            var errorMsg = "Unable to mark this user as Legal Officer";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: userId },
                success: function() {
                    toastr.success(successMsg);
                    $this.parent().html('<label class="label label-success">Legal Officer</label>');
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.confirmOrganizationDeactivation = function (isSingleLegalOfficer, $this) {

        var isChecked = $this.is(":checked");
        if (isChecked) {
            return;
        }

        var title = "Warning: Organization will be deactivated",
            message = "You are a single Legal officer in your organization. Once this change will be saved DataLinker will deactivate your organization until another legal officer registered.\nDo you want to proceed?";
        dialog.init(title, message, undefined, callback);

        function callback() {

            $this.prop("checked", !isChecked);
        }
    };

    self.changeStatus = function (event, $this) {

        var result,
            accountId = $this.data("accountid"),
            isChecked = $this.is(":checked"),
            isAdmin = $this.data("isadmin"),
            isSingleLegal = $this.data("issinglelegal") === "True",
            url = $this.data("url");
        if (isChecked) {
            result = "activate";
        } else {
            result = "deactivate";
        }

        var title = "User status",
            message = "Are you sure you want to " + result + " this user?";
        if (isSingleLegal) {
            title = "WARNING: Deactivation of single Legal Officer";
            message = "Organization which user belongs to will not have legal officers.\nDataLinker will deactivate this organization until another legal officer registered.\nDo you want to proceed?";
        }

        dialog.init(title, message, yesCallback, noCallback);
        function yesCallback() {

            if (!isChecked && isAdmin !== "True") {
                $this[0].setAttribute("disabled", "true");
            }
            var successMsg = "User was saccessfully " + result + "d";
            var errorMsg = "Unable to update status for this user";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: accountId, value: isChecked },
                success: function() {
                    toastr.success(successMsg);
                },
                error: function() {
                    error.init({ body: errorMsg });
                }
            });
        }

        function noCallback() {
            $this.prop("checked", !isChecked);
        }
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
    }

    self.bindUIEvents();
    return self;
});