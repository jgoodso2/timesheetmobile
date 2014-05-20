using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PSIStressTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                throw new Exception("Please specify pwa url as command line argument");
            }
            
            Repository repository = new Repository();
            repository.SetClientEndpointsProg(args[0]);
            var resources = repository.resourceClient.ReadResources(null, false).Resources;


            Parallel.ForEach(resources, resource =>
                {
                    try
                    {
                        var periods = repository.adminClient.ReadPeriods(SvcAdmin.PeriodState.All).TimePeriods.OrderByDescending(t => t.WPRD_START_DATE).Take(20);
                        Parallel.ForEach(periods, period =>
                        {
                            try
                            {
                                Console.WriteLine(string.Format("generating timesheet for {0} for ({1} - {2})", resource.RES_NAME, period.WPRD_START_DATE, period.WPRD_FINISH_DATE));

                                using (OperationContextScope scope = new OperationContextScope(repository.timesheetClient.InnerChannel))
                                {
                                    repository.SetImpersonation(resource.WRES_ACCOUNT);
                                    var timesheet = repository.timesheetClient.ReadTimesheetByPeriod(resource.RES_UID,
                                        period.WPRD_UID, SvcTimeSheet.Navigation.Current);
                                    var resDS = repository.GetResourceAssignmentDataSet(resource.WRES_ACCOUNT);
                                    if (timesheet.Headers.Rows.Count > 0)
                                    {
                                        int Status = (int)timesheet.Headers[0].TS_STATUS_ENUM;
                                        if (Status == 1 || Status == 3 || Status == 5)
                                        {
                                            repository.RecallDelete(resource.WRES_ACCOUNT, period.WPRD_UID.ToString(), period.WPRD_START_DATE, period.WPRD_FINISH_DATE, true);
                                        }
                                        else
                                        {
                                            UpdateTimesheet(timesheet, repository, resource, period, resDS, true);
                                        }
                                    }
                                    else
                                    {
                                        repository.CreateTimesheet(resource.WRES_ACCOUNT, resource.RES_UID, period.WPRD_UID, ref timesheet);
                                        UpdateTimesheet(timesheet, repository, resource, period, resDS, true);
                                    }

                                }
                            }
                            catch
                            {
                                
                            }
                        });
                    }
                    catch
                    {
                       
                    }
                });

            Console.WriteLine("All Timesheets generated");
        }

        private static void UpdateTimesheet(SvcTimeSheet.TimesheetDataSet timesheet,Repository repository, 
            SvcResource.ResourceDataSet.ResourcesRow resource, SvcAdmin.TimePeriodDataSet.TimePeriodsRow period, 
            SvcResource.ResourceAssignmentDataSet resDS, bool save)
        {
            if (timesheet.Lines.Count > 0 && !save)
            {
               
            }
            else
            {

                repository.createRow(resource.WRES_ACCOUNT, ref timesheet, resDS, null, period.WPRD_START_DATE, period.WPRD_FINISH_DATE);
                if (timesheet.Lines.Count > 0)
                {
                    if (timesheet.Lines[0].GetActualsRows().Count() == 0)
                    {
                        repository.CreateActuals(timesheet, timesheet.Lines[0], period.WPRD_START_DATE, period.WPRD_FINISH_DATE);
                    }
                    timesheet.Lines[0].GetActualsRows()[0].TS_ACT_VALUE = (8 * 60000);
                }
            }
            if (save)
            {
                repository.SaveTimesheet(resource.WRES_ACCOUNT, timesheet, timesheet.Headers[0].TS_UID);
            }
            repository.SubmitTimesheet(resource.WRES_ACCOUNT, timesheet);
        }


    }
}