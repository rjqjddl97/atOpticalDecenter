using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;

namespace atOpticalDecenter.Functions.StepHandler.Correction
{
    public class CorrectionStep1 : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        private static WorkingStep mPreStep = WorkingStep.Idle;
        
        public CorrectionStep1()
        {
            //Do some init here.
            ErrorStepString = "교정 모드 진입";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            SensorPowerOn,
            WaitStablePower,
            EnterTestMode,
            RequestSelfTest,
            RequestModelInfo,
            RequestVersionInfo,
            CheckModel,
            RequestEmiterMaxPowerDAC,
            RequestEmiterMaxBandWidth,
            RequestMeasureVref,
            RequestApplyVref,
            SetDetectWhiteObject,
            RequestMeasureWhiteObjectADC,
            SetDetectBlackObject,
            RequestMeasureBlackObjectADC,
            CheckBlackToleranceModel,
            RequestApplyBlackToleranceParam,
            ReleaseTestMode,
            SensorPowerOff,
            ResponseWaitTime,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] iodata = new byte[4];
            //byte[] SeData = new byte[PhotoInsideData.HEADER_LENGTH];
            //byte[] SeOneData = new byte[PhotoInsideData.HEADER_LENGTH + 1];
            //UserCodesysData.DigitalOutputControl mOutputControl = new UserCodesysData.DigitalOutputControl();
            //UserCodesysData.RobotInfomation mPLCInfo = new UserCodesysData.RobotInfomation();
            
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
                        if (mCodesysPLC.IsConnected() && mInspectSensor.IsOpen())
                        {
                            if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000008))
                                mStep = WorkingStep.SensorPowerOn;
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.SensorPowerOn:
                    //mOutputControl.Bit64 |= 0x00000001;               // Photo Sensor Power On Signal Set
                    iodata = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    mTimeChecker.SetTime(PHOTO_SENSOR_POWER_STABLE_TIME);
                    mStep = WorkingStep.WaitStablePower;
                    break;
                case WorkingStep.WaitStablePower:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.EnterTestMode;
                    }
                    break;
                case WorkingStep.EnterTestMode:
                    SeOneData = mPhotoData.mSendSetTestMode.GetData(mPhotoData.ON);                                     
                    mInspectSensor.SendData(SeOneData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.EnterTestMode;
                    break;
                case WorkingStep.RequestSelfTest:
                    // 테스트 모드 진입 응답 확인!!                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.TEST_PBIT;
                    SeData = mPhotoData.mSendCommand.GetData();                                      
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestSelfTest;
                    break;
                case WorkingStep.RequestModelInfo:
                    // 모델 정보 입력 응답 확인!!                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.GET_MODEL;
                    SeData = mPhotoData.mSendCommand.GetData();                                   
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestModelInfo;
                    break;
                case WorkingStep.RequestVersionInfo:
                    // 버전 정보 입력 응답 확인!!                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.GET_VERSION;
                    SeData = mPhotoData.mSendCommand.GetData();                                     
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestVersionInfo;
                    break;
                case WorkingStep.CheckModel:
                    // 모델이 BGS 반사형인지 확인!!
                    //
                    mStep = WorkingStep.RequestEmiterMaxPowerDAC;
                    //mStep = WorkingStep.ReleaseTestMode; //BGS가 아닐경우 TestMode 해제.
                    break;
                case WorkingStep.RequestEmiterMaxPowerDAC:
                    SeOneData = mPhotoData.mSendTestEmitPower.GetData(mPhotoData.ON);                                  
                    mInspectSensor.SendData(SeOneData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestEmiterMaxPowerDAC;
                    break;
                case WorkingStep.RequestEmiterMaxBandWidth:
                    // 투광 파워 DAC 최대 제어요청 응답 확인!!
                    SeOneData = mPhotoData.mSendTestEmitPulse.GetData(mPhotoData.ON);
                    mInspectSensor.SendData(SeOneData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestEmiterMaxBandWidth;
                    break;
                case WorkingStep.RequestMeasureVref:
                    // 투광 폭 최대 제어요청 응답 확인!!                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.GET_REF_VOLTAGE;
                    SeData = mPhotoData.mSendCommand.GetData();
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestMeasureVref;
                    break;
                case WorkingStep.RequestApplyVref:
                    // Vref 측정 요청 응답 확인!!
                    byte[] Se4Data = new byte[PhotoInsideData.HEADER_LENGTH + 4];                    
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.SET_REF_VOLTAGE;
                    Se4Data = mPhotoData.mSendRefVoltage.GetData(1,2);       // Vref 평균값 인자로 입력 필요!                
                    mInspectSensor.SendData(Se4Data);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestApplyVref;
                    break;
                case WorkingStep.SetDetectWhiteObject:
                    // 백색지 검출체 셋팅 메시지 팝업 출력 이벤트!!                                        
                    mStep = WorkingStep.RequestMeasureWhiteObjectADC;
                    break;
                case WorkingStep.RequestMeasureWhiteObjectADC:                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.GET_BW_MEASURE1;
                    SeData = mPhotoData.mSendCommand.GetData();
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestMeasureWhiteObjectADC;
                    break;
                case WorkingStep.SetDetectBlackObject:
                    // 백색지 검출체 ADC 측정 요청 응답 확인!!                                        
                    mStep = WorkingStep.RequestMeasureBlackObjectADC;
                    break;
                case WorkingStep.RequestMeasureBlackObjectADC:                    
                    mPhotoData.mSendCommand.mHeader.DataLength = 0;
                    mPhotoData.mSendCommand.mHeader.Command = (byte)PhotoInsideData.COMMAND_MSG.GET_BW_MEASURE2;
                    SeData = mPhotoData.mSendCommand.GetData();
                    mInspectSensor.SendData(SeData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestMeasureBlackObjectADC;
                    break;
                case WorkingStep.CheckBlackToleranceModel:
                    // 흑색지 검출체 ADC 측정 요청 응답 확인!!  
                    // 흑색 오차 파라미터 적용 모델 확인 (미 적용 모델은 테스트 모드 해제)
                    mStep = WorkingStep.RequestApplyBlackToleranceParam;
                    break;
                case WorkingStep.RequestApplyBlackToleranceParam:
                    byte[] SeMultiData = new byte[PhotoInsideData.HEADER_LENGTH + 16];
                    SeMultiData = mPhotoData.mSendBWParameter.GetData(SeBWParams);
                    //mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);                    
                    mInspectSensor.SendData(SeMultiData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.RequestApplyBlackToleranceParam;
                    break;
                case WorkingStep.ReleaseTestMode:
                    // 흑색 오차 파라미터 적용 응답 확인!!                      
                    SeOneData = mPhotoData.mSendSetTestMode.GetData(mPhotoData.OFF);                                     
                    mInspectSensor.SendData(SeOneData);
                    mTimeChecker.SetTime(PHOTO_SENSOR_RESPONSE_WAIT_TIMEOUT);
                    mStep = WorkingStep.ResponseWaitTime;
                    mPreStep = WorkingStep.ReleaseTestMode;                    
                    break;
                case WorkingStep.SensorPowerOff:
                    //mOutputControl.Bit64 |= 0x00000001;               // Photo Sensor Power Off Signal Set
                    iodata = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    mStep = WorkingStep.Idle;
                    mPreStep = WorkingStep.Idle;
                    break;
                case WorkingStep.ResponseWaitTime:
                    if (mTimeChecker.IsTimeOver())
                    {
                        if (mPreStep == WorkingStep.EnterTestMode)
                        {
                            mStep = WorkingStep.RequestSelfTest;
                        }
                        else if (mPreStep == WorkingStep.RequestSelfTest)
                        {
                            mStep = WorkingStep.RequestModelInfo;
                        }
                        else if (mPreStep == WorkingStep.RequestModelInfo)
                        {
                            mStep = WorkingStep.RequestVersionInfo;
                        }
                        else if (mPreStep == WorkingStep.RequestVersionInfo)
                        {
                            mStep = WorkingStep.CheckModel;
                        }
                        else if (mPreStep == WorkingStep.RequestEmiterMaxPowerDAC)
                        {
                            mStep = WorkingStep.RequestEmiterMaxBandWidth;
                        }
                        else if (mPreStep == WorkingStep.RequestEmiterMaxBandWidth)
                        {
                            mStep = WorkingStep.RequestMeasureVref;
                        }
                        else if (mPreStep == WorkingStep.RequestMeasureVref)
                        {
                            mStep = WorkingStep.RequestApplyVref;
                        }
                        else if (mPreStep == WorkingStep.RequestApplyVref)
                        {
                            mStep = WorkingStep.SetDetectWhiteObject;
                        }
                        else if (mPreStep == WorkingStep.SetDetectWhiteObject)
                        {
                            mStep = WorkingStep.RequestMeasureWhiteObjectADC;
                        }
                        else if (mPreStep == WorkingStep.RequestMeasureWhiteObjectADC)
                        {
                            mStep = WorkingStep.RequestMeasureBlackObjectADC;
                        }
                        else if (mPreStep == WorkingStep.RequestMeasureBlackObjectADC)
                        {
                            mStep = WorkingStep.CheckBlackToleranceModel;
                        }
                        else if (mPreStep == WorkingStep.RequestApplyBlackToleranceParam)
                        {
                            mStep = WorkingStep.ReleaseTestMode;
                        }
                        else if (mPreStep == WorkingStep.ReleaseTestMode)
                        {
                            mStep = WorkingStep.SensorPowerOff;
                        }
                        else
                        {
                            mStep = WorkingStep.ErrorOccured;
                        }
                    }
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
