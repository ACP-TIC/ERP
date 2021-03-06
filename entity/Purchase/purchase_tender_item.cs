namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class purchase_tender_item : Audit
    {
        public purchase_tender_item()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            purchase_tender_dimension = new List<purchase_tender_dimension>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_purchase_tender_item { get; set; }

        public int id_purchase_tender { get; set; }
        public int? id_item { get; set; }
        public int? id_project_task { get; set; }
        public string item_description { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Required]
        public decimal quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    RaisePropertyChanged("quantity");
                    _Quantity_Factored = Brillo.ConversionFactor.Factor_Quantity(item, Convert.ToDecimal(value), GetDimensionValue());
                    RaisePropertyChanged("_Quantity_Factored");
                }
            }
        }

        private decimal _quantity;

        /// <summary>
        ///
        /// </summary>
        [NotMapped]
        public decimal Quantity_Factored
        {
            get { return _Quantity_Factored; }
            set
            {
                if (_Quantity_Factored != value)
                {
                    _Quantity_Factored = value;
                    RaisePropertyChanged("Quantity_Factored");

                    quantity = Brillo.ConversionFactor.Factor_Quantity_Back(item, Quantity_Factored, GetDimensionValue());
                    RaisePropertyChanged("quantity");
                }
            }
        }

        private decimal _Quantity_Factored;

        public virtual item item
        {
            get { return _item; }
            set
            {
                if (_item != value)
                {
                    _item = value;
                    if (_item != null && (item_description == null || item_description == string.Empty))
                    {
                        item_description = _item.name;
                    }
                }
            }
        }

        public item _item;

        public virtual purchase_tender purchase_tender { get; set; }
        public virtual ICollection<purchase_tender_detail> purchase_tender_detail { get; set; }
        public virtual ICollection<purchase_tender_dimension> purchase_tender_dimension { get; set; }
        public virtual project_task project_task { get; set; }

        public decimal GetDimensionValue()
        {
            decimal Dimension = 1M;
            if (purchase_tender_dimension != null)
            {
                foreach (purchase_tender_dimension _purchase_tender_dimension in purchase_tender_dimension)
                {
                    Dimension = Dimension * _purchase_tender_dimension.value;
                }
            }
            return Dimension;
        }
    }
}