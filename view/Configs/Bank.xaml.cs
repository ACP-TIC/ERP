﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Globalization;
using entity;

namespace Cognitivo.Configs
{
    /// <summary>
    /// Interaction logic for Bank.xaml
    /// </summary>
    public partial class Bank : Page
    {
        entity.dbContext entity = new entity.dbContext();
        CollectionViewSource bankViewSource;
        entity.Properties.Settings _entity = new entity.Properties.Settings();

        public Bank()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bankViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("app_bankViewSource")));
            entity.db.app_bank.Where(a=>a.id_company == _entity.company_ID).OrderByDescending(a => a.is_active).Load();
            bankViewSource.Source = entity.db.app_bank.Local;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = System.Windows.Visibility.Visible;
            cntrl.bank objBank = new cntrl.bank();
            app_bank app_bank = new app_bank();
            entity.db.app_bank.Add(app_bank);
            bankViewSource.View.MoveCurrentToLast();
            objBank.app_bankViewSource = bankViewSource;
            objBank._entity = entity;
            crud_modal.Children.Add(objBank);
        }

        private void pnl_Bank_Click(object sender, int idBank)
        {
            crud_modal.Visibility = System.Windows.Visibility.Visible;
            cntrl.bank objBank = new cntrl.bank();
            bankViewSource.View.MoveCurrentTo(entity.db.app_bank.Where(x => x.id_bank == idBank).FirstOrDefault());
            objBank.app_bankViewSource = bankViewSource;
            objBank._entity = entity;
            crud_modal.Children.Add(objBank);
        }        
    }
}
