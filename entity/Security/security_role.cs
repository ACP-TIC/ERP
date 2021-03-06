namespace entity
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class security_role : IDataErrorInfo, INotifyPropertyChanged
    {
        private Brillo.Activation Activation = new Brillo.Activation();

        public security_role()
        {
            is_active = true;
            id_company = CurrentSession.Id_Company;
            see_cost = false;

            security_curd = new List<security_crud>();
            security_user = new List<security_user>();
            security_role_privilage = new List<security_role_privilage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_role { get; set; }

        public int id_company { get; set; }
        public int? id_department { get; set; }

        [Required]
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("name");
            }
        }

        private string _name;

        public bool see_cost { get; set; }
        public bool is_active { get; set; }
        public bool is_master { get; set; }

        public string version { get; set; }

        [NotMapped]
        public CurrentSession.Versions Version
        {
            get { return Activation.VersionDecrypt(this); }
            set { version = Activation.VersionEncrypt(value); }
        }

        public virtual app_company app_company { get; set; }
        public virtual app_department app_department { get; set; }
        public virtual ICollection<security_user> security_user { get; set; }
        public virtual ICollection<security_crud> security_curd { get; set; }
        public virtual ICollection<security_role_privilage> security_role_privilage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        [NotMapped]
        public System.Data.Entity.EntityState State
        {
            get
            {
                return _State;
            }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged("State");
                }
            }
        }

        private System.Data.Entity.EntityState _State;

        [NotMapped]
        public bool IsSelected { get; set; }

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }

        public string this[string columnName]
        {
            get
            {
                // apply property level validation rules
                if (columnName == "name")
                {
                    if (string.IsNullOrEmpty(name))
                        return "Name needs to be filled";
                }
                return "";
            }
        }
    }
}