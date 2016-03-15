﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace cntrl.Controls
{
    public partial class SmartBox_Contact : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SmartBox_Contact));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        entity.dbContext db = new entity.dbContext();
        public event RoutedEventHandler Select;
        private void ContactGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (contactViewSource.View != null)
            {
                Contact = contactViewSource.View.CurrentItem as entity.contact;

                if (Contact != null)
                {
                    ContactID = Contact.id_contact;
                    Text = Contact.name;

                    popContact.IsOpen = false;

                    if (Select != null)
                    { Select(this, new RoutedEventArgs()); }
                }
            }
        }

        public int ContactID { get; set; }
        public bool Get_Customers { get; set; }
        public bool Get_Suppliers { get; set; }
        public bool Get_Employees { get; set; }
        public bool Get_Users { get; set; }

        public entity.contact Contact { get; set; }

        int company_ID = entity.Properties.Settings.Default.company_ID;

        Task taskSearch;
        CancellationTokenSource tokenSource;
        CancellationToken token;

        CollectionViewSource contactViewSource;
       
        public SmartBox_Contact()
        {
            InitializeComponent();
            contactViewSource = ((CollectionViewSource)(FindResource("contactViewSource")));
        }

        private void StartSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ContactGrid_MouseDoubleClick(sender, e);
            }

            else if(e.Key == Key.Up)
            {
                contactViewSource.View.MoveCurrentToPrevious();
                contactViewSource.View.Refresh();
            }
            else if (e.Key == Key.Down)
            {
                contactViewSource.View.MoveCurrentToNext();
                contactViewSource.View.Refresh();
            }
            else
            {
                string SearchText = tbxSearch.Text;

                if (SearchText.Count() >= 3)
                {
                    if (taskSearch != null)
                    {
                        if (taskSearch.Status == TaskStatus.Running)
                        {
                            tokenSource.Cancel();
                        }
                    }

                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    taskSearch = Task.Factory.StartNew(() => Search_OnThread(SearchText), token);
                }
            }
        }

        private static bool EmpSearch(entity.contact emp)
        {
            if (emp.name == "Anshu")
                return true;
            else
                return false;
        }
        private void Search_OnThread(string SearchText)
        {
          
           
            using(entity.db db = new entity.db())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;

                List<entity.contact> results = new List<entity.contact>();
                var param = smartBoxContactSetting.Default.OrderByText;
                var propertyInfo = typeof(entity.contact).GetProperty(param);
               
                var predicate = PredicateBuilder.True<entity.contact>();

                if (Get_Customers)
                {
                    predicate = predicate.And(x => x.is_customer == true);
                }

                if (Get_Suppliers)
                {
                    predicate = predicate.And(x => x.is_supplier == true);
                }

                if (Get_Employees)
                {
                    predicate = predicate.And(x => x.is_employee == true);
                }

                if (param=="code")
                {
                    predicate = predicate.And(x => x.code == SearchText);
                }
                else if (param=="name")
                {
                    predicate = predicate.And(x => x.name == SearchText);
                }
                else if (param=="Default" || param=="")
                {
                    predicate = predicate.And(x => x.code.Contains(SearchText) || x.name.Contains(SearchText));
                }

                predicate = predicate.And(x => x.contact_role.can_transact);
                predicate = predicate.And(x => x.is_active);
                

                results.AddRange(db.contacts
                   .Where(predicate).OrderBy(x=>x.name).ToList());
                Dispatcher.InvokeAsync(new Action(() =>
                {
                    contactViewSource.Source = results;
                    Contact = contactViewSource.View.CurrentItem as entity.contact;

                    popContact.IsOpen = true;
                }));
            }
        }

       

        private void _SmartBox_Contact_GotFocus(object sender, RoutedEventArgs e)
        {
            //popToolBar.IsOpen = true;
        }

        private void Add_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            crudContact.contactobject = new entity.contact();
            
            crudContact.entity = db;
            popCrud.IsOpen = true;

            popCrud.Visibility = Visibility.Visible;

        }

        private void crudContact_btnSave_Click(object sender)
        {
            
                if (crudContact.contactList.Count()>0)
                {
                    foreach (entity.contact contact in crudContact.contactList)
                    {
                        if (contact.id_contact==0)
                        {
                            db.db.contacts.Add(contact);
                        }
                        
                        
                    }
                    
                    db.SaveChanges();
                }
           
                 popCrud.IsOpen = false;
            popCrud.Visibility = System.Windows.Visibility.Collapsed;

        }

      

        private void Edit_PreviewMouseUp_1(object sender, MouseButtonEventArgs e)
        {
            crudContact.contactobject = Contact;
            crudContact.entity = db;
            popCrud.IsOpen = true;
            popCrud.Visibility = System.Windows.Visibility.Visible;
        }

        private void _SmartBox_Contact_LostFocus(object sender, RoutedEventArgs e)
        {
            //popToolBar.IsOpen = false;
            //popToolBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Label_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.IsOpen = true;
            popupCustomize.Visibility = System.Windows.Visibility.Visible;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            Controls.smartBoxContactSetting.Default.Save();
            popupCustomize.IsOpen = false;
            popupCustomize.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (rbtncode.IsChecked==true)
            {
                Controls.smartBoxContactSetting.Default.OrderByText = "code";
            }
            if (rbtnname.IsChecked==true)
            {
                Controls.smartBoxContactSetting.Default.OrderByText = "name";
            }
            if (rbtndefault.IsChecked==true)
            {
                Controls.smartBoxContactSetting.Default.OrderByText = "Default";
            }
        }

       
    }
}
