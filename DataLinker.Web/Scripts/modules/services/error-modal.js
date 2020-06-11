define(["bootstrap"], function (btstrp) {

    "use strict";
    var self = {};

    self.init = function (args) {

        var $modal = $("#error-modal");
        if (args.title !== undefined) {
            $("#error-title").html(args.title);
        }
        $("#error-body").html(args.body);
        
        $modal.modal("toggle");
        return $modal;
    };

    return self;
});