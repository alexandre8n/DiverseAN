using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for FileMoverHelperView.xaml
    /// </summary>
    public partial class HelperCtlFileMover : UserControl
    {
        List<MovedFile> movedFiles = new List<MovedFile>();
        public List<MovedFile> movedFilesSelectedAsDone = new List<MovedFile>();
        public string targetFolder;

        public HelperCtlFileMover()
        {
            InitializeComponent();
        }
        public HelperCtlFileMover(string trgFolder) : this()
        {
            targetFolder = trgFolder;
            GetFilesFromTargetFolder();
            lvFiles.ItemsSource = movedFiles;
        }



        private void GetFilesFromTargetFolder()
        {
            movedFiles.Clear();
            DirectoryInfo dir = new DirectoryInfo(targetFolder);
            foreach (System.IO.FileInfo f in dir.GetFiles("*.*"))
            {
                var movedFile = new MovedFile() { Name = f.Name, FileSize = f.Length, ModDate = f.LastWriteTime.ToString() };
                movedFiles.Add(movedFile);
            }

        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = lvFiles.SelectedItems;
            if (selected.Count < 1)
                return;
            foreach (var item in selected)
            {
                movedFilesSelectedAsDone.Add((MovedFile)item);
            }
            var w1 = Window.GetWindow(this);
            w1.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            var w1 = Window.GetWindow(this);
            w1.DialogResult = false;
        }
    }

    public class MovedFile
    {
        public string Name { get; set; }
        public string ModDate { get; set; }
        public long FileSize { get; set; }
    }

}

/*
                    <ListViewItem>
                        <StackPanel Orientation="Horizontal">
                            <Image Source="/PrepareReport;component/Images/SmallIcon.png" Margin="0,0,5,0" />
                            <TextBlock>Green</TextBlock>
                        </StackPanel>
                    </ListViewItem>
                    <ListViewItem>
                        <StackPanel Orientation="Horizontal">
                            <Image Source="/PrepareReport;component/Images/SmallIcon.png" Margin="0,0,5,0" />
                            <TextBlock>Blue</TextBlock>
                        </StackPanel>
                    </ListViewItem>
                    <ListViewItem IsSelected="True">
                        <StackPanel Orientation="Horizontal">
                            <Image Source="/PrepareReport;component/Images/SmallIcon.png" Margin="0,0,5,0" />
                            <TextBlock>Red</TextBlock>
                        </StackPanel>
                    </ListViewItem>
*/
