using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using PSLibrary = Microsoft.Office.Project.Server.Library;
using TimeSheetIBusiness;

namespace Microsoft.SDK.Project.Samples.ChangeXMLUtil
{

   class ChangeListWrapper
   {
      static private Changes changes = null;
      static private ChangeListWrapper changeListWrapper = null;

      static public Changes  GetCurrentChanges()
      {
         if (null == changes)
         {
            changes = new Changes();
         }
         return changes;
      }

      private ChangeListWrapper()
      {
      }

      static public ChangeListWrapper GetChangeListWrapper ()
      {
         if (null == changeListWrapper)
         {
            changeListWrapper = new ChangeListWrapper();
         }
         return changeListWrapper;
      }

      static public string GetCurrentXmlAsString(bool OmitXmlTag)
      {
         XmlSerializer serializer = new XmlSerializer(typeof(Changes));
         StringWriter writer = new StringWriter();

         serializer.Serialize(writer, changes);

         string xml = writer.ToString();
         writer.Close();

         if(OmitXmlTag)
         {
            xml = xml.Substring(xml.IndexOf("<Changes"));
         }

         // Remove empty ResID attribute.
         string emptyResID = @"ResID=""""";
         int indexEmptyResID = xml.IndexOf(emptyResID);
         if (indexEmptyResID > -1)
         {
             xml = xml.Substring(0, indexEmptyResID) + xml.Substring(indexEmptyResID + emptyResID.Length);
         }

         return xml;
      }

      static public Changes ImportXml(string xmlChanges)
      {
         XmlSerializer serializer = new XmlSerializer(typeof(Changes));
         StringReader reader = new StringReader(xmlChanges);
         changes = (Changes)serializer.Deserialize(reader);
         return changes;
      }

      static public void AddChange(bool isTask, bool isCurrentUser, Guid resUid,
                                   PidDisplayItem changeItem, 
                                   Guid projectId, Guid itemId, string value,
                                   CFDisplayItem cfDisplayItem, LookupTableDisplayItem luDisplayItem, 
                                   bool isPeriodChange, DateTime periodStart, DateTime periodEnd)
      {
         ChangesProj proj = new ChangesProj();

         proj.ID = projectId.ToString();

         if (changes.Proj == null)
         {
            changes.Proj = new ChangesProj[] { proj };
         }
         else
         {
            ChangesProj[] projs = changes.Proj;
            Array.Resize<ChangesProj>(ref projs, ((int)changes.Proj.Length + 1));
            changes.Proj = projs;
            changes.Proj.SetValue(proj, changes.Proj.Length - 1);
         }

         if(isTask)
         {
             AddTaskChange(isCurrentUser, resUid, changes.Proj[changes.Proj.Length-1], changeItem, itemId, value);
         }
         else
         {
             AddAssnChange(isCurrentUser, resUid, changes.Proj[changes.Proj.Length - 1], changeItem, itemId, value, 
                           cfDisplayItem, luDisplayItem, isPeriodChange, periodStart, periodEnd);
         }
      
      }

      private static void AddAssnChange(bool isCurrentUser, Guid resUid, ChangesProj proj, 
                                        PidDisplayItem changeItem, Guid itemId, 
                                        string value, CFDisplayItem cfDisplayItem, 
                                        LookupTableDisplayItem ltDisplayItem, 
                                        bool isPeriodChange, DateTime periodStart, 
                                        DateTime periodEnd)
      {
         // To be robust, don't assume assn is empty.
         if (null == proj.Assn)
         {
            proj.Assn = new ChangesProjAssn[1];
         }
         else
         {
            ChangesProjAssn[] assns = proj.Assn;
            Array.Resize<ChangesProjAssn>(ref assns, ((int)assns.Length + 1));
         }

         // Create the assignment.
         ChangesProjAssn projAssn = new ChangesProjAssn();
         
         // Add it to the project.
         proj.Assn[proj.Assn.Length - 1] = projAssn;
         
         // Fill in the properties.
         projAssn.ID = itemId.ToString();

         if (isCurrentUser) projAssn.ResID = string.Empty;
         else projAssn.ResID = resUid.ToString();
         
         // We can assume that Items is empty, since we are doing everything else right here at once.
         // Items is a generic object that holds a change.
         projAssn.Items= new Object[1];
         
         // Create the boxed typed change.
         Object change  = CreateTypedAssnChange(changeItem, value, cfDisplayItem, ltDisplayItem, 
                                                isPeriodChange, ref periodStart, ref periodEnd);
         projAssn.Items[0] = change;
      }

      private static Object CreateTypedAssnChange(PidDisplayItem changeItem, string value, 
                                                   CFDisplayItem cfDisplayItem, 
                                                   LookupTableDisplayItem ltDisplayItem, 
                                                   bool isPeriodChange, ref DateTime periodStart, 
                                                   ref DateTime periodEnd)
      {
         Object change;
         //Create the change based on type
         if (isPeriodChange)
         {
            if (changeItem.DataFormat == ProjDataFormat.Work)
            {
               ChangesProjAssnPeriodChange periodChange = new ChangesProjAssnPeriodChange();
               periodChange.PID = changeItem.ValueMember;
               periodChange.Start = Convert.ToDateTime(
                                       periodStart.ToString("yyyy-MM-ddTHH:mm:ss"));
               periodChange.End = Convert.ToDateTime(
                                       periodEnd.ToString("yyyy-MM-ddTHH:mm:ss"));
               periodChange.Value = value;
               change = (object)periodChange;
            }
            else
            {
               change = null;
               throw new Exception("Period changes are valid for Work data only.");
            }
         }
         else
         {
            if ((ProjDataFormat)changeItem.DataFormat == ProjDataFormat.CustomField)
            {
               if (cfDisplayItem.HasLookup)
               {

                  if (ltDisplayItem != null)
                  {
                     ChangesProjAssnLookupTableCustomFieldChange cfLuChange = 
                           new ChangesProjAssnLookupTableCustomFieldChange();
                     ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue cfLuChangeValue = 
                              new ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue();
                     cfLuChange.CustomFieldGuid = cfDisplayItem.ValueMember.ToString();
                     cfLuChange.CustomFieldName = cfDisplayItem.DisplayMember;
                     cfLuChange.CustomFieldType = getCustomFieldChageType(cfDisplayItem.DataType);
                     cfLuChange.IsMultiValued = cfDisplayItem.IsMultiValued;
                     cfLuChange.LookupTableValue = 
                           new ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue[1];
                     cfLuChangeValue.Guid = ltDisplayItem.ValueMember.ToString();
                     cfLuChangeValue.Value = ltDisplayItem.BoxedValue.ToString();
                     cfLuChange.LookupTableValue[0] = cfLuChangeValue;
                     change = cfLuChange;
                  }
                  else
                  {
                     throw new MissingFieldException("No lookup value was specified. "+
                                                      "Please choose a lookup table item.", 
                                                      "Lookup Table Value");
                  }
               }
               else
               {
                  ChangesProjAssnSimpleCustomFieldChange cfChange = 
                        new ChangesProjAssnSimpleCustomFieldChange();
                  cfChange.CustomFieldGuid = cfDisplayItem.ValueMember.ToString();
                  cfChange.CustomFieldName = cfDisplayItem.DisplayMember;
                  cfChange.CustomFieldType = getCustomFieldChageType(cfDisplayItem.DataType);
                  cfChange.Value = value;
                  change = cfChange;
               }
            }
            else
            {
               ChangesProjAssnChange stdChange = new ChangesProjAssnChange();
               stdChange.PID = changeItem.ValueMember;
               stdChange.Value = value;

               change = (Object)stdChange;
            }
         }
         return change;
      }

      private static ChangeType getCustomFieldChageType(PSLibrary.PSDataType dataType)
      {
         switch (dataType)
         {
            case PSLibrary.PSDataType.COST:
               return ChangeType.Cost;
            case PSLibrary.PSDataType.NUMBER:
               return ChangeType.Number;
            case PSLibrary.PSDataType.DATE:
               return ChangeType.Date;
            case PSLibrary.PSDataType.DURATION:
               return ChangeType.Duration;
            case PSLibrary.PSDataType.PERCENT:
               return ChangeType.Number;
            case PSLibrary.PSDataType.STRING:
               return ChangeType.Text;
            case PSLibrary.PSDataType.WORK:
               return ChangeType.Number;
            case PSLibrary.PSDataType.YESNO:
               return ChangeType.Flag;
            default:
               return ChangeType.None;
         }
      }

      private static void AddTaskChange(bool isCurrentUser, Guid resUid, ChangesProj proj, PidDisplayItem changeItem, 
                                          Guid itemId, string value)
      {
         // To be robust, don't assume task is empty.
         if (null == proj.Task)
         {
            proj.Task = new ChangesProjTask[1];
         }
         else
         {
            ChangesProjTask[] tasks = proj.Task;
            Array.Resize<ChangesProjTask>(ref tasks, ((int)tasks.Length + 1));
         }
         // Create the task and add it to the Proj.
         ChangesProjTask projTask = new ChangesProjTask();
         proj.Task[proj.Task.Length - 1] = projTask;
         
         // Fill in the properties.
         projTask.ID = itemId.ToString();

         if (isCurrentUser) projTask.ResID = string.Empty;
         else projTask.ResID = resUid.ToString();

         // Create the change and add it to the task.
         // We'll just have the one child.
         projTask.Change =new ChangesProjTaskChange[1];
         ChangesProjTaskChange projTaskChange = new ChangesProjTaskChange();
         projTask.Change[0] = projTaskChange;

         // Fill in the properties.
         projTaskChange.PID = changeItem.ValueMember;
         projTaskChange.Value = value;
      } 

      static public void ClearXml()
      {
         changes = new Changes();
      }

      static public ProjDataFormat GetProjDataFormat4PSData(PSLibrary.PSDataType dataType)
      {
         switch (dataType)
         {
            case PSLibrary.PSDataType.BOOL:
            case PSLibrary.PSDataType.YESNO:
               return ProjDataFormat.YesNo;
            case PSLibrary.PSDataType.DATE:
               return ProjDataFormat.Date;
            case PSLibrary.PSDataType.COST:
               return ProjDataFormat.Cost;
            case PSLibrary.PSDataType.NUMBER:
               return ProjDataFormat.Count;
            case PSLibrary.PSDataType.DURATION:
               return ProjDataFormat.Duration;
            case PSLibrary.PSDataType.STRING:
               return ProjDataFormat.Text;
            default:
               return ProjDataFormat.Text;
         }
      }
   }

   /// <summary>
   /// Data format, used for display purposes.
   /// </summary>
   public enum ProjDataFormat
   {
      /// <summary>
      /// Date and time.
      /// </summary>
      Date,
      /// <summary>
      ///  Project duration time unit (PTU).
      ///  1 PTU = 1/10 of a minute. 4800 PTU=8 hours.
      /// </summary>
      Duration,
      /// <summary>
      /// Project work time unit (PWU).
      /// 1 PWU=1/1000 of a minute. 480,000 PWU=8 hours.
       /// </summary>
      Work,
      /// <summary>
      /// 1 to 100 percent.
      /// </summary>
      Percentage,
      /// <summary>
      /// Text data.
      /// </summary>
      Text,
      /// <summary>
      /// Boolean data.
      /// </summary>
      YesNo,
      /// <summary>
      /// Discrete positive units.
      /// </summary>
      Count,
      /// <summary>
      /// Special custom field.
      /// </summary>
      /// <remarks>
      /// This datatype causes additional work to be done for custom fields.
      /// </remarks>
      CustomField,
      /// <summary>
      /// Currency information.
      /// </summary>
      Cost,
      /// <summary>
      /// No special data type handling is to be done.
      /// </summary>
      None
   }

   /// <summary>
   /// List item for a Project Field ID.
   /// </summary>
   public class PidDisplayItem
   {
      private string displayText;
      private uint value;
      ProjDataFormat dataFormat;

      /// <summary>
      /// List item for a Project Field ID
      /// </summary>
      /// <param name="key">Unsigned integer of the field ID.<seealso cref="PSLibrary.AssnConstID"/><seealso cref="PSLibrary.TaskConstID"/></param>
      /// <param name="text">Display text describing this field.</param>
      /// <param name="projDataFormat">Data format for this item.<seealso cref="ProjDataFormat"/></param>
      public PidDisplayItem(uint key, string text, ProjDataFormat projDataFormat)
      {
         value = key;
         displayText = text;
         dataFormat = projDataFormat;
      }

      /// <summary>
      /// Display text for this item.
      /// </summary>
      public string DisplayMember
      {
         get
         {
            return displayText;
         }
      
      }

      /// <summary>
      /// Unsigned int uniquely identifying this field.
      /// </summary>
      public uint ValueMember
      {
         get
         {
            return value;
         }
      }

      /// <summary>
      /// Data format for this item.<seealso cref="ProjDataFormat"/>
      /// </summary>
       public ProjDataFormat DataFormat
       {
          get
          {
             return dataFormat;
          }
       }
    }

}
