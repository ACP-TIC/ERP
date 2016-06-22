﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Cognitivo.Reporting.Views
{
    public partial class AccountsRecievableAnalisys : Page
    {
        public DateTime EndDate
        {
            get { return _EndDate; }
            set { _EndDate = value; }
        }
        private DateTime _EndDate = DateTime.Now.AddDays(+1);

        public AccountsRecievableAnalisys()
        {
            InitializeComponent();
            Fill(null, null); //
        }

        public void Fill(object sender, EventArgs e)
        {
            this.reportViewer.Reset();

            MySqlConnection con = new MySqlConnection(Properties.Settings.Default.MySQLconnString);
            con.Open();
            string query = "call sales_balance_analisys('" + EndDate.AddDays(+1).ToString("s") + "')";
            MySqlDataAdapter adpt = new MySqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            adpt.Fill(dt);

            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();

            reportDataSource1.Name = "AccountsRecievableAnalisys"; //Name of the report dataset in our .RDLC file
            reportDataSource1.Value = dt;
            this.reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            this.reportViewer.LocalReport.ReportEmbeddedResource = "Cognitivo.Reporting.Reports.AccountsRecievableAnalisys.rdlc";

            this.reportViewer.RefreshReport();
        }
    }
}
