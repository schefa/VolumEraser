using VolumEraser.Controller;
using VolumEraser.Models;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace VolumEraser.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Attributes

        private Volume selectedItem;

        private DeleteAlgorithm.DeleteAlgorithmEnum algorithm = DeleteAlgorithm.DeleteAlgorithmEnum.DoD_7;

        public static ProgressBar PGBar { get; private set; }
        public static ListView LVReport { get; private set; }

        public static Label LabelProgress { get; private set; }

        public CancellationTokenSource cts;
        
        #endregion
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetLanguageDictionary();

            // Assign Elements
            PGBar = progressBar;
            LVReport = lvReport;
            LabelProgress = lblProgress;
              
            foreach (object obj in Enum.GetValues(typeof(DeleteAlgorithm.DeleteAlgorithmEnum)))
            {
                RadioButton rb = new RadioButton() { Content = obj, };
                lbDeleteAlgorithm.Children.Add(rb);
                rb.Unchecked += new RoutedEventHandler(rb_Unchecked);
                rb.Checked += new RoutedEventHandler(rb_Checked);
                rb.IsChecked =  ((DeleteAlgorithm.DeleteAlgorithmEnum)obj == DeleteAlgorithm.DeleteAlgorithmEnum.DoD_7) ? true : false;
            } 
            
            lvDrives.ItemsSource = Volumes.getDrives(); 
        }

        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    dict.Source = new Uri("..\\Resources\\EN.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\DE.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        public void rb_Unchecked(object sender, RoutedEventArgs e)
        {
            algorithm = DeleteAlgorithm.DeleteAlgorithmEnum.DoD_7;
        }

        public void rb_Checked(object sender, RoutedEventArgs e)
        {
            algorithm = (DeleteAlgorithm.DeleteAlgorithmEnum)((sender as RadioButton).Content);
        }

        /// <summary>
        /// Method to set the current volume to be purged
        /// </summary> 
        private void lvDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnClean.IsEnabled = false; 
            selectedItem = (Volume) lvDrives.SelectedItem;

            // Only removable supported
            if(Models.Volume.checkDriveType(selectedItem.DriveType))
            {
                lvReport.Items.Add(FindResource("volumeSelected") + selectedItem.Name + " " + selectedItem.VolumeLabel);
                btnClean.IsEnabled = true;
                lbDeleteAlgorithm.Visibility = Visibility.Visible; 
            }
            else
            {
                selectedItem = null;
            }
        }
        
        /// <summary>
        /// Button click method to erase a volume
        /// </summary> 
        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            
            if (selectedItem != null && Models.Volume.checkDriveType(selectedItem.DriveType))
            {

                resetProgressBar();
                btnClean.IsEnabled = false;
                btnClean.Visibility = Visibility.Hidden;
                lbDeleteAlgorithm.Visibility = Visibility.Hidden;
                btnCancel.IsEnabled = true;
                btnCancel.Visibility = Visibility.Visible;

                // Clear all content
                lvReport.Items.Add("Speicherplatz wird bereinigt");
                VolumeController.deleteVolume(selectedItem);
                lvReport.Items.Add("Speicherplatz bereinigt");
                listViewReportScrollDown();

                // Start writing random data
                cts = new CancellationTokenSource();
                try
                {
                    lvReport.Items.Add("Formatieren gestartet"); 
                    await VolumeController.eraseVolume(cts.Token, selectedItem, algorithm);
                    MessageBox.Show("Löschen erfolgreich"); 
                }
                catch (Exception ex)
                {
                    VolumeController.deleteVolume(selectedItem);
                    lvReport.Items.Add("Vorgang abgebrochen bei " + lblProgress.Content);
                    MessageBox.Show(ex.Message);
                    resetProgressBar();
                }
                 
                lvReport.Items.Add("Fertig");
                listViewReportScrollDown();
                cts.Cancel();

                btnClean.IsEnabled = true; 
                btnClean.Visibility = Visibility.Visible;
                btnCancel.IsEnabled = false;
                btnCancel.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Method to abort or cancel the process
        /// </summary> 
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        { 
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        public void resetProgressBar()
        {
            lblProgress.Content = "0 %";
            progressBar.Value = 0; 
        }

        public void listViewReportScrollDown()
        {
            lvReport.SelectedIndex = lvReport.Items.Count - 1;
            lvReport.ScrollIntoView(lvReport.SelectedItem);
        }
        
    }
}
