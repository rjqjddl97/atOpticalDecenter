using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using System.IO.Ports;
using AiCControlLibrary.SerialCommunication;
using AiCControlLibrary.SerialCommunication.Control;
using AiCControlLibrary.SerialCommunication.Data;
using AiCControlLibrary.SerialCommunication.DataProcessor;
using RecipeManager.Params;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;

namespace CustomPages
{
    public partial class MotionControl : DevExpress.XtraEditors.XtraUserControl
    {
        private CommunicationManager _mAiCCommunicationManager = null;
        private AiCData _mAiCData = new AiCData();
        private RecipeManager.MotionParams _MotionParam = null;

        //public AiCData MotionInfo = new AiCData();
        private bool _isRobotEnable = false;
        private bool _isHWEmergencyStop = false;
        private bool _isSWEmergencyStop = false;
        private bool _isError = false;
        private bool _isInposition = false;
        private bool _isMoveStop = false;
        //private bool _isEtherCATReady = false;
        private bool _isServoOn = false;
        private bool _isMoving = false;
        private bool _isHomming = false;
        private bool _isAutomode = false;

        public double _fUserdefineValue = 1;
        public double _fMenualDefineValue = 0;
        public double[] _fdefineStepValue = new double[3];
        public double[] _fdefineVelValue = new double[3];
        public double _fMenualMoveVelocity = 0;

        public double[] _fPresentPosition = new double[6];

        public double _fAcceleration = 1000F;
        public bool _OperatingMode = false;
        public bool _MenaulCoordinateMode = false;
        public bool _isPLCConnected = false;
        public int _MenaulControlMode = 0;
        public int _iCalibrationMode = 0;
        
        public ulong OutputData = 0;
        public bool IsOpenStatus = false;
        public string _SerialPortName = "COM3";
        public int _iBaudrate = 6;
        public int _iDataBit = 1;
        public int _iStopBit = 0;
        public int _iParity = 0;
        public int _iFlowControl = 0;
        public byte _CmdID = 1;
        public bool _IsInitialDrive = false;

        public event Action<RecipeManager.RobotInformation> RobotInfomationUpdatedEvent;

        RecipeManager.CalibrationParams CalibrationParam = new RecipeManager.CalibrationParams();
        RecipeManager.RobotInformation _mRobotInfomation = new RecipeManager.RobotInformation();

        public System.Timers.Timer UpdateTimer = new System.Timers.Timer();

        public int[][] DrvMotionMonitor = new int[4][];

