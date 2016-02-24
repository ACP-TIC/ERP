namespace entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class purchase_packing_dimension : Audit
    {
        public purchase_packing_dimension()
        {
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id_packing_property { get; set; }
        public long id_purchase_packing_detail { get; set; }
        public int id_dimension { get; set; }
        public decimal value { get; set; }

        public virtual purchase_packing_detail purchase_packing_detail { get; set; }
        public virtual app_dimension app_dimension { get; set; }

    }
}
