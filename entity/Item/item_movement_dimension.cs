namespace entity
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class item_movement_dimension : Audit
    {
        public item_movement_dimension()
        {
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id_movement_property { get; set; }
        public long id_movement { get; set; }
        public int id_dimension { get; set; }
        public decimal value { get; set; }
    
        public virtual item_movement item_movement { get; set; }
        public virtual app_dimension app_dimension { get; set; }
    }
}
