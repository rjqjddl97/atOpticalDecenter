using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogLibrary;
using OpenCvSharp;


namespace ImageLibrary
{
    public class Binary
    {
        bool _IsInRange = false;
        int _minThreshold = 0;
        int _maxThreshold = 255;

        public bool IsInRange
        {
            set { _IsInRange = value; }
        }

        public int MinThreshold
        {
            set { _minThreshold = value; }
        }

        public int MaxThreshold
        {
            set { _maxThreshold = value; }
        }

        public Binary()
        {

        }

        public Binary(int minThreshold, int maxThreshold, bool IsInRange)
        {
            _minThreshold = minThreshold;
            _maxThreshold = maxThreshold;
            _IsInRange = IsInRange;
        }

        public Bitmap Process(Bitmap srcImage)
        {
            Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(srcImage);
            Mat dst = new Mat();

            Cv2.Threshold(src, dst, _maxThreshold, 255, ThresholdTypes.Binary);
            
            return (Bitmap)OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst).Clone();
        }
    }
}
