using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCFHelpers;
using System.Net;
using System.Configuration;
using System.ServiceModel;
using System.Security.Principal;
using SvcTimeSheet;
using System.Web.Services.Protocols;
using SvcAdmin;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.Globalization;
namespace PSITest
{
    [TestClass]
    public class UnitTest1
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

        MySettings mySettings = new MySettings();
        private static SvcLoginWindows.LoginWindows loginWindows;  


        //public static SvcLoginForms.LoginForms loginForms = new SvcLoginForms.LoginForms();
        //public static SvcLoginWindows.LoginWindows loginWindows = new SvcLoginWindows.LoginWindows();

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

        [TestInitialize]
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
                Assert.Fail("Logon failed for current user");
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

        public Guid GetResourceUidFromNtAccount(String ntAccount, out bool isWindowsUser)
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

            isWindowsUser = rds.Resources[0].RES_IS_WINDOWS_USER;
            var obj = (Guid)rds.Resources.Rows[0]["RES_UID"];
            return obj;
        }

        [Ignore]
        public void CreateDelegatedTimesheet()
        {
            //Surrogate Timesheets are deprecated from project server 2010
            //Try creating Surrogate timesheet with delegation

            try
            {
                bool res;
                //get adamb
                Guid myUid = GetResourceUidFromNtAccount("CONTOSO\\adamb",out res);
                // get current resource
                var curres = resourceClient.GetCurrentUserUid();
                
                //Delegation is already set up in pwa outside the appplication
                //read the first delegation which turns out to be admin for adamb-- (myuid = admab)
               // var rdelegation = resourceClient.ReadDelegations(SvcResource.DelegationFilter.All, myUid);
                //pwaClient.UserDelegationActivateDelegation(rdelegation.ResourceDelegations[0].DELEGATION_UID);
            #region Read Timesheet
            // Time periods must be created by the admin to use timesheets.
            // We are just reading the first open period here.
            TimePeriodDataSet timeperiodDs = adminClient.ReadPeriods(PeriodState.Open);
            Guid periodUid = timeperiodDs.TimePeriods[0].WPRD_UID;

            // If the timesheet already exists, read it.
            // (To delete an existing unsubmitted timesheet,
            //   go to the My Timesheet area of the Project Web App site.)
            var timesheetDs = timesheetClient.ReadTimesheetByPeriod(myUid,periodUid,Navigation.Current);
            #endregion
            #region CreateTimesheet if it doesn't exist, then read it
            // If the timesheet does not exist, create it.
            if(timesheetDs.Headers.Count<1)
            {
               timesheetDs = new TimesheetDataSet();
               TimesheetDataSet.HeadersRow headersRow = timesheetDs.Headers.NewHeadersRow();
               headersRow.RES_UID = myUid;
               headersRow.TS_UID = Guid.NewGuid();
               headersRow.WPRD_UID = periodUid;
               headersRow.TS_CREATOR_RES_UID = resourceClient.GetCurrentUserUid();
               headersRow.TS_NAME = "Timesheet ";
               headersRow.TS_COMMENTS = "Random comment text here";
               headersRow.TS_ENTRY_MODE_ENUM = (byte) PSLib.TimesheetEnum.EntryMode.Weekly;
               headersRow.TS_IS_CONTROLLED_BY_OWNER = false;
               timesheetDs.Headers.AddHeadersRow(headersRow);

               // Create the timesheet with the default line types that are specified by the admin.
               timesheetClient.CreateTimesheet(timesheetDs, PreloadType.Default);
               timesheetDs = timesheetClient.ReadTimesheet(headersRow.TS_UID);
            }
            #endregion
         }
         catch (SoapException ex)
         {
           
         }
         catch (WebException ex)
         {
            
         }
         catch (Exception ex)
         {
            
         }
         finally
         {
            
         }

        }

        [TestMethod]
        public void Verify_TaskManagerEnabled_NoReqdLineApproval_NoSingleEntryMode()
        {
            using (OperationContextScope scope = new OperationContextScope(statusingClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\NISHANT");
                var ds  = statusingClient.ReadStatusApprovalsSubmitted(true);
            }
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\ADMINISTRATOR");
                var ds = timesheetClient.ReadTimesheetsPendingApproval(new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), null);
            }
        }

        [TestMethod]
        public void Verify_TaskManagerEnabled_NoReqdLineApproval_SingleEntryMode()
        {
            using (OperationContextScope scope = new OperationContextScope(statusingClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\NISHANT");
                var ds = statusingClient.ReadStatusApprovalsSubmitted(true);
            }
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\ADMINISTRATOR");
                var ds = timesheetClient.ReadTimesheetsPendingApproval(new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), null);
            }
        }

        [TestMethod]
        public void Verify_TaskManagerEnabled_ReqdLineApproval_SingleEntryMode()
        {
            using (OperationContextScope scope = new OperationContextScope(statusingClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\NISHANT");
                var ds = statusingClient.ReadStatusApprovalsSubmitted(true);
            }
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\ADMINISTRATOR");
                var ds = timesheetClient.ReadTimesheetsPendingApproval(new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), null);

            }
        }

        [TestMethod]
        public void ApproveTimesheet()
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation("CONTOSO\\ADMINISTRATOR");
                var ds = timesheetClient.ReadTimesheetsPendingApproval(new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), null);
                var tds = timesheetClient.ReadTimesheet(ds.Timesheets[0].TS_UID);
                bool isWindowsUser;
                timesheetClient.QueueReviewTimesheet(Guid.NewGuid(), tds.Headers[0].TS_UID, 
                    GetResourceUidFromNtAccount("CONTOSO\\ADMINISTRATOR", out isWindowsUser) 
                    //Guid.Empty
                    ,"Approved",SvcTimeSheet.Action.Approve);
                //timesheetClient.ApproveProjectTimesheetLines(timesheetClient.ReadTimesheet(ds.Timesheets[0].TS_UID).Lines.Where(t=>t.TS_LINE_VALIDATION_TYPE == 1).Select(t => t.TS_LINE_UID).ToArray(), null, "Approved by unit test");
                
            }
        }


        public void SetImpersonation(string impersonatedUser)
        {
            Guid trackingGuid = Guid.NewGuid();
            bool isWindowsUser;
            Guid siteId = Guid.Empty;           // Project Web App site ID.
            CultureInfo languageCulture = null; // The language culture is not used.
            CultureInfo localeCulture = null;   // The locale culture is not used.
            Guid resourceGuid = GetResourceUidFromNtAccount(impersonatedUser, out isWindowsUser);
            WcfHelpers.SetImpersonationContext(isWindowsUser, impersonatedUser, resourceGuid, trackingGuid, siteId,
                                               languageCulture, localeCulture);
            WCFHelpers.WcfHelpers.UseCorrectHeaders(true);
        }
        // Set the PSI client endpoints programmatically; don't use app.config.
        private bool SetClientEndpointsProg(string pwaUrl)
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

