using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using atOpticalDecenter.Functions.StepHandler.Base;
using AiCControlLibrary.SerialCommunication.Control;
using AiCControlLibrary.SerialCommunication.Data;
using ARMLibrary.SerialCommunication.Control;
using ARMLibrary.SerialCommunication.Data;

namespace atOpticalDecenter.Functions.StepHandler.Inspection
{
    public class InspectionStep2 : StepHandlerBase, IStepHandler
    {
        private WorkingStep mStep = WorkingStep.Idle;
        public InspectionStep2()
        {
            //Do some init here.
            ErrorStepString = "거리 검사 위치 이동";
        }

        private enum WorkingStep
        {
            Idle,
            CheckStatus,
            MoveInspectPos,
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
                            if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000008))
                                mStep = WorkingStep.MoveInspectPos;
                            else
                                mStep = WorkingStep.ErrorOccured;
                        }
                        else
                            mStep = WorkingStep.ErrorOccured;
                    }
                    break;
                case WorkingStep.MoveInspectPos:
                    if (Convert.ToBoolean(mPLCData.mReceivedRobotInfomation.mStatus & 0x00000050))
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
                                    mCmdPosMove.Pitch = (double)InspectPos[i].PositionFilterZ;
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
