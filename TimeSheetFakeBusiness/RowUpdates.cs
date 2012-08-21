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
        public bool Changed { get; set; }
        public PropertyTracker<BaseRow> Values { get; set; }
        public List<Tracker<decimal?>> TValues{get; set;}
        public ExtendedRow(Tracker<BaseRow> x)
        {
            TChanged=false;
            VChanged=false;
            
            
            if ((x.OldValue == null && x.Value!= null) || (x.OldValue != null && x.Value== null)) x.Changed=true;
            Values = x as PropertyTracker<BaseRow>;
            if (x.Changed) VChanged = true;
            List<decimal?> oldValues =null;
            List<decimal?> values =null;
            TValues = new List<Tracker<decimal?>>();
            if (x.OldValue is SingleValuesRow || x.Value is SingleValuesRow) TChanged = false;
            else
            {
                if (x.OldValue != null || x.Value != null)
                {
                    if (x.OldValue != null) oldValues = x.OldValue.DayTimes;
                    if (x.Value != null) values = x.Value.DayTimes;

                    for (int i = 0; i < values.Count; i++)
                    {
                        var toAdd = new PropertyTracker<decimal?>(oldValues == null ? null : oldValues[i], values == null ? null : values[i]);
                        if (toAdd.Changed) TChanged = true;
                        TValues.Add(toAdd);
                    }
                }
            }
            
            Changed=TChanged||VChanged;

        }
    }
    internal class WholeLine
    {
        public List<ExtendedRow> Actuals { get; set; }
        public bool Changed { get; set; }
        public bool Processed { get; set; }
        public string Key { get; set; }
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
    }
}
