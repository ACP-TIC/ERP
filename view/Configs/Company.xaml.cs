﻿using entity;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Configs
{
    public partial class Company : Page
    {
        private dbContext entity = new dbContext();
        private CollectionViewSource app_companyViewSource, app_companyapp_company_interestViewSource;

        public Company()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                app_companyViewSource = ((CollectionViewSource)(this.FindResource("app_companyViewSource")));
                app_companyapp_company_interestViewSource = ((CollectionViewSource)(this.FindResource("app_companyapp_company_interestViewSource")));
                entity.db.app_company.OrderByDescending(a => a.is_active).Load();
                app_companyViewSource.Source = entity.db.app_company.Local;
                app_companyapp_company_interestViewSource.View.Refresh();
            }
            catch
            {
                //toolBar.msgError(ex);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            cntrl.company objCompany = new cntrl.company();
            UserCurdConditions(ref objCompany);
            objCompany.EnterMode = cntrl.company.Mode.Add;

            app_company app_company = new app_company();
            entity.db.app_company.Add(app_company);
            app_companyViewSource.View.MoveCurrentToLast();
            objCompany.app_companyViewSource = app_companyViewSource;

            objCompany.objEntity = entity;

            crud_modal.Children.Add(objCompany);
        }

        private void pnl_Company_Edit_Click(object sender, int intCompanyId)
        {
            crud_modal.Visibility = System.Windows.Visibility.Visible;
            cntrl.company objCompany = new cntrl.company();
            UserCurdConditions(ref objCompany);
            objCompany.EnterMode = cntrl.company.Mode.Edit;

            app_companyViewSource.View.MoveCurrentTo(entity.db.app_company.Where(x => x.id_company == intCompanyId).FirstOrDefault());
            objCompany.app_companyViewSource = app_companyViewSource;

            objCompany.objEntity = entity;

            crud_modal.Children.Add(objCompany);
        }

        private void UserCurdConditions(ref cntrl.company objCompany)
        {
            objCompany.canedit = true;
            objCompany.candelete = true;

            //if (module > 0)
            //{
            //    if (security_curd != null)
            //    {
            //        objCompany.canedit = security_curd.can_update;
            //        objCompany.candelete = security_curd.can_delete;
            //    }
            //    else
            //    {
            //        objCompany.canedit = true;
            //        objCompany.candelete = true;
            //    }
            //}
            //else
            //{
            //    objCompany.canedit = true;
            //    objCompany.candelete = true;
            //}
        }
    }
}