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

namespace Cognitivo.Commercial
{
    public partial class PromissoryNote : Page
    {
        entity.db db = new entity.db();

        public PromissoryNote()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void toolBar_btnApprove_Click(object sender)
        {

        }

        private void toolBar_btnAnull_Click(object sender)
        {

        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {

        }
    }
}
