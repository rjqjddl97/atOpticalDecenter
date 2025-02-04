﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ARMLibrary.SerialCommunication.Data;
using ARMLibrary.SerialCommunication.DataProcessor;
using ARMLibrary.SerialCommunication.Control;
using System.Collections;

namespace ARMLibrary.SerialCommunication.Control
{
    public class SerialProcessEngine
    {
        public enum SerialEngineStep
        {
            Idle,
            RequestPeriodData
        };
        public enum SerialReceiveStep
        {
            Idle,
            Start,
            End
        };
        private Thread engine;

        private const int EngineSleepTime = 250;
        private const int ReceiveBuffSize = 4096;
        private SerialEngineStep mSerialEngineStep;
        private List<byte[]> mContinuousCheckList = new List<byte[]>();
        private Queue<byte[]> mCommandList = new Queue<byte[]>();
        private Queue<byte[]> mDataTransferList = new Queue<byte[]>();
        //private double CurrentcountForDataTransfer = 0;
        //private double MaximumCountForDataTransfer = 0;        
        public Queue<byte[]> mSendCommand = new Queue<byte[]>();
        private bool IsDequeueData = false;                   
        private bool IsEnqueueData = false;
        private SerialReceiveStep mReceiveStep;

        private byte[] ReceivePacketBuff = new byte[ReceiveBuffSize];
        //private int ReceiveCountIndex;
        public SerialHandler m_SerialHandler;
        public ARMData m_ARMDataCtrl;        
        //public event Action<byte[]> ReceivedDataEvent;
        //public event Action<byte[]> ParsedDataReceivedEvent;
        public event Action<ARMData> ReceiveARMData;

        public delegate void RequestData(byte[] buffer, int offset, int count);
        //public delegate void ReceiveMT4xData(MT4xPanelMeta data);
        public event RequestData RequestDataEventHandler;
        public bool IsReceiveStart { get; set; }

        public bool IsConnected { get; set; }
        public SerialProcessEngine()
        {
            IsConnected = false;
            m_SerialHandler = new SerialHandler();
            m_ARMDataCtrl = new ARMData();
            mSerialEngineStep = SerialEngineStep.Idle;
            mReceiveStep = SerialReceiveStep.Idle;
            //m_SerialHandler.ReceivedQueueDataEventHandler += ReceiveQueueData;
            InitCheckDatas();
            Array.Clear(ReceivePacketBuff, 0x00, ReceiveBuffSize);
            //ReceiveCountIndex = 0;
            engine = new Thread(Run);
            engine.Start();
        }
        ~SerialProcessEngine()
        {
            engine.Abort();
        }

        public void PauseEngine()
        {
            mSerialEngineStep = SerialEngineStep.Idle;
        }

