﻿namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class item_request : Audit
    {
        public item_request()
        {
            item_request_detail = new List<item_request_detail>();
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            status = Status.Documents_General.Pending;
            request_date = DateTime.Now;
            timestamp = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_item_request { get; set; }

        public int? id_project { get; set; }
        public int? id_sales_order { get; set; }
        public int? id_production_order { get; set; }
        public int? id_currency { get; set; }
        public int? id_branch { get; set; }
        public int? id_department { get; set; }

        public DateTime request_date { get; set; }
        public string name { get; set; }
        public string comment { get; set; }

        public Status.Documents_General status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("status");
            }
        }

        private Status.Documents_General _status;

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
                    if (State == System.Data.Entity.EntityState.Added || State == System.Data.Entity.EntityState.Modified || State == 0)
                    {
                        using (db db = new db())
                        {
                            app_document_range _app_range = db.app_document_range.Find(_id_range);
                            if (_app_range != null)
                            {
                                if (app_branch != null)
                                {
                                    Brillo.Logic.Range.branch_Code = CurrentSession.Branches.Where(x => x.id_branch == id_branch).FirstOrDefault().code;
                                }

                                string UserCode = db.security_user.Where(x => x.id_user == id_user).Select(x => x.code).FirstOrDefault();
                                if (!string.IsNullOrEmpty(UserCode))
                                {
                                    Brillo.Logic.Range.user_Code = UserCode;
                                }

                                string ProjectCode = db.projects.Where(x => x.id_project == id_project).Select(x => x.code).FirstOrDefault();
                                if (!string.IsNullOrEmpty(ProjectCode))
                                {
                                    Brillo.Logic.Range.project_Code = ProjectCode;
                                }

                                NumberWatermark = Brillo.Logic.Range.calc_Range(_app_range, false);
                                RaisePropertyChanged("NumberWatermark");
                            }
                        }
                    }
                }
            }
        }

        private int? _id_range;

        /// <summary>
        ///
        /// </summary>
        public string number { get; set; }

        /// <summary>
        ///
        /// </summary>
        [NotMapped]
        public string NumberWatermark { get; set; }

        [NotMapped]
        public int TotalSelected { get; set; }

        public bool is_archived { get { return _is_archived; } set { _is_archived = value; RaisePropertyChanged("is_archived"); } }
        private bool _is_archived;

        public virtual sales_order sales_order { get; set; }
        public virtual project project { get; set; }
        public virtual production_order production_order { get; set; }
        public virtual ICollection<item_request_detail> item_request_detail { get; set; }
        public virtual ICollection<item_transfer> item_transfer { get; set; }
        public virtual security_user request_user { get; set; }
        public virtual security_user given_user { get; set; }
        public virtual app_currency app_currency { get; set; }
        public virtual app_department app_department { get; set; }
        public virtual app_branch app_branch { get; set; }
       // public virtual app_document_range app_document_range { get; set; }

        public void GetTotalDecision()
        {
            int i = 0;

            foreach (item_request_detail detail in item_request_detail)
            {
                i += detail.GetTotalDecision();
            }

            TotalSelected = i;
            RaisePropertyChanged("TotalSelected");
        }
    }
}