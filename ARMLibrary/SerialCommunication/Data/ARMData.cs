using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARMLibrary.SerialCommunication.DataProcessor;
using System.Runtime.InteropServices;

namespace ARMLibrary.SerialCommunication.Data
{
    public class ARMData
    {       
        public enum OUTPUT_CONTROL_MAP
        {
            // Base Address 1~10
            Output0 = 0,
            Output1,
            Output2,
            Output3,
            Output4,
            Output5,
            Output6,
            Output7,
            Output8,
            Output9,
            Output10,
            Output11,
            Output12,
            Output13,
            Output14,
            Output15,
            Output16,
            Output17,
            Output18,
            Output19,
            Output20,
            Output21,
            Output22,
            Output23,
            Output24,
            Output25,
            Output26,
            Output27,
            Output28,
            Output29,
            Output30,
            Output31,
            Output32,
            Output33,
            Output34,
            Output35,
            Output36,
            Output37,
            Output38,
            Output39,
            Output40,
            Output41,
            Output42,
            Output43,
            Output44,
            Output45,
            Output46,
            Output47,
            Output48,
            Output49,
            Output50,
            Output51,
            Output52,
            Output53,
            Output54,
            Output55,
            Output56,
            Output57,
            Output58,
            Output59,
            Output60,
            Output61,
            Output62,
            Output63,
        }
        public enum INPUT_MAP
        {
            //Base Address 100001 ~ 100100.
            Input0 = 0,
            Input1,
            Input2,
            Input3,
            Input4,
            Input5,
            Input6,
            Input7,
            Input8,
            Input9,
            Input10,
            Input11,
            Input12,
            Input13,
            Input14,
            Input15,
            Input16,
            Input17,
            Input18,
            Input19,
            Input20,
            Input21,
            Input22,
            Input23,
            Input24,
            Input25,
            Input26,
            Input27,
            Input28,
            Input29,
            Input30,
            Input31,
            Input32,
            Input33,
            Input34,
            Input35,
            Input36,
            Input37,
            Input38,
            Input39,
            Input40,
            Input41,
            Input42,
            Input43,
            Input44,
            Input45,
            Input46,
            Input47,
            Input48,
            Input49,
            Input50,
            Input51,
            Input52,
            Input53,
            Input54,
            Input55,
            Input56,
            Input57,
            Input58,
            Input59,
            Input60,
            Input61,
            Input62,
            Input63,
        }
        public enum PRODUCT_INPUT_MAP
        {
            // Base Address 300001 ~ 300999
            Input2Byte1 = 0,
            Input2Byte2,
            Input2Byte3,
            Input2Byte4
        }
        public enum PRODUCT_INFORMATION_MAP
        {
            // Base Address 300101 ~ 300129
            SerialNumberH = 100,
            SerialNumberL,
            HardwareVersion,
            SoftwareVersion,
            ModelName1,
            ModelName2,
            ModelName3,
            ModelName4,
            ModelName5,
            ModelName6,
            ModelName7,
            ModelName8,
            ModelName9,
            ModelName10
        }
        public enum PRODUCT_INFORMATION_EXT_UNIT_MAP
        {
            // Base Address 300131 ~ 300999
            Unit1ModelName1 = 129,
            Unit1ModelName2,
            Unit1ModelName3,
            Unit1ModelName4,
            Unit1ModelName5,
            Unit1ModelName6,
            Unit1ModelName7,
            Unit1ModelName8,
            Unit1ModelName9,
            Unit1ModelName10,
            Unit2ModelName1,
            Unit2ModelName2,
            Unit2ModelName3,
            Unit2ModelName4,
            Unit2ModelName5,
            Unit2ModelName6,
            Unit2ModelName7,
            Unit2ModelName8,
            Unit2ModelName9,
            Unit2ModelName10,
            Unit3ModelName1,
            Unit3ModelName2,
            Unit3ModelName3,
            Unit3ModelName4,
            Unit3ModelName5,
            Unit3ModelName6,
            Unit3ModelName7,
            Unit3ModelName8,
            Unit3ModelName9,
            Unit3ModelName10,
            Unit4ModelName1,
            Unit4ModelName2,
            Unit4ModelName3,
            Unit4ModelName4,
            Unit4ModelName5,
            Unit4ModelName6,
            Unit4ModelName7,
            Unit4ModelName8,
            Unit4ModelName9,
            Unit4ModelName10,
            Unit5ModelName1,
            Unit5ModelName2,
            Unit5ModelName3,
            Unit5ModelName4,
            Unit5ModelName5,
            Unit5ModelName6,
            Unit5ModelName7,
            Unit5ModelName8,
            Unit5ModelName9,
            Unit5ModelName10,
            Unit6ModelName1,
            Unit6ModelName2,
            Unit6ModelName3,
            Unit6ModelName4,
            Unit6ModelName5,
            Unit6ModelName6,
            Unit6ModelName7,
            Unit6ModelName8,
            Unit6ModelName9,
            Unit6ModelName10,
            Unit7ModelName1,
            Unit7ModelName2,
            Unit7ModelName3,
            Unit7ModelName4,
            Unit7ModelName5,
            Unit7ModelName6,
            Unit7ModelName7,
            Unit7ModelName8,
            Unit7ModelName9,
            Unit7ModelName10,
        }
        public enum UNIT_COUTER_MAP
        {
            // Base Address 300201 ~ 300299
            BaseUnitCountP0 = 200,
            BaseUnitCountP1,
            BaseUnitCountP2,
            BaseUnitCountP3,
            BaseUnitCountP4,
            BaseUnitCountP5,
            BaseUnitCountP6,
            BaseUnitCountP7,
            ExtUnit1CountP0,
            ExtUnit1CountP1,
            ExtUnit1CountP2,
            ExtUnit1CountP3,
            ExtUnit1CountP4,
            ExtUnit1CountP5,
            ExtUnit1CountP6,
            ExtUnit1CountP7,
            ExtUnit2CountP0,
            ExtUnit2CountP1,
            ExtUnit2CountP2,
            ExtUnit2CountP3,
            ExtUnit2CountP4,
            ExtUnit2CountP5,
            ExtUnit2CountP6,
            ExtUnit2CountP7,
            ExtUnit3CountP0,
            ExtUnit3CountP1,
            ExtUnit3CountP2,
            ExtUnit3CountP3,
            ExtUnit3CountP4,
            ExtUnit3CountP5,
            ExtUnit3CountP6,
            ExtUnit3CountP7,
            ExtUnit4CountP0,
            ExtUnit4CountP1,
            ExtUnit4CountP2,
            ExtUnit4CountP3,
            ExtUnit4CountP4,
            ExtUnit4CountP5,
            ExtUnit4CountP6,
            ExtUnit4CountP7,
            ExtUnit5CountP0,
            ExtUnit5CountP1,
            ExtUnit5CountP2,
            ExtUnit5CountP3,
            ExtUnit5CountP4,
            ExtUnit5CountP5,
            ExtUnit5CountP6,
            ExtUnit5CountP7,
            ExtUnit6CountP0,
            ExtUnit6CountP1,
            ExtUnit6CountP2,
            ExtUnit6CountP3,
            ExtUnit6CountP4,
            ExtUnit6CountP5,
            ExtUnit6CountP6,
            ExtUnit6CountP7,
            ExtUnit7CountP0,
            ExtUnit7CountP1,
            ExtUnit7CountP2,
            ExtUnit7CountP3,
            ExtUnit7CountP4,
            ExtUnit7CountP5,
            ExtUnit7CountP6,
            ExtUnit7CountP7,
        }
        public enum CUMULATIVE_COUNT_MAP
        {
            // Base Address 300301 ~ 300399
            CumulativeBaseUnitCountP0 = 300,
            CumulativeBaseUnitCountP1,
            CumulativeBaseUnitCountP2,
            CumulativeBaseUnitCountP3,
            CumulativeBaseUnitCountP4,
            CumulativeBaseUnitCountP5,
            CumulativeBaseUnitCountP6,
            CumulativeBaseUnitCountP7,
            CumulativeUnit1CountP0,
            CumulativeUnit1CountP1,
            CumulativeUnit1CountP2,
            CumulativeUnit1CountP3,
            CumulativeUnit1CountP4,
            CumulativeUnit1CountP5,
            CumulativeUnit1CountP6,
            CumulativeUnit1CountP7,
            CumulativeUnit2CountP0,
            CumulativeUnit2CountP1,
            CumulativeUnit2CountP2,
            CumulativeUnit2CountP3,
            CumulativeUnit2CountP4,
            CumulativeUnit2CountP5,
            CumulativeUnit2CountP6,
            CumulativeUnit2CountP7,
            CumulativeUnit3CountP0,
            CumulativeUnit3CountP1,
            CumulativeUnit3CountP2,
            CumulativeUnit3CountP3,
            CumulativeUnit3CountP4,
            CumulativeUnit3CountP5,
            CumulativeUnit3CountP6,
            CumulativeUnit3CountP7,
            CumulativeUnit4CountP0,
            CumulativeUnit4CountP1,
            CumulativeUnit4CountP2,
            CumulativeUnit4CountP3,
            CumulativeUnit4CountP4,
            CumulativeUnit4CountP5,
            CumulativeUnit4CountP6,
            CumulativeUnit4CountP7,
            CumulativeUnit5CountP0,
            CumulativeUnit5CountP1,
            CumulativeUnit5CountP2,
            CumulativeUnit5CountP3,
            CumulativeUnit5CountP4,
            CumulativeUnit5CountP5,
            CumulativeUnit5CountP6,
            CumulativeUnit5CountP7,
            CumulativeUnit6CountP0,
            CumulativeUnit6CountP1,
            CumulativeUnit6CountP2,
            CumulativeUnit6CountP3,
            CumulativeUnit6CountP4,
            CumulativeUnit6CountP5,
            CumulativeUnit6CountP6,
            CumulativeUnit6CountP7,
            CumulativeUnit7CountP0,
            CumulativeUnit7CountP1,
            CumulativeUnit7CountP2,
            CumulativeUnit7CountP3,
            CumulativeUnit7CountP4,
            CumulativeUnit7CountP5,
            CumulativeUnit7CountP6,
            CumulativeUnit7CountP7,
        }
        public enum DEVICE_INFOMATION_MAP
        {
            // Base Address 301000 ~ 301100
            ExpandUnitNum = 1000,
            BaseUnitSpec,
            ExpandUnit1Spec,
            ExpandUnit2Spec,
            ExpandUnit3Spec,
            ExpandUnit4Spec,
            ExpandUnit5Spec,
            ExpandUnit6Spec,
            ExpandUnit7Spec,
            ReadInPortSize,
            ReadOutPortSize,            
            Reserved1,
            Reserved2,
            Reserved3,
            Reserved4,
            Reserved5,
            Reserved6,
            Reserved7,
            Reserved8,
            Reserved9,
            Reserved10,
            Reserved11,
            Reserved12,
            ModuleErrorStatus,
            NetworkErrorStatus,
            ExpandUintCommError,
            Reserved13,
            AutoBaudrate,
            E2PROMMacID,
            BaudRate,
            ParityBit,
            StopBit,
            StatusBitFlag,
            Reserved14,
            CounterFunctionFlag
        }
 
