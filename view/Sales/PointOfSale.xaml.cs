﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;
using entity;
using System.Data;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Cognitivo.Sales
{
    public partial class PointOfSale : Page
    {
        entity.SalesInvoiceDB SalesInvoiceDB = new entity.SalesInvoiceDB();
        CollectionViewSource sales_invoiceViewSource;
        //  CollectionViewSource sales_invoicesales_invoice_detailViewSource;
        public PointOfSale()
        {
            InitializeComponent();
        }

        #region Buttons

        private void btnClient_Click(object sender, EventArgs e)
        {
            tabContact.IsSelected = true;
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            tabSales.IsSelected = true;
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            tabPayment.IsSelected = true;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;

            if (SalesInvoiceDB.app_contract.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_default).FirstOrDefault() != null)
            {
                if (SalesInvoiceDB.app_contract.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_default).FirstOrDefault().app_condition != null)
                {
                    sales_invoice.id_condition = SalesInvoiceDB.app_contract.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_default).FirstOrDefault().app_condition.id_condition;
                    sales_invoice.id_contract = SalesInvoiceDB.app_contract.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_default).FirstOrDefault().id_contract;
                }
            }

            if (SalesInvoiceDB.app_currency.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_priority).FirstOrDefault() != null)
            {
                if (SalesInvoiceDB.app_currency.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_priority).FirstOrDefault().app_currencyfx.Where(y=>y.is_active).FirstOrDefault() != null)
                {
                    sales_invoice.id_currencyfx = SalesInvoiceDB.app_currency.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.is_priority).FirstOrDefault().app_currencyfx.Where(y => y.is_active).FirstOrDefault().id_currencyfx;
                    
                }
            }
            if (SalesInvoiceDB.app_document_range.Where(x=>x.is_active && x.id_company==CurrentSession.Id_Company && x.app_document.id_application==entity.App.Names.SalesInvoice).FirstOrDefault()!=null)
            {
                sales_invoice.id_range = SalesInvoiceDB.app_document_range.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company && x.app_document.id_application == entity.App.Names.SalesInvoice).FirstOrDefault().id_range;
            }


            SalesInvoiceDB.SaveChanges();
            SalesInvoiceDB.Approve(true);

            Settings SalesSettings = new Settings();
            sales_invoice Newsales_invoice = SalesInvoiceDB.New(SalesSettings.TransDate_Offset);


            SalesInvoiceDB.sales_invoice.Add(Newsales_invoice);

            sales_invoiceViewSource.View.MoveCurrentToLast();
            tabContact.IsSelected = true;
            sbxContact.Text = "";
            //Run approve code here.
        }

        #endregion

        #region SmartBox Selection

        private void sbxContact_Select(object sender, RoutedEventArgs e)
        {
            if (sbxContact.ContactID > 0)
            {
                contact contact = SalesInvoiceDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                sales_invoice.id_contact = contact.id_contact;
                sales_invoice.contact = contact;


            }
        }


        private void sbxItem_Select(object sender, RoutedEventArgs e)
        {
            if (sbxItem.ItemID > 0)
            {
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                item item = SalesInvoiceDB.items.Where(x => x.id_item == sbxItem.ItemID).FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_invoice != null)
                {
                    Task Thread = Task.Factory.StartNew(() => select_Item(sales_invoice, item));
                }
            }
        }

        private void select_Item(sales_invoice sales_invoice, item item)
        {
            Settings SalesSettings = new Settings();
            if (sales_invoice.sales_invoice_detail.Where(a => a.id_item == item.id_item).FirstOrDefault() == null || SalesSettings.AllowDuplicateItem)
            {
                sales_invoice_detail _sales_invoice_detail = new sales_invoice_detail();
                _sales_invoice_detail.sales_invoice = sales_invoice;
                _sales_invoice_detail.Contact = sales_invoice.contact;
                _sales_invoice_detail.item_description = item.description;
                _sales_invoice_detail.item = item;
                _sales_invoice_detail.id_item = item.id_item;
                _sales_invoice_detail.quantity += 1;

                sales_invoice.sales_invoice_detail.Add(_sales_invoice_detail);
            }
            else
            {
                sales_invoice_detail sales_invoice_detail = sales_invoice.sales_invoice_detail.Where(a => a.id_item == item.id_item).FirstOrDefault();
                sales_invoice_detail.quantity += 1;
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {

                CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                sales_invoicesales_invoice_detailViewSource.View.Refresh();


                sbxItem.Focus();
            }));
        }

        #endregion

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Settings SalesSettings = new Settings();
            if (SalesSettings.FilterByBranch)
            {
                await SalesInvoiceDB.sales_invoice.Where(a => a.id_company == CurrentSession.Id_Company && a.id_branch == CurrentSession.Id_Company
                                               && (a.is_head == true)).OrderByDescending(x => x.trans_date).LoadAsync();

            }
            else
            {
                await SalesInvoiceDB.sales_invoice.Where(a => a.id_company == CurrentSession.Id_Company
                                              && (a.is_head == true)).OrderByDescending(x => x.trans_date).LoadAsync();
            }


            await Dispatcher.InvokeAsync(new Action(() =>
            {
                sales_invoiceViewSource = ((CollectionViewSource)(FindResource("sales_invoiceViewSource")));
                sales_invoiceViewSource.Source = SalesInvoiceDB.sales_invoice.Local;
                sales_invoice sales_invoice = SalesInvoiceDB.New(SalesSettings.TransDate_Offset);
                SalesInvoiceDB.sales_invoice.Add(sales_invoice);

                sales_invoiceViewSource.View.MoveCurrentToLast();
            }));
            SalesInvoiceDB.app_vat_group.Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToList();
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                CollectionViewSource app_vat_groupViewSource = FindResource("app_vat_groupViewSource") as CollectionViewSource;
                app_vat_groupViewSource.Source = SalesInvoiceDB.app_vat_group.Local;
            }));

        }


    }
}
