using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step4Spot1Measure : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public static int iGrapCount = 0;
        string strstep = string.Empty;
        public Step4Spot1Measure()
        {
            //Do some init here.
            //ErrorStepString = "Spot1 Image Capture";
            ErrorStepString = "광원1 크기 측정";
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
                                strstep = "Image Grab Timeout";
                                ErrorStepString += strstep;
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
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (LedSpotImageProcess(0, mRobotInformation.PositionX, mRobotInformation.PositionY, mRobotInformation.PositionZ))
                        mStep = WorkingStep.Idle;
                    else
                    {
                        strstep = "Image Spot Not Detect";
                        ErrorStepString += strstep;
                        mStep = WorkingStep.ErrorOccured;
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
