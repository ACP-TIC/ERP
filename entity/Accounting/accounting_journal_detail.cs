﻿namespace entity
{
    using entity;
    using System.Linq;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class accounting_journal_detail : Audit
    {
        public accounting_journal_detail()
        {
            trans_date = DateTime.Now;
            id_company = Properties.Settings.Default.company_ID;
            id_user = Properties.Settings.Default.user_ID;
            is_head = true;
            debit = 0;
            credit = 0;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_journal_detail { get; set; }
        public int id_journal { get; set; }
        public App.Names id_application { get; set; }
        public int id_chart { get; set; }

        public int id_currencyfx
        {
            get
            {
                return _id_currencyfx;
            }
            set
            {
                _id_currencyfx = value;
                using(db db = new db())
                {
                    if (db.app_currencyfx.Where(x => x.id_currencyfx == value).FirstOrDefault() != null)
                    {
                        // app_currencyfx = db.app_currencyfx.Where(x => x.id_currencyfx == value).FirstOrDefault();
                        RaisePropertyChanged("app_currencyfx");
                    }
                }             
            }
        }
        int _id_currencyfx;
        public decimal debit
        {
            get { return _debit; }
            set
            {
                if (value > 0)
                {
                    _credit = 0;
                    RaisePropertyChanged("credit");
                    _debit = value;
                };
            }
        }
        decimal _debit;
        public decimal credit
        {
            get { return _credit; }
            set
            {
                if (value > 0)
                {
                    _debit = 0;
                    RaisePropertyChanged("debit");
                    _credit = value;
                };
            }
        }
        decimal _credit;
        public DateTime trans_date { get; set; }

        public virtual app_currencyfx app_currencyfx { get; set; }

        public virtual accounting_chart accounting_chart { get; set; }
        public virtual accounting_journal accounting_journal { get; set; }
    }
}
