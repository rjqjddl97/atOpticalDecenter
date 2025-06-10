using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;
using LogLibrary;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step6Spot2Measure : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public static int iGrapCount = 0;
        string strstep = string.Empty;
        public Step6Spot2Measure()
        {
            //Do some init here.
            //ErrorStepString = "Spot1 Image Capture";
            ErrorStepString = "광원2 크기 측정";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CaptureImage,
            WaitStableCaptureTime,
            MeasureSpot,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] posdata = new byte[32];
            switch (mStep)
            {
                case WorkingStep.Idle:
                    break;
                case WorkingStep.CheckStatus:
                    if (!IsEssentialInstanceSetted)
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    else
                    {
                        if (mRobotInformation.mInputData.B0)
                            mStep = WorkingStep.ErrorOccured;

                        if (mRemoteIOCtrl.IsOpen())
                        {
                            _DelayTimerCounter = mWorkParam._LEDInspectionAcquisitionDelaytime;
                            IsGrabbed = false;
                            mStep = WorkingStep.CaptureImage;
                            _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("2번째 광원 크기 측정 검사 시작"));
                        }
                        else
                        {
                            _log.WriteLog(LogLevel.Fatal, LogClass.InspectStep.ToString(), string.Format("Remote I/O 연결 실패!! "));
                            mStep = WorkingStep.ErrorOccured;
                        }
                    }
                    break;
                case WorkingStep.CaptureImage:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if ((mRobotInformation.mStatus & 0x00000042) == 0x00000042)             // Inpsotion, Servo On Satus
                    {
                        TakePicture();
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.WaitStableCaptureTime;
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("2번째 광원 찰영 명령 전송"));
                    }
                    break;
                case WorkingStep.WaitStableCaptureTime:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (mTimeChecker.IsTimeOver() || IsGrabbed)
                    {
                        if (IsGrabbed)
                        {
                            mRetryCount = 0;
                            mStep = WorkingStep.MeasureSpot;
                        }
                        else
                        {
                            mRetryCount++;
                            if (mRetryCount >= RETRY_LIMIT)
                            {
                                mRetryCount = 0;
                                mStep = WorkingStep.ErrorOccured;
                                _log.WriteLog(LogLevel.Error, LogClass.InspectStep.ToString(), string.Format("2번째 광원 찰영 재시도 회수 초과"));
                            }
                            else
                            {
                                iGrapCount = 0;
                                mStep = WorkingStep.CaptureImage;
                                _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("2번째 광원 찰영 {0} 재시도", mRetryCount.ToString()));
                            }
                        }
                    }
                    break;
                case WorkingStep.MeasureSpot:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (LedSpotImageProcess(1, mRobotInformation.PositionX, mRobotInformation.PositionY, mRobotInformation.PositionZ))
                    {
                        mStep = WorkingStep.Idle;
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("2번째 광원 크기 계산 완료"));
                    }
                    else
                    {
                        strstep = "Image Spot Not Detect";
                        ErrorStepString += strstep;
                        mStep = WorkingStep.ErrorOccured;
                        _log.WriteLog(LogLevel.Error, LogClass.InspectStep.ToString(), string.Format("2번째 광원 크기 계산 실패"));
                    }

                    break;
                default: break;
            }
        }
        public void Init()
        {
        }
        public RetType Execute()
        {
            if (mStep == WorkingStep.Idle)
            {
                mStep = WorkingStep.CheckStatus;
                Run();
                return RetType.Busy;
            }
            else
            {
                return RetType.Error;
            }
        }

        public RetType GetStatus()
        {
            Run();

            if (mStep == WorkingStep.ErrorOccured)
                return RetType.Error;
            else if (mStep != WorkingStep.Idle)
                return RetType.Busy;
            else
                return RetType.Ready;
        }

        public bool ClearError()
        {
            if (mStep == WorkingStep.ErrorOccured)
            {
                AlarmNumber = 0;
                mStep = WorkingStep.Idle;
                return true;
            }
            return false;
        }
        public int GetAlarmNumber()
        {
            return AlarmNumber;
        }
    }
}
