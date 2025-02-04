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
    public class InspectionStep6 : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectionStep6()
        {
            //Do some init here.
            ErrorStepString = "초기 소비전류 검사";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CheckOperateMode,
            CheckOutputType,
            SetPowerModeSignal,
            SensorPowerOn,
            WaitStablePower,
            CheckSensorArmpare,
            SensorPowerOff,
            ReleasePowerModeSignal,
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
                                mStep = WorkingStep.CheckOperateMode;
                            }
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.CheckOperateMode:
                    if (mWorkParam._ProductOperatingMdoe == (int)PhotoProduct.Enums.OperatingMode.DarkOn)
                    {
                        mStep = WorkingStep.Idle;
                    }
                    else
                    {
                        mStep = WorkingStep.CheckOutputType;
                    }
                    break;
                case WorkingStep.CheckOutputType:
                    if (mWorkParam._ProductOutputType == (int)PhotoProduct.Enums.OutputType.PNP)
                    {
                        //mOutputControl.Bit64 |= 0x00000001;           // Output Type PNP Signal Set
                    }
                    else
                    {
                        //mOutputControl.Bit64 |= 0x00000001;           // Output Type NPN Signal Set
                    }

                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.SetPowerModeSignal;
                    break;
                case WorkingStep.SetPowerModeSignal:
                    //mOutputControl.Bit64 |= 0x00000001;               // Photo Sensor Load Enable Signal Set
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
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
                        mStep = WorkingStep.CheckSensorArmpare;
                    }
                    break;
                case WorkingStep.CheckSensorArmpare:
                    Buffer.BlockCopy(mPanelData.MT4xProduct[1][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[1].ElementAt(0).Length);
                    fCurrent = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);

                    if (fCurrent > (LIGHT_ON_LOAD_CURRENT + LOAD_CURRENT_MARGIN))
                    {
                        mStep = WorkingStep.SensorPowerOff;                         // 정격 소비전류 범위 체크
                    }

                    mPLCInfo = mPLCData.mReceivedRobotInfomation;

                    if (mPLCInfo.mStatus == (0x00000001))                           // Check Sensor Output Signal
                    {
                        mStep = WorkingStep.SensorPowerOff;
                    }
                    else
                    {
                        mStep = WorkingStep.ErrorOccured;
                    }
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
