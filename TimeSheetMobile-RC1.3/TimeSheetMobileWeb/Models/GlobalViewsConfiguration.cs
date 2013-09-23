using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{    
    [Serializable]
    public class GlobalViewsConfiguration
    {
        public ViewConfigurationRow[] RowsConfiguration;
        public ViewConfigurationTask[] TasksConfiguration;
        public string TaskUpdatorViewField { get; set; }
        public string TimesheetViewField { get; set; }
        public void Save(string filename = null)
        {
            if (filename == null) filename=HttpContext.Current.Server.MapPath("~/ViewsConfiguration.config");
            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            if (stream != null)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GlobalViewsConfiguration));
                    serializer.Serialize(stream, this);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
        public static void Load(string filename=null)
        {
            if (filename == null) filename = HttpContext.Current.Server.MapPath("~/ViewsConfiguration.config");
            XmlSerializer serializer = new XmlSerializer(typeof(GlobalViewsConfiguration));
            FileStream stream =new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream != null)
            {
                try
                {
                    GlobalViewsConfiguration res = serializer.Deserialize(stream) as GlobalViewsConfiguration;
                    ViewConfigurationTask.All = res.TasksConfiguration;
                    ViewConfigurationRow.All = res.RowsConfiguration;
                    if (!string.IsNullOrWhiteSpace(res.TaskUpdatorViewField)) ViewConfigurationTask.ViewFieldName = res.TaskUpdatorViewField;
                    ViewConfigurationTask.Default = res.TasksConfiguration[0];
                    if (!string.IsNullOrWhiteSpace(res.TimesheetViewField)) ViewConfigurationRow.ViewFieldName = res.TimesheetViewField;
                    ViewConfigurationRow.Default = res.RowsConfiguration[0];
                }
                finally
                {
                    stream.Close();
                }
                
            }
        }
        public static void Test()
        {
            GlobalViewsConfiguration res = new GlobalViewsConfiguration();
            res.RowsConfiguration = new ViewConfigurationRow[]
            {
                new ViewConfigurationRow()
                
            };
            res.TasksConfiguration = new ViewConfigurationTask[]
            {
                new ViewConfigurationTask()
                
            };
            res.Save();
        }

    }
}