namespace entity
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class purchase_invoice_detail : CommercialPurchaseDetail, IDataErrorInfo
    {
        public purchase_invoice_detail()
        {
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            is_head = true;
            quantity = 1;
            purchase_invoice_dimension= new List<purchase_invoice_dimension>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_purchase_invoice_detail { get; set; }
        public int id_purchase_invoice { get; set; }
        public int? id_purchase_order_detail { get; set; }
        

        #region "Navigation Properties"
        public virtual purchase_order_detail purchase_order_detail { get; set; } 
        public virtual purchase_invoice purchase_invoice { get; set; }
        public virtual IEnumerable<purchase_return_detail> purchase_return_detail { get; set; }
        public virtual ICollection<purchase_invoice_dimension> purchase_invoice_dimension { get; set; }
        #endregion

        #region "Validation"
        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }
                return error.Length == 0 ? null : error.ToString();
            }
        }
        
        public string this[string columnName]
        {
            get
            {
                if (columnName == "quantity")
                {
                    if (quantity == 0)
                        return "Quantity can not be zero";
                }
                if (columnName == "id_vat_group")
                {
                    if (id_vat_group == 0)
                        return "Vat Group can not be zero";
                }
                if (columnName == "id_cost_center")
                {
                    if (id_cost_center == 0)
                        return "Cost Center can not be zero";
                }
                if (columnName == "unit_cost")
                {
                    if (unit_cost < 0)
                        return "Cannot be less than Zero";
                }
                return "";
            }
        }
        #endregion

    }
}
