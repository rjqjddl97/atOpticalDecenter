using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using atOpticalDecenter.Functions.StepHandler.Base;
using atOpticalDecenter.Functions.Time;
using ImageLibrary;
using PhotoProduct;
using RecipeManager;
using AiCControlLibrary;
using ARMLibrary;
using LogLibrary;

namespace atOpticalDecenter.Functions.StepHandler.Base
{
    public class StepHandlerBase
    {
        #region Define Delay Time
        public const int D_PRODUCT_BOOT_TIME = 500;
        public const int D_MARGIN = 100;
        
        //public const int D_MB_RESPONSE_TIMEOUT = (D_MD_COMMUNICATE_TIME * 10);
        public const int D_GET_INTO_OUTPUT_TRANSFER_STEP = 1200;
        public const int D_GET_INTO_STEP = 100;
        public const int D_GET_INTO_FRONTLED_STEP = 500;
        public const int D_SET_VALUE = 200;
        public const int D_SET_PWM_VALUE = 500;
        public const int D_GET_INTO_BCD_STEP = 1000;
        public const int D_GET_INTO_1_OPTION_CHECK_TIMEOUT = 900;
        public const int D_GET_INTO_4_OPTION_CHECK_TIMEOUT = D_GET_INTO_1_OPTION_CHECK_TIMEOUT * 4;

        public const int D_FG_WAIT_SETTING = 1500 + D_MARGIN;
        public const int D_PLC_RELAY_CONTROL = 2300 + D_MARGIN;
        public const int D_PLC_RELAY_OFF_CONTROL = 1000 + D_MARGIN;
        public const int D_PLC_BUTTON_CONTROL = 3000;
        public const int D_PLC_WAIT_INIT_INSPECTION = 2000 + D_MARGIN;
        public const int D_PLC_MOTION_COMMAND_TIMEOUT = 60000;
        public const int D_MOTION_COMMAND_RESPONSE_WAIT_TIME = 500;
        public const int D_PLC_MOTION_READYSIGNAL_WAIT_TIME = 500;
        public const int D_MICRO_MOTION_VELOCITY_LIMIT = 50;

        public const int D_PERIPHERAL_SETTING_TIME = 2500;
        public const int D_WAIT_PUT_POWERSOURCE_STABLE_STATUS = 1800;
        public const int D_SHORT_WAIT_PUT_POWERSOURCE_STABLE_STATUS = 1300;
        public const int D_WAIT_CUT_POWERSOURCE_STABLE_STATUS = 500;
        public const int D_ZERO_SPAN_SETTING_TIME = 7000;

        public const int D_WAIT_CAMERA_GRAB_DELAY = 3000;

        public const int PLC_OUTPUT_SIGNAL_WAIT_TIME = 1000;        
               
        public const int REVERSE_VOLTAGE_WAIT_TIME = 2000;        
        public const int SENSOR_POWER_STABLE_TIME = 500;
        public const int SENSOR_OVERCURRENT_STABLE_TIME = 1000;
        public const int SENSOR_RESPONSE_WAIT_TIMEOUT = 500;

        public const int LIGHT_ON_LOAD_VOLTAGE = 2;
        public const int DARK_ON_LOAD_VOLTAGE = 24;
        public const int LIGHT_ON_LOAD_CURRENT = 50;
        public const int DARK_ON_LOAD_CURRENT = 1;
        public const double LOAD_VOLTAGE_MARGIN = 2;
        public const double LOAD_CURRENT_MARGIN = 2;
        public const double SHORT_DISTANCE_INSPECTION_REFERENCE = 500.0;
        public const double SHORT_DISTANCE_MOVE_CAMERA_OFFSET = 150.0;
        public const double ND_FILTER_MINMAX_READY_OFFSET = 30;

        #endregion Define Delay Time

        public static AiCControlLibrary.SerialCommunication.Control.CommunicationManager mMotionDrvCtrl { get; private set; } = null;
        public static ARMLibrary.SerialCommunication.Control.CommunicationManager mRemoteIOCtrl { get; private set; } = null;
        public static Log _log = new Log();
        //public static AiCControlLibrary.SerialCommunication.Data

