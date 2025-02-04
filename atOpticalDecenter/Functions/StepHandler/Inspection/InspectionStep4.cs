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
    public class InspectionStep4 : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectionStep4()
        {
            //Do some init here.
            ErrorStepString = "초기 출력 검사";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CheckOutputType,
            SetPowerModeSignal,
            SensorPowerOn,            
            WaitStablePower,
            CheckOutputStatus,
            SensorPowerOff,
            ReleasePowerModeSignal,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] data = new byte[4];
            UserCodesysData.DigitalOutputControl mOutputControl = new UserCodesysData.DigitalOutputControl();
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
                        if (mCodesysPLC.IsConnected())
                        {
                            if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000008))
                                mStep = WorkingStep.CheckOutputType;
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.CheckOutputType:

                    if (mWorkParam._ProductOutputType == (int)PhotoProduct.Enums.OutputType.PNP)
                    {
                        //mOutputControl.Bit64 |= 0x00000001; 
                    }
                    else
                    {
                        //mOutputControl.Bit64 |= 0x00000001; 
                    }

                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.SetPowerModeSignal;
                    break;
                case WorkingStep.SetPowerModeSignal:

                    //mOutputControl.Bit64 |= 0x00000001; 
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);                    
                    mStep = WorkingStep.SensorPowerOn;
                    break;
                case WorkingStep.SensorPowerOn:

                    //mOutputControl.Bit64 |= 0x00000001; 
                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.WaitStablePower;
                    break;
                case WorkingStep.WaitStablePower:
                    
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.CheckOutputStatus;
                    }
                    break;

                case WorkingStep.CheckOutputStatus:

                    double fVoltage = 0.0, fCurrent = 0.0;

                    if (mWorkParam._ProductOperatingMdoe == (int)PhotoProduct.Enums.OperatingMode.DarkOn)
                    {
                        int[] CurrentValue = new int[Enum.GetValues(typeof(MT4xPanelMeta.DeviceValue)).Length];
                        Buffer.BlockCopy(mPanelData.MT4xProduct[2][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[2].ElementAt(0).Length);
                        fVoltage = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);
                        Buffer.BlockCopy(mPanelData.MT4xProduct[3][0], 0, CurrentValue, 0, (int)mPanelData.MT4xProduct[3].ElementAt(0).Length);
                        fCurrent = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);

                        if (fVoltage > (DARK_ON_LOAD_VOLTAGE + LOAD_VOLTAGE_MARGIN) || (fVoltage > (DARK_ON_LOAD_VOLTAGE - LOAD_VOLTAGE_MARGIN)))
                        {
                            mStep = WorkingStep.SensorPowerOff;
                        }
                        if (fCurrent > DARK_ON_LOAD_CURRENT)
                        {
                            mStep = WorkingStep.SensorPowerOff;
                        }
                    }
                    else
                    {
                        int[] CurrentValue = new int[Enum.GetValues(typeof(MT4xPanelMeta.DeviceValue)).Length];
                        Buffer.BlockCopy(mPanelData.MT4xProduct[0][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[0].ElementAt(0).Length);
                        fVoltage = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);
                        Buffer.BlockCopy(mPanelData.MT4xProduct[1][0], 0, CurrentValue, 0, mPanelData.MT4xProduct[1].ElementAt(0).Length);
                        fCurrent = mPanelData.PresentValue((ushort)CurrentValue[0], (ushort)CurrentValue[1]);

                        if (fCurrent > (LIGHT_ON_LOAD_CURRENT + LOAD_CURRENT_MARGIN) )
                        {
                            mStep = WorkingStep.ErrorOccured;
                        }
                        if (fVoltage > LIGHT_ON_LOAD_VOLTAGE)
                        {
                            mStep = WorkingStep.ErrorOccured;
                        }
                    }
                    UserCodesysData.RobotInfomation mPLCInfo = new UserCodesysData.RobotInfomation();
                    mPLCInfo = mPLCData.mReceivedRobotInfomation;

                    //mOutputControl.Bit64 |= 0x00000001;

                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.SensorPowerOff;
                    break;
                case WorkingStep.SensorPowerOff:

                    //mOutputControl.Bit64 |= 0x00000001;

                    data = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, data);
                    mStep = WorkingStep.ReleasePowerModeSignal;
                    break;
                case WorkingStep.ReleasePowerModeSignal:

                    //mOutputControl.Bit64 |= 0x00000001;

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
