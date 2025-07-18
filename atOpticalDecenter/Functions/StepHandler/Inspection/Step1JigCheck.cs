﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;
using LogLibrary;

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
                            mInspectResultData.InspectParameterInitial(mWorkParam._ProductDistance, mWorkParam._LEDInspectionShortDistance, _ImageResolution_H, _ImageResolution_V, fOnePixelResolution);
                            mStep = WorkingStep.JigCheck;
                            _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("Jig 점검 및 검사 설정 초기화"));
                        }
                        else
                        {
                            _log.WriteLog(LogLevel.Fatal, LogClass.InspectStep.ToString(), string.Format("Remote I/O 연결 실패!! "));
                            mStep = WorkingStep.ErrorOccured;
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
                        _log.WriteLog(LogLevel.Info, LogClass.InspectStep.ToString(), string.Format("Jig 확인 완료"));
                    }
                    else
                    {
                        strstep = "Jig Not Contact or Noting";
                        ErrorStepString += strstep;
                        mStep = WorkingStep.ErrorOccured;
                        _log.WriteLog(LogLevel.Error, LogClass.InspectStep.ToString(), string.Format("Jig 확인 실패"));
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
