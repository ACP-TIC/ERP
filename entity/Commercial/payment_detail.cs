namespace entity
{
    using Brillo;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;

    public partial class payment_detail : Audit, IDataErrorInfo
    {
        public payment_detail()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            trans_date = DateTime.Now;
            payment_schedual = new List<payment_schedual>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_payment_detail { get; set; }

        public int? id_payment { get; set; }
        public int? id_sales_return { get; set; }
        public int? id_purchase_return { get; set; }
        public int? id_account { get; set; }

        [NotMapped]
        public int id_currency
        {
            get
            {
                return _id_currency;
            }
            set
            {
                if (_id_currency != value)
                {
                    _id_currency = value;
                    RaisePropertyChanged("id_currency");

                    using (db db = new db())
                    {
                        int old_currencyfx = id_currencyfx;
                        if (db.app_currencyfx.Where(x => x.id_currency == value && x.is_active && x.id_company == CurrentSession.Id_Company).FirstOrDefault() != null)
                        {
                            id_currencyfx = db.app_currencyfx.Where(x => x.id_currency == value && x.is_active && x.id_company == CurrentSession.Id_Company).FirstOrDefault().id_currencyfx;
                            RaisePropertyChanged("id_currencyfx");
                        }
                        else
                        {
                            id_currencyfx = db.app_currencyfx.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company).FirstOrDefault().id_currencyfx;
                            RaisePropertyChanged("id_currencyfx");
                        }
                        RaisePropertyChanged("value");
                    }
                }
            }
        }

        private int _id_currency;

        [NotMapped]
        public int Default_id_currencyfx { get; set; }

        public int id_currencyfx
        {
            get
            {
                return _id_currencyfx;
            }
            set
            {
                if (_id_currencyfx != value)
                {
                    int old_currencyfx = 0;
                    old_currencyfx = _id_currencyfx;
                    _id_currencyfx = value;

                    if (_id_currencyfx > 0)
                    {
                        if (this.value > 0)
                        {
                            using (db db = new db())
                            {
                                if (db.app_currencyfx.Where(x => x.id_currencyfx == old_currencyfx && x.id_company == CurrentSession.Id_Company).FirstOrDefault() != null)
                                {
                                    app_currencyfx oldfx = db.app_currencyfx.Where(x => x.id_currencyfx == old_currencyfx && x.id_company == CurrentSession.Id_Company).FirstOrDefault();
                                    if (db.app_currencyfx.Where(x => x.id_currencyfx == _id_currencyfx && x.id_company == CurrentSession.Id_Company).FirstOrDefault() != null)
                                    {
                                        app_currencyfx newfx = db.app_currencyfx.Where(x => x.id_currencyfx == _id_currencyfx && x.id_company == CurrentSession.Id_Company).FirstOrDefault();

                                        if (oldfx.id_currency != newfx.id_currency)
                                        {
                                            this.value = Currency.convert_Values(this.value, old_currencyfx, _id_currencyfx, App.Modules.Sales);
                                            RaisePropertyChanged("value");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int _id_currencyfx;

        [NotMapped]
        public decimal Max_Value { get; set; }

        /// <summary>
        ///
        /// </summary>
        [NotMapped]
        public App.Names App_Name { get; set; }

        [NotMapped]
        public int id_payment_schedual { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Required]
        [CustomValidation(typeof(Class.EntityValidation), "CheckId")]
        public int id_payment_type
        {
            get
            {
                //If PaymentType is Null. Look for Default Payment Type.
                if (_id_payment_type == 0)
                {
                    using (db db = new db())
                    {
                        payment_type payment_type = db.payment_type.Where(x => x.id_company == CurrentSession.Id_Company && x.is_default).FirstOrDefault();
                        if (payment_type != null)
                        {
                            _id_payment_type = payment_type.id_payment_type;
                        }
                    }
                }

                return _id_payment_type;
            }
            set
            {
                if (_id_payment_type != value)
                {
                    _id_payment_type = value;
                    RaisePropertyChanged("id_payment_type");
                }
            }
        }

        private int _id_payment_type;

        /// <summary>
        ///
        /// </summary>
        public short? payment_type_ref { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Required]
        public decimal value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value > 0)
                {
                    if (_value != value)
                    {
                        _value = value;
                        RaisePropertyChanged("value");

                        if (payment != null)
                        {
                            ValueInDefaultCurrency = Currency.convert_Values(value, id_currencyfx, payment.id_currencyfx, App.Modules.Sales);
                            RaisePropertyChanged("ValueInDefaultCurrency");
                        }
                    }
                }
            }
        }

        private decimal _value;

        /// <summary>
        ///
        /// </summary>
        [NotMapped]
        public decimal ValueInDefaultCurrency
        {
            get
            {
                if (_ValueInDefaultCurrency == 0)
                {
                    if (payment != null)
                    {
                        if (payment.State != System.Data.Entity.EntityState.Added || payment.State != System.Data.Entity.EntityState.Modified)
                        {
                            decimal amount = 0;
                            foreach (payment_detail payment_detail in payment.payment_detail)
                            {
                                amount += Currency.convert_Values(payment_detail.value, payment_detail.id_currencyfx, payment.id_currencyfx, App.Modules.Sales);
                            }

                            this.value = payment.GrandTotal - amount;

                            return payment.GrandTotal - amount;
                        }
                    }
                }

                return _ValueInDefaultCurrency;
            }
            set
            {
                if (_ValueInDefaultCurrency != value)
                {
                    _ValueInDefaultCurrency = value;
                    RaisePropertyChanged("ValueInDefaultCurrency");

                    //To refresh header for payment toal in DefaultCurrency.
                    payment.RaisePropertyChanged("GrandTotalDetail");
                }
            }
        }

        private decimal _ValueInDefaultCurrency;

        /// <summary>
        ///
        /// </summary>
        public DateTime trans_date { get; set; }

        public long? cloud_id { get; set; }

        public bool is_finalize { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int? id_range
        {
            get
            {
                return _id_range;
            }
            set
            {
                if (_id_range != value)
                {
                    _id_range = value;

                    using (db db = new db())
                    {
                        app_document_range _app_range = db.app_document_range.Where(x => x.id_range == _id_range && x.id_company == CurrentSession.Id_Company).FirstOrDefault();

                        if (_app_range != null)
                        {
                            if (payment != null)
                            {
                                if (payment.id_branch!=null)
                                {
                                    Brillo.Logic.Range.branch_Code = CurrentSession.GetBranchCode((int)payment.id_branch);
                                }
                                if (payment.id_terminal != null)
                                {
                                    Brillo.Logic.Range.terminal_Code = CurrentSession.GetTerminalCode((int)payment.id_terminal);
                                }
                            }

                            NumberWatermark = Brillo.Logic.Range.calc_Range(_app_range, false);
                            RaisePropertyChanged("NumberWatermark");
                        }
                    }
                }
            }
        }

        #region Document Range => Navigation & Variables

        private int? _id_range;
        public virtual app_document_range app_document_range { get; set; }

        #endregion Document Range => Navigation & Variables

        /// <summary>
        ///
        /// </summary>
        public string payment_type_number { get; set; }

        /// <summary>
        ///
        /// </summary>
        [NotMapped]
        public string NumberWatermark { get; set; }

        public string comment { get; set; }
        public virtual payment payment { get; set; }
        public virtual payment_type payment_type { get; set; }

        public virtual app_account app_account { get; set; }
        public virtual app_bank app_bank { get; set; }
        public virtual app_currencyfx app_currencyfx { get; set; }

        public virtual ICollection<payment_schedual> payment_schedual { get; set; }
        public virtual ICollection<payment_type_detail> payment_type_detail { get; set; }
        public virtual ICollection<app_account_detail> app_account_detail { get; set; }
        public virtual ICollection<app_account_detail_archive> app_account_detail_archive { get; set; }

        #region Validation

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

                // iterate over all of the properties
                // of this object - aggregating any validation errors
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    String propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }

        [NotMapped]
        public bool IsLocked
        {
            get { return _IsLocked; }
            set { _IsLocked = value; RaisePropertyChanged("IsLocked"); }
        }

        private bool _IsLocked = false;

        public string this[string columnName]
        {
            get
            {
                // apply property level validation rules
                if (columnName == "value")
                {
                    if (value == 0)
                        return "Amount needs to be filled";
                }
                if (columnName == "value")
                {
                    if (Max_Value > 0)
                    {
                        if (value > Max_Value)
                            return "Amount cannot be higher than Credit Note balace: " + Math.Round(Max_Value, 2);
                    }
                    if (payment.Balance > 0)
                    {
                        if (payment.GrandTotalDetail > payment.Balance)
                            return "Amount is higher than Payment Balance: " + Math.Round(payment.Balance, 2);
                    }
                }

                if (columnName == "id_payment_type")
                {
                    if (id_payment_type == 0)
                        return "Payment Type needs to be selected";
                }
                return "";
            }
        }

        #endregion Validation
    }
}