
(function () {
    var cookiename = "_configuration_";
    var userField = "_user_";
    var taskIdField = "_task_";
    var TimesheetIdField = "_row_";
    var cookieUpdatedField = "_updated_";
    window['configuration'] = {
        show: function () {
            var cookie = getCookie(cookiename, true);
            alert("task view: " + cookie[taskIdField] +
                  " row view: " + cookie[TimesheetIdField] +
                  " user: " + cookie[userField] +
                  " updated: " + cookie[cookieUpdatedField]);
        },
        set: function (row, task) {
            var cookie = getCookie(cookiename, true);
            cookie[taskIdField] = task || '';
            cookie[TimesheetIdField] = row || '';
            cookie[cookieUpdatedField] = 'true';
            setCookie(cookiename, cookie, true, 365);
        },
        get: function () {
            var cookie = getCookie(cookiename, true);
            return {
                task: cookie[taskIdField] || '',
                row: cookie[TimesheetIdField] || ''
            };
        }

    };


})();

function TSM_ChangePage(page, options, origin) {
    var data = { cancel: false, action: origin };
    $('#' + page).trigger('gotoing', data);
    if (!data.cancel) $.mobile.changePage('#' + page, options);
    $('#' + page).trigger('goto', data);
}
function TSM_Return(target, options, event, origin) {
    var data = { cancel: false, action: origin };
    $('#' + target).trigger(event + "ing", data);
    if (!data.cancel) history.back();
    $('#' + target).trigger(event, data);
}
function TSM_LeaveTo(target, application, options) {
    var data = { cancel: false };
    $('#' + application).trigger('leaveApplication', data);
    if (data.cancel) return;
    window.location.href = target;
}
function TSM_partialSubmit(jForm) {
    var target = $('#' + jForm.attr('data-ajax-target'));
    $.mobile.showPageLoadingMsg();
    $.post(jForm.attr('data-ajax-url'), jForm.serialize(), function (data) {
        target.html(data);
        $.validator.unobtrusive.reParse(target);
        TSM_initButtons(TSM_PageChangeOptions);
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete();

        }
        $.mobile.hidePageLoadingMsg();
    });
}
function TSM_jsonSubmit(jForm) {
    $.mobile.showPageLoadingMsg();
    $.post(jForm.attr('data-ajax-url'), jForm.serialize(), function (data) {
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete($.parseJSON(data));
            $.mobile.hidePageLoadingMsg();
        }
    },
    "text");
}
function TSM_formDirty(x, y) {
    y = y || 'mainform';
    var cantEdit = ($('.currtotalact').length != 0) && ($('.candelete').val() != 'True');
    var form = $('#' + y);
    form.data('_dirty_', x);
    form.trigger('dirtychange', x);
    var recallDeleteDisabled = ($('.currentperiodid').val() || '') == '';
    //var empty=$('.innerRowsContainer').children(':not(span)').length == 0;
    var empty = false;
    $('.csubmit-' + y).prop('disabled', (!x) || empty || cantEdit);
    $('.fsubmit-' + y).prop('disabled', (!x) || empty || cantEdit);
    $('#btnRecall').prop('disabled', recallDeleteDisabled || $('.canrecall').val() != 'True');
    $('#btnDelete').prop('disabled', recallDeleteDisabled || $('.candelete').val() != 'True');
    $('#btnRecallDelete').prop('disabled', recallDeleteDisabled);
    $('#currstatusdisplay').text($('.currstatus').val());
}
function TSM_formDelete(y) {
    y = y || 'mainform';
    var empty = $('.innerRowsContainer').children().length == 0;
    if (empty) $('.csubmit-' + y).prop('disabled', true);
    $('.fsubmit-' + y).prop('disabled', empty);

}
function TSM_isFormDirty(y) {
    y = y || 'mainform';
    return $('#' + y).data('_dirty_') || false;
}
function TSM_prepareDatePicker(jField) {
    var min = null;
    var max = null;

    var dynamicrange_max = jField.attr('data-val-daterange-max') || '';
    if (dynamicrange_max != '') max = new Date(parseInt(dynamicrange_max));
    var dynamicrange_min = jField.attr('data-val-daterange-min') || '';
    if (dynamicrange_min != '') min = new Date(parseInt(dynamicrange_min));

    var cmax = jField.attr('data-val-clientdynamicdaterange-max') || '';
    var cmin = jField.attr('data-val-clientdynamicdaterange-min') || '';
    var mindelay = jField.attr('data-val-clientdynamicdaterange-mindelay') || '';
    var maxdelay = jField.attr('data-val-clientdynamicdaterange-maxdelay') || '';
    if (cmax != '') {
        cmax = $.validator.takeDynamicValue(jField[0], cmax);
        if (cmax != null) {
            cmax = jQuery.global.parseDate(cmax);
            if (!isNaN(cmax)) {
                if (maxdelay != '') cmax = new Date(cmax.getTime() + parseInt(maxdelay));
                if (max != null) {
                    if (cmax < max) max = cmax;
                }
                else {
                    max = cmax;
                }
            }
        }
    }
    if (cmin != '') {
        cmin = $.validator.takeDynamicValue(jField[0], cmin);
        if (cmin != null) {
            cmin = jQuery.global.parseDate(cmin);
            if (!isNaN(cmin)) {
                if (mindelay != '') cmin = new Date(cmin.getTime() + parseInt(mindelay));
                if (min != null) {
                    if (cmin > min) min = cmin;
                }
                else {
                    min = cmin;
                }
            }
        }
    }
    var val = jQuery.global.parseDate(jField.val());
    var sel = null;
    if (val == null || isNaN(val)) {
        val = null;
        sel = new Date();
        if (min != null && sel < min) sel = min;
        if (max != null && sel > max) sel = max;
    }
    else {
        if (min != null && val < min) val = min;
        if (max != null && val > max) val = max;
        sel = val;
    }
    var picker = $('.datapickerwidget');
    picker.datepicker("option", {
        minDate: min,
        maxDate: max,
        defaultDate: sel,
        gotoCurrent: true
    });
    picker.datepicker("setDate", sel);

}

