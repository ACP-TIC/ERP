﻿using entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cognitivo.Product
{
    public partial class Stock : Page, INotifyPropertyChanged
    {
        private CollectionViewSource item_movementViewSource, inventoryViewSource;
        public bool ShowZeros
        {
            get { return _ShowZeros; }
            set { _ShowZeros = value; TextBox_TextChanged(null, null); }
        }
        private bool _ShowZeros;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime InventoryDate
        {
            get { return _InventoryDate; }
            set
            {
                _InventoryDate = value;
                RaisePropertyChanged("InventoryDate");

                if (chkstock.IsChecked == true)
                {
                    slider.Maximum = DateTime.DaysInMonth(_InventoryDate.Year, _InventoryDate.Month);
                }

                slider.Value = InventoryDate.Day;

                calc_Inventory();
            }
        }

        private DateTime _InventoryDate = DateTime.Now;

        public Stock()
        {
            InitializeComponent();
        }

        private void StockPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dgvBranch.ItemsSource = CurrentSession.Branches; //StockDB.app_branch.Local;
            InventoryDate = DateTime.Now;
        }

        private void calc_Inventory()
        {
            app_branch app_branch = dgvBranch.SelectedItem as app_branch;

            if (app_branch != null)
            {
                inventoryViewSource = FindResource("inventoryViewSource") as CollectionViewSource;

                if (chkstock.IsChecked == true)
                {
                    inventoryViewSource.Source = CurrentItems.getProducts_InStock_GroupByLocationBatch(app_branch.id_branch, InventoryDate, true);
                }
                else
                {
                    inventoryViewSource.Source = CurrentItems.getProducts_InStock_GroupBy(app_branch.id_branch, InventoryDate, true);
                }

                TextBox_TextChanged(null, null);
            }
        }

        private void itemDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            calc_Inventory();
        }

        private void slider_ValueChanged(object sender, EventArgs e)
        {
            InventoryDate = InventoryDate.AddDays(slider.Value - InventoryDate.Day);
        }

        private void RRMonth_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InventoryDate = InventoryDate.AddMonths(-1);
        }

        private void RRDay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InventoryDate = InventoryDate.AddDays(-1);
        }

        private void FFDay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InventoryDate = InventoryDate.AddDays(1);
        }

        private void FFMonth_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InventoryDate = InventoryDate.AddMonths(1);
        }

        private void Today_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InventoryDate = DateTime.Now;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void item_movementDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //entity.Brillo.StockList _item_movement = item_inventoryDataGrid.SelectedItem as entity.Brillo.StockList;
            dynamic selectedITEM = (dynamic)item_inventoryDataGrid.SelectedItem;

            if (selectedITEM != null)
            {
                int id_item_product = (int)selectedITEM.ProductID;
                int id_location = (int)selectedITEM.LocationID;
                int id_movement = (int)selectedITEM.MovementID;

                using (db db = new db())
                {
                    item_movementViewSource = ((CollectionViewSource)(FindResource("item_movementViewSource")));

                    if (chkstock.IsChecked == true)
                    {
                        item_movementViewSource.Source = await db.item_movement
                      .Where(x => x.id_company == CurrentSession.Id_Company
                                                      && x.id_item_product == id_item_product
                                                      && x.app_location.id_location == id_location
                                                      && x.status == Status.Stock.InStock
                                                      && x.trans_date <= InventoryDate
                                                      && (x.parent.id_movement == id_movement || x.id_movement == id_movement)
                                                      )
                                                      .OrderByDescending(x => x.trans_date)
                                                      .Include(x => x.item_product)
                                                      .ToListAsync();
                    }
                    else
                    {
                        item_movementViewSource.Source = await db.item_movement
                    .Where(x => x.id_company == CurrentSession.Id_Company
                                                    && x.id_item_product == id_item_product
                                                    && x.app_location.id_location == id_location
                                                    && x.status == Status.Stock.InStock
                                                    && x.trans_date <= InventoryDate
                                                    )
                                                    .OrderByDescending(x => x.trans_date)
                                                    .Take(100)
                                                    .Include(x => x.item_product)
                                                    .ToListAsync();
                    }


                    foreach (item_movement item_movement in item_movementViewSource.View.Cast<item_movement>().ToList())
                    {
                        if (item_movement.item_movement_dimension.Count() > 0)
                        {
                            foreach (item_movement_dimension item_movement_dimension in item_movement.item_movement_dimension)
                            {
                                if (!(item_movement.comment.Contains(item_movement_dimension.app_dimension.name)))
                                {
                                    item_movement.comment += " " + item_movement_dimension.app_dimension.name + " : " + Math.Round(item_movement_dimension.value, 2) + ",";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            calc_Inventory();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inventoryViewSource != null)
            {
                if (inventoryViewSource.View != null)
                {
                    inventoryViewSource.View.Filter = i =>
                    {
                        entity.Brillo.StockList TmpInventory = i as entity.Brillo.StockList;

                        if (TmpInventory.Code.ToUpper().Contains(txtsearch.Text.ToUpper()) ||
                            TmpInventory.Name.ToUpper().Contains(txtsearch.Text.ToUpper()) ||
                            TmpInventory.Location.ToUpper().Contains(txtsearch.Text.ToUpper()))
                        {
                            //This code checks for Quantity after checking for name. This will cause less loops.
                            return TmpInventory.Quantity == 0 ? ShowZeros : true;
                        }

                        return false;
                    };
                }
            }
        }
    }
}