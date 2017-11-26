using VolumEraser.Helpers;
using System.Collections.Generic;
using System.IO;

namespace VolumEraser.Models
{
    public class Volumes
    {

        /// <summary>
        /// Method to get all available volumes
        /// </summary> 
        public static List<Volume> getDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            List<Volume> items = new List<Volume>();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    var lvItem = new Volume()
                    {
                        Name = d.Name,
                        DriveType = d.DriveType,
                        VolumeLabel = d.VolumeLabel,
                        DriveFormat = d.DriveFormat,
                        AvailableFreeSpace = d.AvailableFreeSpace,
                        TotalFreeSpace = d.TotalFreeSpace,
                        TotalTakenSpace = Utilities.humanFilesize(d.TotalSize - d.AvailableFreeSpace),
                        TotalSize = d.TotalSize,
                        RootDirectory = d.RootDirectory,
                    };
                    items.Add(lvItem);
                }
            }
            return items;
        }
    }
}
