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
    public class Step8CalculateResult : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public static int iGrapCount = 0;
        string strstep = string.Empty;
        public Step8CalculateResult()
        {
            //Do some init here.
            //ErrorStepString = "Spot1 Image Capture";
            ErrorStepString = "광원 편심 계산 및 결과";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CalculateDecenter,
            MoveWaitingPosition,
            WaitDelayTimeCommand,
            InpositionCheck,
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

                        _DelayTimerCounter = D_MOTION_COMMAND_RESPONSE_WAIT_TIME;
                        mStep = WorkingStep.CalculateDecenter;
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("광편심 검사 결과 계산 시작"));
                    }
                    break;
                case WorkingStep.CalculateDecenter:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if ((mRobotInformation.mStatus & 0x00000042) == 0x00000042)             // Inpsotion, Servo On Satus
                    {
                        LedSpotCalcuate();
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_READY_MODE)
                                {
                                    byte[] data = new byte[100];

                                    for (int j = 0; j < mMotionDrvCtrl.mDrvCtrl.DeviceIDCount; j++)
                                    {
                                        if (j == 0)
                                        {
                                            data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[j], Convert.ToInt32((double)(mWorkParam.InspectionPositions[i].PositionX + mWorkParam._LEDInspectionCameraDistance) * mSystemParam._motionParams.MM2PulseRatioX));
                                        }
                                        else if (j == 1)
                                        {
                                            data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[j], Convert.ToInt32((double)mWorkParam.InspectionPositions[i].PositionY * mSystemParam._motionParams.MM2PulseRatioY));
                                        }
                                        else if (j == 2)
                                        {
                                            data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[j], Convert.ToInt32((double)mWorkParam.InspectionPositions[i].PositionZ * mSystemParam._motionParams.MM2PulseRatioZ));
                                        }
                                        mMotionDrvCtrl.SendData(data);
                                    }
                                }
                            }
                            mStep = WorkingStep.MoveWaitingPosition;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }                                   
                    break;
                case WorkingStep.MoveWaitingPosition:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if ((mRobotInformation.mStatus & 0x00000042) == 0x00000042)             // Inpsotion, Servo On Satus
                    {
                        byte[] data = new byte[8];
                        data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        mMotionDrvCtrl.SendData(data);
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.WaitDelayTimeCommand;
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("대기 위치로 이동 명령 전송"));
                    }
                    break;
                case WorkingStep.WaitDelayTimeCommand:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.InpositionCheck;
                    }
                    break;
                case WorkingStep.InpositionCheck:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if ((mRobotInformation.mStatus & 0x00000042) == 0x00000042)             // Inpsotion, Servo On Satus
                    {
                        mStep = WorkingStep.Idle;
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("광편심 검사 작업 완료"));
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