        public enum OPERATION_INOUT_MAP
        {
            // Base Address 400001 ~ 400010
            Input1 = 0,
            Input2,
            Input3,
            Input4,
            Output1,
            Output2,
            Output3,
            Output4,
        }
        public enum OPERATION_COMMUNICATION_MAP
        {
            // Base Address 400404 ~ 400408
            AutoBaudrate,
            E2PROMMacID,
            BaudRate,
            ParityBit,
            StopBit,
        }
        public enum OPERATION_COUTER_MAP
        {
            // Homing Parameter Group, 400200 ~ 400299
            BaseUnitCountP0 = 200,
            BaseUnitCountP1,
            BaseUnitCountP2,
            BaseUnitCountP3,
            BaseUnitCountP4,
            BaseUnitCountP5,
            BaseUnitCountP6,
            BaseUnitCountP7,
            ExtUnit1CountP0,
            ExtUnit1CountP1,
            ExtUnit1CountP2,
            ExtUnit1CountP3,
            ExtUnit1CountP4,
            ExtUnit1CountP5,
            ExtUnit1CountP6,
            ExtUnit1CountP7,
            ExtUnit2CountP0,
            ExtUnit2CountP1,
            ExtUnit2CountP2,
            ExtUnit2CountP3,
            ExtUnit2CountP4,
            ExtUnit2CountP5,
            ExtUnit2CountP6,
            ExtUnit2CountP7,
            ExtUnit3CountP0,
            ExtUnit3CountP1,
            ExtUnit3CountP2,
            ExtUnit3CountP3,
            ExtUnit3CountP4,
            ExtUnit3CountP5,
            ExtUnit3CountP6,
            ExtUnit3CountP7,
            ExtUnit4CountP0,
            ExtUnit4CountP1,
            ExtUnit4CountP2,
            ExtUnit4CountP3,
            ExtUnit4CountP4,
            ExtUnit4CountP5,
            ExtUnit4CountP6,
            ExtUnit4CountP7,
            ExtUnit5CountP0,
            ExtUnit5CountP1,
            ExtUnit5CountP2,
            ExtUnit5CountP3,
            ExtUnit5CountP4,
            ExtUnit5CountP5,
            ExtUnit5CountP6,
            ExtUnit5CountP7,
            ExtUnit6CountP0,
            ExtUnit6CountP1,
            ExtUnit6CountP2,
            ExtUnit6CountP3,
            ExtUnit6CountP4,
            ExtUnit6CountP5,
            ExtUnit6CountP6,
            ExtUnit6CountP7,
            ExtUnit7CountP0,
            ExtUnit7CountP1,
            ExtUnit7CountP2,
            ExtUnit7CountP3,
            ExtUnit7CountP4,
            ExtUnit7CountP5,
            ExtUnit7CountP6,
            ExtUnit7CountP7,
        }
        public enum OPERATION_CUMULATIVE_COUNT_MAP
        {
            // Operation Parameter Group, 400301 ~ 400399
            CumulativeBaseUnitCountP0 = 300,
            CumulativeBaseUnitCountP1,
            CumulativeBaseUnitCountP2,
            CumulativeBaseUnitCountP3,
            CumulativeBaseUnitCountP4,
            CumulativeBaseUnitCountP5,
            CumulativeBaseUnitCountP6,
            CumulativeBaseUnitCountP7,
            CumulativeUnit1CountP0,
            CumulativeUnit1CountP1,
            CumulativeUnit1CountP2,
            CumulativeUnit1CountP3,
            CumulativeUnit1CountP4,
            CumulativeUnit1CountP5,
            CumulativeUnit1CountP6,
            CumulativeUnit1CountP7,
            CumulativeUnit2CountP0,
            CumulativeUnit2CountP1,
            CumulativeUnit2CountP2,
            CumulativeUnit2CountP3,
            CumulativeUnit2CountP4,
            CumulativeUnit2CountP5,
            CumulativeUnit2CountP6,
            CumulativeUnit2CountP7,
            CumulativeUnit3CountP0,
            CumulativeUnit3CountP1,
            CumulativeUnit3CountP2,
            CumulativeUnit3CountP3,
            CumulativeUnit3CountP4,
            CumulativeUnit3CountP5,
            CumulativeUnit3CountP6,
            CumulativeUnit3CountP7,
            CumulativeUnit4CountP0,
            CumulativeUnit4CountP1,
            CumulativeUnit4CountP2,
            CumulativeUnit4CountP3,
            CumulativeUnit4CountP4,
            CumulativeUnit4CountP5,
            CumulativeUnit4CountP6,
            CumulativeUnit4CountP7,
            CumulativeUnit5CountP0,
            CumulativeUnit5CountP1,
            CumulativeUnit5CountP2,
            CumulativeUnit5CountP3,
            CumulativeUnit5CountP4,
            CumulativeUnit5CountP5,
            CumulativeUnit5CountP6,
            CumulativeUnit5CountP7,
            CumulativeUnit6CountP0,
            CumulativeUnit6CountP1,
            CumulativeUnit6CountP2,
            CumulativeUnit6CountP3,
            CumulativeUnit6CountP4,
            CumulativeUnit6CountP5,
            CumulativeUnit6CountP6,
            CumulativeUnit6CountP7,
            CumulativeUnit7CountP0,
            CumulativeUnit7CountP1,
            CumulativeUnit7CountP2,
            CumulativeUnit7CountP3,
            CumulativeUnit7CountP4,
            CumulativeUnit7CountP5,
            CumulativeUnit7CountP6,
            CumulativeUnit7CountP7,
        }
        public enum CommandMassege
        {
            MSG_NONE = 0,
            MSG_OUTPUT,
            MSG_INPUT,
            MSG_PRODUCT_INFO,            
            MSG_EXT_UNIT_INFO,
            MSG_DEVICE_INFO,
            MSG_OPERATION_UNIT_COUNT,
            MSG_OPERATION_CUMULATIVE_UNIT_COUNT,
            //MSG_OPERATION_INOUT,
            //MSG_OPERATION_PARAM,
            MSG_COMM_PARAM
        }
        public ModbusRTU mRemoteIOCommunication = new ModbusRTU();

