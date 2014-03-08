
function TSM_OpenTask(id) {
    var data = $('[data-row-from=' + id + ']');
    var row_type = data.attr('data-button-selection');
    row_choice_ViewList.Select("property_group_" + row_type);
    $('#rowsTitle').text(TSM_DetailTitle(row_type));
    var prefix = data.attr('data-row-from');
    $('#basedetail').data('chosen-row', prefix);
    TSM_CopyFromRow(prefix);
}

function TSM_SaveTask() {
    var data = $(this);
    if (!$('#detailform').validate().form()) {
        data.cancel = true;
        return;
    }
    TSM_ensurePositive();
    TSM_CopyToRow($('#basedetail').data('chosen-row'));
    TSM_UpdateMainLayout();
    TSM_formDirty(true);
}


(function () {
    var cookiename = "_configuration_";
    var userField = "_user_";
    var taskIdField = "_task_";
    var TimesheetIdField = "_row_";
    var ApprovalIdField = "_approval_";
    var cookieUpdatedField = "_updated_";
    window['configuration'] = {
        show: function () {
            var cookie = getCookie(cookiename, true);
            alert("task view: " + cookie[taskIdField] +
                  " row view: " + cookie[TimesheetIdField] +
                  " approval view: " + cookie[ApprovalIdField] +
                  " user: " + cookie[userField] +
                  " updated: " + cookie[cookieUpdatedField]);
        },
        set: function (row, task, approval) {
            var cookie = getCookie(cookiename, true);
            cookie[taskIdField] = task || '';
            cookie[TimesheetIdField] = row || '';
            cookie[ApprovalIdField] = approval || '';
            cookie[cookieUpdatedField] = 'true';
            setCookie(cookiename, cookie, true, 365);
        },
        get: function () {
            var cookie = getCookie(cookiename, true);
            return {
                task: cookie[taskIdField] || '',
                row: cookie[TimesheetIdField] || '',
                approval: cookie[ApprovalIdField] || ''
            };
        }

    };


})();

function TSM_ChangePage(page, options, origin) {
    var data = { cancel: false, action: origin };
    $('#' + page).trigger('gotoing', data);
    $('#' + page).trigger('goto', data);
}
function TSM_Return(target, options, event, origin) {
    var data = { cancel: false, action: origin };
    $('#' + target).trigger(event + "ing", data);
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
    TSM.initPreloader('show');
    $.post(jForm.attr('data-ajax-url'), jForm.serialize(), function (data) {
        target.html(data);
        $.validator.unobtrusive.reParse(target);
        TSM_initButtons(TSM_PageChangeOptions);
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete(data);

        }
        TSM.initPreloader('hide');
    });
}
function TSM_jsonSubmit(jForm) {
    TSM.initPreloader('show');
    $.post(jForm.attr('data-ajax-url'), jForm.serialize(), function (data) {
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete($.parseJSON(data));

        }
    },
    "text");
    TSM.initPreloader('hide');
}
function TSM_formDirty(x, y) {
    y = y || 'mainform';
    var cantEdit = ($('.currtotalact').length != 0) && ($('.candelete').val() != 'True') && ($('.approvalmode').val() != 'True');
    var form = $('#' + y);
    form.data('_dirty_', x);
    form.trigger('dirtychange', x);
    var recallDeleteDisabled = ($('.currentperiodid').val() || '') == '';
    //var empty=$('.innerRowsContainer').children(':not(span)').length == 0;
    var empty = false;
    if ((!x) || empty || cantEdit) {
        $('.csubmit-' + y).addClass('ui-disabled');
        $('.fsubmit-' + y).addClass('ui-disabled');
    }
    else {
        $('.csubmit-' + y).removeClass('ui-disabled');
        $('.fsubmit-' + y).removeClass('ui-disabled');
    }
    if (recallDeleteDisabled || $('.canrecall').val() != 'True') {
        $('#btnRecall').addClass('ui-disabled');
    }
    else {
        $('#btnRecall').removeClass('ui-disabled');
    }
    if (recallDeleteDisabled || $('.candelete').val() != 'True') {
        $('#btnDelete').addClass('ui-disabled');
    }
    else {
        $('#btnDelete').removeClass('ui-disabled');
    }
    if (recallDeleteDisabled) {
        $('#btnRecallDelete').addClass('ui-disabled');
    }
    else {
        $('#btnRecallDelete').removeClass('ui-disabled');
    }
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
            if (TSM_isFormDirty() && !window.confirm($(this).attr('data-leave-application'))) { e.preventDefault(); return; }
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

    $(".leaveApplication").click(
        function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if ($(this).hasClass('ui-disabled')) return;
            TSM_LeaveTo($(this).attr('data-button-target'), $(this).attr('data-button-application'));
        }
    );

    $(".TimesheetId").click(
        function (e) {
            //e.preventDefault();
            e.stopPropagation();
            EnableApprove($(this).attr('data-button-target'));
        }

    );

    $(".TaskId").click(
        function (e) {
            //e.preventDefault();
            e.stopPropagation();
            EnableApprove($(this).attr('data-button-target'));
        }
    );

    $(".taskapprovals").click(
        function (e) {
            //e.preventDefault();
            e.stopPropagation();
            EnableTaskApprove();
        }
    );


}

