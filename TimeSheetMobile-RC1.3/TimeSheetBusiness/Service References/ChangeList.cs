﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace Microsoft.SDK.Project.Samples.ChangeXMLUtil {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class Changes {
        
        private ChangesProj[] projField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Proj")]
        public ChangesProj[] Proj {
            get {
                return this.projField;
            }
            set {
                this.projField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProj 
    {
        
        private ChangesProjAssn[] assnField;
        private ChangesProjTask[] taskField;
        private string idField;                 // Guid of the task.
        private string resIdField;              // Guid of the resource.
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Assn")]
        public ChangesProjAssn[] Assn {
            get {
                return this.assnField;
            }
            set {
                this.assnField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Task")]
        public ChangesProjTask[] Task {
            get {
                return this.taskField;
            }
            set {
                this.taskField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ResID {
            get {
                return this.resIdField;
            }
            set {
                this.resIdField = value;
            }
         }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssn {
        
        private object[] itemsField;
        private string idField;
        private string resIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Change", typeof(ChangesProjAssnChange))]
        [System.Xml.Serialization.XmlElementAttribute("LookupTableCustomFieldChange", typeof(ChangesProjAssnLookupTableCustomFieldChange))]
        [System.Xml.Serialization.XmlElementAttribute("PeriodChange", typeof(ChangesProjAssnPeriodChange))]
        [System.Xml.Serialization.XmlElementAttribute("SimpleCustomFieldChange", typeof(ChangesProjAssnSimpleCustomFieldChange))]
        public object[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ResID
        {
            get
            {
                return this.resIdField;
            }
            set
            {
                this.resIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssnChange {
        
        private uint pIDField;
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint PID {
            get {
                return this.pIDField;
            }
            set {
                this.pIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssnLookupTableCustomFieldChange {
        
        private ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue[] lookupTableValueField;
        
        private bool isMultiValuedField;
        private ChangeType customFieldTypeField;
        private string customFieldGuidField;
        private string customFieldNameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LookupTableValue")]
        public ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue[] LookupTableValue {
            get {
                return this.lookupTableValueField;
            }
            set {
                this.lookupTableValueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IsMultiValued {
            get {
                return this.isMultiValuedField;
            }
            set {
                this.isMultiValuedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ChangeType CustomFieldType {
            get {
                return this.customFieldTypeField;
            }
            set {
                this.customFieldTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CustomFieldGuid {
            get {
                return this.customFieldGuidField;
            }
            set {
                this.customFieldGuidField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CustomFieldName {
            get {
                return this.customFieldNameField;
            }
            set {
                this.customFieldNameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssnLookupTableCustomFieldChangeLookupTableValue {
        
        private string guidField;
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Guid {
            get {
                return this.guidField;
            }
            set {
                this.guidField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    public enum ChangeType {
        
        /// <remarks/>
        Cost,
        
        /// <remarks/>
        Date,
        
        /// <remarks/>
        StartDate,
        
        /// <remarks/>
        FinishDate,
        
        /// <remarks/>
        Duration,
        
        /// <remarks/>
        Flag,
        
        /// <remarks/>
        None,
        
        /// <remarks/>
        Number,
        
        /// <remarks/>
        Text,
        
        /// <remarks/>
        OutlineCode,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssnPeriodChange {
        
        private uint pIDField;
        private System.DateTime startField;
        private System.DateTime endField;
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint PID {
            get {
                return this.pIDField;
            }
            set {
                this.pIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime Start {
            get {
                return this.startField;
            }
            set {
                this.startField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime End {
            get {
                return this.endField;
            }
            set {
                this.endField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjAssnSimpleCustomFieldChange {
        
        private ChangeType customFieldTypeField;
        
        private string customFieldGuidField;
        private string customFieldNameField;
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ChangeType CustomFieldType {
            get {
                return this.customFieldTypeField;
            }
            set {
                this.customFieldTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CustomFieldGuid {
            get {
                return this.customFieldGuidField;
            }
            set {
                this.customFieldGuidField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CustomFieldName {
            get {
                return this.customFieldNameField;
            }
            set {
                this.customFieldNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjTask {
        
        private ChangesProjTaskChange[] changeField;
        private string idField;
        private string resIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Change")]
        public ChangesProjTaskChange[] Change {
            get 
            {
                return this.changeField;
            }
            set 
            {
                this.changeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get 
            {
                return this.idField;
            }
            set 
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ResID
        {
            get
            {
                return this.resIdField;
            }
            set
            {
                this.resIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ChangesProjTaskChange {
        
        private uint pIDField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint PID {
            get {
                return this.pIDField;
            }
            set {
                this.pIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
}
