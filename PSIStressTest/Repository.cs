using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Configuration;
using System.ServiceModel;
using System.Security.Principal;
using WCFHelpers;
using SvcTimeSheet;
using System.Web.Services.Protocols;
using SvcAdmin;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.Globalization;
using TimeSheetBusiness;

namespace PSIStressTest
{
    public class Repository
    {
         // WCF endpoint names in app.config.
        private const string ENDPOINT_ADMIN = "basicHttp_Admin"; 
        private const string ENDPOINT_Q = "basicHttp_QueueSystem";
        private const string ENDPOINT_RES = "basicHttp_Resource";
        private const string ENDPOINT_PROJ = "basicHttp_Project";
        private const string ENDPOINT_LUT = "basicHttp_LookupTable";
        private const string ENDPOINT_CF = "basicHttp_CustomFields";
        private const string ENDPOINT_CAL = "basicHttp_Calendar";
        private const string ENDPOINT_AR = "basicHttp_Archive";
        private const string ENDPOINT_PWA = "basicHttp_PWA";
        private const int NO_QUEUE_MESSAGE = -1;

        public  SvcAdmin.AdminClient adminClient;
        public  SvcQueueSystem.QueueSystemClient queueSystemClient;
        public  SvcResource.ResourceClient resourceClient;
        public  SvcProject.ProjectClient projectClient;
        public  SvcLookupTable.LookupTableClient lookupTableClient;
        public  SvcCustomFields.CustomFieldsClient customFieldsClient;
        public  SvcCalendar.CalendarClient calendarClient;
        public  SvcArchive.ArchiveClient archiveClient;
        public  SvcPWA.PWAClient pwaClient;
        public SvcStatusing.StatusingClient statusingClient;
        public TimeSheetClient timesheetClient;
        public SvcSecurity.SecurityClient securityClient;

        Guid lineclassification;

        MySettings mySettings = new MySettings();
        private static SvcLoginWindows.LoginWindows loginWindows; 
             public string projectServerUrl = "http://intranet.contoso.com/projectserver1";
        public  string userName = "";
        public  string password = "";
        public  bool isWindowsAuth = true;
        
        public  bool useDefaultWindowsCredentials = true; // Currently must be true for Windows authentication in ProjTool.
        public  int windowsPort = 80;
        public  int formsPort = 81;
        public  bool waitForQueue = true;
        public  bool waitForIndividualQueue = false;
        public  bool autoLogin = false;

        public  Guid pwaSiteId = Guid.Empty;
        public  Guid jobGuid;
        public  Guid projectGuid = new Guid();

        public  int loginStatus = 0;
        public  bool isImpersonated = false;
        public  string impersonatedUserName = "";

             public void Setup()
        {
            try
            {
                WcfHelpers.ClearImpersonationContext();
                DisposeClients();
            }
            catch (System.Exception ex)
            {
            }
            if (!P14Login())
            {
                throw new Exception("Logon failed for current user");
            }

        }
        public void DisposeClients()
        {
            //adminClient.Close();
            //queueSystemClient.Close();
            //resourceClient.Close();
            //projectClient.Close();
            //lookupTableClient.Close();
            //customFieldsClient.Close();
            //calendarClient.Close();
            //archiveClient.Close();
            //pwaClient.Close();
        }

