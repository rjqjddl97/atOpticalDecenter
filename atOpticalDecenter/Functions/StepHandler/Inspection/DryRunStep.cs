using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class DryRunStep : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public DryRunStep()
        {
            //Do some init here.
            ErrorStepString = "DryRun 테스트 모드";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,            
            MoveInspectPos,
            CheckInposition,
            ExcuteCameraGrab,
            WaitGrabImage,
            MoveInspectPos2,
            CheckInposition2,
            ExcuteCameraGrab2,
            WaitGrabImage2,
            CalculateData,
            MoveFilterPos,
            CheckInpositionFilter,
            InspectResult,
            ErrorOccured,
        }
        private void Run()
        {
            byte[] posdata = new byte[32];
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
                        ///*
                        if (mMotionDrvCtrl.IsOpen())
                        {
                            if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                            {
                                byte[] data = new byte[100];
                                data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[0], Convert.ToInt32(10 * mSystemParam._motionParams.MM2PulseRatioX));
                                mMotionDrvCtrl.SendData(data);
                                data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                                mMotionDrvCtrl.SendData(data);
                                mStep = WorkingStep.MoveInspectPos;
                                mRobotInformation.mStatus = 0x00000002;
                            }
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                        //*/
                        //mInspectResultData.InspectParameterInitial(mWorkParam._ProductDistance,mWorkParam._LEDInspectionShortDistance, _ImageResolution_H, _ImageResolution_V, fOnePixelResolution);
                        //mStep = WorkingStep.ExcuteCameraGrab;
                    }
                    break;
                case WorkingStep.MoveInspectPos:
                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        //byte[] data = new byte[8];
                        //data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        //mMotionDrvCtrl.SendData(data);
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.CheckInposition;
                        /*
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    //UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    //RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    //RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                    //mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    //mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY;
                                    //mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    //mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    //mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    //mCmdPosMove.Yaw = (double)mWorkParam.InspectionPositions[i].PositionFilterR;

                                    //if (bCalibrationActive)
                                    //{
                                    //    mPrePos.X = mCmdPosMove.X;
                                    //    mPrePos.Y1 = mCmdPosMove.Y;
                                    //    mPrePos.Y2 = mCmdPosMove.Z;
                                    //    mPrePos.Z = mCmdPosMove.Roll;
                                    //    mPrePos.FZ = mCmdPosMove.Pitch;
                                    //    mPrePos.FR = mCmdPosMove.Yaw;

                                    //    mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                    //    mCmdPosMove.X += (double)mDeltaPos.X;
                                    //    mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                    //    mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                    //    mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                    //    mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                    //    mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    //}

                                    //if (fMoveVelocity < 1f)
                                    //    fMoveVelocity = 10f;
                                    //if (fMoveAcceleration < 1f)
                                    //    fMoveAcceleration = 10f;
                                    //mCmdPosMove.Speed = (double)fMoveVelocity;
                                    //mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    //if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    //if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    //posdata = mCmdPosMove.GetData();
                                    //// Send Cordinate Move Command
                                    ////mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                    mStep = WorkingStep.CheckInposition;
                                    break;
                                }
                            }
                        }
                        */
                    }
                    break;
                case WorkingStep.CheckInposition:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.ExcuteCameraGrab;
                    }  
                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    //{
                    //    mStep = WorkingStep.ExcuteCameraGrab;                        
                    //}
                    break;
                case WorkingStep.ExcuteCameraGrab:
                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        byte[] data = new byte[100];
                        data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[0], Convert.ToInt32(50 * mSystemParam._motionParams.MM2PulseRatioX));
                        mMotionDrvCtrl.SendData(data);
                        data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        mMotionDrvCtrl.SendData(data);
                        mStep = WorkingStep.MoveInspectPos2;
                        mRobotInformation.mStatus = 0x00000002;
                    }

                    //TakePicture();                                        
                    //mTimeChecker.SetTime(D_WAIT_CAMERA_GRAB_DELAY);
                    //mStep = WorkingStep.WaitGrabImage;
                    break;
                case WorkingStep.WaitGrabImage:
                    /*
                    if (mTimeChecker.IsTimeOver() || IsGrabbed)
                    {
                        if (IsGrabbed)
                        {
                            mRetryCount = 0;
                            //LedSpotImageProcess(0);
                            //mInspectResultData.mFirstLedSpot = mFirstLedSpot;
                            //mInspectResultData.CalculateLedBlob(0);
                            mStep = WorkingStep.Idle;
                        }
                        else
                        {
                            mRetryCount++;
                            if (mRetryCount >= RETRY_LIMIT)
                            {
                                mRetryCount = 0;
                                mStep = WorkingStep.Idle;
                            }
                            else
                            {
                                mStep = WorkingStep.ExcuteCameraGrab;
                            }
                        }
                    }
                    */                     
                    break;
                case WorkingStep.MoveInspectPos2:
                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        //byte[] data = new byte[8];
                        //data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        //mMotionDrvCtrl.SendData(data);
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.CheckInposition2;
                        /*
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {

                                    ////UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    //RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    //RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();
                                    //mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    //mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1+100;
                                    //mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    //mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    //mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    //mCmdPosMove.Yaw = (double)mWorkParam.InspectionPositions[i].PositionFilterR;

                                    //if (bCalibrationActive)
                                    //{
                                    //    mPrePos.X = mCmdPosMove.X;
                                    //    mPrePos.Y1 = mCmdPosMove.Y;
                                    //    mPrePos.Y2 = mCmdPosMove.Z;
                                    //    mPrePos.Z = mCmdPosMove.Roll;
                                    //    mPrePos.FZ = mCmdPosMove.Pitch;
                                    //    mPrePos.FR = mCmdPosMove.Yaw;

                                    //    mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                    //    mCmdPosMove.X += (double)mDeltaPos.X;
                                    //    mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                    //    mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                    //    mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                    //    mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                    //    mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    //}
                                    //if (fMoveVelocity < 1f)
                                    //    fMoveVelocity = 10f;
                                    //if (fMoveAcceleration < 1f)
                                    //    fMoveAcceleration = 10f;
                                    //mCmdPosMove.Speed = (double)fMoveVelocity;
                                    //mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    //if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    //if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    //posdata = mCmdPosMove.GetData();
                                    //// Send Cordinate Move Command
                                    //mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                    //mStep = WorkingStep.CheckInposition2;
                                    break;
                                }
                            }
                        }
                        */
                    }
                    break;
                case WorkingStep.CheckInposition2:

                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.ExcuteCameraGrab2;
                    }
                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    //{                       
                    //    mStep = WorkingStep.ExcuteCameraGrab2;
                    //}
                    break;
                case WorkingStep.ExcuteCameraGrab2:
                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        byte[] data = new byte[100];
                        data = mMotionDrvCtrl.mDrvCtrl.MoveTargetPositionSendData((byte)mMotionDrvCtrl.mDrvCtrl.DrvID[0], Convert.ToInt32(0 * mSystemParam._motionParams.MM2PulseRatioX));
                        mMotionDrvCtrl.SendData(data);
                        data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        mMotionDrvCtrl.SendData(data);
                        mStep = WorkingStep.CalculateData;
                        mRobotInformation.mStatus = 0x00000002;
                    }
                    //TakePicture();                    
                    //mTimeChecker.SetTime(D_WAIT_CAMERA_GRAB_DELAY);
                    //mStep = WorkingStep.WaitGrabImage2;
                    break;
                case WorkingStep.WaitGrabImage2:
                    /*
                    if (mTimeChecker.IsTimeOver() || IsGrabbed)
                    {
                        if (IsGrabbed)
                        {
                            mRetryCount = 0;
                            LedSpotImageProcess(1);
                            mInspectResultData.mFinalLedSpot = mFinalLedSpot;
                            mInspectResultData.CalculateLedBlob(1);
                            mStep = WorkingStep.CalculateData;
                        }
                        else
                        {
                            mRetryCount++;
                            if (mRetryCount >= RETRY_LIMIT)
                            {
                                mRetryCount = 0;
                                mStep = WorkingStep.ErrorOccured;
                            }
                            else
                            {
                                mStep = WorkingStep.ExcuteCameraGrab2;
                            }
                        }
                    }
                    */
                    break;
                case WorkingStep.CalculateData:
                    //mInspectResultData.CalculateOpticalInspect(mWorkParam._ProductType);
                    //mInspectResultData.bOpticalInspect = true;
                    mStep = WorkingStep.MoveFilterPos;
                    break;
                case WorkingStep.MoveFilterPos:

                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        //byte[] data = new byte[8];
                        //data = mMotionDrvCtrl.mDrvCtrl.MoveAbsoluteCommand(129);
                        //mMotionDrvCtrl.SendData(data);
                        mTimeChecker.SetTime(_DelayTimerCounter);
                        mStep = WorkingStep.CheckInpositionFilter;
                    }

                    //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))
                    {
                        //if (mWorkParam.InspectionPositions.Count > 0)
                        //{
                        //    for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                        //    {
                        //        if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                        //        {                                    
                        //            UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                        //            mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                        //            mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                        //            mCmdPosMove.Z = (double)300;
                        //            mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                        //            mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                        //            mCmdPosMove.Yaw = (double)mInspectResultData.fND_FilterAngle;
                        //            if (fMoveVelocity < 1f)
                        //                fMoveVelocity = 10f;
                        //            if (fMoveAcceleration < 1f)
                        //                fMoveAcceleration = 10f;
                        //            mCmdPosMove.Speed = (double)fMoveVelocity;
                        //            mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                        //            if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                        //            if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                        //            posdata = mCmdPosMove.GetData();
                        //            // Send Cordinate Move Command
                        //            mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                        //            mStep = WorkingStep.CheckInpositionFilter;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case WorkingStep.CheckInpositionFilter:

                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.InspectResult;
                    }
                    //if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    //{
                    //    mStep = WorkingStep.InspectResult;
                    //}
                    break;
                case WorkingStep.InspectResult:
                    if (Convert.ToBoolean(mRobotInformation.mStatus & 0x00000042))
                    {
                        mInspectResultData.bTotalResult = true;
                        mStep = WorkingStep.Idle;
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