function TSM_initButtons(options) {
    function TSM_GoToHandler(e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        if (($(this).attr('data-rel') || '') == 'dialog') {
            options = $.extend({}, options);
            options['changeHash'] = false;
        }
        else {
            options = $.extend({}, options);
            options['changeHash'] = true;
        }
        if ($(this).hasClass('ui-disabled')) return;
        TSM_ChangePage($(this).attr('data-button-target'), options, e.target);
    }

    if ($('.datapickerwidget').length > 0) {
        $('.datapickerwidget').datepicker({ onSelect: function (dateText, inst) {
            $('#dateinput').trigger('dateselected', dateText);
            history.back();

        }
        });
        $("a.datepicker").click(function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            $('#dateinput').data('_returnNode_', this);
            var name = $(this).attr('id')
            name = name.substring(0, name.lastIndexOf("_"));
            var target = $('#' + name);
            TSM_prepareDatePicker(target);
            $.mobile.changePage('#dateinput', options);
        });
    }
    $(".submit").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            jThis = $(this);
            var conf = jThis.attr('data-action-confirm');
            if (conf && !window.confirm(conf)) return;
            var target = $('#' + jThis.attr('data-action-container'));
            target.val(jThis.attr('data-action-value'));
            var fs = target.parents('form').first();
            if (fs.validate().form()) TSM_partialSubmit(fs);
        }
    );

    $(".goto").click(
        TSM_GoToHandler
    );
    $(".confirm").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            TSM_Return($(this).attr('data-button-target'), options, 'confirm', e.target);
        }
    );
    $(".loadConfirm").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            $('#' + $(this).attr('data-button-target')).trigger('confirm', { cancel: false, action: e.target });
        }
    );
    $(".cancel").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            TSM_Return($(this).attr('data-button-target'), options, 'cancel');
        }
    );
    $(".leaveApplication").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            TSM_LeaveTo($(this).attr('data-button-target'), $(this).attr('data-button-application'));
        }
    );
    MvcControlsToolkit_ParseRegister.add(
     function (selector) {
         $(selector).find('.goto').click(
            TSM_GoToHandler
         );

     },

 false)
}

