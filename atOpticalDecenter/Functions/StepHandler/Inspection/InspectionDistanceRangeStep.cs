using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using AiCControlLibrary.SerialCommunication.Control;
using AiCControlLibrary.SerialCommunication.Data;
using ARMLibrary.SerialCommunication.Control;
using ARMLibrary.SerialCommunication.Data;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class InspectionDistanceRangeStep : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectionDistanceRangeStep()
        {
            //Do some init here.
            ErrorStepString = "최대 검출, 동작 거리검사 검사";
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
            CheckFirstOutputSwitching,
            MoveOperatingDistance,              // Max Operating Distance(동작거리)            
            CheckOutputSignal,
            SaveOperatingDistance,
            CheckOperateMode,
            L_ON_OutputStatus,
            D_ON_OutputStatus,
            CheckBGSModel,
            SensorPowerOff,
            ReleasePowerModeSignal,
            CalculateSensorDistance,
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
                                bMaxDistanceInspectEnable = false;
                                mStep = WorkingStep.CheckOperateMode;
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
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.WaitStablePower;
                    break;
                case WorkingStep.WaitStablePower:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.CheckFirstOutputSwitching;
                    }
                    break;
                case WorkingStep.CheckFirstOutputSwitching:

                    mStep = WorkingStep.MoveOperatingDistance;
                    break;
                case WorkingStep.MoveOperatingDistance:
                    if (bMaxDistanceInspectEnable)
                    {
                        if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        {
                            if (InspectPos.Count > 0)
                            {
                                for (int i = 0; i < InspectPos.Count; i++)
                                {
                                    if (InspectPos[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_MAX_DISTANCE_MODE)
                                    {
                                        // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 동작 지점

                                        byte[] posdata = new byte[32];
                                        UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                        mCmdPosMove.X = (double)InspectPos[i].PositionX;
                                        mCmdPosMove.Y = (double)InspectPos[i].PositionY1;
                                        mCmdPosMove.Z = (double)InspectPos[i].PositionY2;
                                        mCmdPosMove.Roll = (double)InspectPos[i].PositionZ;
                                        mCmdPosMove.Pitch = (double)InspectPos[i].PositionFilterZ;
                                        mCmdPosMove.Yaw = (double)InspectPos[i].PositionFilterR;
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
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))          // 정지 상태 체크
                        {
                            if (InspectPos.Count > 0)
                            {
                                for (int i = 0; i < InspectPos.Count; i++)
                                {
                                    if (InspectPos[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_MIN_ORIGIN_DISTANCE_MODE)
                                    {
                                        // 단축 거리 검사 대상 체크 필요!, 단축거리 검사일 경우 ND 필터 조절, 복귀 지점 

                                        byte[] posdata = new byte[32];
                                        UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                        mCmdPosMove.X = (double)InspectPos[i].PositionX;
                                        mCmdPosMove.Y = (double)InspectPos[i].PositionY1;
                                        mCmdPosMove.Z = (double)InspectPos[i].PositionY2;
                                        mCmdPosMove.Roll = (double)InspectPos[i].PositionZ;
                                        mCmdPosMove.Pitch = (double)InspectPos[i].PositionFilterZ;
                                        mCmdPosMove.Yaw = (double)InspectPos[i].PositionFilterR;
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
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.CheckOutputSignal:
                    if (bMaxDistanceInspectEnable)
                    {
                        if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000050))          // 출력 신호 상태 체크
                        {
                            mStep = WorkingStep.SaveOperatingDistance;
                        }
                    }
                    else
                    {
                        if (!Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000050))          // 출력 신호 상태 체크
                        {
                            mStep = WorkingStep.SaveOperatingDistance;
                        }
                    }
                    break;
                case WorkingStep.SaveOperatingDistance:
                    if (bMaxDistanceInspectEnable)
                    {
                        //mPLCData.mReceivedRobotInfomation.mPosition;              // 동작 지점 위치 저장.
                    }
                    else
                    {
                        // 복귀 지점 위치 저장
                    }
                    mStep = WorkingStep.CheckOperateMode;
                    break;
                case WorkingStep.CheckOperateMode:
                    if (mWorkParam._ProductOperatingMdoe == (int)PhotoProduct.Enums.OperatingMode.LightOn)
                    {
                        mStep = WorkingStep.L_ON_OutputStatus;
                    }
                    else
                    {
                        mStep = WorkingStep.D_ON_OutputStatus;
                    }
                    break;
                case WorkingStep.L_ON_OutputStatus:
                    Buffer.BlockCopy(mPanelData.MT4xProduct[2][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[2].ElementAt(0).Length);
                    fVoltage = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);
                    Buffer.BlockCopy(mPanelData.MT4xProduct[3][0], 0, CurrentValue, 0, (int)mPanelData.MT4xProduct[3].ElementAt(0).Length);
                    fCurrent = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);

                    if (fVoltage > (LIGHT_ON_LOAD_VOLTAGE + LOAD_VOLTAGE_MARGIN) || (fVoltage > (LIGHT_ON_LOAD_VOLTAGE - LOAD_VOLTAGE_MARGIN)))
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    if (fCurrent > LIGHT_ON_LOAD_CURRENT)
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    if (mPLCInfo.mStatus == (0x00000001))
                    {
                        mStep = WorkingStep.CheckBGSModel;
                    }
                    else
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.D_ON_OutputStatus:
                    Buffer.BlockCopy(mPanelData.MT4xProduct[2][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[2].ElementAt(0).Length);
                    fVoltage = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);
                    Buffer.BlockCopy(mPanelData.MT4xProduct[3][0], 0, CurrentValue, 0, (int)mPanelData.MT4xProduct[3].ElementAt(0).Length);
                    fCurrent = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);

                    if (fVoltage > (DARK_ON_LOAD_VOLTAGE + LOAD_VOLTAGE_MARGIN) || (fVoltage > (DARK_ON_LOAD_VOLTAGE - LOAD_VOLTAGE_MARGIN)))
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    if (fCurrent > DARK_ON_LOAD_CURRENT)
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    if (mPLCInfo.mStatus == (0x00000001))
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
                    else
                    {
                        mStep = WorkingStep.CheckBGSModel;
                    }
                    break;
                case WorkingStep.CheckBGSModel:
                    if (mWorkParam._ProductType == (int)PhotoProduct.Enums.ProductType.BGS)
                    {
                        bMaxDistanceInspectEnable = true;
                        mStep = WorkingStep.CheckFirstOutputSwitching;
                    }
                    else
                        mStep = WorkingStep.SensorPowerOff;
                    break;
                case WorkingStep.SensorPowerOff:
                    //mOutputControl.Bit64 |= 0x00000001;                           // Sensor Power Signal Off
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.ReleasePowerModeSignal;
                    break;
                case WorkingStep.ReleasePowerModeSignal:
                    //mOutputControl.Bit64 |= 0x00000001;                           // Sensor Setting Signal Off
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.Idle;
                    break;
                case WorkingStep.CalculateSensorDistance:
                    // 응차거리, 응차 계산, 정격 위치 이동
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
