﻿using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Reporting.WinForms;
using System.Windows.Data;
using entity.BrilloQuery;
using entity;
using System.Linq;
using System.Windows.Media;
using System;

namespace cntrl
{
    public class ReportColumns
    {
        public string Columname { get; set; }
        public bool IsVisibility { get; set; }
    }

    public partial class ReportPanel : UserControl
    {

        public bool ShowDateRange
        {
            get { return _ShowDateRange; }
            set
            {
                _ShowDateRange = value;
                if (value == true)
                {
                    DateRange.Visibility = Visibility.Visible;

                }
            }
        }
        private bool _ShowDateRange;
        public bool ShowProject
        {
            get { return _ShowProject; }
            set
            {
               
                if (_ShowProject!=value)
                {
                    _ShowProject = value;
                    if (value == true)
                    {
                        stpProject.Visibility = Visibility.Visible;
                        using (db db = new db())
                        {
                            ComboProject.ItemsSource = db.projects.Where(x => x.id_company == CurrentSession.Id_Company).ToList();
                        }
                    }
                }
               
            }
        }
        private bool _ShowProject;

        private static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register("ApplicationName", typeof(App.Names), typeof(ReportPanel));
        public App.Names ApplicationName
        {
            get { return (App.Names)GetValue(ApplicationNameProperty); }
            set { SetValue(ApplicationNameProperty, value); }
        }

        private CollectionViewSource ReportViewSource;
        public DateTime StartDate
        {
            get { return AbsoluteDate.Start(_StartDate); }
            set { _StartDate = value;Fill(); }
        }
        private DateTime _StartDate = AbsoluteDate.Start(DateTime.Now.AddMonths(-1));

        public int ProjectID
        {
            get { return _ProjectID; }
            set
            {
                _ProjectID = value;
                Fill();
            }
        }
        private int _ProjectID;

        public DateTime EndDate
        {
            get { return AbsoluteDate.End(_EndDate); }
            set { _EndDate = value; Fill(); }
        }
        private DateTime _EndDate = AbsoluteDate.End(DateTime.Now);

        public DataTable ReportDt
        {
            get
            {
                return _ReportDt;
            }
            set
            {
                _ReportDt = value;
                Filterdt = value;

                stpFilter.Children.Clear();
                foreach (DataColumn item in value.Columns)
                {
                    if (item.DataType == typeof(string))
                    {
                        Label Label = new Label();
                        Label.Name = item.ColumnName;
                        Label.Content = item.ColumnName;
                        Label.Foreground = Brushes.Black;
                        Style lblStyle = Application.Current.FindResource("input_label") as Style;
                        Label.Style = lblStyle;
                        stpFilter.Children.Add(Label);


                        ComboBox ComboBox = new ComboBox();
                        Style cbxStyle = Application.Current.FindResource("input_combobox") as Style;
                        ComboBox.Style = cbxStyle;
                        DataView view = new DataView(value);
                        ComboBox.ItemsSource = view.ToTable(true, item.ColumnName).DefaultView;
                        ComboBox.SelectedValuePath = item.ColumnName;
                        ComboBox.DisplayMemberPath = item.ColumnName;
                        ComboBox.Name = "cbx" + item.ColumnName;
                        ComboBox.BorderBrush = Brushes.White;
                        ComboBox.Foreground = Brushes.Black;
                        ComboBox.IsTextSearchEnabled = true;
                        TextSearch.SetTextPath(ComboBox, item.ColumnName);
                        ComboBox.IsEditable = true;
                        stpFilter.Children.Add(ComboBox);
                    }
                }
            }
        }

        public DataTable _ReportDt;

        public DataTable Filterdt
        {
            get
            {
                return _Filterdt;
            }
            set
            {
                _Filterdt = value;

                foreach (DataColumn item in value.Columns)
                {
                    if (item.DataType == typeof(string))
                    {
                        if (stpFilter.FindName("cbx" + item.ColumnName) != null)
                        {
                            ComboBox combocolumndata = stpFilter.FindName("cbx" + item.ColumnName) as ComboBox;
                            DataView view = new DataView(value);
                            combocolumndata.ItemsSource = view.ToTable(true, item.ColumnName).DefaultView;
                        }
                    }
                }
            }
        }

        public DataTable _Filterdt;