        public MotionControl()
        {
            InitializeComponent();
            initialSystemDefineValue();
        }
        public void initialSystemDefineValue()
        {
            string[] tmpStr = System.IO.Ports.SerialPort.GetPortNames();

            if (tmpStr.Length > 0)
            {
                comboBoxEditComPort.Properties.Items.AddRange(tmpStr);
            }

            comboBoxEditComPort.SelectedIndex = 5;
            comboBoxEditBaudRate.Properties.Items.Add("4800");
            comboBoxEditBaudRate.Properties.Items.Add("9600");
            comboBoxEditBaudRate.Properties.Items.Add("14400");
            comboBoxEditBaudRate.Properties.Items.Add("19200");
            comboBoxEditBaudRate.Properties.Items.Add("38400");
            comboBoxEditBaudRate.Properties.Items.Add("57600");
            comboBoxEditBaudRate.Properties.Items.Add("115200");
            comboBoxEditBaudRate.SelectedIndex = _iBaudrate;
            comboBoxEditDataBit.Properties.Items.Add("7");
            comboBoxEditDataBit.Properties.Items.Add("8");
            comboBoxEditDataBit.SelectedIndex = _iDataBit;
            comboBoxEditParity.Properties.Items.Add("None");
            comboBoxEditParity.Properties.Items.Add("Odd");
            comboBoxEditParity.Properties.Items.Add("Even");
            comboBoxEditParity.SelectedIndex = _iParity;
            comboBoxEditStopbit.Properties.Items.Add("1");
            comboBoxEditStopbit.Properties.Items.Add("2");
            comboBoxEditStopbit.SelectedIndex = _iStopBit;
            comboBoxEditFlowControl.Properties.Items.Add("None");
            comboBoxEditFlowControl.SelectedIndex = 0;
            checkButtonMenualMode.Enabled = false;
            textEditPresentPosX.Text = string.Format("0");
            textEditPresentPosY.Text = string.Format("0");
            textEditPresentPosZ.Text = string.Format("0");

            DrvMotionMonitor[0] = new int[Enum.GetValues(typeof(AiCData.MONITOR_DATA_MAP)).Length];
            DrvMotionMonitor[1] = new int[Enum.GetValues(typeof(AiCData.MONITOR_DATA_MAP)).Length];
            DrvMotionMonitor[2] = new int[Enum.GetValues(typeof(AiCData.MONITOR_DATA_MAP)).Length];
            DrvMotionMonitor[3] = new int[Enum.GetValues(typeof(AiCData.MONITOR_DATA_MAP)).Length];

            DisconnectButton.Enabled = false;
            textEditSendPacketData.Enabled = false;
            SendPacketDataButton.Enabled = false;
            memoEditCommunicationLogmessage.Enabled = false;
            UpdateTimer.Interval = 100;
            UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateMotionData);
        }
        public void SetCommunicateManager(ref CommunicationManager manager)
        {
            _mAiCCommunicationManager = manager;
        }
        public void SetCommunicationData(int idnum, byte[] idarry)
        {
            _mAiCCommunicationManager.mDrvCtrl.SetIDNumber(idnum, idarry);            
            _mAiCData = _mAiCCommunicationManager.mDrvCtrl;            
        }
        public void SetMotionParam(ref RecipeManager.MotionParams _param)
        {
            _MotionParam = _param;
        }
        public string SelectPortName(int Select)
        {
            string ret = string.Empty;
            switch (Select)
            {
                case 0: ret = "COM1"; break;
                case 1: ret = "COM2"; break;
                case 2: ret = "COM3"; break;
                case 3: ret = "COM4"; break;
                case 4: ret = "COM5"; break;
                case 5: ret = "COM6"; break;
                case 6: ret = "COM7"; break;
                case 7: ret = "COM8"; break;
                case 8: ret = "COM9"; break;
                default: ret = "COM1"; break;
            }
            return ret;
        }
        public int SelectBaudrate(int Select)
        {
            int ret = 0;
            switch (Select)
            {
                case 0: ret = 4800; break;
                case 1: ret = 9600; break;
                case 2: ret = 14400; break;
                case 3: ret = 19200; break;
                case 4: ret = 38400; break;
                case 5: ret = 57600; break;
                case 6: ret = 115200; break;
                default: ret = 9600; break;
            }
            return ret;
        }
        public int SelectDataBit(int Select)
        {
            int ret = 0;
            switch (Select)
            {
                case 0: ret = 7; break;
                case 1: ret = 8; break;
                default: ret = 8; break;
            }
            return ret;
        }
        public StopBits SelectStopBit(int Select)
        {
            StopBits ret = 0;
            switch (Select)
            {
                case 0: ret = System.IO.Ports.StopBits.None; break;
                case 1: ret = System.IO.Ports.StopBits.One; break;
                case 2: ret = System.IO.Ports.StopBits.Two; break;
                default: ret = System.IO.Ports.StopBits.One; break;
            }
            return ret;
        }
        public Parity SelectParity(int Select)
        {
            Parity ret = 0;
            switch (Select)
            {
                case 0: ret = System.IO.Ports.Parity.None; break;        // None
                case 1: ret = System.IO.Ports.Parity.Odd; break;         // Odd
                case 2: ret = System.IO.Ports.Parity.Even; break;        // Even
                default: ret = System.IO.Ports.Parity.None; break;
            }
            return ret;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            SerialPortSetData setPort = new SerialPortSetData();
            setPort.PortName = SelectPortName(comboBoxEditComPort.SelectedIndex);
            setPort.BaudRate = SelectBaudrate(comboBoxEditBaudRate.SelectedIndex);
            setPort.DataBits = SelectDataBit(comboBoxEditDataBit.SelectedIndex);
            setPort.StopBits = SelectStopBit(comboBoxEditStopbit.SelectedIndex);
            setPort.Parity = SelectParity(comboBoxEditParity.SelectedIndex);
            setPort.WriteTimeout = 2000;
            _mAiCCommunicationManager.SetSerialData(setPort);
            if (!_mAiCCommunicationManager.IsOpen())
            {
                _mAiCCommunicationManager.Connect();
                if (_mAiCCommunicationManager.IsOpen())
                {
                    DisconnectButton.Enabled = true;
                    SendPacketDataButton.Enabled = true;
                    textEditSendPacketData.Enabled = true;
                    IsOpenStatus = true;                    
                    ConnectButton.Enabled = false;                    
                    _mAiCCommunicationManager.ReceiveDataUpdateEvent += UpdateReceiveData;
                    UpdateTimer.Start();

                    //byte[] data = new byte[100];
                    //for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    //{
                    //    data = _mAiCCommunicationManager.mDrvCtrl.DriveInitialSetting((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], 100, 10000, 10000, 10000);
                    //    _mAiCCommunicationManager.SendData(data);
                    //}
                }
            }
        }
        public void ConnectionOpen(SerialPortSetData setPort)
        {
            _mAiCCommunicationManager.SetSerialData(setPort);
            if (!_mAiCCommunicationManager.IsOpen())
            {
                _mAiCCommunicationManager.Connect();
                if (_mAiCCommunicationManager.IsOpen())
                {
                    DisconnectButton.Enabled = true;
                    SendPacketDataButton.Enabled = true;
                    textEditSendPacketData.Enabled = true;
                    IsOpenStatus = true;
                    ConnectButton.Enabled = false;                    
                    _mAiCCommunicationManager.ReceiveDataUpdateEvent += UpdateReceiveData;
                    UpdateTimer.Start();
                }
            }
        }
        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                _mAiCCommunicationManager.Disconnect();
                DisconnectButton.Enabled = false;
                SendPacketDataButton.Enabled = false;
                textEditSendPacketData.Enabled = false;
                IsOpenStatus = false;
                ConnectButton.Enabled = true;                
                _mAiCCommunicationManager.ReceiveDataUpdateEvent -= UpdateReceiveData;
                UpdateTimer.Stop();
                _IsInitialDrive = false;
            }
        }
        public void ConnectionClosed()
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                _mAiCCommunicationManager.Disconnect();
                DisconnectButton.Enabled = false;
                SendPacketDataButton.Enabled = false;
                textEditSendPacketData.Enabled = false;
                IsOpenStatus = false;
                ConnectButton.Enabled = true;                
                _mAiCCommunicationManager.ReceiveDataUpdateEvent -= UpdateReceiveData;
                UpdateTimer.Stop();
                _IsInitialDrive = false;
            }
        }
        public void UpdateReceiveData(AiCData.AiCMotionDatas update)
        {
            _mAiCData._mAiCMotionDatas = update;
            
            SetMotionStatus(_mAiCData._mAiCMotionDatas);
            //RobotInfomationUpdatedEvent?.Invoke(_mRobotInfomation);
        }
        private void UpdateMotionData(object sender, ElapsedEventArgs e)
        {
            //if (_mAiCCommunicationManager.IsOpen())
            //{                
            //    if (!_IsInitialDrive)
            //    {
            //        byte[] data = new byte[100];
            //        for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
            //        {
            //            data = _mAiCCommunicationManager.mDrvCtrl.DriveInitialSetting((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], 10, 10000, 100, 100);
            //            _mAiCCommunicationManager.SendData(data);
            //        }
            //        _IsInitialDrive = true;
            //    }                
            //}
            //SetMotionStatus(_mAiCData._mAiCMotionDatas);
            RobotInfomationUpdatedEvent?.Invoke(_mRobotInfomation); 
        }
        public void ShowStatus(DevExpress.XtraEditors.LabelControl label, bool status)
        {
            try
            {
                if (status)
                {
                    if (label.InvokeRequired)
                    {
                        label.Invoke(new MethodInvoker(delegate { label.ForeColor = Color.FromArgb(255, 255, 255, 255); label.BackColor = Color.FromArgb(255, 20, 200, 129); }));
                    }
                    else
                    {
                        label.ForeColor = Color.FromArgb(255, 255, 255, 255);
                        label.BackColor = Color.FromArgb(255, 20, 200, 129);
                    }
                }
                else
                {
                    if (label.InvokeRequired)
                    {
                        label.Invoke(new MethodInvoker(delegate { label.ForeColor = Color.FromArgb(255, 37, 37, 37); label.BackColor = Color.FromArgb(255, 224, 224, 224); }));
                    }
                    else
                    {
                        label.ForeColor = Color.FromArgb(255, 37, 37, 37);
                        label.BackColor = Color.FromArgb(255, 224, 224, 224);
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void SetMotionStatus(AiCData.AiCMotionDatas info)
        {
            try
            {
                if (info != null)
                {
                    if (_mAiCCommunicationManager.IsOpen())
                    {
                        int[] itempval = new int[4];
                        int datasum = 0;

                        Array.Copy(info._CurrentDatas, 0, DrvMotionMonitor[info._Id - 1], 0, info._CurrentDatas.Length);

                        if (info._Id == 1)
                        {
                            Array.Copy(info._CurrentDatas, 0, itempval, 0, 1);

                            if (textEditOpMode1.InvokeRequired)
                            {
                                textEditOpMode1.Invoke(new MethodInvoker(delegate { textEditOpMode1.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditOpMode1.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 1, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetPos1.InvokeRequired)
                            {
                                textEditTargetPos1.Invoke(new MethodInvoker(delegate { textEditTargetPos1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX); }));
                            }
                            else
                                textEditTargetPos1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX);

                            Array.Copy(info._CurrentDatas, 3, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentPosX.InvokeRequired)
                            {
                                textEditPresentPosX.Invoke(new MethodInvoker(delegate { textEditPresentPosX.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX); }));
                            }
                            else
                                textEditPresentPosX.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX);

                            if (textEditPresentPos1.InvokeRequired)
                            {
                                textEditPresentPos1.Invoke(new MethodInvoker(delegate { textEditPresentPos1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX); }));
                            }
                            else
                                textEditPresentPos1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX);

                            Array.Copy(info._CurrentDatas, 5, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetVel1.InvokeRequired)
                            {
                                textEditTargetVel1.Invoke(new MethodInvoker(delegate { textEditTargetVel1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX); }));
                            }
                            else
                                textEditTargetVel1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX);

                            Array.Copy(info._CurrentDatas, 7, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentVel1.InvokeRequired)
                            {
                                textEditPresentVel1.Invoke(new MethodInvoker(delegate { textEditPresentVel1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX); }));
                            }
                            else
                                textEditPresentVel1.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioX);

                            Array.Copy(info._CurrentDatas, 9, itempval, 0, 1);

                            if (textEditMotorRPM1.InvokeRequired)
                            {
                                textEditMotorRPM1.Invoke(new MethodInvoker(delegate { textEditMotorRPM1.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditMotorRPM1.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 10, itempval, 0, 1);

                            if (textEditProgramStep1.InvokeRequired)
                            {
                                textEditProgramStep1.Invoke(new MethodInvoker(delegate { textEditProgramStep1.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditProgramStep1.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 11, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError1[0].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 12, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError2[0].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 13, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus1[0].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 14, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus2[0].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 15, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.OutputStaus[0].SetData((UInt16)itempval[0]);
                        }
                        else if (info._Id == 2)
                        {
                            Array.Copy(info._CurrentDatas, 0, itempval, 0, 1);

                            if (textEditOpMode2.InvokeRequired)
                            {
                                textEditOpMode2.Invoke(new MethodInvoker(delegate { textEditOpMode2.EditValue = Convert.ToInt32(itempval[1]); }));
                            }
                            else
                                textEditOpMode2.EditValue = Convert.ToInt32(itempval[1]);

                            Array.Copy(info._CurrentDatas, 1, itempval, 0, 2);

                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetPos2.InvokeRequired)
                            {
                                textEditTargetPos2.Invoke(new MethodInvoker(delegate { textEditTargetPos2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY); }));
                            }
                            else
                                textEditTargetPos2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY);

                            Array.Copy(info._CurrentDatas, 3, itempval, 0, 2);

                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentPosY.InvokeRequired)
                            {
                                textEditPresentPosY.Invoke(new MethodInvoker(delegate { textEditPresentPosY.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY); }));
                            }
                            else
                                textEditPresentPosY.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY);

                            if (textEditPresentPos2.InvokeRequired)
                            {
                                textEditPresentPos2.Invoke(new MethodInvoker(delegate { textEditPresentPos2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY); }));
                            }
                            else
                                textEditPresentPos2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY);


                            Array.Copy(info._CurrentDatas, 5, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetVel2.InvokeRequired)
                            {
                                textEditTargetVel2.Invoke(new MethodInvoker(delegate { textEditTargetVel2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY); }));
                            }
                            else
                                textEditTargetVel2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY);

                            Array.Copy(info._CurrentDatas, 7, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentVel2.InvokeRequired)
                            {
                                textEditPresentVel2.Invoke(new MethodInvoker(delegate { textEditPresentVel2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY); }));
                            }
                            else
                                textEditPresentVel2.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioY);

                            Array.Copy(info._CurrentDatas, 9, itempval, 0, 1);

                            if (textEditMotorRPM2.InvokeRequired)
                            {
                                textEditMotorRPM2.Invoke(new MethodInvoker(delegate { textEditMotorRPM2.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditMotorRPM2.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 10, itempval, 0, 1);

                            if (textEditProgramStep2.InvokeRequired)
                            {
                                textEditProgramStep2.Invoke(new MethodInvoker(delegate { textEditProgramStep2.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditProgramStep2.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 11, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError1[1].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 12, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError2[1].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 13, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus1[1].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 14, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus2[1].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 15, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.OutputStaus[1].SetData((UInt16)itempval[0]);
                        }
                        else if (info._Id == 3)
                        {
                            Array.Copy(info._CurrentDatas, 0, itempval, 0, 1);

                            if (textEditOpMode3.InvokeRequired)
                            {
                                textEditOpMode3.Invoke(new MethodInvoker(delegate { textEditOpMode3.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditOpMode3.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 1, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetPos3.InvokeRequired)
                            {
                                textEditTargetPos3.Invoke(new MethodInvoker(delegate { textEditTargetPos3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ); }));
                            }
                            else
                                textEditTargetPos3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ);

                            Array.Copy(info._CurrentDatas, 3, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentPosZ.InvokeRequired)
                            {
                                textEditPresentPosZ.Invoke(new MethodInvoker(delegate { textEditPresentPosZ.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ); }));
                            }
                            else
                                textEditPresentPosZ.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ);

                            if (textEditPresentPos3.InvokeRequired)
                            {
                                textEditPresentPos3.Invoke(new MethodInvoker(delegate { textEditPresentPos3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ); }));
                            }
                            else
                                textEditPresentPos3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ);

                            Array.Copy(info._CurrentDatas, 5, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditTargetVel3.InvokeRequired)
                            {
                                textEditTargetVel3.Invoke(new MethodInvoker(delegate { textEditTargetVel3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ); }));
                            }
                            else
                                textEditTargetVel3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ);

                            Array.Copy(info._CurrentDatas, 7, itempval, 0, 2);
                            datasum = itempval[0];
                            datasum = (datasum << 16) | itempval[1];

                            if (textEditPresentVel3.InvokeRequired)
                            {
                                textEditPresentVel3.Invoke(new MethodInvoker(delegate { textEditPresentVel3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ); }));
                            }
                            else
                                textEditPresentVel3.EditValue = Convert.ToDouble(datasum * _MotionParam.Pulse2MMRatioZ);

                            Array.Copy(info._CurrentDatas, 9, itempval, 0, 1);

                            if (textEditMotorRPM3.InvokeRequired)
                            {
                                textEditMotorRPM3.Invoke(new MethodInvoker(delegate { textEditMotorRPM3.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditMotorRPM3.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 10, itempval, 0, 1);

                            if (textEditProgramStep3.InvokeRequired)
                            {
                                textEditProgramStep3.Invoke(new MethodInvoker(delegate { textEditProgramStep3.EditValue = Convert.ToInt32(itempval[0]); }));
                            }
                            else
                                textEditProgramStep3.EditValue = Convert.ToInt32(itempval[0]);

                            Array.Copy(info._CurrentDatas, 11, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError1[2].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 12, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.AlarmError2[2].SetData(Convert.ToUInt16(itempval[0]));

                            Array.Copy(info._CurrentDatas, 13, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus1[2].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 14, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.InfoStatus2[2].SetData((UInt16)itempval[0]);

                            Array.Copy(info._CurrentDatas, 15, itempval, 0, 1);
                            _mAiCCommunicationManager.mDrvCtrl.OutputStaus[2].SetData((UInt16)itempval[0]);
                        }
                        _mAiCData = _mAiCCommunicationManager.mDrvCtrl;
                        DriveInfoStatus(1);
                        DriveInfoStatus(2);
                        DriveInfoStatus(3);

                        _mRobotInfomation.PositionX = Convert.ToDouble(textEditPresentPosX.Text);
                        _mRobotInfomation.PositionY = Convert.ToDouble(textEditPresentPosY.Text);
                        _mRobotInfomation.PositionZ = Convert.ToDouble(textEditPresentPosZ.Text);

                        //if (Convert.ToBoolean(_mAiCData.OutputStaus[0].B4) && Convert.ToBoolean(_mAiCData.OutputStaus[1].B4) && Convert.ToBoolean(_mAiCData.OutputStaus[2].B4))
                        if ( Convert.ToBoolean(_mAiCData.OutputStaus[0].B4) )
                        {
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.ServoOn, true);
                            _isRobotEnable = true;
                        }
                        else
                        {
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.ServoOn, false);
                            _isRobotEnable = false;
                        }
                        //if ( (Convert.ToDouble(textEditTargetVel1) != 0) && (Convert.ToDouble(textEditTargetVel2) != 0) && (Convert.ToDouble(textEditTargetVel3) != 0) )
                        if ( (Convert.ToDouble(textEditTargetVel1.Text) != 0) )
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.Moving, true);
                        else
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.Moving, false);

                        //if (Convert.ToBoolean(_mAiCData.OutputStaus[0].B1) && Convert.ToBoolean(_mAiCData.OutputStaus[1].B1) && Convert.ToBoolean(_mAiCData.OutputStaus[2].B1))                        
                        if (Convert.ToBoolean(_mAiCData.OutputStaus[0].B1))
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.Inposition, true);                        
                        else
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.Inposition, false);

                        if (Convert.ToBoolean(_mAiCData.InfoStatus1[0].B11) && Convert.ToBoolean(_mAiCData.InfoStatus1[1].B11) && Convert.ToBoolean(_mAiCData.InfoStatus1[2].B11))
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.EmergencyStop, true);
                        else
                            _mRobotInfomation.SetStatus(RecipeManager.RobotInformation.RobotStatus.EmergencyStop, false);

                        if ((_mAiCData.AlarmError1[0].Bit16 != 0) || (_mAiCData.AlarmError1[1].Bit16 != 0) || (_mAiCData.AlarmError1[2].Bit16 != 0))
                            _mRobotInfomation.SetError(RecipeManager.RobotInformation.ErrorStatus.DrvError, true);
                        else
                            _mRobotInfomation.SetError(RecipeManager.RobotInformation.ErrorStatus.DrvError, false);

                        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MotionControl));
                        if (!_mAiCCommunicationManager.IsOpen())
                        {
                            ConnectButton.Enabled = true;
                            DisconnectButton.Enabled = false;
                        }
                        else
                        {
                            ConnectButton.Enabled = false;
                            DisconnectButton.Enabled = true;
                        }

                        if (_isRobotEnable)
                        {
                            this.RobotEnableButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RobotDisableButton.ImageOptions.Image")));
                            RobotEnableButton.Invoke(new MethodInvoker(delegate { RobotEnableButton.Text = "모션제 비활성화"; }));
                        }
                        else
                        {
                            this.RobotEnableButton.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("RobotEnableButton.ImageOptions.Image")));
                            RobotEnableButton.Invoke(new MethodInvoker(delegate { RobotEnableButton.Text = "모션제어 활성화"; }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }

        }
        public void DriveInfoStatus(int AxisNum)
        {
            if (AxisNum > 3) return;
            if (AxisNum == 1)
            {
                ShowStatus(labelControlAlarm1_1, Convert.ToBoolean(_mAiCData.AlarmError1[0].B0));
                ShowStatus(labelControlAlarm1_2, Convert.ToBoolean(_mAiCData.AlarmError1[0].B1));
                ShowStatus(labelControlAlarm1_3, Convert.ToBoolean(_mAiCData.AlarmError1[0].B2));
                ShowStatus(labelControlAlarm1_4, Convert.ToBoolean(_mAiCData.AlarmError1[0].B3));
                ShowStatus(labelControlAlarm1_5, Convert.ToBoolean(_mAiCData.AlarmError1[0].B4));
                ShowStatus(labelControlAlarm1_6, Convert.ToBoolean(_mAiCData.AlarmError1[0].B5));
                ShowStatus(labelControlAlarm1_7, Convert.ToBoolean(_mAiCData.AlarmError1[0].B6));
                ShowStatus(labelControlAlarm1_8, Convert.ToBoolean(_mAiCData.AlarmError1[0].B7));
                ShowStatus(labelControlAlarm1_9, Convert.ToBoolean(_mAiCData.AlarmError1[0].B8));
                ShowStatus(labelControlAlarm1_10, Convert.ToBoolean(_mAiCData.AlarmError1[0].B9));
                ShowStatus(labelControlAlarm1_11, Convert.ToBoolean(_mAiCData.AlarmError1[0].B10));
                ShowStatus(labelControlAlarm1_12, Convert.ToBoolean(_mAiCData.AlarmError1[0].B11));
                ShowStatus(labelControlAlarm1_13, Convert.ToBoolean(_mAiCData.AlarmError1[0].B12));
                ShowStatus(labelControlAlarm1_14, Convert.ToBoolean(_mAiCData.AlarmError1[0].B13));
                ShowStatus(labelControlAlarm1_15, Convert.ToBoolean(_mAiCData.AlarmError1[0].B14));
                ShowStatus(labelControlAlarm1_16, Convert.ToBoolean(_mAiCData.AlarmError1[0].B15));
                ShowStatus(labelControlAlarm1_17, Convert.ToBoolean(_mAiCData.AlarmError2[0].B0));
                ShowStatus(labelControlAlarm1_18, Convert.ToBoolean(_mAiCData.AlarmError2[0].B1));
                ShowStatus(labelControlAlarm1_19, Convert.ToBoolean(_mAiCData.AlarmError2[0].B2));
                ShowStatus(labelControlAlarm1_20, Convert.ToBoolean(_mAiCData.AlarmError2[0].B3));
                ShowStatus(labelControlAlarm1_21, Convert.ToBoolean(_mAiCData.AlarmError2[0].B4));
                ShowStatus(labelControlAlarm1_22, Convert.ToBoolean(_mAiCData.AlarmError2[0].B5));
                ShowStatus(labelControlAlarm1_23, Convert.ToBoolean(_mAiCData.AlarmError2[0].B6));
                ShowStatus(labelControlAlarm1_24, Convert.ToBoolean(_mAiCData.AlarmError2[0].B7));

                ShowStatus(labelControlStatus1_1, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B0));
                ShowStatus(labelControlStatus1_2, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B1));
                ShowStatus(labelControlStatus1_3, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B2));
                ShowStatus(labelControlStatus1_4, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B3));
                ShowStatus(labelControlStatus1_5, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B4));
                ShowStatus(labelControlStatus1_6, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B5));
                ShowStatus(labelControlStatus1_7, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B6));
                ShowStatus(labelControlStatus1_8, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B7));
                ShowStatus(labelControlStatus1_9, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B8));
                ShowStatus(labelControlStatus1_10, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B9));
                ShowStatus(labelControlStatus1_11, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B10));
                ShowStatus(labelControlStatus1_12, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B11));            // Emergency Stop
                ShowStatus(labelControlStatus1_13, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B12));
                ShowStatus(labelControlStatus1_14, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B13));
                ShowStatus(labelControlStatus1_15, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B14));
                ShowStatus(labelControlStatus1_16, Convert.ToBoolean(_mAiCData.InfoStatus1[0].B15));
                ShowStatus(labelControlStatus1_17, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B0));
                ShowStatus(labelControlStatus1_18, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B1));
                ShowStatus(labelControlStatus1_19, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B2));
                ShowStatus(labelControlStatus1_20, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B3));
                ShowStatus(labelControlStatus1_21, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B4));
                ShowStatus(labelControlStatus1_22, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B5));
                ShowStatus(labelControlStatus1_23, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B6));
                ShowStatus(labelControlStatus1_24, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B7));
                ShowStatus(labelControlStatus1_25, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B8));
                ShowStatus(labelControlStatus1_26, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B9));
                ShowStatus(labelControlStatus1_27, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B10));
                ShowStatus(labelControlStatus1_28, Convert.ToBoolean(_mAiCData.InfoStatus2[0].B11));

                ShowStatus(labelControlOutput1_1, Convert.ToBoolean(_mAiCData.OutputStaus[0].B0));
                ShowStatus(labelControlOutput1_2, Convert.ToBoolean(_mAiCData.OutputStaus[0].B1));              // Inposition
                ShowStatus(labelControlOutput1_3, Convert.ToBoolean(_mAiCData.OutputStaus[0].B2));
                ShowStatus(labelControlOutput1_4, Convert.ToBoolean(_mAiCData.OutputStaus[0].B3));
                ShowStatus(labelControlOutput1_5, Convert.ToBoolean(_mAiCData.OutputStaus[0].B4));              // Servo On
                ShowStatus(labelControlOutput1_6, Convert.ToBoolean(_mRobotInfomation.GetStatus(RecipeManager.RobotInformation.RobotStatus.Moving)));              // Servo On
            }
            else if (AxisNum == 2)
            {
                ShowStatus(labelControlAlarm2_1, Convert.ToBoolean(_mAiCData.AlarmError1[1].B0));
                ShowStatus(labelControlAlarm2_2, Convert.ToBoolean(_mAiCData.AlarmError1[1].B1));
                ShowStatus(labelControlAlarm2_3, Convert.ToBoolean(_mAiCData.AlarmError1[1].B2));
                ShowStatus(labelControlAlarm2_4, Convert.ToBoolean(_mAiCData.AlarmError1[1].B3));
                ShowStatus(labelControlAlarm2_5, Convert.ToBoolean(_mAiCData.AlarmError1[1].B4));
                ShowStatus(labelControlAlarm2_6, Convert.ToBoolean(_mAiCData.AlarmError1[1].B5));
                ShowStatus(labelControlAlarm2_7, Convert.ToBoolean(_mAiCData.AlarmError1[1].B6));
                ShowStatus(labelControlAlarm2_8, Convert.ToBoolean(_mAiCData.AlarmError1[1].B7));
                ShowStatus(labelControlAlarm2_9, Convert.ToBoolean(_mAiCData.AlarmError1[1].B8));
                ShowStatus(labelControlAlarm2_10, Convert.ToBoolean(_mAiCData.AlarmError1[1].B9));
                ShowStatus(labelControlAlarm2_11, Convert.ToBoolean(_mAiCData.AlarmError1[1].B10));
                ShowStatus(labelControlAlarm2_12, Convert.ToBoolean(_mAiCData.AlarmError1[1].B11));
                ShowStatus(labelControlAlarm2_13, Convert.ToBoolean(_mAiCData.AlarmError1[1].B12));
                ShowStatus(labelControlAlarm2_14, Convert.ToBoolean(_mAiCData.AlarmError1[1].B13));
                ShowStatus(labelControlAlarm2_15, Convert.ToBoolean(_mAiCData.AlarmError1[1].B14));
                ShowStatus(labelControlAlarm2_16, Convert.ToBoolean(_mAiCData.AlarmError1[1].B15));
                ShowStatus(labelControlAlarm2_17, Convert.ToBoolean(_mAiCData.AlarmError2[1].B0));
                ShowStatus(labelControlAlarm2_18, Convert.ToBoolean(_mAiCData.AlarmError2[1].B1));
                ShowStatus(labelControlAlarm2_19, Convert.ToBoolean(_mAiCData.AlarmError2[1].B2));
                ShowStatus(labelControlAlarm2_20, Convert.ToBoolean(_mAiCData.AlarmError2[1].B3));
                ShowStatus(labelControlAlarm2_21, Convert.ToBoolean(_mAiCData.AlarmError2[1].B4));
                ShowStatus(labelControlAlarm2_22, Convert.ToBoolean(_mAiCData.AlarmError2[1].B5));
                ShowStatus(labelControlAlarm2_23, Convert.ToBoolean(_mAiCData.AlarmError2[1].B6));
                ShowStatus(labelControlAlarm2_24, Convert.ToBoolean(_mAiCData.AlarmError2[1].B7));

                ShowStatus(labelControlStatus2_1, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B0));
                ShowStatus(labelControlStatus2_2, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B1));
                ShowStatus(labelControlStatus2_3, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B2));
                ShowStatus(labelControlStatus2_4, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B3));
                ShowStatus(labelControlStatus2_5, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B4));
                ShowStatus(labelControlStatus2_6, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B5));
                ShowStatus(labelControlStatus2_7, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B6));
                ShowStatus(labelControlStatus2_8, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B7));
                ShowStatus(labelControlStatus2_9, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B8));
                ShowStatus(labelControlStatus2_10, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B9));
                ShowStatus(labelControlStatus2_11, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B10));
                ShowStatus(labelControlStatus2_12, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B11));
                ShowStatus(labelControlStatus2_13, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B12));
                ShowStatus(labelControlStatus2_14, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B13));
                ShowStatus(labelControlStatus2_15, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B14));
                ShowStatus(labelControlStatus2_16, Convert.ToBoolean(_mAiCData.InfoStatus1[1].B15));
                ShowStatus(labelControlStatus2_17, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B0));
                ShowStatus(labelControlStatus2_18, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B1));
                ShowStatus(labelControlStatus2_19, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B2));
                ShowStatus(labelControlStatus2_20, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B3));
                ShowStatus(labelControlStatus2_21, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B4));
                ShowStatus(labelControlStatus2_22, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B5));
                ShowStatus(labelControlStatus2_23, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B6));
                ShowStatus(labelControlStatus2_24, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B7));
                ShowStatus(labelControlStatus2_25, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B8));
                ShowStatus(labelControlStatus2_26, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B9));
                ShowStatus(labelControlStatus2_27, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B10));
                ShowStatus(labelControlStatus2_28, Convert.ToBoolean(_mAiCData.InfoStatus2[1].B11));

                ShowStatus(labelControlOutput2_1, Convert.ToBoolean(_mAiCData.OutputStaus[1].B0));
                ShowStatus(labelControlOutput2_2, Convert.ToBoolean(_mAiCData.OutputStaus[1].B1));
                ShowStatus(labelControlOutput2_3, Convert.ToBoolean(_mAiCData.OutputStaus[1].B2));
                ShowStatus(labelControlOutput2_4, Convert.ToBoolean(_mAiCData.OutputStaus[1].B3));
                ShowStatus(labelControlOutput2_5, Convert.ToBoolean(_mAiCData.OutputStaus[1].B4));

            }
            else if (AxisNum == 3)
            {
                ShowStatus(labelControlAlarm3_1, Convert.ToBoolean(_mAiCData.AlarmError1[2].B0));
                ShowStatus(labelControlAlarm3_2, Convert.ToBoolean(_mAiCData.AlarmError1[2].B1));
                ShowStatus(labelControlAlarm3_3, Convert.ToBoolean(_mAiCData.AlarmError1[2].B2));
                ShowStatus(labelControlAlarm3_4, Convert.ToBoolean(_mAiCData.AlarmError1[2].B3));
                ShowStatus(labelControlAlarm3_5, Convert.ToBoolean(_mAiCData.AlarmError1[2].B4));
                ShowStatus(labelControlAlarm3_6, Convert.ToBoolean(_mAiCData.AlarmError1[2].B5));
                ShowStatus(labelControlAlarm3_7, Convert.ToBoolean(_mAiCData.AlarmError1[2].B6));
                ShowStatus(labelControlAlarm3_8, Convert.ToBoolean(_mAiCData.AlarmError1[2].B7));
                ShowStatus(labelControlAlarm3_9, Convert.ToBoolean(_mAiCData.AlarmError1[2].B8));
                ShowStatus(labelControlAlarm3_10, Convert.ToBoolean(_mAiCData.AlarmError1[2].B9));
                ShowStatus(labelControlAlarm3_11, Convert.ToBoolean(_mAiCData.AlarmError1[2].B10));
                ShowStatus(labelControlAlarm3_12, Convert.ToBoolean(_mAiCData.AlarmError1[2].B11));
                ShowStatus(labelControlAlarm3_13, Convert.ToBoolean(_mAiCData.AlarmError1[2].B12));
                ShowStatus(labelControlAlarm3_14, Convert.ToBoolean(_mAiCData.AlarmError1[2].B13));
                ShowStatus(labelControlAlarm3_15, Convert.ToBoolean(_mAiCData.AlarmError1[2].B14));
                ShowStatus(labelControlAlarm3_16, Convert.ToBoolean(_mAiCData.AlarmError1[2].B15));
                ShowStatus(labelControlAlarm3_17, Convert.ToBoolean(_mAiCData.AlarmError2[2].B0));
                ShowStatus(labelControlAlarm3_18, Convert.ToBoolean(_mAiCData.AlarmError2[2].B1));
                ShowStatus(labelControlAlarm3_19, Convert.ToBoolean(_mAiCData.AlarmError2[2].B2));
                ShowStatus(labelControlAlarm3_20, Convert.ToBoolean(_mAiCData.AlarmError2[2].B3));
                ShowStatus(labelControlAlarm3_21, Convert.ToBoolean(_mAiCData.AlarmError2[2].B4));
                ShowStatus(labelControlAlarm3_22, Convert.ToBoolean(_mAiCData.AlarmError2[2].B5));
                ShowStatus(labelControlAlarm3_23, Convert.ToBoolean(_mAiCData.AlarmError2[2].B6));
                ShowStatus(labelControlAlarm3_24, Convert.ToBoolean(_mAiCData.AlarmError2[2].B7));

                ShowStatus(labelControlStatus3_1, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B0));
                ShowStatus(labelControlStatus3_2, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B1));
                ShowStatus(labelControlStatus3_3, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B2));
                ShowStatus(labelControlStatus3_4, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B3));
                ShowStatus(labelControlStatus3_5, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B4));
                ShowStatus(labelControlStatus3_6, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B5));
                ShowStatus(labelControlStatus3_7, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B6));
                ShowStatus(labelControlStatus3_8, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B7));
                ShowStatus(labelControlStatus3_9, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B8));
                ShowStatus(labelControlStatus3_10, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B9));
                ShowStatus(labelControlStatus3_11, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B10));
                ShowStatus(labelControlStatus3_12, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B11));
                ShowStatus(labelControlStatus3_13, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B12));
                ShowStatus(labelControlStatus3_14, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B13));
                ShowStatus(labelControlStatus3_15, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B14));
                ShowStatus(labelControlStatus3_16, Convert.ToBoolean(_mAiCData.InfoStatus1[2].B15));
                ShowStatus(labelControlStatus3_17, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B0));
                ShowStatus(labelControlStatus3_18, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B1));
                ShowStatus(labelControlStatus3_19, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B2));
                ShowStatus(labelControlStatus3_20, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B3));
                ShowStatus(labelControlStatus3_21, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B4));
                ShowStatus(labelControlStatus3_22, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B5));
                ShowStatus(labelControlStatus3_23, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B6));
                ShowStatus(labelControlStatus3_24, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B7));
                ShowStatus(labelControlStatus3_25, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B8));
                ShowStatus(labelControlStatus3_26, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B9));
                ShowStatus(labelControlStatus3_27, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B10));
                ShowStatus(labelControlStatus3_28, Convert.ToBoolean(_mAiCData.InfoStatus2[2].B11));

                ShowStatus(labelControlOutput3_1, Convert.ToBoolean(_mAiCData.OutputStaus[2].B0));
                ShowStatus(labelControlOutput3_2, Convert.ToBoolean(_mAiCData.OutputStaus[2].B1));
                ShowStatus(labelControlOutput3_3, Convert.ToBoolean(_mAiCData.OutputStaus[2].B2));
                ShowStatus(labelControlOutput3_4, Convert.ToBoolean(_mAiCData.OutputStaus[2].B3));
                ShowStatus(labelControlOutput3_5, Convert.ToBoolean(_mAiCData.OutputStaus[2].B4));
            }
        }
        public void JogControlPannelEnable()
        {
            if (radioGroupMenualValueMode.SelectedIndex == 1)
            {
                textEditUserDefineValue.Enabled = true;
            }
            radioGroupMenualValueMode.Enabled = true;
            labelControl1.Enabled = true;
            CheckButtonXMinusControlCommand.Enabled = true;
            CheckButtonXStopControlCommand.Enabled = true;
            CheckButtonXPlusControlCommand.Enabled = true;

            //labelControl2.Enabled = true;
            CheckButtonYMinusControlCommand.Enabled = true;
            CheckButtonYStopControlCommand.Enabled = true;
            CheckButtonYPlusControlCommand.Enabled = true;

            //labelControl4.Enabled = true;
            CheckButtonZMinusControlCommand.Enabled = true;
            CheckButtonZStopControlCommand.Enabled = true;
            CheckButtonZPlusControlCommand.Enabled = true;
        }
        public void JogControlPannelDIsable()
        {
            if ((radioGroupMenualValueMode.SelectedIndex == 0) || (radioGroupMenualControlMode.SelectedIndex == 1))
            {
                textEditUserDefineValue.Enabled = false;
            }
            radioGroupMenualValueMode.Enabled = false;

            labelControl1.Enabled = false;
            CheckButtonXMinusControlCommand.Enabled = false;
            CheckButtonXStopControlCommand.Enabled = false;
            CheckButtonXPlusControlCommand.Enabled = false;

            //labelControl2.Enabled = false;
            CheckButtonYMinusControlCommand.Enabled = false;
            CheckButtonYStopControlCommand.Enabled = false;
            CheckButtonYPlusControlCommand.Enabled = false;

            //labelControl4.Enabled = false;
            CheckButtonZMinusControlCommand.Enabled = false;
            CheckButtonZStopControlCommand.Enabled = false;
            CheckButtonZPlusControlCommand.Enabled = false;
        }
        public void CoordinateControlPanelEnable()
        {
            textEditTargetPosX.Enabled = true;
            textEditTargetPosY.Enabled = true;            
            textEditTargetPosZ.Enabled = true;;
            textEditTargetVelocity.Enabled = true;
            textEditTargetAcceleration.Enabled = true;
            SendCmdPositionButton.Enabled = true;
            SendCommandMoveStopButton.Enabled = true;
            checkButtonLowValue.Enabled = true;
            checkButtonMiddleValue.Enabled = true;
            checkButtonHighValue.Enabled = true;
        }
        public void CoordinateControlPanelDisable()
        {
            textEditTargetPosX.Enabled = false;
            textEditTargetPosY.Enabled = false;            
            textEditTargetPosZ.Enabled = false;
            SendCmdPositionButton.Enabled = false;
            textEditTargetVelocity.Enabled = false;
            textEditTargetAcceleration.Enabled = false;
            SendCommandMoveStopButton.Enabled = false;
            checkButtonLowValue.Enabled = false;
            checkButtonMiddleValue.Enabled = false;
            checkButtonHighValue.Enabled = false;
        }
        private void checkButtonJogMove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mAiCCommunicationManager != null)
                {
                    //if ((_mPLCCommunicationManager.IsConnected()) && (_isRobotEnable))
                    {
                        if (!(sender is DevExpress.XtraEditors.CheckButton button)) return;

                        if (_MenaulCoordinateMode)
                        {
                            if (_MenaulControlMode == 1)
                            {
                                if ((button.Name == "CheckButtonXPlusControlCommand") || (button.Name == "CheckButtonXMinusControlCommand"))
                                {
                                    byte[] data = new byte[100];
                                    if (button.Name == "CheckButtonXPlusControlCommand")
                                    {
                                        CheckButtonXPlusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x01, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioX));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)1);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        CheckButtonXMinusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x01, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioX));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)1);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else if ((button.Name == "CheckButtonYPlusControlCommand") || (button.Name == "CheckButtonYMinusControlCommand"))
                                {
                                    byte[] data = new byte[8];
                                    if (button.Name == "CheckButtonYPlusControlCommand")
                                    {
                                        CheckButtonXPlusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x02, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioY));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)2);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        CheckButtonXMinusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x02, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioY));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)2);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else if ((button.Name == "CheckButtonZPlusControlCommand") || (button.Name == "CheckButtonZMinusControlCommand"))
                                {
                                    byte[] data = new byte[8];
                                    if (button.Name == "CheckButtonZPlusControlCommand")
                                    {
                                        CheckButtonXPlusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x03, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)3);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        CheckButtonXMinusControlCommand.Checked = false;

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x03, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));
                                        _mAiCCommunicationManager.SendData(data);

                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveReleativeCommand((byte)3);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else
                                {
                                    byte[] data = new byte[8];
                                    data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x81);      // Braodcast all Axis stop command
                                    _mAiCCommunicationManager.SendData(data);

                                    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_MOVE_STOP, null);
                                }
                            }
                        }
                        else
                        {
                            if (_MenaulControlMode == 0)
                            {
                                if ((button.Name == "CheckButtonXPlusControlCommand") || (button.Name == "CheckButtonXMinusControlCommand"))
                                {
                                    //x = _fMenualMoveVelocity;
                                    byte[] data = new byte[8];                                    
                                    if (button.Name == "CheckButtonXPlusControlCommand")
                                    {
                                        if (CheckButtonXMinusControlCommand.Checked)
                                        {
                                            CheckButtonXMinusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)1);
                                            _mAiCCommunicationManager.SendData(data);
                                        }                                        
                                        data = _mAiCCommunicationManager.mDrvCtrl.CWJogCommand((byte)1);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        if (CheckButtonXPlusControlCommand.Checked)
                                        {
                                            //CheckButtonXPlusControlCommand.Toggle();
                                            CheckButtonXPlusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)1);
                                            _mAiCCommunicationManager.SendData(data);
                                        }
                                        data = _mAiCCommunicationManager.mDrvCtrl.CCWJogCommand((byte)1);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else if ((button.Name == "CheckButtonYPlusControlCommand") || (button.Name == "CheckButtonYMinusControlCommand"))
                                {
                                    ////y1 = Convert.ToDouble(textEditJogAbsolutePosition.Text);
                                    byte[] data = new byte[8];                                    
                                    if (button.Name == "CheckButtonYPlusControlCommand")
                                    {
                                        if (CheckButtonYMinusControlCommand.Checked)
                                        {
                                            CheckButtonYMinusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)2);
                                            _mAiCCommunicationManager.SendData(data);
                                        }
                                        data = _mAiCCommunicationManager.mDrvCtrl.CWJogCommand((byte)2);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        if (CheckButtonYPlusControlCommand.Checked)
                                        {
                                            CheckButtonYPlusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)2);
                                            _mAiCCommunicationManager.SendData(data);
                                        }
                                        data = _mAiCCommunicationManager.mDrvCtrl.CCWJogCommand((byte)2);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else if ((button.Name == "CheckButtonZPlusControlCommand") || (button.Name == "CheckButtonZMinusControlCommand"))
                                {
                                    byte[] data = new byte[8];                                    
                                    if (button.Name == "CheckButtonZPlusControlCommand")
                                    {
                                        if (CheckButtonZMinusControlCommand.Checked)
                                        {
                                            CheckButtonZMinusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)3);
                                            _mAiCCommunicationManager.SendData(data);
                                        }
                                        data = _mAiCCommunicationManager.mDrvCtrl.CWJogCommand((byte)3);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                    else
                                    {
                                        if (CheckButtonZPlusControlCommand.Checked)
                                        {
                                            CheckButtonZPlusControlCommand.Checked = false;
                                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)3);
                                            _mAiCCommunicationManager.SendData(data);
                                        }
                                        data = _mAiCCommunicationManager.mDrvCtrl.CCWJogCommand((byte)3);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                            }
                            else if (_MenaulControlMode == 1)
                            {
                                if ((button.Name == "CheckButtonXPlusControlCommand") || (button.Name == "CheckButtonXMinusControlCommand"))
                                {
                                    //byte[] data = new byte[8];
                                    //UserCodesysData.JogControl mJogCtrl = new UserCodesysData.JogControl();
                                    //if (button.Name == "CheckButtonXPlusControlCommand")
                                    //{
                                    //    CheckButtonXPlusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    _mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_X_POSITIVE, data);
                                    //}
                                    //else
                                    //{
                                    //    CheckButtonXMinusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    _mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_X_NEGATIVE, data);
                                    //}
                                }
                                else if ((button.Name == "CheckButtonYPlusControlCommand") || (button.Name == "CheckButtonYMinusControlCommand"))
                                {
                                    byte[] data = new byte[8];
                                    //UserCodesysData.JogControl mJogCtrl = new UserCodesysData.JogControl();
                                    //if (button.Name == "CheckButtonY1PlusControlCommand")
                                    //{
                                    //    CheckButtonYPlusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_Y_POSITIVE, data);
                                    //}
                                    //else
                                    //{
                                    //    CheckButtonYMinusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_Y_NEGATIVE, data);
                                    //}
                                }
                                else if ((button.Name == "CheckButtonZPlusControlCommand") || (button.Name == "CheckButtonZMinusControlCommand"))
                                {
                                    //byte[] data = new byte[8];
                                    ////UserCodesysData.JogControl mJogCtrl = new UserCodesysData.JogControl();
                                    //if (button.Name == "CheckButtonZPlusControlCommand")
                                    //{
                                    //    CheckButtonZPlusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_RX_POSITIVE, data);
                                    //}
                                    //else
                                    //{
                                    //    CheckButtonZMinusControlCommand.Checked = false;
                                    //    mJogCtrl.JogEnable = 1;
                                    //    mJogCtrl.JogValue = Convert.ToSingle(_fMenualMoveVelocity);
                                    //    data = mJogCtrl.GetData();
                                    //    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_RX_NEGATIVE, data);
                                    //}
                                }
                                else
                                {
                                    //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_MOVE_STOP, null);
                                }
                            }
                            else if (_MenaulControlMode == 2)
                            {
                            }
                            else
                            {
                            }
                        }
                        if ( (button.Name == "CheckButtonXStopControlCommand") || (button.Name == "CheckButtonYStopControlCommand") || (button.Name == "CheckButtonZStopControlCommand") )
                        {
                            CheckButtonXPlusControlCommand.Checked = false;
                            CheckButtonXMinusControlCommand.Checked = false;
                            CheckButtonYPlusControlCommand.Checked = false;
                            CheckButtonYMinusControlCommand.Checked = false;
                            CheckButtonZPlusControlCommand.Checked = false;
                            CheckButtonZMinusControlCommand.Checked = false;
                            CheckButtonXStopControlCommand.Checked = false;
                            CheckButtonYStopControlCommand.Checked = false;
                            CheckButtonZStopControlCommand.Checked = false;
                            byte[] data = new byte[8];
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x81);      // Braodcast all Axis stop command
                            _mAiCCommunicationManager.SendData(data);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void CheckButtonStateControlCommand_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_mAiCCommunicationManager != null)
                {
                    //if ((_mPLCCommunicationManager.IsConnected()) && (_isRobotEnable))
                    {
                        if (!(sender is DevExpress.XtraEditors.CheckButton button)) return;

                        if (_MenaulControlMode == 1)
                        {
                            if ((button.Name == "CheckButtonXPlusControlCommand") || (button.Name == "CheckButtonXMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonXMinusControlCommand")
                                {
                                    CheckButtonXMinusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x01, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioX));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                                else
                                {
                                    CheckButtonXPlusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x01, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioX));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                            }
                            else if ((button.Name == "CheckButtonYPlusControlCommand") || (button.Name == "CheckButtonYMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonYMinusControlCommand")
                                {
                                    CheckButtonYMinusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x02, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioY));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                                else
                                {
                                    CheckButtonYPlusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x02, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioY));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                            }
                            else if ((button.Name == "CheckButtonZPlusControlCommand") || (button.Name == "CheckButtonZMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonZMinusControlCommand")
                                {
                                    CheckButtonZMinusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x03, (int)(-_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                                else
                                {
                                    CheckButtonZPlusControlCommand.Checked = false;
                                    //byte[] data = new byte[100];
                                    //data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)0x03, (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));
                                    //_mAiCCommunicationManager.SendData(data);
                                }
                            }
                            else if ( (button.Name == "CheckButtonXStopControlCommand") || (button.Name == "CheckButtonYStopControlCommand") ||
                                        (button.Name == "CheckButtonZStopControlCommand") )
                            {
                                CheckButtonXPlusControlCommand.Checked = false;
                                CheckButtonXMinusControlCommand.Checked = false;
                                CheckButtonYPlusControlCommand.Checked = false;
                                CheckButtonYMinusControlCommand.Checked = false;                                
                                CheckButtonZPlusControlCommand.Checked = false;
                                CheckButtonZMinusControlCommand.Checked = false;
                                CheckButtonXStopControlCommand.Checked = false;
                                CheckButtonYStopControlCommand.Checked = false;                                
                                CheckButtonZStopControlCommand.Checked = false;
                                byte[] data = new byte[8];
                                data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x81);      // Braodcast all Axis stop command
                                _mAiCCommunicationManager.SendData(data);
                            }
                        }
                        else
                        {
                            if ((button.Name == "CheckButtonXPlusControlCommand") || (button.Name == "CheckButtonXMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonXPlusControlCommand")
                                {
                                    if (CheckButtonXPlusControlCommand.Checked == false)
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x01);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else
                                {
                                    if (CheckButtonXMinusControlCommand.Checked == false)
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x01);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                            }
                            else if ((button.Name == "CheckButtonYPlusControlCommand") || (button.Name == "CheckButtonYMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonYPlusControlCommand")
                                {
                                    if (CheckButtonYPlusControlCommand.Checked == false) 
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x02);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else
                                {
                                    if (CheckButtonYMinusControlCommand.Checked == false) 
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x02);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                            }
                            else if ((button.Name == "CheckButtonZPlusControlCommand") || (button.Name == "CheckButtonZMinusControlCommand"))
                            {
                                if (button.Name == "CheckButtonZPlusControlCommand")
                                {
                                    if (CheckButtonZPlusControlCommand.Checked == false) 
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x03);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }
                                else
                                {
                                    if  (CheckButtonZMinusControlCommand.Checked == false)
                                    {
                                        byte[] data = new byte[8];
                                        data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x03);
                                        _mAiCCommunicationManager.SendData(data);
                                    }
                                }

                            }
                            else if ((button.Name == "CheckButtonXStopControlCommand") || (button.Name == "CheckButtonYStopControlCommand") ||
                                        (button.Name == "CheckButtonZStopControlCommand"))
                            {
                                CheckButtonXPlusControlCommand.Checked = false;
                                CheckButtonXMinusControlCommand.Checked = false;
                                CheckButtonYPlusControlCommand.Checked = false;
                                CheckButtonYMinusControlCommand.Checked = false;
                                CheckButtonZPlusControlCommand.Checked = false;
                                CheckButtonZMinusControlCommand.Checked = false;
                                CheckButtonXStopControlCommand.Checked = false;
                                CheckButtonYStopControlCommand.Checked = false;
                                CheckButtonZStopControlCommand.Checked = false;
                                byte[] data = new byte[8];
                                data = _mAiCCommunicationManager.mDrvCtrl.MoveStopCommand((byte)0x81);      // Braodcast all Axis stop command
                                _mAiCCommunicationManager.SendData(data);
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
        }
        private void ButtonStopControlCommand_CheckedChanged(object sender, EventArgs e)
        {

            CheckButtonXPlusControlCommand.Checked = false;
            CheckButtonXMinusControlCommand.Checked = false;
            CheckButtonYPlusControlCommand.Checked = false;
            CheckButtonYMinusControlCommand.Checked = false;
            CheckButtonZPlusControlCommand.Checked = false;
            CheckButtonZMinusControlCommand.Checked = false;
            CheckButtonXStopControlCommand.Checked = false;
            CheckButtonYStopControlCommand.Checked = false;            
            CheckButtonZStopControlCommand.Checked = false;
        }
        private void SendCmdPositionButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mAiCCommunicationManager != null)
                {
                    //if ((_mAiCCommunicationManager.IsOpen()) && (_isRobotEnable))
                    if ( (_mAiCCommunicationManager.IsOpen()) )
                    {
                        byte[] data = new byte[32];

                        data = _mAiCData.MoveAbsoluteCommand(129);

                        _mAiCCommunicationManager.SendData(data);

                        //UserCodesysData.TargetRobotPosition mCmdPosMove = new UserCodesysData.TargetRobotPosition();
                        //RecipeManager.CalibrationParams.Position PrePos = new RecipeManager.CalibrationParams.Position();
                        //RecipeManager.CalibrationParams.Position DeltaPos = new RecipeManager.CalibrationParams.Position();

                        //mCmdPosMove.X = (double)Convert.ToSingle(textEditTargetPosX.Text);
                        //if ((double)Convert.ToSingle(textEditTargetPosY1.Text) <= 9)
                        //    mCmdPosMove.Y = 9f;
                        //else
                        //    mCmdPosMove.Y = (double)Convert.ToSingle(textEditTargetPosY1.Text);
                        //if ((double)Convert.ToSingle(textEditTargetPosY2.Text) <= 100)
                        //    mCmdPosMove.Z = 100f;
                        //else
                        //    mCmdPosMove.Z = (double)Convert.ToSingle(textEditTargetPosY2.Text);
                        //mCmdPosMove.Roll = (double)Convert.ToSingle(textEditTargetPosZ.Text);
                        //mCmdPosMove.Pitch = (double)Convert.ToSingle(textEditTargetPosFZ.Text);
                        //mCmdPosMove.Yaw = (double)Convert.ToSingle(textEditTargetPosFR.Text);
                        //mCmdPosMove.Speed = (double)Convert.ToSingle(textEditTargetVelocity.Text);
                        //mCmdPosMove.Acceleration = (double)Convert.ToSingle(textEditTargetAcceleration.Text);

                        //if (checkEditCalibration.Checked)
                        //{
                        //    PrePos.X = mCmdPosMove.X;
                        //    PrePos.Y1 = mCmdPosMove.Y;
                        //    PrePos.Y2 = mCmdPosMove.Z;
                        //    PrePos.Z = mCmdPosMove.Roll;
                        //    PrePos.FZ = mCmdPosMove.Pitch;
                        //    PrePos.FR = mCmdPosMove.Yaw;

                        //    if (_iCalibrationMode == 0)
                        //    {
                        //        DeltaPos = CalibrationParam.EmiterCalibratinoDeltaPosition(PrePos);
                        //    }
                        //    else
                        //    {
                        //        DeltaPos = CalibrationParam.InspectCalibratinoDeltaPosition(PrePos);
                        //    }
                        //    mCmdPosMove.X += DeltaPos.X;
                        //    mCmdPosMove.Y += DeltaPos.Y1;
                        //    mCmdPosMove.Z += DeltaPos.Y2;
                        //    mCmdPosMove.Roll += DeltaPos.Z;
                        //    mCmdPosMove.Pitch += DeltaPos.FZ;
                        //    mCmdPosMove.Yaw += DeltaPos.FR;
                        //}

                        //if (mCmdPosMove.Speed <= 0) mCmdPosMove.Speed = 1;
                        //if (mCmdPosMove.Acceleration <= 0) mCmdPosMove.Acceleration = 1;
                        //data = mCmdPosMove.GetData();
                        //// Send Cordinate Move Command
                        //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_GOPOS_ABS, data);
                    }
                }
            }
            catch (Exception)
            {
                ;
            }

        }

        private void SendCommandMoveStopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mAiCCommunicationManager != null)
                {
                    if ((_mAiCCommunicationManager.IsOpen()) && (_isRobotEnable))
                    {
                        // Send Move Stop Command                        
                        //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_MOVE_STOP, null);
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        private void checkButtonLowValue_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager != null)
            {
                if ((_mAiCCommunicationManager.IsOpen()) && (_isRobotEnable) && (!_isAutomode))
                {
                    byte[] data = new byte[8];
                    //UserCodesysData.JogModeSwitch mJogData = new UserCodesysData.JogModeSwitch();
                    if (radioGroupMenualMode.SelectedIndex == 0)
                    {
                        if (radioGroupMenualControlMode.SelectedIndex == 0)
                        {
                            _fMenualMoveVelocity = _fdefineVelValue[0];
                            //mJogData.MenualMode = 0;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_OP_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            _fMenualMoveVelocity = _fdefineStepValue[0];
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                    }
                    else
                    {
                        _fMenualMoveVelocity = _fdefineStepValue[0];
                        if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            //mJogData.MenualMode = 2;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_COORDINATE_MODE, data);
                        }
                    }
                }
            }
        }

        private void checkButtonMiddleValue_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager != null)
            {
                if ((_mAiCCommunicationManager.IsOpen()) && (_isRobotEnable) && (!_isAutomode))
                {
                    byte[] data = new byte[8];
                    //UserCodesysData.JogModeSwitch mJogData = new UserCodesysData.JogModeSwitch();

                    if (radioGroupMenualMode.SelectedIndex == 0)
                    {
                        if (radioGroupMenualControlMode.SelectedIndex == 0)
                        {
                            _fMenualMoveVelocity = _fdefineVelValue[1];
                            //mJogData.MenualMode = 0;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_OP_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            _fMenualMoveVelocity = _fdefineStepValue[1];
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                    }
                    else
                    {
                        _fMenualMoveVelocity = _fdefineStepValue[1];
                        if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            //mJogData.MenualMode = 2;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_COORDINATE_MODE, data);
                        }
                    }
                }
            }
        }

        private void checkButtonHighValue_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager != null)
            {
                if ((_mAiCCommunicationManager.IsOpen()) && (_isRobotEnable) && (!_isAutomode))
                {
                    byte[] data = new byte[8];
                    //UserCodesysData.JogModeSwitch mJogData = new UserCodesysData.JogModeSwitch();

                    if (radioGroupMenualMode.SelectedIndex == 0)
                    {
                        if (radioGroupMenualControlMode.SelectedIndex == 0)
                        {
                            _fMenualMoveVelocity = _fdefineVelValue[2];
                            //mJogData.MenualMode = 0;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_OP_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            _fMenualMoveVelocity = _fdefineStepValue[2];
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                    }
                    else
                    {
                        _fMenualMoveVelocity = _fdefineStepValue[2];
                        if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            //mJogData.MenualMode = 1;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_MOTION_MODE, data);
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            //mJogData.MenualMode = 2;
                            //mJogData.MenualValue = Convert.ToSingle(_fMenualMoveVelocity);
                            //data = mJogData.GetData();
                            //_mAiCCommunicationManager.SendCommand(UserCodesysData.Protocol_MSG.MSG_CMD_JOG_COORDINATE_MODE, data);
                        }
                    }
                }
            }
        }

        private void checkButtonLowValue_CheckedChanged(object sender, EventArgs e)
        {
            checkButtonLowValue.Checked = false;            
            if (radioGroupMenualMode.SelectedIndex == 0)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineVelValue[0]); }));
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 0)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[0] * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[0] * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[0] * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
            else if (radioGroupMenualMode.SelectedIndex == 1)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineStepValue[0]); }));
                _fMenualDefineValue = _fdefineStepValue[0];
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 3)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }                
            }
        }
        private void checkButtonMiddleValue_CheckedChanged(object sender, EventArgs e)
        {
            checkButtonMiddleValue.Checked = false;            
            if (radioGroupMenualMode.SelectedIndex == 0)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineVelValue[1]); }));
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 0)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[1] * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[1] * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[1] * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
            else if (radioGroupMenualMode.SelectedIndex == 1)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineStepValue[1]); }));
                _fMenualDefineValue = _fdefineStepValue[1];
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 0)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
        }

        private void checkButtonHighValue_CheckedChanged(object sender, EventArgs e)
        {
            checkButtonHighValue.Checked = false;            
            if (radioGroupMenualMode.SelectedIndex == 0)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineVelValue[2]); }));
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 0)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[2] * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[2] * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fdefineVelValue[2] * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
            else if (radioGroupMenualMode.SelectedIndex == 1)
            {
                textEditUserDefineValue.Invoke(new MethodInvoker(delegate { textEditUserDefineValue.Text = Convert.ToString(_fdefineStepValue[2]); }));
                _fMenualDefineValue = _fdefineStepValue[2];
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 0; i < _mAiCCommunicationManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        if (i == 0)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[i], (int)(_fMenualDefineValue * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
        }

        private void SendCmdHommingButton_Click(object sender, EventArgs e)
        {
            byte[] SeData = new byte[1024];
            for (int i = 1; i < 4; i++)
            {
                SeData = _mAiCCommunicationManager.mDrvCtrl.HomeStartCommand((byte)i);
                _mAiCCommunicationManager.SendData(SeData);
            }
        }

        private void EmergencyStopButton_Click(object sender, EventArgs e)
        {
            byte[] SeData = new byte[1024];
            for (int i = 1; i < 4; i++)
            {
                SeData = _mAiCCommunicationManager.mDrvCtrl.EmergencyCommand((byte)i);
                _mAiCCommunicationManager.SendData(SeData);
            }
        }

        private void ErrorResetButton_Click(object sender, EventArgs e)
        {
            byte[] SeData = new byte[1024];
            for (int i = 1; i < 4; i++)
            {
                SeData = _mAiCCommunicationManager.mDrvCtrl.AlarmResetCommand((byte)i);
                _mAiCCommunicationManager.SendData(SeData);
            }
        }

        private void radioGroupMenualMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {                
                //if (_isRobotEnable)
                {
                    if (radioGroupMenualMode.SelectedIndex == 0)
                    {
                        _MenaulCoordinateMode = false;
                        radioGroupMenualControlMode.Properties.Items[0].Enabled = true;
                        radioGroupMenualControlMode.Properties.Items[1].Enabled = false;
                        radioGroupMenualControlMode.Properties.Items[2].Enabled = false;
                        CoordinateControlPanelDisable();
                        JogControlPannelEnable();
                        if (radioGroupMenualControlMode.SelectedIndex != 0)
                        {
                            radioGroupMenualControlMode.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        _MenaulCoordinateMode = true;
                        radioGroupMenualControlMode.Properties.Items[0].Enabled = false;
                        radioGroupMenualControlMode.Properties.Items[1].Enabled = true;
                        radioGroupMenualControlMode.Properties.Items[2].Enabled = true;
                        if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            JogControlPannelEnable();
                            CoordinateControlPanelDisable();
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            JogControlPannelDIsable();
                            CoordinateControlPanelEnable();
                        }
                        else
                        {
                            radioGroupMenualControlMode.SelectedIndex = 1;
                            JogControlPannelEnable();
                            CoordinateControlPanelDisable();
                        }
                    }
                    if (radioGroupMenualValueMode.SelectedIndex == 0)
                    {
                        defineValueButtonEnable();
                        textEditUserDefineValue.Enabled = false;
                    }
                    else
                    {
                        defineValueButtonDisable();
                        textEditUserDefineValue.Enabled = true;
                    }
                }
            }
            catch
            {
                ;
            }
        }

        private void radioGroupMenualControlMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _MenaulControlMode = radioGroupMenualControlMode.SelectedIndex;
