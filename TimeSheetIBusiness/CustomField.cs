using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCControlsToolkit.DataAnnotations;

namespace TimeSheetIBusiness
{
    [Serializable]
    public class CustomField
    {
        public string Name;
        public string FullName;
        public bool Visible;
    }

    [Serializable]
    public class CustomFieldItem : ICloneable
    {
        public string Name { get; set; }
        [DateRange(DynamicMaximum = "DateValue"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? DateValue { get; set; }
        public string FullName { get; set; }
        public string DataType { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? CostValue { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? DurationValue { get; set; }
        public bool? FlagValue { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? NumValue { get; set; }
        public string TextTValue { get; set; }
        public Guid? LookupID { get; set; }
        public string LookupValue { get; set; }
        public Guid? LookupTableGuid { get; set; }
        public Guid? CustomFieldGuid { get; set; }
        public List<LookupTableDisplayItem> LookupTableItems { get; set; }

        public object Clone()
        {
            CustomFieldItem item = new CustomFieldItem();
            item.Name = Name;
            item.DateValue = DateValue;
            return item;
        }

        public override bool Equals(object obj)
        {
            CustomFieldItem other = obj as CustomFieldItem;
            if (other == null) return false;
            else return this.Name == other.Name && this.DateValue == other.DateValue;
        }
        public override int GetHashCode()
        {
            if (Name == null) return 0;
            return Name.GetHashCode();
        }


        
    }
}