        public bool P14Login()
        {
            bool endPointError = false;
            bool result = false;

            try
            {
                projectServerUrl = projectServerUrl.Trim();

                if (!projectServerUrl.EndsWith("/"))
                {
                    projectServerUrl = projectServerUrl + "/";
                }
                String baseUrl = projectServerUrl;

                // Configure the WCF endpoints of PSI services used in ProjTool, before logging on.
                if (mySettings.UseAppConfig)
                {
                    endPointError = !ConfigClientEndpoints();
                }
                else
                {
                    endPointError = !SetClientEndpointsProg(baseUrl);
                }

                if (endPointError) return false;

                // NOTE: Windows logon with the default Windows credentials, Forms logon, and impersonation work in ProjTool. 
                // Windows logon without the default Windows credentials does not currently work.
                if (!isImpersonated)
                {
                    if (isWindowsAuth)
                    {
                        if (useDefaultWindowsCredentials)
                        {
                            result = true;
                        }
                        else
                        {
                            String[] splits = userName.Split('\\');

                            if (splits.Length != 2)
                            {
                                String errorMessage = "User name must be in the format domainname\\accountname";
                                result = false;
                            }
                            else
                            {
                                // Order of strings returned by String.Split is not deterministic
                                // Hence we cannot use splits[0] and splits[1] to obtain domain name and user name

                                int positionOfBackslash = userName.IndexOf('\\');
                                String windowsDomainName = userName.Substring(0, positionOfBackslash);
                                String windowsUserName = userName.Substring(positionOfBackslash + 1);

                                loginWindows = new SvcLoginWindows.LoginWindows();
                                loginWindows.Url = baseUrl + "_vti_bin/PSI/LoginWindows.asmx";
                                loginWindows.Credentials = new NetworkCredential(windowsUserName, password, windowsDomainName);

                                result = loginWindows.Login();
                            }
                        }
                    }
                    else
                    {
                        // Forms authentication requires the Authentication web service in Microsoft SharePoint Foundation.
                        result = WcfHelpers.LogonWithMsf(userName, password, new Uri(baseUrl));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Guid GetResourceUidFromNtAccount(String ntAccount)
        {
            //ntAccount = "i:0#.w|" + ntAccount;
            SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();

            Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
            filter.FilterTableName = rds.Resources.TableName;


            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField1 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.WRES_ACCOUNTColumn.ColumnName);
            filter.Fields.Add(ntAccountField1);

            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField2 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.RES_IS_WINDOWS_USERColumn.ColumnName);
            filter.Fields.Add(ntAccountField2);

            Microsoft.Office.Project.Server.Library.Filter.FieldOperator op = new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                rds.Resources.WRES_ACCOUNTColumn.ColumnName, ntAccount);
            filter.Criteria = op;




            rds = resourceClient.ReadResources(filter.GetXml(), false);

            
            var obj = (Guid)rds.Resources.Rows[0]["RES_UID"];
            return obj;
        }

        public void SetImpersonation(string impersonatedUser)
        {
            Guid trackingGuid = Guid.NewGuid();
            bool isWindowsUser = true;
            Guid siteId = Guid.Empty;           // Project Web App site ID.
            CultureInfo languageCulture = null; // The language culture is not used.
            CultureInfo localeCulture = null;   // The locale culture is not used.
            Guid resourceGuid = GetResourceUidFromNtAccount(impersonatedUser);
            WcfHelpers.SetImpersonationContext(isWindowsUser, impersonatedUser, resourceGuid, trackingGuid, siteId,
                                               languageCulture, localeCulture);
            WCFHelpers.WcfHelpers.UseCorrectHeaders(true);
        }
        // Set the PSI client endpoints programmatically; don't use app.config.
        public bool SetClientEndpointsProg(string pwaUrl)
        {
            const int MAXSIZE = 500000000;
            const string SVC_ROUTER = "_vti_bin/PSI/ProjectServer.svc";

            bool isHttps = pwaUrl.ToLower().StartsWith("https");
            bool result = true;
            BasicHttpBinding binding = null;

            try
            {
                if (isHttps)
                {
                    // Create a binding for HTTPS.
                    binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                }
                else
                {
                    // Create a binding for HTTP.
                    binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                }

                binding.Name = "basicHttpConf";
                binding.SendTimeout = TimeSpan.MaxValue;
                binding.MaxReceivedMessageSize = MAXSIZE;
                binding.ReaderQuotas.MaxNameTableCharCount = MAXSIZE;
                binding.MessageEncoding = WSMessageEncoding.Text;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

                // The endpoint address is the ProjectServer.svc router for all public PSI calls.
                EndpointAddress address = new EndpointAddress(pwaUrl + SVC_ROUTER);

                adminClient = new SvcAdmin.AdminClient(binding, address);
                adminClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                adminClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                securityClient = new SvcSecurity.SecurityClient(binding, address);
                securityClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                securityClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                timesheetClient = new SvcTimeSheet.TimeSheetClient(binding, address);
                timesheetClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                timesheetClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                projectClient = new SvcProject.ProjectClient(binding, address);
                projectClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                projectClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                queueSystemClient = new SvcQueueSystem.QueueSystemClient(binding, address);
                queueSystemClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                queueSystemClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                resourceClient = new SvcResource.ResourceClient(binding, address);
                resourceClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                resourceClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                lookupTableClient = new SvcLookupTable.LookupTableClient(binding, address);
                lookupTableClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                lookupTableClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                customFieldsClient = new SvcCustomFields.CustomFieldsClient(binding, address);
                customFieldsClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                customFieldsClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                calendarClient = new SvcCalendar.CalendarClient(binding, address);
                calendarClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                calendarClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                archiveClient = new SvcArchive.ArchiveClient(binding, address);
                archiveClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                archiveClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                pwaClient = new SvcPWA.PWAClient(binding, address);
                pwaClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                pwaClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                statusingClient = new SvcStatusing.StatusingClient(binding, address);
                statusingClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                statusingClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;
                lineclassification = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled).LineClasses.First().TS_LINE_CLASS_UID;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        // Configure the PSI client endpoints by using the settings in app.config.
        public  bool ConfigClientEndpoints()
        {
            bool result = true;

            string[] endpoints = { ENDPOINT_ADMIN, ENDPOINT_Q, ENDPOINT_RES, ENDPOINT_PROJ, 
                                   ENDPOINT_LUT, ENDPOINT_CF, ENDPOINT_CAL, ENDPOINT_AR, 
                                   ENDPOINT_PWA };
            try
            {
                foreach (string endPt in endpoints)
                {
                    switch (endPt)
                    {
                        case ENDPOINT_ADMIN:
                            adminClient = new SvcAdmin.AdminClient(endPt);
                            break;
                        case ENDPOINT_PROJ:
                            projectClient = new SvcProject.ProjectClient(endPt);
                            break;
                        case ENDPOINT_Q:
                            queueSystemClient = new SvcQueueSystem.QueueSystemClient(endPt);
                            break;
                        case ENDPOINT_RES:
                            resourceClient = new SvcResource.ResourceClient(endPt);
                            break;
                        case ENDPOINT_LUT:
                            lookupTableClient = new SvcLookupTable.LookupTableClient(endPt);
                            break;
                        case ENDPOINT_CF:
                            customFieldsClient = new SvcCustomFields.CustomFieldsClient(endPt);
                            break;
                        case ENDPOINT_CAL:
                            calendarClient = new SvcCalendar.CalendarClient(endPt);
                            break;
                        case ENDPOINT_AR:
                            archiveClient = new SvcArchive.ArchiveClient(endPt);
                            break;
                        case ENDPOINT_PWA:
                            pwaClient = new SvcPWA.PWAClient(endPt);
                            break;
                        default:
                            result = false;
                            Console.WriteLine("Invalid endpoint: {0}", endPt);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public void createRow(string user, ref SvcTimeSheet.TimesheetDataSet _tsDS, 
            SvcResource.ResourceAssignmentDataSet _resAssDS, 
            SvcTimeSheet.TimesheetDataSet.LinesRow line, DateTime Start, DateTime Stop)
        {
            string assignementId, projectId,  projectName;
            bool isAdmin;
            if (_resAssDS.ResourceAssignment.Count > 0)
            {
                assignementId = _resAssDS.ResourceAssignment[0].ASSN_UID.ToString();
                projectId = _resAssDS.ResourceAssignment[0].PROJ_UID.ToString();
                projectName = _resAssDS.ResourceAssignment[0].PROJ_NAME;
                isAdmin = false;
            }
            else
            {
                var tslineclassDS = adminClient.ReadLineClasses(SvcAdmin.LineClassType.AllNonProject, SvcAdmin.LineClassState.Enabled);
                 assignementId = tslineclassDS.LineClasses[0].TS_LINE_CLASS_UID.ToString();
                 projectId = "";
                 projectName = tslineclassDS.LineClasses[0].TS_LINE_CLASS_NAME.ToString();
                isAdmin = true;
            }
            //if(string.IsNullOrEmpty(assignementId))
            //{
            //    var projectDataSet  = projectClient.ReadProject(new Guid(projectId),
            //}

            if (line == null)//creation
            {
                try
                {
                    SvcAdmin.TimesheetLineClassDataSet tsLineClassDs;

                    tsLineClassDs = new SvcAdmin.TimesheetLineClassDataSet();
                    tsLineClassDs = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled);


                    Guid timeSheetUID = new Guid(_tsDS.Headers[0].TS_UID.ToString());



                    line = _tsDS.Lines.NewLinesRow();  //Create a new row for the timesheet

                    line.TS_UID = timeSheetUID;
                    line.ASSN_UID = new Guid(assignementId);

                    //try if this works, may be we need it when reading the rows; Francesco
                    line.TS_LINE_UID = Guid.NewGuid();
                    line.TS_LINE_COMMENT = "";


                    if (isAdmin)
                    {
                        line.TS_LINE_CLASS_UID = new Guid(assignementId);
                        line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.NotApplicable;
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Unverified;
                        SvcAdmin.TimesheetLineClassDataSet.LineClassesRow foundTSClassRow;
                        foundTSClassRow = tsLineClassDs.LineClasses.FindByTS_LINE_CLASS_UID(new Guid(assignementId));
                        line.TS_LINE_CACHED_ASSIGN_NAME = foundTSClassRow.TS_LINE_CLASS_NAME;
                    }
                    else
                    {

                        if (GetResourceUidFromNtAccount(user) == GetTimesheetMgrUID(user))
                        {
                            line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.Approved;
                        }
                        else
                        {
                            line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.PendingApproval;
                        }
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CLASS_UID = lineclassification;
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CACHED_ASSIGN_NAME = tsLineClassDs.LineClasses[0].TS_LINE_CLASS_DESC;


                        if (!(_resAssDS.ResourceAssignment.Any(t => t.ASSN_UID == line.ASSN_UID)))
                        {
                            line.TS_LINE_VALIDATION_TYPE = (int)Microsoft.Office.Project.Server.Library.TimesheetEnum.ValidationType.ProjectLevel;
                            line.TASK_UID = Guid.NewGuid();
                            line.PROJ_UID = new Guid(projectId);
                            line.TS_LINE_CACHED_PROJ_NAME = projectName;
                            line.TS_LINE_CACHED_ASSIGN_NAME = "Top Level";
                        }
                        else
                        {
                            line.TASK_UID = GetTaskUID(line.ASSN_UID, _resAssDS);
                        }

                    }

                    _tsDS.Lines.AddLinesRow(line);  //add new row to the timesheet dataset

                    Guid[] uids = new Guid[] { line.TS_LINE_UID };

                    using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                    {
                        SetImpersonation(user);
                        timesheetClient.PrepareTimesheetLine(timeSheetUID, ref _tsDS, uids);  //Validates and populates a timesheet line item and preloads actuals table in the dataset
                    }

                    CreateActuals(_tsDS, line, Start, Stop);


                }
                catch (Exception e)
                {
                    
                    return;

                }
            }
        }

        public  void CreateActuals(SvcTimeSheet.TimesheetDataSet _tsDS, SvcTimeSheet.TimesheetDataSet.LinesRow lineRow, DateTime Start, DateTime Stop)
        {
            DateTime day = Start;
            while (day <= Stop)
            {
                SvcTimeSheet.TimesheetDataSet.ActualsRow actualsRow = _tsDS.Actuals.NewActualsRow();
                actualsRow.TS_LINE_UID = lineRow.TS_LINE_UID;

                actualsRow.TS_ACT_START_DATE = day;
                actualsRow.TS_ACT_FINISH_DATE = day.AddDays(1);
                _tsDS.Actuals.AddActualsRow(actualsRow);
                day = day.AddDays(1);
            }
        }
        public void CreateTimesheet(string user, Guid ruid, Guid periodUID, ref SvcTimeSheet.TimesheetDataSet tsDs)
        {
            tsDs = new SvcTimeSheet.TimesheetDataSet();
            SvcTimeSheet.TimesheetDataSet.HeadersRow headersRow = tsDs.Headers.NewHeadersRow();
            headersRow.RES_UID = ruid;  // cant be null.
            var tuid = Guid.NewGuid();
            headersRow.TS_UID = tuid;
            headersRow.WPRD_UID = periodUID;
            headersRow.TS_NAME = "Timesheet";
            headersRow.TS_COMMENTS = "";
            headersRow.TS_ENTRY_MODE_ENUM = (byte)PSLib.TimesheetEnum.EntryMode.Daily;
            tsDs.Headers.AddHeadersRow(headersRow);

            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(user);
                timesheetClient.CreateTimesheet(tsDs, SvcTimeSheet.PreloadType.Default);
            }
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(user);
                tsDs = timesheetClient.ReadTimesheet(tuid); //calling ReadTimesheet to pre populate with default server settings
            }
        }
        public void GetTimesheetAction(int status, out bool canDelete, out bool canRecall)
        {
            canDelete = (status == 0) || (status == 4) || (status == 2);
            canRecall = (status == 1) || (status == 3) || (status == 2);
            

        }

        public void RecallDelete(string user, string periodId, DateTime start, DateTime stop, bool isRecall)
        {

            Guid ruid = GetResourceUidFromNtAccount(user);
            Guid periodUID = new Guid(periodId);
            Guid tuid;
            SvcTimeSheet.TimesheetDataSet tsDs ;
           
            bool canDelete;
            bool canRecall;
           
                try
                {
                    Guid jobUID = Guid.NewGuid();
                    using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                    {
                        SetImpersonation(user);
                        tsDs = timesheetClient.ReadTimesheetByPeriod(ruid, periodUID, SvcTimeSheet.Navigation.Current);
                        timesheetClient.QueueRecallTimesheet(jobUID, tsDs.Headers[0].TS_UID);
                    }
                    bool res = QueueHelper.WaitForQueueJobCompletion(this, jobUID, (int)SvcQueueSystem.QueueMsgType.TimesheetRecall, queueSystemClient);


                }
                catch
                {

                }
            
        }
        public void SubmitTimesheet(string user, SvcTimeSheet.TimesheetDataSet tsDs)
        {
            try
            {
                Guid jobGuid = Guid.NewGuid();
                var tsGuid = (Guid)(tsDs.Headers[0].TS_UID);
                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(user);
                    timesheetClient.QueueSubmitTimesheet(jobGuid, tsGuid, GetTimesheetMgrUID(user), "Approved");
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetSubmit, queueSystemClient);
                if (!res) throw new Exception();
            }
            catch { throw new Exception(); }
        }

        public void SaveTimesheet(string user, SvcTimeSheet.TimesheetDataSet tsDs, Guid tsGuid)
        {

            try
            {
                Guid jobGuid = Guid.NewGuid();
                

                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(user);
                    var temp = tsDs.GetChanges();
                    timesheetClient.QueueUpdateTimesheet(jobGuid,
                         tsGuid,
                        (SvcTimeSheet.TimesheetDataSet)tsDs);  //Saves the specified timesheet data to the Published database
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetUpdate, queueSystemClient);
                if (!res) throw new Exception();
            }
            catch (Exception tex) { throw new Exception(); }
        }

        private static PSLib.Filter GetResourceAssignmentFilter(Guid[] resources)
        {
            SvcResource.ResourceAssignmentDataSet resourceAssignmentDs = new SvcResource.ResourceAssignmentDataSet();
            string foo = resourceAssignmentDs.GetXmlSchema();
            PSLib.Filter resourceFilter = new PSLib.Filter();
            resourceFilter.FilterTableName = resourceAssignmentDs.ResourceAssignment.TableName;
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.RES_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.RES_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));

            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.TASK_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.TASK_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.ASSN_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.PROJ_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.PROJ_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));

            List<PSLib.Filter.FieldOperator> resourceFieldOps = new List<PSLib.Filter.FieldOperator>();
            PSLib.Filter.IOperator[] fos = new PSLib.Filter.IOperator[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                fos[i] = new PSLib.Filter.FieldOperator(PSLib.Filter.FieldOperationType.Equal, resourceAssignmentDs.ResourceAssignment.RES_UIDColumn.ColumnName, resources[i]);
            }

            PSLib.Filter.LogicalOperator lo = new Microsoft.Office.Project.Server.Library.Filter.LogicalOperator(PSLib.Filter.LogicalOperationType.Or, fos);
            resourceFilter.Criteria = lo;
            return resourceFilter;
        }

        public  SvcResource.ResourceAssignmentDataSet GetResourceAssignmentDataSet(string user)
        {

            Guid[] resourceUids = new Guid[1];

            resourceUids[0] = GetResourceUidFromNtAccount(user);

            PSLib.Filter resourceAssignmentFilter = GetResourceAssignmentFilter(resourceUids);
            string resourceAssignmentFilterXml = resourceAssignmentFilter.GetXml();
            using (OperationContextScope scope = new OperationContextScope(resourceClient.InnerChannel))
            {
                SetImpersonation(user);
                return resourceClient.ReadResourceAssignments(resourceAssignmentFilterXml);
            }
        }
        

        public Guid GetTimesheetMgrUID(String ntAccount)
        {
            string ntAccountCopy = ntAccount;
            SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();

            Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
            filter.FilterTableName = rds.Resources.TableName;


            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField1 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.WRES_ACCOUNTColumn.ColumnName);
            filter.Fields.Add(ntAccountField1);

            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField2 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.RES_TIMESHEET_MGR_UIDColumn.ColumnName);
            filter.Fields.Add(ntAccountField2);

            Microsoft.Office.Project.Server.Library.Filter.FieldOperator op = new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                rds.Resources.WRES_ACCOUNTColumn.ColumnName, ntAccountCopy);
            filter.Criteria = op;


            rds = resourceClient.ReadResources(filter.GetXml(), false);
            if (rds.Resources[0].IsRES_TIMESHEET_MGR_UIDNull())
            {
                return rds.Resources[0].RES_UID;
            }
            var obj = rds.Resources[0].RES_TIMESHEET_MGR_UID;
            return obj;
        }

        private Guid GetTaskUID(Guid assn_uid, SvcResource.ResourceAssignmentDataSet _resAssDS)
        {
            string expression = "ASSN_UID = '" + assn_uid + "'";
            //SvcTimeSheet.TimesheetDataSet.LinesRow[] lines = (SvcTimeSheet.TimesheetDataSet.LinesRow[])_tsDS.Lines.Select(expression);
            //DataRow[] lines = (DataRow[])

            SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[] lines = (SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[])_resAssDS.ResourceAssignment.Select(expression);

            return new Guid(lines[0].TASK_UID.ToString());

        }
        }

    sealed class MySettings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("http://LocalHost/PWA/")]
        public string ProjectServerURL
        {
            get { return (string)this["ProjectServerURL"]; }
            set { this["ProjectServerURL"] = value; }
        }
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("FormsAdmin")]
        public string UserName
        {
            get { return (string)this["UserName"]; }
            set { this["UserName"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("pass@word1")]
        public string PassWord
        {
            get { return (string)this["PassWord"]; }
            set { this["PassWord"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("true")]
        public bool IsWindowsAuth
        {
            get { return (bool)this["IsWindowsAuth"]; }
            set { this["IsWindowsAuth"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("true")]
        public bool UseDefaultWindowsCredentials
        {
            get { return (bool)this["UseDefaultWindowsCredentials"]; }
            set { this["UseDefaultWindowsCredentials"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("80")]
        public int WindowsPort
        {
            get { return (int)this["WindowsPort"]; }
            set { this["WindowsPort"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("81")]
        public int FormsPort
        {
            get { return (int)this["FormsPort"]; }
            set { this["FormsPort"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("true")]
        public bool WaitForQueue
        {
            get { return (bool)this["WaitForQueue"]; }
            set { this["WaitForQueue"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool WaitForIndividualQueue
        {
            get { return (bool)this["WaitForIndividualQueue"]; }
            set { this["WaitForIndividualQueue"] = value; }
        }

        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool AutoLogin
        {
            get { return (bool)this["AutoLogin"]; }
            set { this["AutoLogin"] = value; }
        }
        [UserScopedSetting()]
        [DefaultSettingValueAttribute("false")]
        public bool UseAppConfig
        {
            get { return (bool)this["UseAppConfig"]; }
            set { this["UseAppConfig"] = value; }
        }
    }

    public class LangItem
    {
        int lcid; string langName;
        public int LCID
        {
            get { return lcid; }
            set { lcid = value; }
        }
        public string LangName
        {
            get { return langName; }
            set { langName = value; }
        }
        public LangItem(int Lcid, string name)
        { LCID = Lcid; LangName = name; }
    }
    }