        public void Fill()
        {
            this.reportViewer.Reset();

            ReportDataSource reportDataSource1 = new ReportDataSource();
            Class.Report Report = ReportViewSource.View.CurrentItem as Class.Report;

            if (Report.Parameters.Where(x => x == Class.Report.Types.Project).Count() > 0)
            {
                ShowProject = true;
            }
            if (Report.Parameters.Where(x => x == Class.Report.Types.StartDate || x == Class.Report.Types.EndDate).Count() > 0)
            {
                ShowDateRange = true;
            }

            DataTable dt = new DataTable();

            string query = Report.Query;
            if (Report.ReplaceString != null && Report.ReplaceWithString!=null)
            {
                query = query.Replace(Report.ReplaceString, Report.ReplaceWithString);

            }
            query = query.Replace("@CompanyID", CurrentSession.Id_Company.ToString());
            query = query.Replace("@StartDate", StartDate.ToString("yyyy-MM-dd"));
            query = query.Replace("@EndDate", EndDate.ToString("yyyy-MM-dd"));
            query = query.Replace("@ProjectID", ProjectID.ToString());
            dt = QueryExecutor.DT(query);

            ReportDt = dt;




            reportDataSource1.Name = "DataSet1"; //Name of the report dataset in our .RDLC file
            reportDataSource1.Value = dt; //SalesDB.SalesByDate;

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.ReportEmbeddedResource = Report.Path;

            if (ShowDateRange)
            {
                ReportParameter StartDateParameter = new ReportParameter("StartDate", _StartDate.ToString());
                ReportParameter EndtDateParameter = new ReportParameter("EndDate", _EndDate.ToString());
                reportViewer.LocalReport.SetParameters(new ReportParameter[] { StartDateParameter, EndtDateParameter });
            }
           
            reportViewer.Refresh();
            reportViewer.RefreshReport();
        }
        public void Filter()
        {
            this.reportViewer.Reset();

            ReportDataSource reportDataSource1 = new ReportDataSource();
            Class.Report Report = ReportViewSource.View.CurrentItem as Class.Report;

            reportDataSource1.Name = "DataSet1"; //Name of the report dataset in our .RDLC file
            reportDataSource1.Value = Filterdt; //SalesDB.SalesByDate;

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);

            reportViewer.LocalReport.ReportEmbeddedResource = Report.Path;
            if (ShowDateRange)
            {
                ReportParameter StartDateParameter = new ReportParameter("StartDate", _StartDate.ToString());
                ReportParameter EndtDateParameter = new ReportParameter("EndDate", _EndDate.ToString());
                reportViewer.LocalReport.SetParameters(new ReportParameter[] { StartDateParameter, EndtDateParameter });
            }


            reportViewer.Refresh();
            reportViewer.RefreshReport();
        }

        //public List<ReportColumns> ReportColumn
        //{
        //    get
        //    {
        //        return _ReportColumn;
        //    }
        //    set
        //    {
        //        _ReportColumn = value;
        //        foreach (ReportColumns ReportColumns in _ReportColumn)
        //        {
        //            CheckBox chkbox = new CheckBox();
        //            chkbox.Content = ReportColumns.Columname;
        //            chkbox.IsChecked = ReportColumns.IsVisibility;
        //            stpColumn.Children.Add(chkbox);
        //            chkbox.Checked += CheckBox_Checked;
        //            chkbox.Unchecked += CheckBox_Checked;
        //        }
        //    }
        //}
        //List<ReportColumns> _ReportColumn;

        public ReportPanel()
        {
            InitializeComponent();
        }


        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    CheckBox chk = sender as CheckBox;

        //    if (chk != null)
        //    {
        //        string name = chk.Content.ToString();
        //        ReportColumns ReportColumns = ReportColumn.Where(x => x.Columname.Contains(name)).FirstOrDefault();

        //        if (chk.IsChecked == true)
        //        {
        //            ReportColumns.IsVisibility = true;
        //        }
        //        else
        //        {
        //            ReportColumns.IsVisibility = false;
        //        }
        //    }
        //    Filter();
        //    // Data_Update(null, null);
        //}

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Data_Update(null, null);
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Data_Update(null, null);
        //}

        //private void DateRange_DateChanged(object sender, RoutedEventArgs e)
        //{
        //    Data_Update(null, null);
        //}

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            Class.Generate Generate = new Class.Generate();
            Generate.GenerateReportList();
            ReportViewSource = (CollectionViewSource)FindResource("ReportViewSource");
            ReportViewSource.Source = Generate.ReportList.Where(x => x.Application == ApplicationName).ToList();

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Fill();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Fill();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            string filter="";
            foreach (object Combo in stpFilter.Children)
            {
                if (Combo.GetType()==typeof(ComboBox))
                {
                    ComboBox comboobox = Combo as ComboBox;

                    if (comboobox.SelectedValue != null)
                    {
                        filter += " and ";
                        filter += comboobox.DisplayMemberPath + "='" + comboobox.SelectedValue + "'";
                    }
                    
                }
               
                

                
            }
            filter = filter.Substring(5);
            if (ReportDt.Rows.Count>0)
            {
                if (ReportDt.Select(filter).Any())
                {

                    Filterdt = ReportDt.Select(filter).CopyToDataTable();
                 

                }
            }

            Filter();


        }
    }
}
