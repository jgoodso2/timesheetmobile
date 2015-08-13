﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheetIBusiness;
using System.Security.Principal;
using MVCControlsToolkit.Controller;
using System.Data;
using SvcProject;
using SvcQueueSystem;
using PSLib = Microsoft.Office.Project.Server.Library;
using System.ServiceModel;
using System.Xml;


namespace TimeSheetBusiness
{
    public  static class QueueHelper
    {
        private static List<int> CheckStatusRowErrors(string errorInfo)
        {
            List<int> errorList = new List<int>();
            bool containsError = false;

            XmlTextReader xReader = new XmlTextReader(new System.IO.StringReader(errorInfo));
            while (xReader.Read())
            {
                if (xReader.Name == "errinfo" && xReader.NodeType == XmlNodeType.Element)
                {
                    xReader.Read();
                    if (xReader.Value != string.Empty)
                    {
                        containsError = true;
                    }
                }
                if (containsError && xReader.Name == "error" && xReader.NodeType == XmlNodeType.Element)
                {
                    while (xReader.Read())
                    {
                        if (xReader.Name == "id" && xReader.NodeType == XmlNodeType.Attribute)
                        {
                            errorList.Add(Convert.ToInt32(xReader.Value));
                        }
                    }
                }
            }
            return errorList;
        }
        public static bool WaitForQueueJobCompletion(IRepository repository, Guid trackingGuid, int messageType, SvcQueueSystem.QueueSystemClient queueSystemClient)
        {
            //System.Threading.Thread.Sleep(2000);
            SvcQueueSystem.QueueStatusDataSet queueStatusDataSet = new SvcQueueSystem.QueueStatusDataSet();
            SvcQueueSystem.QueueStatusRequestDataSet queueStatusRequestDataSet =
                new SvcQueueSystem.QueueStatusRequestDataSet();

            SvcQueueSystem.QueueStatusRequestDataSet.StatusRequestRow statusRequestRow =
                queueStatusRequestDataSet.StatusRequest.NewStatusRequestRow();
            statusRequestRow.JobGUID = trackingGuid; //Guid.NewGuid();  
            statusRequestRow.JobGroupGUID = Guid.NewGuid();
            statusRequestRow.MessageType = messageType;
            queueStatusRequestDataSet.StatusRequest.AddStatusRequestRow(statusRequestRow);

            bool inProcess = true;
            bool result = false;
            DateTime startTime = DateTime.Now;
            int successState = (int)SvcQueueSystem.JobState.Success;
            int failedState = (int)SvcQueueSystem.JobState.Failed;
            int blockedState = (int)SvcQueueSystem.JobState.CorrelationBlocked;
             DateTime toDate = DateTime.Now + new TimeSpan(1, 0, 0, 0);
                        DateTime fromDate = DateTime.Now - new TimeSpan(7, 0, 0, 0);
            List<int> errorList = new List<int>();

            SvcQueueSystem.QueueMsgType[] messageTypes = new SvcQueueSystem.QueueMsgType[] { (SvcQueueSystem.QueueMsgType)Enum.Parse(typeof(SvcQueueSystem.QueueMsgType), messageType.ToString()) };
            SvcQueueSystem.JobState[] jobCompletionStates = new SvcQueueSystem.JobState[] { SvcQueueSystem.JobState.ReadyForProcessing, SvcQueueSystem.JobState.Processing };
            Guid[] uids = new Guid[] { trackingGuid };

                while (inProcess)
                {

                    queueStatusDataSet = queueSystemClient.ReadMyJobStatus(messageTypes, jobCompletionStates, fromDate, toDate, -1, false, SvcQueueSystem.SortColumn.QueueEntryTime, SvcQueueSystem.SortOrder.Descending);
                        
                    
                    bool noRow = true;
                    foreach (SvcQueueSystem.QueueStatusDataSet.StatusRow statusRow in queueStatusDataSet.Status)
                    {
                        noRow = false;
                        if (statusRow["ErrorInfo"] != System.DBNull.Value)
                        {
                            errorList = CheckStatusRowErrors(statusRow["ErrorInfo"].ToString());

                            if (errorList.Count > 0
                                || statusRow.JobCompletionState == blockedState
                                || statusRow.JobCompletionState == failedState)
                            {
                                inProcess = false;
                                
                            }
                        }
                        if (statusRow.JobCompletionState == successState)
                        {
                            inProcess = false;
                            result = true;
                        }
                        else
                        {
                            inProcess = true;
                            System.Threading.Thread.Sleep(500);  // Sleep 1/2 second.
                        }
                    }
                    if (noRow) return true;
                    DateTime endTime = DateTime.Now;
                    TimeSpan span = endTime.Subtract(startTime);

                    if (span.Seconds > 20) //Wait for only 20 secs - and then bail out.
                    {
                        return result;//result = false;
                    }
                }
            return result;
        }
    }
}
