using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step6Spot2Measure : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public static int iGrapCount = 0;
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
                        if (mRemoteIOCtrl.IsOpen())
                        {
                            _DelayTimerCounter = mWorkParam._LEDInspectionAcquisitionDelaytime;
                            mStep = WorkingStep.CaptureImage;
                        }
                    }
                    break;
                case WorkingStep.CaptureImage:

                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        TakePicture();
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.WaitStableCaptureTime;
                    }
                    break;
                case WorkingStep.WaitStableCaptureTime:

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
                            }
                            else
                            {
                                iGrapCount = 0;
                                mStep = WorkingStep.CaptureImage;
                            }
                        }
                    }
                    break;
                case WorkingStep.MeasureSpot:
                    LedSpotImageProcess(1, mRobotInformation.PositionX, mRobotInformation.PositionY, mRobotInformation.PositionZ);
                    mStep = WorkingStep.Idle;
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