        //public static CodesysCommunication.Control.CommunicationManager mCodesysPLC { get; private set; } = null;
        //public static CodesysCommunication.Data.UserCodesysData mPLCData { get; private set; } = null;
        //public static PhotoSensorCtrlLibrary.SerialCommunication.Control.CommunicationManager mInspectSensor { get; private set; } = null;
        //public static PhotoSensorCtrlLibrary.SerialCommunication.Data.PhotoInsideData mPhotoData { get; private set; } = null;
        //public static MT4WContorlLibrary.SerialCommunication.Control.CommunicationManager mPanelMeta { get; private set; } = null;
        //public static MT4WContorlLibrary.SerialCommunication.Data.MT4xPanelMeta mPanelData { get; private set; } = null;
        public static WorkParams mWorkParam { get; private set; } = null;
        public static string mJobInfo = string.Empty;
        public static string mProductName = string.Empty;
        public static PhotoProduct.Enums.ProductSeries mProductSeries { get; private set; } = PhotoProduct.Enums.ProductSeries.BTS;
        public static PhotoProduct.Enums.ProductType mProductType { get; private set; } = PhotoProduct.Enums.ProductType.Mirror;
        public static PhotoProduct.Enums.OperatingMode mOpMode { get; private set; } = PhotoProduct.Enums.OperatingMode.LightOn;
        public static PhotoProduct.Enums.OutputType mOutputType { get; private set; } = PhotoProduct.Enums.OutputType.NPN;
        public static PhotoProduct.Enums.DetectMeterial mDetectMertial { get; private set; } = PhotoProduct.Enums.DetectMeterial.Mirror;
        public static double fOnePixelResolution { get; set; } = 0.0344827;
        public static float fMoveVelocity { get; set; } = 100f;        
        public static float fMoveAcceleration { get; set; } = 1000f;
        public static bool bCalibrationActive { get; set; } = false;
        public static RecipeManager.CalibrationParams mCalibrationParam = new RecipeManager.CalibrationParams();
        public static RecipeManager.RobotInformation mRobotInformation { get; set; } = null;
        public static RecipeManager.SystemParams mSystemParam { get; set; } = null;

        public static List<InspectionPosition> InspectPos = new List<InspectionPosition>();
        public static InspectResultData mInspectResultData = null;
        
        private const int NumberOfCapacity = 1;
        private static Image mSourceImage = null;

        private static Queue<Stopwatch> mStopWatchForFullSequenceQueue = new Queue<Stopwatch>();
        private static Stopwatch mStopWatchForCorrectionAndInspection = new Stopwatch();
        private static Stopwatch mStopWatchForOptionInspection = new Stopwatch();        

        protected int mRetryCount = 0;
        protected int RETRY_LIMIT = 3;
        protected int AlarmNumber = 0;
        protected byte CorrectionLoopIndex = 1;    //will use for id   this used for highcurrent model
        protected const byte CorrectionLoopIndexMin = 1;   //Last id   this used for highcurrent model
        protected const byte CorrectionLoopIndexMax = 4;   //Last id   this used for highcurrent model

        protected byte OptionInspectionLoopIndex = 5;    //will use for id   this used for highcurrent model
        protected const byte OptionInspectionLoopIndexMin = 5;   //Last id   this used for highcurrent model
        protected const byte OptionInspectionLoopIndexMax = 8;   //Last id   this used for highcurrent model

        protected string ErrorStepString = string.Empty;
        public string StepInformation => ErrorStepString;
        public static bool IsOverCurrentFInishedStep = false;
        public static bool bMaxDistanceInspectEnable = false;
        public static bool bMinDistanceInspectEnable = false;
        public static int _ImageResolution_H = 0;
        public static int _ImageResolution_V = 0;
        public static double CmdFilterAngleIndex = 0;
        public static long _DelayTimerCounter = 0;
        public static long _CameraGrabWaitTime = 0;
        public static string _ImageSavePath = string.Empty;

        public event Action<InspectResultData> ImageDataUpdate;
        public InspectResultData mInspectResultUpdate = new InspectResultData();

