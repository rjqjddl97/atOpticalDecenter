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
    public class InspectMaxDistanceStep : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectMaxDistanceStep()
        {
            //Do some init here.
            ErrorStepString = "최대거리 검사";
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
            CheckBlindEnable,
            WaitBlindEnableCylinderSignal,
            MoveOperatingDistance,              // Max Operating Distance(동작거리)      
            WaitSignalStable,
            CheckOutputSignal,
            SaveOperatingDistance,
            CheckBlindRelease,
            WaitBlindDisableCylinderSignal,
            ReleasePosition,
            SensorPowerOff,
            ErrorOccured,
        }
        private void Run()
        {            
            //int[] CurrentValue = new int[Enum.GetValues(typeof(MT4xPanelMeta.DeviceValue)).Length];
            ARMData mOutputControl = new ARMData();
            //UserCodesysData.RobotInfomation mPLCInfo = new UserCodesysData.RobotInfomation();
            double fVoltage = 0.0, fCurrent = 0.0;
            byte[] iodata = new byte[4];
            switch (mStep)
            {
                case WorkingStep.CheckStatus:
                    if (!IsEssentialInstanceSetted)
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    else
                    {
                        //if (mCodesysPLC.IsConnected())
                        //{
                        //    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000008))          // 로봇 준비 상태 체크
                        //    {
                        //        bMaxDistanceInspectEnable = mWorkParam._ProductMaxDistanceProcessEnable;
                        //        mStep = WorkingStep.CheckShortDistanceInspect;
                        //    }
                        //    else
                        //        mStep = WorkingStep.ErrorOccured;
                        //}
                        //else
                        //    mStep = WorkingStep.ErrorOccured;
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
                    //mOutputControl.Bit64 = mPLCInfomationData.mOutputData.Bit64;               // Photo Sensor Power On Signal Set
                    //if (mWorkParam._ProductType != 5)                                          // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
                    //{
                    //    if (mWorkParam._ProductOutputType == 1)
                    //        mOutputControl.B1 = true;               // Photo Sensor NPN/PNP Signal Set
                    //    else
                    //        mOutputControl.B1 = false;               // Photo Sensor NPN/PNP Signal Set
                    //    if (mWorkParam._ProductType != 6)
                    //    {
                    //        mOutputControl.B8 = true;               // Up/Down Cylinder Signal Set(Up)
                    //    }
                    //    else
                    //    {
                    //        mOutputControl.B8 = false;               // Up/Down Cylinder Signal Set(Down)
                    //    }
                    //}
                    //else
                    //{
                    //    if (mWorkParam._ProductOutputType == 1)
                    //        mOutputControl.B6 = true;               // Photo Sensor NPN/PNP Signal Set
                    //    else
                    //        mOutputControl.B6 = false;               // Photo Sensor NPN/PNP Signal Set
                    //    mOutputControl.B8 = false;               // Up/Down Cylinder Signal Reset(down)
                    //}
                    //iodata = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    mStep = WorkingStep.ReadyPositionMove;
                    break;
                case WorkingStep.ReadyPositionMove:
                    //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    //{
                    //    if (mWorkParam.InspectionPositions.Count > 0)
                    //    {
                    //        for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                    //        {
                    //            if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                    //            {
                    //                // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                    //                byte[] posdata = new byte[32];
                    //                UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                    //                RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                    //                RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                    //                mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                    //                mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                    //                mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                    //                mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                    //                mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                    //                mCmdPosMove.Yaw = (double)mInspectResultData.fND_FilterAngle + ND_FILTER_MINMAX_READY_OFFSET;

                    //                if (bCalibrationActive)
                    //                {
                    //                    mPrePos.X = mCmdPosMove.X;
                    //                    mPrePos.Y1 = mCmdPosMove.Y;
                    //                    mPrePos.Y2 = mCmdPosMove.Z;
                    //                    mPrePos.Z = mCmdPosMove.Roll;
                    //                    mPrePos.FZ = mCmdPosMove.Pitch;
                    //                    mPrePos.FR = mCmdPosMove.Yaw;

                    //                    mDeltaPos = mCalibrationParam.InspectCalibratinoDeltaPosition(mPrePos);
                    //                    mCmdPosMove.X += (double)mDeltaPos.X;
                    //                    mCmdPosMove.Y += (double)mDeltaPos.Y1;
                    //                    mCmdPosMove.Z += (double)mDeltaPos.Y2;
                    //                    mCmdPosMove.Roll += (double)mDeltaPos.Z;
                    //                    mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                    //                    mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                    //                }
                    //                if (fMoveVelocity < 1f)
                    //                    fMoveVelocity = 100f;
                    //                if (fMoveAcceleration < 1f)
                    //                    fMoveAcceleration = 1000f;
                    //                mCmdPosMove.Speed = (double)fMoveVelocity;
                    //                mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                    //                if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                    //                if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                    //                posdata = mCmdPosMove.GetData();
                    //                // Send Cordinate Move Command
                    //                mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                    //                mStep = WorkingStep.CheckFirstOutputSwitching;
                    //                mTimeChecker.SetTime(PLC_OUTPUT_SIGNAL_WAIT_TIME);
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                    break;
                case WorkingStep.CheckFirstOutputSwitching:
                    //if (mTimeChecker.IsTimeOver())
                    //{
                    //    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    //    {
                    //        CmdFilterAngleIndex = 0;
                    //        mStep = WorkingStep.CheckBlindEnable;
                    //    }
                    //}
                    break;
                case WorkingStep.CheckBlindEnable:
                    //if ((mWorkParam._ProductType == 0) || (mWorkParam._ProductType == 1) || (mWorkParam._ProductType == 2) || (mWorkParam._ProductType == 3) || (mWorkParam._ProductType == 4))                 // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
                    //{                        
                    //    mOutputControl.Bit64 = mPLCInfomationData.mOutputData.Bit64;                        
                    //    mOutputControl.B9 = true;               // Photo Sensor Blind Set
                    //    iodata = mOutputControl.GetData();
                    //    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    //    mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);                        
                    //}
                    mStep = WorkingStep.WaitBlindEnableCylinderSignal;
                    break;
                case WorkingStep.WaitBlindEnableCylinderSignal:
                    if (mTimeChecker.IsTimeOver())
                    {
                        //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 실린더 센서 상태 체크
                        {
                            CmdFilterAngleIndex = 0;
                            mStep = WorkingStep.SensorPowerOn;
                        }
                    }
                    break;
                case WorkingStep.SensorPowerOn:
                    //mOutputControl.Bit64 = mPLCInfomationData.mOutputData.Bit64;               // Photo Sensor Power On Signal Set
                    //mOutputControl.B0 = true;               // Photo Sensor Power On Signal Set
                    //iodata = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    //mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.WaitStablePower;
                    break;
                case WorkingStep.WaitStablePower:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.MoveOperatingDistance;
                    }
                    break;
                case WorkingStep.MoveOperatingDistance:
                    if (bMaxDistanceInspectEnable)
                    {
                        //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        //{
                        //    if (mWorkParam.InspectionPositions.Count > 0)
                        //    {
                        //        for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                        //        {
                        //            if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                        //            {
                        //                // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                        //                byte[] posdata = new byte[32];
                        //                UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                        //                RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                        //                RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                        //                mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                        //                mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                        //                mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                        //                mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                        //                mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                        //                mCmdPosMove.Yaw = (double)mInspectResultData.fND_FilterAngle + ND_FILTER_MINMAX_READY_OFFSET - CmdFilterAngleIndex;

                        //                if (bCalibrationActive)
                        //                {
                        //                    mPrePos.X = mCmdPosMove.X;
                        //                    mPrePos.Y1 = mCmdPosMove.Y;
                        //                    mPrePos.Y2 = mCmdPosMove.Z;
                        //                    mPrePos.Z = mCmdPosMove.Roll;
                        //                    mPrePos.FZ = mCmdPosMove.Pitch;
                        //                    mPrePos.FR = mCmdPosMove.Yaw;

                        //                    mDeltaPos = mCalibrationParam.InspectCalibratinoDeltaPosition(mPrePos);
                        //                    mCmdPosMove.X += (double)mDeltaPos.X;
                        //                    mCmdPosMove.Y += (double)mDeltaPos.Y1;
                        //                    mCmdPosMove.Z += (double)mDeltaPos.Y2;
                        //                    mCmdPosMove.Roll += (double)mDeltaPos.Z;
                        //                    mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                        //                    mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                        //                }
                        //                if (fMoveVelocity < 1f)
                        //                    fMoveVelocity = 100f;
                        //                if (fMoveAcceleration < 1f)
                        //                    fMoveAcceleration = 1000f;
                        //                mCmdPosMove.Speed = (double)fMoveVelocity;
                        //                mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                        //                if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                        //                if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                        //                posdata = mCmdPosMove.GetData();
                        //                // Send Cordinate Move Command
                        //                mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                        //                mStep = WorkingStep.WaitSignalStable;                                        
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    break;
                case WorkingStep.WaitSignalStable:
                    //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                    {
                        mTimeChecker.SetTime(PLC_OUTPUT_SIGNAL_WAIT_TIME);
                        mStep = WorkingStep.CheckOutputSignal;
                    }
                    break;
                case WorkingStep.CheckOutputSignal:
                    if (mTimeChecker.IsTimeOver())
                    {
                        if (mWorkParam._ProductOutputType == 0)                                             // 0:NPN, 1:PNP
                        {
                            if (mWorkParam._ProductType != 5)             // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광 
                            {
                                //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000010))          // 출력 신호 상태 체크
                                //{   
                                //    mStep = WorkingStep.SaveOperatingDistance;
                                //}
                                //else
                                //{
                                //    CmdFilterAngleIndex += 1;
                                //    mStep = WorkingStep.MoveOperatingDistance;                                    
                                //}
                            }
                            else
                            {
                                //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000008))          // 출력 신호 상태 체크
                                //{
                                //    CmdFilterAngleIndex += 1;
                                //    mStep = WorkingStep.MoveOperatingDistance;
                                //}
                                //else
                                //{
                                //    mStep = WorkingStep.SaveOperatingDistance;
                                //}
                            }

                        }
                        else
                        {
                            if (mWorkParam._ProductType != 5)             // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광 
                            {
                                //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000010))          // 출력 신호 상태 체크
                                //{                                    
                                //    mStep = WorkingStep.SaveOperatingDistance;

                                //}
                                //else
                                //{
                                //    CmdFilterAngleIndex += 1;
                                //    mStep = WorkingStep.MoveOperatingDistance;
                                //}
                            }
                            else
                            {
                                //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000008))          // 출력 신호 상태 체크
                                //{
                                //    mStep = WorkingStep.SaveOperatingDistance;
                                //}
                                //else
                                //{
                                //    CmdFilterAngleIndex += 1;
                                //    mStep = WorkingStep.MoveOperatingDistance;
                                //}
                            }
                        }
                    }
                    break;
                case WorkingStep.SaveOperatingDistance:
                    //mInspectResultData.fMaxOperateDistance = mPLCData.mReceivedRobotInfomation.mPosition.Yaw;              // 동작 지점 위치 저장.                    
                    mStep = WorkingStep.CheckBlindRelease;
                    break;
                case WorkingStep.CheckBlindRelease :                    
                    //mOutputControl.Bit64 = mPLCInfomationData.mOutputData.Bit64;
                    //mOutputControl.B9 = false;               // Photo Sensor Blind Set
                    //iodata = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    //mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.ReleasePosition;
                    break;
                case WorkingStep.ReleasePosition:
                    if (mTimeChecker.IsTimeOver())
                    {
                        //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        {
                            if (mWorkParam.InspectionPositions.Count > 0)
                            {
                                //for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                                //{
                                //    if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                //    {
                                //        // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                                //        byte[] posdata = new byte[32];
                                //        UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                //        RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                //        RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                //        mCmdPosMove.X = (double)0;
                                //        mCmdPosMove.Y = (double)200;
                                //        mCmdPosMove.Z = (double)200;
                                //        mCmdPosMove.Roll = (double)30;
                                //        mCmdPosMove.Pitch = (double)10;
                                //        mCmdPosMove.Yaw = (double)8;

                                //        if (bCalibrationActive)
                                //        {
                                //            mPrePos.X = mCmdPosMove.X;
                                //            mPrePos.Y1 = mCmdPosMove.Y;
                                //            mPrePos.Y2 = mCmdPosMove.Z;
                                //            mPrePos.Z = mCmdPosMove.Roll;
                                //            mPrePos.FZ = mCmdPosMove.Pitch;
                                //            mPrePos.FR = mCmdPosMove.Yaw;

                                //            mDeltaPos = mCalibrationParam.InspectCalibratinoDeltaPosition(mPrePos);
                                //            mCmdPosMove.X += (double)mDeltaPos.X;
                                //            mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                //            mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                //            mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                //            mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                //            mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                //        }
                                //        if (fMoveVelocity < 1f)
                                //            fMoveVelocity = 100f;
                                //        if (fMoveAcceleration < 1f)
                                //            fMoveAcceleration = 1000f;
                                //        mCmdPosMove.Speed = (double)fMoveVelocity;
                                //        mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                //        if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                //        if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                //        posdata = mCmdPosMove.GetData();
                                //        // Send Cordinate Move Command
                                //        mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                //        mStep = WorkingStep.SensorPowerOff;
                                //        break;
                                //    }
                                //}
                            }
                        }
                    }                    
                    break;
                case WorkingStep.SensorPowerOff:
                    //mOutputControl.Bit64 = mPLCInfomationData.mOutputData.Bit64;               // Photo Sensor Power On Signal Set
                    //mOutputControl.B0 = false;
                    //mOutputControl.B1 = false;
                    //mOutputControl.B6 = false;
                    //mOutputControl.B8 = false;
                    //byte[] Offdata = new byte[4];
                    //Offdata = mOutputControl.GetData();
                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, Offdata);
                    //mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.Idle;
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
