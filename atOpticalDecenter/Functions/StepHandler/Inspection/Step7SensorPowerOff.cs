using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step7SensorPowerOff : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public Step7SensorPowerOff()
        {
            //Do some init here.
            //ErrorStepString = "Sensor Power On";
            ErrorStepString = "센서 전원 Off";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            SensorPowerOff,
            WaitSignalCommandTime,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] data = new byte[8];
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
                            _DelayTimerCounter = SENSOR_POWER_STABLE_TIME;
                            mStep = WorkingStep.SensorPowerOff;
                        }
                    }
                    break;
                case WorkingStep.SensorPowerOff:

                    data = mRemoteIOCtrl.mRemoteIOCtrl.Output1byteCommand(mRemoteIOCtrl.mRemoteIOCtrl.DrvID[0], ARMLibrary.SerialCommunication.Data.ARMData.OUTPUT_CONTROL_MAP.Output0, (ushort)0x0000);
                    mRemoteIOCtrl.SendData(data);

                    mTimeChecker.SetTime(_DelayTimerCounter);
                    mStep = WorkingStep.WaitSignalCommandTime;
                    

                    break;
                case WorkingStep.WaitSignalCommandTime:
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
