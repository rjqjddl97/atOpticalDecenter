using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using RecipeManager;


namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class OpticalInspectStep : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public OpticalInspectStep()
        {
            //Do some init here.
            ErrorStepString = "투광 LED 검사";
        }

        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            CheckShortDistanceInspect,
            MoveInspect1Pos,
            WaitInspect1Pos,
            InspectLEDSpot1,
            WaitGrabImageSpot1,
            AnalysisLEDSpot1,
            MoveInspect2Pos,
            WaitInspect2Pos,
            InspectLedSpot2,
            WaitGrabImageSpot2,
            AnalysisLEDSpot2,
            CalcurateData,
            MoveProductInspectPos,
            WaitInspectPos,
            CheckOutputSignal,
            SaveNomalInpsectDistance,

            SetNDFilter,
            ReleaseNDFilter,
            ErrorOccured,
        }
        private void Run()
        {
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
                            if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000008))
                            {
                                mInspectResultData.InspectParameterInitial(mWorkParam._ProductDistance, mWorkParam._LEDInspectionShortDistance, _ImageResolution_H, _ImageResolution_V, fOnePixelResolution);
                                mStep = WorkingStep.CheckShortDistanceInspect;
                            }
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.CheckShortDistanceInspect:
                    if (mWorkParam._ProductDistance >= mWorkParam._LEDInspectionShortDistance)
                    {
                        mStep = WorkingStep.MoveInspect1Pos;
                    }
                    else
                        mStep = WorkingStep.Idle;
                    break;
                case WorkingStep.MoveInspect1Pos:
                    if (mWorkParam.InspectionPositions.Count > 0)
                    {
                        for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                        {
                            if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
                            {
                                byte[] data = new byte[32];
                                UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                                mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                mCmdPosMove.Yaw = (double)mWorkParam.InspectionPositions[i].PositionFilterR;

                                if (bCalibrationActive)
                                {
                                    mPrePos.X = mCmdPosMove.X;
                                    mPrePos.Y1 = mCmdPosMove.Y;
                                    mPrePos.Y2 = mCmdPosMove.Z;
                                    mPrePos.Z = mCmdPosMove.Roll;
                                    mPrePos.FZ = mCmdPosMove.Pitch;
                                    mPrePos.FR = mCmdPosMove.Yaw;

                                    mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                    mCmdPosMove.X += (double)mDeltaPos.X;
                                    mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                    mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                    mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                    mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                    mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                }

                                if (fMoveVelocity < 1f)
                                    fMoveVelocity = 100f;
                                if (fMoveAcceleration < 1f)
                                    fMoveAcceleration = 1000f;

                                mCmdPosMove.Speed = (double)fMoveVelocity;
                                mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                data = mCmdPosMove.GetData();
                                // Send Cordinate Move Command
                                mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, data);
                                mStep = WorkingStep.WaitInspect1Pos;
                                mRetryCount = 0;
                                mTimeChecker.SetTime(D_PLC_MOTION_READYSIGNAL_WAIT_TIME);
                                break;
                            }
                        }
                    }
                    else
                        mStep = WorkingStep.ErrorOccured;
                    break;
                case WorkingStep.WaitInspect1Pos:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.InspectLEDSpot1;
                    }
                    //else
                    //{
                    //    mRetryCount++;
                    //    if (mRetryCount >= RETRY_LIMIT)
                    //    {
                    //        mRetryCount = 0;
                    //        mStep = WorkingStep.Idle;
                    //    }
                    //    else
                    //    {
                    //        mStep = WorkingStep.WaitInspect1Pos;
                    //    }
                    //}
                    break;
                case WorkingStep.InspectLEDSpot1:
                    if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000050))
                    {
                        TakePicture();
                        mTimeChecker.SetTime(D_WAIT_CAMERA_GRAB_DELAY);
                        mStep = WorkingStep.WaitGrabImageSpot1;
                    }
                    else
                    {
                        if (mTimeChecker.IsTimeOver())
                        {
                            mStep = WorkingStep.Idle;
                        }
                    }
                    break;
                case WorkingStep.WaitGrabImageSpot1:
                    if (mTimeChecker.IsTimeOver() || IsGrabbed)
                    {
                        if (IsGrabbed)
                        {
                            mRetryCount = 0;
                            mStep = WorkingStep.AnalysisLEDSpot1;
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
                                mStep = WorkingStep.InspectLEDSpot1;
                            }
                        }
                    }
                    break;
                case WorkingStep.AnalysisLEDSpot1:
                    LedSpotImageProcess(0);
                    mStep = WorkingStep.MoveInspect2Pos;
                    break;
                case WorkingStep.MoveInspect2Pos:
                    if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000050))
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
                                {
                                    byte[] data = new byte[32];
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                    mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1 + SHORT_DISTANCE_MOVE_CAMERA_OFFSET;
                                    mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    mCmdPosMove.Yaw = (double)mWorkParam.InspectionPositions[i].PositionFilterR;

                                    if (bCalibrationActive)
                                    {
                                        mPrePos.X = mCmdPosMove.X;
                                        mPrePos.Y1 = mCmdPosMove.Y;
                                        mPrePos.Y2 = mCmdPosMove.Z;
                                        mPrePos.Z = mCmdPosMove.Roll;
                                        mPrePos.FZ = mCmdPosMove.Pitch;
                                        mPrePos.FR = mCmdPosMove.Yaw;

                                        mDeltaPos = mCalibrationParam.EmiterCalibratinoDeltaPosition(mPrePos);
                                        mCmdPosMove.X += (double)mDeltaPos.X;
                                        mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                        mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                        mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                        mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                        mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    }
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    data = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    TakePicture();
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, data);
                                    mStep = WorkingStep.WaitInspect2Pos;
                                    mRetryCount = 0;
                                    mTimeChecker.SetTime(D_PLC_MOTION_READYSIGNAL_WAIT_TIME);
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.WaitInspect2Pos:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.InspectLedSpot2;
                    }
                    //else
                    //{
                    //    mRetryCount++;
                    //    if (mRetryCount >= RETRY_LIMIT)
                    //    {
                    //        mRetryCount = 0;
                    //        mStep = WorkingStep.Idle;
                    //    }
                    //    else
                    //    {
                    //        mStep = WorkingStep.WaitInspect1Pos;
                    //    }
                    //}
                    break;
                case WorkingStep.InspectLedSpot2:
                    if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000050))
                    {
                        TakePicture();
                        mTimeChecker.SetTime(D_WAIT_CAMERA_GRAB_DELAY);
                        mStep = WorkingStep.WaitGrabImageSpot2;
                    }
                    else
                    {
                        if (mTimeChecker.IsTimeOver())
                        {
                            mStep = WorkingStep.Idle;
                        }
                    }
                    break;
                case WorkingStep.WaitGrabImageSpot2:
                    if (mTimeChecker.IsTimeOver() || IsGrabbed)
                    {
                        if (IsGrabbed)
                        {
                            mRetryCount = 0;
                            mStep = WorkingStep.AnalysisLEDSpot2;
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
                                mStep = WorkingStep.InspectLedSpot2;
                            }
                        }
                    }
                    break;
                case WorkingStep.AnalysisLEDSpot2:
                    LedSpotImageProcess(1);

                    mStep = WorkingStep.CalcurateData;
                    break;
                case WorkingStep.CalcurateData:
                    // 투광 LED 편심, 발산각, ND Filter의 감쇄율 계산 및 합격 여부 판단.
                    LedSpotCalcuate();
                    mStep = WorkingStep.MoveProductInspectPos;
                    break;

                case WorkingStep.MoveProductInspectPos:
                    if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000050))
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    byte[] data = new byte[32];
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    RecipeManager.CalibrationParams.Position mPrePos = new RecipeManager.CalibrationParams.Position();
                                    RecipeManager.CalibrationParams.Position mDeltaPos = new RecipeManager.CalibrationParams.Position();

                                    mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                                    mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    mCmdPosMove.Yaw = (double)(double)mInspectResultData.fND_FilterAngle;

                                    if (bCalibrationActive)
                                    {
                                        mPrePos.X = mCmdPosMove.X;
                                        mPrePos.Y1 = mCmdPosMove.Y;
                                        mPrePos.Y2 = mCmdPosMove.Z;
                                        mPrePos.Z = mCmdPosMove.Roll;
                                        mPrePos.FZ = mCmdPosMove.Pitch;
                                        mPrePos.FR = mCmdPosMove.Yaw;

                                        mDeltaPos = mCalibrationParam.InspectCalibratinoDeltaPosition(mPrePos);
                                        mCmdPosMove.X += (double)mDeltaPos.X;
                                        mCmdPosMove.Y += (double)mDeltaPos.Y1;
                                        mCmdPosMove.Z += (double)mDeltaPos.Y2;
                                        mCmdPosMove.Roll += (double)mDeltaPos.Z;
                                        mCmdPosMove.Pitch += (double)mDeltaPos.FZ;
                                        mCmdPosMove.Yaw += (double)mDeltaPos.FR;
                                    }
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    data = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    TakePicture();
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, data);
                                    mStep = WorkingStep.Idle;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.SetNDFilter:
                    break;
                case WorkingStep.ReleaseNDFilter:
                    if (Convert.ToBoolean(mPLCInfomationData.mStatus & 0x00000050))
                    {
                        if (InspectPos.Count > 0)
                        {
                            for (int i = 0; i < InspectPos.Count; i++)
                            {
                                if (InspectPos[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    byte[] data = new byte[32];
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    mCmdPosMove.X = (double)InspectPos[i].PositionX;
                                    mCmdPosMove.Y = (double)InspectPos[i].PositionY1;
                                    mCmdPosMove.Z = (double)InspectPos[i].PositionY2;
                                    mCmdPosMove.Roll = (double)InspectPos[i].PositionZ;
                                    mCmdPosMove.Pitch = (double)0f;
                                    mCmdPosMove.Yaw = (double)InspectPos[i].PositionFilterR;
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    data = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, data);
                                    mStep = WorkingStep.Idle;
                                    break;
                                }
                            }
                        }
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