var TSM_PageChangeOptions = null;
$(document).ready(function () {
    TSM_initButtons(TSM_PageChangeOptions);
    $('#dateinput').bind('dateselected', function (e, data) {
        var link = $($(this).data('_returnNode_'));
        link.text(data);
        var name = link.attr('id')
        name = name.substring(0, name.lastIndexOf("_"));
        var target = $('#' + name);
        target.val(data);
    });

    $('.updatetasks').change(function () {
        var jThis = $(this);
        if (jThis.val() == "-1") {
            $('.normalRows').hide();
            $('.adminRows').show();

        }
        else {
            $('.adminRows').hide();
            $('.normalRows').show();
        }

        $('.currentprojname').val(jThis.find('option:selected').text());
        var data = $('.currentprojname').val();
        var selectedid = this.value


        MvcControlsToolkit_UpdateDropDownOptions(
                jThis.attr('data-action') + '?projectId=' + selectedid,
                $('.dynamictasks'),
                jThis.attr('data-prompt'), null, null, TSM_TasksOptionsCallback);




    });

    $('.dynamictasks').change(function () {
        $('.currentassname').val($(this).find('option:selected').text());
        TSM_ConfirmAdd(true);
    });
    //    $('.dynamicperiods').change(function (e) {
    //        TSM_NewPeriod();
    //    });
    $('.updatetimesheets').change(function () {

        var jForm = $('#periodform');
        var target = $('#' + jForm.attr('data-ajax-target'));
        $.mobile.showPageLoadingMsg();
        var jForm = $('#periodform');
        $.post(jForm.attr('data-ajax-url') + '?speriod=' + this.value, jForm.serialize(), function (data) {
            target.html(data);
            $.validator.unobtrusive.reParse(target);
            TSM_initButtons(TSM_PageChangeOptions);
            var onComplete = jForm.attr('data-ajax-oncomplete') || null;
            if (onComplete != null) {
                onComplete = eval(onComplete);
                onComplete();

            }
            $.mobile.hidePageLoadingMsg();
        });
    });
    $('.updaterowview,  .updatetaskview').change(function () {
        TSM_View();
    });
    TSM_ConfirmPeriod(true, null);
    TSM_View();
    TSM_UpdateMainLayout();
    $('#basedetail').bind('goto', function (e, data) {
        var row_type = $(data.action).attr('data-button-selection');
        row_choice_ViewList.Select("property_group_" + row_type);
        $('#rowsTitle').text(TSM_DetailTitle(row_type));
        var prefix = $(data.action).attr('data-row-from');
        $('#basedetail').data('chosen-row', prefix);
        TSM_CopyFromRow(prefix);
    })

    $('#basedetail').bind('goto', function (e, data) {
        var row_type = $(data.action).attr('data-button-selection');
        row_choice_ViewList.Select("property_group_" + row_type);
        $('#rowsTitle').text(TSM_DetailTitle(row_type));
        var prefix = $(data.action).attr('data-row-from');
        $('#basedetail').data('chosen-row', prefix);
        TSM_CopyFromRow(prefix);
    })




    .bind('confirming', function (e, data) {
        if (!$('#detailform').validate().form()) {
            data.cancel = true;
            return;
        }
        TSM_ensurePositive();
        TSM_CopyToRow($('#basedetail').data('chosen-row'));
        TSM_UpdateMainLayout();
        TSM_formDirty(true);

    }
    )
    .bind('cancel', function () {

    }
    );
    $('.allrowsContainer').bind('itemChange', function (e, data) {
        if (data.ChangeType == 'ItemCreated') {
            TSM_UpdateMainLayout();
            TSM_formDirty(true);
        }
        else if (data.ChangeType == 'ItemDeleted') {
            TSM_UpdateMainLayout();
            TSM_formDirty(true);
        }
    });
});
$(document).live('pageinit', function (e) {
    if (e.target.id == 'taskselection') {
        TSM_TasksOptionsCallback($('.dynamictasks'));
        TSM_ConfirmAdd();

    }
    else if (e.target.id == 'periodselection') {
        TSM_PeriodsOptionsCallback($('.updatetimesheets'));
    }

})
function TSM_TasksOptionsCallback(jTarget) {
    if (jTarget.length > 0) {
        if (jTarget[0].options.length > 1) jTarget.selectmenu("enable");
        else jTarget.selectmenu("disable");
        jTarget.selectmenu("refresh");
        TSM_ConfirmAdd();
    }
    else {
        jTarget.selectmenu("disable");
        TSM_ConfirmAdd();
        if ($('#RequiredProgectId option:selected').text() != 'Choose project') {
            $('.rowlist').each(function () {
                var jThis = $(this);
                jThis.button('enable');
            });
            $('#' + $('#RequiredProgectId option:selected').text() + '_display').val('Top Level');

        }

    }
}

