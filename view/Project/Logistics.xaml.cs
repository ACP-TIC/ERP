﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;
using entity;
using System.Data.Entity.Validation;
namespace Cognitivo.Production
{
    public partial class Logistics : Page
    {
        dbContext _entity = new dbContext();
        CollectionViewSource project_ViewSource,contractViewSource;
        entity.Properties.Settings _Setting = new entity.Properties.Settings();
        List<project_task> Project_task = new List<project_task>();

        //SetIsEnableProperty
        public static readonly DependencyProperty SetIsEnableProperty =
            DependencyProperty.Register("SetIsEnable", typeof(bool), typeof(Logistics),
            new FrameworkPropertyMetadata(false));
        public Logistics()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            project_ViewSource = ((CollectionViewSource)(this.FindResource("projectViewSource")));
            _entity.db.projects.Where(a => a.is_active == true && a.id_company == _Setting.company_ID).Load();
            project_ViewSource.Source = _entity.db.projects.Local;

            project_ViewSource.Source = _entity.db.projects.Local;

            contractViewSource = (CollectionViewSource)this.FindResource("contractViewSource");
            contractViewSource.Source = _entity.db.app_contract.Where(a => a.is_active == true && a.id_company == _Setting.company_ID).ToList();

            CollectionViewSource conditionViewSource = (CollectionViewSource)this.FindResource("conditionViewSource");
            conditionViewSource.Source = _entity.db.app_condition.Where(a => a.is_active == true && a.id_company == _Setting.company_ID).OrderBy(a => a.name).ToList();
        }

        private void projectDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int _id_project = 0;

            _id_project = ((project)project_ViewSource.View.CurrentItem).id_project;

            var productlist = (from IT in _entity.db.project_task
                               where IT.items.id_item_type == item.item_type.Product
                               join IK in _entity.db.item_product on IT.id_item equals IK.id_item
                               join IO in _entity.db.item_movement on IK.id_item_product equals IO.id_item_product
                               group IT by new { IK, IO }
                                   into last
                                   select new
                                   {
                                       _id_item = last.Max(x => x.id_item),
                                       _id_parent = last.Max(x=>x.parent.id_project_task),
                                       _id_project = last.Max(x => x.id_project),
                                       _code = last.Max(x => x.items.code),
                                       _name = last.Max(x => x.items.name),
                                       _avlqtyColumn = last.Key.IO.credit - last.Key.IO.debit,
                                       _ordered_quantity = last.Sum(x => x.quantity_est) - last.Max(x => x.purchase_invoice_detail.Count() != 0 ? x.purchase_invoice_detail.Where(q => q.id_item == x.id_item).Sum(j => j.quantity) : 0)

                                   }).ToList();


            productDataGrid.ItemsSource = productlist.Where(x => x._id_project == _id_project).ToList();

            var servicelist = (from IT in _entity.db.project_task
                               where IT.items.id_item_type == item.item_type.Service
                               join IK in _entity.db.item_product on IT.id_item equals IK.id_item
                               join IO in _entity.db.item_movement on IK.id_item_product equals IO.id_item_product
                               group IT by new { IK, IO }
                                   into last
                                   select new
                                   {
                                       _id_item = last.Max(x => x.id_item),
                                       _id_project = last.Max(x => x.id_project),
                                       _code = last.Max(x => x.items.code),
                                       _name = last.Max(x => x.items.name),
                                       _avlqtyColumn = last.Key.IO.credit - last.Key.IO.debit,
                                       _ordered_quantity = last.Sum(x => x.quantity_est) - last.Max(x => x.purchase_invoice_detail.Count() != 0 ? x.purchase_invoice_detail.Where(q => q.id_item == x.id_item).Sum(j => j.quantity) : 0)

                                   }).ToList();


            serviceDataGrid.ItemsSource = servicelist.Where(x => x._id_project == _id_project).ToList();
            var rawlist = (from IT in _entity.db.project_task
                           where IT.items.id_item_type == item.item_type.RawMaterial
                           join IK in _entity.db.item_product on IT.id_item equals IK.id_item
                           join IO in _entity.db.item_movement on IK.id_item_product equals IO.id_item_product
                           group IT by new { IK, IO }
                               into last
                               select new
                               {
                                   _id_item = last.Max(x => x.id_item),
                                   _id_project = last.Max(x => x.id_project),
                                   _code = last.Max(x => x.items.code),
                                   _name = last.Max(x => x.items.name),
                                   _avlqtyColumn = last.Key.IO.credit - last.Key.IO.debit,
                                   _ordered_quantity = last.Sum(x => x.quantity_est) - last.Max(x => x.purchase_invoice_detail.Count() != 0 ? x.purchase_invoice_detail.Where(q => q.id_item == x.id_item).Sum(j => j.quantity) : 0)

                               }).ToList();

            rawmaterialDataGrid.ItemsSource = rawlist.Where(x => x._id_project == _id_project).ToList();

