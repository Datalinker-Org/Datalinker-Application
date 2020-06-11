define(["bootstrap"], function (btstrp) {

    "use strict";
    var self = {};
    var $modal = $("#rezare-dialog");
    var toggle = "toggle";

    self.bindUIEvents = function (yesCallback, noCallback) {

        var yesBtn = ".rezare-dialog-yes-btn";
        var noBtn = ".rezare-dialog-no-btn";
        var click = "click";

        $modal.off(click, yesBtn);
        $modal.on(click, yesBtn, function () {
            if (yesCallback != undefined) {
                yesCallback();
            }

            $modal.modal(toggle);
        });

        $modal.off(click, noBtn);
        $modal.on(click, noBtn, function () {
            if (noCallback != undefined) {
                noCallback();
            }

            $modal.modal(toggle);
        });
    };

    self.init = function (title, message, yesCallback, noCallback) {

        $("#rezare-dialog-title").html(title);
        $("#rezare-dialog-body").html(message);

        self.bindUIEvents(yesCallback, noCallback);
        $modal.modal(toggle);
        return $modal;
    };

    return self;
});