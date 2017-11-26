using VolumEraser.Helpers;
using System.Collections.Generic;
using System.IO;

namespace VolumEraser.Models
{
    public class HardDrives
    {

        public static List<HardDrive> getDrives()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            List<HardDrive> items = new List<HardDrive>();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    var lvItem = new HardDrive()
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
