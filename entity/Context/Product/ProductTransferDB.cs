﻿using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace entity
{
    public partial class ProductTransferDB : BaseDB
    {
        public override int SaveChanges()
        {
            validate_ProductTransfer();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            validate_ProductTransfer();
            return base.SaveChangesAsync();
        }

        private void validate_ProductTransfer()
        {
            foreach (item_transfer item_transfer in base.item_transfer.Local)
            {
                if (item_transfer.IsSelected)
                // && item_transfer.Error == null)
                {
                    if (item_transfer.State == EntityState.Added)
                    {
                        item_transfer.timestamp = DateTime.Now;
                        item_transfer.State = EntityState.Unchanged;
                        Entry(item_transfer).State = EntityState.Added;
                    }
                    else if (item_transfer.State == EntityState.Modified)
                    {
                        item_transfer.timestamp = DateTime.Now;
                        item_transfer.State = EntityState.Unchanged;
                        Entry(item_transfer).State = EntityState.Modified;
                    }
                    else if (item_transfer.State == EntityState.Deleted)
                    {
                        item_transfer.timestamp = DateTime.Now;
                        item_transfer.State = EntityState.Unchanged;
                        base.item_transfer.Remove(item_transfer);
                    }
                }
                else if (item_transfer.State > 0)
                {
                    if (item_transfer.State != EntityState.Unchanged)
                    {
                        Entry(item_transfer).State = EntityState.Unchanged;
                    }
                }
            }
        }

        /// <summary>
        /// Should only
        /// </summary>
        public int ApproveOrigin(int ID_BranchOrigin, int ID_BranchDestination, bool movebytruck)
        {
            NumberOfRecords = 0;

            foreach (item_transfer item_transfer in base.item_transfer.Local.Where(x => x.IsSelected))
            {
                foreach (item_transfer_detail item_transfer_detail in item_transfer.item_transfer_detail.Where(x => x.status != Status.Documents_General.Approved))
                {
                    Discount_Items_Origin(item_transfer_detail, ID_BranchOrigin, ID_BranchDestination, movebytruck);

                    //transit
                    NumberOfRecords += 1;
                    item_transfer.status = Status.Transfer.Transit;
                    item_transfer.RaisePropertyChanged("status");
                }

                if (item_transfer != null)
                {
                    ///entity.Brillo.Document.Start.Manual(item_transfer, item_transfer.app_document_range);
                }
            }

            try
            {
                base.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return NumberOfRecords;
        }

        public int ApproveDestination(int ID_BranchOrigin, int ID_BranchDestination, bool MoveByTruck)
        {
            NumberOfRecords = 0;

            foreach (item_transfer item_transfer in base.item_transfer.Local.Where(x => x.IsSelected))
            {
                foreach (item_transfer_detail item_transfer_detail in item_transfer.item_transfer_detail.Where(x => x.IsSelected && x.status != Status.Documents_General.Approved))
                {
                    if (item_transfer_detail.item_product != null)
                    {
                        Credit_Items_Destination(item_transfer_detail, ID_BranchOrigin, ID_BranchDestination, MoveByTruck);

                        NumberOfRecords += 1;

                        item_transfer_detail.status = Status.Documents_General.Approved;
                        item_transfer_detail.RaisePropertyChanged("status");
                        item_transfer.status = Status.Transfer.Approved;
                        item_transfer.RaisePropertyChanged("status");
                    }
                }

                ///Print Document only if 
                if ((item_transfer.number == null || item_transfer.number == string.Empty) && item_transfer.id_range > 0)
                {

                    if (base.app_branch.Where(x => x.id_branch == item_transfer.id_branch).FirstOrDefault() != null)
                    {
                        Brillo.Logic.Range.branch_Code = base.app_branch.Where(x => x.id_branch == item_transfer.id_branch).FirstOrDefault().code;
                    }

                    if (base.app_terminal.Where(x => x.id_terminal == item_transfer.id_terminal).FirstOrDefault() != null)
                    {
                        Brillo.Logic.Range.terminal_Code = base.app_terminal.Where(x => x.id_terminal == item_transfer.id_terminal).FirstOrDefault().code;
                    }

                    if (base.security_user.Where(x => x.id_user == item_transfer.id_user).FirstOrDefault() != null)
                    {
                        Brillo.Logic.Range.user_Code = base.security_user.Where(x => x.id_user == item_transfer.id_user).FirstOrDefault().code;
                    }

                    if (base.projects.Where(x => x.id_project == item_transfer.id_project).FirstOrDefault() != null)
                    {
                        Brillo.Logic.Range.project_Code = base.projects.Where(x => x.id_project == item_transfer.id_project).FirstOrDefault().code;
                    }

                    app_document_range app_document_range = base.app_document_range.Where(x => x.id_range == item_transfer.id_range).FirstOrDefault();
                    item_transfer.number = Brillo.Logic.Range.calc_Range(app_document_range, true);
                    item_transfer.RaisePropertyChanged("number");
                }

            }

            base.SaveChanges();

            return NumberOfRecords;
        }

        /// <summary>
        /// Executes code that will insert Invoiced Items into Movement.
        /// </summary>
        /// <param name="invoice"></param>
        public void Credit_Items_Destination(item_transfer_detail item_transfer_detail, int ID_BranchOrigin, int ID_BranchDestination, bool MoveByTruck)
        {
            entity.Brillo.Logic.Stock stock = new Brillo.Logic.Stock();
            app_currencyfx app_currencyfx = base.app_currencyfx.Where(x => x.app_currency.is_priority && x.is_active).FirstOrDefault();
            app_location app_location_dest = base.app_location.Where(x => x.id_branch == ID_BranchDestination && x.is_default).FirstOrDefault();

            if (MoveByTruck)
            {
                List<item_movement> Items_InStockLIST = base.item_movement.Where(x => x.id_location == app_location_dest.id_location
                                                        && x.id_item_product == item_transfer_detail.id_item_product
                                                        && x.status == entity.Status.Stock.InStock
                                                        && (x.credit - (x._child.Count() > 0 ? x._child.Sum(y => y.debit) : 0)) > 0).ToList();

                List<item_movement> item_movement_LIST = new List<item_movement>();
                ///Discount From Destination. Because merchendice is returned to Origin, so it must be discounted from Destintation.
                item_movement_LIST =
                    stock.DebitOnly_MovementLIST(Items_InStockLIST, Status.Stock.InStock, App.Names.Transfer, item_transfer_detail.id_transfer, item_transfer_detail.id_transfer_detail,
                    app_currencyfx, item_transfer_detail.item_product, app_location_dest, item_transfer_detail.quantity_destination,
                    item_transfer_detail.item_transfer.trans_date, stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""));

                base.item_movement.AddRange(item_movement_LIST);

                app_location app_location_origin = base.app_location.Where(x => x.id_branch == ID_BranchOrigin && x.is_default).FirstOrDefault();

                //Credit in Origin only if it is MoveByTruck.
                item_movement item_movement_origin;
                item_movement_origin =
                    stock.CreditOnly_Movement(
                        Status.Stock.InStock,
                        App.Names.Transfer,
                        item_transfer_detail.id_transfer,
                        item_transfer_detail.id_transfer_detail,
                        app_currencyfx,
                        item_transfer_detail.item_product,
                        app_location_origin,
                        item_transfer_detail.quantity_destination,
                        item_transfer_detail.item_transfer.trans_date,
                        0,
                        stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""),
                        null
                        );


                base.item_movement.Add(item_movement_origin);
            }
            else
            {
                //Credit Destination.
                item_movement item_movement_dest;
                List<item_movement> Items_InStockLIST = base.item_movement.Where(x => x.id_item_product == item_transfer_detail.id_item_product
                                                      && x.status == entity.Status.Stock.InStock && x.id_transfer_detail == item_transfer_detail.id_transfer_detail
                                                      && x.debit > 0).ToList();

                item_movement parent_item_movement = Items_InStockLIST.FirstOrDefault()._parent;
                if (parent_item_movement!=null)
                {
                    if (parent_item_movement.id_purchase_invoice_detail > 0)
                    {
                        app_currencyfx = parent_item_movement.purchase_invoice_detail.purchase_invoice.app_currencyfx;
                    }
                    if (parent_item_movement.id_inventory_detail > 0)
                    {
                        item_inventory_detail item_inventory_detail = base.item_inventory_detail.Where(x => x.id_inventory_detail == parent_item_movement.id_inventory_detail).FirstOrDefault();
                        if (item_inventory_detail != null)
                        {
                            app_currencyfx currencyfx = base.app_currencyfx.Where(x => x.id_currencyfx == item_inventory_detail.id_currencyfx).FirstOrDefault();
                            if (currencyfx != null)
                            {
                                app_currencyfx = currencyfx;
                            }
                        }
                    }
                }
               

                item_movement_dest =
                            stock.CreditOnly_Movement(
                                Status.Stock.InStock,
                                App.Names.Transfer,
                                item_transfer_detail.id_transfer,
                                item_transfer_detail.id_transfer_detail,
                                app_currencyfx,
                                item_transfer_detail.item_product,
                                app_location_dest,
                                item_transfer_detail.quantity_destination,
                                item_transfer_detail.item_transfer.trans_date,
                                Items_InStockLIST.Sum(x => x.item_movement_value.Sum(y => y.unit_value)),
                                stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""),
                                null
                                );
                item_movement_dest._parent = Items_InStockLIST.FirstOrDefault();
                base.item_movement.Add(item_movement_dest);
            }
        }

        /// <summary>
        /// Executes code that will insert Invoiced Items into Movement.
        /// </summary>
        /// <param name="invoice"></param>
        public void Discount_Items_Origin(item_transfer_detail item_transfer_detail, int ID_BranchOrigin, int ID_BranchDestination, bool movebytruck)
        {
            entity.Brillo.Logic.Stock stock = new Brillo.Logic.Stock();

            if (item_transfer_detail.item_product != null)
            {
                if (movebytruck)
                {
                    app_currencyfx app_currencyfx = base.app_currencyfx.Where(x => x.app_currency.is_active).FirstOrDefault();
                    app_location app_location = base.app_location.Where(x => x.id_branch == ID_BranchOrigin && x.is_default).FirstOrDefault();
                    List<item_movement> Items_InStockLIST = base.item_movement.Where(x => x.id_location == app_location.id_location
                                                            && x.id_item_product == item_transfer_detail.id_item_product
                                                            && x.status == entity.Status.Stock.InStock
                                                            && (x.credit - (x._child.Count() > 0 ? x._child.Sum(y => y.debit) : 0)) > 0).ToList();

                    List<item_movement> item_movement_originList;
                    item_movement_originList = stock.DebitOnly_MovementLIST(Items_InStockLIST, Status.Stock.InStock, App.Names.Transfer, item_transfer_detail.id_transfer, item_transfer_detail.id_transfer_detail, app_currencyfx, item_transfer_detail.item_product, app_location,
                            item_transfer_detail.quantity_origin, item_transfer_detail.item_transfer.trans_date, stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""));

                    base.item_movement.AddRange(item_movement_originList);

                    item_movement item_movement_Dest;
                    app_currencyfx app_currencyfxdest = base.app_currencyfx.Where(x => x.app_currency.is_active).FirstOrDefault();
                    app_location app_locationdest = base.app_location.Where(x => x.id_branch == ID_BranchDestination && x.is_default).FirstOrDefault();

                    item_movement_Dest = stock.CreditOnly_Movement(
                        Status.Stock.InStock,
                        App.Names.Transfer,
                        item_transfer_detail.id_transfer,
                        item_transfer_detail.id_transfer_detail,
                        app_currencyfxdest,
                        item_transfer_detail.item_product,
                        app_locationdest,
                            item_transfer_detail.quantity_origin,
                            item_transfer_detail.item_transfer.trans_date,
                            0,
                            stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""),
                            null);

                    base.item_movement.Add(item_movement_Dest);
                }
                else
                {
                    app_currencyfx app_currencyfx = base.app_currencyfx.Where(x => x.app_currency.is_active).FirstOrDefault();
                    app_location app_location = base.app_location.Where(x => x.id_branch == ID_BranchOrigin && x.is_default).FirstOrDefault();

                    List<item_movement> Items_InStockLIST = base.item_movement.Where(x => x.id_location == app_location.id_location
                                                            && x.id_item_product == item_transfer_detail.id_item_product
                                                            && x.status == entity.Status.Stock.InStock
                                                            && (x.credit - (x._child.Count() > 0 ? x._child.Sum(y => y.debit) : 0)) > 0).ToList();
                    ///Debit Movement from Origin.
                    List<item_movement> item_movement_originList;
                    item_movement_originList = stock.DebitOnly_MovementLIST(Items_InStockLIST, Status.Stock.InStock, App.Names.Transfer, item_transfer_detail.id_transfer, item_transfer_detail.id_transfer_detail, app_currencyfx, item_transfer_detail.item_product, app_location,
                            item_transfer_detail.quantity_origin, item_transfer_detail.item_transfer.trans_date, stock.comment_Generator(App.Names.Transfer, item_transfer_detail.item_transfer.number != null ? item_transfer_detail.item_transfer.number.ToString() : "", ""));

                    base.item_movement.AddRange(item_movement_originList);
                }
            }
        }
    }
}
