using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using LogLibrary;
using OpenCvSharp;
using RecipeManager;

namespace ImageLibrary
{
    public class OpticalSpot : IDisposable
    {
        public Log _log = new Log();

        private bool _isLog = false;
        public SystemParams _SystemParameter = new SystemParams();
        public int PeakIndex_X = 0, PeakIndex_Y = 0;
        public static int IndexPeak_X = 0, IndexPeak_Y = 0;
        public bool IsLog
        {
            set { _isLog = value; }
            get { return _isLog; }
        }

        public OpticalSpot(SystemParams mSystemParam)
        {
            _SystemParameter = mSystemParam;
        }

        public void Dispose()
        {

        }

        unsafe static public Blob OpticalSpotDetectProcess(Bitmap sourceImage, int binaryTheshold_H, int binaryTheshold_V, int blobSizeMinimum, int blobSizeMaximum)
        {
            int i;
            if (sourceImage != null)
            {
                List<Blob> mLedBlobs = new List<Blob>();
                mLedBlobs.Clear();

                List<Blob> mLedBlobs_H = new List<Blob>();
                mLedBlobs_H.Clear();

                List<Blob> mLedBlobs_V = new List<Blob>();
                mLedBlobs_V.Clear();

                //mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("자동 검사 중 광축 측정 시작"));        


                Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(sourceImage);
                Mat dst = new Mat();
                Mat labels = new Mat(), stats = new Mat(), centeroids = new Mat();
                           

                Cv2.Threshold(src, dst, binaryTheshold_H, 255, ThresholdTypes.Binary);

                //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst).Save(@"D:\Work\Software\PhotoInpsection\atPhotoInspection\ImageprocessDebug\binary.bmp", ImageFormat.Bmp);                

                int labelCount = Cv2.ConnectedComponentsWithStats(dst, labels, stats, centeroids, PixelConnectivity.Connectivity8);
                int label = 0;

                //float fCenterx = tempImage.Width / 2;
                //float fCentery = tempImage.Height / 2;
                //float fMin = float.MaxValue;
                double[] tempimagew = new double[sourceImage.Width];
                double[] tempimageh = new double[sourceImage.Height];

                ref double[] refimgw = ref tempimagew;
                ref double[] refimgh = ref tempimageh;
                for (i = 1; i < labelCount; ++i)
                {
                    int size = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                    if (size >= (blobSizeMinimum * blobSizeMinimum) && size <= (blobSizeMaximum * blobSizeMaximum))
                    {
                        int left, top, width, height, peakindexx, peakindexy, peak;

                        left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                        top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                        width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                        height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                        Rect inspetarea = new Rect(left, top, width, height);

                        //PointF ptCenter = new PointF((float)centeroids.At<double>(i, 0), (float)centeroids.At<double>(i, 1));
                        PointF ptCenter = new PointF((float)(left + (width / 2f)), (float)(top + (height / 2f)));

                        GetPixelHistogramStep(sourceImage, inspetarea, ref refimgw, ref refimgh, ref IndexPeak_X, ref IndexPeak_Y);

                        peakindexx = IndexPeak_X;
                        peakindexy = IndexPeak_Y;

                        //peak = (int)((refimgh[peakindexy] + refimgw[peakindexx]) / 2);
                        peak = (int)(refimgw[peakindexx]);
                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y));
                        if ((width >= (int)(blobSizeMinimum * 0.5)) && (height >= (int)(blobSizeMinimum * 0.5)))
                            mLedBlobs_H.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));
                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));                        
                    }
                }
                Cv2.Threshold(src, dst, binaryTheshold_V, 255, ThresholdTypes.Binary);
                labelCount = Cv2.ConnectedComponentsWithStats(dst, labels, stats, centeroids, PixelConnectivity.Connectivity8);
                label = 0;
                for (i = 1; i < labelCount; ++i)
                {
                    int size = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                    if (size >= (blobSizeMinimum * blobSizeMinimum) && size <= (blobSizeMaximum * blobSizeMaximum))
                    {
                        int left, top, width, height, peakindexx, peakindexy, peak;

                        left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                        top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                        width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                        height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                        Rect inspetarea = new Rect(left, top, width, height);

                        //PointF ptCenter = new PointF((float)centeroids.At<double>(i, 0), (float)centeroids.At<double>(i, 1));
                        PointF ptCenter = new PointF((float)(left + (width / 2f)), (float)(top + (height / 2f)));

                        GetPixelHistogramStep(sourceImage, inspetarea, ref refimgw, ref refimgh, ref IndexPeak_X, ref IndexPeak_Y);

                        peakindexx = IndexPeak_X;
                        peakindexy = IndexPeak_Y;

                        peak = (int)(refimgh[peakindexy]);

                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y));
                        if ((width >= (int)(blobSizeMinimum * 0.5)) && (height >= (int)(blobSizeMinimum * 0.5)))
                            mLedBlobs_V.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));
                        
                    }
                }

                if (mLedBlobs_H.Count == mLedBlobs_V.Count)
                {
                    label = 0;
                    int imgpeak = 0;
                    for (i = 0; i < mLedBlobs_H.Count; ++i)
                    {
                        imgpeak = (mLedBlobs_H[i].PixelPeak + mLedBlobs_V[i].PixelPeak) / 2;
                        //if(mLedBlobs_H[i].Height > )
                        mLedBlobs.Add(new Blob(label++, mLedBlobs_H[i].Left, mLedBlobs_V[i].Top, mLedBlobs_H[i].Width, mLedBlobs_V[i].Height,mLedBlobs_H[i].Area, mLedBlobs_H[i].CenterX, mLedBlobs_V[i].CenterY, mLedBlobs_H[i].PixelPeakXIndex, mLedBlobs_V[i].PixelPeakYIndex, imgpeak));
                    }
                }
                //if (_isLog)
                //    _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("CCL 완료(레이블 수:{0}, Blob 수:{1})", labelCount, mLedBlobs.Count));

                if (mLedBlobs.Count == 0)
                {
                    src.Dispose();
                    dst.Dispose();
                    labels.Dispose();
                    stats.Dispose();
                    centeroids.Dispose();
                    mLedBlobs = null;
                    return null;
                }
                else if (mLedBlobs.Count == 1)
                {
                    return mLedBlobs[0];
                }
                else
                    return null;
            }
            else
            {
                //mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("자동 검사 중 LED 검사 이미지가 로드되지 않았습니다"));
                return null;
            }
        
        /*
        // 투광축의 평균 좌표를 이용하여, 편심 측정
        // X축 Pitch 거리 계산

        for(int i = 1; i < workParams.InspectionPositions.Count; ++i)
        {
            if(workParams.InspectionPositions[i - 1].ePositionType == INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE
                && workParams.InspectionPositions[i].ePositionType == INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
            {
                if(workParams.InspectionPositions[i-1].OpticalSpotROI.Width == 0 || workParams.InspectionPositions[i-1].OpticalSpotROI.Height == 0
                    || workParams.InspectionPositions[i].OpticalSpotROI.Width == 0 || workParams.InspectionPositions[i].OpticalSpotROI.Height == 0)
                {
                    workParams.InspectionPositions[i].DistanceX = -1f;
                    workParams.InspectionPositions[i].IsResult = InspectionResult.False;

                    continue;
                }

                float fPreviousX = (workParams.InspectionPositions[i - 1].OpticalSpotROI.Left + workParams.InspectionPositions[i - 1].OpticalSpotROI.Right) / 2f;
                float fCurrentX = (workParams.InspectionPositions[i].OpticalSpotROI.Left + workParams.InspectionPositions[i].OpticalSpotROI.Right) / 2f;

                float fDiffX1 = (fPreviousX - workParams.ReferencePositionX) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f;
                float fDiffX2 = (fCurrentX - workParams.ReferencePositionX) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f;

                float fDistance = Math.Abs(fDiffX2 - fDiffX1);

                workParams.InspectionPositions[i].DistanceX = fDistance;

                if(fDistance > workParams.OpticalSpotDistanceFromSpotCenterToReferenceThreshold)
                {
                    workParams.InspectionPositions[i].IsResult = InspectionResult.False;
                }
            }
        }


        // Y축은 평균 Y축에서 벗어난 광축을 찾는다.
        //float fMeanX = 0;
        float fMeanY = 0;

        int opticalSpotCount = 0;

        for (int i = 0; i < workParams.InspectionPositions.Count; ++i)
        {
            if (workParams.InspectionPositions[i].ePositionType == INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
            {
                if (workParams.InspectionPositions[i].OpticalSpotROI.Width == 0 || workParams.InspectionPositions[i].OpticalSpotROI.Height == 0)
                    continue;

                //float fCenterX = (workParams.InspectionPositions[i].OpticalSpotROI.Left + workParams.InspectionPositions[i].OpticalSpotROI.Right) / 2f;
                float fCenterY = (workParams.InspectionPositions[i].OpticalSpotROI.Top + workParams.InspectionPositions[i].OpticalSpotROI.Bottom) / 2f;

                //fMeanX += fCenterX;
                fMeanY += fCenterY;

                opticalSpotCount++;
            }
        }

        if (opticalSpotCount > 0)
        {
            //fMeanX /= opticalSpotCount;
            fMeanY /= opticalSpotCount;
        }

        for (int i = 0; i < workParams.InspectionPositions.Count; ++i)
        {
            if (workParams.InspectionPositions[i].ePositionType == INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
            {
                //float fCenterX = (workParams.InspectionPositions[i].OpticalSpotROI.Left + workParams.InspectionPositions[i].OpticalSpotROI.Right) / 2f;
                float fCenterY = (workParams.InspectionPositions[i].OpticalSpotROI.Top + workParams.InspectionPositions[i].OpticalSpotROI.Bottom) / 2f;

                //float fDistanceX = Math.Abs(Convert.ToSingle(fCenterX - fMeanX) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);
                float fDistanceY = Math.Abs(Convert.ToSingle(fCenterY - fMeanY) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);

                //workParams.InspectionPositions[i].DistanceX = fDistanceX;
                workParams.InspectionPositions[i].DistanceY = fDistanceY;

                if (fDistanceY > workParams.OpticalSpotDistanceFromSpotCenterToReferenceThreshold)
                {
                    workParams.InspectionPositions[i].IsResult = InspectionResult.False;
                }
            }
        }
        */
        }

        unsafe public bool OpticalSpotProperties(Bitmap srcImage, WorkParams workParams, int index)
        {
            /*
            Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(srcImage);
            
            int i;

            bool IsAverageBrightness = true;
            bool IsOpticalSpotSize = true;
            bool IsOpticalSpotDistance = true;
            bool IsOpticalSpotDerivation = true;

            float fCenterx = srcImage.Width / 2f;
            float fCentery = srcImage.Height / 2f;
                        
            if (workParams.InspectionPositions[index].OpticalSpotROI.Width > 0 && workParams.InspectionPositions[index].OpticalSpotROI.Height > 0)
            {
                workParams.InspectionPositions[index].OpticalSpotSizeX = Convert.ToSingle(workParams.InspectionPositions[index].OpticalSpotROI.Width * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);
                workParams.InspectionPositions[index].OpticalSpotSizeY = Convert.ToSingle(workParams.InspectionPositions[index].OpticalSpotROI.Height * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);

                if (_isLog)
                    _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("스팟 크기 X:{0:0.000}mm, Y:{1:0.000}mm", workParams.InspectionPositions[index].OpticalSpotSizeX, workParams.InspectionPositions[index].OpticalSpotSizeY));

                // 광축 크기
                if (workParams.IsOpticalSpotSizeInspection)
                {
                    if (workParams.InspectionPositions[index].OpticalSpotSizeX < workParams.OpticalSpotMinSizeXThreshold
                        || workParams.InspectionPositions[index].OpticalSpotSizeX > workParams.OpticalSpotMaxSizeXThreshold
                        || workParams.InspectionPositions[index].OpticalSpotSizeY < workParams.OpticalSpotMinSizeYThreshold
                        || workParams.InspectionPositions[index].OpticalSpotSizeY > workParams.OpticalSpotMaxSizeYThreshold)
                    {
                        IsOpticalSpotSize = false;
                    }
                }

                // 투광축 X, Y 거리 측정
                // 평균 광축을 이용하지 않을때는 절대 좌표(카메라 중심)를 이용한다.
                if (!workParams.IsOpticalSpotDistanceFromSpotCenterToReference)
                {
                    if (workParams.InspectionPositions[index].eInspectionMode == INSPECTION_MODE.INSPECTION_MODE_OPTICAL_SPOT)
                    {
                        float fCenterX = (workParams.InspectionPositions[index].OpticalSpotROI.Left + workParams.InspectionPositions[index].OpticalSpotROI.Right) / 2f;
                        float fCenterY = (workParams.InspectionPositions[index].OpticalSpotROI.Top + workParams.InspectionPositions[index].OpticalSpotROI.Bottom) / 2f;

                        float fDistanceX = Math.Abs(Convert.ToSingle(fCenterX - workParams.ReferencePositionX) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);
                        float fDistanceY = Math.Abs(Convert.ToSingle(fCenterY - workParams.ReferencePositionY) * workParams.CameraOpticalSpotOnePixelResolution * 0.001f);

                        workParams.InspectionPositions[index].DistanceX = fDistanceX;
                        workParams.InspectionPositions[index].DistanceY = fDistanceY;

                        if (fDistanceX > workParams.OpticalSpotDistanceFromSpotCenterToReferenceThreshold || fDistanceY > workParams.OpticalSpotDistanceFromSpotCenterToReferenceThreshold)
                        {
                            workParams.InspectionPositions[index].IsResult = InspectionResult.False;
                        }
                    }
                }


                int width = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Width);
                int height = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Height);

                float[] fDataX = new float[width];
                float[] fDataY = new float[height];

                workParams.ProfileX = new float[width];
                workParams.ProfileY = new float[height];

                byte* image = (byte*)src.DataPointer;
                long stride = src.Step1();
                long tempIndex = 0;

                int count = 0;

                float fMax = 0f;
                float fMaxX = 0;
                float fMaxY = 0;

                workParams.InspectionPositions[index].AverageBrightness = 0f;

                for (int y = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Top); y < workParams.InspectionPositions[index].OpticalSpotROI.Bottom; ++y)
                {
                    tempIndex = y * stride;

                    for (int x = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Left); x < workParams.InspectionPositions[index].OpticalSpotROI.Right; ++x)
                    {
                        fDataY[count] += image[tempIndex + x];
                        workParams.InspectionPositions[index].AverageBrightness += image[tempIndex + x];
                    }

                    fDataY[count] /= workParams.InspectionPositions[index].OpticalSpotROI.Width;

                    count++;
                }

                count = 0;

                for (int x = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Left); x < workParams.InspectionPositions[index].OpticalSpotROI.Right; ++x)
                {
                    for (int y = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Top); y < workParams.InspectionPositions[index].OpticalSpotROI.Bottom; ++y)
                    {
                        fDataX[count] += image[y * stride + x];
                    }

                    fDataX[count] /= workParams.InspectionPositions[index].OpticalSpotROI.Height;

                    count++;
                }

                workParams.InspectionPositions[index].AverageBrightness /= (workParams.InspectionPositions[index].OpticalSpotROI.Width * workParams.InspectionPositions[index].OpticalSpotROI.Height);

                // 광량 체크
                if (workParams.IsOpticalSpotAverageBrightnessInspection)
                {
                    if (_isLog)
                        _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("광량:{0:0.000}DN", workParams.InspectionPositions[index].AverageBrightness));

                    if (workParams.InspectionPositions[index].AverageBrightness < workParams.OpticalSpotMinAverageBrightnessThreshold
                        || workParams.InspectionPositions[index].AverageBrightness > workParams.OpticalSpotMaxAverageBrightnessThreshold)
                    {
                        IsAverageBrightness = false;
                    }
                }

                // Mean Filter
                workParams.ProfileX[0] = (fDataX[0] + fDataX[1]) / 2f;
                workParams.ProfileX[fDataX.Length - 1] = (fDataX[fDataX.Length - 1] + fDataX[fDataX.Length - 2]) / 2f;

                workParams.ProfileY[0] = (fDataY[0] + fDataY[1]) / 2f;
                workParams.ProfileY[fDataY.Length - 1] = (fDataY[fDataY.Length - 1] + fDataY[fDataY.Length - 2]) / 2f;

                for (i = 1; i < workParams.InspectionPositions[index].OpticalSpotROI.Width - 1; ++i)
                {
                    workParams.ProfileX[i] = (fDataX[i - 1] + fDataX[i] + fDataX[i + 1]) / 3f;
                }

                for (i = 1; i < workParams.InspectionPositions[index].OpticalSpotROI.Height - 1; ++i)
                {
                    workParams.ProfileY[i] = (fDataY[i - 1] + fDataY[i] + fDataY[i + 1]) / 3f;
                }

                // profile Min, Max
                fMax = 0;

                for (i = 0; i < workParams.InspectionPositions[index].OpticalSpotROI.Width; ++i)
                {
                    if (workParams.ProfileX[i] > fMax)
                    {
                        fMax = workParams.ProfileX[i];
                        fMaxX = i;
                    }
                }

                fMax = 0;

                for (i = 0; i < workParams.InspectionPositions[index].OpticalSpotROI.Height; ++i)
                {
                    if (workParams.ProfileY[i] > fMax)
                    {
                        fMax = workParams.ProfileY[i];
                        fMaxY = i;
                    }
                }

                workParams.InspectionPositions[index].OpticalSpotCenter = new PointF(workParams.InspectionPositions[index].OpticalSpotROI.Left + fMaxX,
                    workParams.InspectionPositions[index].OpticalSpotROI.Top + fMaxY);

                // Brightness Horizontal, Vertical
                workParams.InspectionPositions[index].HorizontalBrightnessA = 0;
                count = 0;

                for (i = 0; i < workParams.InspectionPositions[index].OpticalSpotROI.Width / 2; ++i)
                {
                    workParams.InspectionPositions[index].HorizontalBrightnessA += workParams.ProfileX[i];
                    count++;
                }

                workParams.InspectionPositions[index].HorizontalBrightnessA /= count;

                workParams.InspectionPositions[index].HorizontalBrightnessB = 0;
                count = 0;

                for (i = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Width / 2); i < workParams.InspectionPositions[index].OpticalSpotROI.Width; ++i)
                {
                    workParams.InspectionPositions[index].HorizontalBrightnessB += workParams.ProfileX[i];
                    count++;
                }

                workParams.InspectionPositions[index].HorizontalBrightnessB /= count;

                workParams.InspectionPositions[index].VerticalBrightnessA = 0;
                count = 0;

                for (i = 0; i < workParams.InspectionPositions[index].OpticalSpotROI.Height / 2; ++i)
                {
                    workParams.InspectionPositions[index].VerticalBrightnessA += workParams.ProfileY[i];
                    count++;
                }

                workParams.InspectionPositions[index].VerticalBrightnessA /= count;

                workParams.InspectionPositions[index].VerticalBrightnessB = 0;
                count = 0;

                for (i = Convert.ToInt32(workParams.InspectionPositions[index].OpticalSpotROI.Height / 2); i < workParams.InspectionPositions[index].OpticalSpotROI.Height; ++i)
                {
                    workParams.InspectionPositions[index].VerticalBrightnessB += workParams.ProfileY[i];
                    count++;
                }

                workParams.InspectionPositions[index].VerticalBrightnessB /= count;

                workParams.InspectionPositions[index].VerticalDeviation = Convert.ToInt32(Math.Abs(workParams.InspectionPositions[index].VerticalBrightnessA - workParams.InspectionPositions[index].VerticalBrightnessB) / workParams.InspectionPositions[index].AverageBrightness * 100f);
                workParams.InspectionPositions[index].HorizontalDeviation = Convert.ToInt32(Math.Abs(workParams.InspectionPositions[index].HorizontalBrightnessA - workParams.InspectionPositions[index].HorizontalBrightnessB) / workParams.InspectionPositions[index].AverageBrightness * 100f);

                // 수평/수직 광량 비교 결과
                if (workParams.IsOpticalSpotDerivationInspection)
                {
                    if (_isLog)
                        _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("수평({0:0.000}DN,{1:0.000}DN), 수직({2:0.000}DN,{3:0.000}DN)",
                            workParams.InspectionPositions[index].VerticalBrightnessA,
                            workParams.InspectionPositions[index].VerticalBrightnessB,
                            workParams.InspectionPositions[index].HorizontalBrightnessA,
                            workParams.InspectionPositions[index].HorizontalBrightnessB));

                    if (workParams.InspectionPositions[index].HorizontalDeviation > workParams.OpticalSpotHVDerivationThreshold
                        || workParams.InspectionPositions[index].VerticalDeviation > workParams.OpticalSpotHVDerivationThreshold)
                    {
                        IsOpticalSpotDerivation = false;
                    }
                }

                workParams.InspectionPositions[index].IsResult = (IsAverageBrightness && IsOpticalSpotDerivation && IsOpticalSpotDistance && IsOpticalSpotSize) ?
                    InspectionResult.True : InspectionResult.False;
            }
            else
            {
                workParams.InspectionPositions[index].IsResult = InspectionResult.False;
            }

            src.Dispose();
            src = null;

            //if (_isLog)
            //{
            //    for (i = 0; i < workParams.Blobs.Count; ++i)
            //    {
            //        _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("Blob{0} 중심(X:{1}, Y:{2})", i, (int)workParams.Blobs[i].CenterX, (int)workParams.Blobs[i].CenterY));
            //    }
            //}

            return (workParams.InspectionPositions[index].IsResult > 0) ? true : false;
            */
            return false;
        }

        //unsafe public bool OpticalSpotBlobProcess(Bitmap srcImage, List<Blob> blob, int binaryTheshold, int blobSizeMinimum, int blobSizeMaximum)
        //{
        //    Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(srcImage);
        //    Mat dst = new Mat();
        //    Mat labels = new Mat(), stats = new Mat(), centeroids = new Mat();

        //    int i;

        //    string sPath = Environment.CurrentDirectory;

        //    //dst = src.Blur(new OpenCvSharp.Size(3, 3));

        //    Cv2.Threshold(src, dst, binaryTheshold, 255, ThresholdTypes.Binary);

        //    OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst).Save(@"D:\Work\\Visual_C++\\2024\\atPhotoInspection\\ImageprocessDebug\\binary.bmp", ImageFormat.Bmp);
            
        //    int labelCount = Cv2.ConnectedComponentsWithStats(dst, labels, stats, centeroids, PixelConnectivity.Connectivity8);
        //    int label = 0;

        //    float fCenterx = srcImage.Width / 2;
        //    float fCentery = srcImage.Height / 2;
        //    float fMin = float.MaxValue;

        //    Mat hist = new Mat();
        //    int[] hdims = { 256 };
        //    Rangef[] ranges = { new Rangef(0, 256), };
        //    Cv2.CalcHist(new Mat[] { dst }, new int[] { 0 }, null, hist, 1, hdims, ranges);

        //    for (i = 1; i < labelCount; ++i)
        //    {
        //        int size = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

        //        if (size >= blobSizeMinimum && size <= (blobSizeMaximum * blobSizeMinimum))
        //        {
        //            int left, top, width, height;

        //            left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
        //            top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
        //            width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
        //            height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                    
        //            PointF ptCenter = new PointF((float)centeroids.At<double>(i, 0), (float)centeroids.At<double>(i, 1));

        //            blob.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y));
        //        }
        //    }

        //    if (_isLog)
        //        _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("CCL 완료(레이블 수:{0}, Blob 수:{1})", labelCount, blob.Count));

        //    if(blob.Count == 0)
        //    {
        //        src.Dispose();
        //        dst.Dispose();
        //        labels.Dispose();
        //        stats.Dispose();
        //        centeroids.Dispose();

        //        return false;
        //    }           

        //    return true;
        //}
        unsafe public bool OpticalSpotBlobProcess(Bitmap srcImage, List<Blob> blob, WorkParams workParam, int binaryTheshold_H, int binaryTheshold_V, int blobSizeMinimum, int blobSizeMaximum, ref double[] dHist_W, ref double[] dHist_H)
        {
            Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(srcImage);
            Mat dst = new Mat();
            Mat labels = new Mat(), stats = new Mat(), centeroids = new Mat();
            Mat ledSearch = new Mat();
            int i;
            
            string sPath = Environment.CurrentDirectory;
            
            //dst = src.Blur(new OpenCvSharp.Size(3, 3));

            //Cv2.Threshold(src, dst, binaryTheshold_H, 255, ThresholdTypes.Binary);

            //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst).Save(@"D:\Work\Software\PhotoInpsection\atPhotoInspection\ImageprocessDebug\binary.bmp", ImageFormat.Bmp);
            OpenCvSharp.Point originLoc1 = new OpenCvSharp.Point(0, 0), originLoc2 = new OpenCvSharp.Point(0, 0);
            float fleft = 0, fright = 0, ftop = 0, fbottom = 0;

            if ((workParam.AreaEnd.X == 0) && (workParam.AreaEnd.Y == 0))
                workParam.AreaEnd = new PointF((float)srcImage.Width, (float)srcImage.Height);
            else
            {
                if (workParam.AreaEnd.X == 0)
                {
                    workParam.AreaEnd = new PointF((float)srcImage.Width, workParam.AreaEnd.Y);
                }
                else if (workParam.AreaEnd.Y == 0)
                {
                    workParam.AreaEnd = new PointF(workParam.AreaEnd.X, (float)srcImage.Height);
                }
            }
                

            originLoc1 = new OpenCvSharp.Point(workParam.AreaStart.X, workParam.AreaStart.Y);
            originLoc2 = new OpenCvSharp.Point(workParam.AreaEnd.X, workParam.AreaEnd.Y);

            fleft = (originLoc1.X > originLoc2.X) ? originLoc2.X / 16f : originLoc1.X / 1f;
            fright = (originLoc1.X > originLoc2.X) ? originLoc1.X / 16f : originLoc2.X / 1f;
            ftop = (originLoc1.Y > originLoc2.Y) ? originLoc2.Y / 16f : originLoc1.Y / 1f;
            fbottom = (originLoc1.Y > originLoc2.Y) ? originLoc1.Y / 16f : originLoc2.Y / 1f;

            // ROI 자르기
            OpenCvSharp.Rect rect = new Rect(Convert.ToInt32(fleft), Convert.ToInt32(ftop), Convert.ToInt32(Math.Abs(fright - fleft)), Convert.ToInt32(Math.Abs(fbottom - ftop)));
            ledSearch = src.SubMat(rect);
            Cv2.Threshold(ledSearch, dst, binaryTheshold_H, 255, ThresholdTypes.Binary);
            int labelCount = Cv2.ConnectedComponentsWithStats(dst, labels, stats, centeroids, PixelConnectivity.Connectivity8);
            int label = 0;

            float fCenterx = srcImage.Width / 2;
            float fCentery = srcImage.Height / 2;
            float fMin = float.MaxValue;

            double[] tempimagew = new double[srcImage.Width];
            double[] tempimageh = new double[srcImage.Height];            
            
            List<Blob> mLedBlobs_H = new List<Blob>();
            mLedBlobs_H.Clear();

            List<Blob> mLedBlobs_V = new List<Blob>();
            mLedBlobs_V.Clear();
            //Cv2.ImShow("threshold Image", ledSearch);
            for (i = 1; i < labelCount; ++i)
            {
                int size = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                if (size >= (blobSizeMinimum * blobSizeMinimum) && size <= (blobSizeMaximum * blobSizeMaximum))
                {
                    int left, top, width, height, peakindexx, peakindexy, peak;

                    left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left) + rect.Left;
                    top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top) + rect.Top;
                    width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                    height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                    Rect inspetarea = new Rect(left, top, width, height);

                    //PointF ptCenter = new PointF((float)centeroids.At<double>(i, 0) + rect.Left, (float)centeroids.At<double>(i, 1) + rect.Top);
                    PointF ptCenter = new PointF((float)(left+(width / 2f)) + rect.Left, (float)(top+(height / 2f)) + rect.Top);

                    GetPixelHistogram(srcImage, inspetarea, ref tempimagew, ref tempimageh, ref PeakIndex_X, ref PeakIndex_Y);

                    peakindexx = PeakIndex_X;
                    peakindexy = PeakIndex_Y;
                    
                    //peak = (int)((refimgh[peakindexy] + refimgw[peakindexx]) / 2);
                    peak = (int)(tempimagew[peakindexx]);
                    if ((width >= (int)(blobSizeMinimum * 0.5)) && (height >= (int)(blobSizeMinimum * 0.5)))
                    {
                        //dimagew = tempimagew;
                        tempimagew.CopyTo(dHist_W, 0);
                        //Buffer.BlockCopy(tempimagew, 0, dimagew, 0, tempimagew.Length);                        
                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y));
                        mLedBlobs_H.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));
                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));                        
                    }
                }
            }            
            ledSearch = src.SubMat(rect);
            Cv2.Threshold(ledSearch, dst, binaryTheshold_V, 255, ThresholdTypes.Binary);
            labelCount = Cv2.ConnectedComponentsWithStats(dst, labels, stats, centeroids, PixelConnectivity.Connectivity8);
            label = 0;            
            for (i = 1; i < labelCount; ++i)
            {
                int size = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                if (size >= (blobSizeMinimum * blobSizeMinimum) && size <= (blobSizeMaximum * blobSizeMaximum))
                {
                    int left, top, width, height, peakindexx, peakindexy, peak;

                    left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left) + rect.Left;
                    top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top) + rect.Top;
                    width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                    height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);

                    Rect inspetarea = new Rect(left, top, width, height);

                    //PointF ptCenter = new PointF((float)centeroids.At<double>(i, 0) + rect.Left, (float)centeroids.At<double>(i, 1) + rect.Top);
                    PointF ptCenter = new PointF((float)(left + (width / 2f)) + rect.Left, (float)(top + (height / 2f)) + rect.Top);

                    GetPixelHistogram(srcImage, inspetarea, ref tempimagew, ref tempimageh, ref PeakIndex_X, ref PeakIndex_Y);

                    peakindexx = PeakIndex_X;
                    peakindexy = PeakIndex_Y;                    
                    peak = (int)(tempimageh[peakindexy]);
                    
                    if ((width >= (int)(blobSizeMinimum * 0.5)) && (height >= (int)(blobSizeMinimum * 0.5)))
                    {
                        tempimageh.CopyTo(dHist_H, 0);
                       // Buffer.BlockCopy(tempimageh, 0, dimageh, 0, tempimageh.Length);
                        //mLedBlobs.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y));
                        mLedBlobs_V.Add(new Blob(label++, left, top, width, height, size, ptCenter.X, ptCenter.Y, peakindexx, peakindexy, peak));
                    }
                }
            }

            if (mLedBlobs_H.Count == mLedBlobs_V.Count)
            {
                label = 0;
                int imgpeak = 0;
                for (i = 0; i < mLedBlobs_H.Count; ++i)
                {
                    imgpeak = (mLedBlobs_H[i].PixelPeak + mLedBlobs_V[i].PixelPeak) / 2;
                    blob.Add(new Blob(label++, mLedBlobs_H[i].Left, mLedBlobs_V[i].Top, mLedBlobs_H[i].Width, mLedBlobs_V[i].Height, mLedBlobs_H[i].Area, mLedBlobs_H[i].CenterX, mLedBlobs_V[i].CenterY, mLedBlobs_H[i].PixelPeakXIndex, mLedBlobs_V[i].PixelPeakYIndex, imgpeak));
                }
            }
            if (_isLog)
                _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("CCL 완료(레이블 수:{0}, Blob 수:{1})", labelCount, blob.Count));

            if (blob.Count == 0)
            {
                src.Dispose();
                dst.Dispose();
                labels.Dispose();
                stats.Dispose();
                centeroids.Dispose();
                //Array.Clear(dHist_W, 0, dHist_W.Length);
                //Array.Clear(dHist_H, 0, dHist_H.Length);
                tempimageh = null;
                tempimagew = null;
                dHist_H = null;
                dHist_W = null;
                return false;
            }
            return true;
        }
        public bool GetPixelHistogram(Bitmap srcImage, Rect _blob, ref double[] dHist_W, ref double[] dHist_H,ref int indexx, ref int indexy)
        {
            int img_w = srcImage.Width;
            int img_h = srcImage.Height;
            
            int byteperpixel = (System.Drawing.Bitmap.GetPixelFormatSize(srcImage.PixelFormat) / 8);            

            BitmapData bitmapData = srcImage.LockBits(new Rectangle(0, 0, img_w, img_h),
                                                     ImageLockMode.ReadOnly,
                                                     PixelFormat.Format8bppIndexed);

            // 픽셀 데이터 복사
            //int widthinbytes = byteperpixel * srcImage.Width;
            int widthinbytes = byteperpixel * bitmapData.Stride;
            int bytes = Math.Abs(bitmapData.Stride) * img_h;
            byte[] pixelData = new byte[bytes];
            Marshal.Copy(bitmapData.Scan0, pixelData, 0, bytes);

            // 이미지 데이터 잠금 해제
            srcImage.UnlockBits(bitmapData);

            int pixelIndex = 0;

            int i, j;
            long[] sum_h_by_w = new long[srcImage.Width];
            long[] sum_w_by_h = new long[srcImage.Height];
            long avg_h_by_w = 0, avg_w_by_h = 0;
            int x_index = 0, y_index = 0;

            long hist_w_max = 0, hist_h_max = 0;
            int hist_w_max_pos = 0, hist_h_max_pos = 0;
            int pixel_h_peak = 0, pixel_w_peak = 0;

            double dHist;
            double scale = 0;

            if ((_blob.Width == 0) && (_blob.Left == 0))
            {
                _blob.Width = img_w;
            }
            if ((_blob.Height == 0) && (_blob.Top == 0))
            {
                _blob.Height = img_h;
            }

            for (i = 0; i < img_w; i++)
            {
                sum_h_by_w[i] = 0;
                x_index = i * byteperpixel;
                if ((i >= _blob.Left) && (i <= (_blob.Left + _blob.Width)))
                {
                    for (j = 0; j < img_h; j++)
                    {
                        sum_h_by_w[i] += pixelData[x_index + (j * widthinbytes)];
                    }
                    avg_w_by_h = sum_h_by_w[i] / img_h;
                }
                else
                    avg_w_by_h = 0;

                if (hist_w_max < avg_w_by_h)
                {
                    hist_w_max = avg_w_by_h;
                    hist_w_max_pos = i;
                    pixel_w_peak = i;
                    indexx = i;
                }
                dHist_W[i] = avg_w_by_h;
            }
            //scale = (double)200 / (double)hist_w_max;
            //for (i = 0; i < img_w; i++)
            //{
            //    dHist_W[i] = (double)((sum_h_by_w[i] / img_h) * scale);
            //}
            for (i = 0; i < img_h; i++)
            {
                sum_w_by_h[i] = 0;
                y_index = widthinbytes * i;
                if ((i >= _blob.Top) && (i <= (_blob.Top + _blob.Height)))
                {
                    for (j = 0; j < img_w; j++)
                    {
                        sum_w_by_h[i] += pixelData[y_index + (j * byteperpixel)];
                    }
                    avg_h_by_w = sum_w_by_h[i] / img_w;
                }
                else
                    avg_h_by_w = 0;

                if (hist_h_max < avg_h_by_w)
                {
                    hist_h_max = avg_h_by_w;
                    hist_h_max_pos = i;
                    pixel_h_peak = i;
                    indexy = i;
                }
                dHist_H[i] = avg_h_by_w;
            }
            //scale = (double)200 / (double)hist_h_max;
            //for (i = 0; i < img_h; i++)
            //{
            //    dHist_H[i] = (double)((sum_w_by_h[i] / img_w) * scale);
            //}
            
            return true;
        }
        public static bool GetPixelHistogramStep(Bitmap srcImage, Rect _blob, ref double[] dHist_W, ref double[] dHist_H, ref int indexx, ref int indexy)
        {
            int img_w = srcImage.Width;
            int img_h = srcImage.Height;
            int byteperpixel = (System.Drawing.Bitmap.GetPixelFormatSize(srcImage.PixelFormat) / 8);            

            BitmapData bitmapData = srcImage.LockBits(new Rectangle(0, 0, img_w, img_h),
                                                     ImageLockMode.ReadOnly,
                                                     PixelFormat.Format8bppIndexed);

            // 픽셀 데이터 복사
            int widthinbytes = byteperpixel * bitmapData.Stride;
            int bytes = Math.Abs(bitmapData.Stride) * img_h;
            byte[] pixelData = new byte[bytes];
            Marshal.Copy(bitmapData.Scan0, pixelData, 0, bytes);

            // 이미지 데이터 잠금 해제
            srcImage.UnlockBits(bitmapData);

            int pixelIndex = 0;

            int i, j;
            long[] sum_h_by_w = new long[srcImage.Width];
            long[] sum_w_by_h = new long[srcImage.Height];
            long avg_h_by_w = 0, avg_w_by_h = 0;
            int x_index = 0, y_index = 0;

            long hist_w_max = 0, hist_h_max = 0;
            int hist_w_max_pos = 0, hist_h_max_pos = 0;
            int pixel_h_peak = 0, pixel_w_peak = 0;

            double dHist;
            double scale = 0;

            if ((_blob.Width == 0) && (_blob.Left == 0))
            {
                _blob.Width = img_w;
            }
            if ((_blob.Height == 0) && (_blob.Top == 0))
            {
                _blob.Height = img_h;
            }

            for (i = 0; i < img_w; i++)
            {
                sum_h_by_w[i] = 0;
                x_index = i * byteperpixel;
                if ((i >= _blob.Left) && (i <= (_blob.Left + _blob.Width)))
                {
                    for (j = 0; j < img_h; j++)
                    {
                        sum_h_by_w[i] += pixelData[x_index + (j * widthinbytes)];
                    }
                    avg_w_by_h = sum_h_by_w[i] / img_h;
                }
                else
                    avg_w_by_h = 0;

                if (hist_w_max < avg_w_by_h)
                {
                    hist_w_max = avg_w_by_h;
                    hist_w_max_pos = i;
                    pixel_w_peak = i;
                    indexx = i;
                }
                dHist_W[i] = avg_w_by_h;
            }
            //scale = (double)200 / (double)hist_w_max;
            //for (i = 0; i < img_w; i++)
            //{
            //    dHist_W[i] = (double)((sum_h_by_w[i] / img_h) * scale);
            //}
            for (i = 0; i < img_h; i++)
            {
                sum_w_by_h[i] = 0;
                y_index = widthinbytes * i;
                if ((i >= _blob.Top) && (i <= (_blob.Top + _blob.Height)))
                {
                    for (j = 0; j < img_w; j++)
                    {
                        sum_w_by_h[i] += pixelData[y_index + (j * byteperpixel)];
                    }
                    avg_h_by_w = sum_w_by_h[i] / img_w;
                }
                else
                    avg_h_by_w = 0;

                if (hist_h_max < avg_h_by_w)
                {
                    hist_h_max = avg_h_by_w;
                    hist_h_max_pos = i;
                    pixel_h_peak = i;
                    indexy = i;
                }
                dHist_H[i] = avg_h_by_w;
            }
            //scale = (double)200 / (double)hist_h_max;
            //for (i = 0; i < img_h; i++)
            //{
            //    dHist_H[i] = (double)((sum_w_by_h[i] / img_w) * scale);
            //}

            return true;
        }
    }
}
