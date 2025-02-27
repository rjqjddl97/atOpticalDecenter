using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class Step1JigCheck : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        string strstep = string.Empty;
        public Step1JigCheck()
        {
            //Do some init here.
            ErrorStepString = "Jig Check";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            JigCheck,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] posdata = new byte[32];
            //ErrorStepString = "Jig Check - " + strstep;
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
                            mStep = WorkingStep.JigCheck;
                        }
                    }
                    break;
                case WorkingStep.JigCheck:
                    //strstep = "지그신호 확인중";
                    if (mRobotInformation.mInputData.B0)
                        mStep = WorkingStep.ErrorOccured;

                    if (mRobotInformation.mInputData.B3)                    // Jig Input Ch0 ~ Ch7 Select. 
                    {
                        mStep = WorkingStep.Idle;
                    }
                    else
                    {
                        strstep = "Jig Not Contact or Noting";
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
