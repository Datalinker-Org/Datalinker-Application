define(["jquery", "dialog", "toastr", "error-modal"], function($, dialog, toastr, error) {

    "use strict";
    var self = {};
    var $ui = $("#licensesUI");
    var click = "click";

    self.bindUIEvents = function() {
        var sendForApprovalBtn = ".sendForLegalApproval";
        $ui.off(click, sendForApprovalBtn);
        $ui.on(click, sendForApprovalBtn, function(event) {

            var btn = $(this),
                isShowDialog = btn.data("show-dialog"),
                url = btn.data("url");
            self.sendForLegalApproval(isShowDialog, url, btn);
        });

        var publishBtn = ".publish";
        $ui.off(click, publishBtn);
        $ui.on(click, publishBtn, function(event) {

            var btn = $(this),
                url = btn.data("url");
            self.publish(url, btn);
        });

        var retractBtn = ".retract";
        $ui.off(click, retractBtn);
        $ui.on(click, retractBtn, function(event) {

            var btn = $(this),
                url = btn.data("url");
            self.retract(url, btn);
        });
    };

    self.sendForLegalApproval = function(isShowDialog, url, btn) {
        var title = "Send data agreement for Legal Approval",
            message = "This will send your license to Legal officer of your organization. Do you want to proceed?";
        if (isShowDialog === "True") {
            message = "You are sending a licence for approval. This will automatically make the exisitng 'Pending approval' option revert back to Draft status. Do you want to proceed?";

        }

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "License was sent to Legal Officer of your organization.";
            var errorMsg = "Unable to send for legal approval";
            $.ajax({
                type: "GET",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function(response) {
                    if (response.isSuccess === true) {
                        toastr.success(successMsg);
                        btn.addClass("disabled");
                        var $row = btn.closest("tr");
                        $row.find(".status").replaceWith('<label class="label status label-info">Pending Approval</label>');
                        $row.find(".to-draft").removeClass("disabled");
                    } else {

                        error.init({ body: errorMsg });
                    }
                },
                error: function(jqXHR, textStatus) {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.publish = function(url, btn) {
        var title = "Publish data agreement",
            message = "Publishing this schema will enable it to be available for consumers to read your data agreement and your organization will be notified if they choose to access your schema and execute a license agreement with you. Are you ready to publish this schema?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "License was successfully published.";
            var errorMsg = "Unable to publish data agreement";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function(response) {
                    toastr.success(successMsg);
                    window.location.href = response.Url;
                },
                error: function(jqXHR, textStatus) {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.retract = function(url, btn) {
        var title = "Retract data agreement",
            message = "This will retract your data agreement permanently. Do you want to proceed?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "Data agreement was retracted.";
            var errorMsg = "Unable to retract data agreement";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function(response) {
                    toastr.info(successMsg);
                    window.location.href = response.Url;
                },
                error: function(jqXHR, textStatus) {
                    error.init({ body: errorMsg });
                }
            });
        }
    };

    self.bindUIEvents();
    return self;
});