            var capitallist = (from IT in _entity.db.project_task
                               where IT.items.id_item_type == item.item_type.Product
                               join IK in _entity.db.item_asset on IT.id_item equals IK.id_item
                               join IO in _entity.db.item_movement on IK.id_item_asset equals IO.id_item_product
                               group IT by new { IK, IO }
                                   into last
                                   select new
                                   {
                                       _id_item = last.Max(x => x.id_item),
                                       _id_project = last.Max(x => x.id_project),
                                       _code = last.Max(x => x.items.code),
                                       _name = last.Max(x => x.items.name),
                                       _avlqtyColumn = last.Key.IO.credit - last.Key.IO.debit,
                                       _ordered_quantity = last.Sum(x => x.quantity_est) - last.Max(x => x.purchase_invoice_detail.Count() != 0 ? x.purchase_invoice_detail.Where(q => q.id_item == x.id_item).Sum(j => j.quantity) : 0)

                                   }).ToList();


            assetDataGrid.ItemsSource = capitallist.Where(x => x._id_project == _id_project).ToList();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
             
                project objProject = project_ViewSource.View.CurrentItem as project;
                if (objProject != null)
                {
                    using (db dbTender = new db())
                    {
                        int idProject = objProject.id_project;
                        purchase_tender purchase_tender = new purchase_tender();
                        if (idProject > 0)
                            purchase_tender.id_project = idProject;
                        purchase_tender.name = txtName.Text;
                        //purchase_tender.code = Convert.ToInt16(txtCode.Text);
                        purchase_tender.comment = txtComment.Text;

                        purchase_tender_contact purchase_tender_contact = new purchase_tender_contact();
                        purchase_tender_contact.id_contact = Convert.ToInt32(objProject.id_contact);
                        purchase_tender_contact.id_contract = Convert.ToInt32(cbxContract.SelectedValue);
                        purchase_tender_contact.id_condition = Convert.ToInt32(cbxCondition.SelectedValue);
                        purchase_tender_contact.id_currencyfx = Convert.ToInt32(cbxCurrency.SelectedValue);
                        purchase_tender.purchase_tender_contact_detail.Add(purchase_tender_contact);


                        int id = ((project)projectDataGrid.SelectedItem).id_project;


                       // List<project_task> productlist = entity.db.project_task.Where(x => x.id_project == id && x.items.id_item_type != item.item_type.Task && x.selected == true).ToList();
                        foreach (project_task data in Project_task)
                        {

                            purchase_tender_item purchase_tender_item = new purchase_tender_item();
                            //purchase_tender_item.id_cost_center = dbTender.app_cost_center.Where(a => a.id_company == _Setting.company_ID && a.is_active == true && a.is_administrative == true).FirstOrDefault().id_cost_center;
                            int idItem = data.items.id_item;
                            purchase_tender_item.id_item = idItem;
                            purchase_tender_item.item_description = dbTender.items.Where(a => a.id_item == idItem).FirstOrDefault().name;
                            purchase_tender_item.quantity = (decimal)data.quantity_est;
                            purchase_tender.purchase_tender_item_detail.Add(purchase_tender_item);
                        }

                        dbTender.purchase_tender.Add(purchase_tender);
                        dbTender.SaveChanges();
                        //dbTender.Entry(purchase_tender).State = EntityState.Detached;
                        //entity.db.purchase_tender.Attach(purchase_tender);
                    }
                    lblCancel_MouseDown(null, null);
                }
                else
                {
                    MessageBox.Show("Error in getting Project details.", "Cognitivo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch { }
        }

        private void productDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int _id_project = 0;
            int _id_item = 0;
            dynamic obj = productDataGrid.SelectedItem;

            _id_item = obj._id_item;

            _id_project = ((project)project_ViewSource.View.CurrentItem).id_project;

            var tasklist = _entity.db.project_task.Where(x => x.id_project == _id_project && x.id_item == _id_item).ToList(); 
                            
                           


            taskDataGrid.ItemsSource = tasklist;
        }
        void AddCart(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (!Project_task.Contains((project_task)btn.Tag))
            {
                Project_task.Add((project_task)btn.Tag);
            }
            shoppingcartDataGrid.ItemsSource = Project_task;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                project objProject = project_ViewSource.View.CurrentItem as project;
                if (objProject != null)
                {
                    txtName.Text = string.Empty;
                    //txtCode.Text = string.Empty;
                    txtComment.Text = string.Empty;

                    int idContact = Convert.ToInt32(objProject.id_contact);
                    contact contact = _entity.db.contacts.Where(a => a.id_contact == idContact).Include("app_contract").Include("app_currency").FirstOrDefault();
                    if (contact.app_contract != null)
                    {
                        cbxCondition.SelectedValue = Convert.ToInt32(contact.app_contract.app_condition.id_condition);
                        cbxContract.SelectedValue = Convert.ToInt32(contact.app_contract.id_contract);
                    }
                    if (contact.app_currency != null && contact.app_currency.app_currencyfx != null)
                    {
                        cbxCurrency.SelectedValue = Convert.ToInt32(contact.app_currency.app_currencyfx.Where(a => a.is_active == true).FirstOrDefault().id_currencyfx);
                    }
                    crud_modal.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Error in getting Project details.", "Cognitivo", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch { }
        }

        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            crud_modal.Visibility = System.Windows.Visibility.Hidden;
        }

        private void cbxCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Contract
            if (cbxCondition.SelectedItem != null)
            {
                app_condition app_condition = cbxCondition.SelectedItem as app_condition;
                contractViewSource.View.Filter = i =>
                {
                    app_contract objContract = (app_contract)i;
                    if (objContract.id_condition == app_condition.id_condition)
                    { return true; }
                    else
                    { return false; }
                };
                cbxContract.SelectedIndex = 0;
            }
        }
    }
}
