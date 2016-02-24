
namespace entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class payment_withholding_detail : Audit
    {
        public payment_withholding_detail()
        {
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_withholding_detail { get; set; }
        public int id_withholding { get; set; }
        public int id_sales_invoice { get; set; }
        public int id_purchase_invoice { get; set; }

        public virtual payment_withholding_tax payment_withholding_tax { get; set; }
        public virtual sales_invoice sales_invoice { get; set; }
        public virtual purchase_invoice purchase_invoice { get; set; }
    }
}
