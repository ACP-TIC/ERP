﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using entity;
using System;
using System.Windows.Input;

namespace Cognitivo.Production
{
    public partial class Execution : Page
    {
        ExecutionDB ExecutionDB = new ExecutionDB();

        //Production EXECUTION CollectionViewSource
        CollectionViewSource
            projectViewSource,
              project_task_dimensionViewSource,
          
            production_execution_detailViewSource
          ;

        //Production ORDER CollectionViewSource
        CollectionViewSource
            production_orderViewSource,
            production_order_detaillViewSource,
       
            item_dimensionViewSource;

        public Execution()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            item_dimensionViewSource = FindResource("item_dimensionViewSource") as CollectionViewSource;
            item_dimensionViewSource.Source = ExecutionDB.item_dimension.Where(x => x.id_company == CurrentSession.Id_Company).ToList();

       
            production_execution_detailViewSource = FindResource("production_execution_detailViewSource") as CollectionViewSource;
            production_order_detaillViewSource = FindResource("production_order_detailViewSource") as CollectionViewSource;
       

            production_orderViewSource = FindResource("production_orderViewSource") as CollectionViewSource;
            ExecutionDB.production_order.Where(x => x.id_company == CurrentSession.Id_Company && x.type != production_order.ProductionOrderTypes.Fraction).Load();
            production_orderViewSource.Source = ExecutionDB.production_order.Local;

            projectViewSource = FindResource("projectViewSource") as CollectionViewSource;
            ExecutionDB.projects.Where(a => a.id_company == CurrentSession.Id_Company).Load();
            projectViewSource.Source = ExecutionDB.projects.Local;

            CollectionViewSource production_lineViewSource = FindResource("production_lineViewSource") as CollectionViewSource;
            ExecutionDB.production_line.Where(x => x.id_company == CurrentSession.Id_Company).Load();
            production_lineViewSource.Source = ExecutionDB.production_line.Local;

            CollectionViewSource hr_time_coefficientViewSource = FindResource("hr_time_coefficientViewSource") as CollectionViewSource;
            ExecutionDB.hr_time_coefficient.Where(x => x.id_company == CurrentSession.Id_Company).Load();
            hr_time_coefficientViewSource.Source = ExecutionDB.hr_time_coefficient.Local;

            CollectionViewSource app_dimensionViewSource = ((CollectionViewSource)(FindResource("app_dimensionViewSource")));
            app_dimensionViewSource.Source = ExecutionDB.app_dimension.Where(a => a.id_company == CurrentSession.Id_Company).ToList();

            CollectionViewSource app_measurementViewSource = ((CollectionViewSource)(FindResource("app_measurementViewSource")));
            app_measurementViewSource.Source = ExecutionDB.app_measurement.Where(a => a.id_company == CurrentSession.Id_Company).ToList();

            cmbcoefficient.SelectedIndex = -1;

            filter_order(production_order_detaillViewSource, item.item_type.Product);

            dtpenddate.Text = DateTime.Now.ToString();
            dtpstartdate.Text = DateTime.Now.ToString();
        }

        public void filter_order(CollectionViewSource CollectionViewSource, item.item_type item_type)
        {
            int id_production_order = 0;
            if (production_orderViewSource.View.CurrentItem != null)
            {
                id_production_order = ((production_order)production_orderViewSource.View.CurrentItem).id_production_order;
            }

            if (CollectionViewSource != null)
            {

                List<production_order_detail> _production_order_detail =
                    ExecutionDB.production_order_detail.Where(a =>
                           a.status == Status.Production.Approved
                 
                        && a.id_production_order == id_production_order)
                         .ToList();

                if (_production_order_detail.Count() > 0)
                {
                    CollectionViewSource.Source = _production_order_detail;
                }
                else
                {
                    CollectionViewSource.Source = null;
                }
            }

            if (CollectionViewSource != null)
            {
                if (CollectionViewSource.View != null)
                {
                    CollectionViewSource.View.Filter = i =>
                    {

                        production_order_detail production_order_detail = (production_order_detail)i;
                        if (production_order_detail.parent == null)
                        {

                            return true;

                        }
                        else { return false; }

                    };
                }
            }

        }

  


        private void toolBar_btnSave_Click(object sender)
        {
            ExecutionDB.SaveChanges();
        }

