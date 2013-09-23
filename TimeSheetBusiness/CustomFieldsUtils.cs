using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;
using System.Web.Services.Protocols;
using PSLibrary = Microsoft.Office.Project.Server.Library;
using TimeSheetBusiness;

namespace Microsoft.SDK.Project.Samples.ChangeXMLUtil
{
   class CustomFieldsUtils //: ProjSecureWebSvc
   {
      private static SvcCustomFields.CustomFieldDataSet customFieldDs;

      private static CustomFieldsUtils customFieldsUtils;
      string lastErrors = string.Empty;

      private CustomFieldsUtils()
      {
      }

      static public CustomFieldsUtils GetCustomFieldsUtils()
      {
         if (null == customFieldsUtils)
         {
            customFieldsUtils = new CustomFieldsUtils();
         }
         return customFieldsUtils;
      }

      public void RefreshCustomFields()
      {
         PSLibrary.Filter filter = GetFilter();

         try
         {
             Repository repository = new Repository();
             
             customFieldDs = repository.customFieldsClient.ReadCustomFields(filter.GetXml(), false);
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
      }

      public CFDisplayItem[] GetCustomFieldsAsItems()
      {
         RefreshCustomFields();
      
         SvcCustomFields.CustomFieldDataSet.CustomFieldsDataTable cfTable =
                                                               customFieldDs.CustomFields;
         CFDisplayItem[] items =new CFDisplayItem[customFieldDs.CustomFields.Count];

         for(int i=0; i < cfTable.Count; i++)
         {
            items[i] = new CFDisplayItem( cfTable[i].MD_PROP_UID_SECONDARY,
               cfTable[i].MD_PROP_NAME,
               (PSLibrary.PSDataType)cfTable[i].MD_PROP_TYPE_ENUM,
               cfTable[i].MD_ENT_TYPE_UID,
               (cfTable[i].IsMD_LOOKUP_TABLE_UIDNull() ? Guid.Empty : cfTable[i].MD_LOOKUP_TABLE_UID),
               (cfTable[i].IsMD_PROP_DEFAULT_VALUENull() ? Guid.Empty : cfTable[i].MD_PROP_DEFAULT_VALUE),
               (cfTable[i].IsMD_PROP_MAX_VALUESNull() ? false : cfTable[i].MD_PROP_MAX_VALUES>1));
         }
         return items;
      }

      public DataSet GetCustomFields()
      {
         if (null == customFieldDs)
         {
            RefreshCustomFields();
         }
         return (DataSet) customFieldDs;
      }

      private static PSLibrary.Filter GetFilter()
      {
         // Instantiate the dataset to retrieve the list of literals.
         // This helps to ensure an absence of typos.
         SvcCustomFields.CustomFieldDataSet ds = new SvcCustomFields.CustomFieldDataSet();

         PSLibrary.Filter filter = new PSLibrary.Filter();

         filter.FilterTableName = ds.CustomFields.TableName;

         // List the custom fields.
         // No sort order is specified because sorting is done in the list box.
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_ENT_TYPE_UIDColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));
         