function TSM_ConfirmAdd(show) {
    var cGuid = $('.dynamictasks').val();
    var projectId = $('#RequiredProgectId').val();
    if (cGuid == '') {
        $('.rowlist').each(function () { $(this).button('disable'); });
        return;
    }
    $('.rowlist').each(function () {
        var jThis = $(this);
        var rowId = "p-" + cGuid + "-" + jThis.attr('data-button-selection');
        var assnid = "p-" + "Top Level" + projectId + "-" + jThis.attr('data-button-selection');
        if (((cGuid || '') != '') && ($('.currentperiodid').val() != '')) {

            if ($('.dynamictasks option:selected').text() == 'Top Level') {
                if ($("." + assnid).length == 0) jThis.button('enable');
                else jThis.button('disable');
            }
            else {
                if ($("." + rowId).length == 0) {
                    jThis.button('enable');
                }
                else {
                    jThis.button('enable');
                }

            }


        }
        else jThis.button('disable');
    });
}
var TSM_CurrRowType = null;
function TSM_PrepareRowType(button) {
    TSM_CurrRowType = $(button).attr('data-button-selection') || null;
    $('.currentrowtype').val(TSM_CurrRowType);
}
function TSM_PeriodsOptionsCallback(jTarget) {
    ////    if (jTarget[0].options.length > 0) {
    jTarget.selectmenu("enable");
    $('#btnConfirmPeriod').prop('disabled', false);
    ////        
    ////    }
    ////    else {
    ////        jTarget.selectmenu("disable");
    ////        $('#btnConfirmPeriod').prop('disabled', true);
    ////    }
    jTarget.selectmenu("refresh");
    TSM_NewPeriod();

}

function TSM_NewPeriod() {
    var newVal = $('.updatetimesheets option:selected').text() || "";
    if (newVal == "") newVal = $('.updatetimesheets').attr('data-novalue-message');
    $('.newtimesheet').text(newVal);

}

function TSM_ConfirmPeriod(x, y) {
    var newVal = $('.updatetimesheets option:selected').text() || "";
    if (newVal == '') {
        newVal = $('#periodstart').val();
    }
    if (y != null) newVal = "";
    $('#btnAdd').prop('disabled', newVal == '');
    $('#btnRecallDelete').prop('disabled', true);
    var jSelect = $('.updatetimesheets');
    if (newVal == "") {
        $('#btnRecallDelete').prop('disabled', true);
        $('.currenttimesheet').text(jSelect.attr('data-novalue-message'));
        $('.currentperiodid').val('');
        $('.currentperiodstart').val('');
        $('.currentperiodstop').val('');
        $('.allTimesheetsEdit').empty();
        $('.currentperiodSet').val($('.updatetimesheets').val());
        TSM_PrepareDays('', '');
        TSM_formDirty(false);
        if (x == null) $('#allTimesheetsEdit').empty();
    }
    else {

        $('#btnRecallDelete').prop('disabled', false);
        var period = $('#indexPeriod').text();
        var periodid = $('#periodid').val();

        if (period != undefined) {
            var dates = period.replace('(', '').replace(')', '').split(' ');
            $('.currenttimesheet').text(period);
            $('.currentperiodid').val(periodid);
            $('.currentperiodstart').val(dates[0]);
            $('.currentperiodstop').val(dates[2]);
        }
        TSM_PrepareDays($('#periodstart').val(), $('#periodend').val());
        $('.currentperiodSet').val($('.updatetimesheets').val());
        if (x == null) TSM_partialSubmit($('#periodform'));

    }

}

function TSM_ConfirmPeriodCallBack(data) {
    var s = data;
    history.back();
    TSM_formDirty(false);
    TSM_UpdateMainLayout();
}

