using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ARMLibrary.SerialCommunication;
using ARMLibrary.SerialCommunication.Control;
using ARMLibrary.SerialCommunication.Data;
using ARMLibrary.SerialCommunication.DataProcessor;
using System.IO;
using System.IO.Ports;
using System.Timers;
using RecipeManager;
using LogLibrary;
namespace CustomPages
{
    public partial class RemoteIOControl : DevExpress.XtraEditors.XtraUserControl
    {
        private CommunicationManager _mARMCommunicationManager = null;
        private ARMData _mARMData;        

        RecipeManager.RobotInformation _mRobotInfomation = new RecipeManager.RobotInformation();
        public event Action<RecipeManager.RobotInformation> RobotInfomationUpdatedEvent;
        public event Action<string> LogWriteEvent;

        public bool IsOpenStatus = false;
        public string _SerialPortName = "COM5";
        public int _iBaudrate = 6;
        public int _iDataBit = 1;
        public int _iStopBit = 0;
        public int _iParity = 0;
        public int _iFlowControl = 0;

        public System.Timers.Timer UpdateTimer = new System.Timers.Timer();
        public RemoteIOControl()
        {
            InitializeComponent();
            initialSystemDefineValue();
            LogWriteEvent?.Invoke(string.Format("원격IO 초기화 완료."));
        }
        public void ChangeSystemLanguage(bool _bsystemlanguage)
        {
            if (!_bsystemlanguage)
            {
                xtraTabPageConnect.Text = "Connection";
                SendPacketDataButton.Text = "Send";
                DisconnectButton.Text = "Disconnect";
                ConnectButton.Text = "Connect";
                layoutControlItem2.Text = "COM Port";
                layoutControlItem3.Text = "Baudrate";
                layoutControlItem5.Text = "Parity";
                layoutControlItem6.Text = "Stop Bit";
                layoutControlItem7.Text = "Flow Control";
                layoutControlItem10.Text = "Send Message";
                layoutControlItem4.Text = "Data Bit";
                layoutControlItem12.Text = "ReceiveData";
                xtraTabPageControl.Text = "Control";
                groupControl2.Text = "Remote I/O Information";
                groupControl6.Text = "Digital Input";
                labelControlDIn32.Text = "32";
                labelControlDIn31.Text = "31";
                labelControlDIn30.Text = "30";
                labelControlDIn29.Text = "29";
                labelControlDIn28.Text = "28";
                labelControlDIn27.Text = "27";
                labelControlDIn26.Text = "26";
                labelControlDIn25.Text = "25";
                labelControlDIn24.Text = "24";
                labelControlDIn23.Text = "23";
                labelControlDIn22.Text = "22";
                labelControlDIn21.Text = "21";
                labelControlDIn20.Text = "20";
                labelControlDIn19.Text = "19";
                labelControlDIn18.Text = "18";
                labelControlDIn17.Text = "17";
                labelControlDIn16.Text = "16";
                labelControlDIn15.Text = "15";
                labelControlDIn14.Text = "14";
                labelControlDIn13.Text = "13";
                labelControlDIn12.Text = "12";
                labelControlDIn11.Text = "11";
                labelControlDIn10.Text = "10";
                labelControlDIn9.Text = "9";
                labelControlDIn8.Text = "8";
                labelControlDIn7.Text = "7";
                labelControlDIn6.Text = "6";
                labelControlDIn5.Text = "5";
                labelControlDIn4.Text = "4";
                labelControlDIn3.Text = "3";
                labelControlDIn2.Text = "2";
                labelControlDIn1.Text = "1";

                groupControl5.Text = "Digital Output";
                labelControlDOut32.Text = "32";
                labelControlDOut31.Text = "31";
                labelControlDOut30.Text = "30";
                labelControlDOut29.Text = "29";
                labelControlDOut28.Text = "28";
                labelControlDOut27.Text = "27";
                labelControlDOut26.Text = "26";
                labelControlDOut25.Text = "25";
                labelControlDOut24.Text = "24";
                labelControlDOut23.Text = "23";
                labelControlDOut22.Text = "22";
                labelControlDOut21.Text = "21";
                labelControlDOut20.Text = "20";
                labelControlDOut19.Text = "19";
                labelControlDOut18.Text = "18";
                labelControlDOut17.Text = "17";
                labelControlDOut16.Text = "16";
                labelControlDOut15.Text = "15";
                labelControlDOut14.Text = "14";
                labelControlDOut13.Text = "13";
                labelControlDOut12.Text = "12";
                labelControlDOut11.Text = "11";
                labelControlDOut10.Text = "10";
                labelControlDOut9.Text = "9";
                labelControlDOut8.Text = "8";
                labelControlDOut7.Text = "7";
                labelControlDOut6.Text = "6";
                labelControlDOut5.Text = "5";
                labelControlDOut4.Text = "4";
                labelControlDOut3.Text = "3";
                labelControlDOut2.Text = "2";
                labelControlDOut1.Text = "1";
            }
            else
            {
                xtraTabPageConnect.Text = "연결";
                SendPacketDataButton.Text = "전송 하기";
                DisconnectButton.Text = "연결 해제";
                ConnectButton.Text = "연결";
                layoutControlItem2.Text = "통신 포트";
                layoutControlItem3.Text = "통신 속도";
                layoutControlItem5.Text = "패리티";
                layoutControlItem6.Text = "정지 비트";
                layoutControlItem7.Text = "흐름 제어";
                layoutControlItem10.Text = "전송 메시지";
                layoutControlItem4.Text = "데이터 비트";
                layoutControlItem12.Text = "통신 내역";
                xtraTabPageControl.Text = "제어";
                groupControl2.Text = "Remote I/O 정보";
                groupControl6.Text = "Digital Input";
                labelControlDIn32.Text = "32";
                labelControlDIn31.Text = "31";
                labelControlDIn30.Text = "30";
                labelControlDIn29.Text = "29";
                labelControlDIn28.Text = "28";
                labelControlDIn27.Text = "27";
                labelControlDIn26.Text = "26";
                labelControlDIn25.Text = "25";
                labelControlDIn24.Text = "24";
                labelControlDIn23.Text = "23";
                labelControlDIn22.Text = "22";
                labelControlDIn21.Text = "21";
                labelControlDIn20.Text = "20";
                labelControlDIn19.Text = "19";
                labelControlDIn18.Text = "18";
                labelControlDIn17.Text = "17";
                labelControlDIn16.Text = "16";
                labelControlDIn15.Text = "15";
                labelControlDIn14.Text = "14";
                labelControlDIn13.Text = "13";
                labelControlDIn12.Text = "12";
                labelControlDIn11.Text = "11";
                labelControlDIn10.Text = "10";
                labelControlDIn9.Text = "9";
                labelControlDIn8.Text = "8";
                labelControlDIn7.Text = "7";
                labelControlDIn6.Text = "6";
                labelControlDIn5.Text = "5";
                labelControlDIn4.Text = "4";
                labelControlDIn3.Text = "3";
                labelControlDIn2.Text = "2";
                labelControlDIn1.Text = "1";

                groupControl5.Text = "Digital Output";
                labelControlDOut32.Text = "32";
                labelControlDOut31.Text = "31";
                labelControlDOut30.Text = "30";
                labelControlDOut29.Text = "29";
                labelControlDOut28.Text = "28";
                labelControlDOut27.Text = "27";
                labelControlDOut26.Text = "26";
                labelControlDOut25.Text = "25";
                labelControlDOut24.Text = "24";
                labelControlDOut23.Text = "23";
                labelControlDOut22.Text = "22";
                labelControlDOut21.Text = "21";
                labelControlDOut20.Text = "20";
                labelControlDOut19.Text = "19";
                labelControlDOut18.Text = "18";
                labelControlDOut17.Text = "17";
                labelControlDOut16.Text = "16";
                labelControlDOut15.Text = "15";
                labelControlDOut14.Text = "14";
                labelControlDOut13.Text = "13";
                labelControlDOut12.Text = "12";
                labelControlDOut11.Text = "11";
                labelControlDOut10.Text = "10";
                labelControlDOut9.Text = "9";
                labelControlDOut8.Text = "8";
                labelControlDOut7.Text = "7";
                labelControlDOut6.Text = "6";
                labelControlDOut5.Text = "5";
                labelControlDOut4.Text = "4";
                labelControlDOut3.Text = "3";
                labelControlDOut2.Text = "2";
                labelControlDOut1.Text = "1";

            }
        }
        public void initialSystemDefineValue()
        {
            string[] tmpStr = System.IO.Ports.SerialPort.GetPortNames();

            if (tmpStr.Length > 0)
            {
                comboBoxEditComPort.Properties.Items.AddRange(tmpStr);
            }
                        
            comboBoxEditComPort.SelectedIndex = 4;
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

            DisconnectButton.Enabled = false;
            textEditSendPacketData.Enabled = false;
            SendPacketDataButton.Enabled = false;
            memoEditCommunicationLogmessage.Enabled = false;
            UpdateTimer.Interval = 100;
            UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateRemoteIOData);
        }
        public void SetCommunicateManager(ref CommunicationManager manager)
        {
            _mARMCommunicationManager = manager;            
        }
        public void SetCommunicationData(int idnum, byte[] idarry)
        {
            _mARMCommunicationManager.mRemoteIOCtrl.SetIDNumber(idnum, idarry);
            _mARMData = _mARMCommunicationManager.mRemoteIOCtrl;
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
            LogWriteEvent?.Invoke(string.Format("원격IO 통신 포트 {0} 설정.",ret));
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
            LogWriteEvent?.Invoke(string.Format("원격IO 통신 속도 {0} 설정.", ret));
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
            LogWriteEvent?.Invoke(string.Format("원격IO 통신 데이터비트 {0} 설정.", ret));
            return ret;
        }
        public System.IO.Ports.StopBits SelectStopBit(int Select)
        {
            System.IO.Ports.StopBits ret = 0;
            switch (Select)
            {
                case 0: ret = System.IO.Ports.StopBits.One; break;
                case 1: ret = System.IO.Ports.StopBits.Two; break;
                default: ret = System.IO.Ports.StopBits.One; break;
            }
            LogWriteEvent?.Invoke(string.Format("원격IO 통신 정지비트 {0} 설정.", ret));
            return ret;
        }
        public System.IO.Ports.Parity SelectParity(int Select)
        {
            System.IO.Ports.Parity ret = 0;
            switch (Select)
            {
                case 0: ret = System.IO.Ports.Parity.None; break;        // None
                case 1: ret = System.IO.Ports.Parity.Odd; break;         // Odd
                case 2: ret = System.IO.Ports.Parity.Even; break;        // Even
                default: ret = System.IO.Ports.Parity.None; break;
            }
            LogWriteEvent?.Invoke(string.Format("원격IO 통신 Parity {0} 설정.", ret));
            return ret;
        }
        private void OutputControl_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mARMCommunicationManager != null)
                {
                    if (_mARMCommunicationManager.IsOpen())
                    {
                        ///*
                        if (!(sender is DevExpress.XtraEditors.LabelControl label)) return;
                        byte[] data = new byte[8];
                        int OutputData = 0;
                        ushort maskdata = 0;
                        switch (label.Name)
                        {
                            case "labelControlDOut1":
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];                                
                                
                                if (Convert.ToBoolean(OutputData & (1 << 0)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력1 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력1 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output0, maskdata);
                                _mARMCommunicationManager.SendData(data);                                
                                break;
                            case "labelControlDOut2":
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 1)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력2 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력2 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output1, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut3":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 2)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력3 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력3 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output2, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut4":
                                
                                 OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 3)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력4 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력4 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output3, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut5":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 4)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력5 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력5 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output4, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut6":

                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 5)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력6 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력6 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output5, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut7":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 6)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력7 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력7 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output6, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut8":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 7)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력8 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력8 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output7, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut9":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 8)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력9 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력9 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output8, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut10":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 9)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력10 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력10 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output9, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut11":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 10)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력11 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력11 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output10, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut12":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 11)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력12 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력12 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output11, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut13":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 12)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력13 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력13 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output12, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut14":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 13)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력14 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력14 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output13, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut15":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 14)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력15 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력15 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output14, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            case "labelControlDOut16":
                                
                                OutputData = _mARMData._mRemoteIODatas._CurrentOutputs[0];

                                if (Convert.ToBoolean(OutputData & (1 << 15)))
                                {
                                    maskdata = 0;               // Output Off;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력16 Off."));
                                }
                                else
                                {
                                    maskdata = 0xff00;          // Output On;
                                    LogWriteEvent?.Invoke(string.Format("원격IO 출력16 On."));
                                }
                                data = _mARMData.Output1byteCommand(_mARMCommunicationManager.mRemoteIOCtrl.DrvID[0], ARMData.OUTPUT_CONTROL_MAP.Output15, maskdata);
                                _mARMCommunicationManager.SendData(data);
                                break;
                            default:
                                break;
                        }
                        //*/
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
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
            _mARMCommunicationManager.SetSerialData(setPort);
            if (!_mARMCommunicationManager.IsOpen())
            {
                _mARMCommunicationManager.Connect();
                if (_mARMCommunicationManager.IsOpen())
                {
                    DisconnectButton.Enabled = true;
                    SendPacketDataButton.Enabled = true;
                    textEditSendPacketData.Enabled = true;
                    IsOpenStatus = true;
                    ConnectButton.Enabled = false;
                    _mARMCommunicationManager.ReceiveDataUpdateEvent += UpdateReceiveData;
                    UpdateTimer.Start();
                    LogWriteEvent?.Invoke(string.Format("원격 I/O 통신({0})이 연결 되었습니다", setPort.PortName));
                }
            }
        }
        public void ConnectionOpen(SerialPortSetData setPort)
        {
            _mARMCommunicationManager.SetSerialData(setPort);
            if (!_mARMCommunicationManager.IsOpen())
            {
                _mARMCommunicationManager.Connect();
                if (_mARMCommunicationManager.IsOpen())
                {
                    DisconnectButton.Enabled = true;
                    SendPacketDataButton.Enabled = true;
                    textEditSendPacketData.Enabled = true;
                    IsOpenStatus = true;
                    ConnectButton.Enabled = false;                    
                    _mARMCommunicationManager.ReceiveDataUpdateEvent += UpdateReceiveData;
                    UpdateTimer.Start();
                    LogWriteEvent?.Invoke(string.Format("원격 I/O 통신({0})이 연결 되었습니다", setPort.PortName));
                }
            }
        }
        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            if (_mARMCommunicationManager.IsOpen())
            {
                _mARMCommunicationManager.Disconnect();
                DisconnectButton.Enabled = false;
                SendPacketDataButton.Enabled = false;
                textEditSendPacketData.Enabled = false;
                IsOpenStatus = false;
                ConnectButton.Enabled = true;
                _mARMCommunicationManager.ReceiveDataUpdateEvent -= UpdateReceiveData;
                UpdateTimer.Stop();
                LogWriteEvent?.Invoke(string.Format("원격 I/O 통신이 연결해제 되었습니다"));
            }
        }
        public void ConnectionClosed()
        {
            if (_mARMCommunicationManager.IsOpen())
            {
                _mARMCommunicationManager.Disconnect();
                DisconnectButton.Enabled = false;
                SendPacketDataButton.Enabled = false;
                textEditSendPacketData.Enabled = false;
                IsOpenStatus = false;
                ConnectButton.Enabled = true;
                _mARMCommunicationManager.ReceiveDataUpdateEvent -= UpdateReceiveData;                
                UpdateTimer.Stop();
                LogWriteEvent?.Invoke(string.Format("원격 I/O 통신이 연결해제 되었습니다"));
            }
        }
        public void UpdateReceiveData(ARMData.RemoteIODatas update)
        {
            _mARMData._mRemoteIODatas = update;

            SetIOStatus(_mARMData._mRemoteIODatas);
        }
        private void UpdateRemoteIOData(object sender, ElapsedEventArgs e)
        {
            if (_mARMCommunicationManager.IsOpen())
            {
                //SetIOStatus( _mARMData._mRemoteIODatas);     
                RobotInfomationUpdatedEvent?.Invoke(_mRobotInfomation);
            }
        }
        public void SetIOStatus(ARMData.RemoteIODatas update)
        {
            ARMData.IO_16Bit InData = new ARMData.IO_16Bit();
            ulong inputs = 0;
            inputs = Convert.ToUInt16(update._CurrentInputs[1]);
            inputs = (inputs << 8) | (Convert.ToUInt16(update._CurrentInputs[0]));
            _mRobotInfomation.mInputData.Bit64 = inputs;

            inputs = Convert.ToUInt16(update._CurrentOutputs[1]);
            inputs = (inputs << 8) | (Convert.ToUInt16(update._CurrentOutputs[0]));
            _mRobotInfomation.mOutputData.Bit64 = inputs;

            InData.Bit16 = Convert.ToUInt16(update._CurrentInputs[0]);
            ShowStatus(labelControlDIn1, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDIn2, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDIn3, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDIn4, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDIn5, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDIn6, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDIn7, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDIn8, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentInputs[1]);
            ShowStatus(labelControlDIn9, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDIn10, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDIn11, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDIn12, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDIn13, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDIn14, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDIn15, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDIn16, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentInputs[2]);
            ShowStatus(labelControlDIn17, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDIn18, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDIn19, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDIn20, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDIn21, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDIn22, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDIn23, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDIn24, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentInputs[3]);
            ShowStatus(labelControlDIn25, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDIn26, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDIn27, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDIn28, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDIn29, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDIn30, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDIn31, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDIn32, Convert.ToBoolean(InData.B7));

            InData.Bit16 = Convert.ToUInt16(update._CurrentOutputs[0]);
            ShowStatus(labelControlDOut1, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDOut2, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDOut3, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDOut4, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDOut5, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDOut6, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDOut7, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDOut8, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentOutputs[1]);
            ShowStatus(labelControlDOut9, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDOut10, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDOut11, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDOut12, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDOut13, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDOut14, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDOut15, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDOut16, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentOutputs[2]);
            ShowStatus(labelControlDOut17, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDOut18, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDOut19, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDOut20, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDOut21, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDOut22, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDOut23, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDOut24, Convert.ToBoolean(InData.B7));
            InData.Bit16 = Convert.ToUInt16(update._CurrentOutputs[3]);
            ShowStatus(labelControlDOut25, Convert.ToBoolean(InData.B0));
            ShowStatus(labelControlDOut26, Convert.ToBoolean(InData.B1));
            ShowStatus(labelControlDOut27, Convert.ToBoolean(InData.B2));
            ShowStatus(labelControlDOut28, Convert.ToBoolean(InData.B3));
            ShowStatus(labelControlDOut29, Convert.ToBoolean(InData.B4));
            ShowStatus(labelControlDOut30, Convert.ToBoolean(InData.B5));
            ShowStatus(labelControlDOut31, Convert.ToBoolean(InData.B6));
            ShowStatus(labelControlDOut32, Convert.ToBoolean(InData.B7));
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
    }
}
