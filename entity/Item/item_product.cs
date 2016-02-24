namespace entity
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class item_product : Audit
    {
        public enum COGS_Types
        {
            FIFO = 1,
            LIFO = 2
        }

        public item_product()
        {
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            cogs_type = COGS_Types.FIFO;
            is_head = true;
            item_movement = new List<item_movement>();
            item_conversion_factor = new List<item_conversion_factor>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_item_product { get; set; }
        public int id_item { get; set; }
        public decimal? stock_min { get; set; }
        public decimal? stock_max { get; set; }
        public bool can_expire { get; set; }
        public bool is_weigted { get; set; }
        public COGS_Types cogs_type { get; set; }

        public virtual item item { get; set; }
        public virtual IEnumerable<item_movement> item_movement { get; set; }
        public virtual IEnumerable<item_inventory_detail> item_inventory_detail { get; set; }
        public virtual IEnumerable<item_transfer_detail> item_transfer_detail { get; set; }
        public virtual ICollection<item_conversion_factor> item_conversion_factor { get; set; }
    }
}
