using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogLibrary;
using OpenCvSharp;
using RecipeManager;

namespace ImageLibrary
{
    public class OpticalHistory
    {
        public Log _log = new Log();
        private bool _isLog = false;
        public SystemParams _SystemParameter = new SystemParams();
        public bool IsLog
        {
            set { _isLog = value; }
            get { return _isLog; }
        }
        public OpticalHistory()
        {

        }
        public OpticalHistory(SystemParams mSystemParam)
        {
            _SystemParameter = mSystemParam;
        }
        unsafe public bool OpticalHistoryProcess(Bitmap srcImage, int binaryTheshold)
        {
            Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(srcImage);
            Mat dst = new Mat();
            Mat labels = new Mat(), stats = new Mat(), centeroids = new Mat();

            int i;

            string sPath = Environment.CurrentDirectory;

            //dst = src.Blur(new OpenCvSharp.Size(3, 3));

            //Cv2.Threshold(src, dst, binaryTheshold, 255, ThresholdTypes.Binary);
            Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
            
            int label = 0;

            float fCenterx = srcImage.Width / 2;
            float fCentery = srcImage.Height / 2;
            float fMin = float.MaxValue;

            Mat hist = new Mat();
            Mat result = Mat.Ones(new OpenCvSharp.Size(256, src.Height), MatType.CV_8UC1);
            int[] hdims = { 256 };
            Rangef[] ranges = { new Rangef(0, 256), };
            Cv2.CalcHist(new Mat[] { dst }, new int[] { 0 }, null, hist, 1, hdims, ranges);
            Cv2.Normalize(hist, hist, 0, 255, NormTypes.MinMax);
            for (i = 0; i < hist.Rows; i++)
            {
                Cv2.Line(result, new OpenCvSharp.Point(i, src.Height), new OpenCvSharp.Point(i, src.Height - hist.Get<float>(i)), Scalar.White);
            }
            

            Cv2.ImShow("Histogram", result);

            if (_isLog)
                _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("Histogram Process 완료"));

            if (dst == null)
            {
                src.Dispose();
                dst.Dispose();
                labels.Dispose();
                stats.Dispose();
                centeroids.Dispose();

                return false;
            }

            return true;
        }        
    }
}