        public void SetJobInfo(string info) => mJobInfo = info;
        public void SetProductName(String modelname) => mProductName = modelname;
        public void SetProductSeries(PhotoProduct.Enums.ProductSeries series) => mProductSeries = series;
        public void SetProductType(PhotoProduct.Enums.ProductType type) => mProductType = type;
        public void SetOutputType(PhotoProduct.Enums.OutputType type) => mOutputType = type;
        public void SetOPMode(PhotoProduct.Enums.OperatingMode OpMode) => mOpMode = OpMode;
        public void SetDetectMertrial(PhotoProduct.Enums.DetectMeterial detector) => mDetectMertial = detector;
        public void SetImageSavePath(string path) => _ImageSavePath = path;

        public long GetCorrectionAndInspectionElapseTime => mStopWatchForCorrectionAndInspection.ElapsedMilliseconds;
        public long GetOptionInspectionElapseTime => mStopWatchForOptionInspection.ElapsedMilliseconds;

        public static bool IsGrabbed = false;

        public static Blob mFirstLedSpot = null;
        public static Blob mFinalLedSpot = null;

        public double[] _dHist_H = null;
        public double[] _dHist_V = null;

        protected TimeChecker mTimeChecker = new TimeChecker();
        public void LogInital(Log mlog)
        {
            _log = mlog;
        }
        protected static void TakePicture()
        {
            mSourceImage = null;
            IsGrabbed = false;
            atOpticalDecenter.TakePicture();
        }
        protected bool LedSpotImageProcess(int Index, double posx,double posy, double posz)
        {
            int iMin = 0, iMax = 0,iThreshold_H = 0, iThreshold_V = 0;

            iMin = (int)(mWorkParam._LEDInspectionSpotMinSize / fOnePixelResolution);
            iMax = (int)(mWorkParam._LEDInspectionSpotMaxSize / fOnePixelResolution);

            iThreshold_H = mWorkParam._LEDInspectionReferenceThresholdH;
            iThreshold_V = mWorkParam._LEDInspectionReferenceThresholdV;
            Bitmap tempImage = Utils.Clone<Bitmap>((Bitmap)mSourceImage);
            Bitmap inspectsource = new Bitmap(mSourceImage.Width, mSourceImage.Height);
            
            
            string filepath = global::atOpticalDecenter.Properties.Settings.Default.strImageFolderPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(filepath))
                Directory.CreateDirectory(filepath);
            if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                inspectsource = ConverterToGray(tempImage);
            }
            if (Index == 0)
            {
                mFirstLedSpot = null;
                if (_dHist_H == null)
                {
                    _dHist_H = new double[inspectsource.Width];
                }
                if (_dHist_V == null)
                {
                    _dHist_V = new double[inspectsource.Height];
                }
                mFirstLedSpot = ImageLibrary.OpticalSpot.OpticalSpotDetectProcess(inspectsource, iThreshold_H, iThreshold_V, iMin, iMax, ref _dHist_H, ref _dHist_V);

                if (mFirstLedSpot != null)
                {
                    mInspectResultData.mFirstLedSpot = mFirstLedSpot;
                    mInspectResultData.fOpticalSpotImageBright = (double)mFirstLedSpot.PixelPeak;
                    mInspectResultData.WorkDistance = (double)mWorkParam._LEDInspectionCameraDistance;
                    mInspectResultData.CalculateLedBlob(0);
                    mInspectResultData.fMeasureP1X = posx;
                    mInspectResultData.fMeasureP1Y = posy + ((mFirstLedSpot.CenterX - (_ImageResolution_H / 2)) * fOnePixelResolution);
                    mInspectResultData.fMeasureP1Z = posz + ((mFirstLedSpot.CenterY - (_ImageResolution_V / 2)) * fOnePixelResolution);
                    mInspectResultData.fOpticalCenterPositionX = posx;
                    mInspectResultData.fOpticalCenterPositionY = posy;
                    mInspectResultData.fOpticalCenterPositionZ = posz;
                    mInspectResultData.fMeasureOpticaCenterP1X = 0;
                    mInspectResultData.fMeasureOpticaCenterP1Y = ((mFirstLedSpot.CenterX - (_ImageResolution_H / 2)) * fOnePixelResolution); 
                    mInspectResultData.fMeasureOpticaCenterP1Z = ((mFirstLedSpot.CenterY - (_ImageResolution_V / 2)) * fOnePixelResolution); 
                    Array.Copy(_dHist_H,0,mInspectResultData._ImageHist_H,0,_dHist_H.Length);
                    Array.Copy(_dHist_V,0,mInspectResultData._ImageHist_V,0,_dHist_V.Length);
                    LedSpotBlobUpdate(mFirstLedSpot);
                    if (mSystemParam._saveResultLEDMeasurement)
                    {
                        inspectsource.Save(filepath + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_P1.bmp");
                    }                    
                    return true;
                }
                else
                    return false;

            }
            else
            {
                mFinalLedSpot = null;
                if (_dHist_H == null)
                {
                    _dHist_H = new double[inspectsource.Width];
                }
                if (_dHist_V == null)
                {
                    _dHist_V = new double[inspectsource.Height];
                }
                mFinalLedSpot = ImageLibrary.OpticalSpot.OpticalSpotDetectProcess(inspectsource, (iThreshold_H*1)/1, (iThreshold_V*1)/1, iMin, iMax,ref _dHist_H,ref _dHist_V);

                if (mFinalLedSpot != null)
                {
                    mInspectResultData.mFinalLedSpot = mFinalLedSpot;
                    mInspectResultData.CalculateLedBlob(1);
                    mInspectResultData.fMeasureP2X = posx;
                    mInspectResultData.fMeasureP2Y = posy + ((mFinalLedSpot.CenterX - (_ImageResolution_H / 2)) * fOnePixelResolution);
                    mInspectResultData.fMeasureP2Z = posz + ((mFinalLedSpot.CenterY - (_ImageResolution_V / 2)) * fOnePixelResolution);
                    mInspectResultData.fMeasureOpticaCenterP2X = 0;
                    mInspectResultData.fMeasureOpticaCenterP2Y = ((mFinalLedSpot.CenterX - (_ImageResolution_H / 2)) * fOnePixelResolution);
                    mInspectResultData.fMeasureOpticaCenterP2Z = ((mFinalLedSpot.CenterY - (_ImageResolution_V / 2)) * fOnePixelResolution);
                    mInspectResultData.CalculateOpticalDecenter();

                    Array.Copy(_dHist_H, 0, mInspectResultData._ImageHist_H, 0, _dHist_H.Length);
                    Array.Copy(_dHist_V, 0, mInspectResultData._ImageHist_V, 0, _dHist_V.Length);
                    LedSpotBlobUpdate(mFinalLedSpot);
                    if (mSystemParam._saveResultLEDMeasurement)
                    {
                        inspectsource.Save(filepath + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_P2.bmp");
                    }                    
                    return true;
                }
                else
                    return false;
            }            
        }
        protected static void LedSpotBlobUpdate(Blob ledspot)
        {
            mInspectResultData.mLedSpot = ledspot;
            atOpticalDecenter.UpdateEventLedBlobStart(mInspectResultData);
        }
        protected void LedSpotCalcuate()
        {
            mInspectResultData.CalculateOpticalInspect(mWorkParam._ProductType);

            if (mWorkParam._ProductSeries == 0)
            {
                // BTS Series, Mirror Type
                if (mWorkParam._ProductType == 0)
                {
                    mInspectResultData.fOpticalEccentricAngle = mInspectResultData.fOpticalEccentricAngle * (-1);
                }
                if ((mInspectResultData.fOpticalEccentricAngle_H >= mWorkParam._LEDInspectionDivergenceHMinAngle) && (mInspectResultData.fOpticalEccentricAngle_H <= mWorkParam._LEDInspectionDivergenceHMaxAngle)
                    && (mInspectResultData.fOpticalEccentricAngle_V >= mWorkParam._LEDInspectionDivergenceVMinAngle) && (mInspectResultData.fOpticalEccentricAngle_V <= mWorkParam._LEDInspectionDivergenceVMaxAngle)
                    )
                    mInspectResultData.bTotalResult = true;
                else
                    mInspectResultData.bTotalResult = false;
            }
            else
            {
                if ((mInspectResultData.fOpticalEccentricAngle >= mWorkParam._LEDInspectionDivergenceHMinAngle) && (mInspectResultData.fOpticalEccentricAngle <= mWorkParam._LEDInspectionDivergenceHMaxAngle)
                    //&& (mInspectResultData.fOpticalEccentricAngle >= mWorkParam._LEDInspectionDivergenceVMinAngle) && (mInspectResultData.fOpticalEccentricAngle <= mWorkParam._LEDInspectionDivergenceVMaxAngle)
                    )
                    mInspectResultData.bTotalResult = true;
                else
                    mInspectResultData.bTotalResult = false;
            }
            //if ((mInspectResultData.fOpticalEmiterAngle >= -(mWorkParam._LEDInspectionDivergenceAngle*(Math.PI/180))) && (mInspectResultData.fOpticalEmiterAngle <= (mWorkParam._LEDInspectionDivergenceAngle * (Math.PI / 180))))

        }
        public InspectResultData UpdateInspectdData()
        {
            mInspectResultUpdate =  mInspectResultData;            
            return mInspectResultUpdate;
        }
        public enum WorkingSection
        {
            CorrectionAndInspection,
            OptionInspection
        }

        public enum RetType
        {
            Ready,
            Busy,
            Error
        }

        public StepHandlerBase()
        {
            
        }
        public StepHandlerBase(AiCControlLibrary.SerialCommunication.Control.CommunicationManager aic, ARMLibrary.SerialCommunication.Control.CommunicationManager arm, SystemParams systemparam, WorkParams workparam, InspectResultData ResultData, RobotInformation robotinfo)
        {
            mMotionDrvCtrl = aic;
            mRemoteIOCtrl = arm;
            mWorkParam = workparam;
            mSystemParam = systemparam;
            mRobotInformation = robotinfo;
            mFirstLedSpot = new Blob();
            mFinalLedSpot = new Blob();
            mInspectResultData = ResultData;
            bCalibrationActive = systemparam._calibrationParams._CoordinateCalibrationActive;
            _ImageResolution_H = systemparam._cameraParams.HResolution;
            _ImageResolution_V = systemparam._cameraParams.VResolution;
            fOnePixelResolution = systemparam._cameraParams.OnePixelResolution;
            _DelayTimerCounter = D_MOTION_COMMAND_RESPONSE_WAIT_TIME;
            _CameraGrabWaitTime = D_WAIT_CAMERA_GRAB_DELAY;
            _dHist_H = new double[_ImageResolution_H];
            _dHist_V = new double[_ImageResolution_V];
            mInspectResultData.InspectInitialImageBuffer(_ImageResolution_H, _ImageResolution_V);
        }
        public void StepHandlerBaseSystemParamUpdate(SystemParams systemparam)
        {
            mSystemParam = systemparam;
        }
        protected bool IsEssentialInstanceSetted
        {
            get
            {
                if ((mMotionDrvCtrl != null) && (mRemoteIOCtrl != null))
                {
                    return true;
                }
                else
                    return false;
                return true;
            }
        }
        public void UpdateRobotInfomation(RecipeManager.RobotInformation InfoData)
        {
            mRobotInformation.PositionX = InfoData.PositionX;
            mRobotInformation.PositionY = InfoData.PositionY;
            mRobotInformation.PositionZ = InfoData.PositionZ;
            mRobotInformation.mStatus = InfoData.mStatus;
            mRobotInformation.mError = InfoData.mError;            

        }
        public void UpdateRobotIOInfomation(RecipeManager.RobotInformation InfoData)
        {
            mRobotInformation.mInputData = InfoData.mInputData;
            mRobotInformation.mOutputData = InfoData.mOutputData;
        }
        public void OnCameraImageGrab(Image image)
        {
            try
            {
                if (image != null)
                {
                    mSourceImage = null;
                    mSourceImage = Utils.Clone(image);
                    IsGrabbed = true;
#if DEBUG
                    Bitmap mBitmap = new Bitmap(image);
                    //mBitmap.Save("D:\\Picture\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Image.bmp");
#endif
                }
            }
            catch (Exception ex)
            {
                mSourceImage = null;
                IsGrabbed = false;
                Console.WriteLine("StepHandlerBase::OnCameraImageGrab => Error Occured : {0}", ex.ToString());
            }
        }

        public void ClearTimeForFullSequence()
        {
            mStopWatchForFullSequenceQueue.Clear();
        }

        public void StartTimeCheckForFullSequence()
        {
            Stopwatch temp = new Stopwatch();
            temp.Restart();
            mStopWatchForFullSequenceQueue.Enqueue(temp);
        }

        public void StopTimeCheckForFullSequence()
        {
            if (mStopWatchForFullSequenceQueue.Count > 0)
            {
                Stopwatch temp = mStopWatchForFullSequenceQueue.Dequeue();
                temp.Stop();
                //for (int i = 0; i < NumberOfCapacity; i++)
                //    mADMSProductResultInfoForOptionInspection[i].ElapsedTime = string.Format("{0:0.0}", (float)(temp.ElapsedMilliseconds * 0.001));
            }
        }

        public void StartTimeCheckForCorrectionAndInspection()
        {
            mStopWatchForCorrectionAndInspection.Restart();
        }

        public void StopTimeCheckForCorrectionAndInspection()
        {
            mStopWatchForCorrectionAndInspection.Stop();
            //for (int i = 0; i < NumberOfCapacity; i++)
            //    mADMSProductResultInfoForCorrection[i].CorrectionAndInspectionElapsedTime = string.Format("{0:0.0}", (float)(mStopWatchForCorrectionAndInspection.ElapsedMilliseconds * 0.001));
        }

        public void StartTimeCheckForOptionInspection()
        {
            mStopWatchForOptionInspection.Restart();
        }

        public void StopTimeCheckForOptionInspection()
        {
            mStopWatchForOptionInspection.Stop();
            //for (int i = 0; i < NumberOfCapacity; i++)
            //    mADMSProductResultInfoForOptionInspection[i].OptionInspectionElapsedTime = string.Format("{0:0.0}", (float)(mStopWatchForOptionInspection.ElapsedMilliseconds * 0.001));
        }
        public Bitmap ConverterToGray(Bitmap colorBitmap)
        {
            int w = colorBitmap.Width,
                h = colorBitmap.Height,
                r, ic, oc, bmpStride, outputStride, bytesPerPixel;
            PixelFormat pfIn = colorBitmap.PixelFormat;
            BitmapData bmpData, outputData;

            Bitmap outImage = colorBitmap;
            //Create the new bitmap
            outImage = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Build a grayscale color Palette
            System.Drawing.Imaging.ColorPalette cvtpalette = outImage.Palette;
            for (int i = 0; i < 256; i++)
            {
                Color tmp = Color.FromArgb(255, i, i, i);
                cvtpalette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            outImage.Palette = cvtpalette;

            //Get the number of bytes per pixel
            switch (pfIn)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                default: throw new InvalidOperationException("Image format not supported");
            }

            //Lock the images
            bmpData = colorBitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                    pfIn);
            outputData = outImage.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            bmpStride = bmpData.Stride;
            outputStride = outputData.Stride;

            //Traverse each pixel of the image
            unsafe
            {
                byte* bmpPtr = (byte*)bmpData.Scan0.ToPointer(),
                outputPtr = (byte*)outputData.Scan0.ToPointer();

                if (bytesPerPixel == 3)
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = .299*R + .587*G + .114*B
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 3, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                (0.299f * bmpPtr[r * bmpStride + ic] +
                                    0.587f * bmpPtr[r * bmpStride + ic + 1] +
                                    0.114f * bmpPtr[r * bmpStride + ic + 2]);
                }
                else //bytesPerPixel == 4
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = alpha * (.299*R + .587*G + .114*B)
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 4, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                ((bmpPtr[r * bmpStride + ic + 3] / 255.0f) *
                                (0.299f * bmpPtr[r * bmpStride + ic] +
                                    0.587f * bmpPtr[r * bmpStride + ic + 1] +
                                    0.114f * bmpPtr[r * bmpStride + ic + 2]));
                }
            }

            //Unlock the images
            colorBitmap.UnlockBits(bmpData);
            outImage.UnlockBits(outputData);
            return outImage;
        }
    }
}
