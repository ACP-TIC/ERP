﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cognitivo.Purchase {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    public sealed partial class InvoiceSetting : global::System.Configuration.ApplicationSettingsBase {
        
        private static InvoiceSetting defaultInstance = ((InvoiceSetting)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new InvoiceSetting())));
        
        public static InvoiceSetting Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool VAT_Included {
            get {
                return ((bool)(this["VAT_Included"]));
            }
            set {
                this["VAT_Included"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool VAT_Excluded {
            get {
                return ((bool)(this["VAT_Excluded"]));
            }
            set {
                this["VAT_Excluded"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int TransDate_OffSet {
            get {
                return ((int)(this["TransDate_OffSet"]));
            }
            set {
                this["TransDate_OffSet"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Telephone {
            get {
                return ((bool)(this["quicklook_Telephone"]));
            }
            set {
                this["quicklook_Telephone"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_PurchaseOrder {
            get {
                return ((bool)(this["quicklook_PurchaseOrder"]));
            }
            set {
                this["quicklook_PurchaseOrder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Project {
            get {
                return ((bool)(this["quicklook_Project"]));
            }
            set {
                this["quicklook_Project"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Branch {
            get {
                return ((bool)(this["quicklook_Branch"]));
            }
            set {
                this["quicklook_Branch"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AllowDuplicateItems {
            get {
                return ((bool)(this["AllowDuplicateItems"]));
            }
            set {
                this["AllowDuplicateItems"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool filterbyBranch {
            get {
                return ((bool)(this["filterbyBranch"]));
            }
            set {
                this["filterbyBranch"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowFactorQty {
            get {
                return ((bool)(this["ShowFactorQty"]));
            }
            set {
                this["ShowFactorQty"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DiscountStock_Packing {
            get {
                return ((bool)(this["DiscountStock_Packing"]));
            }
            set {
                this["DiscountStock_Packing"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowCode {
            get {
                return ((bool)(this["ShowCode"]));
            }
            set {
                this["ShowCode"] = value;
            }
        }
    }
}
