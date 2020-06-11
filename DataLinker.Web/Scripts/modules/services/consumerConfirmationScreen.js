define(["jquery"], function ($) {

	"use strict";
	var self = {};
	var $ui = $("#ConsumerConfirmationScreenUI");
	var change = "change";
	var acceptTnC,
		authorisedBy,
		agreeOnBehalf,
		buttons = $("#control-panel");

	self.bindUIEvents = function () {

		var acceptTcCheckBox = "#acceptTnC";
		$ui.off(change, acceptTcCheckBox);
		$ui.on(change, acceptTcCheckBox, function (event) {
			var checkbox = $(this);
			self.setAcceptTnC(event, checkbox);
		});

		var authorizedByCheckBox = "#authorisedBy";
		$ui.off(change, authorizedByCheckBox);
		$ui.on(change, authorizedByCheckBox, function (event) {
			var checkbox = $(this);
			self.setAuthorisedBy(event, checkbox);
		});

		var agreeOnBehalfCheckBox = "#agreeOnBehalf";
		$ui.off(change, agreeOnBehalfCheckBox);
		$ui.on(change, agreeOnBehalfCheckBox, function (event) {
			var checkbox = $(this);
			self.setAgreeOnBehalf(event, checkbox);
		});
	};

	self.setAcceptTnC = function (event, checkbox) {
		acceptTnC = checkbox.is(":checked");
		self.showAcceptDeclineBtns();
	};
	self.setAuthorisedBy = function (event, checkbox) {
		authorisedBy = checkbox.is(":checked");
		self.showAcceptDeclineBtns();
	};
	self.setAgreeOnBehalf = function (event, checkbox) {
		agreeOnBehalf = checkbox.is(":checked");
		self.showAcceptDeclineBtns();
	};

	self.showAcceptDeclineBtns = function () {
		if (acceptTnC && authorisedBy && agreeOnBehalf) {
			buttons.removeClass("hidden");
		} else {
			buttons.addClass("hidden");
		}
	};

	self.bindUIEvents();
	return self;
});