//               if (_mAiCCommunicationManager != null)
                {          
                    if (radioGroupMenualMode.SelectedIndex == 0)
                    {
                        _MenaulCoordinateMode = false;
                        radioGroupMenualControlMode.Properties.Items[0].Enabled = true;
                        radioGroupMenualControlMode.Properties.Items[1].Enabled = false;
                        radioGroupMenualControlMode.Properties.Items[2].Enabled = false;
                        CoordinateControlPanelDisable();
                        JogControlPannelEnable();
                        if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            radioGroupMenualControlMode.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        _MenaulCoordinateMode = true;
                        radioGroupMenualControlMode.Properties.Items[0].Enabled = false;
                        radioGroupMenualControlMode.Properties.Items[1].Enabled = true;
                        radioGroupMenualControlMode.Properties.Items[2].Enabled = true;
                        if (radioGroupMenualControlMode.SelectedIndex == 1)
                        {
                            JogControlPannelEnable();
                            CoordinateControlPanelDisable();
                        }
                        else if (radioGroupMenualControlMode.SelectedIndex == 2)
                        {
                            JogControlPannelDIsable();
                            CoordinateControlPanelEnable();
                        }
                        else
                        {
                            radioGroupMenualControlMode.SelectedIndex = 1;
                            JogControlPannelEnable();
                            CoordinateControlPanelDisable();
                        }
                    }
                    if (radioGroupMenualValueMode.SelectedIndex == 0)
                    {
                        defineValueButtonEnable();
                        textEditUserDefineValue.Enabled = false;
                    }
                    else
                    {
                        defineValueButtonDisable();
                        textEditUserDefineValue.Enabled = true;
                    }                    
                }
            }
            catch (Exception)
            {
            }
        }

        private void radioGroupMenualValueMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