        public byte DeviceIDCount { get; set; }

        private int[] CurrentOutput = null;
        private int[] CurrentInput = null;
        private int[] CurrentProdInfomation = null;
        private int[] CurrentExUnitInformation = null;
        private int[] CurrentUnitCount = null;
        private int[] CurrentCumulativeUnitCount = null;
        private int[] CurrentDeviceInformation = null;
        private int[] CurrentCommParam = null;
        public byte[] DrvID = null;
        public int GetCurrentOutput(OUTPUT_CONTROL_MAP putput) => CurrentOutput[((int)putput - Enum.GetValues(typeof(OUTPUT_CONTROL_MAP)).Cast<int>().Min()) / 8];
        public int GetCurrentInput(INPUT_MAP input) => CurrentInput[((int)input - Enum.GetValues(typeof(INPUT_MAP)).Cast<int>().Min()) / 8];        
        public int GetCurrentProdInfo(PRODUCT_INFORMATION_MAP prodinfo) => CurrentProdInfomation[(int)prodinfo - Enum.GetValues(typeof(PRODUCT_INFORMATION_MAP)).Cast<int>().Min()];
        public int GetCurrentExUnitInfo(PRODUCT_INFORMATION_EXT_UNIT_MAP exProdInfo) => CurrentExUnitInformation[(int)exProdInfo - Enum.GetValues(typeof(PRODUCT_INFORMATION_EXT_UNIT_MAP)).Cast<int>().Min()];
        public int GetCurrentDeviceInfo(DEVICE_INFOMATION_MAP deviceinfo) => CurrentDeviceInformation[(int)deviceinfo - Enum.GetValues(typeof(DEVICE_INFOMATION_MAP)).Cast<int>().Min()];
        public int GetCurrentUnitCount(UNIT_COUTER_MAP unitcount) => CurrentUnitCount[(int)unitcount - Enum.GetValues(typeof(UNIT_COUTER_MAP)).Cast<int>().Min()];
        public int GetCurrentCumulativeCount(CUMULATIVE_COUNT_MAP cumunit) => CurrentCumulativeUnitCount[(int)cumunit - Enum.GetValues(typeof(CUMULATIVE_COUNT_MAP)).Cast<int>().Min()];
        public int GetCurrentCommParam(OPERATION_COMMUNICATION_MAP comm) => CurrentCommParam[(int)comm - Enum.GetValues(typeof(OPERATION_COMMUNICATION_MAP)).Cast<int>().Min()];