         // ! Important !
         // The custom field ID to use is the secondary ID, because this 
         // implies the assignment level, rather than the parent level.
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_PROP_UID_SECONDARYColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_PROP_NAMEColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_PROP_TYPE_ENUMColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));

         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_LOOKUP_TABLE_UIDColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_PROP_DEFAULT_VALUEColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));
         filter.Fields.Add(new PSLibrary.Filter.Field(filter.FilterTableName,
                     ds.CustomFields.MD_PROP_MAX_VALUESColumn.ColumnName,
                     PSLibrary.Filter.SortOrderTypeEnum.None));

         // Set the filter. We want only resource and task custom fields.
         PSLibrary.Filter.IOperator[] fieldOperators = new PSLibrary.Filter.IOperator[2];

         fieldOperators[0] = new PSLibrary.Filter.FieldOperator(PSLibrary.Filter.FieldOperationType.Equal,
                     ds.CustomFields.MD_ENT_TYPE_UIDColumn.ColumnName,
                     PSLibrary.EntityCollection.Entities.TaskEntity.UniqueId);
         fieldOperators[1] = new PSLibrary.Filter.FieldOperator(PSLibrary.Filter.FieldOperationType.Equal,
                     ds.CustomFields.MD_ENT_TYPE_UIDColumn.ColumnName,
                     PSLibrary.EntityCollection.Entities.ResourceEntity.UniqueId);

         // Set the logical operator.
         PSLibrary.Filter.LogicalOperator logicalOp = new
                     PSLibrary.Filter.LogicalOperator(PSLibrary.Filter.LogicalOperationType.Or, fieldOperators);

         filter.Criteria = logicalOp;
         return filter;
      }
   }

   /// <summary>
   /// Custom Field Display Item
   /// </summary>
   /// <remarks>
   /// Object for use in UI lists  
   ///</remarks>
   public class CFDisplayItem
   {
      private string myDisplayMember;
      private Guid myValueMember;
      private PSLibrary.PSDataType myDataType;
      private Guid myEntityType;
      private Guid myLookupTableUid;
      private Guid myLookupTableDefaultItemUid;
      private bool myIsMultiValued;

      /// <summary>
      /// Constructor for custom field display item.
      /// Use this signature for lookup table custom fields.
      /// </summary>
      /// <param name="valueMember">GUID of custom field</param>
      /// <param name="displayMember">Display text of custom field</param>
      /// <param name="dataType">Custom field Project Server data type</param>
      /// <param name="entityType">GUID representing the entity type,
      /// such as task, resource, etc.
      /// </param>
      /// <param name="lookupTableUid">Guid of lookup table. Use Guid.Empty 
      /// if there is no lookup table. 
      /// </param>
      /// <param name="lookupTableDefaultItemUid">Default item Guid. Use Guid.Empty 
      /// if there is no lookup table.
      /// </param>
      /// <param name="isMultiValued">True if lookup table allows multiple values. Use false 
      /// if there is no lookup table.
      /// </param>
      public CFDisplayItem(Guid valueMember, string displayMember, 
                           PSLibrary.PSDataType dataType, Guid entityType, 
                           Guid lookupTableUid, Guid lookupTableDefaultItemUid, 
                           bool isMultiValued)
      {
         
         myDisplayMember = displayMember;
         myValueMember = valueMember;
         myDataType = dataType;
         myEntityType = entityType;
         myLookupTableUid = lookupTableUid;
         myLookupTableDefaultItemUid = lookupTableDefaultItemUid;
         myIsMultiValued = isMultiValued;
      }

      /// <summary>
      /// Constructor for custom field display item. Use this signature
      /// for custom fields with no lookup table.
      /// </summary>
      /// <param name="valueMember">GUID of custom field</param>
      /// <param name="displayMember">Display text of custom field</param>
      /// <param name="dataType">Custom field Project Server data type</param>
      /// <param name="entityType">GUID representing the entity type,
      /// such as task, resource, etc.
      /// </param>
      public CFDisplayItem(Guid valueMember, 
                           string displayMember, 
                           PSLibrary.PSDataType dataType, 
                           Guid entityType) 
               : this(valueMember, displayMember, dataType, entityType, Guid.Empty, Guid.Empty, false) 
      { }
      
      /// <summary>
      /// Text description of custom field
      /// </summary>
      public string DisplayMember
      {
          get
          {
              return myDisplayMember;
          }
      }

      /// <summary>
      /// Guid of custom field
      /// </summary>
      public Guid ValueMember
      {
         get
         {
            return myValueMember;
         }
      }

      /// <summary>
      /// Project Server Data Type of Custom Field
      /// </summary>
      public PSLibrary.PSDataType DataType
      {
         get
         {
            return myDataType;
         }
      }

      /// <summary>
      /// Entity type GUID of custom field.
      /// </summary>
      /// <seealso cref="Microsoft.Office.Project.Server.Library.EntityCollection.Entities"/>
      public Guid EntityType
      {
         get
         {
            return myEntityType;
         }
      }

      /// <summary>
      /// Default item Guid. Returns Guid.Empty 
      /// if there is not lookup table.
      /// </summary>
      public Guid DefaultLookupItem
      {
         get
         {
            return myLookupTableDefaultItemUid;
         }
      }

      /// <summary>
      /// Returns true if this custom field has a lookup table.
      /// </summary>
      public bool HasLookup
      {
         get
         {
            return (Guid.Empty != myLookupTableUid);
         }
      }

      /// <summary>
      /// Guid of the lookup table. Returns Guid.empty if there is no lookup table.
      /// </summary>
      public Guid LookupTableUid
      {
         get
         {
            return myLookupTableUid;
         }
      }

      /// <summary>
      /// Returns true if this custom field has a lookup table
      /// and that lookup table is multi-valued.
      /// </summary>
      public bool IsMultiValued
      {
         get
         {
            return myIsMultiValued;
         }
      }
   }
}