//                if (_mAiCCommunicationManager != null)
                {                    
                    {
                        if (radioGroupMenualValueMode.SelectedIndex == 0)
                        {
                            defineValueButtonEnable();
                            textEditUserDefineValue.Enabled = false;
                        }
                        else
                        {
                            if (radioGroupMenualControlMode.SelectedIndex != 2)
                            {
                                defineValueButtonDisable();
                                textEditUserDefineValue.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public void defineValueButtonEnable()
        {
            checkButtonLowValue.Enabled = true;
            checkButtonMiddleValue.Enabled = true;
            checkButtonHighValue.Enabled = true;
            //textEditUserDefineValue.Enabled = false;
        }
        public void defineValueButtonDisable()
        {
            checkButtonLowValue.Enabled = false;
            checkButtonMiddleValue.Enabled = false;
            checkButtonHighValue.Enabled = false;
            //textEditUserDefineValue.Enabled = true;
        }

        private void textEditUserDefineValue_EditValueChanged(object sender, EventArgs e)
        {
            if (radioGroupMenualMode.SelectedIndex == 0)
            {                
                byte[] data = new byte[100];

                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 1; i < 4; i++)
                    {
                        if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 3)
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
            else if (radioGroupMenualMode.SelectedIndex == 1)
            {                
                byte[] data = new byte[100];
                _fMenualDefineValue = Convert.ToDouble(textEditUserDefineValue.Text);
                if (_mAiCCommunicationManager.IsOpen())
                {
                    for (int i = 1; i < 4; i++)
                    {
                        if (i == 1)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioX));      //
                        else if (i == 2)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioY));      //
                        else if (i == 3)
                            data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)i, (int)(Convert.ToDouble(textEditUserDefineValue.Text) * _MotionParam.MM2PulseRatioZ));      //
                        _mAiCCommunicationManager.SendData(data);
                    }
                }
            }
        }

        private void simpleButtonReqMotionStatus_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[8];
                data = _mAiCCommunicationManager.mDrvCtrl.GetSettingMotionDatas(1);
                _mAiCCommunicationManager.SendData(data);
            }
            
        }

        private void RobotEnableButton_Click(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[8];
                for (int i = 1; i < 4; i++)
                {
                    data = _mAiCCommunicationManager.mDrvCtrl.ServoOnOffControl((byte)i, false);
                    _mAiCCommunicationManager.SendData(data);
                }
            }
        }

        private void textEditTargetPosX_EditValueChanged(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[100];
                data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[0], Convert.ToInt32( Convert.ToDouble(textEditTargetPosX.Text) * _MotionParam.MM2PulseRatioX));
                _mAiCCommunicationManager.SendData(data);
            }
        }

        private void textEditTargetPosY_EditValueChanged(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[100];
                data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[1], Convert.ToInt32(Convert.ToDouble(textEditTargetPosY.Text) * _MotionParam.MM2PulseRatioY));
                _mAiCCommunicationManager.SendData(data);
            }
        }

        private void textEditTargetPosZ_EditValueChanged(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[100];
                data = _mAiCCommunicationManager.mDrvCtrl.MoveTargetPositionSendData((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[2], Convert.ToInt32(Convert.ToDouble(textEditTargetPosZ.Text) * _MotionParam.MM2PulseRatioZ));
                _mAiCCommunicationManager.SendData(data);
            }
        }

        private void textEditTargetVelocity_EditValueChanged(object sender, EventArgs e)
        {
            if (_mAiCCommunicationManager.IsOpen())
            {
                byte[] data = new byte[100];

                for (int i = 0; i < _mAiCData.DeviceIDCount; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[0], Convert.ToInt32(Convert.ToDouble(textEditTargetVelocity.Text) * _MotionParam.MM2PulseRatioX));
                            break;
                        case 1:
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[1], Convert.ToInt32(Convert.ToDouble(textEditTargetVelocity.Text) * _MotionParam.MM2PulseRatioY));
                            break;
                        case 2:
                            data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[2], Convert.ToInt32(Convert.ToDouble(textEditTargetVelocity.Text) * _MotionParam.MM2PulseRatioZ));
                            break;
                        default: data = _mAiCCommunicationManager.mDrvCtrl.SetMoveTargetVelocity((byte)_mAiCCommunicationManager.mDrvCtrl.DrvID[0], 10000); break;
                    }
                    _mAiCCommunicationManager.SendData(data);
                }
            }
        }
    }
}