var TSM_PageChangeOptions = null;
$(document).ready(function () {
    TSM_initButtons(TSM_PageChangeOptions);

    $('.leaveapp').bind('click', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($(this).attr('data-leave-application'))) { e.preventDefault(); return; }

        GotoUrl($(this).attr('data-target'));
    });
    $(function () {
        $(".datepicker").datepicker();
    });

    $('.updatelineclass').change(function () {
        var jThis = $(this);
        $('.currentlineclassid').val(jThis.find('option:selected').val());
        $('.currentlineclassname').val(jThis.find('option:selected').text());
        TSM_ConfirmAdd(true);
    });

    $('.dynamictasks').change(function () {
        $('.currentassname').val($(this).find('option:selected').text());
        $('.currentassid').val($(this).find('option:selected').val());
        TSM_ConfirmAdd(true);
    });

    $('.updatetasks').change(function () {
        TSM.initPreloader('show');
        var jThis = $('.updatetasks');
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
        var selectedid = jThis.val();

        $.ajax({
            cache: false,
            type: "POST",
            url: jThis.attr('data-action') + '?projectId=' + selectedid,
            async: "false",
            contentType: 'application/json',
            success: function (data) {
                $('#assignmentsContainer').html(data);
                TSM_TasksOptionsCallback($('.dynamictasks'));
                TSM_ConfirmAdd();
                $('.dynamictasks').change(function () {
                    $('.currentassname').val($(this).find('option:selected').text());
                    $('.currentassid').val($(this).find('option:selected').val());
                    TSM_ConfirmAdd(true);
                });
                TSM.initPreloader('hide');
            }
        });

    });


    //    $('.dynamicperiods').change(function (e) {
    //        TSM_NewPeriod();
    //    });
    $('.updatetimesheets').change(function () {

        var jForm = $('#periodform');
        var target = $('#' + jForm.attr('data-ajax-target'));
        TSM.initPreloader('show');
        var jForm = $('#periodform');
        $.post(jForm.attr('data-ajax-url') + '?speriod=' + $('.updatetimesheets').val(), jForm.serialize(), function (data) {
            target.html(data);
            $.validator.unobtrusive.reParse(target);
            TSM_initButtons(TSM_PageChangeOptions);
            var onComplete = jForm.attr('data-ajax-oncomplete') || null;
            if (onComplete != null) {
                onComplete = eval(onComplete);
                onComplete();

            }
            TSM.initPreloader('hide');
        });
    });
    $('.updaterowview,  .updatetaskview').change(function () {
        TSM_View();
    });
    TSM_ConfirmPeriod(true, null);
    TSM_View();
    TSM_UpdateMainLayout();









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
$(document).ready(function (e) {
    if (e.target != undefined) {
        if (e.target.id == 'taskselection') {
            TSM_TasksOptionsCallback($('.dynamictasks'));
            TSM_ConfirmAdd();

        }
        else if (e.target.id == 'periodselection') {
            TSM_PeriodsOptionsCallback($('.updatetimesheets'));
        }
    }

})
function TSM_TasksOptionsCallback(jTarget) {
    if (jTarget.length > 0) {
        if (jTarget[0].options.length > 1) jTarget.removeClass('ui-disabled');
        else jTarget.addClass('ui-disabled');
        TSM_ConfirmAdd();
    }
    else {
        jTarget.addClass('ui-disabled');
        TSM_ConfirmAdd();
        if ($('#RequiredProgectId option:selected').text() != 'Choose project') {
            $('.rowlist').each(function () {
                var jThis = $(this);
                jThis.removeClass('ui-disabled');
            });
            $('#' + $('#RequiredProgectId option:selected').text() + '_display').val('Top Level');

        }

    }
}

