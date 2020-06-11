/*
* Customised initialiser for jQuery twitter bootstrap wizard plugin
*/

;
(function ($) {
    $.fn.rezBootstrapWizard = function (options) {

        var wizard = this;
        // Default settings and handlers for next / previous and last
        // tab display
        var settings = $.extend({

            'tabClass': 'nav nav-pills',
            'nextSelector': '.button-next',
            'previousSelector': '.button-previous',

            'onTabShow': function (tab, navigation, index) {
                var lastPage = navigation.find('li').length <= (index + 1);
                if (lastPage) {
                    wizard.find('.button-next').addClass('hidden');
                    wizard.find('.button-finish').removeClass('hidden');
                } else {
                    wizard.find('.button-finish').addClass('hidden');
                    wizard.find('.button-next').removeClass('hidden');
                }
            },

            'onNext': function (tab, navigation, index) {

                var form = wizard.closest("FORM")[0];
                if (form !== undefined && form !== null) {
                    if (!$("#" + form.id).valid()) {
                        return false;
                    }
                }

                // New user-organization screen: accept T&C
                $('#terms-user-name').text($('#Name').val());

                var nextTab = navigation.find('li:nth-child(' + (index + 1) + ')');
                if (nextTab.hasClass('disabled') || nextTab.is(':hidden')) {
                    for (var i = 2; navigation.find('li').length >= (index + i) ; i++) {
                        nextTab = navigation.find('li:nth-child(' + (index + i) + ')');
                        if (!nextTab.hasClass('disabled') && !nextTab.is(':hidden')) {
                            break;
                        }
                    }
                    if (nextTab.length > 0) {
                        nextTab.find('a:first').trigger('click');
                    }

                    // We either clicked a tab, or all the tabs after to the one we're
                    // on are disabled.
                    return false;
                }
            },

            'onPrevious': function (tab, navigation, index) {
                var previousTab = navigation.find('li:nth-child(' + (index + 1) + ')');
                if (previousTab.hasClass('disabled') || previousTab.is(':hidden')) {
                    for (var i = 0; (index - i) >= 1 ; i++) {
                        previousTab = navigation.find('li:nth-child(' + (index - i) + ')');
                        if (!previousTab.hasClass('disabled') || !previousTab.is(':hidden')) {
                            break;
                        }
                    }
                    if (previousTab.length > 0) {
                        previousTab.find('a:first').trigger('click');
                    }

                    // We either clicked a tab, or all the tabs prior to the one we're
                    // on are disabled.
                    return false;
                }
            }
        }, options);

        return this.bootstrapWizard(settings);
    };

})(jQuery);