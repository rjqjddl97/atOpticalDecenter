﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using DevExpress.XtraEditors;
using System.IO;
using RecipeManager;
using LogLibrary;

namespace atOpticalDecenter
{
    public partial class SystemEditor : DevExpress.XtraEditors.XtraForm
    {
        SystemParams _systemParameters = new SystemParams();
        public Log _log = new Log();
        public SystemEditor()
        {
            InitializeComponent();
        }
        public SystemEditor(bool _bKoreaLanguage)
        {
            InitializeComponent();
            if (!_bKoreaLanguage)
            {
                categoryCameraParameters.Properties.Caption = "Camera Parameter";                
                rowCameraFriendlyName.Properties.Caption = "Camera Name";
                rowCameraHResolution.Properties.Caption = "Horizone Pixel(H)";
                rowCameraVResolution.Properties.Caption = "Vertical Pixel(V)";
                rowCameraOnePixelResolution.Properties.Caption = "PixelResolution(mm)";
                rowCameraFrameRate.Properties.Caption = "Frame Ratio(fps)";
                rowCameraExposureTime.Properties.Caption = "Exposure Time(us)";
                rowCameraGain.Properties.Caption = "Gain";
                rowCameraImageSensorSizeH.Properties.Caption = "Image Sensor Size(H)[mm]";
                rowCameraImageSensorSizeV.Properties.Caption = "Image Sensor Size(V)[mm]";
                rowCameraLensFocusLenth.Properties.Caption = "Focus Length[mm]";

                categoryCalibraitionParameters.Properties.Caption = "Calibration Parameter";
                rowCoordinateSwitch.Properties.Caption = "Use X,Y symmetry";
                rowImageToSystemX.Properties.Caption = "Image X Coordinate -> Sytem Y Coordiante Polarity";
                rowImageToSystemZ.Properties.Caption = "Image Y Coordinate -> Sytem Z Coordiante Polarity";
                rowCoordinateCalibrationActive.Properties.Caption = "Use Coordinate Calibration";
                rowReference_X.Properties.Caption = "Reference X Position";
                rowReference_Y.Properties.Caption = "Reference Y Position";
                rowReference_Z.Properties.Caption = "Reference Z Position";
                rowReferenceP1_X.Properties.Caption = "Calibration P1 X Position";
                rowReferenceP1_Y.Properties.Caption = "Calibration P1 Y Position";
                rowReferenceP1_Z.Properties.Caption = "Calibration P1 Z Position";
                rowReferenceP2_X.Properties.Caption = "Calibration P2 X Position";
                rowReferenceP2_Y.Properties.Caption = "Calibration P2 Y Position";
                rowReferenceP2_Z.Properties.Caption = "Calibration P2 Z Position";

                categoryMotionParameters.Properties.Caption = " Motion Parameter";
                rowMotionResolutionX.Properties.Caption = "X Axis Resolution[pulse]";
                rowMotionResolutionY.Properties.Caption = "Y Axis Resolution[pulse]";
                rowMotionResolutionZ.Properties.Caption = "Z Axis Resolution[pulse]";
                rowMotionGearRatioX.Properties.Caption = "X Axis Gear Ratio[1:N]";
                rowMotionGearRatioY.Properties.Caption = "Y Axis Gear Ratio[1:N]";
                rowMotionGearRatioZ.Properties.Caption = "Z Axis Gear Ratio[1:N]";
                rowMotionBallLeadX.Properties.Caption = "X Axis BallLead[mm]";
                rowMotionBallLeadY.Properties.Caption = "Y Axis BallLead[mm，˚]";
                rowMotionBallLeadZ.Properties.Caption = "Z Axis BallLead[mm，˚]";
                rowMotionMoveVelocity.Properties.Caption = "Moving Velocity[mm/s]";
                rowMotionMoveAcceleration.Properties.Caption = "Moving Acceleratoin[mm/s^2]";
                rowMotionMenaulVelocity.Properties.Caption = "Menual Moving Velocity[mm/s]";

                categoryAiCCommunicationParameters.Properties.Caption = "AiC Communication Parameter";
                rowAiCCommunicationPortName.Properties.Caption = "ComPort";
                rowAiCCommunicationBaudRate.Properties.Caption = "Baudrate[bps]";
                rowAiCCommunicationDatabit.Properties.Caption = "Data Bit";
                rowAiCCommunicationParity.Properties.Caption = "Parity";
                rowAiCCommunicationStopbit.Properties.Caption = "Stop Bit";
                rowAiCCommunicationHandshake.Properties.Caption = "Flow Control";
                rowAiCCommunicationCounter.Properties.Caption = "Connection Counter";
                categoryAiCCommunicationIDs.Properties.Caption = "AiC ID Setup";

                categoryRemoteIOCommunicationParameters.Properties.Caption = "Remote IO Communication Parameter";
                rowRemoteIOCommunicationPortName.Properties.Caption = "ComPort";
                rowRemoteIOCommunicationBaudRate.Properties.Caption = "Baudrate[bps]";
                rowRemoteIOCommunicationDatabit.Properties.Caption = "Data Bit";
                rowRemoteIOCommunicationParity.Properties.Caption = "Parity";
                rowRemoteIOCommunicationStopbit.Properties.Caption = "Stop Bit";
                rowRemoteIOCommunicationHandshake.Properties.Caption = "Flow Control";
                rowRemoteIOCommunicationCount.Properties.Caption = "Connection Counter";
                categoryRemoteIOCommunicationIDs.Properties.Caption = "Remote IO ID Setup";
                rowRemoteIOInputProperties1.Caption = "I/O Module1";
                rowRemoteIOOutputProperties1.Caption = "I/O Module2";

                categorySystemADMSInformation.Properties.Caption = "ADMS Information";
                rowSystemADMSUse.Properties.Caption = "Use DB";
                rowSystemJobWorkUse.Properties.Caption = "Use JobOrder DB";
                rowSystemADMSIPAddress.Properties.Caption = "IP Address";
                rowSystemADMSPort.Properties.Caption = "Port";
                rowSystemADMSUserID.Properties.Caption = "User ID";
                rowSystemADMSPassWD.Properties.Caption = "Password";
                rowSystemADMSEquipmentID.Properties.Caption = "Equipment ID";
                rowSystemADMSSchemaName.Properties.Caption = "Schema DB name";
                rowSystemADMSEquipmentDBName.Properties.Caption = "Equipment DB name";
                rowSystemADMSProductDBName.Properties.Caption = "Product DB name";

                categorySaveResult.Properties.Caption = "Inspection Result Parameter";
                rowSaveResultLEDMeasurement.Properties.Caption = "Save Image File";
                rowSaveResultStatistics.Properties.Caption = "Save Chart Data";

                categorySystemLanguage.Properties.Caption = "System Language";
                rowSystemUseLanguage.Properties.Caption = "Use Korea Language";

                simpleButtonSystemSaveCancel.Text = "Cancle";
                simpleButtonSystemFileSave.Text = "Save";
            }
            else
            {
                categoryCameraParameters.Properties.Caption = "카메라 파라미터";                
                rowCameraFriendlyName.Properties.Caption = "카메라 이름";
                rowCameraHResolution.Properties.Caption = "가로 해상도(H)";
                rowCameraVResolution.Properties.Caption = "세로 해상도(V)";
                rowCameraOnePixelResolution.Properties.Caption = "한 픽셀 해상도(mm)";
                rowCameraFrameRate.Properties.Caption = "프레임비(fps)";
                rowCameraExposureTime.Properties.Caption = "노출 시간(us)";
                rowCameraGain.Properties.Caption = "게인";
                rowCameraImageSensorSizeH.Properties.Caption = "카메라 이미지센서 크기(가로)[mm]";
                rowCameraImageSensorSizeV.Properties.Caption = "카메라 이미지센서 크기(세로)[mm]";
                rowCameraLensFocusLenth.Properties.Caption = "카메라렌즈 초첨거리[mm]";

                categoryCalibraitionParameters.Properties.Caption = "보정 파라미터";
                rowCoordinateSwitch.Properties.Caption = "X,Y좌표계 대칭변환유무";
                rowImageToSystemX.Properties.Caption = "이미지 X 좌표계 -> 시스템 X 좌표계";
                rowImageToSystemZ.Properties.Caption = "이미지 Y 좌표계 -> 시스템 Z 좌표계";
                rowCoordinateCalibrationActive.Properties.Caption = "좌표계 보정 활성화유무";
                rowReference_X.Properties.Caption = "기준X 좌표";
                rowReference_Y.Properties.Caption = "기준Y 좌표";
                rowReference_Z.Properties.Caption = "기준Z 좌표";
                rowReferenceP1_X.Properties.Caption = "교정점 P1 X좌표";
                rowReferenceP1_Y.Properties.Caption = "교정점 P1 Y좌표";
                rowReferenceP1_Z.Properties.Caption = "교정점 P1 Z좌표";
                rowReferenceP2_X.Properties.Caption = "교정점 P2 X좌표";
                rowReferenceP2_Y.Properties.Caption = "교정점 P2 Y좌표";
                rowReferenceP2_Z.Properties.Caption = "교정점 P2 Z좌표";
                
                categoryMotionParameters.Properties.Caption = " 모션 파라미터";
                rowMotionResolutionX.Properties.Caption = "X축 1회전 분해능[pulse]";
                rowMotionResolutionY.Properties.Caption = "Y축 1회전 분해능[pulse]";
                rowMotionResolutionZ.Properties.Caption = "Z축 1회전 분해능[pulse]";
                rowMotionGearRatioX.Properties.Caption = "X축 기어비[1:N]";
                rowMotionGearRatioY.Properties.Caption = "Y축 기어비[1:N]";
                rowMotionGearRatioZ.Properties.Caption = "Z축 기어비[1:N]";
                rowMotionBallLeadX.Properties.Caption = "X축 볼리드[mm]";
                rowMotionBallLeadY.Properties.Caption = "Y출 볼리드[mm，˚]";
                rowMotionBallLeadZ.Properties.Caption = "Z축 볼리드[mm，˚]";
                rowMotionMoveVelocity.Properties.Caption = "이동 속도[mm/s]";
                rowMotionMoveAcceleration.Properties.Caption = "이동 가속도[mm/s^2]";
                rowMotionMenaulVelocity.Properties.Caption = "수동 이동 속도[mm/s]";

                categoryAiCCommunicationParameters.Properties.Caption = "AiC 모션 통신 설정";
                rowAiCCommunicationPortName.Properties.Caption = "통신 포트";
                rowAiCCommunicationBaudRate.Properties.Caption = "통신속도[bps]";
                rowAiCCommunicationDatabit.Properties.Caption = "데이터 비트";
                rowAiCCommunicationParity.Properties.Caption = "패리티";
                rowAiCCommunicationStopbit.Properties.Caption = "정지 비트";
                rowAiCCommunicationHandshake.Properties.Caption = "흐름 제어";
                rowAiCCommunicationCounter.Properties.Caption = "연결 대수";
                categoryAiCCommunicationIDs.Properties.Caption = "AiC 모션 ID 설정";

                categoryRemoteIOCommunicationParameters.Properties.Caption = "Remote IO 통신 설정";
                rowRemoteIOCommunicationPortName.Properties.Caption = "통신 포트";
                rowRemoteIOCommunicationBaudRate.Properties.Caption = "통신속도[bps]";
                rowRemoteIOCommunicationDatabit.Properties.Caption = "데이터 비트";
                rowRemoteIOCommunicationParity.Properties.Caption = "패리티";
                rowRemoteIOCommunicationStopbit.Properties.Caption = "정지 비트";
                rowRemoteIOCommunicationHandshake.Properties.Caption = "흐름 제어";
                rowRemoteIOCommunicationCount.Properties.Caption = "연결 대수";
                categoryRemoteIOCommunicationIDs.Properties.Caption = "Remote IO ID설정";
                rowRemoteIOInputProperties1.Caption = "입출력 모듈1";
                rowRemoteIOOutputProperties1.Caption = "입출력 모듈2";

                categorySystemADMSInformation.Properties.Caption = "ADMS 정보";
                rowSystemADMSUse.Properties.Caption = "DB 사용";
                rowSystemJobWorkUse.Properties.Caption = "작업지시서 DB연계사용";
                rowSystemADMSIPAddress.Properties.Caption = "IP 주소";
                rowSystemADMSPort.Properties.Caption = "포트";
                rowSystemADMSUserID.Properties.Caption = "사용자 ID";
                rowSystemADMSPassWD.Properties.Caption = "비밀번호";
                rowSystemADMSEquipmentID.Properties.Caption = "Equipment ID";
                rowSystemADMSSchemaName.Properties.Caption = "Schema DB 이름";
                rowSystemADMSEquipmentDBName.Properties.Caption = "Equipment DB 이름";
                rowSystemADMSProductDBName.Properties.Caption = "Product DB 이름";

                categorySaveResult.Properties.Caption = "결과 저장 옵션";
                rowSaveResultLEDMeasurement.Properties.Caption = "투광 LED 이미지 저장";
                rowSaveResultStatistics.Properties.Caption = "통계 데이터 저장";

                categorySystemLanguage.Properties.Caption = "시스템 언어 설정";
                rowSystemUseLanguage.Properties.Caption = "한국어 사용";

                simpleButtonSystemSaveCancel.Text = "취소하기";
                simpleButtonSystemFileSave.Text = "저장하기";
            }
        }
        private void vGridControlSystemParameters_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            SetCellValue(e.Row);
        }
        private void SystemEditor_Load(object sender, EventArgs e)
        {
            simpleButtonSystemFileSave.Enabled = false;

            InitializeSystemParameters();
            LoadSystemParameters();
            _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("System 설정 로드 완료"));
        }
        private void InitializeSystemParameters()
        {
            for (int i = 0; i < RecipeFileIO.SerialPortName.Length; ++i)
            {
                repositoryItemComboBoxAiCCommunicationPortName.Items.Add(RecipeFileIO.SerialPortName[i]);
                repositoryItemComboBoxRemoteIOCommunicationPortName.Items.Add(RecipeFileIO.SerialPortName[i]);
            }

            for (int i = 0; i < RecipeFileIO.SerialBaudRates.Length; ++i)
            {
                repositoryItemComboBoxAiCCommunicationBaudRate.Items.Add(RecipeFileIO.SerialBaudRates[i]);
                repositoryItemComboBoxRemoteIOCommunicationBaudRate.Items.Add(RecipeFileIO.SerialBaudRates[i]);
            }

            for (int i = 0; i < RecipeFileIO.SerialDataBits.Length; ++i)
            {
                repositoryItemComboBoxAiCCommunicationDatabit.Items.Add(RecipeFileIO.SerialDataBits[i]);
                repositoryItemComboBoxRemoteIOCommunicationDatabit.Items.Add(RecipeFileIO.SerialDataBits[i]);
            }

            foreach (string strParity in Enum.GetNames(typeof(Parity)))
            {
                repositoryItemComboBoxAiCCommunicationParity.Items.Add(strParity);
                repositoryItemComboBoxRemoteIOCommunicationParity.Items.Add(strParity);
            }

            foreach (string strStopbits in Enum.GetNames(typeof(StopBits)))
            {
                repositoryItemComboBoxAiCCommunicationStopbit.Items.Add(strStopbits);
                repositoryItemComboBoxRemoteIOCommunicationStopbit.Items.Add(strStopbits);
            }

            foreach (string strHandshake in Enum.GetNames(typeof(Handshake)))
            {
                repositoryItemComboBoxAiCCommunicationHandshake.Items.Add(strHandshake);
                repositoryItemComboBoxRemoteIOCommunicationHandshake.Items.Add(strHandshake);
            }
            for (int i = 0; i < RecipeFileIO.TransitionCoordinate.Length;i++)
            {
                repositoryItemComboBoxCalibrationImageX.Items.Add(RecipeFileIO.TransitionCoordinate[i]);
                repositoryItemComboBoxCalibrationImageY.Items.Add(RecipeFileIO.TransitionCoordinate[i]);
            }
            // System Parameter의 Light Serial 초기화
            rowAiCCommunicationPortName.Properties.Value = repositoryItemComboBoxAiCCommunicationPortName.Items[0].ToString();
            rowAiCCommunicationBaudRate.Properties.Value = repositoryItemComboBoxAiCCommunicationBaudRate.Items[2].ToString();
            rowAiCCommunicationDatabit.Properties.Value = repositoryItemComboBoxAiCCommunicationDatabit.Items[4].ToString();
            rowAiCCommunicationParity.Properties.Value = repositoryItemComboBoxAiCCommunicationParity.Items[0].ToString();
            rowAiCCommunicationStopbit.Properties.Value = repositoryItemComboBoxAiCCommunicationStopbit.Items[0].ToString();            
            rowAiCCommunicationHandshake.Properties.Value = repositoryItemComboBoxAiCCommunicationHandshake.Items[0].ToString();

            rowRemoteIOCommunicationPortName.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationPortName.Items[1].ToString();
            rowRemoteIOCommunicationBaudRate.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationBaudRate.Items[4].ToString();
            rowRemoteIOCommunicationDatabit.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationDatabit.Items[4].ToString();
            rowRemoteIOCommunicationParity.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationParity.Items[0].ToString();
            rowRemoteIOCommunicationStopbit.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationStopbit.Items[0].ToString();
            rowRemoteIOCommunicationHandshake.Properties.Value = repositoryItemComboBoxRemoteIOCommunicationHandshake.Items[0].ToString();

            // System Parameter의 Camera 파라미터 초기화
            _systemParameters._cameraParams.HResolution = Convert.ToInt32(rowCameraHResolution.Properties.Value);
            _systemParameters._cameraParams.VResolution = Convert.ToInt32(rowCameraVResolution.Properties.Value);
            _systemParameters._cameraParams.OnePixelResolution = Convert.ToSingle(rowCameraOnePixelResolution.Properties.Value);
            _systemParameters._cameraParams.FrameRate = Convert.ToInt32(rowCameraFrameRate.Properties.Value);
            _systemParameters._cameraParams.ExposureTime = Convert.ToInt32(rowCameraExposureTime.Properties.Value);
            _systemParameters._cameraParams.Gain = Convert.ToInt32(rowCameraGain.Properties.Value);
            _systemParameters._cameraParams.ImageSensorHSize = Convert.ToSingle(rowCameraImageSensorSizeH.Properties.Value);
            _systemParameters._cameraParams.ImageSensorVSize = Convert.ToSingle(rowCameraImageSensorSizeV.Properties.Value);
            _systemParameters._cameraParams.LensFocusLength = Convert.ToSingle(rowCameraLensFocusLenth.Properties.Value);

            // Calibration Coordinate 파라미터 초기화
            _systemParameters._calibrationParams._CoordinateSwitchEnable = Convert.ToBoolean(rowCoordinateSwitch.Properties.Value);
            rowImageToSystemX.Properties.Value = repositoryItemComboBoxCalibrationImageX.Items[0].ToString();
            rowImageToSystemZ.Properties.Value = repositoryItemComboBoxCalibrationImageY.Items[0].ToString();
            _systemParameters._calibrationParams._imagetoSystemXcoordi = Convert.ToSingle(rowImageToSystemX.Properties.Value);
            _systemParameters._calibrationParams._imagetoSystemYcoordi = Convert.ToSingle(rowImageToSystemX.Properties.Value);
            _systemParameters._calibrationParams._CoordinateCalibrationActive = Convert.ToBoolean(rowCoordinateCalibrationActive.Properties.Value);
            _systemParameters._calibrationParams._Position_Reference_X = Convert.ToSingle(rowReference_X.Properties.Value);
            _systemParameters._calibrationParams._Position_Reference_Y = Convert.ToSingle(rowReference_Y.Properties.Value);
            _systemParameters._calibrationParams._Position_Reference_Z = Convert.ToSingle(rowReference_Z.Properties.Value);
            _systemParameters._calibrationParams._Position_1_X = Convert.ToSingle(rowReferenceP1_X.Properties.Value);
            _systemParameters._calibrationParams._Position_1_Y = Convert.ToSingle(rowReferenceP1_Y.Properties.Value);
            _systemParameters._calibrationParams._Position_1_Z = Convert.ToSingle(rowReferenceP1_Z.Properties.Value);
            _systemParameters._calibrationParams._Position_2_X = Convert.ToSingle(rowReferenceP2_X.Properties.Value);
            _systemParameters._calibrationParams._Position_2_Y = Convert.ToSingle(rowReferenceP2_Y.Properties.Value);
            _systemParameters._calibrationParams._Position_2_Z = Convert.ToSingle(rowReferenceP2_Z.Properties.Value);

            // System Parameter의 Motion 파라미터 초기화
            _systemParameters._motionParams.MenualMoveVelocity = Convert.ToSingle(rowMotionMenaulVelocity.Properties.Value);
            _systemParameters._motionParams.MoveVelocity = Convert.ToSingle(rowMotionMoveVelocity.Properties.Value);
            _systemParameters._motionParams.MoveAcceleration = Convert.ToSingle(rowMotionMoveAcceleration.Properties.Value);
            _systemParameters._motionParams.OneTurnResolutionX = Convert.ToInt32(rowMotionResolutionX.Properties.Value);
            _systemParameters._motionParams.OneTurnResolutionY = Convert.ToInt32(rowMotionResolutionY.Properties.Value);
            _systemParameters._motionParams.OneTurnResolutionZ = Convert.ToInt32(rowMotionResolutionZ.Properties.Value);
            _systemParameters._motionParams.GearRatioX = Convert.ToSingle(rowMotionGearRatioX.Properties.Value);
            _systemParameters._motionParams.GearRatioY = Convert.ToSingle(rowMotionGearRatioY.Properties.Value);
            _systemParameters._motionParams.GearRatioZ = Convert.ToSingle(rowMotionGearRatioZ.Properties.Value);
            _systemParameters._motionParams.BallLeadX = Convert.ToSingle(rowMotionBallLeadX.Properties.Value);
            _systemParameters._motionParams.BallLeadY = Convert.ToSingle(rowMotionBallLeadY.Properties.Value);
            _systemParameters._motionParams.BallLeadZ = Convert.ToSingle(rowMotionBallLeadZ.Properties.Value);

            // System Parameter의 AiC 파라미터 초기화
            _systemParameters._AiCParams.ConnectedNumber = Convert.ToInt32(rowAiCCommunicationCounter.Properties.Value);            
            _systemParameters._AiCParams.SerialParameters.PortName = Convert.ToString(rowAiCCommunicationPortName.Properties.Value);
            _systemParameters._AiCParams.SerialParameters.BaudRates = Convert.ToInt32(rowAiCCommunicationBaudRate.Properties.Value);
            _systemParameters._AiCParams.SerialParameters.Parity = (Parity)Enum.Parse(typeof(Parity), Convert.ToString(rowAiCCommunicationParity.Properties.Value));
            _systemParameters._AiCParams.SerialParameters.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Convert.ToString(rowAiCCommunicationStopbit.Properties.Value));
            _systemParameters._AiCParams.SerialParameters.DataBits = Convert.ToInt32(rowAiCCommunicationDatabit.Properties.Value);
            _systemParameters._AiCParams.SerialParameters.Handshake = (Handshake)Enum.Parse(typeof(Handshake), Convert.ToString(rowAiCCommunicationHandshake.Properties.Value));
            _systemParameters._AiCParams.ConnectedNumber = Convert.ToInt32(rowAiCCommunicationCounter.Properties.Value);
            AiCParams._IDs items = new AiCParams._IDs();            
            items._devicename = Convert.ToString(rowAiC1Properties1.Value);
            items._idNumber = Convert.ToInt32(rowAiC1Properties2.Value);
            _systemParameters._AiCParams.IDs.Add(items);
            items._devicename = Convert.ToString(rowAiC2Properties1.Value);
            items._idNumber = Convert.ToInt32(rowAiC2Properties2.Value);
            _systemParameters._AiCParams.IDs.Add(items);
            items._devicename = Convert.ToString(rowAiC3Properties1.Value);
            items._idNumber = Convert.ToInt32(rowAiC3Properties2.Value);
            _systemParameters._AiCParams.IDs.Add(items);
            items._devicename = Convert.ToString(rowAiC4Properties1.Value);
            items._idNumber = Convert.ToInt32(rowAiC4Properties2.Value);
            _systemParameters._AiCParams.IDs.Add(items);

            // System Parameter의 Remote I/O Serial 초기화
            _systemParameters._remoteIOParams.SerialParameters.PortName = Convert.ToString(rowRemoteIOCommunicationPortName.Properties.Value);
            _systemParameters._remoteIOParams.SerialParameters.BaudRates = Convert.ToInt32(rowRemoteIOCommunicationBaudRate.Properties.Value);
            _systemParameters._remoteIOParams.SerialParameters.Parity = (Parity)Enum.Parse(typeof(Parity), Convert.ToString(rowRemoteIOCommunicationParity.Properties.Value));
            _systemParameters._remoteIOParams.SerialParameters.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Convert.ToString(rowRemoteIOCommunicationStopbit.Properties.Value));
            _systemParameters._remoteIOParams.SerialParameters.DataBits = Convert.ToInt32(rowRemoteIOCommunicationDatabit.Properties.Value);
            _systemParameters._remoteIOParams.SerialParameters.Handshake = (Handshake)Enum.Parse(typeof(Handshake), Convert.ToString(rowRemoteIOCommunicationHandshake.Properties.Value));
            _systemParameters._remoteIOParams.ConnectedNumber = Convert.ToInt32(rowRemoteIOCommunicationCount.Properties.Value);
            RemoteIOParams._IDs remoteitems = new RemoteIOParams._IDs();
            remoteitems._devicename = Convert.ToString(rowRemoteIOInputProperties1.Value);
            remoteitems._idNumber = Convert.ToInt32(rowRemoteIOInputProperties2.Value);
            _systemParameters._remoteIOParams.IDs.Add(remoteitems);
            remoteitems._devicename = Convert.ToString(rowRemoteIOOutputProperties1.Value);
            remoteitems._idNumber = Convert.ToInt32(rowRemoteIOOutputProperties2.Value);
            _systemParameters._remoteIOParams.IDs.Add(remoteitems);
            // System Parameter의 ADMS 파라미터 초기화
            _systemParameters._admsParams._enableCheck = Convert.ToBoolean(rowSystemADMSUse.Properties.Value);
            _systemParameters._bJobWorkInfomationEnable = Convert.ToBoolean(rowSystemJobWorkUse.Properties.Value);
            _systemParameters._admsParams._IpAddress = Convert.ToString(rowSystemADMSIPAddress.Properties.Value);
            _systemParameters._admsParams._port = Convert.ToInt32(rowSystemADMSPort.Properties.Value);
            _systemParameters._admsParams._userID = Convert.ToString(rowSystemADMSUserID.Properties.Value);
            _systemParameters._admsParams._password = Convert.ToString(rowSystemADMSPassWD.Properties.Value);
            _systemParameters._admsParams._equipmentname = Convert.ToString(rowSystemADMSEquipmentDBName.Properties.Value);
            _systemParameters._admsParams._eqpmentID = Convert.ToInt32(rowSystemADMSEquipmentID.Properties.Value);
            _systemParameters._admsParams._dbschemaname = Convert.ToString(rowSystemADMSSchemaName.Properties.Value);
            _systemParameters._admsParams._productname = Convert.ToString(rowSystemADMSProductDBName.Properties.Value);

            // Save Result
            _systemParameters._saveResultLEDMeasurement = Convert.ToBoolean(rowSaveResultLEDMeasurement.Properties.Value);
            _systemParameters._saveResultStatistics = Convert.ToBoolean(rowSaveResultStatistics.Properties.Value);

            _systemParameters._SystemLanguageKoreaUse = Convert.ToBoolean(rowSystemUseLanguage.Properties.Value);
        }
        private void LoadSystemParameters()
        {
            string strSystemFilePath = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName);

            if (File.Exists(strSystemFilePath))
            {
                RecipeFileIO.ReadSystemFile(_systemParameters, strSystemFilePath);
            }
            else
            {
                MessageBox.Show(
                    string.Format(string.Format("시스템 파일이 존재하지 않습니다. 기본 파일을 생성합니다.\r\n{0}", strSystemFilePath), "시스템 파일 생성",
                    MessageBoxButtons.OK, MessageBoxIcon.Information));

                RecipeFileIO.WriteSystemFile(_systemParameters, strSystemFilePath);
            }
            _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("System 설정 로드 완료"));
            UpdateSystemControls();
        }
        private void UpdateSystemControls()
        {
            // Camera Parameters
            rowCameraFriendlyName.Properties.Value = _systemParameters._cameraParams.FriendlyName;
            rowCameraHResolution.Properties.Value = _systemParameters._cameraParams.HResolution;
            rowCameraVResolution.Properties.Value = _systemParameters._cameraParams.VResolution;
            rowCameraOnePixelResolution.Properties.Value = _systemParameters._cameraParams.OnePixelResolution;
            rowCameraFrameRate.Properties.Value = _systemParameters._cameraParams.FrameRate;
            rowCameraExposureTime.Properties.Value = _systemParameters._cameraParams.ExposureTime;
            rowCameraGain.Properties.Value = _systemParameters._cameraParams.Gain;
            rowCameraImageSensorSizeH.Properties.Value = _systemParameters._cameraParams.ImageSensorHSize;
            rowCameraImageSensorSizeV.Properties.Value = _systemParameters._cameraParams.ImageSensorVSize;
            rowCameraLensFocusLenth.Properties.Value = _systemParameters._cameraParams.LensFocusLength;

            // Calibration Coordinate Parameters
            rowCoordinateSwitch.Properties.Value = _systemParameters._calibrationParams._CoordinateSwitchEnable;
            rowImageToSystemX.Properties.Value = _systemParameters._calibrationParams._imagetoSystemXcoordi;
            rowImageToSystemZ.Properties.Value = _systemParameters._calibrationParams._imagetoSystemYcoordi;
            rowCoordinateCalibrationActive.Properties.Value = _systemParameters._calibrationParams._CoordinateCalibrationActive;
            rowReference_X.Properties.Value = _systemParameters._calibrationParams._Position_Reference_X;
            rowReference_Y.Properties.Value = _systemParameters._calibrationParams._Position_Reference_Y;
            rowReference_Z.Properties.Value = _systemParameters._calibrationParams._Position_Reference_Z;
            rowReferenceP1_X.Properties.Value = _systemParameters._calibrationParams._Position_1_X;
            rowReferenceP1_Y.Properties.Value = _systemParameters._calibrationParams._Position_1_Y;
            rowReferenceP1_Z.Properties.Value = _systemParameters._calibrationParams._Position_1_Z;
            rowReferenceP2_X.Properties.Value = _systemParameters._calibrationParams._Position_2_X;
            rowReferenceP2_Y.Properties.Value = _systemParameters._calibrationParams._Position_2_Y;
            rowReferenceP2_Z.Properties.Value = _systemParameters._calibrationParams._Position_2_Z;

            // Motion Parameters
            rowMotionMenaulVelocity.Properties.Value = _systemParameters._motionParams.MenualMoveVelocity;
            rowMotionMoveVelocity.Properties.Value = _systemParameters._motionParams.MoveVelocity;
            rowMotionMoveAcceleration.Properties.Value = _systemParameters._motionParams.MoveAcceleration;
            rowMotionResolutionX.Properties.Value = _systemParameters._motionParams.OneTurnResolutionX;
            rowMotionResolutionY.Properties.Value = _systemParameters._motionParams.OneTurnResolutionY;
            rowMotionResolutionZ.Properties.Value = _systemParameters._motionParams.OneTurnResolutionZ;
            rowMotionGearRatioX.Properties.Value = _systemParameters._motionParams.GearRatioX;
            rowMotionGearRatioY.Properties.Value = _systemParameters._motionParams.GearRatioY;
            rowMotionGearRatioZ.Properties.Value = _systemParameters._motionParams.GearRatioZ;
            rowMotionBallLeadX.Properties.Value = _systemParameters._motionParams.BallLeadX;
            rowMotionBallLeadY.Properties.Value = _systemParameters._motionParams.BallLeadY;
            rowMotionBallLeadZ.Properties.Value = _systemParameters._motionParams.BallLeadZ;

            // AiC Parameters                        
            rowAiCCommunicationPortName.Properties.Value = _systemParameters._AiCParams.SerialParameters.PortName;
            rowAiCCommunicationBaudRate.Properties.Value = _systemParameters._AiCParams.SerialParameters.BaudRates;
            rowAiCCommunicationDatabit.Properties.Value = _systemParameters._AiCParams.SerialParameters.DataBits;
            rowAiCCommunicationStopbit.Properties.Value = _systemParameters._AiCParams.SerialParameters.StopBits;
            rowAiCCommunicationParity.Properties.Value = _systemParameters._AiCParams.SerialParameters.Parity;
            rowAiCCommunicationHandshake.Properties.Value = _systemParameters._AiCParams.SerialParameters.Handshake;
            rowAiCCommunicationCounter.Properties.Value = _systemParameters._AiCParams.ConnectedNumber;
            if (_systemParameters._AiCParams.IDs.Count > 0)
            {
                rowAiC1Properties1.Value = _systemParameters._AiCParams.IDs[0]._devicename;
                rowAiC1Properties2.Value = _systemParameters._AiCParams.IDs[0]._idNumber;
            }
            if (_systemParameters._AiCParams.IDs.Count > 1)
            {
                rowAiC2Properties1.Value = _systemParameters._AiCParams.IDs[1]._devicename;
                rowAiC2Properties2.Value = _systemParameters._AiCParams.IDs[1]._idNumber;
            }
            if (_systemParameters._AiCParams.IDs.Count > 2)
            {
                rowAiC3Properties1.Value = _systemParameters._AiCParams.IDs[2]._devicename;
                rowAiC3Properties2.Value = _systemParameters._AiCParams.IDs[2]._idNumber;
            }
            if (_systemParameters._AiCParams.IDs.Count > 3)
            {
                rowAiC4Properties1.Value = _systemParameters._AiCParams.IDs[3]._devicename;
                rowAiC4Properties2.Value = _systemParameters._AiCParams.IDs[3]._idNumber;
            }

            // Remote I/O Parameters
            rowRemoteIOCommunicationPortName.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.PortName;
            rowRemoteIOCommunicationBaudRate.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.BaudRates;
            rowRemoteIOCommunicationDatabit.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.DataBits;
            rowRemoteIOCommunicationStopbit.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.StopBits;
            rowRemoteIOCommunicationParity.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.Parity;
            rowRemoteIOCommunicationHandshake.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.Handshake;
            rowRemoteIOCommunicationCount.Properties.Value = _systemParameters._remoteIOParams.ConnectedNumber;
            if (_systemParameters._remoteIOParams.IDs.Count > 0)
            {
                rowRemoteIOInputProperties1.Value = _systemParameters._remoteIOParams.IDs[0]._devicename;
                rowRemoteIOInputProperties2.Value = _systemParameters._remoteIOParams.IDs[0]._idNumber;
            }
            if (_systemParameters._remoteIOParams.IDs.Count > 1)
            {
                rowRemoteIOOutputProperties1.Value = _systemParameters._remoteIOParams.IDs[1]._devicename;
                rowRemoteIOOutputProperties2.Value = _systemParameters._remoteIOParams.IDs[1]._idNumber;
            }

            // ADMS Parameters
            rowSystemADMSUse.Properties.Value = _systemParameters._admsParams._enableCheck;
            rowSystemJobWorkUse.Properties.Value = _systemParameters._bJobWorkInfomationEnable;
            rowSystemADMSIPAddress.Properties.Value = _systemParameters._admsParams._IpAddress;
            rowSystemADMSPort.Properties.Value = _systemParameters._admsParams._port;
            rowSystemADMSUserID.Properties.Value = _systemParameters._admsParams._userID;
            rowSystemADMSPassWD.Properties.Value = _systemParameters._admsParams._password;
            rowSystemADMSEquipmentDBName.Properties.Value = _systemParameters._admsParams._equipmentname;
            rowSystemADMSEquipmentID.Properties.Value = _systemParameters._admsParams._eqpmentID;
            rowSystemADMSSchemaName.Properties.Value = _systemParameters._admsParams._dbschemaname;
            rowSystemADMSProductDBName.Properties.Value = _systemParameters._admsParams._productname;

            // Save Result
            
            rowSaveResultLEDMeasurement.Properties.Value = _systemParameters._saveResultLEDMeasurement;            
            rowSaveResultStatistics.Properties.Value = _systemParameters._saveResultStatistics;

            // System Language Check
            rowSystemUseLanguage.Properties.Value = _systemParameters._SystemLanguageKoreaUse;
        }
        private void vGridControlSystemParameters_EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DevExpress.XtraVerticalGrid.VGridControl vGrid = sender as DevExpress.XtraVerticalGrid.VGridControl;
                DevExpress.XtraVerticalGrid.Rows.BaseRow currentRow = vGrid.FocusedRow as DevExpress.XtraVerticalGrid.Rows.BaseRow;

                SetCellValue(currentRow);
            }
        }
        private void SetCellValue(DevExpress.XtraVerticalGrid.Rows.BaseRow currentRow)
        {
            int value = 0;
            float fValue = 0;
            double dfValue = 0;
            string strTemp = string.Empty;

            if (currentRow == rowCameraFriendlyName)
            {
                strTemp = Convert.ToString(rowCameraFriendlyName.Properties.Value);

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.FriendlyName = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 이름이 {0}로 변경되었습니다.", _systemParameters._cameraParams.FriendlyName));
            }
            else if (currentRow == rowCameraHResolution)
            {
                value = Convert.ToInt32(rowCameraHResolution.Properties.Value);

                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 가로 해상도는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraHResolution.Properties.Value = _systemParameters._cameraParams.HResolution;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.HResolution = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 가로 해상도가 {0}로 변경되었습니다.", _systemParameters._cameraParams.HResolution.ToString()));
            }
            else if (currentRow == rowCameraVResolution)
            {
                value = Convert.ToInt32(rowCameraVResolution.Properties.Value);

                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 세로 해상도는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraVResolution.Properties.Value = _systemParameters._cameraParams.VResolution;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.VResolution = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 세로 해상도가 {0}로 변경되었습니다.", _systemParameters._cameraParams.VResolution.ToString()));
            }
            else if (currentRow == rowCameraOnePixelResolution)
            {
                fValue = Convert.ToSingle(rowCameraOnePixelResolution.Properties.Value);

                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 한 픽셀 크기는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraOnePixelResolution.Properties.Value = _systemParameters._cameraParams.OnePixelResolution;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.OnePixelResolution = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 픽셀 해상도가 {0}로 변경되었습니다.", _systemParameters._cameraParams.OnePixelResolution.ToString()));
            }
            else if (currentRow == rowCameraExposureTime)
            {
                value = Convert.ToInt32(rowCameraExposureTime.Properties.Value);

                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 노출 시간은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraExposureTime.Properties.Value = _systemParameters._cameraParams.ExposureTime;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.ExposureTime = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 노출시간이 {0}로 변경되었습니다.", _systemParameters._cameraParams.ExposureTime.ToString()));
            }
            else if (currentRow == rowCameraFrameRate)
            {
                value = Convert.ToInt32(rowCameraFrameRate.Properties.Value);

                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 프레임비(fps)는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraFrameRate.Properties.Value = _systemParameters._cameraParams.FrameRate;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.FrameRate = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 프레임이 {0}로 변경되었습니다.", _systemParameters._cameraParams.FrameRate.ToString()));
            }
            else if (currentRow == rowCameraGain)
            {
                value = Convert.ToInt32(rowCameraGain.Properties.Value);

                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라의 게인(fps)은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraGain.Properties.Value = _systemParameters._cameraParams.Gain;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.Gain = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 게인이 {0}로 변경되었습니다.", _systemParameters._cameraParams.Gain.ToString()));
            }
            else if (currentRow == rowCameraImageSensorSizeH)
            {
                fValue = Convert.ToSingle(rowCameraImageSensorSizeH.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라 센서의 가로 크기(mm)는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraImageSensorSizeH.Properties.Value = _systemParameters._cameraParams.ImageSensorHSize;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.ImageSensorHSize = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 센서의 가로 크기가 {0}로 변경되었습니다.", _systemParameters._cameraParams.ImageSensorHSize.ToString()));
            }
            else if (currentRow == rowCameraImageSensorSizeV)
            {
                fValue = Convert.ToSingle(rowCameraImageSensorSizeV.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라 센서의 세로 크기(mm)는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraImageSensorSizeV.Properties.Value = _systemParameters._cameraParams.ImageSensorVSize;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.ImageSensorVSize = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("카메라 센서의 세로 크기가 {0}로 변경되었습니다.", _systemParameters._cameraParams.ImageSensorVSize.ToString()));
            }
            else if (currentRow == rowCameraLensFocusLenth)
            {
                fValue = Convert.ToSingle(rowCameraLensFocusLenth.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n카메라 렌즈의 초점거리(mm)는 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowCameraLensFocusLenth.Properties.Value = _systemParameters._cameraParams.LensFocusLength;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._cameraParams.LensFocusLength = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("렌즈 초점거리가 {0}로 변경되었습니다.", _systemParameters._cameraParams.LensFocusLength.ToString()));
            }
            else if (currentRow == rowCoordinateSwitch)
            {
                bool check = Convert.ToBoolean(rowCoordinateSwitch.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._CoordinateSwitchEnable = check;
            }
            else if (currentRow == rowImageToSystemX)
            {
                fValue = Convert.ToSingle(rowImageToSystemX.Properties.Value);

                if (fValue == 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n좌표계 보정값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowImageToSystemX.Properties.Value = _systemParameters._calibrationParams._imagetoSystemXcoordi;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._imagetoSystemXcoordi = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("이미지 X 좌표계 -> X 실좌표계 극성값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._imagetoSystemXcoordi.ToString()));
            }
            else if (currentRow == rowImageToSystemZ)
            {
                fValue = Convert.ToSingle(rowImageToSystemZ.Properties.Value);

                if (fValue == 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n좌표계 보정값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowImageToSystemZ.Properties.Value = _systemParameters._calibrationParams._imagetoSystemYcoordi;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._imagetoSystemYcoordi = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("이미지 Y 좌표계 -> Z 실좌표계 극성값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._imagetoSystemYcoordi.ToString()));
            }
            else if (currentRow == rowCoordinateCalibrationActive)
            {
                bool check = Convert.ToBoolean(rowCoordinateCalibrationActive.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._CoordinateCalibrationActive = check;
            }
            else if (currentRow == rowReference_X)
            {
                dfValue = Convert.ToDouble(rowReference_X.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_Reference_X = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("X축 이동 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_Reference_X.ToString()));
            }
            else if (currentRow == rowReference_Y)
            {
                dfValue = Convert.ToDouble(rowReference_Y.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_Reference_Y = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Y축 이동 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_Reference_Y.ToString()));
            }
            else if (currentRow == rowReference_Z)
            {
                dfValue = Convert.ToDouble(rowReference_Z.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_Reference_Z = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Z축 이동 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_Reference_Z.ToString()));
            }
            else if (currentRow == rowReferenceP1_X)
            {
                dfValue = Convert.ToDouble(rowReferenceP1_X.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_1_X = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P1 X축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_1_X.ToString()));
            }
            else if (currentRow == rowReferenceP1_Y)
            {
                dfValue = Convert.ToDouble(rowReferenceP1_Y.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_1_Y = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P1 Y축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_1_Y.ToString()));
            }
            else if (currentRow == rowReferenceP1_Z)
            {
                dfValue = Convert.ToDouble(rowReferenceP1_Z.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_1_Z = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P1 Z축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_1_Z.ToString()));
            }
            else if (currentRow == rowReferenceP2_X)
            {
                dfValue = Convert.ToDouble(rowReferenceP2_X.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_2_X = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P2 X축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_2_X.ToString()));
            }
            else if (currentRow == rowReferenceP2_Y)
            {
                dfValue = Convert.ToDouble(rowReferenceP2_Y.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_2_Y = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P2 Y축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_2_Y.ToString()));
            }
            else if (currentRow == rowReferenceP2_Z)
            {
                dfValue = Convert.ToDouble(rowReferenceP2_Z.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._calibrationParams._Position_2_Z = dfValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("교정점 P2 Z축 기준값이 {0}로 변경되었습니다.", _systemParameters._calibrationParams._Position_2_Z.ToString()));
            } 
            else if (currentRow == rowMotionMoveVelocity)
            {
                fValue = Convert.ToSingle(rowMotionMoveVelocity.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n이동 속도 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionMoveVelocity.Properties.Value = _systemParameters._motionParams.MoveVelocity;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.MoveVelocity = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("이동 속도 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.MoveVelocity.ToString()));
            }
            else if (currentRow == rowMotionMenaulVelocity)
            {
                fValue = Convert.ToSingle(rowMotionMenaulVelocity.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n수동 이동 속도 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionMenaulVelocity.Properties.Value = _systemParameters._motionParams.MenualMoveVelocity;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.MenualMoveVelocity = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("수동이동 속도 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.MenualMoveVelocity.ToString()));
            }
            else if (currentRow == rowMotionMoveAcceleration)
            {
                fValue = Convert.ToSingle(rowMotionMoveAcceleration.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n이동 가속도 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionMoveAcceleration.Properties.Value = _systemParameters._motionParams.MoveAcceleration;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.MoveAcceleration = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("이동 가속도 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.MoveAcceleration.ToString()));
            }
            else if (currentRow == rowMotionResolutionX)
            {
                value =Convert.ToInt32(rowMotionGearRatioX.Properties.Value);
                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nX축 분해능 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionResolutionX.Properties.Value = _systemParameters._motionParams.OneTurnResolutionX;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.OneTurnResolutionX = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("X축 분해능 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.OneTurnResolutionX.ToString()));
            }
            else if (currentRow == rowMotionResolutionX)
            {
                value = Convert.ToInt32(rowMotionGearRatioY.Properties.Value);
                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nY축 분해능 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionResolutionY.Properties.Value = _systemParameters._motionParams.OneTurnResolutionY;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.OneTurnResolutionY = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Y축 분해능 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.OneTurnResolutionY.ToString()));
            }
            else if (currentRow == rowMotionResolutionZ)
            {
                value = Convert.ToInt32(rowMotionGearRatioZ.Properties.Value);
                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nZ축 분해능 값은 0보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionResolutionZ.Properties.Value = _systemParameters._motionParams.OneTurnResolutionZ;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.OneTurnResolutionZ = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Z축 분해능 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.OneTurnResolutionZ.ToString()));
            }
            else if (currentRow == rowMotionGearRatioX)
            {
                fValue = Convert.ToSingle(rowMotionGearRatioX.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nX축 기어비 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionGearRatioX.Properties.Value = _systemParameters._motionParams.GearRatioX;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.GearRatioX = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("X축 기어비 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.GearRatioX.ToString()));
            }
            else if (currentRow == rowMotionGearRatioY)
            {
                fValue = Convert.ToSingle(rowMotionGearRatioY.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nY축 기어비 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionGearRatioY.Properties.Value = _systemParameters._motionParams.GearRatioY;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.GearRatioY = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Y축 기어비 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.GearRatioY.ToString()));
            }
            else if (currentRow == rowMotionGearRatioZ)
            {
                fValue = Convert.ToSingle(rowMotionGearRatioZ.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nZ축 기어비 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionGearRatioZ.Properties.Value = _systemParameters._motionParams.GearRatioZ;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.GearRatioZ = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Z축 기어비 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.GearRatioZ.ToString()));
            }
            else if (currentRow == rowMotionBallLeadX)
            {
                fValue = Convert.ToSingle(rowMotionBallLeadX.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nX축 Ball Lead 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionBallLeadX.Properties.Value = _systemParameters._motionParams.BallLeadX;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.BallLeadX = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("X축 볼리드 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.BallLeadX.ToString()));
            }
            else if (currentRow == rowMotionBallLeadY)
            {
                fValue = Convert.ToSingle(rowMotionBallLeadY.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nY축 Ball Lead 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionBallLeadY.Properties.Value = _systemParameters._motionParams.BallLeadY;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.BallLeadY = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Y축 볼리드 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.BallLeadY.ToString()));
            }
            else if (currentRow == rowMotionBallLeadZ)
            {
                fValue = Convert.ToSingle(rowMotionGearRatioZ.Properties.Value);
                if (fValue <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nZ축 Ball Lead 값은 1보다 큰 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowMotionBallLeadZ.Properties.Value = _systemParameters._motionParams.BallLeadZ;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._motionParams.BallLeadZ = fValue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("Z축 볼리드 값이 {0}로 변경되었습니다.", _systemParameters._motionParams.BallLeadZ.ToString()));
            }
            else if (currentRow == rowAiCCommunicationBaudRate)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowAiCCommunicationBaudRate.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationBaudRate.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxAiCCommunicationBaudRate.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nBaudRates 값은 9600, 19200, 38400, 57600, 115200 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowAiCCommunicationBaudRate.Properties.Value = _systemParameters._AiCParams.SerialParameters.BaudRates;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._AiCParams.SerialParameters.BaudRates = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 속도를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.BaudRates.ToString()));
            }
            else if (currentRow == rowAiCCommunicationStopbit)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowAiCCommunicationStopbit.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationStopbit.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxAiCCommunicationStopbit.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n정지 비트 값은 None, One, Two, One5 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationStopbit.Properties.Value = _systemParameters._panelmeterParams.SerialParameters.StopBits;
                    rowAiCCommunicationStopbit.Properties.Value = Enum.GetName(typeof(StopBits), (StopBits)_systemParameters._AiCParams.SerialParameters.StopBits);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                //_systemParameters._panelmeterParams.SerialParameters.StopBits = value;
                _systemParameters._AiCParams.SerialParameters.StopBits = (StopBits)Enum.Parse(typeof(StopBits), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 정지 비트를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.StopBits.ToString()));
            }
            else if (currentRow == rowAiCCommunicationDatabit)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowAiCCommunicationDatabit.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationDatabit.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxAiCCommunicationDatabit.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDataBits 값은 4, 5, 6, 7, 8 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowAiCCommunicationDatabit.Properties.Value = _systemParameters._AiCParams.SerialParameters.DataBits;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._AiCParams.SerialParameters.DataBits = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 데이터 비트를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.DataBits.ToString()));
            }
            else if (currentRow == rowAiCCommunicationHandshake)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowAiCCommunicationHandshake.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationHandshake.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxAiCCommunicationHandshake.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n흐름 제어 값은 None, XonXoff, RequestToSend, RequestToSendXonXoff 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationHandshake.Properties.Value = _systemParameters._panelmeterParams.SerialParameters.Handshake;
                    rowAiCCommunicationHandshake.Properties.Value = Enum.GetName(typeof(Handshake), (Handshake)_systemParameters._AiCParams.SerialParameters.Handshake);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                //_systemParameters._panelmeterParams.SerialParameters.Handshake = strTemp;
                _systemParameters._AiCParams.SerialParameters.Handshake = (Handshake)Enum.Parse(typeof(Handshake), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 흐름 제어를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.Handshake.ToString()));
            }
            else if (currentRow == rowAiCCommunicationParity)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowAiCCommunicationParity.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationParity.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxAiCCommunicationParity.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n패리티 값은 None, Odd, Even, Mark, Space 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationParity.Properties.Value = _systemParameters._panelmeterParams.SerialParameters.Parity;
                    rowAiCCommunicationParity.Properties.Value = Enum.GetName(typeof(Parity), (Parity)_systemParameters._AiCParams.SerialParameters.Parity);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                //_systemParameters._panelmeterParams.SerialParameters.Parity = strTemp;
                _systemParameters._AiCParams.SerialParameters.Parity = (Parity)Enum.Parse(typeof(Parity), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 parity를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.Parity.ToString()));
            }
            else if (currentRow == rowAiCCommunicationPortName)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowAiCCommunicationPortName.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxAiCCommunicationPortName.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxAiCCommunicationPortName.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n통신 포트 값은 COM1~20 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowAiCCommunicationPortName.Properties.Value = _systemParameters._AiCParams.SerialParameters.PortName;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._AiCParams.SerialParameters.PortName = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 포트를 {0}로 변경되었습니다.", _systemParameters._AiCParams.SerialParameters.PortName));
            }
            else if (currentRow == rowAiCCommunicationCounter)
            {
                ///*
                value = Convert.ToInt32(rowAiCCommunicationCounter.Properties.Value);
                if (value < 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n AiC 연결대수는 0보다 크거나 같은 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowAiCCommunicationCounter.Properties.Value = _systemParameters._AiCParams.ConnectedNumber;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._AiCParams.ConnectedNumber = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC 통신 연결 대수를 {0}로 변경되었습니다.", _systemParameters._AiCParams.ConnectedNumber.ToString()));
                //*/
            }
            else if (currentRow == rowAiCXAxis)
            {
                AiCParams._IDs tempIDs = new AiCParams._IDs();
                tempIDs._devicename = Convert.ToString(rowAiC1Properties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowAiC1Properties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nAiC 모션 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._AiCParams.IDs.Count > 0)
                    {
                        rowAiC1Properties1.Value = _systemParameters._AiCParams.IDs[0]._devicename;
                        rowAiC1Properties2.Value = _systemParameters._AiCParams.IDs[0]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._AiCParams.IDs.Count > 0)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._AiCParams.IDs[0] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC X축 ID를 {0}로 변경되었습니다.", _systemParameters._AiCParams.IDs[0].ToString()));
            }
            else if (currentRow == rowAiCYAxis)
            {
                AiCParams._IDs tempIDs = new AiCParams._IDs();
                tempIDs._devicename = Convert.ToString(rowAiC2Properties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowAiC2Properties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nAiC 모션 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._AiCParams.IDs.Count > 1)
                    {
                        rowAiC2Properties1.Value = _systemParameters._AiCParams.IDs[1]._devicename;
                        rowAiC2Properties2.Value = _systemParameters._AiCParams.IDs[1]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._AiCParams.IDs.Count > 1)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._AiCParams.IDs[1] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC Y축 ID를 {0}로 변경되었습니다.", _systemParameters._AiCParams.IDs[1].ToString()));
            }
            else if (currentRow == rowAiCZAxis)
            {
                AiCParams._IDs tempIDs = new AiCParams._IDs();
                tempIDs._devicename = Convert.ToString(rowAiC3Properties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowAiC3Properties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nAiC 모션 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._AiCParams.IDs.Count > 2)
                    {
                        rowAiC3Properties1.Value = _systemParameters._AiCParams.IDs[2]._devicename;
                        rowAiC3Properties2.Value = _systemParameters._AiCParams.IDs[2]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._AiCParams.IDs.Count > 2)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._AiCParams.IDs[2] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC Z축 ID를 {0}로 변경되었습니다.", _systemParameters._AiCParams.IDs[2].ToString()));
            }
            else if (currentRow == rowAiCLoaderAxis)
            {
                AiCParams._IDs tempIDs = new AiCParams._IDs();
                tempIDs._devicename = Convert.ToString(rowAiC4Properties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowAiC4Properties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nAiC 모션 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._AiCParams.IDs.Count > 3)
                    {
                        rowAiC4Properties1.Value = _systemParameters._AiCParams.IDs[3]._devicename;
                        rowAiC4Properties2.Value = _systemParameters._AiCParams.IDs[3]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._AiCParams.IDs.Count > 3)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._AiCParams.IDs[3] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("AiC R축 ID를 {0}로 변경되었습니다.", _systemParameters._AiCParams.IDs[3].ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationStopbit)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowRemoteIOCommunicationStopbit.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationStopbit.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxRemoteIOCommunicationStopbit.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n정지 비트 값은 None, One, Two, One5 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPhotoCommunicationStopbit.Properties.Value = _systemParameters._photoParams.SerialParameters.StopBits;
                    rowRemoteIOCommunicationStopbit.Properties.Value = Enum.GetName(typeof(StopBits), (StopBits)_systemParameters._remoteIOParams.SerialParameters.StopBits);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                //_systemParameters._photoParams.SerialParameters.StopBits = value;
                _systemParameters._remoteIOParams.SerialParameters.StopBits = (StopBits)Enum.Parse(typeof(StopBits), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 정지 비트를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.StopBits.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationBaudRate)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowRemoteIOCommunicationBaudRate.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationBaudRate.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxRemoteIOCommunicationBaudRate.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nBaudRates 값은 9600, 19200, 38400, 57600, 115200 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowRemoteIOCommunicationBaudRate.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.BaudRates;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.SerialParameters.BaudRates = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 속도를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.BaudRates.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationDatabit)
            {
                bool IsValidate = false;
                value = Convert.ToInt32(rowRemoteIOCommunicationDatabit.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationDatabit.Items.Count; ++i)
                {
                    if (value == Convert.ToInt32(repositoryItemComboBoxRemoteIOCommunicationDatabit.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDataBits 값은 4, 5, 6, 7, 8 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowRemoteIOCommunicationDatabit.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.DataBits;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.SerialParameters.DataBits = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 데이터 비트를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.DataBits.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationHandshake)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowRemoteIOCommunicationHandshake.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationHandshake.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxRemoteIOCommunicationHandshake.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n흐름 제어 값은 None, XonXoff, RequestToSend, RequestToSendXonXoff 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPhotoCommunicationHandshake.Properties.Value = _systemParameters._photoParams.SerialParameters.Handshake;
                    rowRemoteIOCommunicationHandshake.Properties.Value = Enum.GetName(typeof(Handshake), (Handshake)_systemParameters._remoteIOParams.SerialParameters.Handshake);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                //_systemParameters._photoParams.SerialParameters.Handshake = strTemp;
                _systemParameters._remoteIOParams.SerialParameters.Handshake = (Handshake)Enum.Parse(typeof(Handshake), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 흐름제어를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.Handshake.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationParity)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowRemoteIOCommunicationParity.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationParity.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxRemoteIOCommunicationParity.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n패리티 값은 None, Odd, Even, Mark, Space 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPhotoCommunicationParity.Properties.Value = _systemParameters._photoParams.SerialParameters.Parity;
                    rowRemoteIOCommunicationParity.Properties.Value = Enum.GetName(typeof(Parity), (Parity)_systemParameters._remoteIOParams.SerialParameters.Parity);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.SerialParameters.Parity = (Parity)Enum.Parse(typeof(Parity), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 Parity를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.Parity.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationPortName)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowRemoteIOCommunicationPortName.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationPortName.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxRemoteIOCommunicationPortName.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n통신 포트 값은 COM1~20 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowRemoteIOCommunicationPortName.Properties.Value = _systemParameters._remoteIOParams.SerialParameters.PortName;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.SerialParameters.PortName = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 포트를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.PortName));
            }
            else if (currentRow == rowRemoteIOCommunicationStopbit)
            {
                bool IsValidate = false;
                strTemp = Convert.ToString(rowRemoteIOCommunicationStopbit.Properties.Value);

                for (int i = 0; i < repositoryItemComboBoxRemoteIOCommunicationStopbit.Items.Count; ++i)
                {
                    if (strTemp == Convert.ToString(repositoryItemComboBoxRemoteIOCommunicationStopbit.Items[i]))
                    {
                        IsValidate = true;
                        break;
                    }
                }

                if (!IsValidate)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n정지 비트 값은 None, One, Two, OnePointFive 중 하나입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowRemoteIOCommunicationStopbit.Properties.Value = Enum.GetName(typeof(StopBits), (StopBits)_systemParameters._remoteIOParams.SerialParameters.StopBits);
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.SerialParameters.StopBits = (StopBits)Enum.Parse(typeof(StopBits), strTemp);
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 정지 비트를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.SerialParameters.StopBits.ToString()));
            }
            else if (currentRow == rowRemoteIOCommunicationCount)
            {
                ///*
                value = Convert.ToInt32(rowRemoteIOCommunicationCount.Properties.Value);
                if (value < 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n 리모트I/O 연결대수는 0보다 크거나 같은 값입니다.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowRemoteIOCommunicationCount.Properties.Value = _systemParameters._remoteIOParams.ConnectedNumber;
                    vGridControlSystemParameters.Refresh();
                    return;
                }

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._remoteIOParams.ConnectedNumber = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 통신 연결 대수를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.ConnectedNumber.ToString()));
                //*/
            }
            else if (currentRow == rowRemoteIOInputIDs)
            {
                ///*
                RemoteIOParams._IDs tempIDs = new RemoteIOParams._IDs();
                tempIDs._devicename = Convert.ToString(rowRemoteIOInputProperties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowRemoteIOInputProperties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n판넬메타 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._remoteIOParams.IDs.Count > 0)
                    {
                        rowRemoteIOInputProperties1.Value = _systemParameters._remoteIOParams.IDs[0]._devicename;
                        rowRemoteIOInputProperties2.Value = _systemParameters._remoteIOParams.IDs[0]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._remoteIOParams.IDs.Count > 0)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._remoteIOParams.IDs[0] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 모듈 1 ID를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.IDs[0].ToString()));
                //*/
            }
            else if (currentRow == rowRemoteIOOutputIDs)
            {
                ///*
                RemoteIOParams._IDs tempIDs = new RemoteIOParams._IDs();
                tempIDs._devicename = Convert.ToString(rowRemoteIOOutputProperties1.Value);
                tempIDs._idNumber = Convert.ToInt32(rowRemoteIOOutputProperties2.Value);

                if (tempIDs._devicename == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\n판넬메타 이름 설정을 입력하세요.", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //rowPanelMeterCommunicationPortName.Properties.Value = _systemParameters._panelmeterParams.;
                    if (_systemParameters._remoteIOParams.IDs.Count > 1)
                    {
                        rowRemoteIOOutputProperties1.Value = _systemParameters._remoteIOParams.IDs[1]._devicename;
                        rowRemoteIOOutputProperties2.Value = _systemParameters._remoteIOParams.IDs[1]._idNumber;
                    }
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                if (_systemParameters._remoteIOParams.IDs.Count > 1)
                {
                    simpleButtonSystemFileSave.Enabled = true;
                    _systemParameters._remoteIOParams.IDs[1] = tempIDs;
                }
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("RemoteIO 모듈 2 ID를 {0}로 변경되었습니다.", _systemParameters._remoteIOParams.IDs[1].ToString()));
                //*/
            }
            else if (currentRow == rowSystemADMSUse)
            {
                bool check = Convert.ToBoolean(rowSystemADMSUse.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._enableCheck = check;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS 사용여부가 {0}로 변경되었습니다.", _systemParameters._admsParams._enableCheck.ToString()));
            }
            else if (currentRow == rowSystemJobWorkUse)
            {
                bool check = Convert.ToBoolean(rowSystemJobWorkUse.Properties.Value);
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._bJobWorkInfomationEnable = check;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("작업지시서 DB 사용여부가 {0}로 변경되었습니다.", _systemParameters._bJobWorkInfomationEnable.ToString()));
            }
            else if (currentRow == rowSystemADMSIPAddress)
            {
                strTemp = Convert.ToString(rowSystemADMSIPAddress.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDB Ip주소를 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSIPAddress.Properties.Value = _systemParameters._admsParams._IpAddress;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._IpAddress = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS IP 주소가 {0}로 변경되었습니다.", _systemParameters._admsParams._IpAddress));
            }
            else if (currentRow == rowSystemADMSPort)
            {
                value = Convert.ToInt32(rowSystemADMSPort.Properties.Value);
                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDB 포트를 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSPort.Properties.Value = _systemParameters._admsParams._port;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._port = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB 포트가 {0}로 변경되었습니다.", _systemParameters._admsParams._port.ToString()));
            }
            else if (currentRow == rowSystemADMSUserID)
            {
                strTemp = Convert.ToString(rowSystemADMSUserID.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDB User ID를 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSUserID.Properties.Value = _systemParameters._admsParams._userID;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._userID = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB User ID가 {0}로 변경되었습니다.", _systemParameters._admsParams._userID));
            }
            else if (currentRow == rowSystemADMSPassWD)
            {
                strTemp = Convert.ToString(rowSystemADMSPassWD.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDB User ID를 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSPassWD.Properties.Value = _systemParameters._admsParams._password;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._password = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB User 비밀번호가 {0}로 변경되었습니다.", _systemParameters._admsParams._password));
            }
            else if (currentRow == rowSystemADMSEquipmentID)
            {
                value = Convert.ToInt32(rowSystemADMSEquipmentID.Properties.Value);
                if (value <= 0)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nEquipmentID를 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSEquipmentID.Properties.Value = _systemParameters._admsParams._eqpmentID;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._eqpmentID = value;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB eqpmentID가 {0}로 변경되었습니다.", _systemParameters._admsParams._eqpmentID.ToString()));
            }
            else if (currentRow == rowSystemADMSEquipmentDBName)
            {
                strTemp = Convert.ToString(rowSystemADMSEquipmentDBName.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nEquipmentDB 이름 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSEquipmentDBName.Properties.Value = _systemParameters._admsParams._equipmentname;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._equipmentname = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB eqpment name이 {0}로 변경되었습니다.", _systemParameters._admsParams._equipmentname));
            }
            else if (currentRow == rowSystemADMSSchemaName)
            {
                strTemp = Convert.ToString(rowSystemADMSSchemaName.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nDB schema 이름 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSSchemaName.Properties.Value = _systemParameters._admsParams._dbschemaname;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._dbschemaname = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB schema name이 {0}로 변경되었습니다.", _systemParameters._admsParams._dbschemaname));
            }
            else if (currentRow == rowSystemADMSProductDBName)
            {
                strTemp = Convert.ToString(rowSystemADMSProductDBName.Properties.Value);
                if (strTemp == string.Empty)
                {
                    MessageBox.Show("잘못된 값을 입력했습니다.\r\nProduct DB 이름 입력하세요", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    rowSystemADMSProductDBName.Properties.Value = _systemParameters._admsParams._productname;
                    vGridControlSystemParameters.Refresh();
                    return;
                }
                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._admsParams._productname = strTemp;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("ADMS DB product name이 {0}로 변경되었습니다.", _systemParameters._admsParams._productname));
            }
            else if (currentRow == rowSaveResultLEDMeasurement)
            {
                bool bvalue = false;
                bvalue = Convert.ToBoolean(rowSaveResultLEDMeasurement.Properties.Value);

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._saveResultLEDMeasurement = bvalue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("검사 결과 데이터 저장 유무를 {0}로 변경되었습니다.", _systemParameters._saveResultLEDMeasurement.ToString()));
            }
            else if (currentRow == rowSaveResultStatistics)
            {
                bool bvalue = false;
                bvalue = Convert.ToBoolean(rowSaveResultStatistics.Properties.Value);

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._saveResultStatistics = bvalue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("통계 데이터 저장 유무를 {0}로 변경되었습니다.", _systemParameters._saveResultStatistics.ToString()));
            }
            else if (currentRow == rowSystemUseLanguage)
            {
                bool bvalue = false;
                bvalue = Convert.ToBoolean(rowSystemUseLanguage.Properties.Value);

                simpleButtonSystemFileSave.Enabled = true;
                _systemParameters._SystemLanguageKoreaUse = bvalue;
                _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("한국어 언어 사용 유무를 {0}로 변경되었습니다.", _systemParameters._SystemLanguageKoreaUse.ToString()));
                if (!_systemParameters._SystemLanguageKoreaUse)
                    MessageBox.Show("언어가 변경되었습니다.\r\n프로그램을 재시작이 필요합니다.", "알람", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("System Language changed now.\r\nMust restart program.", "Alarm", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void simpleButtonSystemFileSave_Click(object sender, EventArgs e)
        {
            string strSavePath = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName);

            if (MessageBox.Show(string.Format("시스템 파일을 저장하시겠습니까?\r\n저장 위치:{0}", strSavePath), "시스템파일 저장", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                try
                {
                    if (!Directory.Exists(SystemDirectoryParams.SystemFolderPath))
                    {
                        Directory.CreateDirectory(strSavePath);
                    }

                    // Recipe File
                    RecipeFileIO.WriteSystemFile(_systemParameters, strSavePath);
                    _log.WriteLog(LogLevel.Info, LogClass.SystemEditor.ToString(), string.Format("시스템 파라미터가 저장되었습니다."));                    
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }
        }
    }
}