function TSM_UpdateMainLayout() {
    $('.updatemainlayout').trigger('updatelayout');



}
function TSM_View() {
    var current = configuration.get();
    var oldTask = '';
    var oldRow = '';
    if (current.task) oldTask = $('.updatetaskview option[value="' + current.task + '"]').text();
    if (current.row) oldRow = $('.updaterowview option[value="' + current.row + '"]').text();
    if (oldTask) $('.currenttaskview').text(oldTask);
    if (oldRow) $('.currentrowview').text(oldRow);
    if (window['_rowViewDescriptions_']) {
        $('.rowViewDescription-o').text(window['_rowViewDescriptions_'][current.row] || '');
        $('.rowViewDescription').text(window['_rowViewDescriptions_'][$('.updaterowview').val()] || '');
    }
    if (window['_taskViewDescriptions_']) {
        $('.taskViewDescription-o').text(window['_taskViewDescriptions_'][current.task] || '');
        $('.taskViewDescription').text(window['_taskViewDescriptions_'][$('.updatetaskview').val()] || '');
    }
}
function TSM_ConfirmView() {
    current = configuration.get();
    var rowC = $('.updaterowview');
    var taskC = $('.updatetaskview');
    if (rowC.length > 0) current.row = rowC.val();
    if (taskC.length > 0) current.task = taskC.val();
    configuration.set(current.row, current.task);
    if (($('.currentperiodid').val() || '') == '') {
        history.back();
        return;
    }
    $.mobile.showPageLoadingMsg();
    $('#periodform').submit();
}
function TSM_startAddRow(origin) {
    var currRowType = $(origin).attr('data-button-selection') || null;
    $('.currentrowtype').val(currRowType);
    TSM_jsonSubmit($('#rowRequest'));
}
function TSM_CompleteAddRow(data) {

    TSM_formDirty(true);
    var rowType = $('.currentrowtype').val();
    var template = TSM_ChooseTemplate(rowType);
    var repeater = $(".innerRowsContainer");
    var repeaterName = repeater.attr('data-rows-prefix');
    var item = null;
    var childrens = repeater.children();
    if (childrens.length > 0)
        item = MvcControlsToolkit_SortableList_AddNewChoice(repeaterName, template, childrens[0]).first();
    else
        item = MvcControlsToolkit_SortableList_AddNewChoice(repeaterName, template).first();
    var prefix = item.attr("id");
    var end_prefix = prefix.lastIndexOf("_");
    prefix = prefix.substring(0, end_prefix);
    TSM_CopyObjectToRow(item, data, prefix);
    if (data["CustomFieldItems"] != undefined) {
        $.ajax({
            cache: false,
            type: "POST",
            url: 'Timesheet/CustomFields',
            contentType: 'application/json',
            dataType: "json",
            data: JSON.stringify(data["CustomFieldItems"]),
            success: function (data) {
                $('#' + prefix).empty();
                $('#' + prefix).html(data);
                $('#' + prefix).find('[class=datatype]').each(function () {
                    var datatype = $(this).val();
                    var name = $(this).attr('field');

                });
                $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + data.AssignementId + "-" + rowType);
                $(".p-project-container").removeClass("p-project-container").addClass("p-" + data.ProjectId + data.AssignementName + "-" + rowType);
                item.find('.goto').trigger('click');

            },
            error: function (xhr, ajaxOptions, thrownError) {
                if (xhr.readyState == 4) {
                    $('#' + prefix).empty();
                    $('#' + prefix).html(xhr.responseText);
                    $('#' + prefix).find('[class=datatype]').each(function () {
                        var datatype = $(this).val();
                        var name = $(this).attr('field');
                        $('#' + prefix).find('[class=datatype]').each(function () {
                            var datatype = $(this).val();
                            var name = $(this).attr('field');
                            if (name == 'Lookup') {
                                name = $(this).attr('lookuptable');
                                datatype = 'Lookup';
                            }
                            $('#' + prefix).find('[class=datatype]').each(function () {
                                var datatype = $(this).val();
                                var name = $(this).attr('field');
                                if (name == 'Lookup') {
                                    name = $(this).attr('lookuptable');
                                    datatype = 'Lookup';
                                }
                                $(".detailTBCS[name=" + name + "]").each(function () {
                                    var jThis = $(this);
                                    jThis.hide();
                                    if (jThis.attr('class').indexOf(datatype) >= 0) {
                                        if (datatype == 'Date') {
                                            if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() != '') {
                                                jThis.text($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val().split(" ")[0]);
                                            }
                                            else {
                                                jThis.text('No Date');
                                            }
                                        }
                                        else if (datatype == 'Flag') {
                                            if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == 'True') {
                                                jThis[0].children[0].checked = true;
                                                jThis[0].children[1].checked = false;
                                            }
                                            else if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == 'False') {
                                                jThis[0].children[0].checked = false;
                                                jThis[0].children[1].checked = true;
                                            }
                                            else {
                                                if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == '') {
                                                    jThis[0].children[0].checked = false;
                                                    jThis[0].children[1].checked = false;
                                                }
                                            }
                                        }
                                        else if (datatype == 'Lookup') {
                                            jThis.html($('#' + prefix).find("." + name + "_display").html());
                                            jThis.find('option').val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val());
                                        }
                                        else {
                                            jThis.val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val());
                                        }
                                        jThis.show();
                                    }
                                    else {
                                        jThis.empty();
                                    }

                                });

                            });
                        });

                    });
                    $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + data.AssignementId + "-" + rowType);
                    $(".p-project-container").removeClass("p-project-container").addClass("p-" + data.ProjectId + data.AssignementName + "-" + rowType);
                    item.find('.goto').trigger('click');
                }
            }

        });
    }

}

