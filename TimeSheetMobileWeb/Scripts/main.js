
/*
*  @author : Srikrishna Gumma 
*  NameSpace : TSM 
*/

var TSM = TSM || {};

TSM = (function ($) {

    "use strict";
    var modalOverlay = '#modalOveray'
    , modalTarget;

    function Call(fun) {
        //assuming it's a function
        if (arguments.length == 1)
            fun.call(this, []);
    }
    function enableTouchGuestures(event) {

    }
    function initModalOverlay(event) {
        $('*[data-role]').removeAttr('click');
        $('*[data-role]').on('click', function (argument) {
            var $role = $(this).data('role');
            modalTarget = $(this).data('target');
            var postscript = function (arg1, arg2) {
                if (arg1 != undefined) {
                    var method = eval('(' + arg1 + ')');
                    method(arg2);
                }
            }

            if ($role === 'modal') {
                $(modalOverlay)
               .fadeIn(400, function () {
                   $(modalTarget)
                   .fadeIn('fast');
               });
                postscript($(this).attr('data-callback'), $(this).attr('data-row-from'));
            } else if ($role === 'dismiss') {
                $(modalTarget)
                .fadeOut(400, function () {
                    $(modalOverlay)
                    .fadeOut('fast');
                });
                postscript($(this).attr('data-callback'), $(this).attr('data-row-from'));
            }

        });
        

    }

    return {
        init: function () {

            /* Act on the event */
            initModalOverlay();
            enableTouchGuestures();
        }
    };

    // Pull in jQuery and Underscore
})(jQuery);

TSM.init();

function OpenDialog(modalTarget) {
    $('#modalOveray')
               .fadeIn(400, function () {
                   $(modalTarget)
                   .fadeIn('fast');
               });
}
function CloseDialog(modalTarget) {
    $(modalTarget)
                .fadeOut(400, function () {
                    $('#modalOveray')
                    .fadeOut('fast');
                });
}
function callMethod(method) {
    method();
}