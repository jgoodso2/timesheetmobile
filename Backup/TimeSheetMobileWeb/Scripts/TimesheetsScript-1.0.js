function timesheetsSubmittedOk() {
    TSM_formDirty(false);
    TSM_UpdateMainLayout();
    TSM_CopySummary(false);
    OpenDialog('#tsSummary');
}

function TSM_CompleteRD(data) {
    if (data) {
        if (data['IsRecall']) {
            CloseDialog('#recallDelete');
            TSM_CopySummary(true,data['ErrorMessage']);
            OpenDialog('#tskupdateSummary');
            //
            return;setTimeout(function () { TSM_partialSubmit($('#periodform')); }, 0);
        }
        else {
            window.location.href = data['ReturnUrl'];
            return;
        }
    }
    var jSelect = $('.updatetimesheets');
    jSelect.val('');
    TSM_ConfirmPeriod(null, true);
    history.back();
    $.mobile.changePage('#periodselection', { changeHash: true });
    $('.updatetimesheets').trigger('change');
}

$(document).ready(function () {
    TSM_formDirty(false);
   
    $('.leaveApplication').attr('data-button-application', 'timesheet');
    $('#timesheet').bind('leaveApplication', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#timesheet').attr('data-leave-application'))) data.cancel = true;
    });
    $('#updatesummary').bind('goto', function (e, data) {
        TSM_CopySummary(false);
    });
    $('#taskselection').bind('goto', function (e, data) {
        TSM_ConfirmAdd();
        TSM_PrepareRowType(data.action);
    })
        .bind('confirm', function (e, data) {
            TSM_startAddRow(data.action);
        }
        )
        .bind('cancel', function () {

        }
        );
    $('#periodselection').bind('gotoing', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#timesheet').attr('data-leave-application'))) data.cancel = true;
    })
    .bind('confirm', function () {
        TSM_ConfirmPeriod(null, null);
    });
    $('#viewselection').bind('gotoing', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#timesheet').attr('data-leave-application'))) data.cancel = true;
    })
    .bind('confirm', function () {
        TSM_ConfirmView();
    });
    $('#recalldelete').bind('goto', function () {

    })
    .bind('confirm', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#recalldelete').attr('data-leave-application'))) data.cancel = true;
        jThis = $(data.action);
        var target = $('#' + jThis.attr('data-action-container'));
        target.val(jThis.attr('data-action-value'));

        TSM_jsonSubmit($('#rDRequest'));
    })


    .bind('confirming', function (e, data) {

    })
    .bind('cancel', function () {

    }
    );

});
    