        private void itemserviceComboBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (CmbService.ContactID > 0)
            {

                contact contact = ExecutionDB.contacts.Where(x => x.id_contact == CmbService.ContactID).FirstOrDefault();
                adddatacontact(contact, treeSupply);
                production_order_detail production_order_detail = (production_order_detail)treeSupply.SelectedItem;
                if (production_order_detail != null)
                {
                    production_execution_detailViewSource.Source =  ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToList();
                    //production_order_detaillProductViewSource.View.MoveCurrentTo(production_order_detail);
                }
            }
        }

        public void adddatacontact(contact contact, cntrl.ExtendedTreeView treeview)
        {
            production_order_detail production_order_detail = (production_order_detail)treeview.SelectedItem_;
            if (production_order_detail != null)
            {
                if (contact != null)
                {
                    //Product
                    int id = Convert.ToInt32(((contact)contact).id_contact);
                    if (id > 0)
                    {
                      //  production_execution _production_execution = (production_execution)production_executionViewSource.View.CurrentItem;
                        production_execution_detail _production_execution_detail = new entity.production_execution_detail();

                        //Check for contact
                        _production_execution_detail.id_contact = ((contact)contact).id_contact;
                        _production_execution_detail.contact = contact;
                        _production_execution_detail.quantity = 1;
                        _production_execution_detail.item = production_order_detail.item;
                        _production_execution_detail.id_item = production_order_detail.item.id_item;
                    //    _production_execution.RaisePropertyChanged("quantity");
                        _production_execution_detail.is_input = true;
                        _production_execution_detail.name = contact.name + ": " + production_order_detail.name;

                        if (production_order_detail.id_project_task > 0)
                        {
                            _production_execution_detail.id_project_task = production_order_detail.id_project_task;
                        }

                        //Gets the Employee's contracts Hourly Rate.
                        hr_contract contract = ExecutionDB.hr_contract.Where(x => x.id_contact == id && x.is_active).FirstOrDefault();
                        if (contract != null)
                        {
                            _production_execution_detail.unit_cost = contract.Hourly;
                        }

                        if (production_order_detail.item.id_item_type == item.item_type.Service)
                        {
                            if (cmbcoefficient.SelectedValue != null)
                            {
                                _production_execution_detail.id_time_coefficient = (int)cmbcoefficient.SelectedValue;
                            }

                            string start_date = string.Format("{0} {1}", dtpstartdate.Text, dtpstarttime.Text);
                            _production_execution_detail.start_date = Convert.ToDateTime(start_date);
                            string end_date = string.Format("{0} {1}", dtpenddate.Text, dtpendtime.Text);
                            _production_execution_detail.end_date = Convert.ToDateTime(end_date);

                            //_production_execution_detail.id_production_execution = _production_execution.id_production_execution;
                            //_production_execution_detail.production_execution = _production_execution;
                            _production_execution_detail.id_project_task = production_order_detail.id_project_task;
                            _production_execution_detail.id_order_detail = production_order_detail.id_order_detail;
                            _production_execution_detail.production_order_detail = production_order_detail;

                            ExecutionDB.production_execution_detail.Add(_production_execution_detail);
                            RefreshData();
                            
                        }
                        else if (production_order_detail.item.id_item_type == item.item_type.ServiceContract)
                        {
                            if (cmbcoefficient.SelectedValue != null)
                            {
                                _production_execution_detail.id_time_coefficient = (int)cmbsccoefficient.SelectedValue;
                            }

                            string start_date = string.Format("{0} {1}", dtpscstartdate.Text, dtpscstarttime.Text);
                            _production_execution_detail.start_date = Convert.ToDateTime(start_date);
                            string end_date = string.Format("{0} {1}", dtpscenddate.Text, dtpscendtime.Text);
                            _production_execution_detail.end_date = Convert.ToDateTime(end_date);

                            //_production_execution_detail.id_production_execution = _production_execution.id_production_execution;
                            //_production_execution_detail.production_execution = _production_execution;
                            _production_execution_detail.id_project_task = production_order_detail.id_project_task;
                            _production_execution_detail.id_order_detail = production_order_detail.id_order_detail;
                            _production_execution_detail.production_order_detail = production_order_detail;

                            production_order_detail.production_execution_detail.Add(_production_execution_detail);

                            RefreshData();
                        }
                    }
                }
            }
            else
            {
                toolBar.msgWarning("select Production order for insert");
            }
        }


        private void toolBar_btnNew_Click(object sender)
        {
           // production_execution production_execution =OrderDB.NewExecustion();
     
           

           // production_executionViewSource.View.MoveCurrentToLast();
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (projectDataGrid.SelectedItem != null)
            {
                production_order production_order = (production_order)projectDataGrid.SelectedItem;
                production_order.IsSelected = true;
                production_order.State = EntityState.Modified;
                ExecutionDB.Entry(production_order).State = EntityState.Modified;
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            production_order production_order = (production_order)projectDataGrid.SelectedItem;
            production_order.State = EntityState.Unchanged;
           
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            //if (MessageBox.Show("Are you sure want to Delete?", "Cognitivo", MessageBoxButton.YesNo, MessageBoxImage.Question)
            //                == MessageBoxResult.Yes)
            //{
            //    OrderDB.production_execution.Remove((production_execution)production_executionViewSource.View.CurrentItem);
            //    production_executionViewSource.View.MoveCurrentToFirst();
            //}

        }

        private void toolBar_btnApprove_Click(object sender)
        {
            toolBar_btnSave_Click(sender);

            if (ExecutionDB.Approve(entity.production_order.ProductionOrderTypes.Production) > 0)
            {
                toolBar.msgApproved(1);
            }
        }

        private void projectDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshData();
        }

   
     
      
      

        private async void treeProduct_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeSupply.SelectedItem_;
            if (production_order_detail != null)
            {
                if (production_order_detail.item.id_item_type == item.item_type.Product)
                {
                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                }
                else if (production_order_detail.item.id_item_type == item.item_type.RawMaterial)
                {
                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                    if (production_order_detail.project_task != null)
                    {
                        int _id_task = production_order_detail.project_task.id_project_task;
                        project_task_dimensionViewSource = (CollectionViewSource)FindResource("project_task_dimensionViewSource");
                        project_task_dimensionViewSource.Source = ExecutionDB.project_task_dimension.Where(x => x.id_project_task == _id_task).ToList();
                    }
                }
                else if (production_order_detail.item.id_item_type == item.item_type.FixedAssets)
                {
                    pnlFixedAsset.Visibility = Visibility.Visible;

                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                }
                else if (production_order_detail.item.id_item_type == item.item_type.Supplies)
                {
                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                }
                else if (production_order_detail.item.id_item_type == item.item_type.Service)
                {
                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                }
                else if (production_order_detail.item.id_item_type == item.item_type.ServiceContract)
                {
                    production_execution_detailViewSource.Source = await ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToListAsync();
                }
            
                //production_order_detaillProductViewSource.View.MoveCurrentTo(production_order_detail);
            }

        }


        private void dgSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailViewSource != null)
            {
                if (production_execution_detailViewSource.View != null)
                {
                    production_execution_detail obj = production_execution_detailViewSource.View.CurrentItem as production_execution_detail;

                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

        private void dgproduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailViewSource != null)
            {
                if (production_execution_detailViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }

        }

        private void dgRaw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailViewSource != null)
            {
                if (production_execution_detailViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

        private void dgCapital_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailViewSource != null)
            {
                if (production_execution_detailViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

     

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            try
            {
                if (!string.IsNullOrEmpty(query))
                {
                    production_orderViewSource.View.Filter = i =>
                    {
                        production_order production_order = i as production_order;
                        if (production_order.name.ToLower().Contains(query.ToLower()))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    };
                }
                else
                {
                    production_orderViewSource.View.Filter = null;
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }

        }

        private void btnInsert_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tbx = sender as TextBox;
                Button btn = new Button();
                btn.Name = tbx.Name;
                btnInsert_Click(btn, e);

                //This is to clean contents after enter.
                tbx.Text = string.Empty;
            }
        }

        private void txtsupplier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button btn = new Button();
                btn.Name = "Supp";
                btnInsert_Click(btn, e);
            }
        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as production_execution_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DataGrid exexustiondetail = (DataGrid)e.Source;
                MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    //production_execution production_execution = production_executionViewSource.View.CurrentItem as production_execution;
                    //DeleteDetailGridRow
                    exexustiondetail.CancelEdit();
                    production_execution_detail production_execution_detail = e.Parameter as production_execution_detail;
                    production_execution_detail.State = EntityState.Deleted;

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        public void RefreshData()
        {
            try
            {
                production_execution_detailViewSource.View.Refresh();
                production_execution_detailViewSource.View.MoveCurrentToLast();

               

               

                production_order production_order = production_orderViewSource.View.CurrentItem as production_order;
                foreach (production_order_detail production_order_detail in production_order.production_order_detail)
                {
                    production_order_detail.CalcExecutedQty_TimerTaks();
                    production_order_detail.CalcExecutedCost_TimerTaks();
                   
                }
            }
            catch { }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            production_order_detail production_order_detail = null;
            Button btn = sender as Button;
            decimal Quantity = 0M;
            CollectionViewSource Collection = null; 
            item.item_type type = item.item_type.Task;
            production_order_detail = treeSupply.SelectedItem_ as production_order_detail;
            Collection = production_execution_detailViewSource;
            if (btn.Name.Contains("Prod"))
            {
                Quantity = Convert.ToDecimal(txtProduct.Text);
               
                type = item.item_type.Product;
              
            }
            else if (btn.Name.Contains("Raw"))
            {
                Quantity = Convert.ToDecimal(txtraw.Text);
              
                type = item.item_type.RawMaterial;
               
            }
            else if (btn.Name.Contains("Asset"))
            {
                Quantity = Convert.ToDecimal(txtAsset.Text);
               
                type = item.item_type.FixedAssets;
               
            }
            else if (btn.Name.Contains("Supp"))
            {
                Quantity = Convert.ToDecimal(txtSupply.Text);
                
                type = item.item_type.Supplies;
              
            }
            else if (btn.Name.Contains("ServiceContract"))
            {
                Quantity = Convert.ToDecimal(txtServicecontract.Text);
               
                type = item.item_type.ServiceContract;
               
            }

            try
            {
                if (production_order_detail.is_input)
                {
                    if (production_order_detail != null && Quantity > 0 && (
                        type == item.item_type.Product ||
                        type == item.item_type.RawMaterial ||
                        type == item.item_type.Supplies)
                        )
                    {
                        if (production_order_detail.item.item_dimension.Count() > 0)
                        {
                            Cognitivo.Configs.itemMovementFraction DimensionPanel = new Cognitivo.Configs.itemMovementFraction();
                            DimensionPanel.mode = Configs.itemMovementFraction.modes.Execution;

                           // production_execution _production_execution = production_executionViewSource.View.CurrentItem as production_execution;

                            DimensionPanel.id_item = (int)production_order_detail.id_item;
                            DimensionPanel.ExecutionDB = ExecutionDB;
                            DimensionPanel.production_order_detail = production_order_detail;
                            //DimensionPanel._production_execution = _production_execution;
                            DimensionPanel.Quantity = Quantity;

                            crud_modal.Visibility = Visibility.Visible;
                            crud_modal.Children.Add(DimensionPanel);
                        }
                        else
                        {
                            Insert_IntoDetail(production_order_detail, Quantity);
                            RefreshData();
                        }
                    }
                    else
                    {
                        Insert_IntoDetail(production_order_detail, Quantity);
                        RefreshData();
                    }
                }
                else
                {
                    Insert_IntoDetail(production_order_detail, Quantity);
                    RefreshData();
                }

                Collection.Source = ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToList();
                //production_order_detaillProductViewSource.View.MoveCurrentTo(production_order_detail);
               
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void Insert_IntoDetail(production_order_detail production_order_detail, decimal Quantity)
        {
           // production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
            production_execution_detail _production_execution_detail = new entity.production_execution_detail();

            //Adds Parent so that during approval, because it is needed for approval.
            if (production_order_detail.parent != null)
            {
                if (production_order_detail.parent.production_execution_detail != null)
                {
                    _production_execution_detail.parent = production_order_detail.parent.production_execution_detail.FirstOrDefault();
                }
            }

            _production_execution_detail.State = EntityState.Added;
            _production_execution_detail.id_item = production_order_detail.id_item;
            _production_execution_detail.item = production_order_detail.item;
            _production_execution_detail.quantity = Quantity;
            _production_execution_detail.id_project_task = production_order_detail.id_project_task;
            
            if (production_order_detail.item.unit_cost != null)
            {
                _production_execution_detail.unit_cost = (decimal)production_order_detail.item.unit_cost;
            }

          //  _production_execution_detail.production_execution = _production_execution;
            _production_execution_detail.id_order_detail = production_order_detail.id_order_detail;

            if (production_order_detail.item.is_autorecepie)
            {
                _production_execution_detail.is_input = false;
            }
            else
            {
                _production_execution_detail.is_input = true;
            }
               production_order_detail.production_execution_detail.Add(_production_execution_detail);

            ExecutionDB.SaveChanges();
        }


        private void dgServicecontract_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailViewSource != null)
            {
                if (production_execution_detailViewSource.View != null)
                {
                    production_execution_detail obj = production_execution_detailViewSource.View.CurrentItem as production_execution_detail;

                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }
                    }
                }
            }
        }

        private void CmbServicecontract_Select(object sender, RoutedEventArgs e)
        {
            if (CmbServicecontract.ContactID > 0)
            {
                contact contact = ExecutionDB.contacts.Where(x => x.id_contact == CmbServicecontract.ContactID).FirstOrDefault();
                adddatacontact(contact, treeSupply);
                production_order_detail production_order_detail = (production_order_detail)treeSupply.SelectedItem;
                if (production_order_detail != null)
                {
                    production_execution_detailViewSource.Source = ExecutionDB.production_execution_detail.Where(x => x.id_order_detail == production_order_detail.id_order_detail).ToList();
                    //production_order_detaillProductViewSource.View.MoveCurrentTo(production_order_detail);
                }
            }
        }
        private void crud_modal_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            crud_modal.Children.Clear();
            RefreshData();
        }
    }
}
