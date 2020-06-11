define(["jquery"], function ($) {

    "use strict";
    var self = {};
    var $ui = $("#ConfirmationScreenUI");
    var change = "change";
    var acceptTnC,
        authorisedBy,
        agreeOnBehalf,
        buttons = $("#control-panel");

    self.bindUIEvents = function() {

        var acceptTcCheckBox = "#acceptTnC";
        $ui.off(change, acceptTcCheckBox);
        $ui.on(change, acceptTcCheckBox, function(event) {
            var checkbox = $(this);
            self.setAcceptTnC(event, checkbox);
        });

        var authorizedByCheckBox = "#authorisedBy";
        $ui.off(change, authorizedByCheckBox);
        $ui.on(change, authorizedByCheckBox, function(event) {
            var checkbox = $(this);
            self.setAuthorisedBy(event, checkbox);
        });

        var agreeOnBehalfCheckBox = "#agreeOnBehalf";
        $ui.off(change, agreeOnBehalfCheckBox);
        $ui.on(change, agreeOnBehalfCheckBox, function(event) {
            var checkbox = $(this);
            self.setAgreeOnBehalf(event, checkbox);
        });

        var declineReasonBtn = ".decline-reason";
        $ui.off("click", declineReasonBtn);
        $ui.on("click", declineReasonBtn, function (event) {
            debugger;
            var $this = $(this);
            self.showDeclineReasonDialog(event, $this.data("url"));
        });
    };

    self.setAcceptTnC = function(event, checkbox) {
        acceptTnC = checkbox.is(":checked");
        self.showAcceptDeclineBtns();
    };
    self.setAuthorisedBy = function(event, checkbox) {
        authorisedBy = checkbox.is(":checked");
        self.showAcceptDeclineBtns();
    };
    self.setAgreeOnBehalf = function(event, checkbox) {
        agreeOnBehalf = checkbox.is(":checked");
        self.showAcceptDeclineBtns();
    };

    self.showAcceptDeclineBtns = function() {
        if (acceptTnC && authorisedBy && agreeOnBehalf) {
            buttons.removeClass("hidden");
        } else {
            buttons.addClass("hidden");
        }
    };

    self.showDeclineReasonDialog = function (event, url) {
        debugger;
        var $modalLocation = $(".decline-reason-location");
        $.ajax({
            url: url,
            success: function (r) {
                debugger;
                $modalLocation.html(r);
                //$.validator.unobtrusive.parse(".create-new-application-modal-location");
                $("#decline-reason-modal").modal("toggle");
            },
            error: function (r) {
                $modalLocation.html(r);
                //$modalLocation.html(errorMsg);
                //if (r.responseJSON != undefined) {
                //    error.init({ body: r.responseJSON.msg });
                //} else {
                //    error.init({ body: errorMsg });
                //}
            }
        });
    };

    self.bindUIEvents();
    return self;
});