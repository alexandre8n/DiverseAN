using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace PrepareReport.SubForms
{
    /// <summary>
    /// Interaction logic for ReportMoverHelperCtl.xaml
    /// </summary>
    public partial class HelperCtlReportMover : UserControl
    {
        ObservableCollection<MissedFolder> missedFolders = null;
        public List<MissedFolder> MissedFoldersSelectedAsDone = new List<MissedFolder>();
        //private ObservableCollection<User> users = new ObservableCollection<User>();

        public string targetFolder;

        public HelperCtlReportMover(List<string> folders)
            : this()
        {
            missedFolders = new ObservableCollection<MissedFolder>();
            foreach (var s in folders)
                missedFolders.Add(new MissedFolder() { Name = s });
            lvFolders.ItemsSource = missedFolders;
        }

        public HelperCtlReportMover()
        {
            InitializeComponent();
        }

        private void btnOpen_Click_1(object sender, RoutedEventArgs e)
        {
            MissedFolder f = (MissedFolder)(((Button)sender).DataContext);
            Process.Start(f.Name);
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            MissedFoldersSelectedAsDone = missedFolders.Where(x => x.IsFixed).ToList();
            var w1 = Window.GetWindow(this);
            w1.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            MissedFoldersSelectedAsDone = null;
            var w1 = Window.GetWindow(this);
            w1.DialogResult = false;

        }

        private void btnResolve_Click(object sender, RoutedEventArgs e)
        {
            MissedFolder f = (MissedFolder)(((Button)sender).DataContext);
            f.IsFixed = !f.IsFixed;
        }
    }

    public class MissedFolder : INotifyPropertyChanged
    {
        public MissedFolder()
        {
            IsFixed = false;
        }
        public string Name { get; set; }
        private bool _isFixed;
        public bool IsFixed 
        {
            get { return this._isFixed; }
            set
            {
                if (this._isFixed != value)
                {
                    this._isFixed = value;
                    this.NotifyPropertyChanged("IsFixed");
                    this.NotifyPropertyChanged("ImgVisibility");
                }
            }
        }
        public string FixedBtnText 
        { 
            get
            {
                return (IsFixed) ? "Mark as Unresolved" : "Mark as fixed";
            } 
        }
        public string ImgVisibility
        { 
            get
            {
                return (IsFixed) ? "Visible" : "Hidden";
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        
    }

}
