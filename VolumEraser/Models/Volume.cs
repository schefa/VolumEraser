using VolumEraser.Helpers;
using System.IO;

namespace VolumEraser.Models
{
    public class Volume
    {

        #region Attributes

        public string Name { get; set; }
        
        public DriveType DriveType { get; set; }

        public string VolumeLabel { get; set; }

        public string DriveFormat { get; set; }

        public long AvailableFreeSpace { get; set; }

        public long TotalFreeSpace { get; set; }

        public long TotalSize { get; set; }

        public string TotalTakenSpace { get; set; }

        public string TotalSizeDisplay
        {
            get { return Utilities.humanFilesize(TotalSize); }
        }

        public string TotalFreeSpaceDisplay
        {
            get { return Utilities.humanFilesize(TotalFreeSpace); }
        }

        public DirectoryInfo RootDirectory { get; set; }

        #endregion

        /// <summary>
        /// Whitelist for all volume types that are allowed to be purged
        /// </summary> 
        public static bool checkDriveType(DriveType driveType)
        {
            return (driveType == DriveType.Removable);
        }
    }
}
