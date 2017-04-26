﻿using entity.Brillo;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace entity.Controller.Purcahse
{
    public class PurchaseInvoice
    {
        public int NumberOfRecords;
        public db db { get; set; }

        public DateTime Start_Range
        {
            get { return _start_Range; }
            set
            {
                if (_start_Range != value)
                {
                    _start_Range = value;
                }
            }
        }
        private DateTime _start_Range = DateTime.Now.AddDays(-7);

        public DateTime End_Range
        {
            get { return _end_Range; }
            set
            {
                if (_end_Range != value)
                {
                    _end_Range = value;
                }
            }
        }
        private DateTime _end_Range = DateTime.Now.AddDays(+1);

       

        public PurchaseInvoice()
        {

        }

        public async void Load(bool filterbyBranch)
        {
           
            if (filterbyBranch)
            {
                await db.purchase_invoice.Where(a => a.id_company == CurrentSession.Id_Company && a.id_branch == CurrentSession.Id_Branch && a.is_archived == false).Include(x => x.contact).OrderByDescending(x => x.trans_date).ToListAsync();
            }
            else
            {
                await db.purchase_invoice.Where(a => a.id_company == CurrentSession.Id_Company && a.is_archived == false).Include(x => x.contact).OrderByDescending(x => x.trans_date).ToListAsync();
            }

        }
        #region Create

        public purchase_invoice Create(int TransDate_OffSet)
        {
            purchase_invoice purchase_invoice = new purchase_invoice()
            {
                State = EntityState.Added,
                status = Status.Documents_General.Pending,
                IsSelected = true,
                trans_type = Status.TransactionTypes.Normal,
                trans_date = DateTime.Now.AddDays(TransDate_OffSet),
                timestamp = DateTime.Now,

                //Navigation Properties
                app_currencyfx = db.app_currencyfx.Find(CurrentSession.Get_Currency_Default_Rate().id_currencyfx),
                app_branch = db.app_branch.Find(CurrentSession.Id_Branch)
            };

            //This is to skip query code in case of Migration. Helps speed up migrations.
          

            return purchase_invoice;
        }



        #endregion
        #region Save

        public int SaveChanges_and_Validate()
        {
            NumberOfRecords = 0;

            foreach (purchase_invoice purchase_invoice in db.purchase_invoice.Local)
            {
                if (purchase_invoice.IsSelected && purchase_invoice.Error == null)
                {
                    //Data Loading Code. If data is not set, then Cognitivo ERP should try to fill up.
                    if (purchase_invoice.contact.id_contract == 0)
                    {
                        purchase_invoice.contact.id_contract = purchase_invoice.id_contract;
                    }

                    if (purchase_invoice.contact.id_currency == 0)
                    {
                        purchase_invoice.contact.id_currency = purchase_invoice.app_currencyfx.id_currency;
                    }

                    if (purchase_invoice.contact.id_cost_center == 0 && purchase_invoice.purchase_invoice_detail.FirstOrDefault() != null)
                    {
                        purchase_invoice.contact.id_cost_center = purchase_invoice.purchase_invoice_detail.FirstOrDefault().id_cost_center;
                    }

                    if (purchase_invoice.State == EntityState.Added)
                    {
                        purchase_invoice.timestamp = DateTime.Now;
                        purchase_invoice.State = EntityState.Unchanged;
                        db.Entry(purchase_invoice).State = EntityState.Added;
                    }
                    else if (purchase_invoice.State == EntityState.Modified)
                    {
                        purchase_invoice.timestamp = DateTime.Now;
                        purchase_invoice.State = EntityState.Unchanged;
                        db.Entry(purchase_invoice).State = EntityState.Modified;
                    }
                    else if (purchase_invoice.State == EntityState.Deleted)
                    {
                        purchase_invoice.timestamp = DateTime.Now;
                        purchase_invoice.State = EntityState.Unchanged;
                        db.purchase_invoice.Remove(purchase_invoice);
                    }
                    NumberOfRecords += 1;
                }
                else if (purchase_invoice.State > 0)
                {
                    if (purchase_invoice.State != EntityState.Unchanged)
                    {
                        db.Entry(purchase_invoice).State = EntityState.Unchanged;
                    }
                }
            }
            return db.SaveChanges();
        }

        

        public bool CancelAllChanges()
        {
            if (MessageBox.Show(Localize.Question_Cancel, "Cognitivo ERP", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var entry in db.ChangeTracker.Entries())
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            {
                                entry.CurrentValues.SetValues(entry.OriginalValues);
                                entry.State = EntityState.Unchanged;
                                break;
                            }
                        case EntityState.Deleted:
                            {
                                entry.State = EntityState.Unchanged;
                                break;
                            }
                        case EntityState.Added:
                            {
                                entry.State = EntityState.Detached;
                                break;
                            }
                    }
                }
            }

            return true;
        }

        #endregion
        #region Approve
        public void Approve()
        {
            foreach (purchase_invoice invoice in db.purchase_invoice.Local.Where(x => x.IsSelected == true))
            {
                if (invoice.Error == null)
                {
                    if (invoice.id_purchase_invoice == 0)
                    {
                        SaveChanges_and_Validate();
                    }

                    invoice.app_condition = db.app_condition.Find(invoice.id_condition);
                    invoice.app_contract = db.app_contract.Find(invoice.id_contract);
                    invoice.app_currencyfx = db.app_currencyfx.Find(invoice.id_currencyfx);

                    if (invoice.status == Status.Documents_General.Pending)
                    {
                        List<payment_schedual> payment_schedualList = new List<payment_schedual>();
                        Brillo.Logic.Payment _Payment = new Brillo.Logic.Payment();

                        ///Insert Payment Schedual Logic
                        payment_schedualList = _Payment.insert_Schedual(invoice);

                        //Insert into Stock.
                        Insert_Items_2_Movement(invoice);

                        if (payment_schedualList != null && payment_schedualList.Count > 0)
                        {
                            db.payment_schedual.AddRange(payment_schedualList);
                        }

                        invoice.status = Status.Documents_General.Approved;
                        SaveChanges_and_Validate();
                    }
                }
                else if (invoice.Error != null)
                {
                    invoice.HasErrors = true;
                }

                invoice.IsSelected = false;
            }
        }
        /// <summary>
        /// Executes code that will insert Invoiced Items into Movement.
        /// </summary>
        /// <param name="Invoice"></param>
        public void Insert_Items_2_Movement(purchase_invoice invoice)
        {
            if (invoice.purchase_invoice_detail.Where(x => x.item != null).Any())
            {
                if (invoice.status == Status.Documents_General.Annulled)
                {
                    //Logica
                    ReApprove(invoice);
                }
                else // Pending
                {
                    List<item_movement> item_movementList = new List<item_movement>();

                    Brillo.Logic.Stock _Stock = new Brillo.Logic.Stock();
                    item_movementList = _Stock.PurchaseInvoice_Approve(db, invoice);

                    if (item_movementList != null && item_movementList.Count > 0)
                    {
                        db.item_movement.AddRange(item_movementList);
                    }
                }
            }
        }
        private void ReApprove(purchase_invoice invoice)
        {
            foreach (purchase_invoice_detail purchase_invoice_detail in invoice.purchase_invoice_detail.Where(x => x.item.item_product.Count() > 0))
            {
                if (purchase_invoice_detail.item_movement.Count > 0)
                {
                    item_movement item_movement = purchase_invoice_detail.item_movement.FirstOrDefault();
                    if (item_movement != null)
                    {
                        item_movement.trans_date = invoice.trans_date;
                        item_movement.timestamp = DateTime.Now;

                        if (item_movement.credit != purchase_invoice_detail.quantity)
                        {
                            item_movement.credit = purchase_invoice_detail.quantity;
                        }

                        item_movement_value item_movement_value = item_movement.item_movement_value.FirstOrDefault();
                        decimal UnitValue = Brillo.Currency.convert_Values(purchase_invoice_detail.unit_cost, invoice.id_currencyfx, CurrentSession.Get_Currency_Default_Rate().id_currencyfx, App.Modules.Purchase);
                        if (item_movement_value != null)
                        {
                            if (item_movement_value.unit_value != UnitValue)
                            {
                                item_movement_value.unit_value = UnitValue;
                            }
                        }
                    }
                }
                else
                {
                    //New
                    Brillo.Logic.Stock _Stock = new Brillo.Logic.Stock();
                    db.item_movement.Add(_Stock.CreditOnly_Movement(Status.Stock.InStock, App.Names.PurchaseInvoice, invoice.id_purchase_invoice, purchase_invoice_detail.id_purchase_invoice_detail,
                        invoice.id_currencyfx, purchase_invoice_detail.item.item_product.FirstOrDefault().id_item_product,
                        (int)purchase_invoice_detail.id_location, purchase_invoice_detail.quantity,
                        invoice.trans_date, purchase_invoice_detail.unit_cost, "Purchase Invoice Fix", null, purchase_invoice_detail.expire_date, purchase_invoice_detail.batch_code));
                }
            }
            SaveChanges_and_Validate();
        }
        #endregion

        #region Annul

        public void Annull()
        {
            ///Only run through Invoices that have been approved.
            foreach (purchase_invoice Invoice in db.purchase_invoice.Local
                .Where(x => x.IsSelected && x.status == Status.Documents_General.Approved))
            {
                //Block any annull if user is not Master.
                if (Invoice.is_accounted == false || CurrentSession.UserRole.is_master)
                {
                    //Loop through the Payment Schedual. And remove payments made.
                    foreach (payment_schedual payment_schedual in Invoice.payment_schedual)
                    {
                        if (payment_schedual.Action == payment_schedual.Actions.Delete)
                        {
                            Brillo.Logic.Payment _Payment = new Brillo.Logic.Payment();
                            _Payment.DeletePaymentSchedual(db, payment_schedual.id_payment_schedual);
                        }
                    }

                    ///Since the above Foreach will run through a mix of payment scheduals, we have no way of knowing if we will have
                    ///payment headers. So we run this code to clean.
                    List<payment> EmptyPayments = db.payments.Where(x => x.payment_detail.Count() == 0).ToList();
                    if (EmptyPayments.Count() > 0)
                    {
                        db.payments.RemoveRange(EmptyPayments);
                    }

                    foreach (purchase_invoice_detail detail in Invoice.purchase_invoice_detail)
                    {
                        List<item_movement> ItemMovementList = detail.item_movement.ToList();
                        foreach (item_movement item_movement in ItemMovementList)
                        {
                            if (item_movement.Action == item_movement.Actions.Delete)
                            {
                                Brillo.Logic.Stock _Stock = new Brillo.Logic.Stock();
                                db.item_movement.Remove(item_movement);
                            }
                            else if (item_movement.Action == item_movement.Actions.ReApprove)
                            {
                                foreach (var item in item_movement.child)
                                {
                                    List<item_movement> item_movementList = db.item_movement.Where(x =>
                                    x.id_item_product == item_movement.id_item_product &&
                                    x.id_movement != item_movement.id_movement &&
                                    x.debit > 0).ToList();
                                    foreach (item_movement _item_movement in item_movementList)
                                    {
                                        if (_item_movement.avlquantity > item.credit)
                                        {
                                            item.parent = _item_movement;
                                        }
                                        else
                                        {
                                            item.parent = null;
                                        }
                                    }
                                }
                                db.item_movement.Remove(item_movement);
                            }
                        }
                    }

                    //Change Status to Annulled.
                    Invoice.status = Status.Documents_General.Annulled;
                }

                Invoice.IsSelected = false;
                Invoice.RaisePropertyChanged("status");
            }

            db.SaveChanges();
        }

        #endregion
    }
}