function TSM_GetRowValue(name, prefix, value) {
    if (value == null) return;
    name = prefix + "_" + name;
    return $('#' + name).val();

}
function TSM_SetRowValue(name, prefix, value) {
    if (value == null) return;
    name = prefix + "_" + name;
    var el = document.getElementById(name);
    if (el == null) return;

    var tv = typeof (value);
    if (tv == "boolean") {
        $(el).val(value ? "True" : "False");
        $(name + "_display").text(value ? TSM_Checked : TSM_UnChecked);
    }
    else {
        var vtype = -1;
        var prefix = '';
        var postfix = '';
        var nullString = ''
        var format = '';
        var target = $('#' + name + "_display");
        var htarget = $(el);
        var fobj = target.attr('data-format') || null;
        if (fobj == null) {
            if (tv == "string") vtype = MvcControlsToolkit_DataType_String;
            else if (tv == "number") {
                if (!isNaN(parseInt(value * 1)) && parseInt(value * 1) === value) vtype = MvcControlsToolkit_DataType_Int;
                else vtype = MvcControlsToolkit_DataType_Float;
            }
            else if (value instanceof Date) vtype = MvcControlsToolkit_DataType_DateTime;
        }
        else {
            fobj = $.parseJSON(fobj);
            vtype = fobj.type;
            prefix = fobj.prefix;
            postfix = fobj.postfix;
            nullString = fobj.nullstring;
            format = fobj.format;
            if (tv == "string" && vtype > 0)
                value = MvcControlsToolkit_Parse(value, vtype);
        }
        if (target.length > 0) target.text(MvcControlsToolkit_FormatDisplay(value, format, vtype, prefix, postfix, nullString));
        if (htarget.length > 0) htarget.val(MvcControlsToolkit_Format(value, '', vtype, '', ''));
    }
}

function TSM_CopyObjectToRow(item, source, prefix) {
    for (var property in source) {
        if (property == "DayTimes") {
            var val = source[property] || null;
            if (val != null) {
                for (var i = 0; i < val.length; i++) {
                    TSM_SetRowValue(property + '_' + i + '_', prefix, val[i]);
                }
            }
        }


        TSM_SetRowValue(property, prefix, source[property]);
    }
}

function TSM_CopyToRow(prefix) {
    $(".detailBoolean").each(function () {
        var jThis = $(this);
        var val = jThis.prop('checked');
        var name = jThis.attr('id');
        $('#' + prefix + '_' + name).val(val ? "True" : "False");
        $('#' + prefix + '_' + name + '_display').text(val ? TSM_Checked : TSM_UnChecked);

    });
    $(".detailTB").each(function () {
        var jThis = $(this);
        var name = jThis.attr('id');
        if ((jThis.attr('data-element-type') || '') != '') {
            name = name.substring(0, name.lastIndexOf("_"));
            jThis = $('#' + name);
        }

        tname = '#' + name.substring(0, name.length - 3) + '_Total_' + name.substring(name.length - 2, name.length - 1);
        var val;
        if (!isNaN(parseFloat(jThis.val()))) {
            val = parseFloat(jThis.val());
        }
        else {
            val = 0.00;
        }


        var txtval;
        if (name.indexOf("plh") == -1 && name.indexOf("DayTime") != -1) {
            if (!isNaN(parseFloat($(tname).text()))) {
                txtval = parseFloat($(tname).text());
            }
            else {
                txtval = 0.00;
            }

            var oldvalue = TSM_GetRowValue(name, prefix, jThis.val());

            TSM_SetRowValue(name, prefix, val);
            if (!isNaN(parseFloat(oldvalue)) && !isNaN(parseFloat(jThis.val()))) {
                $(tname).text((txtval + parseFloat(jThis.val()) - parseFloat(oldvalue)).toFixed(2));
            }
            else if (isNaN(parseFloat(oldvalue))) {
                $(tname).text((txtval + val).toFixed(2));
            }
            else {
                $(tname).text((txtval + val).toFixed(2));
            }
        }
        else {
            TSM_SetRowValue(name, prefix, jThis.val());
        }
    });

    $(".detailTBCS").each(function () {
        var jThis = $(this);
        var name = jThis.attr('name');
        var valuetype = jThis.attr('valuetype');
        if (valuetype == 'Date') {
            $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text(jThis.text());
            $('#' + prefix).find('[class=' + name + '][valuetype=' + valuetype + ']').val(jThis.text());
        }
        else if (valuetype == 'Flag') {
            if (jThis[0].children.length > 0 && jThis[0].children[0].checked) {
                $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text('Yes');
                $('#' + prefix).find('[class=' + name + '][valuetype=' + valuetype + ']').val('True');
            }
            if (jThis[0].children.length > 0 && jThis[0].children[1].checked) {
                $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text('No');
                $('#' + prefix).find('[class=' + name + '][valuetype=' + valuetype + ']').val('False');
            }
        }
        else if (valuetype == 'Lookup') {

            if (jThis.find('option:selected').length > 0 && jThis.find('option:selected').val() != '') {
                $('#' + prefix).find('[class=' + name + '][valuetype = Lookupid]').val(jThis.find('option:selected').val());
            }
           
            $('#' + prefix).find('[class=' + name + '][valuetype = Lookupvalue]').val(jThis.find('option:selected').text());
            $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text(jThis.find('option:selected').text());
        }
        else {
            $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text(jThis.val());
            $('#' + prefix).find('[class=' + name + '][valuetype=' + valuetype + ']').val(jThis.val());
        }
    });

}
function TSM_ensurePositive() {
    $('.positive').each(function () {
        var value = MvcControlsToolkit_TypedTextBox_Get(this, MvcControlsToolkit_DataType_UInt);
        if (value != null && value < 0) MvcControlsToolkit_TypedTextBox_Set(this, 0, '', MvcControlsToolkit_DataType_UInt);
        if (value != null && $(this).hasClass('dayTime') && value > 24) MvcControlsToolkit_TypedTextBox_Set(this, 24, '', MvcControlsToolkit_DataType_UInt);
    });
}

