using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCControlsToolkit.Controller;
using System.Security.Principal;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public static class ErrorHandlingHelpers
    {
        public static void UpdateRows(bool isApprovalMode,IRepository rep, UpdateViewBase model, string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit)
        {
            try
            {
                rep.UpdateRows(isApprovalMode,user, configuration, periodId, start, stop, rows, submit);
            }
            catch (UpdateException uex)
            {
                if (uex is TimesheetSubmitException)
                {
                    model.ErrorMessage = SiteResources.TimesheetSubmitError;
                }
                else if (uex is TimesheetUpdateException)
                {
                    model.ErrorMessage = SiteResources.TimesheetUpdateError;
                }
                else if (uex is StatusSubmitException)
                {
                    model.ErrorMessage = SiteResources.StatusSubmitError;
                }
                else
                {
                    model.ErrorMessage = SiteResources.StatusUpdateError;
                }
                return;
            }
            catch 
            {
                model.ErrorMessage = SiteResources.GenericUpdateError;
                return;
            }
            model.ErrorMessage = SiteResources.UpdateSuccesfull;
        }
    }
}