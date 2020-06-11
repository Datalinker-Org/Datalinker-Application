define(["dialog", "toastr", "error-modal", "jqueryValidateUnobtrusive"], function (dialog,toastr,error,jQueryVal) {

    "use strict";
    var self = {};
    var $ui = $("#organization-ui");
    var change = "change";
    self.bindUIEvents = function() {

        var statusCheckBox = ".organization-status";
        $ui.off(change, statusCheckBox);
        $ui.on(change, statusCheckBox, function(event) {

            var checkbox = $(this);
            self.changeStatus(event, checkbox);
        });
    };

    self.changeStatus = function (event, checkbox) {
        var result,
            organizationId = checkbox.data("organizationid"),
            url = checkbox.data("url"),
            isChecked = checkbox.is(":checked");
        if (isChecked) {
            result = "activate";
        } else {
            result = "deactivate";
        }

        var title = "Organization status",
            message = "Are you sure you want to " + result + " this organization?";

        dialog.init(title, message, yescallback, nocallback);

        function yescallback() {
            var successMsg = "Organization was successfilly " + result + "d";
            var errorMsg = "Unable to update status for this organization";
            $.ajax({
                type: "GET",
                url: url,
                content: "application/json; charset=utf-8",
                data: { id: organizationId, value: isChecked },
                success: function () {
                    toastr.success(successMsg);
                },
                error: function (jqXHR, textStatus) {
                    checkbox.prop("checked", !isChecked);
                    error.init({ body: errorMsg});
                }
            });
        }

        function nocallback() {

            // return old value
            checkbox.prop("checked", !isChecked);
        }
    };

    self.setupCreateOrganization = function () {

        var register = ".register";
        var $register = $(register);

        var blur = "blur";
        // extend jquery range validator to work for required checkboxes
        var defaultRangeValidator = $.validator.methods.range;
        $.validator.methods.range = function (value, element, param) {
            if (element.type === "checkbox") {
                // if it's a checkbox return true if it is checked
                return element.checked;
            } else {
                // otherwise run the default validation function
                return defaultRangeValidator();
            }
        }
        var username = 'input[name="Name"]';
        $register.off(blur, username);
        $register.on(blur, username, function () {
            if ($(this).val().length > 0) {
                var userName = $(this).val();
                $(".register .terms-user-name").each(function () { $(this).text(userName); });
            }
        });

        var orgName = 'input[name="OrganizationName"]';
        $register.off(blur, orgName);
        $register.on(blur, orgName, function () {
            if ($(this).val().length > 0) {
                var orgName = $(this).val();
                $(".register .terms-org-name").each(function () { $(this).text(orgName); });
            }
        });
    };

    self.bindUIEvents();
    return self;
});