function TSM_DeleteRow(prefix) {
    prefix = prefix.replace("_remove", "");
    $(".detailTB").each(function () {
        var jThis = $(this);
        var name = jThis.attr('id');
        if ((jThis.attr('data-element-type') || '') != '') {
            name = name.substring(0, name.lastIndexOf("_"));
            jThis = $('#' + name);
        }

        tname = '#' + name.substring(0, name.length - 3) + '_Total_' + name.substring(name.length - 2, name.length - 1);
        var val;
        if (!isNaN(parseFloat(jThis.val()))) {
            val = parseFloat(jThis.val());
        }
        else {
            val = 0.00;
        }


        var txtval;
        if (name.indexOf("plh") == -1 && name.indexOf("DayTime") != -1) {
            if (!isNaN(parseFloat($(tname).text()))) {
                txtval = parseFloat($(tname).text());
            }
            else {
                txtval = 0.00;
            }

            var oldvalue = TSM_GetRowValue(name, prefix, jThis.val());
            if (!isNaN(parseFloat(oldvalue))) {
                $(tname).text((txtval - parseFloat(oldvalue)).toFixed(2));
            }
        }
    });


    $('#' + prefix.replace("_remove", "") + '_Container').remove();
}
function TSM_CopyFromRow(prefix) {
    $(".detailBoolean").each(function () {
        var jThis = $(this);
        var name = jThis.attr('id');
        jThis.prop('checked', $('#' + prefix + '_' + name).val() == 'True');
    });
    $(".descriptiveDetail").each(function () {
        var jThis = $(this);
        var name = jThis.attr('id');
        jThis.text($('#' + prefix + '_' + name).text());
    });


    $(".detailTB").each(function () {
        var jThis = $(this);
        var name = jThis.attr('id');
        if ((jThis.attr('data-element-type') || '') != '') {
            name = name.substring(0, name.lastIndexOf("_"));
            jThis = $('#' + name);
            MvcControlsToolkit_TypedTextBox_SetString(this,
                $('#' + prefix + '_' + name).val());
            $(this).trigger('pblur');
        }
        else {
            var value = $('#' + prefix + '_' + name).val();
            jThis.val(value);
            var target = $('#' + this.id + '_display');
            var fobj = target.attr('data-format') || null;
            if (fobj != null) {
                fobj = $.parseJSON(fobj);
                value = MvcControlsToolkit_Parse(value, fobj.type);
                value = MvcControlsToolkit_FormatDisplay(value, fobj.format, fobj.type, fobj.prefix, fobj.postfix, fobj.nullstring);
            }
            target.text(value);
        }
    });

    $('#' + prefix).find('[class=datatype]').each(function () {
        var datatype = $(this).val();
        var name = $(this).attr('field');
        if (name == 'Lookup') {
            name = $(this).attr('lookuptable');
            datatype = 'Lookup';
        }
        $(".detailTBCS[name=" + name + "]").each(function () {
            var jThis = $(this);
            jThis.hide();
            if (jThis.attr('class').indexOf(datatype) >= 0) {
                if (datatype == 'Date') {
                    if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() != '') {
                        jThis.text($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val().split(" ")[0]);
                    }
                    else {
                        jThis.text('No Date');
                    }
                }
                else if (datatype == 'Flag') {
                    if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == 'True') {
                        jThis[0].children[0].checked = true;
                        jThis[0].children[1].checked = false;
                    }
                    else if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == 'False') {
                        jThis[0].children[0].checked = false;
                        jThis[0].children[1].checked = true;
                    }
                    else {
                        if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() == '') {
                            jThis[0].children[0].checked = false;
                            jThis[0].children[1].checked = false;
                        }
                    }
                }
                else if (datatype == 'Lookup') {
                    jThis.html($('#' + prefix).find("." + name + "_display").html());
                    if($('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != undefined && $('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != '')
                    $('.' + name + 'lkp').val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val());
                }
                else {
                    jThis.val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val());
                }
                jThis.show();
            }
            else {
                jThis.empty();
            }

        });

    });

}
function TSM_CopySummary(isTask) {
    $('.updateerrormessage-d').text($('.updateerrormessage').val() || '');
    if (!isTask) {
        $('.currstatus-d').text($('.currstatus').val() || '');
        $('.currcomments-d').text($('.currcomments').val() || '');
        $('.currname-d').text($('.currname').val() || '');
        var target;
        var fobj;
        var value;

        value = $('.currtotalact').val();
        target = $('.currtotalact-d');
        fobj = target.attr('data-format') || null;
        fobj = $.parseJSON(fobj);
        value = MvcControlsToolkit_Parse(value, fobj.type);
        value = MvcControlsToolkit_FormatDisplay(value, fobj.format, fobj.type, fobj.prefix || '', fobj.postfix || '', fobj.nullstring || '');
        target.text(value);

        value = $('.currtotalovertime').val();
        target = $('.currtotalovertime-d');
        fobj = target.attr('data-format') || null;
        fobj = $.parseJSON(fobj);
        value = MvcControlsToolkit_Parse(value, fobj.type);
        value = MvcControlsToolkit_FormatDisplay(value, fobj.format, fobj.type, fobj.prefix || '', fobj.postfix || '', fobj.nullstring || '');
        target.text(value);

        value = $('.currtotalnonbill').val();
        target = $('.currtotalnonbill-d');
        fobj = target.attr('data-format') || null;
        fobj = $.parseJSON(fobj);
        value = MvcControlsToolkit_Parse(value, fobj.type);
        value = MvcControlsToolkit_FormatDisplay(value, fobj.format, fobj.type, fobj.prefix || '', fobj.postfix || '', fobj.nullstring || '');
        target.text(value);

        value = $('.currtotalovertimenonbill').val();
        target = $('.currtotalovertimenonbill-d');
        fobj = target.attr('data-format') || null;
        fobj = $.parseJSON(fobj);
        value = MvcControlsToolkit_Parse(value, fobj.type);
        value = MvcControlsToolkit_FormatDisplay(value, fobj.format, fobj.type, fobj.prefix || '', fobj.postfix || '', fobj.nullstring || '');
        target.text(value);
    }
}

