define(["jquery","dialog","toastr","error-modal"], function($,dialog,toastr,error) {

    "use strict";
    var self = {};
    var $ui = $("#LicenseClausesUI");
    var uploadClauseDialog = $("#upload-new-clause-dialog");
    var modalTitle = $("#section-title");
    var click = "click";
    self.bindUIEvents = function() {

        var uploadBtn = ".upload-new-clauses";
        $ui.off(click, uploadBtn);
        $ui.on(click, uploadBtn, function(event) {
            var $this = $(this);
            self.openModal(event, $this, $this.data("url"));
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

    self.openModal = function(event, link, url) {
        var sectionId = link.data("section-id");
        var sectionTitle = link.data("section-name");

        var uploadBtn = "#uploadClause";
        $(uploadBtn).off(click);
        $(uploadBtn).on(click, function(event) {
            self.upload(url);
        });

        modalTitle.text(sectionTitle);
        $("#section-id").text(sectionId);

        uploadClauseDialog.modal({
            toogle: "modal",
            backdrop: "static",
            keyboard: false
        });
    };

    self.upload = function(url) {
        var actionParameterName = "sectionId";
        var errorMsg = "This browser doesn't support HTML5 file uploads!";
        var files = $("#uploadFile")[0].files;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append("file" + x, files[x]);
                }
                data.append(actionParameterName, $("#section-id").text());
                var successMsg = "Clause was successfully uploaded";
                errorMsg = "Unable to upload this file";
                $.ajax({
                    type: "POST",
                    url: url,
                    data: data,
                    processData: false,
                    contentType: false,
                    dataType: "json",
                    success: function(response) {
                        toastr.success(successMsg);
                        uploadClauseDialog.modal("hide");
                        window.location.href = response.Url;
                    },
                    error: function () {
                        error.init({ body: errorMsg});
                        uploadClauseDialog.modal("hide");
                    }
                });
            } else {
                error.init({ body: errorMsg});
            }
        }
    };

    self.publish = function(url, btn) {
        var title = "Publish Clause Template",
            message = "This will make license clause template available for creating new licenses. Do you want to proceed?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "License clause template was successfully published.";
            var errorMsg = "Unable to publish license clause template";

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
                    error.init({ body:  errorMsg});
                }
            });
        }
    };

    self.retract = function(url, btn) {
        var title = "Retract Clause Template",
            message = "This will retract clause template, so it will not be available for using anymore. Do you want to proceed?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "License clause template was retracted.";
            var errorMsg = "Unable to retract license clause template";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
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