using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using RecipeManager;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using CustomPages;
using Basler;
using LogLibrary;
using ImageLibrary;
using PhotoProduct;
using atOpticalDecenter;
using atOpticalDecenter.Functions.StepHandler;

namespace atOpticalDecenter
{
    public partial class atOpticalDecenter
    {
        private static event Action TakePictureEvent;
        public event Action<Image> ImageGrabbed;        
        private static event Action<InspectResultData> LEDSpotBlobProcessEvent;
        private static event Action OpticalDistanceAnalysisEvent;
        private Functions.StepHandler.Base.StepHandlerBase mStepBase = null;
        private List<Functions.StepHandler.Base.IStepHandler> mPhotoInspectionList = new List<Functions.StepHandler.Base.IStepHandler>();
        private InspectionStepType mInspectStep = InspectionStepType.Idle;
        bool _InspectionWorking = false;
        bool _HommingProcess = false;
        bool _InspectionResult = false;
        public InspectResultData mResultData = new InspectResultData();
        public RobotInformation mRobotInformation = new RobotInformation();
        
        private enum InspectionStepType
        {
            Idle = 0,
            CheckWaitRobotReady,
            ExcuteInspection,
            CheckInspection,
            FinishedInspection,
            ErrorOccurred
        }
        public static void TakePicture()
        {
            TakePictureEvent?.Invoke();
        }

