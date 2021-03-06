﻿using entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Sales
{
    public partial class Packing : Page
    {
        private PackingListDB dbContext = new PackingListDB();

        private app_branch app_branch;

        private CollectionViewSource item_movementViewSource;
        private CollectionViewSource inventoryViewSource, sales_packingViewSource;

        public string InvoiceCode
        {
            get { return _InvoiceCode; }
            set
            {
                if (_InvoiceCode != value)
                {
                    _InvoiceCode = value;
                }
            }
        }

        private string _InvoiceCode;

        public Packing()
        {
            InitializeComponent();
            item_movementViewSource = ((CollectionViewSource)(FindResource("item_movementViewSource")));
            inventoryViewSource = ((CollectionViewSource)(FindResource("inventoryViewSource")));
            cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(dbContext, entity.App.Names.PackingList, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);

            sales_packing sales_packing = new sales_packing();
            dbContext.sales_packing.Add(sales_packing);
            sales_packingViewSource = ((CollectionViewSource)(FindResource("sales_packingViewSource")));
            sales_packingViewSource.Source = dbContext.sales_packing.Local;
            sales_packingViewSource.View.MoveCurrentToLast();

            app_branch = dbContext.app_branch.Where(x => x.id_branch == CurrentSession.Id_Branch).FirstOrDefault();
            cbxLocation.ItemsSource = app_branch.app_location.ToList();
        }

        private void ListProducts(object sender, EventArgs e)
        {
            if (InvoiceCode != string.Empty)
            {
                List<sales_invoice_detail> sales_invoice_detailLIST = dbContext.sales_invoice_detail
                    .Where(x => x.sales_invoice.number.Contains(InvoiceCode) &&
                        //Contado (Cash) + Payment Made
                        (
                           (x.sales_invoice.payment_schedual.Sum(z => z.credit) > 0 && x.sales_invoice.app_contract.app_contract_detail.Sum(z => z.interval) == 0)
                        ||
                        //Credit
                        (x.sales_invoice.app_contract.app_contract_detail.Sum(y => y.interval) > 0)
                        ) &&
                         x.sales_invoice.status == Status.Documents_General.Approved).ToList();

                List<item_movement> item_movementLIST = new List<item_movement>();

                foreach (sales_invoice_detail sales_invoice_detail in sales_invoice_detailLIST.Where(x => x.item.item_product.Count > 0))
                {
                    item_movement item_movement = new item_movement();

                    item_movement.trans_date = DateTime.Now;
                    item_movement.id_item_product = sales_invoice_detail.item.item_product.FirstOrDefault().id_item_product;
                    item_movement.item_product = sales_invoice_detail.item.item_product.FirstOrDefault();
                    item_movement.id_sales_invoice_detail = sales_invoice_detail.id_sales_invoice_detail;
                    item_movement.sales_invoice_detail = sales_invoice_detail;
                    item_movement.debit = 0;
                    item_movement.credit = 0;
                    item_movement.status = Status.Stock.InStock;
                    item_movement.timestamp = DateTime.Now;
                    item_movement.State = EntityState.Added;

                    if (sales_invoice_detail.id_location != null || sales_invoice_detail.id_location > 0)
                    {
                        item_movement.id_location = (int)sales_invoice_detail.id_location;
                    }
                    else
                    {
                        //find location code by using checkbox.
                        item_movement.id_location = app_branch.app_location.Where(x => x.is_default).FirstOrDefault().id_location;
                    }

                    if (sales_invoice_detail.item_movement != null)
                    {
                        item_movement.debit = sales_invoice_detail.quantity - sales_invoice_detail.item_movement.Sum(x => x.debit);
                    }
                    else
                    {
                        item_movement.debit = sales_invoice_detail.quantity;
                    }

                    if (item_movement.debit > 0)
                    {
                        item_movementLIST.Add(item_movement);
                    }
                }

                dbContext.item_movement.AddRange(item_movementLIST);
                item_movementViewSource.Source = item_movementLIST;
                item_movementViewSource.View.Refresh();
            }
        }

        private void StockList(object sender, EventArgs e)
        {
            if (item_movementViewSource.View != null)
            {
                if (item_movementViewSource.View.CurrentItem != null)
                {
                    item_product _item_product = (item_movementViewSource.View.CurrentItem as item_movement).item_product;
                    if (_item_product != null && inventoryViewSource != null)
                    {
                        using (StockDB StockDB = new StockDB())
                        {
                            var movement =
                               (from items in StockDB.items
                                join item_product in StockDB.item_product on items.id_item equals item_product.id_item
                                    into its
                                from p in its
                                join item_movement in StockDB.item_movement on p.id_item_product equals item_movement.id_item_product
                                into IMS
                                from a in IMS
                                join AM in StockDB.app_branch on a.app_location.id_branch equals AM.id_branch
                                where a.status == Status.Stock.InStock
                                && a.id_item_product == _item_product.id_item_product
                                && a.trans_date <= DateTime.Now
                                && a.app_location.id_branch == CurrentSession.Id_Branch
                                group a by new { a.item_product, a.app_location }
                                    into last
                                select new
                                {
                                    code = last.Key.item_product.item.code,
                                    name = last.Key.item_product.item.name,
                                    location = last.Key.app_location.name,
                                    itemid = last.Key.item_product.item.id_item,
                                    quantity = last.Sum(x => x.credit) - last.Sum(x => x.debit),
                                    id_item_product = last.Key.item_product.id_item_product,
                                    measurement = last.Key.item_product.item.app_measurement.code_iso,
                                    id_location = last.Key.app_location.id_location
                                }).ToList().OrderBy(y => y.name);

                            inventoryViewSource.Source = movement;
                        }
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            sales_packing sales_packing = sales_packingViewSource.View.CurrentItem as sales_packing;
            item_movement item_movement = item_movementViewSource.View.CurrentItem as item_movement;

            sales_packing.contact = item_movement.sales_invoice_detail.sales_invoice.contact;
            sales_packing.id_contact = item_movement.sales_invoice_detail.sales_invoice.id_contact;

            foreach (item_movement _item_movement in item_movementViewSource.View.OfType<item_movement>().ToList())
            {
                //Creates a Packing Detail
                sales_packing_detail sales_packing_detail = new sales_packing_detail();
                sales_packing_detail.id_location = _item_movement.id_location;
                sales_packing_detail.id_item = _item_movement.item_product.id_item;
                sales_packing_detail.quantity = _item_movement.debit;
                sales_packing_detail.sales_packing = sales_packing;

                //Creates relationship with Sales Invoice.
                sales_packing_relation sales_packing_relation = new entity.sales_packing_relation();
                sales_packing_relation.sales_packing_detail = sales_packing_detail;
                sales_packing_relation.sales_invoice_detail = _item_movement.sales_invoice_detail;
                sales_packing.id_opportunity = _item_movement.sales_invoice_detail.sales_invoice.id_opportunity;

                dbContext.sales_packing_relation.Add(sales_packing_relation);
                sales_packing.sales_packing_detail.Add(sales_packing_detail);

                //Relates the Item Movement to Packing
                _item_movement.sales_packing_detail = sales_packing_detail;
            }
            if (sales_packing.number == null && sales_packing.id_range > 0)
            {
                entity.Brillo.Logic.Range.branch_Code = dbContext.app_branch.Where(x => x.id_branch == sales_packing.id_branch).FirstOrDefault().code;
                entity.Brillo.Logic.Range.terminal_Code = dbContext.app_terminal.Where(x => x.id_terminal == sales_packing.id_terminal).FirstOrDefault().code;
                app_document_range app_document_range = dbContext.app_document_range.Where(x => x.id_range == sales_packing.id_range).FirstOrDefault();
                sales_packing.number = entity.Brillo.Logic.Range.calc_Range(app_document_range, true);
                sales_packing.RaisePropertyChanged("number");
                sales_packing.is_issued = true;

                entity.Brillo.Document.Start.Automatic(sales_packing, app_document_range);
            }
            else
            {
                sales_packing.is_issued = false;
            }
            dbContext.SaveChanges();
            item_movementViewSource.Source = null;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            btnCancel_Click(null, null);
            item_movementViewSource.Source = null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Cancel Code.
        }
    }
}