using VolumEraser.Controller;
using VolumEraser.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VolumEraser
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Attributes

        private HardDrive selectedItem;

        public static ProgressBar PGBar { get; private set; }
        public static ListView LVReport { get; private set; }

        public static Label LabelProgress { get; private set; }

        public CancellationTokenSource cts;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Assign Elements
            PGBar = progressBar;
            LVReport = lvReport;
            LabelProgress = lblProgress;

            lblSelectedDisk.Content = "";
            lvDrives.ItemsSource = HardDrives.getDrives(); 
        }

        private void lvDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnClean.IsEnabled = false; 
            selectedItem = (HardDrive) lvDrives.SelectedItem;

            // Only removable supported
            if(checkDriveType(selectedItem.DriveType))
            {
                lblSelectedDisk.Content = selectedItem.Name + " "+ selectedItem.VolumeLabel;
                btnClean.IsEnabled = true; 
            }
            else
            {
                selectedItem = null;
                lblSelectedDisk.Content = null;
            }
        }
        
        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            
            if (selectedItem != null && checkDriveType(selectedItem.DriveType))
            {

                resetProgressBar();
                btnClean.IsEnabled = false;
                btnCancel.IsEnabled = true;

                // Clear all content
                lvReport.Items.Add("Speicherplatz wird bereinigt");
                HardDriveController.deleteContent(selectedItem);
                lvReport.Items.Add("Speicherplatz bereinigt");
                listViewReportScrollDown();

                // Start writing random data
                cts = new CancellationTokenSource();
                try
                {
                    lvReport.Items.Add("Formatieren gestartet");
                    await HardDriveController.Delete(cts.Token, selectedItem);
                    MessageBox.Show("Löschen erfolgreich"); 
                }
                catch (Exception ex)
                {
                    HardDriveController.deleteContent(selectedItem);
                    lvReport.Items.Add("Vorgang abgebrochen bei " + lblProgress.Content);
                    MessageBox.Show(ex.Message);
                    resetProgressBar();
                }
                 
                lvReport.Items.Add("Fertig");
                listViewReportScrollDown();
                cts.Cancel();

                btnClean.IsEnabled = true;
                btnCancel.IsEnabled = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        { 
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        public bool checkDriveType(DriveType driveType) {
            return (driveType == DriveType.Removable); 
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
