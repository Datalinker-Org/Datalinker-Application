define(["jquery", "dialog", "toastr", "error-modal"], function($, dialog, toastr, error) {

    "use strict";
    var dataSchemaId = "DataSchemaID",
        self = {},
        $ui = $("#SchemaUI"),
        schemaModel = {
            Id: dataSchemaId,
            Name: "Name",
            Description: "Description",
            Version: "Version",
            Status: "Status",
            PublishedAt: "PublishedAt"
        };
    var click = "click";

    self.bindUIEvents = function() {

        // Publish
        var publishBtn = ".publish";
        $ui.off(click, publishBtn);
        $ui.on(click, publishBtn, function(event) {
            var btn = $(this);
            self.publishSchema(event, btn.data("url"), btn);
        });

        // Retract
        var retractBtn = ".retract";
        $ui.off(click, retractBtn);
        $ui.on(click, retractBtn, function(event) {
            var btn = $(this);
            self.retractSchema(event, btn.data("url"), btn);
        });
    };

    self.publishSchema = function(event, url, btn) {
        var id = {};
        id[schemaModel.Id] = btn.data("id");
        var title = "Publish schema",
            message = "Are you sure you want to publish this schema?";

        dialog.init(title, message, callback);

        function callback() {

            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                data: id,
                success: self.successUpdateStatus,
                error: self.errorUpdateStatus
            });
        }
    };

    self.retractSchema = function(event, url, btn) {
        var id = {};
        id[schemaModel.Id] = btn.data("id");
        var title = "Retract schema",
            message = "Are you sure you want to retract this schema?";

        dialog.init(title, message, callback);

        function callback() {
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "json",
                data: id,
                success: self.successUpdateStatus,
                error: self.errorUpdateStatus
            });
        }
    };

    self.successUpdateStatus = function(response) {

        window.location.href = response.Url;
    }

    self.errorUpdateStatus = function(xhr, textStatus, errorThrown) {
        error.init({ body: "Unable to update status for this schema" });
    }

    self.bindUIEvents();
    return self;
});