        public void StartEngine()
        {
            mSerialEngineStep = SerialEngineStep.RequestPeriodData;
        }
        public void StopEngine()
        {
            engine.Abort();            
        }
        public void SwitchMode(SerialEngineStep mMode)
        {
            if (mMode != SerialEngineStep.Idle)
                mSerialEngineStep = mMode;
        }
        private void InitCheckDatas()
        {
            //Get Info. 주기적 요청.
            mContinuousCheckList.Add(m_ARMDataCtrl.GetSettingInputs(5));
            mContinuousCheckList.Add(m_ARMDataCtrl.GetSettingOutputs(5));
            //mContinuousCheckList.Add(RobotDataHandler.GetCommand(RobotData.ROBOT_MSG.MSG_GET_INFO)[0]);
        }
        public void ParsingData(byte[] data)
        {
            try
            {
                ushort CheckSum = 0, iCRC16 = 0;
                CheckSum = CRC.CRC16(data, data.Length - 2);
                iCRC16 = (ushort)data[data.Length - 1];
                iCRC16 = (ushort)((iCRC16 << 8) | data[data.Length - 2]);
                //ARMData.CommandMassege reCommMassege = new ARMData.CommandMassege();
                if (CheckSum == iCRC16)
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputs)
                    {
                        m_ARMDataCtrl.ReceiveSetInput(data);
                    }
                    else if(data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadCoils)
                    {
                        m_ARMDataCtrl.ReceiveSetOutput(data);
                    }

                    //reCommMassege = m_ARMDataCtrl.GetRequestedCommand();
                    //if (reCommMassege == ARMData.CommandMassege.MSG_INPUT)
                    //    m_ARMDataCtrl.ReceiveSetInput(data);
                    //else if (reCommMassege == ARMData.CommandMassege.MSG_OUTPUT)
                    //    m_ARMDataCtrl.ReceiveSetOutput(data);
                    //else if (reCommMassege == ARMData.CommandMassege.MSG_PRODUCT_INFO)
                    //    m_ARMDataCtrl.ReceiveSetProductInfo(data);
                    //else if (reCommMassege == ARMData.CommandMassege.MSG_EXT_UNIT_INFO)
                    //    m_ARMDataCtrl.ReceiveSetExtProductInfo(data);
                    //else if (reCommMassege == ARMData.CommandMassege.MSG_DEVICE_INFO)
                    //    m_ARMDataCtrl.ReceiveSetDeviceInfo(data);
                }
                else
                {
                    // CheckSum Error( Modulo256 Error )
                }              
            }
            catch (Exception)
            {
                //Log.LogManager.AddSystemLog(Log.Log.LogType.Error, "CommunicateEngine::ParsingData -> Received data parsing error.");
            }
        }
        public void SendCommand(byte[] data)
        {
            byte[] Sedata = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Sedata, 0, data.Length);
            mDataTransferList.Enqueue(Sedata);
        }
        public void SendData(byte[] data)
        {
            byte[] Sedata = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Sedata, 0, data.Length);
            mDataTransferList.Enqueue(Sedata);
        }
        public void ReceiveQueueData(byte[] data, int length)
        {
            for (int i = 0; i < length; i++)
            {
                m_SerialHandler.mQueue.Push(data[i]);
            }
        }
        public void ReceivePacket()
        {
            int i;
            byte ReData = 0;
            UInt32 buffsize = m_SerialHandler.mQueue.GetFilledSize();
            if (buffsize != 0)
            {                
                int ReCounter = 0;
                for (i = 0; i < buffsize; i++)
                {
                    m_SerialHandler.mQueue.Pop(ref ReData);
                    if ((IsReceiveStart == false) && ((ReData == 0x05) || (ReData == 0x06)))
                    {
                        IsReceiveStart = true;
                        ReCounter = 0;
                    }
                    if (IsReceiveStart)
                    {
                        ReceivePacketBuff[ReCounter] = ReData;
                        ReCounter++;
                    }

                    if (ReCounter > 2)
                    {
                        if (ReceivePacketBuff[1] > (byte)ModbusRTU.FunctionCodes.Exception)
                        {
                            if (ReCounter == 5)
                            {
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                            else if (ReCounter > 5)
                            {
                                for (int j = 0; j < ReCounter; j++) ReceivePacketBuff[j] = 0;
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                        }
                        else if ((ReceivePacketBuff[1] != (byte)ModbusRTU.WriteFunctionCodes.WriteSingleCoil) && (ReceivePacketBuff[1] != (byte)ModbusRTU.WriteFunctionCodes.WriteSingleRegister) && (ReceivePacketBuff[1] != (byte)ModbusRTU.MultipleWriteFunctionCodes.WriteMultipleCoils) && (ReceivePacketBuff[1] != (byte)ModbusRTU.MultipleWriteFunctionCodes.WriteMultipleRegisters))
                        {
                            if (ReCounter == ReceivePacketBuff[2] + 5)
                            {
                                byte[] MainData = new byte[ReCounter];
                                Buffer.BlockCopy(ReceivePacketBuff, 0, MainData, 0, ReCounter);
                                ParsingData(MainData);
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                            else if (ReCounter > ReceivePacketBuff[2] + 5)
                            {
                                for (int j = 0; j < ReCounter; j++) ReceivePacketBuff[j] = 0;
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                        }
                        else if ((ReceivePacketBuff[1] == (byte)ModbusRTU.MultipleWriteFunctionCodes.WriteMultipleRegisters) || (ReceivePacketBuff[1] == (byte)ModbusRTU.WriteFunctionCodes.WriteSingleCoil) || (ReceivePacketBuff[1] == (byte)ModbusRTU.WriteFunctionCodes.WriteSingleRegister))
                        {
                            if (ReCounter == 8)
                            {
                                //ParsingData(MainData);
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                            else if (ReCounter > 8)
                            {
                                for (int j = 0; j < ReCounter; j++) ReceivePacketBuff[j] = 0;
                                ReCounter = 0;
                                IsReceiveStart = false;
                            }
                        }                        
                    }
                }
            }
        }
        private void Run()
        {
            byte[] data = null;
            int mContinuousCheckIndex = 0;
            while (true)
            {
                try
                {
                    if (!IsConnected)
                    {
                        mCommandList.Clear();
                        mDataTransferList.Clear();
                        //IsEnqueueData = IsDequeueData = false;
                        Thread.Sleep(EngineSleepTime);
                        continue;
                    }

                    if (IsEnqueueData && IsDequeueData)
                    {
                        mDataTransferList.Clear();
                        //IsEnqueueData = IsDequeueData = false;
                        
                    }
                    // receive Data Packet Parsor 구문 추가 필요.
                    if (m_SerialHandler.mQueue.GetFilledSize() != 0)
                    {
                        ReceivePacket();
                    }
                    /////////////////////////////////////////////
                    switch (mSerialEngineStep)
                    {
                        case SerialEngineStep.Idle:
                            //Do nothing
                            mCommandList.Clear();
                            mDataTransferList.Clear();
                            break;

                        case SerialEngineStep.RequestPeriodData:
                            if ((mDataTransferList.Count != 0) && !IsEnqueueData)
                            {
                                IsDequeueData = true;
                                data = mDataTransferList.Dequeue();
                                if (mDataTransferList.Count == 0)
                                {
                                    IsDequeueData = false;                                    
                                }
                            }
                            else if (mCommandList.Count != 0)
                            {
                                data = mCommandList.Dequeue();
                            }
                            else if (mContinuousCheckList.Count != 0)
                            {
                                data = mContinuousCheckList.ElementAt(mContinuousCheckIndex++);
                                if (mContinuousCheckIndex >= mContinuousCheckList.Count)
                                    mContinuousCheckIndex = 0;
                            }

                            break;
                        default:
                            //Do some error action.
                            break;
                    }

                    if ((data != null) && (mSerialEngineStep != SerialEngineStep.Idle))
                    {
                        RequestDataEventHandler?.Invoke(data, 0, data.Length);
                        data = null;
                    }
                }
                catch (Exception)
                {
                    //Log.LogManager.AddSystemLog(Log.Log.LogType.Error, "CommunicateEngine::Run -> Fail to working.");
                }
                Thread.Sleep(EngineSleepTime);
            }
        }
    }
}
