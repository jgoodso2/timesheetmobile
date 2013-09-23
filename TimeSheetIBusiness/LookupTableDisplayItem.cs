using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace TimeSheetIBusiness
{
   /// <summary>
   /// List item for a selection in a custom field lookup table
   /// </summary>
   [Serializable]
    public class LookupTableDisplayItem
   {
      private string myDisplayMember;
      private Guid myValueMember;
      private string myDataType;
      private object myBoxedValue;
      public LookupTableDisplayItem()
      {
      }
      /// <summary>
      /// List item for a selection in a custom field lookup table.
      /// </summary>
      /// <param name="valueMember">Guid of lookup table item</param>
      /// <param name="displayMember">Display text for lookup table item</param>
      /// <param name="dataType">Project Server datatype of selection<seealso cref="PSLibrary.PSDataType"/></param>
      /// <param name="boxedValue">The value of the selection boxed in an object</param>
      /// 
      public LookupTableDisplayItem(Guid valueMember, 
                                    string displayMember, 
                                    string dataType, 
                                    object boxedValue)
      {
         myDisplayMember = displayMember;
         myValueMember = valueMember;
         myDataType = dataType;
         myBoxedValue = boxedValue;
      }
      /// <summary>
      /// Display text for the lookup table item
      /// </summary>
      public string DisplayMember
      {
         get
         {
            return myDisplayMember;
         }

      }

      /// <summary>
      /// Guid of the lookup table item
      /// </summary>
      public Guid ValueMember
      {
         get
         {
            return myValueMember;
         }
          set
          {
              myValueMember = value;
          }
      }

      /// <summary>
      /// Project Server datatype of selection<seealso cref="PSLibrary.PSDataType"/>
      /// </summary>
      public string DataType
      {
         get
         {
            return myDataType;
         }
      }

      /// <summary>
      /// The value of the selection boxed in an object
      /// </summary>
      public object BoxedValue
      {
         get
         {
            return myBoxedValue;
         }
      }

      public override bool Equals(object obj)
      {
          LookupTableDisplayItem other = obj as LookupTableDisplayItem;
          if (other == null) return false;
          else return this.BoxedValue == other.BoxedValue && this.DataType == other.DataType && other.DisplayMember == this.DisplayMember && other.ValueMember == this.ValueMember;
      }
      public override int GetHashCode()
      {
          return ValueMember.GetHashCode();
      }
      public LookupTableDisplayItem GetCopy()
      {
          return this.MemberwiseClone() as LookupTableDisplayItem;
      }
   }
}
