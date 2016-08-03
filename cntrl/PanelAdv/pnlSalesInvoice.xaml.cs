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
using entity;
namespace cntrl.PanelAdv
{
    /// <summary>
    /// Interaction logic for pnlSalesInvoice.xaml
    /// </summary>
    public partial class pnlSalesInvoice : UserControl
    {
        CollectionViewSource sales_invoiceViewSource;

        private List<sales_invoice> _selected_sales_invoice;
        public List<sales_invoice> selected_sales_invoice
        {
            get { return _selected_sales_invoice; }
            set
            {
                _selected_sales_invoice = value;
            }
        }

        public contact _contact { get; set; }
        public ImpexDB _entity { get; set; }

        public pnlSalesInvoice()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Do not load your data at design time.
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                //Load your data here and assign the result to the CollectionViewSource.


                if (_contact != null)
                {
                    sales_invoiceViewSource = (CollectionViewSource)Resources["sales_invoiceViewSource"];
                    sales_invoiceViewSource.Source = _entity.sales_invoice.Where(x => x.id_contact == _contact.id_contact && x.sales_return.Count()==0).ToList();




                }
            }
        }

        public event btnSave_ClickedEventHandler SalesInvoice_Click;
        public delegate void btnSave_ClickedEventHandler(object sender);
        public void btnSave_MouseUp(object sender, EventArgs e)
        {
            if (sales_invocieDatagrid.ItemsSource != null)
            {
                List<sales_invoice> sales_invoice = sales_invocieDatagrid.ItemsSource.OfType<sales_invoice>().ToList();
                selected_sales_invoice = sales_invoice.Where(x => x.IsSelected == true).ToList();

                if (SalesInvoice_Click != null)
                {
                    SalesInvoice_Click(sender);
                }   
            }
        }

        private void sales_invocieDatagrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            if (_entity.sales_invoice_detail.Count() > 0)
            {
                sales_invoice _sales_invoice = ((System.Windows.Controls.DataGrid)sender).SelectedItem as sales_invoice;
                int id_purchase_invoice = _sales_invoice.id_sales_invoice;
                System.Windows.Controls.DataGrid RowDataGrid = e.DetailsElement as System.Windows.Controls.DataGrid;
                var salesInvoice = _sales_invoice.sales_invoice_detail;
                RowDataGrid.ItemsSource = salesInvoice;
            }
        }

        private void ContactPref(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sbxContact.ContactID > 0)
                {
                    contact contact = _entity.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                    sales_invoiceViewSource = (CollectionViewSource)Resources["sales_invoiceViewSource"];
                    sales_invoiceViewSource.Source = _entity.sales_invoice.Where(x => x.id_contact == _contact.id_contact && x.sales_return.Count() == 0).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
