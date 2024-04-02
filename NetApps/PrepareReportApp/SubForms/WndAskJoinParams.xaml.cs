using System;
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
using System.Windows.Shapes;

namespace PrepareReport.SubForms
{
    /// <summary>
    /// Interaction logic for WndAskJoinParams.xaml
    /// </summary>
    public partial class WndAskJoinParams : Window
    {
        public string SrcFolder;
        public string TargetFilePath;

        public WndAskJoinParams()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SrcFolder = srcFolder.Text;
            TargetFilePath = trgFilePath.Text;
            if (string.IsNullOrEmpty(SrcFolder) || string.IsNullOrEmpty(TargetFilePath))
            {
                string msg = string.Format("Both Source Folder and Target File should be specified!");
                System.Windows.MessageBox.Show(msg);
            }
            DialogResult = true;
            Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
