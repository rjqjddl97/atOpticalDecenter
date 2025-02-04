using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class FinishInspection : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public FinishInspection()
        {
            //Do some init here.
            ErrorStepString = "센서 검사 완료";
        }
        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            DoorOpen,
            WaitDoorOpenStatus,
            CheckDoorOpen,
            MoveEjectPosition,
            CheckInposition,
            ErrorOccured,
        }
        private void Run()
        {            
            UserCodesysData.DigitalOutputControl mOutputControl = new UserCodesysData.DigitalOutputControl();
            UserCodesysData.RobotInfomation mPLCInfo = new UserCodesysData.RobotInfomation();
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
                            if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x0000000b))
                                mStep = WorkingStep.DoorOpen;
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.DoorOpen:
                    byte[] iodata = new byte[4];
                    //mOutputControl.Bit64 |= 0x00000001;               // Door Open Cylinder On Signal Set
                    iodata = mOutputControl.GetData();
                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_OUT_CTRL, iodata);
                    mTimeChecker.SetTime(PLC_OUTPUT_SIGNAL_WAIT_TIME);
                    mStep = WorkingStep.WaitDoorOpenStatus;
                    break;
                case WorkingStep.WaitDoorOpenStatus:
                    if (mTimeChecker.IsTimeOver())
                    {
                        mStep = WorkingStep.CheckDoorOpen;
                    }
                    break;
                case WorkingStep.CheckDoorOpen:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mInputData.Bit64 & 0x00000050))
                    {
                        mStep = WorkingStep.MoveEjectPosition;
                    }
                    break;
                case WorkingStep.MoveEjectPosition:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))
                    {
                        if (mWorkParam.InspectionPositions.Count > 0)
                        {
                            for (int i = 0; i < mWorkParam.InspectionPositions.Count; i++)
                            {
                                if (mWorkParam.InspectionPositions[i].ePositionType == RecipeManager.INSPECTION_POSITION_MODE.POSITION_BASE_MODE)
                                {
                                    byte[] posdata = new byte[32];
                                    // 제품 배출 위치로 이동!!
                                    UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                                    mCmdPosMove.X = (double)mWorkParam.InspectionPositions[i].PositionX;
                                    mCmdPosMove.Y = (double)mWorkParam.InspectionPositions[i].PositionY1;
                                    mCmdPosMove.Z = (double)mWorkParam.InspectionPositions[i].PositionY2;
                                    mCmdPosMove.Roll = (double)mWorkParam.InspectionPositions[i].PositionZ;
                                    mCmdPosMove.Pitch = (double)mWorkParam.InspectionPositions[i].PositionFilterZ;
                                    mCmdPosMove.Yaw = (double)mWorkParam.InspectionPositions[i].PositionFilterR;
                                    if (fMoveVelocity < 1f)
                                        fMoveVelocity = 100f;
                                    if (fMoveAcceleration < 1f)
                                        fMoveAcceleration = 1000f;
                                    mCmdPosMove.Speed = (double)fMoveVelocity;
                                    mCmdPosMove.Acceleration = (double)fMoveAcceleration;
                                    if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                                    if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                                    posdata = mCmdPosMove.GetData();
                                    // Send Cordinate Move Command
                                    mCodesysPLC.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, posdata);
                                    mStep = WorkingStep.CheckInposition;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case WorkingStep.CheckInposition:
                    //if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))
                    {
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
