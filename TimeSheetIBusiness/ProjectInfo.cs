using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheetIBusiness
{
    public class ProjectInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

   
    public class AssignementInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsProjectLineType { get; set; }
    }

    public class CustomFieldInfo
    {
        public string DataType { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }

    }
}
