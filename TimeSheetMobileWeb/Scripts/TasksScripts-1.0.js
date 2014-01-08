function timesheetsSubmittedOk() {
    TSM_formDirty(false);
    TSM_UpdateMainLayout();
    TSM_CopySummary(true);
    OpenDialog('#updaterecallsummary');
}
$(document).ready(function () {
    TSM_formDirty(false);
    $('.leaveApplication').attr('data-button-application', 'tasks');
    $('#tasks').bind('leaveApplication', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#tasks').attr('data-leave-application'))) data.cancel = true;
    });
    $('#updatesummary').bind('goto', function (e, data) {
        TSM_CopySummary(true);
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
        if (TSM_isFormDirty() && !window.confirm($('#tasks').attr('data-leave-application'))) data.cancel = true;
    })
    .bind('confirm', function () {
        TSM_ConfirmPeriod(null,null);
    });
    $('#viewselection').bind('gotoing', function (e, data) {
        if (TSM_isFormDirty() && !window.confirm($('#tasks').attr('data-leave-application'))) data.cancel = true;
    })
    .bind('confirm', function () {
        TSM_ConfirmView();
    });
});