        public Queue<CommandMassege> RequestedCommandQueue = new Queue<CommandMassege>();
        public List<int[]> ARMElement = new List<int[]>();
        public Dictionary<int, List<int[]>> ARMProduct = new Dictionary<int, List<int[]>>();
        public RemoteIODatas _mRemoteIODatas = new RemoteIODatas();        

        public event Action<RemoteIODatas> RemoteIODataEvent;

        public class RemoteIODatas
        {
            public int _Id;
            public int[] _CurrentOutputs = new int[(Enum.GetValues(typeof(OUTPUT_CONTROL_MAP)).Length / 8) + 1];
            public int[] _CurrentInputs = new int[(Enum.GetValues(typeof(INPUT_MAP)).Length / 8) + 1];
        }
        public class IO_8Bit
        {
            public byte Bit8;
            public int B0 { get { return Bit8 & (1 << 0); } set { Bit8 |= (1 << 0); } }
            public int B1 { get { return Bit8 & (1 << 1); } set { Bit8 |= (1 << 1); } }
            public int B2 { get { return Bit8 & (1 << 2); } set { Bit8 |= (1 << 2); } }
            public int B3 { get { return Bit8 & (1 << 3); } set { Bit8 |= (1 << 3); } }
            public int B4 { get { return Bit8 & (1 << 4); } set { Bit8 |= (1 << 4); } }
            public int B5 { get { return Bit8 & (1 << 5); } set { Bit8 |= (1 << 5); } }
            public int B6 { get { return Bit8 & (1 << 6); } set { Bit8 |= (1 << 6); } }
            public int B7 { get { return Bit8 & (1 << 7); } set { Bit8 |= (1 << 7); } }
        }
        public class IO_16Bit
        {
            public UInt16 Bit16;            
            public int B0 { get { return Bit16 & (1 << 0); } set { Bit16 |= (1 << 0); } }
            public int B1 { get { return Bit16 & (1 << 1); } set { Bit16 |= (1 << 1); } }
            public int B2 { get { return Bit16 & (1 << 2); } set { Bit16 |= (1 << 2); } }
            public int B3 { get { return Bit16 & (1 << 3); } set { Bit16 |= (1 << 3); } }
            public int B4 { get { return Bit16 & (1 << 4); } set { Bit16 |= (1 << 4); } }
            public int B5 { get { return Bit16 & (1 << 5); } set { Bit16 |= (1 << 5); } }
            public int B6 { get { return Bit16 & (1 << 6); } set { Bit16 |= (1 << 6); } }
            public int B7 { get { return Bit16 & (1 << 7); } set { Bit16 |= (1 << 7); } }
            public int B8 { get { return Bit16 & (1 << 8); } set { Bit16 |= (1 << 8); } }
            public int B9 { get { return Bit16 & (1 << 9); } set { Bit16 |= (1 << 9); } }
            public int B10 { get { return Bit16 & (1 << 10); } set { Bit16 |= (1 << 10); } }
            public int B11 { get { return Bit16 & (1 << 11); } set { Bit16 |= (1 << 11); } }
            public int B12 { get { return Bit16 & (1 << 12); } set { Bit16 |= (1 << 12); } }
            public int B13 { get { return Bit16 & (1 << 13); } set { Bit16 |= (1 << 13); } }
            public int B14 { get { return Bit16 & (1 << 14); } set { Bit16 |= (1 << 14); } }
            public int B15 { get { return Bit16 & (1 << 15); } set { Bit16 |= (1 << 15); } }
        }
        public ARMData()
        {
            CurrentOutput = new int[(Enum.GetValues(typeof(OUTPUT_CONTROL_MAP)).Length/8) + 1];
            CurrentInput = new int[(Enum.GetValues(typeof(INPUT_MAP)).Length/8) + 1];
            CurrentProdInfomation = new int[Enum.GetValues(typeof(PRODUCT_INFORMATION_MAP)).Length];
            CurrentExUnitInformation = new int[Enum.GetValues(typeof(PRODUCT_INFORMATION_EXT_UNIT_MAP)).Length];
            CurrentUnitCount = new int[Enum.GetValues(typeof(UNIT_COUTER_MAP)).Length];        
            CurrentCumulativeUnitCount = new int[Enum.GetValues(typeof(CUMULATIVE_COUNT_MAP)).Length];
            CurrentDeviceInformation = new int[Enum.GetValues(typeof(DEVICE_INFOMATION_MAP)).Length];            
            CurrentCommParam = new int[Enum.GetValues(typeof(OPERATION_COMMUNICATION_MAP)).Length];
            ARMElement.Clear();
            ARMProduct.Clear();
        }
        ~ARMData()
        {
            CurrentOutput = null;
            CurrentInput = null;
            CurrentProdInfomation = null;
            CurrentExUnitInformation = null;
            CurrentDeviceInformation = null;
            CurrentUnitCount = null;
            CurrentCumulativeUnitCount = null;
            CurrentCommParam = null;
            if (ARMElement != null)
                ARMElement.Clear();
            if (ARMProduct != null)
                ARMProduct.Clear();
        }
        public void SetIDNumber(int IdNum,byte[] ID)
        {
            if ((CurrentOutput == null) && (CurrentInput == null) &&  (CurrentProdInfomation == null) && (CurrentExUnitInformation == null) && 
                (CurrentDeviceInformation == null) && (CurrentUnitCount == null) && (CurrentCumulativeUnitCount == null) && (CurrentCommParam == null)) return;

            List<int[]> listElement = new List<int[]>();

            ARMElement.Add(CurrentOutput);
            ARMElement.Add(CurrentInput);
            ARMElement.Add(CurrentProdInfomation);
            ARMElement.Add(CurrentExUnitInformation);
            ARMElement.Add(CurrentDeviceInformation);
            ARMElement.Add(CurrentUnitCount);
            ARMElement.Add(CurrentCumulativeUnitCount);
            ARMElement.Add(CurrentCommParam);

            DeviceIDCount = (byte)IdNum;
            DrvID = new byte[DeviceIDCount];
            if (IdNum > 0)
            {
                for (int i = 0; i < IdNum; i++)
                {
                    ARMProduct.Add(ID[i], ARMElement);
                    DrvID[i] = ID[i];
                }
            }
            else
                ARMProduct.Add(1, ARMElement);
        }
        public void SetRequestedCommand(CommandMassege cmd)
        {
            RequestedCommandQueue.Enqueue(cmd);
        }
        public CommandMassege GetRequestedCommand()
        {
            if (RequestedCommandQueue.Count > 0)
                return RequestedCommandQueue.Dequeue();
            else
                return CommandMassege.MSG_NONE;
        }
        public void ClearRequestedCommand()
        {
            RequestedCommandQueue.Clear();
        }
        public void ReceiveSetOutput(byte[] data)
        {
            try
            {
                if (data == null) return;                

                byte nID = data[0];
                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.WriteFunctionCodes.WriteSingleCoil)
                    {
                        short addr = BitConverter.ToInt16(data, 2);
                        if ((addr <= ((ushort)Enum.GetValues(typeof(OUTPUT_CONTROL_MAP)).Length)))
                        {
                            CurrentOutput[addr] = BitConverter.ToInt16(data, 4);
                        }
                    }
                    else if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadCoils)
                    {
                        short count = data[2];
                        //if (count == CurrentOutput.Length)
                        {
                            for (int i = 0; i < count; i++)
                                CurrentOutput[i] = (data[3 + i] & 0x0000ffff);
                        }
                        _mRemoteIODatas._Id = nID;
                        Array.Copy(CurrentOutput, 0, _mRemoteIODatas._CurrentOutputs, 0, CurrentOutput.Length);

                        RemoteIODataEvent?.Invoke(_mRemoteIODatas);
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] SetSettingOutput(byte nID, OUTPUT_CONTROL_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);            
            return mRemoteIOCommunication.GetMessageForWrite(nID,
                DataProcessor.ModbusRTU.WriteFunctionCodes.WriteSingleCoil,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingOutput(byte nID, OUTPUT_CONTROL_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadCoils,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingOutputs(byte nID)
        {            
            
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadCoils,
                (ushort)OUTPUT_CONTROL_MAP.Output0, (ushort)(Enum.GetValues(typeof(OUTPUT_CONTROL_MAP)).Length / 8));
        }
        public byte[] Output1byteCommand(byte nID, OUTPUT_CONTROL_MAP addr, ushort data)
        {
            return mRemoteIOCommunication.GetMessageForWrite(nID, DataProcessor.ModbusRTU.WriteFunctionCodes.WriteSingleCoil, (ushort)addr, (ushort) data);
        }
        public void ReceiveSetInput(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];
                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.WriteFunctionCodes.WriteSingleCoil)
                    {
                        short addr = BitConverter.ToInt16(data, 2);
                        if ((addr <= ((ushort)Enum.GetValues(typeof(INPUT_MAP)).Length)))
                        {
                            CurrentInput[addr] = BitConverter.ToInt16(data, 4);
                        }
                    }
                    else if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputs)
                    {
                        short count = data[2];
                        //if (count == CurrentInput.Length)
                        {
                            for (int i = 0; i < count; i++)
                                CurrentInput[i] = (data[3 + i] & 0x0000ffff);
                        }
                        _mRemoteIODatas._Id = nID;
                        Buffer.BlockCopy(CurrentInput, 0, _mRemoteIODatas._CurrentInputs, 0, CurrentInput.Length);

                        RemoteIODataEvent?.Invoke(_mRemoteIODatas);
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingInput(byte nID, INPUT_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            SetRequestedCommand(CommandMassege.MSG_INPUT);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputs,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingInputs(byte nID)
        {            
            SetRequestedCommand(CommandMassege.MSG_INPUT);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputs,
                (ushort)INPUT_MAP.Input0, (ushort)(Enum.GetValues(typeof(INPUT_MAP)).Length / 8));
        }
        public void ReceiveSetProductInfo(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentProdInfomation[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentProdInfomation, 0, ARMElement[2], 0, CurrentProdInfomation.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingProductInfo(byte nID, PRODUCT_INFORMATION_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            SetRequestedCommand(CommandMassege.MSG_PRODUCT_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingProductInfos(byte nID)
        {            
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(PRODUCT_INFORMATION_MAP)).Cast<int>().Min());
            SetRequestedCommand(CommandMassege.MSG_PRODUCT_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(PRODUCT_INFORMATION_MAP)).Length);
        }
        public void ReceiveSetExtProductInfo(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentExUnitInformation[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentExUnitInformation, 0, ARMElement[3], 0, CurrentExUnitInformation.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingExtProductInfo(byte nID, PRODUCT_INFORMATION_EXT_UNIT_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            SetRequestedCommand(CommandMassege.MSG_EXT_UNIT_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingExtProductInfos(byte nID)
        {
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(PRODUCT_INFORMATION_EXT_UNIT_MAP)).Cast<int>().Min());
            SetRequestedCommand(CommandMassege.MSG_EXT_UNIT_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(PRODUCT_INFORMATION_EXT_UNIT_MAP)).Length);
        }
        public void ReceiveSetDeviceInfo(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentDeviceInformation[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentExUnitInformation, 0, ARMElement[4], 0, CurrentDeviceInformation.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingDeviceInfo(byte nID, DEVICE_INFOMATION_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            SetRequestedCommand(CommandMassege.MSG_DEVICE_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingDeviceInfos(byte nID)
        {
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(DEVICE_INFOMATION_MAP)).Cast<int>().Min());
            SetRequestedCommand(CommandMassege.MSG_DEVICE_INFO);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadInputRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(DEVICE_INFOMATION_MAP)).Length);
        }
        public void ReceiveSetUnitCount(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentUnitCount[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentUnitCount, 0, ARMElement[5], 0, CurrentUnitCount.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingUnitCount(byte nID, OPERATION_COUTER_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingUnitCounts(byte nID)
        {
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(OPERATION_COUTER_MAP)).Cast<int>().Min());
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(OPERATION_COUTER_MAP)).Length);
        }
        public void ReceiveSetCumulativeCount(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentCumulativeUnitCount[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentCumulativeUnitCount, 0, ARMElement[5], 0, CurrentCumulativeUnitCount.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingCumulativeCount(byte nID, OPERATION_CUMULATIVE_COUNT_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingCumulativeCounts(byte nID)
        {
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(OPERATION_CUMULATIVE_COUNT_MAP)).Cast<int>().Min());
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(OPERATION_CUMULATIVE_COUNT_MAP)).Length);
        }
        public void ReceiveSetCommunicationData(byte[] data)
        {
            try
            {
                if (data == null) return;
                if (data.Length < DataProcessor.ModbusRTU.MINIMUM_RESPONSE_SIZE) return;

                byte nID = data[0];

                if (ARMProduct.ContainsKey((int)nID))
                {
                    if (data[1] == (byte)DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters)
                    {
                        short count = data[2];
                        if (count != 0) count /= 2;
                        for (int i = 0; i < count; i++)
                            CurrentCommParam[i] = (mRemoteIOCommunication.GetShortValueFromTwoBytes(data, 3 + (i * 2)) & 0x0000ffff);
                        Buffer.BlockCopy(CurrentCommParam, 0, ARMElement[7], 0, CurrentCommParam.Length);

                        ARMProduct[nID] = ARMElement;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        public byte[] GetSettingCommunicationData(byte nID, OPERATION_COMMUNICATION_MAP func, ushort numberOfPoints)
        {
            ushort startAddr = Convert.ToUInt16((ushort)func);
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr,
                numberOfPoints);
        }
        public byte[] GetSettingCommunicationDatas(byte nID)
        {
            ushort startAddr = Convert.ToUInt16((ushort)Enum.GetValues(typeof(OPERATION_COMMUNICATION_MAP)).Cast<int>().Min());
            //SetRequestedCommand(CommandMassege.MSG_COMM_PARAM);
            return mRemoteIOCommunication.GetMessageForRead(nID,
                DataProcessor.ModbusRTU.ReadFunctionCodes.ReadHoldingRegisters,
                startAddr, (ushort)Enum.GetValues(typeof(OPERATION_COMMUNICATION_MAP)).Length);
        }
    }
}
