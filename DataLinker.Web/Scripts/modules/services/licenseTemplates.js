define(["jquery", "dialog","toastr","error-modal"], function($, dialog, toastr,error) {

    "use strict";
    var self = {};
    var $ui = $("#licenseTemplatesUI");
    var click = "click";
    self.bindUIEvents = function() {

        var uploadBtn = ".uploadButton";
        $ui.off(click, uploadBtn);
        $ui.on(click, uploadBtn, function(event) {
            self.uploadFile(event);
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

    self.publish = function(url, btn) {
        var title = "Publish License Template",
            message = "\n\nThis will make license template available for providers and consumers. Do you want to proceed?";
        var warningMsg = "<b>WARNING</b>: DataLinker do not allow publication of more than one license template. Existing active template will retracted.";
        if (btn.data("has-published-template") === "True") {
            message = warningMsg + message;
        }

        dialog.init(title,message,callback);

        function callback() {
            var successMsg = "License template was successfully published.";
            var errorMsg = "Unable to publish license template";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function(response) {
                    toastr.success(successMsg);
                    window.location.href = response.Url;
                },
                error: function (jqXHR, textStatus) {
                    error.init({ body: errorMsg});
                }
            });
        }
    };

    self.retract = function(url, btn) {
        var title = "Retract License Template",
            message = "This will retract license template, so it will not be available for using anymore. Do you want to proceed?";

        dialog.init(title,message,callback);

        function callback() {
            var successMsg = "License template was retracted.";
            var errorMsg = "Unable to retract license template";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function(response) {
                    toastr.info(successMsg);
                    window.location.href = response.Url;
                },
                error: function (jqXHR, textStatus) {
                    error.init({ body: errorMsg});
                }
            });
        }
    };

    self.bindUIEvents();
    return self;
});