using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step2SensorPowerOn : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        string strstep = string.Empty;
        public Step2SensorPowerOn()
        {
            //Do some init here.
            //ErrorStepString = "Sensor Power On";
            ErrorStepString = "센서 전원 On";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            SensorPowerOn,
            WaitSignalCommandTime,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] data = new byte[32];
            //ErrorStepString = "센서전원 - " + strstep;
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
                            _DelayTimerCounter = SENSOR_POWER_STABLE_TIME;
                            mStep = WorkingStep.SensorPowerOn;
                        }
                    }
                    break;
                case WorkingStep.SensorPowerOn:
                    //strstep = "Power On";
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    data = mRemoteIOCtrl.mRemoteIOCtrl.Output1byteCommand(mRemoteIOCtrl.mRemoteIOCtrl.DrvID[0], ARMLibrary.SerialCommunication.Data.ARMData.OUTPUT_CONTROL_MAP.Output0, (ushort)0xff00);
                    mRemoteIOCtrl.SendData(data);

                    mTimeChecker.SetTime(_DelayTimerCounter);
                    mStep = WorkingStep.WaitSignalCommandTime;


                    break;
                case WorkingStep.WaitSignalCommandTime:
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (mTimeChecker.IsTimeOver())
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