function TSM_ConfirmAdd(show) {
    var cGuid = $('.dynamictasks').val();
    var lineclassGuid = $(".updatelineclass").val()
    var projectId = $('#RequiredProgectId').val();
    if (cGuid == '') {
        $('.rowlist').each(function () { $(this).addClass('ui-disabled'); });
        if (projectId == "-1") {
            $(".updatelineclass").addClass('ui-disabled');
        }
        else {
            $(".updatelineclass").removeClass('ui-disabled');
        }
        return;
    }

    if (projectId == "-1") {
        $(".updatelineclass").addClass('ui-disabled');
    }
    else {
        $(".updatelineclass").removeClass('ui-disabled');
    }
    $('.rowlist').each(function () {
        var jThis = $(this);
        var rowId;
        if (projectId == "-1") {
            rowId = "p-" + cGuid + "-" + jThis.attr('data-button-selection');
        }
        else {
            rowId = "p-" + cGuid + "_" + lineclassGuid + "-" + jThis.attr('data-button-selection');
        }
        var assnid = "p-" + "TopLevel_" + lineclassGuid + projectId + "-" + jThis.attr('data-button-selection');
        if (((cGuid || '') != '') && ($('.currentperiodid').val() != '')) {

            if ($('.dynamictasks option:selected').text() == 'Top Level') {
                if ($("." + assnid).length == 0) jThis.removeClass('ui-disabled');
                else jThis.addClass('ui-disabled');
            }
            else {
                if ($("." + rowId).length == 0) {
                    jThis.removeClass('ui-disabled');
                }
                else {
                    jThis.addClass('ui-disabled');

                }

            }


        }
        else jThis.addClass('ui-disabled');
    });
}
var TSM_CurrRowType = null;
function TSM_PrepareRowType(button) {
    TSM_CurrRowType = $(button).attr('data-button-selection') || null;
    $('.currentrowtype').val(TSM_CurrRowType);
}
function TSM_PeriodsOptionsCallback(jTarget) {
    ////    if (jTarget[0].options.length > 0) {
    jTarget.removeClass('ui-disabled');
    $('#btnConfirmPeriod').prop('disabled', false);
    ////        
    ////    }
    ////    else {
    ////        jTarget.addClass('ui-disabled')
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
    TSM_formDirty(false);
    TSM_UpdateMainLayout();
    $('.currstatus-d').text($('.currstatus').val() || '');
}

