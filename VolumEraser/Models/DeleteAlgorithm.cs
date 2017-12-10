namespace VolumEraser.Models
{
    /// <summary>
    /// Model : DeleteAlgorithm
    /// </summary>
    public class DeleteAlgorithm
    {

        public string Title { get; set; } 
        public DeleteAlgorithmEnum Algorithm { get; internal set; }

        public enum DeleteAlgorithmEnum : int
        {
            /// <summary>
            /// 3 passes.
            /// This method is based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (DoD 5220.22-M E).  
            /// It will overwrite a file 3 times.  This method offers medium security, use it only on files that do not contain sensitive information.
            /// </summary>
            DoD_3 = 4,
            /// <summary>
            /// 7 passes.
            /// This method is based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (US DoD 5220.22-M ECE).  
            /// It will overwrite a volume 7 times.  This method incorporates the DoD-3 method.  It is secure and should be used for general files.
            /// </summary>
            DoD_7 = 8,
        }
    }
}