function daysBetween(sfirst, ssecond) {

    var first = new Date(sfirst);
    var second = new Date(ssecond);
    // Copy date parts of the timestamps, discarding the time parts.
    var one = new Date(first.getFullYear(), first.getMonth(), first.getDate());
    var two = new Date(second.getFullYear(), second.getMonth(), second.getDate());

    // Do the math.
    var millisecondsPerDay = 1000 * 60 * 60 * 24;
    var millisBetween = two.getTime() - one.getTime();
    var days = millisBetween / millisecondsPerDay;

    // Round down.
    return Math.floor(days);
}
function TSM_PrepareDays(start, end) {
    var container = $('#dayContainer');
    if (container.length == 0) return;
    var oldStart = container.data('oldStart') || '';
    var olddur = container.data('olddur') || '';
    if (start == '' || end == '' || start == undefined || end == undefined) return;
    var dur = daysBetween(start, end);
    if ((start == oldStart && dur == olddur)) {
        return;
    }
    container.empty();
    var template = $('#dayTemplate').html();
    var toBuild = "";
    var dateStart = MvcControlsToolkit_Parse(start, MvcControlsToolkit_DataType_DateTime);
    var curr = dateStart;
    var i = 0;
    while (i < dur) {
        if (i != 0 && i % 2 == 0) toBuild = toBuild + "</br>";
        var ds = MvcControlsToolkit_Format(curr, 'ddd dd/MM', MvcControlsToolkit_DataType_DateTime, '', ' ');
        toBuild = toBuild + template.replace(/_plh_/g, i + '').replace(/_p1lh_/g, ds);
        i++;
        curr = new Date(curr.getFullYear(), curr.getMonth(), curr.getDate() + 1);
    }
    $(toBuild).appendTo(container);
    $.validator.unobtrusive.parseExt('#dayContainer');
    $(document).ready(function () { container.find("input:text").focus(function () { $(this).select(); }); });
    container.data('oldStart', start);
    container.data('olddur', dur);
}

MvcControlsToolkit_ParseRegister.add(

 function (selector) { if (selector != '#dayContainer') $(selector).trigger('create'); },

 false)


$.validator.setDefaults({
    ignore: ""
});




