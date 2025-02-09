﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step5MovePosition2 : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public Step5MovePosition2()
        {
            //Do some init here.
            //ErrorStepString = "Move Inpsect Position 1";
            ErrorStepString = "2번째 검사 위치 이동";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            SetMoveTargetPosition,
            MoveInspectPosition,
            WaitDelayTimeCommand,
            WaitCheckInposition,
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

                        if (mMotionDrvCtrl.IsOpen())
                        {
                            if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                            {
                                byte[] data = new byte[100];

                                double vel = (double)mSystemParam._motionParams.MoveVelocity;

                                for (int i = 0; i < 3; i++)
                                {
                                    if (i == 0)
                                    {
                                        data = mMotionDrvCtrl.mDrvCtrl.SetMoveTargetVelocity((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[i], Convert.ToInt32(vel * mSystemParam._motionParams.MM2PulseRatioX));
                                    }
                                    else if (i == 1)
                                    {
                                        if (vel > D_MICRO_MOTION_VELOCITY_LIMIT)
                                            vel = D_MICRO_MOTION_VELOCITY_LIMIT;
                                        data = mMotionDrvCtrl.mDrvCtrl.SetMoveTargetVelocity((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[i], Convert.ToInt32(vel * mSystemParam._motionParams.MM2PulseRatioY));
                                    }
                                    else if (i == 2)
                                    {
                                        if (vel > D_MICRO_MOTION_VELOCITY_LIMIT)
                                            vel = D_MICRO_MOTION_VELOCITY_LIMIT;
                                        data = mMotionDrvCtrl.mDrvCtrl.SetMoveTargetVelocity((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[i], Convert.ToInt32(vel * mSystemParam._motionParams.MM2PulseRatioZ));
                                    }
                                    mMotionDrvCtrl.SendData(data);
                                }
                                _DelayTimerCounter = D_MOTION_COMMAND_RESPONSE_WAIT_TIME;
                                mStep = WorkingStep.SetMoveTargetPosition;
                            }
                        }
                    }
                    break;
                case WorkingStep.SetMoveTargetPosition:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {                            
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
                                {
                                    byte[] data = new byte[100];

                                    for (int j = 0; j < 3; j++)
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
                            mStep = WorkingStep.MoveInspectPosition;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.MoveInspectPosition:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        byte[] data = new byte[8];
                        data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        mMotionDrvCtrl.SendData(data);
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.WaitDelayTimeCommand;
                    }
                    break;
                case WorkingStep.WaitDelayTimeCommand:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.WaitCheckInposition;
                    }
                    break;
                case WorkingStep.WaitCheckInposition:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        mStep = WorkingStep.Idle;
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
