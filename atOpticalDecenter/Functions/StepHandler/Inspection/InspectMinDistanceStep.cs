using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;
using AiCControlLibrary.SerialCommunication.Control;
using AiCControlLibrary.SerialCommunication.Data;
using ARMLibrary.SerialCommunication.Control;
using ARMLibrary.SerialCommunication.Data;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class InspectMinDistanceStep : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectMinDistanceStep()
        {
            //Do some init here.
            ErrorStepString = "동작거리 검사";
        }

        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CheckShortDistanceInspect,
            CheckOutputType,
            SetPowerModeSignal,
            SensorPowerOn,
            WaitStablePower,
            ReadyPositionMove,            
            CheckFirstOutputSwitching,
            MoveOperatingDistance,              // Max Operating Distance(동작거리)            
            CheckOutputSignal,
            SaveOperatingDistance,
            ReleasePosition,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] data = new byte[4];
            int[] CurrentValue = new int[Enum.GetValues(typeof(MT4xPanelMeta.DeviceValue)).Length];
            UserCodesysData.DigitalOutputControl mOutputControl = new UserCodesysData.DigitalOutputControl();
            UserCodesysData.RobotInfomation mPLCInfo = new UserCodesysData.RobotInfomation();
            double fVoltage = 0.0, fCurrent = 0.0;
            
            switch (mStep)
            {
                case WorkingStep.CheckStatus:
                    if (!IsEssentialInstanceSetted)
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    else
                    {
                        if (mCodesysPLC.IsConnected())
                        {
                            if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000008))          // 로봇 준비 상태 체크
                            {
                                bMinDistanceInspectEnable = mWorkParam._ProductMinObjectDetectProcessEnable;
                                mStep = WorkingStep.CheckShortDistanceInspect;
                            }
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.CheckShortDistanceInspect:
                    if (mWorkParam._ProductDistance >= mWorkParam._LEDInspectionShortDistance)
                    {
                        mStep = WorkingStep.CheckOutputType;
                    }
                    else
                        mStep = WorkingStep.Idle;
                    break;
                case WorkingStep.CheckOutputType:
                    //if (mWorkParam._ProductOutputType == (int)PhotoProduct.Enums.OutputType.PNP)
                    //{
                    //    //mOutputControl.Bit64 |= 0x00000001;           // Output Type PNP Signal Set
                    //}
                    //else
                    //{
                    //    //mOutputControl.Bit64 |= 0x00000001;           // Output Type NPN Signal Set
                    //}

                    //data = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.SetPowerModeSignal;
                    break;
                case WorkingStep.SetPowerModeSignal:
                    //mOutputControl.Bit64 |= 0x00000001;               // Photo Sensor Load Enable Signal Set
                    //data = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.SensorPowerOn;
                    break;
                case WorkingStep.SensorPowerOn:
                    //mOutputControl.Bit64 |= 0x00000001;               // Photo Sensor Power On Signal Set
                    //data = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.WaitStablePower;
                    break;
                case WorkingStep.WaitStablePower:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.ReadyPositionMove;
                    }
                    break;
                case WorkingStep.ReadyPositionMove:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                                    byte[] posdata = new byte[32];
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                    mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                                    mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    mCmdPosMove.Yaw = (double)mInspectResultData.fND_FilterAngle - ND_FILTER_MINMAX_READY_OFFSET;

                                    if (bCalibrationActive)
                                    {
                                        mPrePos.X = mCmdPosMove.X;
                                        mPrePos.Y1 = mCmdPosMove.Y;
                                        mPrePos.Y2 = mCmdPosMove.Z;
                                        mPrePos.Z = mCmdPosMove.Roll;
                                        mPrePos.FZ = mCmdPosMove.Pitch;
                                        mPrePos.FR = mCmdPosMove.Yaw;

                                        mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                        mCmdPosMove.X += (double)mDeltaPos.X;
                                        mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                        mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                        mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                        mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                        mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    }
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    posdata = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                    mStep = WorkingStep.CheckFirstOutputSwitching;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.CheckFirstOutputSwitching:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    {
                        CmdFilterAngleIndex = 0;
                        mStep = WorkingStep.MoveOperatingDistance;
                    }
                    break;
                case WorkingStep.MoveOperatingDistance:
                    if (bMinDistanceInspectEnable)
                    {
                        if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        {
                            if (mWorkParam.InspectionPositions.Count > 0)
                            {
                                for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                                {
                                    if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                    {
                                        // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                                        byte[] posdata = new byte[32];
                                        UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                        RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                        RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                        mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                        mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                                        mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                        mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                        mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                        mCmdPosMove.Yaw = (double)mInspectResultData.fND_FilterAngle - ND_FILTER_MINMAX_READY_OFFSET + CmdFilterAngleIndex;

                                        if (bCalibrationActive)
                                        {
                                            mPrePos.X = mCmdPosMove.X;
                                            mPrePos.Y1 = mCmdPosMove.Y;
                                            mPrePos.Y2 = mCmdPosMove.Z;
                                            mPrePos.Z = mCmdPosMove.Roll;
                                            mPrePos.FZ = mCmdPosMove.Pitch;
                                            mPrePos.FR = mCmdPosMove.Yaw;

                                            mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                            mCmdPosMove.X += (double)mDeltaPos.X;
                                            mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                            mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                            mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                            mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                            mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                        }
                                        if (fMoveVelocity < 1f)
                                            fMoveVelocity = 100f;
                                        if (fMoveAcceleration < 1f)
                                            fMoveAcceleration = 1000f;
                                        mCmdPosMove.Speed = (double)fMoveVelocity;
                                        mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                        if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                        if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                        posdata = mCmdPosMove.GetData();
                                        // Send Cordinate Move Command
                                        mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                        mStep = WorkingStep.CheckOutputSignal;
                                        mTimeChecker.SetTime(PLC_OUTPUT_SIGNAL_WAIT_TIME);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.CheckOutputSignal:
                    if (mTimeChecker.IsTimeOver())
                    {
                        if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        {
                            if (mWorkParam._ProductOutputType == 0)                                             // 0:NPN, 1:PNP
                            {
                                if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000008))          // 출력 신호 상태 체크
                                {
                                    CmdFilterAngleIndex += 1;
                                    mStep = WorkingStep.MoveOperatingDistance;
                                }
                                else
                                {
                                    mStep = WorkingStep.SaveOperatingDistance;
                                }
                            }
                            else
                            {
                                if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000008))          // 출력 신호 상태 체크
                                {
                                    mStep = WorkingStep.SaveOperatingDistance;
                                }
                                else
                                {
                                    CmdFilterAngleIndex += 1;
                                    mStep = WorkingStep.MoveOperatingDistance;
                                }
                            }
                        }
                    }                    
                    break;
                case WorkingStep.SaveOperatingDistance:
                    mInspectResultData.fMinOperateDistance = mPLCData.mReceivedRobotInfomation.mPosition.Yaw;              // 동작 지점 위치 저장.                    
                    mStep = WorkingStep.ReleasePosition;
                    break;
                case WorkingStep.ReleasePosition:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                                    byte[] posdata = new byte[32];
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                    mCmdPosMove.X = (double)0;
                                    mCmdPosMove.Y = (double)200;
                                    mCmdPosMove.Z = (double)200;
                                    mCmdPosMove.Roll = (double)30;
                                    mCmdPosMove.Pitch = (double)10;
                                    mCmdPosMove.Yaw = (double)8;

                                    if (bCalibrationActive)
                                    {
                                        mPrePos.X = mCmdPosMove.X;
                                        mPrePos.Y1 = mCmdPosMove.Y;
                                        mPrePos.Y2 = mCmdPosMove.Z;
                                        mPrePos.Z = mCmdPosMove.Roll;
                                        mPrePos.FZ = mCmdPosMove.Pitch;
                                        mPrePos.FR = mCmdPosMove.Yaw;

                                        mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                        mCmdPosMove.X += (double)mDeltaPos.X;
                                        mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                        mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                        mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                        mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                        mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    }
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    posdata = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                    mStep = WorkingStep.Idle;
                                    break;
                                }
                            }
                        }
                    }                    
                    break;
                case WorkingStep.ErrorOccured:
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
                //Run();
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
