define(["jquery", "dialog", "toastr", "error-modal"], function($, dialog, toastr, error) {

    "use strict";
    var self = {};
    var $ui = $("#schema-providers-ui");
    var click = "click";

    self.bindUIEvents = function() {
        var requestAccessBtn = ".request-access";
        $ui.off(click, requestAccessBtn);
        $ui.on(click, requestAccessBtn, function (event) {

            var btn = $(this),
                url = btn.data("url"),
                successUrl = btn.data("success-url"),
                providerName = btn.data("provider-name"),
                schemaName = btn.data("schema-name");
            self.requestAccess(url, btn, schemaName, providerName, successUrl);
        });
    };
    
    self.requestAccess = function(url, btn, schemaName, providerName, successUrl) {
        var title = "Confirm request access",
            message = "Clicking Yes will send an email to the Legal Officer of your organization to approve your request. Are you sure you want to request access to " + schemaName + " from " + providerName + "?",
            successStatus = '<span style="color: orange; font-weight: 500">Pending consumer legal approval</span>';

        dialog.init(title, message, callback);

        function callback() {
            var errorMsg = "Unable to request access";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    toastr.success(response.message);
                    //btn.parent().html(successStatus);
                    location.href = successUrl;
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