﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using entity;
using System.Data.Entity;

namespace cntrl
{
    /// <summary>
    /// Interaction logic for SalesInvocie.xaml
    /// </summary>
    public partial class SalesInvoice : UserControl
    {

        public project project
        {
            get { return _project; }
            set
            {
                if (_project != value)
                {
                    _project = value;

                    if (_project != null)
                    {
                        if (_project.contact != null)
                        {
                            contact contact = _project.contact;

                            if (contact.app_contract != null)
                                cbxCondition.SelectedValue = contact.app_contract.id_condition;
                            //Contract
                            if (contact.id_contract != null)
                                cbxContract.SelectedValue = Convert.ToInt32(contact.id_contract);

                            cbxCurrency.get_ActiveRateXContact(ref contact);
                        }
                    }
                }
            }
        }

        private project _project;
        public db db { get; set; }

        public SalesInvoice()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (project != null)
            {
                List<project_task> project_task = project.project_task.Where(x => x.IsSelected).ToList();

                sales_invoice sales_invoice = new entity.sales_invoice();
                sales_invoice.id_contact = (int)project.id_contact;
                sales_invoice.contact = db.contacts.Where(x => x.id_contact == (int)project.id_contact).FirstOrDefault();

                sales_invoice.id_project = project.id_project;
                sales_invoice.id_condition = (int)cbxCondition.SelectedValue;
                sales_invoice.id_contract = (int)cbxContract.SelectedValue;
                sales_invoice.id_currencyfx = (int)cbxCurrency.SelectedValue;
                sales_invoice.comment = "Project -> " + project.name;

                sales_invoice_detail sales_invoice_detail = null;


                sales_invoice_detail = new sales_invoice_detail();
                sales_invoice_detail.id_sales_invoice = sales_invoice.id_sales_invoice;
                sales_invoice_detail.sales_invoice = sales_invoice;
                sales_invoice_detail.id_item = (int)sbxItem.ItemID;
                item item = db.items.Where(x => x.id_item == sbxItem.ItemID).FirstOrDefault();
                if (item != null)
                {
                    sales_invoice_detail.item = item;
                }
                sales_invoice_detail.id_vat_group = CurrentSession.Get_VAT_Group().Where(x => x.is_default).FirstOrDefault().id_vat_group;
                
             
                sales_invoice_detail.id_item = (int)sbxItem.ItemID;
                sales_invoice_detail.quantity = 1;
                sales_invoice_detail.UnitPrice_Vat = Convert.ToDecimal(txtvalue.Text);


                sales_invoice.sales_invoice_detail.Add(sales_invoice_detail);


                sales_invoice.State = EntityState.Added;
                sales_invoice.IsSelected = true;


                crm_opportunity crm_opportunity = new crm_opportunity();
                crm_opportunity.id_contact = sales_invoice.id_contact;
                crm_opportunity.id_currency = sales_invoice.id_currencyfx;
                crm_opportunity.value = sales_invoice.GrandTotal;

                crm_opportunity.sales_invoice.Add(sales_invoice);
                db.crm_opportunity.Add(crm_opportunity);
              
                db.SaveChanges();
                btnCancel_Click(null, null);
            }
        }

        private void btnCancel_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Grid parentGrid = (Grid)this.Parent;
                parentGrid.Children.Clear();
                parentGrid.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            { throw ex; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            db.app_contract.Where(a => a.is_active == true && a.id_company == entity.Properties.Settings.Default.company_ID).ToList();
            cbxContract.ItemsSource = db.app_contract.Local;

            db.app_condition.Where(a => a.is_active == true && a.id_company == entity.Properties.Settings.Default.company_ID).OrderBy(a => a.name).ToList();
            cbxCondition.ItemsSource = db.app_condition.Local;

            stackMain.DataContext = project;
        }

        private void cbxCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxCondition.SelectedItem != null)
            {

                app_condition app_condition = cbxCondition.SelectedItem as app_condition;
                cbxContract.ItemsSource = db.app_contract.Where(a => a.is_active == true
                                                                        && a.id_company == entity.Properties.Settings.Default.company_ID
                                                                        && a.id_condition == app_condition.id_condition).ToList();
                cbxContract.SelectedIndex = 0;
            }
        }
    }
}