        private void GrabPicture()
        {
            try
            {
                if (_Camera.IsAllocated)
                {
                    _Camera.OneShot(_waitHandle);
                    _isOpticalMeasurement = false;                                        
                    barButtonItemFitSize.PerformClick();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "자동 검사 중 싱글 샷");
                }
            }
            catch (Exception)
            {
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "자동 검사 중 싱글 샷 오류");
            }
        }
        public void UpdatePhotoInspectedData(InspectResultData InspectedData)
        {
            mResultData = InspectedData;
        }
        public static void UpdateEventLedBlobStart(InspectResultData InspectedData)
        {
            LEDSpotBlobProcessEvent?.Invoke(InspectedData);
        }
        public void UpdateImageSpotBlob(InspectResultData InspectedData)
        {
            _blobs.Clear();
            _blobs.Add(InspectedData.mLedSpot);            
            Array.Copy(InspectedData._ImageHist_H, 0, _ImageHist_W, 0, InspectedData._ImageHist_H.Length);
            Array.Copy(InspectedData._ImageHist_V, 0, _ImageHist_H, 0, InspectedData._ImageHist_V.Length);
            _isAutoInspectMeasurement = true;
            barButtonItemFitSize.PerformClick();
            //barCheckItemShowCenterMark.PerformClick();            
            barCheckItemShowCenterMark.Checked = true;
            //pictureEditSystemImage.Refresh();
        }
        public void UpdateRobotInfomation(RobotInformation update)
        {
            mRobotInformation.PositionX = update.PositionX;
            mRobotInformation.PositionY = update.PositionY;
            mRobotInformation.PositionZ = update.PositionZ;
            mRobotInformation.mStatus = update.mStatus;
            mRobotInformation.mError = update.mError;

            if (_IsHommingFinished)
                mRobotInformation.SetStatus(RobotInformation.RobotStatus.OperationReady, _IsHommingFinished);

            if (mStepBase != null)
                mStepBase.UpdateRobotInfomation(mRobotInformation);

            if (mRobotInformation.GetStatus(RobotInformation.RobotStatus.OperationReady))
                _IsHommingFinished = true;
            ImageUpdateEvents?.Invoke();
        }
        public void UpdateRobotIOInfomation(RobotInformation update)
        {
            mRobotInformation.mInputData = update.mInputData;
            mRobotInformation.mOutputData = update.mOutputData;

            if (!mRobotInformation.mInputData.B0)
            {
                if (mRobotInformation.mInputData.B1)
                {
                    if (_IsReciepLoad)
                    {
                        InspectionSequenceStart();
                    }
                }
                if (mRobotInformation.mInputData.B2)
                {
                    if ( (mRobotInformation.mError !=0) || (mRobotInformation.GetStatus(RobotInformation.RobotStatus.EmergencyStop)) )
                    {
                        //InspectionSequenceStop();
                        byte[] SeData = new byte[8];
                        for (int i = 0; i < _mMotionControlCommManager.mDrvCtrl.DeviceIDCount; i++)
                        {
                            SeData = _mMotionControlCommManager.mDrvCtrl.AlarmResetCommand((byte)_mMotionControlCommManager.mDrvCtrl.DrvID[i]);
                            _mMotionControlCommManager.SendData(SeData);
                        }
                    }
                }
            }
            else
            {
                InspectionSequenceStop();
                _IsHommingFinished = false;
            }
            if (mStepBase != null)
                mStepBase.UpdateRobotIOInfomation(mRobotInformation);
        }
        
        private void InitStepBase()
        {
            mResultData.ImageResolution = (double)_systemParams._cameraParams.OnePixelResolution;
            mResultData.fImageSensorSize_H = (double)_systemParams._cameraParams.ImageSensorHSize;
            mResultData.fImageSensorSize_V = (double)_systemParams._cameraParams.ImageSensorVSize;
            mResultData.fLensFocusLength = (double)_systemParams._cameraParams.LensFocusLength;            
            mStepBase = new Functions.StepHandler.Base.StepHandlerBase(_mMotionControlCommManager,_mRemteIOCommManager,_systemParams,_workParams, mResultData,mRobotInformation);            
            mStepBase.SetImageSavePath(global::atOpticalDecenter.Properties.Settings.Default.strImageFolderPath);
            TakePictureEvent += GrabPicture;            
            ImageGrabbed += mStepBase.OnCameraImageGrab;
            LEDSpotBlobProcessEvent += UpdateImageSpotBlob;
        }
        private void StepBaseSystemParameterUpdate()
        {
            if (mStepBase != null)
                mStepBase.StepHandlerBaseSystemParamUpdate(_systemParams);
        }
        private void MakeInspectionList()
        {
            mPhotoInspectionList.Clear();

            if ((_workParams._ProductType == 0) || (_workParams._ProductType == 1) || (_workParams._ProductType == 2) || (_workParams._ProductType == 3) || (_workParams._ProductType == 4) || (_workParams._ProductType == 5))       // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
            {
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step1JigCheck());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step2SensorPowerOn());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step3MovePostion1());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step4Spot1Measure());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step5MovePosition2());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step6Spot2Measure());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step7SensorPowerOff());
                mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.Step8CalculateResult());
            }
        }
        private void backgroundWorkerMotionHome_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RobotInformation info = e.Argument as RobotInformation;
                if (sender is BackgroundWorker worker)
                {
                    if (_mMotionControlCommManager.IsOpen())
                    {
                        if (!_IsHommingFinished)
                        {
                            if (MessageBox.Show("원점복귀를 진행을 합니다.", "원점복귀", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                            {
                                byte[] SeData = new byte[8];
                                for (int i = 0; i < _mMotionControlCommManager.mDrvCtrl.DeviceIDCount; i++)
                                {
                                    SeData = _mMotionControlCommManager.mDrvCtrl.HomeStartCommand((byte)_mMotionControlCommManager.mDrvCtrl.DrvID[i]);
                                    _mMotionControlCommManager.SendData(SeData);
                                }
                                _HommingProcess = true;
                                //mRobotInformation.SetStatus(RobotInformation.RobotStatus.OperationReady, _IsHommingFinished);
                                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("원점 복귀 실행을 시작합니다."));
                            }
                            Thread.Sleep(3000);
                            while (_HommingProcess)              // Inpsotion, Servo On Satus
                            {
                                Thread.Sleep(500);
                                if ((info.mStatus & 0x00000042) == 0x00000042)
                                {
                                    _HommingProcess = false;
                                }
                            }
                        }

                    }
                    else
                        _IsHommingFinished = false;

                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Warn, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
        private void backgroundWorkerMotionHome_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _IsHommingFinished = true;
            mRobotInformation.SetStatus(RobotInformation.RobotStatus.OperationReady, _IsHommingFinished);
            AutoStartButtonRelease();
            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "원점 복귀 실행 종료합니다.");
        }
        private void MakeDryRunList()
        {
            mPhotoInspectionList.Clear();            
            mPhotoInspectionList.Add(new Functions.StepHandler.Inspection.DryRunStep());
        }
        private void backgroundWorkerInspection_DoWork(object sender, DoWorkEventArgs e)
        {
            //MessageBox.Show("제품고정을 확인하세요.");
            try
            {
                if (sender is BackgroundWorker worker)
                {
                    Functions.Time.TimeChecker mTimeChecker = new Functions.Time.TimeChecker();
                    int RunningIndex = 0;
                    bool AlarmTriggedToPLC = false;
                    Functions.StepHandler.Base.StepHandlerBase.RetType mWorkingStatus;
                    while (_InspectionWorking)
                    {
                        TimeSpan ts = CheckTackTime.Elapsed;
                        barStaticItemInspectionTime.Caption = string.Format("검사 시간: 00:{0:00}:{1:00}.{2}", ts.Minutes, ts.Seconds, ts.Milliseconds);
                        if (RunningIndex < mPhotoInspectionList.Count)
                            barStaticItemInspectionStatus.Caption = string.Format("진행: ") + ((Functions.StepHandler.Base.StepHandlerBase)mPhotoInspectionList[RunningIndex]).StepInformation;
                        barEditItemInspectionResult.EditValue = "Running";
                        repositoryItemTextEditInspectionResult.Appearance.ForeColor = System.Drawing.Color.Black;
                        switch (mInspectStep)
                        {
                            case InspectionStepType.Idle:
                                AlarmTriggedToPLC = false;
                                break;
                            case InspectionStepType.CheckWaitRobotReady:
                                // Robot 체크후 준비 상태면 실행 상태로 변환 아니면 대기!! 
                                RunningIndex = 0;
                                mInspectStep = InspectionStepType.ExcuteInspection;

                                break;
                            case InspectionStepType.ExcuteInspection:
                                if (RunningIndex < mPhotoInspectionList.Count)
                                {
                                    if (RunningIndex == 0)
                                    {
                                    }
                                    if (mPhotoInspectionList[RunningIndex].Execute() != Functions.StepHandler.Base.StepHandlerBase.RetType.Error)
                                    {
                                        mInspectStep = InspectionStepType.CheckInspection;
                                    }
                                    else
                                    {
                                        mInspectStep = InspectionStepType.ErrorOccurred;
                                    }
                                }
                                else
                                {
                                    mInspectStep = InspectionStepType.FinishedInspection;
                                }
                                break;
                            case InspectionStepType.CheckInspection:
                                mWorkingStatus = mPhotoInspectionList[RunningIndex].GetStatus();
                                if (mWorkingStatus == Functions.StepHandler.Base.StepHandlerBase.RetType.Busy)
                                {
                                    ;
                                }
                                else if (mWorkingStatus == Functions.StepHandler.Base.StepHandlerBase.RetType.Ready)
                                {
                                    RunningIndex++;
                                    mInspectStep = InspectionStepType.ExcuteInspection;
                                }
                                else
                                {
                                    mInspectStep = InspectionStepType.ErrorOccurred;
                                }
                                break;
                            case InspectionStepType.FinishedInspection:
                                mInspectStep = InspectionStepType.Idle;
                                
                                _InspectionWorking = false;
                                break;
                            case InspectionStepType.ErrorOccurred:
                                _InspectionWorking = false;
                                barEditItemInspectionResult.EditValue = "Error" + ((Functions.StepHandler.Base.StepHandlerBase)mPhotoInspectionList[RunningIndex]).StepInformation;
                                repositoryItemTextEditInspectionResult.Appearance.ForeColor = System.Drawing.Color.Red;
                                _isInspectError = false;
                                //_isAutoInspectMeasurement = false;
                                break;
                            default: break;                                
                        }

                        if (mInspectStep == InspectionStepType.Idle)
                        {
                            _backgroundWorkerOpticalDecenterInspection.ReportProgress(RunningIndex, new WorkingStateInfo()
                            {
                                WorkingStatus = WorkingStateInfo.WorkingType.Checking
                            });
                        }
                        else if (mInspectStep == InspectionStepType.ExcuteInspection)
                        {
                            if (RunningIndex < mPhotoInspectionList.Count)
                            {
                                _backgroundWorkerOpticalDecenterInspection.ReportProgress(RunningIndex, new WorkingStateInfo()
                                {
                                    WorkingStatus = WorkingStateInfo.WorkingType.CorrectionAndInspection,
                                    CurrentStep = RunningIndex,
                                    CurrentStepName = ((Functions.StepHandler.Base.StepHandlerBase)mPhotoInspectionList[RunningIndex]).StepInformation,
                                    LastStep = mPhotoInspectionList.Count,
                                    ElapsedTime = mStepBase.GetOptionInspectionElapseTime
                                });                                                                
                            }
                        }
                        else if (mInspectStep == InspectionStepType.FinishedInspection)
                        {
                            _backgroundWorkerOpticalDecenterInspection.ReportProgress(RunningIndex, new WorkingStateInfo()
                            {
                                WorkingStatus = WorkingStateInfo.WorkingType.CorrectionAndInspection,
                                CurrentStep = mPhotoInspectionList.Count,
                                CurrentStepName = ((Functions.StepHandler.Base.StepHandlerBase)mPhotoInspectionList[mPhotoInspectionList.Count - 1]).StepInformation,
                                LastStep = mPhotoInspectionList.Count,
                                ElapsedTime = mStepBase.GetOptionInspectionElapseTime
                            });                            
                            mResultData = mStepBase.UpdateInspectdData();
                        }
                        else if (mInspectStep == InspectionStepType.ErrorOccurred)
                        {
                            _backgroundWorkerOpticalDecenterInspection.ReportProgress(RunningIndex, new WorkingStateInfo()
                            {
                                WorkingStatus = WorkingStateInfo.WorkingType.Error,
                                CurrentStep = RunningIndex,
                            });
                            _isInspectError = false;
                            string strerr = string.Empty;
                            strerr = "Error : " + ((Functions.StepHandler.Base.StepHandlerBase)mPhotoInspectionList[RunningIndex]).StepInformation;                            
                            mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), strerr);
                            AutoStartButtonRelease();
                            MessageBox.Show(strerr, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            break;
                        }
                        System.Threading.Thread.Sleep(97);
                        //11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
                        //101, 103, 107, 109, 113, 127, 131, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,479
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
        private void backgroundWorkerInspection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //mWorkingStep = CorrectionStepType.Idle;
            _isInspecting = false;
            _InspectionWorking = false;
            //_isAutoInspectMeasurement = false;
            UpdateProcessTime(false);
            barCheckItemInspectionStart.Caption = string.Format("검사 시작");
            barStaticItemInspectionStatus.Caption = string.Format("진행: 검사 완료");
            if (!_isInspectError)
            {
                InpsectResultUpdate();
                CreateResultFile(mResultData.bTotalResult);
                UpdateStaticsData();
            }
            barEditItemInspectionProgress.EditValue = 100;            
            AutoStartButtonRelease();
            System.Console.WriteLine("bacground work Photo Inspection run worker completed");
        }
        private void backgroundWorkerInspection_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                WorkingStateInfo mStateInfo = (WorkingStateInfo)e.UserState;                
                barStaticItemInspectionStatus.Caption = string.Format("진행: ") + mStateInfo.CurrentStepName;
                
                if (mStateInfo.WorkingStatus == WorkingStateInfo.WorkingType.Checking)
                {
                }
                else if (mStateInfo.WorkingStatus == WorkingStateInfo.WorkingType.CorrectionAndInspection)
                {
                    int position = 0;
                    if ((mStateInfo.CurrentStep != 0) && (mStateInfo.LastStep != 0))
                        position = (int)(((double)mStateInfo.CurrentStep / (double)mStateInfo.LastStep) * 100);

                    barEditItemInspectionProgress.EditValue = position;
                }
                else if (mStateInfo.WorkingStatus == WorkingStateInfo.WorkingType.Error)
                {
                    ;
                }
            }
            catch (Exception)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), "작업 상태 갱신 실패.");
            }
        }
        public void InpsectResultUpdate()
        {
            string series = string.Empty;
            if (_workParams._ProductSeries == 0)                     // 0: BTS, 1: BTF, 2: BJ, 3: BEN
            {
                series = "BTS Series";
            }
            else if (_workParams._ProductSeries == 1)
            {
                series = "BTF Series";
            }
            else if (_workParams._ProductSeries == 2)
            {
                series = "BJ Series";
            }
            else if (_workParams._ProductSeries == 3)
            {
                series = "BEN Series";
            }
            else
                series = "BTS Series";
            pledSpotInspectionInfomation._InspectProductSeries = series;
            pledSpotInspectionInfomation._InspectProductModelName = _workParams._ProductModelName;
            pledSpotInspectionInfomation._InspectLedBlobHsize = mResultData.fOpticalSize[0];
            pledSpotInspectionInfomation._InspectLedBlobVsize = mResultData.fOpticalSize[1];
            pledSpotInspectionInfomation._InspectLedBlobBright = mResultData.fOpticalSpotImageBright;
            pledSpotInspectionInfomation._InspectLedOpticalEccentricity = mResultData.fOpticalEccentricity;
            pledSpotInspectionInfomation._InspectLedOpticalEmiterAngle = mResultData.fOpticalEmiterAngle * (180 / Math.PI);
            pledSpotInspectionInfomation._InspectLedOpticalEccentricAngle = mResultData.fOpticalEccentricAngle;
            pledSpotInspectionInfomation._InspectLedODFilterReduce = mResultData.fODFilterReduce;
            pledSpotInspectionInfomation._InspectLedND_FilterAngle = mResultData.fND_FilterAngle;
            pledSpotInspectionInfomation._InspectOperateMax_Distance = mResultData.fMaxOperateDistance;
            pledSpotInspectionInfomation._InspectOperateMin_Distance = mResultData.fMinOperateDistance;
            pledSpotInspectionInfomation._InspectOpticalResult = mResultData.bTotalResult;

            xtraTabControlMainSetup.Invoke(new MethodInvoker(delegate { xtraTabControlMainSetup.SelectedTabPageIndex = 4; }));
            _InspectionResult = true;
            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("투광 LED 특성 검사 결과 , Spot1 Size :{0:000.000}mm, Spot2 Size :{1:000.000}mm, " +
                "이미지 밝기 :{2:000}pixel, 광원 편심 :{3:00.000}mm, 광원 편심각 :{4:00.000}˚, 광 발산각 :{5:00.000}˚, 감쇄율 :{6:00.000}, ND필터 예측각도 :{7:000}˚ , 최대거리 ND필터 :{8:000}˚, 검사 결과 : {9}",
                mResultData.fOpticalSize[0], mResultData.fOpticalSize[1], mResultData.fOpticalSpotImageBright, mResultData.fOpticalEccentricity, mResultData.fOpticalEccentricAngle, mResultData.fOpticalEccentricity, (mResultData.fOpticalEmiterAngle * (180 / Math.PI)),
                mResultData.fODFilterReduce, mResultData.fND_FilterAngle, mResultData.fMaxOperateDistance, (mResultData.bTotalResult ? "Pass" : "Fail")));
        }
        public void CreateResultFile(bool IsPass)
        {
            try
            {
                // Report.csv
                if (_systemParams._saveResultStatistics)
                {
                    _inspectionStartTime = DateTime.Now;
                    string strFilePath = string.Format(@"{0}\{1:0000}{2:00}{3:00}",
                        SystemDirectoryParams.ResultFolderPath,
                        _inspectionStartTime.Year, _inspectionStartTime.Month, _inspectionStartTime.Day);

                    if (IsPass)
                    {
                        //strFilePath += string.Format(@"\Pass\{0}_{1:00}{2:00}{3:00}", _workParams.RecipeName, _inspectionStartTime.Hour, _inspectionStartTime.Minute, _inspectionStartTime.Second);
                        strFilePath += string.Format(@"\Pass\{0}", _workParams.RecipeName);
                    }
                    else
                    {
                        //strFilePath += string.Format(@"\Fail\{0}_{1:00}{2:00}{3:00}", _workParams.RecipeName, _inspectionStartTime.Hour, _inspectionStartTime.Minute, _inspectionStartTime.Second);
                        strFilePath += string.Format(@"\Fail\{0}", _workParams.RecipeName);
                    }
                    string strResultFile = strFilePath + @"\Result.csv";
                    string strTemp = string.Empty;

                    if (!Directory.Exists(strFilePath))
                    {
                        Directory.CreateDirectory(strFilePath);
                        strTemp = string.Format("WorkTime, RecipeName,Product Model, Operating Distance,Camera ExposureTime(us),Spot1 Size(mm),Spot2 Size(mm),Image Bright(pixel),Eccentricity Distane(mm),Eccentric Angle,Divergence Angle,Reduction rate, ND Filter Angle, Min Distance ND Angle, Max Distance ND Angle, Inspect Result");
                        using (StreamWriter sw = new StreamWriter(strResultFile, true))
                        {
                            sw.WriteLine(strTemp);
                        }
                    }
                    //string strResultFile = strFilePath + @"\Result.csv";

                    using (StreamWriter sw = new StreamWriter(strResultFile, true))
                    {
                        strTemp = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                            _inspectionStartTime.TimeOfDay.ToString(),
                            _workParams.RecipeName,
                            _workParams._ProductModelName,
                            Convert.ToString(_workParams._ProductDistance),
                            _workParams._LEDInspectionExposureTime,
                            mResultData.fOpticalSize[0],
                            mResultData.fOpticalSize[1],
                            mResultData.fOpticalSpotImageBright,
                            mResultData.fOpticalEccentricity,                            
                            mResultData.fOpticalEccentricAngle,
                            mResultData.fOpticalEmiterAngle * (180 / Math.PI),
                            mResultData.fODFilterReduce,
                            mResultData.fND_FilterAngle,
                            mResultData.fMinOperateDistance,
                            mResultData.fMaxOperateDistance,
                            (mResultData.bTotalResult ? "Pass" : "Fail")
                            );
                        sw.WriteLine(strTemp);
                    }
                }                
                //if (_systemParams.IsSaveDistanceMap)
                //{
                //    // 거리 데이터 저장
                //    Bitmap distanceMap = new Bitmap(pictureEditInspectionStatistics.ClientSize.Width, pictureEditInspectionStatistics.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                //    for (int y = 0; y < distanceMap.Height; ++y)
                //    {
                //        for (int x = 0; x < distanceMap.Width; ++x)
                //        {
                //            distanceMap.SetPixel(x, y, Color.Black);
                //        }
                //    }

                //    Graphics graphic = Graphics.FromImage(distanceMap);

                //    graphic.InterpolationMode = InterpolationMode.Bicubic;
                //    graphic.SmoothingMode = SmoothingMode.AntiAlias;

                //    int width = pictureEditInspectionStatistics.ClientSize.Width;
                //    int height = pictureEditInspectionStatistics.ClientSize.Height;
                //    int passCount = 0;
                //    int failCount = 0;

                //    PointF fptCenter = new PointF(width / 2, height / 2);
                //    float fScale = height / 100f;
                //    float onePixelResolution = _systemParams.CameraParameter.OnePixelResolution;
                //    float threshold = _workParams.DistanceAlignMarkCenterToLEDMark;

                //    Pen pen = new Pen(Brushes.White, 1f);
                //    pen.DashStyle = DashStyle.Dash;

                //    // 중심선 라인 그리기
                //    graphic.DrawLine(pen, new Point(0, height / 2), new Point(width, height / 2));
                //    graphic.DrawLine(pen, new Point(width / 2, 0), new Point(width / 2, height));

                //    // 중심에서 40um 원 그리기
                //    graphic.DrawEllipse(pen, (fptCenter.X - threshold / onePixelResolution * fScale), (fptCenter.Y - threshold / onePixelResolution * fScale), threshold * 2 / onePixelResolution * fScale, threshold * 2 / onePixelResolution * fScale);

                //    for (int i = 0; i < _workParams.InspectionPositions.Count; ++i)
                //    {
                //        if (_workParams.InspectionPositions[i].eInspectionMode == INSPECTION_MODE.INSPECTION_MODE_LED_INSPECTION)
                //        {
                //            PointF fDiff = new PointF((_workParams.InspectionPositions[i].LEDMarkX - _workParams.InspectionPositions[i].LEDAlignCenterX) * fScale,
                //                (_workParams.InspectionPositions[i].LEDMarkY - _workParams.InspectionPositions[i].LEDAlignCenterY) * fScale);

                //            if (_workParams.InspectionPositions[i].IsResult)
                //            {
                //                Pen greenPen = new Pen(Brushes.LightGreen, 2f);

                //                graphic.DrawLine(greenPen, new PointF(fptCenter.X + fDiff.X - 10, fptCenter.Y + fDiff.Y), new PointF(fptCenter.X + fDiff.X + 10, fptCenter.Y + fDiff.Y));
                //                graphic.DrawLine(greenPen, new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y - 10), new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y + 10));

                //                passCount++;
                //            }
                //            else
                //            {
                //                Pen redPen = new Pen(Brushes.Red, 2f);

                //                graphic.DrawLine(redPen, new PointF(fptCenter.X + fDiff.X - 10, fptCenter.Y + fDiff.Y), new PointF(fptCenter.X + fDiff.X + 10, fptCenter.Y + fDiff.Y));
                //                graphic.DrawLine(redPen, new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y - 10), new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y + 10));

                //                failCount++;
                //            }
                //        }
                //    }

                //    graphic.DrawString(string.Format("Pass:{0:00}, Fail:{1:00}", passCount, failCount),
                //        new Font(FontFamily.GenericSansSerif, 15f, FontStyle.Underline),
                //        Brushes.LightGreen, new PointF(10, 10));

                //    string strFile = strFilePath + @"\DistanceMap.jpg";
                //    distanceMap.Save(strFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                //}

                //if (_systemParams.IsSaveStatistics)
                //{
                //    //string strStatisticsImage = string.Format(@"{0}\{1}", strFilePath, "TotalStatistics.jpg");
                //    //chartControlInspectionStatistics.ExportToImage(strStatisticsImage, System.Drawing.Imaging.ImageFormat.Jpeg);

                //    string strStatistics = string.Format(@"{0}\{1}", strFilePath, "TotalStatistics.ini");
                //    RecipeFileIO.WriteInspectionStatisticsFile(strStatistics, _statistics);

                //    strStatistics = string.Format(@"{0}\{1}", strFilePath, "CurrentStatistics.ini");
                //    RecipeFileIO.WriteInspectionStatisticsFile(strStatistics, _currentStatistics);
                //}
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.Message);
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.StackTrace.ToString());
            }
        }
        public void LedSpotImageProcessing(Image ProcessImage, ref Blob retBlob)
        {
            try
            {
                if (ProcessImage != null)
                {
                    Bitmap tempImage = Utils.Clone<Bitmap>((Bitmap)ProcessImage);
                    Bitmap outImage = Utils.Clone<Bitmap>((Bitmap)ProcessImage);

                    if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                    {
                        int w = tempImage.Width,
                            h = tempImage.Height,
                            r, ic, oc, bmpStride, outputStride, bytesPerPixel;
                        System.Drawing.Imaging.PixelFormat pfIn = tempImage.PixelFormat;
                        System.Drawing.Imaging.ColorPalette palette;
                        Bitmap output;
                        System.Drawing.Imaging.BitmapData bmpData, outputData;

                        //Create the new bitmap
                        outImage = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                        //Build a grayscale color Palette
                        palette = outImage.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            Color tmp = Color.FromArgb(255, i, i, i);
                            palette.Entries[i] = Color.FromArgb(255, i, i, i);
                        }
                        outImage.Palette = palette;

                        //Get the number of bytes per pixel
                        switch (pfIn)
                        {
                            case System.Drawing.Imaging.PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                            case System.Drawing.Imaging.PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                            case System.Drawing.Imaging.PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                            default: throw new InvalidOperationException("Image format not supported");
                        }

                        //Lock the images
                        bmpData = tempImage.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly,
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
                                            ((bmpPtr[r * bmpStride + ic] / 255.0f) *
                                            (0.299f * bmpPtr[r * bmpStride + ic + 1] +
                                                0.587f * bmpPtr[r * bmpStride + ic + 2] +
                                                0.114f * bmpPtr[r * bmpStride + ic + 3]));
                            }
                        }

                        //Unlock the images
                        tempImage.UnlockBits(bmpData);
                        outImage.UnlockBits(outputData);
                    }
                    if ((_workParams._LedInspectionWorkAreaWidth == 0) && (_workParams._LedInspectionWorkAreaHeight == 0))
                    {
                        _workParams._LEDInspectionWorkAreaLeft = 0;
                        _workParams._LedInspectionWorkAreaWidth = _systemParams._cameraParams.HResolution;
                        _workParams._LEDInspectionWorkAreaTop = 0;
                        _workParams._LedInspectionWorkAreaHeight = _systemParams._cameraParams.VResolution;
                        _workParams.AreaEnd = new PointF(_workParams._LedInspectionWorkAreaWidth,_workParams._LedInspectionWorkAreaHeight);
                    }

                    _blobs.Clear();

                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("자동 검사 중 광축 측정 시작"));

                    OpticalSpot spot = new OpticalSpot(_systemParams);

                    spot._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                    spot.IsLog = true;

                    int BlobMinSize = 0, BlobMaxSize = 0;

                    BlobMinSize = (int)(_workParams._LEDInspectionSpotMinSize / _systemParams._cameraParams.OnePixelResolution);
                    BlobMaxSize = (int)(_workParams._LEDInspectionSpotMaxSize / _systemParams._cameraParams.OnePixelResolution);

                    spot.OpticalSpotBlobProcess(tempImage, _blobs, _workParams, _workParams._LEDInspectionReferenceThresholdH, _workParams._LEDInspectionReferenceThresholdV, BlobMinSize, BlobMaxSize, ref _ImageHist_W, ref _ImageHist_H);

                    _isOpticalMeasurement = true;
                    _resultImage = outImage;
                    if (_blobs.Count == 1)
                    {
                        retBlob = _blobs[0];
                    }
                    else
                       retBlob = null;
                }
                else
                {
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("자동 검사 중 LED 검사 이미지가 로드되지 않았습니다"));
                    retBlob = null;
                }               
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("자동 검사 중 LED 검사 프로세싱이 실행되지 않았습니다."));
                retBlob = null;
            }
        }
    }
}
