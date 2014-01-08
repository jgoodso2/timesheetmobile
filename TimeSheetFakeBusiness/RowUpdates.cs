using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheetIBusiness;
using System.Security.Principal;
using MVCControlsToolkit.Controller;

namespace TimeSheetIBusiness
{
    internal class ExtendedRow
    {
        public bool VChanged { get; set; }
        public bool TChanged { get; set; }
        public bool CChanged { get; set; }
        public bool Changed { get; set; }
        public PropertyTracker<BaseRow> Values { get; set; }
        public List<Tracker<decimal?>> TValues{get; set;}
        public List<Tracker<CustomFieldItem>> TCFValues { get; set; }
        public string ProjectId { get; set; }
        public ExtendedRow(Tracker<BaseRow> x)
        {
            TChanged=false;
            VChanged=false;
            CChanged = false;
            
            if ((x.OldValue == null && x.Value!= null) || (x.OldValue != null && x.Value== null)) x.Changed=true;
            Values = x as PropertyTracker<BaseRow>;
            if (x.Changed) VChanged = true;
            List<decimal?> oldValues =null;
            List<decimal?> values =null;
            List<CustomFieldItem> oldcfvalues = null;
            List<CustomFieldItem> cfvalues = null;

            TValues = new List<Tracker<decimal?>>();
            TCFValues = new List<Tracker<CustomFieldItem>>();
            
                if (x.OldValue != null || x.Value != null)
                {
                    if (x.OldValue != null) 
                    { 
                        oldValues = x.OldValue.DayTimes; 
                        if(x.OldValue is ActualWorkRow)
                        {
                            oldcfvalues = (x.OldValue as ActualWorkRow).CustomFieldItems;
                        }
                        if (x.OldValue is SingleValuesRow)
                        {
                            oldcfvalues = (x.OldValue as SingleValuesRow).CustomFieldItems;
                        }
                    }
                    if (x.Value != null)
                    {
                        values = x.Value.DayTimes;
                        if (x.Value is ActualWorkRow)
                        {
                            cfvalues = (x.Value as ActualWorkRow).CustomFieldItems;
                        }
                        if (x.Value is SingleValuesRow)
                        {
                            cfvalues = (x.Value as SingleValuesRow).CustomFieldItems;
                        }
                    }

                    if (values != null)
                    {
                        for (int i = 0; i < values.Count; i++)
                        {
                            var toAdd = new PropertyTracker<decimal?>(oldValues == null ? null : oldValues[i], values == null ? null : values[i]);
                            if (toAdd.Changed) TChanged = true;
                            TValues.Add(toAdd);
                        }
                    }

                    if (cfvalues != null)
                    {
                        for (int i = 0; i < cfvalues.Count; i++)
                        {
                            var toAdd = new PropertyTracker<CustomFieldItem>(oldcfvalues == null ? null : oldcfvalues[i], cfvalues == null ? null : cfvalues[i]);
                            if (toAdd.Changed)
                            {
                               
                                if(!(toAdd.ChangedProperties.Count == 1 && toAdd.ChangedProperties[0] == "LookupTableItems"))
                                {
                                     CChanged = true;
                                }
                            }
                            TCFValues.Add(toAdd);
                        }
                    }
                }

                if (x.Value is SingleValuesRow)
                {
                    Changed = CChanged || VChanged;
                }
                else
                {
                    Changed = TChanged || VChanged || CChanged;
                }

        }
    }
    internal class WholeLine
    {
        public List<ExtendedRow> Actuals { get; set; }
        public bool Changed { get; set; }
        public bool Processed { get; set; }
        public string Key { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        
        public WholeLine(IGrouping<string, Tracker<BaseRow>> x)
        {
            Key = x.Key;
            
            Actuals = new List<ExtendedRow>();
            Changed = false;
            Processed = false;
            foreach (var y in x)
            {
                var z = new ExtendedRow(y);
                if (z.Changed) Changed = true;
                Actuals.Add(z);
            }
        }

        public bool IsTopLevelTask { get; set; }
    }
}
