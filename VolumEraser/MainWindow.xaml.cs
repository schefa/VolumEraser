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

        private Volume selectedItem;

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

            // Assign Elements
            PGBar = progressBar;
            LVReport = lvReport;
            LabelProgress = lblProgress;

            lblSelectedDisk.Content = "";
            lvDrives.ItemsSource = Volumes.getDrives(); 
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
                lblSelectedDisk.Content = selectedItem.Name + " "+ selectedItem.VolumeLabel;
                btnClean.IsEnabled = true; 
            }
            else
            {
                selectedItem = null;
                lblSelectedDisk.Content = null;
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
                btnCancel.IsEnabled = true;

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
                    await VolumeController.eraseVolume(cts.Token, selectedItem);
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
                btnCancel.IsEnabled = false;
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
