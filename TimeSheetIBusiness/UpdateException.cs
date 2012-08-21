using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheetIBusiness
{
    public class UpdateException:Exception
    {
    }
    public class TimesheetUpdateException : UpdateException
    {
    }
    public class TimesheetSubmitException : UpdateException
    {
    }
    public class StatusUpdateException : UpdateException
    {
    }
    public class StatusSubmitException : UpdateException
    {
    }
}
