using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;

namespace atOpticalDecenter.Functions.StepHandler.Correction
{
    public class CheckProductModel : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public CheckProductModel()
        {
            //Do some init here.
            ErrorStepString = "제품 모델 체크";
        }

        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            UpdateProductData,
            CheckProductModel,
            GenerateSerialNumber,
            SetProductionYears,
            SetProductionMonth,
            SetProductionDay,
            SetProductionCountry,
            SetProductionSerial,
            CheckProductionInfo,
            ErrorOccured,
        }
        private void Run()
        {

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