function TSM_UpdateMainLayout() {
    // $('.updatemainlayout').trigger('updatelayout');



}
function TSM_View() {
    var current = configuration.get();
    var oldTask = '';
    var oldRow = '';
    var oldapproval = '';
    if (current.task) oldTask = $('.updatetaskview option[value="' + current.task + '"]').text();
    if (current.row) oldRow = $('.updaterowview option[value="' + current.row + '"]').text();
    if (current.approval) oldapproval = $('.updateapprovalview option[value="' + current.approval + '"]').text();

    if (oldTask) $('.currenttaskview').text(oldTask);
    if (oldRow) $('.currentrowview').text(oldRow);
    if (oldapproval) $('.currentapprovalview').text(oldapproval);
    if (window['_rowViewDescriptions_']) {
        $('.rowViewDescription-o').text(window['_rowViewDescriptions_'][current.row] || '');
        $('.rowViewDescription').text(window['_rowViewDescriptions_'][$('.updaterowview').val()] || '');
    }
    if (window['_taskViewDescriptions_']) {
        $('.taskViewDescription-o').text(window['_taskViewDescriptions_'][current.task] || '');
        $('.taskViewDescription').text(window['_taskViewDescriptions_'][$('.updatetaskview').val()] || '');
    }

    if (window['_approvalViewDescriptions_']) {
        $('.approvalViewDescriptions-o').text(window['_approvalViewDescriptions_'][current.task] || '');
        $('.approvalViewDescriptions').text(window['_approvalViewDescriptions_'][$('.updateapprovalview').val()] || '');
    }
}
function TSM_ConfirmView() {
    current = configuration.get();
    var rowC = $('.updaterowview');
    var taskC = $('.updatetaskview');
    var approvalC = $('.updateapprovalview');
    if (rowC.length > 0) current.row = rowC.val();
    var taskC = $('.updatetaskview');
    var approvalC = $('.updateapprovalview');
    configuration.set(current.row, current.task, current.approval);
    GotoUrl($('#periodform').attr('data-ajax-url'));
    //$('#periodform').submit();
}
function TSM_startAddRow(origin) {
    var currRowType = $(origin).attr('data-button-selection') || null;
    $('.currentrowtype').val(currRowType);
    TSM_jsonSubmit($('#rowRequest'));
}
function TSM_CompleteAddRow(data) {
    data.ProjectId = $('#RequiredProgectId').val();
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


    var projectId = $('#RequiredProgectId').val();
    if (projectId != "-1") {
        if (data["CustomFieldItems"] != undefined) {
            $.ajax({
                cache: false,
                type: "POST",
                url: 'Timesheet/CustomFields',
                async: "false",
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
                    if (data.AssignementName == 'Top Level') {
                        $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + "TopLevel_" + data.LineClass.Id + data.ProjectId + "-" + rowType);
                    }
                    else {
                        $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + data.AssignementId + "_" + data.LineClass.Id + "-" + rowType);
                    }
                    $(".p-project-container").removeClass("p-project-container").addClass("p-" + data.ProjectId + data.AssignementName + "-" + rowType);
                    TSM.init();

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
                                $(".detailTBCS[name=" + name + "]").each(function () {
                                    var jThis = $(this);
                                    jThis.hide();
                                    if (jThis.attr('class').indexOf(datatype) >= 0) {
                                        if (datatype == 'Date') {
                                            if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val() != '') {
                                                jThis.val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val().split(" ")[0]);
                                            }
                                            else {
                                                jThis.val('No Date');
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
                                            if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != undefined && $('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != '')
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

                        });
                        if (data.AssignementName == 'Top Level') {
                            $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + "TopLevel_" + data.LineClass.Id + data.ProjectId + "-" + rowType);
                        }
                        else {
                            $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + data.AssignementId + "_" + data.LineClass.Id + "-" + rowType);
                        }
                        $(".p-project-container").removeClass("p-project-container").addClass("p-" + data.ProjectId + data.AssignementName + "-" + rowType);
                        TSM.init();

                    }
                }

            });
        }
    }
    else {
        $(".p-guid-container").removeClass("p-guid-container").addClass("p-" + data.AssignementId + "-" + rowType);
        $(".p-project-container").removeClass("p-project-container").addClass("p-" + data.ProjectId + data.AssignementName + "-" + rowType);
        TSM.init();
    }
    TSM.init();
    CloseDialog('#taskselection');
    $('#RequiredProgectId').val('-100');
    $('#assignments').val('-100');
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
        if (property == "AssignementName") {
            var val = source[property] || null;
            if (val != null) {
                TSM_SetRowValue(property, prefix, source[property] + " " + source["LineClass"].Name);
                $(prefix).find('.currentlineclassid').val(source["LineClass"].Id);
                continue;
            }
        }

        if (property == "LineClass") {
            $('#' + prefix + "_LineClass_Id").val(source["LineClass"].Id);
            $('#' + prefix + "_LineClass_Name").val(source["LineClass"].Name);
        }
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
$("#" + prefix + "_Container").addClass("dirty");
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
        var allTotal;
        var txtval;
        if (name.indexOf("plh") == -1 && name.indexOf("DayTime") != -1) {
            if (!isNaN(parseFloat($(tname).text()))) {
                txtval = parseFloat($(tname).text());
            }
            else {
                txtval = 0.00;
            }

            if (!isNaN(parseFloat($('#allTotal').text()))) {
                allTotal = parseFloat($('#allTotal').text());
            }
            else {
                allTotal = 0.00;
            }

            var oldvalue = TSM_GetRowValue(name, prefix, jThis.val());
            if (isNaN(parseFloat(oldvalue))) {
                oldvalue = 0.00;
            }

            TSM_SetRowValue(name, prefix, val);
            if (!isNaN(parseFloat(oldvalue)) && !isNaN(parseFloat(jThis.val()))) {
                $(tname).text((txtval + parseFloat(jThis.val()) - parseFloat(oldvalue)).toFixed(2));
                $('#allTotal').text((allTotal + parseFloat(jThis.val()) - parseFloat(oldvalue)).toFixed(2));
            }
            else if (isNaN(parseFloat(jThis.val()))) {
                $(tname).text(val.toFixed(2));
                $('#allTotal').text((allTotal - parseFloat(oldvalue)).toFixed(2));
            }
            else if (isNaN(parseFloat(oldvalue))) {
                $(tname).text((txtval + val).toFixed(2));
                $('#allTotal').text((allTotal + val).toFixed(2));
            }
            else {
                $(tname).text((txtval + val).toFixed(2));
                $('#allTotal').text((allTotal + val).toFixed(2));
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
            $('#' + prefix).find('[class=' + name + '_cf_display][valuetype=' + valuetype + ']').text(jThis.val());
            if (jThis.val() != 'No Date')
                $('#' + prefix).find('[class=' + name + '][valuetype=' + valuetype + ']').val(jThis.val());
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

function TSM_DeleteRow() {
    var prefix = $('#basedetail').data('chosen-row');
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
        var total;
        if (name.indexOf("plh") == -1 && name.indexOf("DayTime") != -1) {
            if (!isNaN(parseFloat($(tname).text()))) {
                txtval = parseFloat($(tname).text());
            }
            else {
                txtval = 0.00;
            }

            if (!isNaN(parseFloat($('#allTotal').text()))) {
                total = parseFloat($('#allTotal').text());
            }
            else {
                total = 0.00;
            }

            var oldvalue = TSM_GetRowValue(name, prefix, jThis.val());
            if (!isNaN(parseFloat(oldvalue))) {
                $(tname).text((txtval - parseFloat(oldvalue)).toFixed(2));
                $('#allTotal').text((total - parseFloat(oldvalue)).toFixed(2));
            }
        }
    });


    $('#' + prefix.replace("_remove", "") + '_Container').remove();
    TSM_formDirty(true);
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
            if ($('#' + prefix + '_' + name).val() == 0) {
                MvcControlsToolkit_TypedTextBox_SetString(this,
                '');
            }
            else {
                MvcControlsToolkit_TypedTextBox_SetString(this,
                $('#' + prefix + '_' + name).val());
            }
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
                        jThis.val($('#' + prefix).find('[class=' + name + ']' + '[valuetype=' + datatype + ']').val().split(" ")[0]);
                    }
                    else {
                        jThis.val('No Date');
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
                    if ($('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != undefined && $('#' + prefix).find('[class=' + name + ']' + '[valuetype=Lookupid]').val() != '')
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

function reload() {
    TSM.initPreloader("show");
    window.location.href = window.location.href;
}

function GotoUrl(url) {
    TSM.initPreloader("show");
    window.location.href = url;
}

function TSM_ConfirmApproveTimesheet(data) {
    if (data) {
        TSM_CopySummary(true, data['ErrorMessage']);
        OpenDialog('#tskupdateSummary');
    }
}

function EnableApprove(checkbox) {
    if ($('.TimesheetId').is(':checked') || $('.TaskId').is(':checked')) {
        $('.approvals').removeClass('ui-disabled');
    }
    else {
        $('.approvals').addClass('ui-disabled');
    }
    $('#' + checkbox).children('.selectedtimeappr:first').val($('#' + checkbox).children('.TimesheetId:first').is(':checked'));
    $('#' + checkbox).children('.selectedtaskappr:first').val($('#' + checkbox).children('.TaskId:first').is(':checked'));
}

function EnableTaskApprove(checkbox) {
    if ($('.taskapprovals').is(':checked')) {
        $('.tskbtns').removeClass('ui-disabled');
    }
    else {
        $('.tskbtns').addClass('ui-disabled');
    }
}

function ApproveRejectSelectedTimesheets(mode) {
    var jForm = $('#approvalform');
    TSM.initPreloader('show');
    $.post(jForm.attr('data-ajax-url') + "/?mode=" + mode, jForm.serialize(), function (data) {
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete(data);

        }
        TSM.initPreloader('hide');
    });

}

function ApproveRejectTimesheet(mode) {
    var jForm = $('#approvalform');
    TSM.initPreloader('show');
    $.post(jForm.attr('data-ajax-url') + "/?mode=" + mode, jForm.serialize(), function (data) {
        var onComplete = jForm.attr('data-ajax-oncomplete') || null;
        if (onComplete != null) {
            onComplete = eval(onComplete);
            onComplete(data);

        }
        TSM.initPreloader('hide');
    });
}



function ApproveSelectedTasks(aapprovalmode) {
    var jForm = $('#approvalform');
    TSM.initPreloader('show');
    var selectedtasks = new Array();
    var count = 0;
    $('.taskapprovals').each(function () {
        if ($(this).is(':checked')) {
            selectedtasks[count] = $(this).attr('data-row-from');
            count++;
        }
    });

    var arr = JSON.stringify(selectedtasks);
    $.ajax({
        cache: false,
        type: "POST",
        url: jForm.attr('data-ajax-url'),
        async: "false",
        contentType: 'application/json',
        data: JSON.stringify({ assignments: selectedtasks, mode: aapprovalmode }),
        success: function (data) {
            TSM_ConfirmApproveTimesheet(data);
            TSM.initPreloader('hide');
        }
    });
}


function TSM_CopySummary(isTask, msg) {
    if (msg) {
        $('.updateerrormessage-d').text(msg);
    }
    else {
        $('.updateerrormessage-d').text($('.updateerrormessage').val() || '');
    }

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
    return Math.floor(days) + 1;
}
function TSM_PrepareDays(start, end) {
    var container = $('#dayTemplate');
    var hdrContainer = $('#dayHdrTemplate');
    if (container.length == 0) return;
    var oldStart = container.data('oldStart') || '';
    var olddur = container.data('olddur') || '';
    if (start == '' || end == '' || start == undefined || end == undefined) return;

    var dur = parseInt($('#noOfDays').val());
    var periodStart = $('#periodStart').val();
    if ((start == oldStart && dur == olddur)) {
        return;
    }

    var template = $('#dayTemplate').html();
    var hdrTemplate = $('#dayHdrTemplate').html();
    container.empty();
    hdrContainer.empty();
    var toBuild = "";
    var toBuildhdr = "";
    var dateStart = periodStart;
    var i = 0;
    $('.shortPeriods').each(function () {
        toBuildhdr = toBuildhdr + hdrTemplate.replace(/_p1lh_/g,  $(this).val());
        toBuild = toBuild + template.replace(/_plh_/g, i + '');
        i++;
    });
    container.html(toBuild);
    hdrContainer.html(toBuildhdr);
    //$(toBuild).appendTo(container);
    //$(toBuildhdr).appendTo(hdrContainer);
    //$.validator.unobtrusive.parseExt('#dayContainer');
    //$.validator.unobtrusive.parseExt('#dayhdrContainer');
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




