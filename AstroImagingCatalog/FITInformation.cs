using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroImagingCatalog
{
    public class FITInformation
    {
        public long ID { get; set; }
        public string ObjectName { get; set; }
        public string DateTaken { get; set; }
        public string FilesDirectory { get; set; }
        public int FocalLength { get; set; }
        public string ImagingCamera { get; set; }
        public string Binning { get; set; }
        public string SiteLat { get; set; }
        public string SiteLong { get; set; }
        public int CameraGain { get; set; }
        public int CameraOffset { get; set; }
        public decimal CameraTemp { get; set; }
        public string ExposureTime { get; set; }
    }
}
