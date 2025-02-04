using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atOpticalDecenter.Functions.StepHandler
{
    public class WorkingStateInfo
    {
        public enum WorkingType
        {
            Checking,
            CorrectionAndInspection,
            OptionInspection,
            Error
        }

        public WorkingType WorkingStatus = WorkingType.Checking;
        public string CurrentStepName = string.Empty;
        public int CurrentStep = 0;
        public int LastStep = 0;
        public long ElapsedTime = 0;
    }
}
