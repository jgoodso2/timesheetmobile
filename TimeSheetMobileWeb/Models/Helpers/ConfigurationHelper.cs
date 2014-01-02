﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;


namespace TimeSheetMobileWeb.Models
{
    public static class ConfigurationHelper
    {
        private static string cookiename = "_configuration_";
        private static string userField = "_user_";
        private static string taskIdField = "_task_";
        private static string TimesheetIdField = "_row_";
        private static string cookieUpdatedField = "_updated_";
        public static void UserConfiguration(IRepository rep, string user)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            string taskId = null;
            string rowId = null;
            if (cookie != null && cookie.Values[userField] == user)
            {
                taskId = cookie.Values[taskIdField];
                rowId = cookie.Values[TimesheetIdField];
                if (!string.IsNullOrWhiteSpace(cookie.Values[cookieUpdatedField]))
                {
                    ChangeUserConfiguration(rep, user, new UserConfigurationInfo { RowViewId = rowId, TaskViewId = taskId });  
                }
            }
            else
            {
                if (ViewConfigurationRow.ViewFieldName != null || ViewConfigurationTask.ViewFieldName != null)
                {
                    UserConfigurationInfo conf = rep.UserConfiguration(user, ViewConfigurationRow.ViewFieldName, ViewConfigurationTask.ViewFieldName);
                    taskId = conf.TaskViewId;
                    rowId = conf.RowViewId;
                }

                
                    HttpCookie newCookie = null;
                    if (newCookie==null) newCookie=new HttpCookie(cookiename);
                    newCookie.Values[taskIdField] = taskId;
                    newCookie.Values[TimesheetIdField] = rowId;
                    newCookie.Values[userField] = user;
                    newCookie.Values[cookieUpdatedField] = string.Empty;
                    newCookie.Expires = DateTime.Now.AddYears(1);
                    HttpContext.Current.Response.Cookies.Add(newCookie);

                
            }
            
            ViewConfigurationTask taskSelected = null;
            ViewConfigurationRow rowSelected = null;
            if (!string.IsNullOrWhiteSpace(taskId))
            {
                taskSelected = ViewConfigurationTask.Find(taskId);
            }
            if (taskSelected == null) taskSelected = ViewConfigurationTask.All[0];
            if (!string.IsNullOrWhiteSpace(rowId))
            {
                rowSelected = ViewConfigurationRow.Find(rowId);
            }
            if (rowSelected == null) rowSelected = ViewConfigurationRow.All[0];
            ViewConfigurationRow.Default = rowSelected;
            ViewConfigurationTask.Default = taskSelected;
        }
        public static void ChangeUserConfiguration(IRepository rep, string user, UserConfigurationInfo conf)
        {
            if (ViewConfigurationRow.ViewFieldName != null || ViewConfigurationTask.ViewFieldName != null)
            {
                rep.ChangeUserConfiguration(user, conf, ViewConfigurationRow.ViewFieldName, ViewConfigurationTask.ViewFieldName);
            }
            HttpCookie newCookie = new HttpCookie(cookiename);
            newCookie.Values[taskIdField] = conf.TaskViewId;
            newCookie.Values[TimesheetIdField] = conf.RowViewId;
            newCookie.Values[userField] = user;
            newCookie.Expires = DateTime.Now.AddYears(1);
            newCookie.Values[cookieUpdatedField] = string.Empty;
            HttpContext.Current.Response.Cookies.Add(newCookie);

            

        }
    }
}