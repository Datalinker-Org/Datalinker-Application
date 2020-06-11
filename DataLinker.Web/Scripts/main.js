require.config({
    baseUrl: "/Scripts/modules/services",
    paths: {
        jquery: "../../libs/jquery-2.1.4.min",
        jqueryValidate: "../../libs/jquery.validate.min",
        jqueryValidateUnobtrusive: "../../libs/jquery.validate.unobtrusive.min",
        bootstrap: "../../libs/bootstrap.min",
        tokenfield: "../../libs/bootstrap-tokenfield",
        bootstrapWizard: "../../libs/jquery.bootstrap.wizard.min",
        rezBootstrapWizard: "../../libs/jquery.rez.bootstrap.wizard",
        domReady: "../../libs/domReady",
        spin: "../../libs/spin.min",
        copy: "../../libs/copyToClipboard",
        toggle: "../../libs/bootstrap-toggle.min",
        toastr: "../../libs/toastr.min"
    },
    shim: {
        bootstrap:["jquery"],
        spin:["jquery"],
        toggle:["bootstrap"],
        bootstrapWizard: {
            deps: ["bootstrap"],
            exports: "jQuery.fn.bootstrapWizard"
        },
        rezBootstrapWizard: {
            deps:["bootstrapWizard"],
            exports: "jQuery.fn.rezBootstrapWizard"
        },
        jqueryValidate: ["jquery"],
        jqueryValidateUnobtrusive: ["jquery", "jqueryValidate"]
    }
});

// On every page
require(["bootstrap", "spinner", "jqueryValidateUnobtrusive", "toastr", "error-modal"], function (btstrp, spinner, jqVal,toastr,error) {

    "use strict";
    // Edit profile details
    var $ui = $(".account-ui");
    var self = {};
    var editBtn = ".account-edit-btn";
    var click = "click";
    $ui.off(click, editBtn);
    $ui.on(click, editBtn, function (event) {

        event.preventDefault();
        var url = $(this).data("url");
        var $location = $(".edit-user-details-modal-location");
        self.edit($location,url);
    });

    self.bindResendEmailUI = function($ui) {
        var resendConfirmationBtn = ".resend-email-confirmation";
        $ui.off(click, resendConfirmationBtn);
        $ui.on(click, resendConfirmationBtn, function(event) {
            event.preventDefault();
            var $this = $(this);
            self.resendConfirmationEmail($this);
        });
    };

    self.resendConfirmationEmail = function ($this) {
        var errorMsg = "Unable to process your request";
        var successMsg = "Confirmation link was successfully sent";
        spinner.on();
        $.ajax({
            url: $this.data("url"),
            success: function () {
                spinner.off();
                $(".confirmation-location").addClass("disabled");
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
    };

    self.edit = function($location, url) {
        spinner.on();
        $location.load(url, function (response, status, xhr) {
            spinner.off();
            self.processLoadCallback(openEditModal, response, status, xhr);
        });

        function openEditModal() {
            self.bindResendEmailUI($location);
            self.saveProfileDetails($location, url);
            $("#edit-account-modal").modal("toggle");
        }
    };

    self.saveProfileDetails = function($location, url) {

        var $saveChangesBtn = $(".save-profile-details-btn");
        $saveChangesBtn.off(click);
        $saveChangesBtn.on(click, function () {
            var form = "#account-edit-form";
            var $form = $(form);
            $.validator.unobtrusive.parse(form);
            $form.validate();
            if ($form.valid()) {
                self.saveChanges($form,url);
            }
        });
    };
    self.saveChanges=function($form,url) {

        var succeMsg = "User details has been successfully updated.";
        var errorMsg = "Unable to update user details";
        spinner.on();
        $.ajax({
            type: "POST",
            url: url,
            data: $form.serializeArray(),
            content: "application/json; charset=utf-8",
            success: function () {
                spinner.off();
                toastr.success(succeMsg);
                self.refreshUsersTable();
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

        $("#edit-account-modal").modal("toggle");
    }

    self.refreshUsersTable = function() {
        var $usersTable = $(".users-table");
        if ($usersTable.length > 0) {

            var url = $usersTable.data("refresh-url");
            spinner.on();
            $usersTable.load(url, function (response, status, xhr) {
                spinner.off();
                self.processLoadCallback(undefined, response, status, xhr);
            });
        }
    }

    self.processLoadCallback = function (success, response, status, xhr) {
        if (status === "error") {
            var errorDetails = ' Log In required. Click <a href="' + window.location.href + '"> to continue.</a>';
            var errorMsg = "Error: unable to load page.(code: " + xhr.status + ")";
            if (xhr.status === 0 || xhr.status === 200) {
                errorMsg += errorDetails;
            }

            error.init({ body: errorMsg });
        } else {
            if (success !== undefined) {
                success();
            }
        }
    };
});