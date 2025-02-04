using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atOpticalDecenter.Functions.StepHandler.Base
{
    interface IStepHandler
    {
        void Init();
        StepHandlerBase.RetType Execute();
        StepHandlerBase.RetType GetStatus();
        bool ClearError();
        int GetAlarmNumber();
    }
}
