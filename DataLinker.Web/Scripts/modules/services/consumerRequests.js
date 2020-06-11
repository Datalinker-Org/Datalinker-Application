define(["jquery","dialog","toastr","error-modal"], function($, dialog,toastr,error) {

    "use strict";
    var self = {};
    var $ui = $("#consumer-requests-ui");
    var click = "click";
    self.bindUIEvents = function () {

        var approveBtn = "#approve-consumer-request";
        $ui.off(click, approveBtn);
        $ui.on(click, approveBtn, function(event) {
            var btn = $(this),
                id = btn.data("id"),
                url = btn.data("url");
            self.approveConsumerRequest(id, url, btn);
        });

        var declineBtn = "#decline-consumer-request";
        $ui.off(click, declineBtn);
        $ui.on(click, declineBtn, function(event) {
            var btn = $(this),
                id = btn.data("id"),
                url = btn.data("url");
            self.declineConsumerRequest(id, url, btn);
        });
    };

    self.declineConsumerRequest = function(requestId, url) {

        var title = "Decline consumer registration",
            message = "Are you sure you want to decline this registration?";

        dialog.init(title, message, callback);

        function callback() {
            var errorMsg = "Unable to decline request.";
            var successMsg = "Request was declined.";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: requestId },
                success: function() {
                    toastr.info(successMsg);
                    window.location.reload();
                },
                error: function () {
                    error.init({ body: errorMsg});
                }
            });
        }
    };

    self.approveConsumerRequest = function(requestId, url) {

        var title = "Approve consumer registration",
            message = "Are you sure you want to approve this registration?";

        dialog.init(title, message, callback);

        function callback() {
            var successMsg = "Request was successfully approved.";
            var errorMsg = "Unable approve request.";
            $.ajax({
                type: "POST",
                url: url,
                content: "application/json; charset=utf-8",
                dataType: "text",
                data: { id: requestId },
                success: function () {
                    toastr.success(successMsg);
                    location.reload();
                },
                error: function () {
                    error.init({ body: errorMsg});
                }
            });
        }
    };

    self.bindUIEvents();
    return self;
});