﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace entity
{
    public partial class ProductMovementDB : BaseDB
    {
        public override int SaveChanges()
        {
            //validate_ProductMovement();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            //validate_ProductMovement();
            return base.SaveChangesAsync();
        }

        private void validate_ProductMovement()
        {
            foreach (item_movement item_movement in base.item_movement.Local)
            {
                if (item_movement.IsSelected)
                {
                    if (item_movement.State == EntityState.Added)
                    {
                        item_movement.timestamp = DateTime.Now;
                        item_movement.State = EntityState.Unchanged;
                        Entry(item_movement).State = EntityState.Added;
                    }
                    else if (item_movement.State == EntityState.Modified)
                    {
                        item_movement.timestamp = DateTime.Now;
                        item_movement.State = EntityState.Unchanged;
                        Entry(item_movement).State = EntityState.Modified;
                    }
                    else if (item_movement.State == EntityState.Deleted)
                    {
                        item_movement.timestamp = DateTime.Now;
                        item_movement.State = EntityState.Unchanged;
                        base.item_movement.Remove(item_movement);
                    }
                }
                else if (item_movement.State > 0)
                {
                    if (item_movement.State != EntityState.Unchanged)
                    {
                        Entry(item_movement).State = EntityState.Unchanged;
                    }
                }
            }
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public string Generate_ProductMovement()
        {
            string ErrorMsg = "";

            ///Delete all Movements.
            if (base.item_movement.Where(x => x.id_company == CurrentSession.Id_Company).Count() > 0)
            {
                base.item_movement.RemoveRange(base.item_movement.Where(x => x.id_company == CurrentSession.Id_Company).ToList());
                base.SaveChanges();
            }

            ///Purchase

            using (PurchaseInvoiceDB PurchaseDB = new PurchaseInvoiceDB())
            {
                List<purchase_invoice> purchaseLIST = PurchaseDB.purchase_invoice
              .Where(x =>
                  x.id_company == CurrentSession.Id_Company &&
                  x.status == Status.Documents_General.Approved
                  )
                  .OrderBy(y => y.trans_date)
                  .ToList();

                foreach (purchase_invoice purchase in purchaseLIST)
                {
                    try
                    {
                        purchase.IsSelected = true;
                        PurchaseDB.Insert_Items_2_Movement(purchase);
                        PurchaseDB.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        ErrorMsg += string.Format("/n Purchase: {0}, {1}, Error Msg: {2}", purchase.number, purchase.trans_date, e.Message);
                    }
                }
            }

            using (ImpexDB ImpexDB = new ImpexDB())
            {
                List<impex> impexLIST = ImpexDB.impex.Where(x => x.status == Status.Documents_General.Approved).ToList();

                foreach (impex impex in impexLIST)
                {
                    try
                    {
                        impex.IsSelected = true;
                        impex.status = Status.Documents_General.Pending;
                        ImpexDB.ApproveImport();
                    }
                    catch (Exception e)
                    {
                        ErrorMsg += string.Format("/n Impex: {0}, {1}, Error Msg: {2}", impex.number, impex.contact.name, e.Message);
                    }
                }
            }

            DateTime StartDate = DateTime.Now.AddMonths(-12);
            DateTime EndDate = DateTime.Now;

            base.Configuration.LazyLoadingEnabled = false;
            base.Configuration.AutoDetectChangesEnabled = false;

            List<item_inventory> item_inventoryList = new List<entity.item_inventory>();
            List<item_transfer> item_transferList = new List<entity.item_transfer>();
            List<sales_invoice> sales_invoiceLIST = new List<entity.sales_invoice>();

            using (db db = new db())
            {
                item_inventoryList = db.item_inventory.Where(x => x.id_company == CurrentSession.Id_Company && x.status == Status.Documents.Issued).ToList();
                item_transferList = db.item_transfer.Where(x => x.id_company == CurrentSession.Id_Company && x.status == Status.Transfer.Approved).AsNoTracking().ToList();
                sales_invoiceLIST = base.sales_invoice.Where(x => x.id_company == CurrentSession.Id_Company && x.status == Status.Documents_General.Approved).ToList();
            }

            foreach (DateTime day in EachDay(StartDate, EndDate))
            {
                if (item_inventoryList.Any(x => x.trans_date.Date == day.Date))
                {
                    List<int> IntArray = item_inventoryList.Where(z => z.trans_date.Date == day.Date).Select(y => y.id_inventory).ToList();

                    ///Inventory
                    using (InventoryDB InventoryDB = new InventoryDB())
                    {
                        List<item_inventory> item_inventoryLIST =
                            InventoryDB.item_inventory
                                .Where(x => IntArray.Contains(x.id_inventory))
                                .OrderBy(y => y.trans_date)
                                .ToList();

                        foreach (item_inventory inventory in item_inventoryLIST)
                        {
                            if (inventory.status == Status.Documents.Issued)
                            {
                                inventory.status = Status.Documents.Pending;
                                inventory.IsSelected = true;
                            }

                            InventoryDB.Approve();

                            try
                            {
                                InventoryDB.Approve();
                            }
                            catch (Exception e)
                            {
                                ErrorMsg += string.Format("/n Inventory: {0}, {1}, Error Msg: {2}", inventory.app_branch.name, inventory.trans_date, e.Message);
                            }
                        }
                    }
                }

                if (item_transferList.Any(x => x.trans_date.Date == day.Date))
                {
                    List<int> IntArray = item_transferList.Where(z => z.trans_date.Date == day.Date).Select(y => y.id_transfer).ToList();

                    ///Transfers & Movement
                    using (ProductTransferDB ProductTransferDB = new ProductTransferDB())
                    {
                        List<item_transfer> TrasnferList = ProductTransferDB.item_transfer
                          .Where(x => IntArray.Contains(x.id_transfer))
                          .OrderBy(y => y.trans_date)
                          .ToList();

                        foreach (item_transfer transfer in TrasnferList)
                        {
                            transfer.IsSelected = true;
                            foreach (item_transfer_detail detail in transfer.item_transfer_detail.Where(x => x.status == Status.Documents_General.Approved))
                            {
                                detail.status = Status.Documents_General.Pending;
                                detail.IsSelected = true;
                            }

                            try
                            {
                                ProductTransferDB.SaveChanges();

                                ProductTransferDB.ApproveOrigin(transfer, false);
                                ProductTransferDB.ApproveDestination(transfer, false);
                            }
                            catch (Exception e)
                            {
                                ErrorMsg += string.Format("/n Transfer: {0}, {1}, Error Msg: {2}", transfer.number, transfer.trans_date, e.Message);
                            }

                            transfer.IsSelected = false;
                        }
                    }
                }

                foreach (sales_invoice sales in sales_invoiceLIST.Where(z => z.trans_date.Date == day.Date))
                {
                    ///Sales
                    using (SalesInvoiceDB SalesInvoiceDB = new SalesInvoiceDB())
                    {
                        sales_invoice sales_invoice = SalesInvoiceDB.sales_invoice.Find(sales.id_sales_invoice);
                        if (sales_invoice != null)
                        {
                            try
                            {
                                SalesInvoiceDB.Insert_Items_2_Movement(sales_invoice);
                                SalesInvoiceDB.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                ErrorMsg += string.Format("/n SalesInvoice: {0}, {1}, Error Msg: {2}", sales.number, sales.trans_date, e.Message);
                            }
                        }
                    }
                }
            }

            return ErrorMsg;
        }

        public void ChangeBarcode_ProductMovement()
        {
            long n;
            foreach (item_movement item_movement in base.item_movement)
            {
                if (item_movement.barcode != null && !long.TryParse(item_movement.barcode, out n))
                {
                    BarcodeGenerator.BarcodeGenerate BG = new BarcodeGenerator.BarcodeGenerate();
                    item_movement.barcode = BG.Decodestring(item_movement.barcode);
                }
            }
        }
    }
}