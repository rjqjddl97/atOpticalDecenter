using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using RecipeManager;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CustomPages;
using Basler;
using LogLibrary;
using ImageLibrary;
using PhotoProduct;
using atOpticalDecenter;
using AiCControlLibrary;
using PhotoDBLibrary;

namespace atOpticalDecenter
{
    public partial class atOpticalDecenter : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        WorkParams _workParams = new WorkParams();
        SystemParams _systemParams = new SystemParams();
        BaslerCamera _Camera = new BaslerCamera();

        private LoginForm mLogin = new LoginForm();
        private Log mLog = new Log();
        private List<LogData> mLogList = new List<LogData>();
        List<Blob> _blobs = new List<Blob>();
        List<Blob> _MenaulInspectLed1blobs = new List<Blob>();
        List<Blob> _MenaulInspectLed2blobs = new List<Blob>();
        private const int MAX_LOG_QUEUE_COUNT = 10;

        const int MAX_STATISTICS = 10;

        StatisticParams _statistics = new StatisticParams(MAX_STATISTICS);
        StatisticParams _currentStatistics = new StatisticParams(MAX_STATISTICS);
        Dictionary<string, TackParams> _dicTackTimes = new Dictionary<string, TackParams>();
        Dictionary<int, double> _PanelMedtaData = new Dictionary<int, double>();
        Stopwatch CheckTackTime = new Stopwatch();
        DateTime _inspectionStartTime = new DateTime();
        public AiCControlLibrary.SerialCommunication.Control.CommunicationManager _mMotionControlCommManager = null;
        public ARMLibrary.SerialCommunication.Control.CommunicationManager _mRemteIOCommManager = null;
        public event Action ImageUpdateEvents;

        DBControl _JobWorkDbCtrl = new DBControl();
        ADMSEquipmentInfo _admsEquipment = new ADMSEquipmentInfo();
        ADMSProductInfo _admsProduct = new ADMSProductInfo();
        BackgroundWorker _bwMotionHome = new BackgroundWorker();

        bool _isInspecting = false;
        bool _isInspectionDone = false;
        bool _isInsepctionResult = false;
        bool _isInspectError = false;
        System.Drawing.Image _sourceImage = null;
        System.Drawing.Image _resultImage = null;

        System.Drawing.Image _BaseXImage = null;
        System.Drawing.Image _ActuatorXImage = null;
        System.Drawing.Image _BaseYImage = null;
        System.Drawing.Image _ActuatorYImage = null;
        System.Drawing.Image _BaseZImage = null;
        System.Drawing.Image _ActuatorZImage = null;

        bool _isContinuousShot = false;
        bool _isCameraOpen = false;

        bool _isOpticalMeasurement = false;
        bool _isAutoInspectMeasurement = false;

        bool _isCropMove = false;
        bool _isGrabbed = false;
        bool _isImageFitSize = false;
        bool _patternMatching = false;
        bool _isAreaMove = false;
        bool _isSetROICheck = false;
        bool _IsLogin = false;
        bool _IsReciepLoad = false;
        bool _IsHommingFinished = false;
        
        int _frameCount = 0;
        bool IsCameraOpen = false;
        float _fHScrollPos = 0f;
        float _fVScrollPos = 0f;

        PointF _fptCropStart = new PointF();
        PointF _fptCropEnd = new PointF();
        PointF _fptMoveStart = new PointF();
        RectangleF _frtCrop = new RectangleF();
        PointF _fptAreaStart = new PointF();
        PointF _fptAreaEnd = new PointF();
        RectangleF _frtArearect = new RectangleF();
        ManualResetEvent _waitHandle = new ManualResetEvent(false);

        BackgroundWorker _backgroundWorkerOpticalDecenterInspection = new BackgroundWorker();

        string Cameraname;

        double[] _ImageHist_W = null;
        double[] _ImageHist_H = null;
        public System.Drawing.Image Spot1BlobImage = null;
        public System.Drawing.Image Spot2BlobImage = null;

        public atOpticalDecenter()
        {
            InitializeComponent();

            // 통신 초기화
            _mMotionControlCommManager = new AiCControlLibrary.SerialCommunication.Control.CommunicationManager();
            _mRemteIOCommManager = new ARMLibrary.SerialCommunication.Control.CommunicationManager();
            //mMT4xPanelMeterCommunicationManager = new MT4WContorlLibrary.SerialCommunication.Control.CommunicationManager();
            ////plcRemoteCtrl.SetCommunicateManager(ref mPLCCommunicateManager);
            //sensorCalibrationControl.SetCommunicateManager(mPhotoCommunicationManager);
            //panelMeterControl.SetCommunicateManager(ref mMT4xPanelMeterCommunicationManager);
        }
        private void barButtonItemSystemFolderPathSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            SystemDirectorySetting system = new SystemDirectorySetting();

            if (system.ShowDialog(this) == DialogResult.OK)
            {
                if (MessageBox.Show("시스템 폴더 경로를 변경하시겠습니까?", "시스템 폴더 경로 변경", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    // 변경된 경로로 로그파일 저장
                    //_log.SetLogPath(system.LogFolderPath);

                    SystemDirectoryParams.RootFolderPath = system.RootFolderPath;
                    SystemDirectoryParams.LogFolderPath = system.LogFolderPath;
                    SystemDirectoryParams.RecipeFolderPath = system.RecipeFolderPath;
                    SystemDirectoryParams.ResultFolderPath = system.ResultFolderPath;
                    SystemDirectoryParams.SystemFolderPath = system.SystemFolderPath;
                    SystemDirectoryParams.ImageFolderPath = system.ImageFolderPath;

                    global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath = SystemDirectoryParams.RootFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.strLogFolderPath = SystemDirectoryParams.LogFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.strRecipeFolderPath = SystemDirectoryParams.RecipeFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.strResultFolderPath = SystemDirectoryParams.ResultFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath = SystemDirectoryParams.SystemFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.strImageFolderPath = SystemDirectoryParams.ImageFolderPath;
                    global::atOpticalDecenter.Properties.Settings.Default.Save();

                    SystemDirectoryParams.CreateSystemDirectory();
                }
            }
        }
        private void barButtonItemSystemEditor_ItemClick(object sender, ItemClickEventArgs e)
        {
            SystemEditor editor = new SystemEditor(_systemParams._SystemLanguageKoreaUse);

            editor.ShowDialog();
            string strTemp = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName);

            if (File.Exists(strTemp))
            {
                // System 파일 열기
                RecipeFileIO.ReadSystemFile(_systemParams, strTemp);
                _ImageHist_W = new double[_systemParams._cameraParams.HResolution];
                _ImageHist_H = new double[_systemParams._cameraParams.VResolution];
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("시스템 파일 읽기 성공:{0}", strTemp));
            }
            else
            {
                _ImageHist_W = null;
                _ImageHist_H = null;
                // Default File 생성
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("시스템 파라미터를 읽을 수 없습니다.{0}", strTemp));
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("메뉴-시스템 편집기를 이용하여, 시스템 파일을 생성하십시오."));
            }
            StepBaseSystemParameterUpdate();
        }
        private void atOpticalDecenter_Load(object sender, EventArgs e)
        {
            // Log Event Allcation
            try
            {
                //_log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                mLog.WriteLogViewer += LogUpdated;//new Log.EventWriteLogViewer(LogUpdated);
                gridControl1.DataSource = mLogList;
                //gridControlBlobs.DataSource = _workParams.Blobs;
                //gridControlResult.DataSource = _workParams.InspectionPositions;                

                InitializeFileSystem();

                string strTemp = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName);

                if (File.Exists(strTemp))
                {
                    // System 파일 열기
                    RecipeFileIO.ReadSystemFile(_systemParams, strTemp);
                    _ImageHist_W = new double[_systemParams._cameraParams.HResolution];
                    _ImageHist_H = new double[_systemParams._cameraParams.VResolution];
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("시스템 파일 읽기 성공:{0}", strTemp));
                }
                else
                {
                    _ImageHist_W = null;
                    _ImageHist_H = null;
                    // Default File 생성
                    mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("시스템 파라미터를 읽을 수 없습니다.{0}", strTemp));
                    mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("메뉴-시스템 편집기를 이용하여, 시스템 파일을 생성하십시오."));
                }

                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "로그 데이터 및 이벤트 등록을 완료");
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "통신 모듈 초기화 완료.");
                /*
                if (mLogin.ShowDialog() == DialogResult.OK)
                {
                    _IsLogin = true;
                    //xtraTabControlMainSetting.Enabled = true;

                    //_admsEquipment.WorkerID = logIn.WorkerID;
                    //_admsEquipment.WorkerName = logIn.WorkerName;
                    //_admsEquipment.JobInformation = logIn.JobInformation;

                    //barStaticItemLogIn.Caption = string.Format("사번: {0}, 이름: {1}, 작업지시서: {2}", _admsEquipment.WorkerID, _admsEquipment.WorkerName, _admsEquipment.JobInformation);
                }
                else
                {
                    Application.Exit();
                }
                */
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "Photo 센서 검사 프로그램을 시작");
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "파일 시스템을 설정 및 로드 시작");

                // System 초기화
                if (_systemParams != null)
                    pledSpotInspectionInfomation.ChangeSystemLanguage(_systemParams._SystemLanguageKoreaUse);
                
                // Camera 연결
                if (InitializeCamera())
                {
                    _systemParams.InspectionOpticalSpotCenterX = _systemParams._cameraParams.HResolution / 2;
                    _systemParams.InspectionOpticalSpotCenterY = _systemParams._cameraParams.VResolution / 2;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 초기화 완료"));
                }
                else
                    mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 초기화 실패"));

                InitailProgramFormLanguage();
                InitializeAiCModule();
                InitializeARMRemoteIOModule();                

                InitializeStatistics();
                InitializeTackTimes();
                InitializeChartOpticalInspect();
                InitStepBase();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "검사 통계 초기화 완료");

                //ribbonPageGroupImageViewer.Enabled = false;
                //barCheckItemImageCrop.Enabled = false;
                //barCheckItemShowCenterMark.Enabled = true;

                //// 검사 및 결과 UI의 구분자 추가
                barEditItemInspectionProgress.Links[0].BeginGroup = true;
                barEditItemTotalInspectionCount.Links[0].BeginGroup = true;
                barButtonItemInitializeStatistics.Links[0].BeginGroup = true;

                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "리본 메뉴 구성을 초기화 완료");

                InitializeRecipe();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "레시피 초기화 완료");

                InitializedBackGroundWorkers();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "검사 동작 스레드 등록 완료");

                if (_systemParams.LatestUsedRecipe != string.Empty)
                    RecipeOpen(_systemParams.LatestUsedRecipe);

                UpdateConnectStatusForAll();
                InitialGuiAllEdit();
                ImageUpdateEvents += UpdateGUI;

                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "X_Base.bmp");
                _BaseXImage = System.Drawing.Image.FromFile(strTemp);
                pictureEditActuatorX.Image = _BaseXImage;
                FilterActuatorImageXFitSize();
                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "X_Actuator.png");
                _ActuatorXImage = System.Drawing.Image.FromFile(strTemp);
                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "Y_Base.bmp");
                _BaseYImage = System.Drawing.Image.FromFile(strTemp);
                pictureEditActuatorY.Image = _BaseYImage;                
                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "Y_Actuator.png");
                _ActuatorYImage = System.Drawing.Image.FromFile(strTemp);
                FilterActuatorImageYFitSize();
                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "Z_Base.bmp");
                _BaseZImage = System.Drawing.Image.FromFile(strTemp);
                pictureEditActuatorZ.Image = _BaseZImage;
                FilterActuatorImageZFitSize();
                strTemp = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, "Z_Actuator.png");
                _ActuatorZImage = System.Drawing.Image.FromFile(strTemp);                
                _bwMotionHome.RunWorkerAsync(mRobotInformation);
                AutoStartButtonLock();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public void InitialGuiAllEdit()
        {
            for (int i = 0; i < RecipeFileIO.ProductType.Length; ++i)
            {
                repositoryItemSpotProductType.Items.Add(RecipeFileIO.ProductType[i]);
            }
            rowInspectSpotProductType.Properties.Value = repositoryItemSpotProductType.Items[0].ToString();
        }
        private void atOpticalDecenter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(string.Format("광 편심 검사 시스템을 종료하시겠습니까?"), "시스템 종료", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            if (_isContinuousShot)
                _Camera.Stop();

            ////CloseTheImageProvider();

            if (_Camera.IsAllocated)
                _Camera.Close();
            ImageUpdateEvents -= UpdateGUI;
            //if ()

        }
        private void InitializedBackGroundWorkers()
        {
            _backgroundWorkerOpticalDecenterInspection.WorkerReportsProgress = true;
            _backgroundWorkerOpticalDecenterInspection.WorkerSupportsCancellation = true;
            _backgroundWorkerOpticalDecenterInspection.DoWork += new DoWorkEventHandler(backgroundWorkerInspection_DoWork);
            _backgroundWorkerOpticalDecenterInspection.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerInspection_ProgressChanged);
            _backgroundWorkerOpticalDecenterInspection.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerInspection_RunWorkerCompleted);
            _bwMotionHome.DoWork += new DoWorkEventHandler(backgroundWorkerMotionHome_DoWork);
            _bwMotionHome.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerMotionHome_RunWorkerCompleted);
        }
        private void LogUpdated(object obj, LogEventArgs e)
        {
            try
            {
                if (gridControl1.InvokeRequired)
                {
                    BeginInvoke(new Action<object, LogEventArgs>(LogUpdated), obj, e);
                    return;
                }

                mLogList.Add(e.Data);

                if (mLogList.Count > MAX_LOG_QUEUE_COUNT)
                    mLogList.RemoveAt(0);

                gridControlLogView.RefreshData();
                gridControlLogView.MoveLast();
            }
            catch
            {
                ;
            }
        }
        private void InitializeFileSystem()
        {
            try
            {
                if (string.IsNullOrEmpty(global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath)
                    || string.IsNullOrWhiteSpace(global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath)
                    || !Directory.Exists(global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath))
                {
                    string strRootFolder = string.Empty;
                    string strTempFolder = string.Empty;

                    strRootFolder = string.Format(@"{0}\Autonics\atOpticalDecenters", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath = strRootFolder;
                    SystemDirectoryParams.RootFolderPath = strRootFolder;

                    strTempFolder = string.Format(@"{0}\System", strRootFolder);
                    global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath = strTempFolder;
                    SystemDirectoryParams.SystemFolderPath = strTempFolder;

                    strTempFolder = string.Format(@"{0}\Recipe", strRootFolder);
                    global::atOpticalDecenter.Properties.Settings.Default.strRecipeFolderPath = strTempFolder;
                    SystemDirectoryParams.RecipeFolderPath = strTempFolder;

                    strTempFolder = string.Format(@"{0}\Log", strRootFolder);
                    global::atOpticalDecenter.Properties.Settings.Default.strLogFolderPath = strTempFolder;
                    SystemDirectoryParams.LogFolderPath = strTempFolder;

                    strTempFolder = string.Format(@"{0}\Result", strRootFolder);
                    global::atOpticalDecenter.Properties.Settings.Default.strResultFolderPath = strTempFolder;
                    SystemDirectoryParams.ResultFolderPath = strTempFolder;
                    
                    strTempFolder = string.Format(@"{0}\Image", strRootFolder);
                    global::atOpticalDecenter.Properties.Settings.Default.strImageFolderPath = strTempFolder;
                    SystemDirectoryParams.ImageFolderPath = strTempFolder;

                    global::atOpticalDecenter.Properties.Settings.Default.Save();
                }
                else
                {
                    SystemDirectoryParams.RootFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strRootFolderPath;
                    SystemDirectoryParams.SystemFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath;
                    SystemDirectoryParams.RecipeFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strRecipeFolderPath;
                    SystemDirectoryParams.LogFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strLogFolderPath;
                    SystemDirectoryParams.ResultFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strResultFolderPath;
                    SystemDirectoryParams.ImageFolderPath = global::atOpticalDecenter.Properties.Settings.Default.strImageFolderPath;
                }

                SystemDirectoryParams.CreateSystemDirectory();
                SystemDirectoryParams.WriteFileSystem();

                // 변경된 경로로 로그 파일을 저장
                mLog.SetLogPath(SystemDirectoryParams.LogFolderPath);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        private bool InitializeCamera()
        {
            bool IsInitialized = false;

            _Camera.OnCameraConnectionLost += new BaslerCamera.EventCameraConnectionLost(OnCameraConnectionLost);
            _Camera.OnCameraConnectionOpen += new BaslerCamera.EventCameraConnectionOpen(OnCameraConnectionOpen);
            _Camera.OnCameraImageGrab += new BaslerCamera.EventCameraImageGrab(OnCameraImageGrab);
            _Camera.OnCameraClose += new BaslerCamera.EventCameraClose(OnCameraClose);
            _Camera.OnCameraImageGrabStart += new BaslerCamera.EventCameraImageGrab(OnCameraImageGrabStart);
            _Camera.OnCameraImageGrabEnd += new BaslerCamera.EventCameraImageGrab(OnCameraImageGrabEnd);

            // 카메라 라이브러리 로그 연결
            _Camera._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);

            List<string> liststrFriendlyNames = _Camera.FindCameras();

            //// System 파일에 카메라가 등록되지 않은 경우
            if (_systemParams._cameraParams.FriendlyName == "None" || string.IsNullOrEmpty(_systemParams._cameraParams.FriendlyName))
            {
                for (int i = 0; i < liststrFriendlyNames.Count; ++i)
                {
                    repositoryItemComboBoxCameraName.Items.Add(liststrFriendlyNames[i]);
                }

                // 카메라가 있는 경우
                try
                {
                    if (liststrFriendlyNames.Count > 0)
                    {
                        //rowCameraFriendlyName.Properties.Value = liststrFriendlyNames[0];

                        Cameraname = liststrFriendlyNames[0];
                        if (_Camera.Open(liststrFriendlyNames[0]))
                        {
                            CameraParameters cameraParam = new CameraParameters();

                            rowCameraName.Properties.Value = repositoryItemComboBoxCameraName.Items[repositoryItemComboBoxCameraName.Items.IndexOf(liststrFriendlyNames[0])].ToString();

                            // 노출 시간
                            cameraParam = _Camera.ExposureTime;
                            rowCameraExposureTime.Properties.Value = cameraParam.Value;
                            //Cameraexposuretime = (int)cameraParam.Value;
                            //_systemParams.CameraParameters.ExposureTime = (int)cameraParam.Value;

                            // 게인
                            cameraParam = _Camera.Gain;
                            rowCameraGain.Properties.Value = cameraParam.Value;
                            //_systemParams.CameraParameters.Gain = (int)cameraParam.Value;                        

                            // Frame Rate
                            cameraParam = _Camera.FrameRate;
                            rowCameraFrame.Properties.Value = cameraParam.Value;
                            //_systemParams.CameraParameters.FrameRate = (int)cameraParam.Value;                        

                            cameraParam = _Camera.Width;
                            rowCameraHResolution.Properties.Value = cameraParam.Value;
                            //_systemParams.CameraParameters.HResolution = (int)cameraParam.Value;                        

                            cameraParam = _Camera.Height;
                            rowCameraVResolution.Properties.Value = cameraParam.Value;
                            //_systemParams.CameraParameters.VResolution = (int)cameraParam.Value;                        


                            IsInitialized = true;
                            _isCameraOpen = true;
                            repositoryItemToggleSwitchCameraConnection.ValueOff = true;
                            repositoryItemToggleSwitchCameraConnection.ValueOn = false;
                            // System 파라미터를 Update한다.
                            //RecipeFileIO.WriteSystemFile(_systemParams, string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName));

                            //if (_ImageHist_H == null)
                                _ImageHist_H = new double[Convert.ToInt32(rowCameraVResolution.Properties.Value)];
                            //if (_ImageHist_W == null)
                                _ImageHist_W = new double[Convert.ToInt32(rowCameraHResolution.Properties.Value)];

                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 연결 성공:{0}", liststrFriendlyNames[0]));
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(),
                                string.Format("노출 시간:{0}, 게인:{1}, 프레임비:{2}", Convert.ToString(rowCameraExposureTime.Properties.Value), Convert.ToString(rowCameraGain.Properties.Value), Convert.ToString(rowCameraFrame.Properties.Value)));

                        }
                        else
                        {
                            mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 연결 실패:{0}", liststrFriendlyNames[0]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _Camera.Close();
                    _isCameraOpen = false;
                    mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("카메라 연결 실패:{0}", liststrFriendlyNames[0]));
                    mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
                }
            }
            else
            {
                repositoryItemComboBoxCameraName.Items.Add(_systemParams._cameraParams.FriendlyName);
                rowCameraName.Properties.Value = repositoryItemComboBoxCameraName.Items[repositoryItemComboBoxCameraName.Items.IndexOf(_systemParams._cameraParams.FriendlyName)].ToString();
                rowCameraExposureTime.Properties.Value = _systemParams._cameraParams.ExposureTime;
                rowCameraHResolution.Properties.Value = _systemParams._cameraParams.HResolution;
                rowCameraVResolution.Properties.Value = _systemParams._cameraParams.VResolution;
                rowCameraGain.Properties.Value = _systemParams._cameraParams.Gain;
                rowCameraFrame.Properties.Value = _systemParams._cameraParams.FrameRate;
                if(_Camera.Open(_systemParams._cameraParams.FriendlyName))
                {
                    IsInitialized = true;
                    _isCameraOpen = true;
                    repositoryItemToggleSwitchCameraConnection.ValueOff = true;
                    repositoryItemToggleSwitchCameraConnection.ValueOn = false;
                    // System 파라미터를 Update한다.
                    //RecipeFileIO.WriteSystemFile(_systemParams, string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName));

                    if (_ImageHist_H != null)
                        _ImageHist_H = new double[Convert.ToInt32(rowCameraVResolution.Properties.Value)];
                    if (_ImageHist_W != null)
                        _ImageHist_W = new double[Convert.ToInt32(rowCameraHResolution.Properties.Value)];
                }
            }            
            return IsInitialized;
        }
        public bool InitializeAiCModule()
        {
            if (_systemParams != null)
                MotionControl.ChangeSystemLanguage(_systemParams._SystemLanguageKoreaUse);

            MotionControl.SetCommunicateManager(ref _mMotionControlCommManager);
            MotionControl.SetMotionParam(ref _systemParams._motionParams);
            byte[] _id = new byte[3];
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    MotionControl._fdefineStepValue[i] = (double)0.1;
                    MotionControl._fdefineVelValue[i] = (double)10;
                }
                else if (i == 1)
                {
                    MotionControl._fdefineStepValue[i] = (double)1;
                    MotionControl._fdefineVelValue[i] = (double)50;
                }
                else
                {
                    MotionControl._fdefineStepValue[i] = (double)10;
                    MotionControl._fdefineVelValue[i] = (double)100;
                }
                _id[i] = (byte)_systemParams._AiCParams.IDs[i]._idNumber;
            }
            MotionControl.SetCommunicationData(3, _id);
            AiCModuleConnect(); // connect command
            return true;
        }

        public bool InitializeARMRemoteIOModule()
        {
            if (_systemParams != null)
                RemoteIOControl.ChangeSystemLanguage(_systemParams._SystemLanguageKoreaUse);

            RemoteIOControl.SetCommunicateManager(ref _mRemteIOCommManager);
            byte[] _id = new byte[_systemParams._remoteIOParams.ConnectedNumber];

            for (int i = 0; i < _systemParams._remoteIOParams.ConnectedNumber; i++)
            {
                _id[i] = (byte)_systemParams._remoteIOParams.IDs[i]._idNumber;
            }
            RemoteIOControl.SetCommunicationData(_systemParams._remoteIOParams.ConnectedNumber, _id);
            ARMRemoteIOModuleConnect();// connect command
            return true;
        }
        private bool AiCModuleConnect(string sComport = null)
        {
            if (!_mMotionControlCommManager.IsOpen())
            {
                AiCControlLibrary.SerialCommunication.Control.SerialPortSetData setPort = new AiCControlLibrary.SerialCommunication.Control.SerialPortSetData();
                setPort.PortName = _systemParams._AiCParams.SerialParameters.PortName;
                setPort.BaudRate = (int)_systemParams._AiCParams.SerialParameters.BaudRates;
                setPort.DataBits = (int)_systemParams._AiCParams.SerialParameters.DataBits;
                setPort.StopBits = System.IO.Ports.StopBits.One; //(StopBits)_systemParams._AiCParams.SerialParameters.StopBits;
                setPort.Parity = System.IO.Ports.Parity.None;

                MotionControl.ConnectionOpen(setPort);

                if (_mMotionControlCommManager.IsOpen())
                {
                    MotionControl.RobotInfomationUpdatedEvent += UpdateRobotInfomation;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 성공."));
                }
                else
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 실패."));
            }
            else
            {
                MotionControl.ConnectionClosed();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 해제 성공."));
            }
            return _mMotionControlCommManager.IsOpen();
        }
        private bool AiCModuleDisConnect()
        {
            if (_mMotionControlCommManager.IsOpen())
            {
                MotionControl.ConnectionClosed();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 해제 성공."));
            }
            return _mMotionControlCommManager.IsOpen();
        }
        private bool ARMRemoteIOModuleConnect(string sComport = null)
        {
            if (!_mRemteIOCommManager.IsOpen())
            {
                ARMLibrary.SerialCommunication.Control.SerialPortSetData setPort = new ARMLibrary.SerialCommunication.Control.SerialPortSetData();
                setPort.PortName = _systemParams._remoteIOParams.SerialParameters.PortName;
                setPort.BaudRate = (int)_systemParams._remoteIOParams.SerialParameters.BaudRates;
                setPort.DataBits = (int)_systemParams._remoteIOParams.SerialParameters.DataBits;
                setPort.StopBits = System.IO.Ports.StopBits.One;
                setPort.Parity = System.IO.Ports.Parity.None;

                RemoteIOControl.ConnectionOpen(setPort);

                if (_mRemteIOCommManager.IsOpen())
                {
                    RemoteIOControl.RobotInfomationUpdatedEvent += UpdateRobotIOInfomation;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 성공."));
                }
                else
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 실패."));
            }
            else
            {                
                RemoteIOControl.ConnectionClosed();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 해제 성공."));
            }
            return _mRemteIOCommManager.IsOpen();            
        }
        private bool ARMRemoteIOModuleDisconnet()
        {
            if (_mRemteIOCommManager.IsOpen())
            {                
                _mRemteIOCommManager.Disconnect();
            }
            return _mRemteIOCommManager.IsOpen();            
        }    
        private void InitializeStatistics()
        {
            string strTemp = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.StatisticsFileName);

            if (File.Exists(strTemp))
            {
                _statistics = RecipeFileIO.ReadInspectionStatisticsFile(strTemp, MAX_STATISTICS);
            }
            else
            {
                for (int i = 0; i < MAX_STATISTICS; ++i)
                {
                    _statistics.Statistics.Add("0");
                }

                _statistics.TotalCount = 0;
                _statistics.PassCount = 0;
                _statistics.FailCount = 0;

                RecipeFileIO.WriteInspectionStatisticsFile(strTemp, _statistics);
            }

            if (_systemParams._SystemLanguageKoreaUse)
            {
                barEditItemTotalInspectionCount.EditValue = string.Format("총 검사 수: {0:00000}", _statistics.TotalCount);
                barEditItemTotalFailCount.EditValue = string.Format("불합격: {0:00000}", _statistics.FailCount);
            }
            else
            {
                barEditItemTotalInspectionCount.EditValue = string.Format("Total Count: {0:00000}", _statistics.TotalCount);
                barEditItemTotalFailCount.EditValue = string.Format("Fail Count: {0:00000}", _statistics.FailCount);
            }


            // Variable Init.
            if (_currentStatistics.Statistics.Count == 0)
            {
                for (int i = 0; i < MAX_STATISTICS; ++i)
                    _currentStatistics.Statistics.Add(0);
            }
        }
        private void InitializeTackTimes()
        {
            string strFilePath = string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.TackTimesFileName);

            if (Directory.Exists(SystemDirectoryParams.RecipeFolderPath))
            {
                string[] recipes = null;
                recipes = Directory.GetDirectories(SystemDirectoryParams.RecipeFolderPath);

                if (recipes != null)
                {
                    for (int i = 0; i < recipes.Length; ++i)
                    {
                        string[] strTemp = recipes[i].Split('\\');

                        string strRecipeName = strTemp[strTemp.Length - 1];

                        _dicTackTimes.Add(strRecipeName, new TackParams(0, 0));
                    }
                }
            }

            if (File.Exists(strFilePath))
            {
                _dicTackTimes = RecipeFileIO.ReadTackTimeFile(strFilePath);
            }
            else
            {
                RecipeFileIO.WriteTackTimeFile(strFilePath, _dicTackTimes);
            }
        }
        private void UpdateProcessTime(bool IsLogWrite)
        {
            CheckTackTime.Stop();
            TimeSpan ts = CheckTackTime.Elapsed;
            barStaticItemInspectionTime.Caption = string.Format("검사 시간: 00:{0:00}:{1:00}.{2}",ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (IsLogWrite)
            {
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("레시피:{0}, 검사 시간:{1:000.000} sec", _workParams.RecipeName, (ts.Milliseconds/1000F)));
            }
        }
        private void OnCameraImageGrabStart(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(OnCameraImageGrabStart), sender, e);
                return;
            }

            if (_isContinuousShot)
                _frameCount = 0;
        }

        private void OnCameraImageGrabEnd(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(OnCameraImageGrabEnd), sender, e);
                return;
            }
        }

        private void OnCameraConnectionLost(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(OnCameraConnectionLost), sender, e);
                return;
            }
        }

        private void OnCameraConnectionOpen(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(OnCameraConnectionOpen), sender, e);
                return;
            }            
        }

        private void OnCameraImageGrab(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new EventHandler<EventArgs>(OnCameraImageGrab), sender, e);
                    return;
                }

                GrabEndParam grabEnd = (GrabEndParam)sender as GrabEndParam;
                System.Drawing.Image sourceImage = _sourceImage;

                if (grabEnd != null)
                {
                    if (_isContinuousShot)
                    {
                        pictureEditSystemImage.Image = grabEnd.Image;
                        _sourceImage = grabEnd.Image;
                    }
                    else
                    {
                        pictureEditSystemImage.Image = grabEnd.Image;
                        _sourceImage = Utils.Clone<Bitmap>(grabEnd.Image);
                    }                    
                    ImageGrabbed?.Invoke(_sourceImage);
                    if (grabEnd.WaitHandle != null)
                        grabEnd.WaitHandle.Set();
                    _patternMatching = false;
                    _isOpticalMeasurement = false;

                }

                _isGrabbed = true;


                if (sourceImage != null)
                {
                    sourceImage.Dispose();
                    sourceImage = null;
                }

                if (_isContinuousShot)
                {
                    _frameCount++;
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.Message);
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.StackTrace.ToString());
            }
        }

        private void OnCameraClose(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(OnCameraClose), sender, e);                
                return;
            }
        }
        private void OnWriteLogViewer(object sender, LogEventArgs e)
        {
            try
            {
                if (mLogList.Count > 1000)
                    mLogList.Clear();


                mLogList.Add(e.Data);

                //UpdateGridControlLogView();
            }
            catch (Exception ex)
            {

            }
        }
        private void barButtonItemRecipeEditorOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_systemParams != null)
            {
                RecipeEditor edit = new RecipeEditor(_systemParams._SystemLanguageKoreaUse);
                edit.SetSystemParam(_systemParams);
                edit.Show(this);
            }
        }

        private void barListItemRecipeOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            barListItemRecipeOpen.Strings.Clear();

            InitializeRecipe();
        }
        private void InitializeRecipe()
        {
            if (Directory.Exists(SystemDirectoryParams.RecipeFolderPath))
            {
                string[] recipes = null;
                recipes = Directory.GetDirectories(SystemDirectoryParams.RecipeFolderPath);

                for (int i = 0; i < recipes.Length; ++i)
                {
                    string[] strTemp = recipes[i].Split('\\');

                    string strRecipeName = strTemp[strTemp.Length - 1];

                    if (!_dicTackTimes.ContainsKey(strRecipeName))
                    {
                        _dicTackTimes.Add(strRecipeName, new TackParams(0, 0));
                    }

                    barListItemRecipeOpen.Strings.Add(strRecipeName);
                }
            }
        }

        private void barListItemRecipeOpen_ListItemClick(object sender, ListItemClickEventArgs e)
        {
            if (e.Index < 0)
                return;

            RecipeOpen(barListItemRecipeOpen.Strings[e.Index]);
        }
        private void RecipeOpen(string strRecipeName)
        {
            try
            {
                string strSelectedPath = string.Format(@"{0}\{1}", SystemDirectoryParams.RecipeFolderPath, strRecipeName);

                if (!string.IsNullOrEmpty(strSelectedPath))
                {
                    string strRecipeFilePath = string.Format(@"{0}\{1}.rcp", strSelectedPath, strRecipeName);

                    if (!File.Exists(strRecipeFilePath))
                    {
                        MessageBox.Show(string.Format("레시피 파일을 불러올 수 없습니다. 경로를 확인해 주십시오.\r\n{0}", strRecipeFilePath), "불러오기 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Recipe File 읽기
                    RecipeFileIO.ReadRecipeFile(_workParams, strRecipeFilePath);

                    UpdateRecipeUI(strRecipeName);
                    _workParams.ImageCenterX = (_systemParams._cameraParams.HResolution / 2);
                    _workParams.ImageCenterY = (_systemParams._cameraParams.VResolution / 2);
                    // inspection flag
                    _isInspecting = false;
                    _isInspectionDone = false;
                    _IsReciepLoad = true;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("레시피 읽기 완료. 레시피 경로:{0}", strRecipeFilePath));
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
        private void UpdateRecipeUI(string strRecipeName)
        {
            ;
        }

        private void barButtonItemRecipeOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                xtraFolderBrowserDialogRecipeOpen.Title = "레시피 폴더를 선택하세요.";
                xtraFolderBrowserDialogRecipeOpen.SelectedPath = SystemDirectoryParams.RecipeFolderPath;

                if (xtraFolderBrowserDialogRecipeOpen.ShowDialog() == DialogResult.OK)
                {
                    string[] strTemp = null;
                    string strRecipeName = string.Empty;
                    string strSelectedPath = xtraFolderBrowserDialogRecipeOpen.SelectedPath;

                    if (!string.IsNullOrEmpty(strSelectedPath))
                    {
                        strTemp = strSelectedPath.Split('\\');

                        if (strTemp.Length > 0)
                        {
                            strRecipeName = strTemp[strTemp.Length - 1];
                        }

                        string strRecipeFilePath = string.Format(@"{0}\{1}.rcp", strSelectedPath, strRecipeName);

                        if (!File.Exists(strRecipeFilePath))
                        {
                            MessageBox.Show("레시피 파일을 불러올 수 없습니다. 경로를 확인해 주십시오.", "불러오기 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // System File 읽기
                        RecipeFileIO.ReadRecipeFile(_workParams, strRecipeFilePath);

                        UpdateRecipeUI(strRecipeName);

                        // inspection flag 
                        _isInspecting = false;
                        _isInspectionDone = false;
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("레시피 읽기 완료. 레시피 경로:{0}", strRecipeFilePath));
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
        private void barButtonItemConnectAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.Item.Name == "barButtonItemConnectAll")
                {
                    if (!_mMotionControlCommManager.IsOpen())                                        
                    {
                        AiCControlLibrary.SerialCommunication.Control.SerialPortSetData setPort = new AiCControlLibrary.SerialCommunication.Control.SerialPortSetData();
                        setPort.PortName = _systemParams._AiCParams.SerialParameters.PortName;
                        setPort.BaudRate = (int)_systemParams._AiCParams.SerialParameters.BaudRates;
                        setPort.DataBits = (int)_systemParams._AiCParams.SerialParameters.DataBits;
                        setPort.StopBits = System.IO.Ports.StopBits.One; //(StopBits)_systemParams._AiCParams.SerialParameters.StopBits;
                        setPort.Parity = System.IO.Ports.Parity.None;

                        MotionControl.ConnectionOpen(setPort);

                        if (_mMotionControlCommManager.IsOpen())
                        {
                            MotionControl.RobotInfomationUpdatedEvent += UpdateRobotInfomation;
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 성공."));
                        }
                        else
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 실패."));
                    }

                    if (!_mRemteIOCommManager.IsOpen())
                    {
                        ARMLibrary.SerialCommunication.Control.SerialPortSetData setPort = new ARMLibrary.SerialCommunication.Control.SerialPortSetData();
                        setPort.PortName = _systemParams._remoteIOParams.SerialParameters.PortName;
                        setPort.BaudRate = (int)_systemParams._remoteIOParams.SerialParameters.BaudRates;
                        setPort.DataBits = (int)_systemParams._remoteIOParams.SerialParameters.DataBits;
                        setPort.StopBits = System.IO.Ports.StopBits.One;
                        setPort.Parity = System.IO.Ports.Parity.None;

                        RemoteIOControl.ConnectionOpen(setPort);

                        if (_mRemteIOCommManager.IsOpen())
                        {
                            RemoteIOControl.RobotInfomationUpdatedEvent += UpdateRobotIOInfomation;
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 성공."));
                        }
                        else
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 실패."));
                    }
                }
                else if (e.Item.Name == "barButtonItemConnectionAiC")
                {
                    if (!_mMotionControlCommManager.IsOpen())
                    {
                        AiCControlLibrary.SerialCommunication.Control.SerialPortSetData setPort = new AiCControlLibrary.SerialCommunication.Control.SerialPortSetData();
                        setPort.PortName = _systemParams._AiCParams.SerialParameters.PortName;
                        setPort.BaudRate = (int)_systemParams._AiCParams.SerialParameters.BaudRates;
                        setPort.DataBits = (int)_systemParams._AiCParams.SerialParameters.DataBits;
                        setPort.StopBits = System.IO.Ports.StopBits.One; //(StopBits)_systemParams._AiCParams.SerialParameters.StopBits;
                        setPort.Parity = System.IO.Ports.Parity.None;

                        MotionControl.ConnectionOpen(setPort);

                        if (_mMotionControlCommManager.IsOpen())
                        {
                            MotionControl.RobotInfomationUpdatedEvent += UpdateRobotInfomation;
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 성공."));
                        }
                        else
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 실패."));
                    }
                    else
                    {
                        //_mMotionControlCommManager.Disconnect();
                        MotionControl.ConnectionClosed();
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("AiC 통신 연결 해제 성공."));
                    }
                }
                else if (e.Item.Name == "barButtonItemConnectionRemeteIO")
                {
                    if (!_mRemteIOCommManager.IsOpen())
                    {
                        ARMLibrary.SerialCommunication.Control.SerialPortSetData setPort = new ARMLibrary.SerialCommunication.Control.SerialPortSetData();
                        setPort.PortName = _systemParams._remoteIOParams.SerialParameters.PortName;
                        setPort.BaudRate = (int)_systemParams._remoteIOParams.SerialParameters.BaudRates;
                        setPort.DataBits = (int)_systemParams._remoteIOParams.SerialParameters.DataBits;
                        setPort.StopBits = System.IO.Ports.StopBits.One;
                        setPort.Parity = System.IO.Ports.Parity.None;

                        RemoteIOControl.ConnectionOpen(setPort);

                        if (_mRemteIOCommManager.IsOpen())
                        {
                            RemoteIOControl.RobotInfomationUpdatedEvent += UpdateRobotIOInfomation;
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 성공."));
                        }
                        else
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 실패."));
                    }
                    else
                    {
                        //_mMotionControlCommManager.Disconnect();
                        RemoteIOControl.ConnectionClosed();
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("ARM 통신 연결 해제 성공."));
                    }
                }
                else if (e.Item.Name == "barButtonItemConnectionCamera")
                {
                    if (_isContinuousShot)
                        _Camera.Stop();

                    if (!_Camera.IsAllocated)
                    {                            
                        if (!string.IsNullOrEmpty(Cameraname))
                        {
                            if (_Camera.Open(Cameraname))
                            {
                                _isCameraOpen = true;
                                rowCameraConnect.Properties.Value = true;
                                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 연결 성공:{0}", Cameraname));
                            }
                            else
                            {
                                repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                                repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                                _isCameraOpen = false;                                
                                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 연결 실패:{0}", Cameraname));
                            }
                        }
                        else
                        {
                            _isCameraOpen = false;
                            repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                            repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                            mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 이름이 없습니다:{0}", Cameraname));
                        }
                    }
                    else
                    {
                        _Camera.Close();
                        _isCameraOpen = false;
                        rowCameraConnect.Properties.Value = false;
                        repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                        repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 연결이 끊겼습니다."));
                    }

                }
                else
                {
                    ;
                }
                UpdateConnectStatusForAll();
            }
            catch (Exception ex)
            {
                ;
            }
        }
        private void UpdateConnectStatusForAll()
        {
            try
            {

                if (!_mMotionControlCommManager.IsOpen())
                    barButtonItemConnectionAiC.ImageOptions.Image = Properties.Resources.disconnect_16x16;
                else
                    barButtonItemConnectionAiC.ImageOptions.Image = Properties.Resources.connect_16x16;

                if (!_mRemteIOCommManager.IsOpen())
                    barButtonItemConnectionRemeteIO.ImageOptions.Image = Properties.Resources.disconnect_16x16;
                else
                    barButtonItemConnectionRemeteIO.ImageOptions.Image = Properties.Resources.connect_16x16;

                if (!_isCameraOpen)
                    barButtonItemConnectionCamera.ImageOptions.Image = Properties.Resources.disconnect_16x16;
                else
                    barButtonItemConnectionCamera.ImageOptions.Image = Properties.Resources.connect_16x16;

                if ((_mMotionControlCommManager.IsOpen()) && (_mRemteIOCommManager.IsOpen()) && (_isCameraOpen))
                    barButtonItemConnectAll.ImageOptions.LargeImage = Properties.Resources.connectedall_32x32;
                //else if ((!_mMotionControlCommManager.IsOpen()) && (!_mRemteIOCommManager.IsOpen()) && (!_isCameraOpen))
                //    barButtonItemConnectAll.ImageOptions.LargeImage = Properties.Resources.disconnectedall_32x32;
                else
                    barButtonItemConnectAll.ImageOptions.LargeImage = Properties.Resources.connectedpart_32x32;
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("UpdateConnectionStatusForAll. \n{0}", ex.ToString()));
            }
        }
        private void CameraListRefresh(object sender, ItemClickEventArgs e)
        {
            List<string> liststrFriendlyNames = _Camera.FindCameras();
            try
            {
                if (liststrFriendlyNames.Count > 0)
                {
                    //rowCameraFriendlyName.Properties.Value = liststrFriendlyNames[0];

                    Cameraname = liststrFriendlyNames[0];
                    if (_Camera.Open(liststrFriendlyNames[0]))
                    {
                        //_systemParams.CameraParameters.FriendlyName = liststrFriendlyNames[0];

                        CameraParameters cameraParam = new CameraParameters();
                        repositoryItemComboBoxCameraName.Items.Add(liststrFriendlyNames[0]);
                        rowCameraName.Properties.Value = repositoryItemComboBoxCameraName.Items[repositoryItemComboBoxCameraName.Items.IndexOf(liststrFriendlyNames[0])].ToString();

                        // 노출 시간
                        cameraParam = _Camera.ExposureTime;
                        rowCameraExposureTime.Properties.Value = cameraParam.Value;
                        //Cameraexposuretime = (int)cameraParam.Value;
                        //_systemParams.CameraParameters.ExposureTime = (int)cameraParam.Value;

                        // 게인
                        cameraParam = _Camera.Gain;
                        rowCameraGain.Properties.Value = cameraParam.Value;
                        //_systemParams.CameraParameters.Gain = (int)cameraParam.Value;                        

                        // Frame Rate
                        cameraParam = _Camera.FrameRate;
                        rowCameraFrame.Properties.Value = cameraParam.Value;
                        //_systemParams.CameraParameters.FrameRate = (int)cameraParam.Value;                        

                        cameraParam = _Camera.Width;
                        _systemParams._cameraParams.HResolution = (int)cameraParam.Value;
                        rowCameraHResolution.Properties.Value = cameraParam.Value;
                        //_systemParams.CameraParameters.HResolution = (int)cameraParam.Value;                        

                        cameraParam = _Camera.Height;
                        _systemParams._cameraParams.VResolution = (int)cameraParam.Value;
                        rowCameraVResolution.Properties.Value = cameraParam.Value;
                        //_systemParams.CameraParameters.VResolution = (int)cameraParam.Value;                        

                        _isCameraOpen = true;
                        repositoryItemToggleSwitchCameraConnection.ValueOff = true;
                        repositoryItemToggleSwitchCameraConnection.ValueOn = false;
                        // System 파라미터를 Update한다.
                        //RecipeFileIO.WriteSystemFile(_systemParams, string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.SystemFileName));

                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 연결 성공:{0}", liststrFriendlyNames[0]));
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("노출 시간:{0}, 게인:{1}, 프레임비:{2}", _Camera.ExposureTime, _Camera.Gain, _Camera.FrameRate));

                    }
                    UpdateConnectStatusForAll();
                    _systemParams.InspectionOpticalSpotCenterX = _systemParams._cameraParams.HResolution / 2;
                    _systemParams.InspectionOpticalSpotCenterY = _systemParams._cameraParams.VResolution / 2;
                    //if (_ImageHist_H == null)
                        _ImageHist_H = new double[_systemParams._cameraParams.VResolution];
                    //if (_ImageHist_W == null)
                        _ImageHist_W = new double[_systemParams._cameraParams.HResolution];
                }                
                else
                {
                    Cameraname = _systemParams._cameraParams.FriendlyName;
                    repositoryItemComboBoxCameraName.Items.Add(_systemParams._cameraParams.FriendlyName);
                    rowCameraName.Properties.Value = repositoryItemComboBoxCameraName.Items[repositoryItemComboBoxCameraName.Items.IndexOf(_systemParams._cameraParams.FriendlyName)].ToString();
                    rowCameraExposureTime.Properties.Value = _systemParams._cameraParams.ExposureTime;
                    rowCameraHResolution.Properties.Value = _systemParams._cameraParams.HResolution;
                    rowCameraVResolution.Properties.Value = _systemParams._cameraParams.VResolution;
                    rowCameraGain.Properties.Value = _systemParams._cameraParams.Gain;
                    rowCameraFrame.Properties.Value = _systemParams._cameraParams.FrameRate;
                }                
            }
            catch (Exception ex)
            {
                _Camera.Close();
                _isCameraOpen = false;
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("카메라 연결 실패:{0}", Cameraname));
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }

        private void vGridControl1_CellValueChanging(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (e.Row.Name == "rowCameraConnect")
            {
                if (e.CellIndex == 0)
                {
                    bool IsCameraOpen = Convert.ToBoolean(e.Value);
                    if (_isContinuousShot)
                        _Camera.Stop();
                    if (IsCameraOpen)
                    {
                        if (!_Camera.IsAllocated)
                        {
                            string strCameraName = Convert.ToString(rowCameraName.Properties.Value);
                            if (!string.IsNullOrEmpty(strCameraName))
                            {
                                if (_Camera.Open(strCameraName))
                                {
                                    rowCameraName.Properties.Value = strCameraName;
                                    rowCameraConnect.Properties.Value = e.Value;
                                    _isCameraOpen = true;
                                }
                                else
                                {
                                    repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                                    repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                                    _isCameraOpen = false;
                                    mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 연결 실패:{0}", strCameraName));
                                }
                            }
                            else
                            {
                                _isCameraOpen = false;
                                repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                                repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("카메라 이름이 없습니다:{0}", strCameraName));
                            }
                        }
                        else
                        {
                            _Camera.Close();
                            _isCameraOpen = false;
                            repositoryItemToggleSwitchCameraConnection.ValueOn = true;
                            repositoryItemToggleSwitchCameraConnection.ValueOff = false;
                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 연결이 끊겼습니다."));
                        }
                    }
                    else
                    {
                        _Camera.Close();
                        _isCameraOpen = false;
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 연결이 끊겼습니다."));
                    }
                    UpdateConnectStatusForAll();
                }
            }
        }

        private void vGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (_Camera.IsAllocated)
            {
                if (e.Row.Name == "rowCameraFrame")
                {
                    double oldValue = _Camera.FrameRate.Value;
                    CameraParameters cameraParam = new CameraParameters();
                    cameraParam.Value = Convert.ToInt32(rowCameraFrame.Properties.Value);
                    _Camera.FrameRate = cameraParam;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 프레임 비(fps) 변경 전:{0}, 변경 후:{1}", (int)oldValue, (int)cameraParam.Value));
                }
                else if (e.Row.Name == "rowCameraExposureTime")
                {
                    double oldValue = _Camera.ExposureTime.Value;
                    CameraParameters cameraParam = new CameraParameters();
                    int ExposerRange = Convert.ToInt32(e.Value);
                    if ((ExposerRange >= 50) && (ExposerRange <= 10000000))
                    {
                        cameraParam.Value = Convert.ToInt32(rowCameraExposureTime.Properties.Value);
                        _Camera.ExposureTime = cameraParam;
                        mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 노출 시간 변경 전:{0}, 변경 후:{1}", (int)oldValue, (int)cameraParam.Value));
                    }
                }
                else if (e.Row.Name == "rowCameraGain")
                {
                    double oldValue = _Camera.Gain.Value;
                    CameraParameters cameraParam = new CameraParameters();
                    cameraParam.Value = Convert.ToInt32(rowCameraGain.Properties.Value);
                    _Camera.Gain = cameraParam;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 게인 변경 전:{0}, 변경 후:{1}", (int)oldValue, (int)cameraParam.Value));
                }
            }
        }

        private void barButtonItemSingleShot_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (_isCameraOpen)
            if (_Camera.IsAllocated)
            {
                try
                {
                    _Camera.OneShot(_waitHandle);

                    _isOpticalMeasurement = false;
                    pictureEditSystemImage.Refresh();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "싱글 샷");
                    barButtonItemFitSize.PerformClick();
                }
                catch (Exception)
                {

                }
            }
        }

        private void barButtonItemContinueousShot_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_Camera.IsAllocated)
            {
                try
                {
                    _Camera.ContinuousShot(_systemParams._cameraParams.FrameRate);
                    _isContinuousShot = true;
                    _frameCount = 0;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "연속 샷 시작");
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void barButtonItemCameraStop_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_Camera.IsAllocated)
            {
                try
                {
                    _Camera.Stop();
                    _isContinuousShot = false;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "연속 샷 정지");
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void pictureEditSystemImage_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (pictureEditSystemImage.Image == null)
                    return;

                Graphics gp = e.Graphics;

                //gp.Clear(Color.White);                
                gp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int crossMarkOffset = 10;
                int InspectionOpticalSpotCenterX = _systemParams._cameraParams.HResolution/2;
                int InspectionOpticalSpotCenterY = _systemParams._cameraParams.VResolution/2;
                float fScale = (float)(pictureEditSystemImage.Properties.ZoomPercent / 100.0f);
                float fHScroll = pictureEditSystemImage.HScrollBar.Value;
                float fVScroll = pictureEditSystemImage.VScrollBar.Value;
                float fCharacter = (fScale > 1f) ? 1f : fScale;

                float fImageWidth = pictureEditSystemImage.Image.Width;
                float fImageHeight = pictureEditSystemImage.Image.Height;
                float fCenterx = InspectionOpticalSpotCenterX;
                float fCentery = InspectionOpticalSpotCenterY;


                Matrix matrix = new Matrix();
                matrix.Scale(fScale, fScale);
                matrix.Translate(-fHScroll / fScale, -fVScroll / fScale);

                gp.Transform = matrix;

                //RectangleF rtPassArea = new RectangleF(400, 100, 500, 700);
                RectangleF rtPassArea = new RectangleF(_workParams.AreaStart.X, _workParams.AreaStart.Y, Math.Abs(_workParams.AreaEnd.X - _workParams.AreaStart.X), Math.Abs(_workParams.AreaEnd.Y - _workParams.AreaStart.Y));
                RectangleF rtBlob = new RectangleF();

                _fHScrollPos = pictureEditSystemImage.HScrollBar.Value;
                _fVScrollPos = pictureEditSystemImage.VScrollBar.Value;

                GraphicsPath path = new GraphicsPath();
                Font font = new Font("Arial", 30f * fCharacter, FontStyle.Bold);
                PointF fptDrawString = new PointF();

                // 중심선 그리기
                if (barCheckItemShowCenterMark.Checked)
                {
                    if (fScale <= 1.0f)
                    {
                        float imageWidth = pictureEditSystemImage.Image.Width * fScale;
                        float imageHeight = pictureEditSystemImage.Image.Height * fScale;

                        //PointF fptHLineStart = new PointF(0, imageHeight / 2f);
                        //PointF fptHLineEnd = new PointF(imageWidth, imageHeight / 2f);
                        //PointF fptVLineStart = new PointF(imageWidth / 2f, 0);
                        //PointF fptVLineEnd = new PointF(imageWidth / 2f, imageHeight);
                        PointF fptHLineStart = new PointF(0, InspectionOpticalSpotCenterY);
                        PointF fptHLineEnd = new PointF(InspectionOpticalSpotCenterX * 2, InspectionOpticalSpotCenterY);
                        PointF fptVLineStart = new PointF(InspectionOpticalSpotCenterX, 0);
                        PointF fptVLineEnd = new PointF(InspectionOpticalSpotCenterX, InspectionOpticalSpotCenterY * 2);

                        gp.DrawLine(Pens.Red, fptHLineStart, fptHLineEnd);
                        gp.DrawLine(Pens.Red, fptVLineStart, fptVLineEnd);
                    }
                    else
                    {
                        float imageWidth = pictureEditSystemImage.Image.Width * fScale;
                        float imageHeight = pictureEditSystemImage.Image.Height * fScale;

                        PointF fptHLineStart = new PointF(0, (imageHeight / 2f) / fScale);
                        PointF fptHLineEnd = new PointF(imageWidth, (imageHeight / 2f) / fScale);
                        PointF fptVLineStart = new PointF((imageWidth / 2f) / fScale, 0);
                        PointF fptVLineEnd = new PointF((imageWidth / 2f) / fScale, imageHeight);
                        //PointF fptHLineStart = new PointF(0, imageHeight / 2f - fVScroll);                // 사이즈가 유동적일 때 스크롤 위치에 따른 중심점 보정
                        //PointF fptHLineEnd = new PointF(imageWidth, imageHeight / 2f - fVScroll);
                        //PointF fptVLineStart = new PointF(imageWidth / 2f - fHScroll, 0);
                        //PointF fptVLineEnd = new PointF(imageWidth / 2f - fHScroll, imageHeight);


                        gp.DrawLine(Pens.Red, fptHLineStart, fptHLineEnd);
                        gp.DrawLine(Pens.Red, fptVLineStart, fptVLineEnd);
                    }                    
                }
                if (barCheckItemImageCrop.Checked)
                {
                    //GraphicsPath path = new GraphicsPath();

                    RectangleF frtTemp = Utils.RectRealToDraw(_fptCropStart, _fptCropEnd, 1, 0, 0);
                    _frtCrop = Utils.RectDrawToReal(frtTemp, 1, 0, 0);

                    PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;
                    PointF ptCenter = new PointF((_fptCropStart.X + _fptCropEnd.X) / 2f, (_fptCropStart.Y + _fptCropEnd.Y) / 2f);

                    Utils.CrossMarkPosition(
                         ptCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                         1, 0, 0, crossMarkOffset);

                    //RectangleF frtTemp = Utils.RectRealToDraw(_fptCropStart, _fptCropEnd, fScale, fHScroll, fVScroll);                    // 이미지 출력창의 크기가 가변일 경우 스크롤 위치를 포함한 보상값.
                    //_frtCrop = Utils.RectDrawToReal(frtTemp, fScale, fHScroll, fVScroll);

                    //PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;
                    //PointF ptCenter = new PointF((_fptCropStart.X + _fptCropEnd.X) / 2f, (_fptCropStart.Y + _fptCropEnd.Y) / 2f);

                    //Utils.CrossMarkPosition(
                    //     ptCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                    //     fScale, fHScroll, fVScroll, crossMarkOffset);

                    //textMousePos.Text = _fptCropStart.X.ToString() + "," + _fptCropStart.Y.ToString();
                    //textImagePos.Text = _fptCropEnd.X.ToString() + "," + _fptCropEnd.Y.ToString();

                    Pen newPenCrop = new Pen(Color.Black, 3.0f);
                    Pen oldPenCrop = new Pen(Color.LimeGreen, 1.5f);

                    path.AddRectangle(frtTemp);

                    gp.DrawPath(newPenCrop, path);
                    gp.DrawPath(oldPenCrop, path);

                    gp.DrawLine(newPenCrop, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(newPenCrop, ptCrossStart2, ptCrossEnd2);
                    gp.DrawLine(oldPenCrop, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(oldPenCrop, ptCrossStart2, ptCrossEnd2);
                    //gp.FillPath(Brushes.LightGreen, path);
                }
                if (_isSetROICheck)
                {
                    RectangleF frtTemp = Utils.RectRealToDraw(_fptAreaStart, _fptAreaEnd, 1, 0, 0);
                    _frtArearect = Utils.RectDrawToReal(frtTemp, 1, fHScroll, fVScroll);

                    PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;
                    PointF ptCenter = new PointF((_fptAreaStart.X + _fptAreaEnd.X) / 2f, (_fptAreaStart.Y + _fptAreaEnd.Y) / 2f);

                    Utils.CrossMarkPosition(
                         ptCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                         1, 0, 0, crossMarkOffset);

                    _workParams.AreaStart = _fptAreaStart;
                    _workParams.AreaEnd = _fptAreaEnd;

                    //_workParams.AreaCenter = ptCenter;
                    //_workParams. = _fptAreaStart.X;

                    //RectangleF frtTemp = Utils.RectRealToDraw(_fptCropStart, _fptCropEnd, fScale, fHScroll, fVScroll);                    // 이미지 출력창의 크기가 가변일 경우 스크롤 위치를 포함한 보상값.
                    //_frtCrop = Utils.RectDrawToReal(frtTemp, fScale, fHScroll, fVScroll);

                    //PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;
                    //PointF ptCenter = new PointF((_fptCropStart.X + _fptCropEnd.X) / 2f, (_fptCropStart.Y + _fptCropEnd.Y) / 2f);

                    //Utils.CrossMarkPosition(
                    //     ptCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                    //     fScale, fHScroll, fVScroll, crossMarkOffset);

                    Pen newPen = new Pen(Color.Black, 3.0f);
                    Pen oldPen = new Pen(Color.Yellow, 1.5f);

                    path.AddRectangle(frtTemp);

                    gp.DrawPath(newPen, path);
                    gp.DrawPath(oldPen, path);

                    gp.DrawLine(newPen, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(newPen, ptCrossStart2, ptCrossEnd2);
                    gp.DrawLine(oldPen, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(oldPen, ptCrossStart2, ptCrossEnd2);

                    path = new GraphicsPath();
                    fptDrawString = new PointF(_workParams.AreaStart.X + 25, _workParams.AreaEnd.Y + 25);
                    path.AddString(string.Format("Area Size(Start X:{0:0000.0},Start Y:{1:0000.0}, Stop X:{2:0000.0},Stop Y{3:0000.0} )", _workParams.AreaStart.X, _workParams.AreaStart.Y, _workParams.AreaEnd.X, _workParams.AreaEnd.Y), font.FontFamily, (int)font.Style, font.Size, fptDrawString, null);
                    gp.DrawPath(new Pen(Brushes.Black, 3), path);
                    gp.FillPath(Brushes.BlueViolet, path);
                }
                if (_isOpticalMeasurement)
                {
                    //for Debugging
                    Font mfont = new Font("Arial", 15f / fCharacter, FontStyle.Bold);
                    PointF mfptDrawString = new PointF();

                    for (int i = 0; i < _blobs.Count; ++i)
                    {
                        path = new GraphicsPath();

                        rtBlob = new RectangleF(_blobs[i].Left, _blobs[i].Top, _blobs[i].Width, _blobs[i].Height);

                        path.AddRectangle(rtBlob);

                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_workParams.ImageCenterX - crossMarkOffset, _workParams.ImageCenterY), new PointF(_workParams.ImageCenterX + crossMarkOffset, _workParams.ImageCenterY));
                        gp.DrawPath(new Pen(Brushes.Red, 1f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_workParams.ImageCenterX, _workParams.ImageCenterY - crossMarkOffset), new PointF(_workParams.ImageCenterX, _workParams.ImageCenterY + crossMarkOffset));
                        gp.DrawPath(new Pen(Brushes.Red, 1f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_blobs[i].CenterX - crossMarkOffset, _blobs[i].CenterY), new PointF(_blobs[i].CenterX + crossMarkOffset, _blobs[i].CenterY));
                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_blobs[i].CenterX, _blobs[i].CenterY - crossMarkOffset), new PointF(_blobs[i].CenterX, _blobs[i].CenterY + crossMarkOffset));
                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + ((_blobs[i].Height / 2) / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (350 * fCharacter));
                        path.AddString(string.Format("Blob Center(X:{0:000.0}, Y:{1:000.0})Pixel", _blobs[i].CenterX, _blobs[i].CenterY), mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.Yellow, path);

                        // Spot Size
                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + (_blobs[i].Height / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (300 * fCharacter));                        

                        path.AddString(string.Format("Spot Size(X:{0:000.000}mm, Y:{1:000.000}mm, Avg:{2:000.000}mm)", 
                                                        _blobs[i].Width * _systemParams._cameraParams.OnePixelResolution, 
                                                        _blobs[i].Height * _systemParams._cameraParams.OnePixelResolution, 
                                                        ((_blobs[i].Width + _blobs[i].Height) / 2f) * _systemParams._cameraParams.OnePixelResolution), 
                                                        mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.LightGreen, path);
                        
                        // Image Bright
                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + (_blobs[i].Height / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (250 * fCharacter));

                        path.AddString(string.Format("Spot Bright(X:{0:000.0}pixel, Y:{1:000.0}pixel, Avg:{2:000.0}pixel)",
                                                        _ImageHist_W[_blobs[i].PixelPeakXIndex],
                                                        _ImageHist_H[_blobs[i].PixelPeakYIndex],
                                                        _blobs[i].PixelPeak),
                                                        mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.LightGreen, path);

                        Pen newPen = new Pen(Color.White, 2f / fCharacter);
                        newPen.DashStyle = DashStyle.Dash;

                        gp.DrawLine(
                            newPen,
                            _blobs[i].CenterX,
                            _blobs[i].CenterY,
                            InspectionOpticalSpotCenterX,//_systemParams.InspectionOpticalSpotCenterX,
                            InspectionOpticalSpotCenterY);//_systemParams.InspectionOpticalSpotCenterY);

                        int j = 0;
                        double scale = (double)100 / (double)_ImageHist_H[_blobs[i].PixelPeakYIndex];
                        Pen histpen = new Pen(Color.White, 1);
                        for (j = 0; j < _systemParams._cameraParams.VResolution; j++)
                        {
                            gp.DrawLine(histpen, 0, j, (float)(_ImageHist_H[j] * scale), j);
                        }
                        scale = (double)100 / (double)_ImageHist_W[_blobs[i].PixelPeakXIndex];
                        for (j = 0; j < _systemParams._cameraParams.HResolution; j++)
                        {
                            gp.DrawLine(histpen, j, _systemParams._cameraParams.VResolution, j, (float)(_systemParams._cameraParams.VResolution - (_ImageHist_W[j] * scale)));
                        }
                        j = 0;
                    }

                    path = new GraphicsPath();
                    //fptDrawString = new PointF((_blobs[0].CenterX + _blobs[0].Width) / 1f + (20 * fCharacter),
                    //    (_systemParams.InspectionOpticalSpotCenterY + _blobs[0].CenterY) / 2f - (60 * fCharacter));

                    fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (200 * fCharacter));

                    float fDistance = (float)(Math.Sqrt(Math.Pow(InspectionOpticalSpotCenterX - _blobs[0].CenterX, 2) + Math.Pow(InspectionOpticalSpotCenterY - _blobs[0].CenterY, 2)) * _systemParams._cameraParams.OnePixelResolution);

                    //if (barCheckItemDistance.Checked)
                    //{
                    if (fDistance < Convert.ToSingle(rowAlignmentDistance.Properties.Value))
                    {
                        path.AddString(string.Format("Distance:{0:000.000}mm, dx:{1:000.000}mm, dy:{2:000.000}mm ", fDistance, 
                                                        (InspectionOpticalSpotCenterX - _blobs[0].CenterX) * _systemParams._cameraParams.OnePixelResolution * (-1), 
                                                        (InspectionOpticalSpotCenterY - _blobs[0].CenterY) * _systemParams._cameraParams.OnePixelResolution * (-1)), mfont.FontFamily, 
                                                        (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.LimeGreen, path);
                    }
                    else
                    {
                        path.AddString(string.Format("Distance:{0:000.000}mm, dx:{1:000.000}mm, dy:{2:000.000}mm ", fDistance,
                                                        (InspectionOpticalSpotCenterX - _blobs[0].CenterX) * _systemParams._cameraParams.OnePixelResolution * (-1),
                                                        (InspectionOpticalSpotCenterY - _blobs[0].CenterY) * _systemParams._cameraParams.OnePixelResolution * (-1)), mfont.FontFamily,
                                                        (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.Red, path);
                    }
                    //}

                    path = new GraphicsPath();
                    font = new Font("Arial", 50, FontStyle.Bold);
                    fptDrawString = new PointF(100, 50);

                    if (rtPassArea.Left <= rtBlob.Left && rtPassArea.Top <= rtBlob.Top && rtPassArea.Right >= rtBlob.Right && rtPassArea.Bottom >= rtBlob.Bottom
                        && fDistance < Convert.ToSingle(rowAlignmentDistance.Properties.Value))
                    {
                        path.AddString("PASS", font.FontFamily, (int)font.Style, font.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.LimeGreen, path);
                    }
                    else
                    {
                        path.AddString("FAIL", font.FontFamily, (int)font.Style, font.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.Red, path);
                    }                    
                }
                if (_isAutoInspectMeasurement)
                {
                    //for Debugging
                    Font mfont = new Font("Arial", 15f / fCharacter, FontStyle.Bold);
                    PointF mfptDrawString = new PointF();

                    for (int i = 0; i < _blobs.Count; ++i)
                    {
                        path = new GraphicsPath();

                        rtBlob = new RectangleF(_blobs[i].Left, _blobs[i].Top, _blobs[i].Width, _blobs[i].Height);

                        path.AddRectangle(rtBlob);

                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_workParams.ImageCenterX - crossMarkOffset, _workParams.ImageCenterY), new PointF(_workParams.ImageCenterX + crossMarkOffset, _workParams.ImageCenterY));
                        gp.DrawPath(new Pen(Brushes.Red, 1f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_workParams.ImageCenterX, _workParams.ImageCenterY - crossMarkOffset), new PointF(_workParams.ImageCenterX, _workParams.ImageCenterY + crossMarkOffset));
                        gp.DrawPath(new Pen(Brushes.Red, 1f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_blobs[i].CenterX - crossMarkOffset, _blobs[i].CenterY), new PointF(_blobs[i].CenterX + crossMarkOffset, _blobs[i].CenterY));
                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        path.AddLine(new PointF(_blobs[i].CenterX, _blobs[i].CenterY - crossMarkOffset), new PointF(_blobs[i].CenterX, _blobs[i].CenterY + crossMarkOffset));
                        gp.DrawPath(new Pen(Brushes.Black, 2f / fCharacter), path);
                        gp.DrawPath(new Pen(Brushes.Yellow, 1.2f / fCharacter), path);

                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + ((_blobs[i].Height / 2) / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (350 * fCharacter));
                        path.AddString(string.Format("Blob Center(X:{0:000.0}, Y:{1:000.0})Pixel", _blobs[i].CenterX, _blobs[i].CenterY), mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.Yellow, path);

                        // Spot Size
                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + (_blobs[i].Height / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (300 * fCharacter));

                        path.AddString(string.Format("Spot Size(X:{0:000.000}mm, Y:{1:000.000}mm, Avg:{2:000.000}mm)",
                                                        _blobs[i].Width * _systemParams._cameraParams.OnePixelResolution,
                                                        _blobs[i].Height * _systemParams._cameraParams.OnePixelResolution,
                                                        ((_blobs[i].Width + _blobs[i].Height) / 2f) * _systemParams._cameraParams.OnePixelResolution),
                                                        mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.LightGreen, path);

                        // Image Bright
                        path = new GraphicsPath();
                        //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + (_blobs[i].Height / fCharacter));
                        fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (250 * fCharacter));

                        path.AddString(string.Format("Spot Bright(X:{0:000.0}pixel, Y:{1:000.0}pixel, Avg:{2:000.0}pixel)",
                                                        _ImageHist_W[_blobs[i].PixelPeakXIndex],
                                                        _ImageHist_H[_blobs[i].PixelPeakYIndex],
                                                        _blobs[i].PixelPeak),
                                                        mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                        gp.FillPath(Brushes.LightGreen, path);

                        Pen newPen = new Pen(Color.White, 2f / fCharacter);
                        newPen.DashStyle = DashStyle.Dash;

                        gp.DrawLine(
                            newPen,
                            _blobs[i].CenterX,
                            _blobs[i].CenterY,
                            InspectionOpticalSpotCenterX,//_systemParams.InspectionOpticalSpotCenterX,
                            InspectionOpticalSpotCenterY);//_systemParams.InspectionOpticalSpotCenterY);

                        int j = 0;
                        double scale = (double)100 / (double)_ImageHist_H[_blobs[i].PixelPeakYIndex];
                        Pen histpen = new Pen(Color.White, 1);
                        for (j = 0; j < _systemParams._cameraParams.VResolution; j++)
                        {
                            gp.DrawLine(histpen, 0, j, (float)(_ImageHist_H[j] * scale), j);
                        }
                        scale = (double)100 / (double)_ImageHist_W[_blobs[i].PixelPeakXIndex];
                        for (j = 0; j < _systemParams._cameraParams.HResolution; j++)
                        {
                            gp.DrawLine(histpen, j, _systemParams._cameraParams.VResolution, j, (float)(_systemParams._cameraParams.VResolution - (_ImageHist_W[j] * scale)));
                        }
                        j = 0;
                    }

                    path = new GraphicsPath();

                    fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (200 * fCharacter));

                    float fDistance = (float)(Math.Sqrt(Math.Pow(InspectionOpticalSpotCenterX - _blobs[0].CenterX, 2) + Math.Pow(InspectionOpticalSpotCenterY - _blobs[0].CenterY, 2)) * _systemParams._cameraParams.OnePixelResolution);

                    if (fDistance < Convert.ToSingle(rowAlignmentDistance.Properties.Value))
                    {
                        path.AddString(string.Format("Distance:{0:000.000}mm, dx:{1:000.000}mm, dy:{2:000.000}mm ", fDistance,
                                                        (InspectionOpticalSpotCenterX - _blobs[0].CenterX) * _systemParams._cameraParams.OnePixelResolution * (-1),
                                                        (InspectionOpticalSpotCenterY - _blobs[0].CenterY) * _systemParams._cameraParams.OnePixelResolution * (-1)), mfont.FontFamily,
                                                        (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.LimeGreen, path);
                    }
                    else
                    {
                        path.AddString(string.Format("Distance:{0:000.000}mm, dx:{1:000.000}mm, dy:{2:000.000}mm ", fDistance,
                                                        (InspectionOpticalSpotCenterX - _blobs[0].CenterX) * _systemParams._cameraParams.OnePixelResolution * (-1),
                                                        (InspectionOpticalSpotCenterY - _blobs[0].CenterY) * _systemParams._cameraParams.OnePixelResolution * (-1)), mfont.FontFamily,
                                                        (int)mfont.Style, mfont.Size, fptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.Red, path);
                    }
                }
                if (_patternMatching)
                {
                    PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;
                    PointF ptCenter = new PointF((_systemParams._cameraParams.HResolution) / 2f, (_systemParams._cameraParams.VResolution) / 2f);

                    Utils.CrossMarkPosition(
                         ptCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                         1, 0, 0, crossMarkOffset);

                    gp.DrawLine(Pens.Red, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(Pens.Red, ptCrossStart2, ptCrossEnd2);

                    //////////////////////////////////////////////////////////////////////////////////////
                    // LED Mark
                    //PointF ptCrossStart1, ptCrossStart2, ptCrossEnd1, ptCrossEnd2;

                    Utils.CrossMarkPosition(
                         _workParams.AreaCenter, out ptCrossStart1, out ptCrossEnd1, out ptCrossStart2, out ptCrossEnd2,
                         1, 0, 0, crossMarkOffset);

                    Pen newPenPattern = new Pen(Color.Black, 3.0f);
                    Pen oldPenPattern = new Pen(Color.LightGreen, 1.5f);                    

                    gp.DrawPath(newPenPattern, path);
                    gp.DrawPath(oldPenPattern, path);

                    gp.DrawLine(newPenPattern, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(newPenPattern, ptCrossStart2, ptCrossEnd2);
                    gp.DrawLine(oldPenPattern, ptCrossStart1, ptCrossEnd1);
                    gp.DrawLine(oldPenPattern, ptCrossStart2, ptCrossEnd2);
                    // 매칭점 중심을 표시한다.
                    //gp.DrawLine(Pens.LightGreen, ptCrossStart1, ptCrossEnd1);
                    //gp.DrawLine(Pens.LightGreen, ptCrossStart2, ptCrossEnd2);

                    float fLedAlignmentCenterToLedMarkDistance = (float)Math.Sqrt(Math.Pow((_systemParams._cameraParams.HResolution / 2) - _workParams.AreaCenter.X, 2.0)
                        + Math.Pow((_systemParams._cameraParams.VResolution / 2) - _workParams.AreaCenter.Y, 2.0));

                    double dx, dy;
                    dx = ((_systemParams._cameraParams.HResolution / 2) - _workParams.AreaCenter.X) * _systemParams._cameraParams.OnePixelResolution * _systemParams._calibrationParams._imagetoSystemXcoordi;
                    dy = ((_systemParams._cameraParams.VResolution / 2) - _workParams.AreaCenter.Y) * _systemParams._cameraParams.OnePixelResolution * _systemParams._calibrationParams._imagetoSystemYcoordi;

                    path = new GraphicsPath();
                    Font mfont = new Font("Arial", 15f / fCharacter, FontStyle.Bold);
                    //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + ((_blobs[i].Height / 2) / fCharacter));
                    fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (300 * fCharacter));
                    path.AddString(string.Format("패턴유사도{0:0.0}%, 중심과 Pattern사이:{1:0.000}mm, dx:{2:0.000}mm, dy:{3:0.000}mm", 
                                                    _workParams.InspectionPositions[0].Similarity, fLedAlignmentCenterToLedMarkDistance * _systemParams._cameraParams.OnePixelResolution, dx, dy), 
                                    mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                    gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                    gp.FillPath(Brushes.LimeGreen, path);



                    path = new GraphicsPath();
                    //fptDrawString = new PointF(_blobs[i].CenterX * fCharacter, _blobs[i].CenterY + (_blobs[i].Height / fCharacter));
                    fptDrawString = new PointF((100 * fCharacter), _systemParams._cameraParams.VResolution - (250 * fCharacter));

                    path.AddString(string.Format("중심 픽셀 좌표 X:{0:0000.0}, Y:{1:0000.0}", _workParams.AreaCenter.X, _workParams.AreaCenter.Y), mfont.FontFamily, (int)mfont.Style, mfont.Size, fptDrawString, null);
                    gp.DrawPath(new Pen(Brushes.Black, 2 / fCharacter), path);
                    gp.FillPath(Brushes.LightGreen, path);
                    //gp.DrawString(
                    //        string.Format("패턴유사도{0:0.0}%, 중심과 Pattern사이:{1:0.000}mm, dx:{2:0.000}mm, dy:{3:0.000}mm", _workParams.InspectionPositions[0].Similarity, fLedAlignmentCenterToLedMarkDistance * _systemParams._cameraParams.OnePixelResolution, dx, dy), //_systemParams.CameraParameter.OnePixelResolution),
                    //        new Font(FontFamily.GenericSansSerif, 20.0F),
                    //        Brushes.LimeGreen,
                    //        new Point((int)(100 * fCharacter), (int)(_systemParams._cameraParams.VResolution - (200 * fCharacter))));
                    //new PointF((float)(_workParams.LEDMark.X * fScale - pictureEditImage.HScrollBar.Value), (float)(_workParams.LEDMark.Y * fScale - pictureEditImage.VScrollBar.Value - 15)));

                    // 검사 결과 표시


                    Font refont = new Font("Arial", 50, FontStyle.Bold);
                    

                    PointF refptDrawString = new PointF(100,50);

                    if (fLedAlignmentCenterToLedMarkDistance * _systemParams._cameraParams.OnePixelResolution <= Convert.ToSingle(rowResultPosition.Properties.Value))
                    {
                        path.AddString("PASS", refont.FontFamily, (int)font.Style, refont.Size, refptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.LimeGreen, path);
                    }
                    else
                    {
                        path.AddString("FAIL", refont.FontFamily, (int)font.Style, refont.Size, refptDrawString, null);
                        gp.DrawPath(new Pen(Brushes.Black, 3 / fCharacter), path);
                        gp.FillPath(Brushes.Red, path);
                    }
                    //_patternMatching = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void pictureEditSystemImage_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureEditSystemImage.Image == null)//|| barCheckItemInspectionStart.Checked)
                    return;

                GraphicsPath path = new GraphicsPath();

                float fScale = (float)(pictureEditSystemImage.Properties.ZoomPercent / 100f);
                float fHScroll = pictureEditSystemImage.HScrollBar.Value;
                float fVScroll = pictureEditSystemImage.VScrollBar.Value;

                path.AddRectangle(_frtCrop);
                path.AddRectangle(_frtArearect);

                PointF fptTemp = Utils.PointDrawToReal(e.Location, fScale, fHScroll, fVScroll);

                if (barCheckItemImageCrop.Checked)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (!path.IsVisible(fptTemp))
                        {
                            _fptCropStart = fptTemp;
                            _fptCropEnd = fptTemp;

                            pictureEditSystemImage.Cursor = Cursors.Cross;
                        }
                        else
                        {
                            _fptMoveStart = fptTemp;
                            pictureEditSystemImage.Cursor = Cursors.SizeAll;

                            _isCropMove = true;
                        }
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        Point ptPos = new Point(e.X, pictureEditSystemImage.Size.Height + e.Y);

                        if (path.IsVisible(fptTemp))
                        {
                            popupMenuTemplateCrop.ShowPopup(ptPos);
                        }
                    }
                }
                else
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        contextMenuStripImageROI.Show(e.Location);
                    }
                }
                if (_isSetROICheck)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (!path.IsVisible(fptTemp))
                        {
                            _fptAreaStart = fptTemp;
                            _fptAreaEnd = fptTemp;

                            pictureEditSystemImage.Cursor = Cursors.Cross;
                        }
                        else
                        {
                            _fptMoveStart = fptTemp;
                            pictureEditSystemImage.Cursor = Cursors.SizeAll;
                            _isAreaMove = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void pictureEditSystemImage_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureEditSystemImage.Image != null && !_isInspecting)
                {
                    // 마우스 좌표를 받아온다.
                    PointF ptMouse = e.Location;

                    float fScale = (float)(pictureEditSystemImage.Properties.ZoomPercent / 100f);

                    float fHScroll = pictureEditSystemImage.HScrollBar.Value;
                    float fVScroll = pictureEditSystemImage.VScrollBar.Value;

                    // 배율에 따른 실 좌표를 계산한다.
                    ptMouse = Utils.PointDrawToReal(ptMouse, fScale, fHScroll, fVScroll);

                    if (ptMouse.X >= 0 && ptMouse.Y >= 0 && ptMouse.X < _sourceImage.Width && ptMouse.Y < _sourceImage.Height)
                    {
                        //barStaticItemPosition.Caption = string.Format("( X:{0,4}, Y:{1,4} )", ptMouse.X, ptMouse.Y);

                        Color color = ((Bitmap)_sourceImage).GetPixel((int)ptMouse.X, (int)ptMouse.Y);
                        //barStaticItemPixelValue.ItemAppearance.Normal.ForeColor = color.R > 128 ? Color.Black : Color.White;
                        //barStaticItemPixelValue.ItemAppearance.Normal.BackColor = color;

                        //if (_sourceImage.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        //{
                        //    // barStaticItemPixelValue.Caption = string.Format("밝기:{0,3}", color.R);
                        //}
                        //else
                        //{
                        //    barStaticItemPixelValue.Caption = string.Format("R:{0,3}, G:{1,3}, B:{2,3}", color.R, color.G, color.B);
                        //}

                        // Crop Image
                        GraphicsPath path = new GraphicsPath();

                        path.AddRectangle(_frtCrop);
                        path.AddRectangle(_frtArearect);
                        //this.Text = string.Format("X:{0}, Y:{1}, Rect(left:{2}, top:{3}, right:{4}, bottom:{5})", ptMouse.X, ptMouse.Y, _frtCrop.Left, _frtCrop.Top, _frtCrop.Right, _frtCrop.Bottom);

                        if (barCheckItemImageCrop.Checked && e.Button == MouseButtons.Left)
                        {
                            if (_isCropMove)
                            {
                                float fDiffx = _fptMoveStart.X - ptMouse.X;
                                float fDiffy = _fptMoveStart.Y - ptMouse.Y;

                                _fptCropStart.X -= fDiffx; _fptCropStart.Y -= fDiffy;
                                _fptCropEnd.X -= fDiffx; _fptCropEnd.Y -= fDiffy;

                                _fptMoveStart = ptMouse;
                            }
                            else
                            {
                                if (_fptCropStart.Equals(ptMouse))
                                {
                                    return;
                                }
                                else
                                    _fptCropEnd = new PointF(ptMouse.X, ptMouse.Y);
                            }

                            pictureEditSystemImage.Refresh();
                        }
                        else
                        {
                            if (path.IsVisible(ptMouse))
                            {
                                if (pictureEditSystemImage.Cursor != Cursors.SizeAll)
                                    pictureEditSystemImage.Cursor = Cursors.SizeAll;
                            }
                            else
                            {
                                if (pictureEditSystemImage.Cursor != Cursors.Default)
                                    pictureEditSystemImage.Cursor = Cursors.Default;
                            }
                        }
                        if (_isSetROICheck && e.Button == MouseButtons.Left)
                        {
                            if (_isAreaMove)
                            {
                                float fDiffx = _fptMoveStart.X - ptMouse.X;
                                float fDiffy = _fptMoveStart.Y - ptMouse.Y;

                                _fptAreaStart.X -= fDiffx; _fptAreaStart.Y -= fDiffy;
                                _fptAreaEnd.X -= fDiffx; _fptAreaEnd.Y -= fDiffy;

                                _fptMoveStart = ptMouse;
                            }
                            else
                            {
                                if (_fptAreaStart.Equals(ptMouse))
                                {
                                    return;
                                }
                                else
                                    _fptAreaEnd = new PointF(ptMouse.X, ptMouse.Y);
                            }
                            pictureEditSystemImage.Refresh();
                        }
                        else
                        {
                            if (path.IsVisible(ptMouse))
                            {
                                if (pictureEditSystemImage.Cursor != Cursors.SizeAll)
                                    pictureEditSystemImage.Cursor = Cursors.SizeAll;
                            }
                            else
                            {
                                if (pictureEditSystemImage.Cursor != Cursors.Default)
                                    pictureEditSystemImage.Cursor = Cursors.Default;
                            }
                        }
                    }
                }
                else
                {
                    // 이미지가 없는 경우는 초기화 상태로 돌린다.
                    ;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void pictureEditSystemImage_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                GraphicsPath path = new GraphicsPath();

                float fScale = (float)(pictureEditSystemImage.Properties.ZoomPercent / 100f);
                float fHScroll = pictureEditSystemImage.HScrollBar.Value;
                float fVScroll = pictureEditSystemImage.VScrollBar.Value;

                path.AddRectangle(_frtCrop);
                path.AddRectangle(_frtArearect);

                if (e.Button == MouseButtons.Right)
                {
                    ;
                }

                if (barCheckItemImageCrop.Checked && e.Button == MouseButtons.Left)
                {
                    //PointF fptTemp = Utils.PointDrawToReal(e.Location, 1.0f, fHScroll, fVScroll);
                    PointF fptTemp = Utils.PointDrawToReal(e.Location, fScale, fHScroll, fVScroll);

                    if (!path.IsVisible(fptTemp))
                    {
                        if (!_fptCropStart.Equals(fptTemp))
                        {
                            _fptCropEnd = fptTemp;
                        }
                        else
                        {
                            _fptCropStart = new PointF(_frtCrop.Left, _frtCrop.Top);
                            _fptCropEnd = new PointF(_frtCrop.Right, _frtCrop.Bottom);
                        }

                        pictureEditSystemImage.Cursor = Cursors.Default;
                    }
                    //textMousePos.Text = e.X.ToString() + "," + e.Y.ToString();
                    //int ImgX, ImgY;
                    //ImgX = e.X + Convert.ToInt32(fHScroll);
                    //ImgY = e.Y + Convert.ToInt32(fVScroll);
                    //textImagePos.Text = _fptCropEnd.X.ToString() + "," + _fptCropEnd.Y.ToString();

                    _isCropMove = false;

                    pictureEditSystemImage.Refresh();
                }
                if (_isSetROICheck && e.Button == MouseButtons.Left)
                {
                    PointF fptTemp = Utils.PointDrawToReal(e.Location, fScale, fHScroll, fVScroll);

                    if (!path.IsVisible(fptTemp))
                    {
                        if (!_fptAreaStart.Equals(fptTemp))
                        {
                            _fptAreaEnd = fptTemp;
                            if (_fptAreaStart.X > _fptAreaEnd.X)
                            {
                                if (_fptAreaStart.Y > _fptAreaEnd.Y)
                                {
                                    _fptAreaEnd = _fptAreaStart;
                                    _fptAreaStart = fptTemp;
                                }
                                else
                                {
                                    _fptAreaEnd.X = _fptAreaStart.X;
                                    _fptAreaStart.X = fptTemp.X;
                                }
                            }
                            else
                            {
                                if (_fptAreaStart.Y > _fptAreaEnd.Y)
                                {
                                    _fptAreaEnd.Y = _fptAreaStart.Y;
                                    _fptAreaStart.Y = fptTemp.Y;
                                }
                            }
                        }
                        else
                        {
                            _fptAreaStart = new PointF(_frtArearect.Left, _frtArearect.Top);
                            _fptAreaEnd = new PointF(_frtArearect.Right, _frtArearect.Bottom);
                        }

                        pictureEditSystemImage.Cursor = Cursors.Default;
                    }
                    _isAreaMove = false;
                    pictureEditSystemImage.Refresh();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void pictureEditSystemImage_ImageChanged(object sender, EventArgs e)
        {
            try
            {
                pictureEditSystemImage.HScrollBar.Value = Convert.ToInt32(_fHScrollPos);
                pictureEditSystemImage.VScrollBar.Value = Convert.ToInt32(_fVScrollPos);
            }
            catch (Exception ex)
            {
            }
        }

        private void barButtonItemImageZoomIn_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pictureEditSystemImage.Image != null)
            {
                if (pictureEditSystemImage.Properties.ZoomPercent < 1600)
                {
                    pictureEditSystemImage.Properties.ZoomPercent *= 2;

                    _isImageFitSize = false;

                    //_log.WriteLog(LogLevel.Info, LogClass.atSFL.ToString(), string.Format("확대: {0:0.0}%", pictureEditImage.Properties.ZoomPercent));
                }
            }
        }

        private void barButtonItemImageZoomOut_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pictureEditSystemImage.Image != null)
            {
                if (pictureEditSystemImage.Properties.ZoomPercent >= 12.5)
                {
                    pictureEditSystemImage.Properties.ZoomPercent /= 2;

                    _isImageFitSize = false;

                    //_log.WriteLog(LogLevel.Info, LogClass.atSFL.ToString(), string.Format("축소: {0:0.0}%", pictureEditImage.Properties.ZoomPercent));
                }
            }
        }

        private void barButtonItemImageOriginSize_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pictureEditSystemImage.Image != null)
            {
                _isImageFitSize = false;

                pictureEditSystemImage.Properties.ZoomPercent = 100;

                //_log.WriteLog(LogLevel.Info, LogClass.atSFL.ToString(), string.Format("원래 크기: {0:0.0}%", pictureEditImage.Properties.ZoomPercent));
            }
        }

        private void barButtonItemFitSize_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_sourceImage != null)
            {
                try
                {
                    float width = pictureEditSystemImage.ClientSize.Width * 100.0f / _sourceImage.Width;
                    float height = (pictureEditSystemImage.ClientSize.Height - pictureEditSystemImage.ClientSize.Height * 0.01f) * 100.0f / _sourceImage.Height;

                    float i = Math.Min(100.0f, Math.Min(width, height));

                    pictureEditSystemImage.Properties.ZoomPercent = i;
                    pictureEditSystemImage.HScrollBar.Value = 0;
                    pictureEditSystemImage.VScrollBar.Value = 0;

                    _isImageFitSize = true;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("원본 화면 맞춤: {0:0.0}%", pictureEditSystemImage.Properties.ZoomPercent));
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        private void barCheckItemImageCrop_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            try
            {
                pictureEditSystemImage.Refresh();
            }
            catch (Exception ex)
            {

            }
        }

        private void barButtonItemImageSave_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (pictureEditSystemImage.Image != null)
            {
                if (barCheckItemImageCrop.Checked)
                {
                    string strBackupFilter = saveFileDialogImage.Filter;

                    try
                    {
                        saveFileDialogImage.Filter = "Bitmap Files(*.bmp) | *.bmp";

                        if (saveFileDialogImage.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap templete = new Bitmap(_sourceImage.Width, _sourceImage.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                            templete = ConverterColorToGray((Bitmap) pictureEditSystemImage.Image);
                            if (_frtCrop.Width > 0 && _frtCrop.Height > 0)
                                Utils.SaveImage((Bitmap)templete, _frtCrop, saveFileDialogImage.FileName);
                            else
                                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("이미지 저장 실패(너비:{0}, 높이:{1})", (int)_frtCrop.Width, (int)_frtCrop.Height));
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.StackTrace.ToString());
                    }
                    finally
                    {
                        saveFileDialogImage.Filter = strBackupFilter;
                    }

                }
                else
                {
                    try
                    {
                        if (saveFileDialogImage.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap templete = new Bitmap(pictureEditSystemImage.Image.Width, pictureEditSystemImage.Image.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                            templete = ConverterColorToGray((Bitmap)pictureEditSystemImage.Image);
                            pictureEditSystemImage.Image = templete;

                            //Utils.SaveImage((Bitmap)templete, new RectangleF(0, 0, (float)pictureEditSystemImage.Image.Width, (float)pictureEditSystemImage.Image.Height), saveFileDialogImage.FileName);
                            ///*
                            if (saveFileDialogImage.FilterIndex == 1)
                                pictureEditSystemImage.Image.Save(saveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            else if (saveFileDialogImage.FilterIndex == 2)
                                pictureEditSystemImage.Image.Save(saveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                            else if (saveFileDialogImage.FilterIndex == 3)
                                pictureEditSystemImage.Image.Save(saveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            else if (saveFileDialogImage.FilterIndex == 4)
                                pictureEditSystemImage.Image.Save(saveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Icon);
                            else if (saveFileDialogImage.FilterIndex == 5)
                                pictureEditSystemImage.Image.Save(saveFileDialogImage.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            //*/

                            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("이미지 저장: {0}", saveFileDialogImage.FileName));
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.StackTrace.ToString());
                    }
                }
            }
        }

        private void barButtonItemImageOpen_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (openFileDialogImageFileOpen.ShowDialog() == DialogResult.OK)
                {
                    _sourceImage = System.Drawing.Image.FromFile(openFileDialogImageFileOpen.FileName);
                    pictureEditSystemImage.Image = _sourceImage;
                    _patternMatching = false;
                    _isOpticalMeasurement = false;
                    pictureEditSystemImage.Refresh();
                    barButtonItemFitSize.PerformClick();
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }

        private void repositoryItemButtonLedSpotImagePath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                if (openFileDialogInspectLedImage.ShowDialog() == DialogResult.OK)
                {
                    rowSpotImagePath.Properties.Value = openFileDialogInspectLedImage.FileName;

                    rowSpotImage.Properties.Value = System.Drawing.Image.FromFile(openFileDialogInspectLedImage.FileName);

                    vGridControl2.Refresh();

                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("LED Spot Image 로드완료:{0}", openFileDialogInspectLedImage.FileName));
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private void repositoryItemButtonPatterImagePath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                if (openFileDialogPatternImage.ShowDialog() == DialogResult.OK)
                {
                    rowTempletePath.Properties.Value = openFileDialogPatternImage.FileName;
                    _workParams.MatchingImagePath = openFileDialogPatternImage.FileName;
                    rowTempleteImage.Properties.Value = System.Drawing.Image.FromFile(openFileDialogPatternImage.FileName);

                    vGridControl2.Refresh();

                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("Pattern Image 로드완료:{0}", openFileDialogPatternImage.FileName));
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private void MenualInspectButton_Click(object sender, EventArgs e)
        {
            try
            {
                _patternMatching = false;
                
                if (pictureEditSystemImage.Image != null)
                {
                    if (_isContinuousShot)
                    {
                        mLog.WriteLog(LogLevel.Warn, LogClass.atPhoto.ToString(), string.Format("연속 샷 모드를 해제하세요."));
                    }
                    Bitmap tempImage = Utils.Clone<Bitmap>((Bitmap)_sourceImage);
                    Bitmap outImage = Utils.Clone<Bitmap>((Bitmap)_sourceImage);
                    Bitmap inspectsource = new Bitmap(_sourceImage.Width, _sourceImage.Height);
                    if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                    {
                        inspectsource = ConverterColorToGray(tempImage);
                        tempImage = inspectsource;
                        outImage = inspectsource;
                    }

                    if ((_fptAreaEnd.X == 0) && (_fptAreaEnd.Y == 0))
                    {
                        _fptAreaEnd.X = _systemParams._cameraParams.HResolution;
                        _fptAreaEnd.Y = _systemParams._cameraParams.VResolution;
                        _workParams.AreaEnd = _fptAreaEnd;
                    }

                    _blobs.Clear();

                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("광축 측정 시작"));

                    OpticalSpot spot = new OpticalSpot(_systemParams);
                    OpticalHistory history = new OpticalHistory(_systemParams);

                    spot._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                    spot.IsLog = true;

                    history._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                    history.IsLog = true;
                    //Array.Clear(_ImageHist_W, 0, _ImageHist_W.Length);
                    //Array.Clear(_ImageHist_H, 0, _ImageHist_H.Length);
                    if (_ImageHist_H == null)
                    {
                        _ImageHist_H = new double[tempImage.Height];
                    }
                    if (_ImageHist_W == null)
                    {
                        _ImageHist_W = new double[tempImage.Width];
                    }
                    spot.OpticalSpotBlobProcess(tempImage, _blobs, _workParams, Convert.ToInt32(rowThresholdH.Properties.Value),Convert.ToInt32(rowThresholdV.Properties.Value), Convert.ToInt32(Convert.ToSingle(rowSpotBlobHSizeMin.Properties.Value)/(_systemParams._cameraParams.OnePixelResolution)), Convert.ToInt32(Convert.ToSingle(rowSpotBlobHSizeMax.Properties.Value) / (_systemParams._cameraParams.OnePixelResolution)),ref _ImageHist_W,ref _ImageHist_H);
                    //spot.OpticalSpotBlobProcess(tempImage, _blobs, Convert.ToInt32(rowThreshold.Properties.Value), Convert.ToInt32(rowSpotBlobHSizeMin.Properties.Value), Convert.ToInt32(rowSpotBlobHSizeMax.Properties.Value));
                    //history.OpticalHistoryProcess(tempImage, Convert.ToInt32(rowThreshold.Properties.Value));
                    _isOpticalMeasurement = true;
                    _isAutoInspectMeasurement = false;
                    _resultImage = outImage;
                    pictureEditSystemImage.Refresh();
                }
                else
                {
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("LED 검사 이미지가 로드되지 않았습니다"));
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("LED 검사 프로세싱이 실행되지 않았습니다."));
            }
        }

        private void menualPatternMatchingButton_Click(object sender, EventArgs e)
        {
            try
            {
                _isOpticalMeasurement = false;
                InspectionPosition inspectionPos = new InspectionPosition();
                inspectionPos.Index = 0;
                inspectionPos.PositionX = 0;
                inspectionPos.PositionY = 0;                
                inspectionPos.PositionZ = 0;                
                inspectionPos.ePositionType = INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE;
                _workParams.InspectionPositions.Add(inspectionPos);
                _workParams.MatchingSimilarityThreshold = Convert.ToInt32(rowrThresholdValue.Properties.Value);
                _workParams.Blobs.Clear();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("패턴 매칭 시작"));

                if (pictureEditSystemImage.Image == null)
                {
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("입력 영상 또는 매칭 영상 읽기 실패"));
                    return;
                }

                TemplateMatch tm = new TemplateMatch();

                tm._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                tm.IsLog = true;

                Bitmap tempImage = Utils.Clone<Bitmap>((Bitmap)_sourceImage);
                Bitmap outImage = Utils.Clone<Bitmap>((Bitmap)_sourceImage);
                Bitmap inspectsource = new Bitmap(_sourceImage.Width, _sourceImage.Height);

                if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    inspectsource = ConverterColorToGray(tempImage);
                    tempImage = inspectsource;
                    outImage = tempImage;
                }
                if ((_fptAreaEnd.X == 0) && (_fptAreaEnd.Y == 0))
                {
                    _fptAreaEnd.X = _systemParams._cameraParams.HResolution;
                    _fptAreaEnd.Y = _systemParams._cameraParams.VResolution;
                    _workParams.AreaEnd = _fptAreaEnd;
                }

                if (tm.PatternMatching(outImage, _workParams, 0))
                {
                    _patternMatching = true;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("패턴 매칭 완료"));                    
                    pictureEditSystemImage.Image = _sourceImage;
                    pictureEditSystemImage.Refresh();           
                    //_resultImage = outImage;                                      
                    barButtonItemFitSize.PerformClick();
                }
                else
                {
                    _patternMatching = false;
                    mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("패턴 매칭 실패"));
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("패턴 매칭 프로세싱이 실행되지 않았습니다."));
            }
        }
        public Bitmap ConverterColorToGray(Bitmap colorBitmap)
        {
            //int Width = colorBitmap.Width;
            //int Height = colorBitmap.Height;

            //BitmapData cBd = colorBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, colorBitmap.PixelFormat);

            //int cStride = cBd.Stride;
            //int cOffset = cStride - ((Width * Bitmap.GetPixelFormatSize(colorBitmap.PixelFormat)) / 8);

            //Bitmap grayBitmap = new Bitmap(colorBitmap.Width,colorBitmap.Height,PixelFormat.Format8bppIndexed);
            //BitmapData gBd = grayBitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            //int gStride = gBd.Stride;
            //int gOffset = gStride - (Width * Bitmap.GetPixelFormatSize(grayBitmap.PixelFormat) / 8);

            //System.Drawing.Imaging.ColorPalette palette;
            //palette = grayBitmap.Palette;
            //for (int i = 0; i < 256; i++)
            //{
            //    Color tmp = Color.FromArgb(255, i, i, i);
            //    palette.Entries[i] = Color.FromArgb(255, i, i, i);
            //}
            //grayBitmap.Palette = palette;

            //unsafe
            //{
            //    byte* cPtr = (byte*)cBd.Scan0.ToPointer();
            //    byte* gPtr = (byte*)gBd.Scan0.ToPointer();

            //    for (int y = 0; y < Height; y++)
            //    {
            //        for (int x = 0; x < Width; x++)
            //        {
            //            //byte a = cPtr[0];
            //            byte b = cPtr[0];
            //            byte g = cPtr[1];
            //            byte r = cPtr[2];

            //            gPtr[0] = (byte)((r + g + b) / 3);
            //            cPtr += 3;
            //            gPtr += 1;
            //        }
            //        cPtr += cOffset;
            //        gPtr += gOffset;
            //    }
            //    colorBitmap.UnlockBits(cBd);
            //    grayBitmap.UnlockBits(gBd);
            //}
            //return grayBitmap;
            int w = colorBitmap.Width,
                h = colorBitmap.Height,
                r, ic, oc, bmpStride, outputStride, bytesPerPixel;
            PixelFormat pfIn = colorBitmap.PixelFormat;            
            BitmapData bmpData, outputData;

            Bitmap outImage = colorBitmap;
            //Create the new bitmap
            outImage = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //Build a grayscale color Palette
            System.Drawing.Imaging.ColorPalette cvtpalette = outImage.Palette;
            for (int i = 0; i < 256; i++)
            {
                Color tmp = Color.FromArgb(255, i, i, i);
                cvtpalette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            outImage.Palette = cvtpalette;
            
            //Get the number of bytes per pixel
            switch (pfIn)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                default: throw new InvalidOperationException("Image format not supported");
            }

            //Lock the images
            bmpData = colorBitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                    pfIn);
            outputData = outImage.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            bmpStride = bmpData.Stride;
            outputStride = outputData.Stride;

            //Traverse each pixel of the image
            unsafe
            {
                byte* bmpPtr = (byte*)bmpData.Scan0.ToPointer(),
                outputPtr = (byte*)outputData.Scan0.ToPointer();

                if (bytesPerPixel == 3)
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = .299*R + .587*G + .114*B
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 3, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                (0.299f * bmpPtr[r * bmpStride + ic] +
                                    0.587f * bmpPtr[r * bmpStride + ic + 1] +
                                    0.114f * bmpPtr[r * bmpStride + ic + 2]);
                }
                else //bytesPerPixel == 4
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = alpha * (.299*R + .587*G + .114*B)
                    // L = (.299*R + .587*G + .114*B) * alpha
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 4, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                ((bmpPtr[r * bmpStride + ic + 3] / 255.0f) *
                                (0.299f * bmpPtr[r * bmpStride + ic ] +
                                    0.587f * bmpPtr[r * bmpStride + ic + 1] +
                                    0.114f * bmpPtr[r * bmpStride + ic + 2]));
                }
            }

            //Unlock the images
            colorBitmap.UnlockBits(bmpData);
            outImage.UnlockBits(outputData);
            return outImage;
        }
        private void contextMenuStripImageROI_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "Set Templete Image Area":
                    if (barCheckItemImageCrop.Checked == false)
                    {
                        barCheckItemImageCrop.Checked = true;
                        e.ClickedItem.Image = global::atOpticalDecenter.Properties.Resources.Apply_16x16;
                    }
                    else
                    {
                        barCheckItemImageCrop.Checked = false;
                        e.ClickedItem.Image = global::atOpticalDecenter.Properties.Resources.Cancel_16x16;
                        _fptCropStart = new PointF();
                        _fptCropEnd = new Point();
                        _frtCrop = new RectangleF();
                        pictureEditSystemImage.Refresh();
                    }
                    break;
                case "Set Work ROI":
                    if (_isSetROICheck == false)
                    {
                        _isSetROICheck = true;
                        e.ClickedItem.Image = global::atOpticalDecenter.Properties.Resources.Apply_16x16;
                    }
                    else
                    {
                        _isSetROICheck = false;
                        e.ClickedItem.Image = global::atOpticalDecenter.Properties.Resources.Cancel_16x16;
                        //_fptAreaStart = new PointF();
                        //_fptAreaEnd = new PointF();
                        //_frtArearect = new RectangleF();
                        pictureEditSystemImage.Refresh();
                    }
                    break;
                case "Clear Work ROI":                
                    _fptAreaStart = new PointF();
                    _fptAreaEnd = new PointF();
                    _frtArearect = new RectangleF();
                    pictureEditSystemImage.Refresh();
                    break;
                case "Fit Size Image":
                    barButtonItemFitSize.PerformClick();
                    break;

            }
        }      

        private void barButtonItemInitializeStatistics_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (MessageBox.Show("검사 통계를 초기화하시겠습니까?", "통계 초기화", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    return;

                this.Cursor = Cursors.WaitCursor;

                _statistics.TotalCount = 0;
                _statistics.FailCount = 0;
                _statistics.PassCount = 0;

                if (_systemParams._SystemLanguageKoreaUse)
                {
                    barEditItemTotalInspectionCount.EditValue = string.Format("총 검사 수: {0:00000}", _statistics.TotalCount);
                    barEditItemTotalFailCount.EditValue = string.Format("불합격: {0:00000}", _statistics.FailCount);
                }
                else
                {
                    barEditItemTotalInspectionCount.EditValue = string.Format("Total Count: {0:00000}", _statistics.TotalCount);
                    barEditItemTotalFailCount.EditValue = string.Format("Fail Count: {0:00000}", _statistics.FailCount);
                }

                for (int i = 0; i < _statistics.Statistics.Count; ++i)
                {
                    _statistics.Statistics[i] = 0;                    
                }

                RecipeFileIO.WriteInspectionStatisticsFile(string.Format(@"{0}\{1}", SystemDirectoryParams.SystemFolderPath, SystemDirectoryParams.StatisticsFileName), _statistics);
                InitializeChartOpticalInspect();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("검사 통계를 초기화합니다. 총 검사 수:{0:00000}, 불합격:{1:00000}", _statistics.TotalCount, _statistics.FailCount));
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.Message);
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.StackTrace.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void barCheckItemInspectionStart_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_isInspecting == false)
            {
                CheckTackTime.Reset();
                barCheckItemInspectionStart.Caption = string.Format("검사 중지");
                _isInspecting = true;
                _isInspectError = false;
                // PLC 통신 연결 및 상태 정보 확인 후 로봇상태가 아닐 경우 검사 중지! 구문 추가.
                //

                if (_isContinuousShot)
                {
                    _Camera.Stop();
                    _isContinuousShot = false;
                    barButtonItemSingleShot.Enabled = true;
                    barButtonItemContinueousShot.Enabled = true;
                }
                barCheckItemShowCenterMark.Enabled = true;

                if (mRobotInformation.GetStatus(RobotInformation.RobotStatus.OperationReady))
                {
                    mStepBase.SetJobInfo(mLogin.JobInformation);
                    mStepBase.SetProductSeries((PhotoProduct.Enums.ProductSeries)_workParams._ProductSeries);
                    mStepBase.SetProductType((PhotoProduct.Enums.ProductType)_workParams._ProductType);
                    mStepBase.SetProductName(_workParams._ProductModelName);
                    mStepBase.SetOutputType((PhotoProduct.Enums.OutputType)_workParams._ProductOutputType);
                    mStepBase.SetOPMode((PhotoProduct.Enums.OperatingMode)_workParams._ProductOperatingMdoe);
                    mStepBase.SetDetectMertrial((PhotoProduct.Enums.DetectMeterial)_workParams._ProductDetectMerterial);
                    mStepBase.ClearTimeForFullSequence();

                    MakeInspectionList();
                    _InspectionWorking = true;
                    _InspectionResult = false;
                    mInspectStep = InspectionStepType.CheckWaitRobotReady;

                    InspectionRecipeParameterSetup();

                    CheckTackTime.Start();
                    _backgroundWorkerOpticalDecenterInspection.RunWorkerAsync();
                    AutoStartButtonLock();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "포토 센서 검사 실행");
                }
                else
                {
                    barCheckItemInspectionStart.Caption = string.Format("검사 시작");
                    _isInspecting = false;
                    MessageBox.Show("모션 원점복귀를 실행하세요","검사 중지");
                }

            }
            else
            {
                barCheckItemInspectionStart.Caption = string.Format("검사 시작");
                _isInspecting = false;
                _InspectionWorking = false;
                _isInspectError = false;
                _InspectionResult = false;
                mInspectStep = InspectionStepType.Idle;
                _backgroundWorkerOpticalDecenterInspection.CancelAsync();
                UpdateProcessTime(false);
                barEditItemInspectionResult.EditValue = "Ready";
                AutoStartButtonRelease();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "포토 센서 검사 강제 종료");
            }
        }
        public void InspectionSequenceStart()
        {
            if (_isInspecting == false)
            {
                CheckTackTime.Reset();                
                _isInspecting = true;
                _isInspectError = false;
                // PLC 통신 연결 및 상태 정보 확인 후 로봇상태가 아닐 경우 검사 중지! 구문 추가.
                //
                if (_isContinuousShot)
                {
                    _Camera.Stop();
                    _isContinuousShot = false;
                    barButtonItemSingleShot.Enabled = true;
                    barButtonItemContinueousShot.Enabled = true;
                }
                barCheckItemShowCenterMark.Enabled = true;

                mStepBase.SetJobInfo(mLogin.JobInformation);
                mStepBase.SetProductSeries((PhotoProduct.Enums.ProductSeries)_workParams._ProductSeries);
                mStepBase.SetProductType((PhotoProduct.Enums.ProductType)_workParams._ProductType);
                mStepBase.SetProductName(_workParams._ProductModelName);
                mStepBase.SetOutputType((PhotoProduct.Enums.OutputType)_workParams._ProductOutputType);
                mStepBase.SetOPMode((PhotoProduct.Enums.OperatingMode)_workParams._ProductOperatingMdoe);
                mStepBase.SetDetectMertrial((PhotoProduct.Enums.DetectMeterial)_workParams._ProductDetectMerterial);
                mStepBase.ClearTimeForFullSequence();

                MakeInspectionList();
                _InspectionWorking = true;
                _InspectionResult = false;
                mInspectStep = InspectionStepType.CheckWaitRobotReady;

                InspectionRecipeParameterSetup();                
                CheckTackTime.Start();
                _backgroundWorkerOpticalDecenterInspection.RunWorkerAsync();
                barCheckItemInspectionStart.Caption = string.Format("검사 중지");
                AutoStartButtonLock();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "포토 센서 검사 실행");
            }
        }
        public void InspectionSequenceStop()
        {
            if (_isInspecting)
            {                
                _isInspecting = false;
                _InspectionWorking = false;
                _isInspectError = false;
                _InspectionResult = false;
                mInspectStep = InspectionStepType.Idle;
                _backgroundWorkerOpticalDecenterInspection.CancelAsync();
                UpdateProcessTime(false);
                barCheckItemInspectionStart.Caption = string.Format("검사 시작");                
                AutoStartButtonRelease();
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "포토 센서 검사 강제 종료");
            }
        }
        private void barButtonItemWorkInfo_ItemClick(object sender, ItemClickEventArgs e)
        {
            //LogInForm logInForm = new LogInForm(_admsEquipment.WorkerID, _admsEquipment.WorkerName, _admsEquipment.JobInformation);
            LoginForm logInForm = new LoginForm(_systemParams._SystemLanguageKoreaUse);
            if (logInForm.ShowDialog() == DialogResult.OK)
            {
                if (_admsEquipment.WorkerID.Equals(logInForm.WorkerID) && _admsEquipment.WorkerName.Equals(logInForm.WorkerName) && _admsEquipment.JobInformation.Equals(logInForm.JobInformation))
                {
                    MessageBox.Show("같은 로그인 정보입니다.", "변경", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string strLog = string.Format("로그인 정보가 변경되었습니다.\n\n작업자 사번: {0} → {1}\n작업자 이름: {2} → {3}\n작업지시서: {4} → {5}\n\n변경하시겠습니까?",
                        _admsEquipment.WorkerID, logInForm.WorkerID, _admsEquipment.WorkerName, logInForm.WorkerName, _admsEquipment.JobInformation, logInForm.JobInformation);

                    if (MessageBox.Show(strLog, "변경", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        //if (_admsEquipment.JobInformation != logInForm.JobInformation)
                        //{
                        //    _admsEquipment.Time = DateTime.Now;
                        //    _admsEquipment.Event = ADMSEquipmentEvent.JOB;
                        //    _admsEquipment.EventSubState = ADMSEquipmentEventSubState.CHANGE;
                        //    _admsEquipment.JobInformation = logInForm.JobInformation;
                        //    _admsEquipment.Description = "작업지시서를 변경합니다.";

                        //    if (_systemParams._admsParams._enableCheck)
                        //    {
                        //        ;// DB 상태 업데이트 구문 추가 필요!!
                        //    }
                        //}

                        _admsEquipment.WorkerID = logInForm.WorkerID;
                        _admsEquipment.WorkerName = logInForm.WorkerName;
                        _admsEquipment.JobInformation = logInForm.JobInformation;

                        if (_systemParams._bJobWorkInfomationEnable)
                        {

                            if (_JobWorkDbCtrl.SearchDBforKeyword("work_ord_no", string.Format("{0}", logInForm.JobInformation), true))
                            {
                                string recpename = string.Empty;

                                recpename = GetRecipeNameToDB();

                                if (recpename != "")
                                    RecipeOpen(recpename);

                                MessageBox.Show("작업지시서를 확인 후 레시피를 선택 했습니다.", "DB확인", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            }
                            else
                                MessageBox.Show("작업지시서를 없습니다.", "DB 미확인", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        else
                        {
                            barListItemRecipeOpen.Enabled = true;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("로그인 정보가 변경되지 않았습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }        
        public void InspectionRecipeParameterSetup()
        {
            // Camera exposure time change
            double oldValue = _Camera.ExposureTime.Value;
            CameraParameters cameraParam = new CameraParameters();
            int ExposerRange = _workParams._LEDInspectionExposureTime;
            if ((ExposerRange >= 50) && (ExposerRange <= 10000000))
            {
                cameraParam.Value = Convert.ToInt32(_workParams._LEDInspectionExposureTime);
                _Camera.ExposureTime = cameraParam;
                mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("카메라 노출 시간 변경 전:{0}, 변경 후:{1}", (int)oldValue, (int)cameraParam.Value));
            }
        }

        private void MenualOpticalInspectButton_Click(object sender, EventArgs e)
        {
            try
            {
                _patternMatching = false;
                _isOpticalMeasurement = false;
                if ((Spot1BlobImage != null) && (Spot2BlobImage != null))
                {
                    Bitmap tempImage = Utils.Clone<Bitmap>((Bitmap)Spot1BlobImage);                    
                    Bitmap inspectsource = new Bitmap(Spot1BlobImage.Width, Spot1BlobImage.Height);
                    if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                    {                       
                        inspectsource = ConverterColorToGray(tempImage);
                        tempImage = inspectsource;
                    }

                    if ((_fptAreaEnd.X == 0) && (_fptAreaEnd.Y == 0))
                    {
                        _fptAreaEnd.X = _systemParams._cameraParams.HResolution;
                        _fptAreaEnd.Y = _systemParams._cameraParams.VResolution;
                        _workParams.AreaEnd = _fptAreaEnd;
                    }

                    _MenaulInspectLed1blobs.Clear();
                    _MenaulInspectLed2blobs.Clear();
                    mResultData.ClearData();
                    mResultData.InspectParameterInitial(Convert.ToDouble(rowInspectSpotProductDistance.Properties.Value), Convert.ToDouble(rowInspectSpotInspectDistance.Properties.Value), tempImage.Width, tempImage.Height, (double)_systemParams._cameraParams.OnePixelResolution);

                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("투광 LED 발산각 측정 시작"));

                    OpticalSpot spot = new OpticalSpot(_systemParams);
                    OpticalHistory history = new OpticalHistory(_systemParams);

                    spot._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                    spot.IsLog = true;

                    history._log.WriteLogViewer += new Log.EventWriteLogViewer(OnWriteLogViewer);
                    history.IsLog = true;

                    if (_ImageHist_H == null)
                    {
                        _ImageHist_H = new double[tempImage.Height];
                    }
                    if (_ImageHist_W == null)
                    {
                        _ImageHist_W = new double[tempImage.Width];
                    }
                    spot.OpticalSpotBlobProcess(tempImage, _MenaulInspectLed1blobs, _workParams, Convert.ToInt32(rowInspectSpotThresholdH.Properties.Value), Convert.ToInt32(rowInspectSpotThresholdV.Properties.Value), Convert.ToInt32(Convert.ToSingle(rowInspectSpotBlobSizeMin.Properties.Value) / (_systemParams._cameraParams.OnePixelResolution)), Convert.ToInt32(Convert.ToSingle(rowInspectSpotBlobSizeMax.Properties.Value) / (_systemParams._cameraParams.OnePixelResolution)), ref _ImageHist_W, ref _ImageHist_H);
                    if (_MenaulInspectLed1blobs.Count == 1)
                    {
                        mResultData.mFirstLedSpot = _MenaulInspectLed1blobs[0];
                        mResultData.fOpticalEccentricity = (double)(Math.Sqrt(Math.Pow((_systemParams._cameraParams.HResolution / 2) - mResultData.mFirstLedSpot.CenterX, 2) + Math.Pow((_systemParams._cameraParams.VResolution / 2) - mResultData.mFirstLedSpot.CenterY, 2)) * _systemParams._cameraParams.OnePixelResolution);                        
                        mResultData.fOpticalSpotImageBright = (double)mResultData.mFirstLedSpot.PixelPeak;
                        mResultData.CalculateLedBlob(0);
                    }
                    tempImage = Utils.Clone<Bitmap>((Bitmap)Spot2BlobImage); ;
                    if (tempImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                    {
                        inspectsource = ConverterColorToGray(tempImage);
                        tempImage = inspectsource;                        
                    }
                    spot.OpticalSpotBlobProcess(tempImage, _MenaulInspectLed2blobs, _workParams, Convert.ToInt32(rowInspectSpotThresholdH.Properties.Value), Convert.ToInt32(rowInspectSpotThresholdV.Properties.Value), Convert.ToInt32(Convert.ToSingle(rowInspectSpotBlobSizeMin.Properties.Value) / (_systemParams._cameraParams.OnePixelResolution)), Convert.ToInt32(Convert.ToSingle(rowInspectSpotBlobSizeMax.Properties.Value) / (_systemParams._cameraParams.OnePixelResolution)), ref _ImageHist_W, ref _ImageHist_H);
                    if (_MenaulInspectLed2blobs.Count == 1)
                    {
                        mResultData.mFinalLedSpot = _MenaulInspectLed2blobs[0];                        
                        mResultData.CalculateLedBlob(1);
                    }
                    mResultData.WorkDistance = Convert.ToDouble(rowInspectSpotCameraDistance.Properties.Value);
                    mResultData.CalculateOpticalInspect((int)Enum.Parse(typeof(ModelType), rowInspectSpotProductType.Properties.Value.ToString()));
                    InpsectResultUpdate();
                    xtraTabControlMainSetup.SelectedTabPageIndex = 4;
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("투광 LED 발산각 측정 종료"));
                }
                else
                {
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("LED 발산각 검사 이미지가 로드되지 않았습니다"));
                }
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Error, LogClass.atPhoto.ToString(), string.Format("LED 발산각 검사 프로세싱이 실행되지 않았습니다."));
            }
        }

        private void repositoryItemButtonLedSpot2ImagePath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                if (openFileDialogSpot2Image.ShowDialog() == DialogResult.OK)
                {
                    rowInspectSpot2ImagePath.Properties.Value = openFileDialogSpot2Image.FileName;

                    rowInspectSpot2Image.Properties.Value = System.Drawing.Image.FromFile(openFileDialogSpot2Image.FileName);

                    Spot2BlobImage = System.Drawing.Image.FromFile(openFileDialogSpot2Image.FileName);
                    vGridControl5.Refresh();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("LED Spot1 Image 로드완료:{0}", openFileDialogSpot2Image.FileName));
                }
                if (Spot2BlobImage != null)
                {
                    _sourceImage = Spot2BlobImage;
                    pictureEditSystemImage.Image = _sourceImage;
                    pictureEditSystemImage.Refresh();
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private void repositoryItemButtonLedSpot1ImagePath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                if (openFileDialogSpot1Image.ShowDialog() == DialogResult.OK)
                {
                    rowInspectSpot1ImagePath.Properties.Value = openFileDialogSpot1Image.FileName;

                    rowInspectSpot1Image.Properties.Value = System.Drawing.Image.FromFile(openFileDialogSpot1Image.FileName);

                    Spot1BlobImage =  System.Drawing.Image.FromFile(openFileDialogSpot1Image.FileName);
                    vGridControl5.Refresh();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("LED Spot1 Image 로드완료:{0}", openFileDialogSpot1Image.FileName));
                }
                if (Spot1BlobImage != null)
                {
                    _sourceImage = Spot1BlobImage;
                    pictureEditSystemImage.Image = _sourceImage;
                    pictureEditSystemImage.Refresh();
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public void MenualInpsectResultUpdate()
        {
            string series = string.Empty;
            if (_workParams._ProductSeries == 0)                     // 0: BTS, 1: BTF, 2: BJ, 3: BEN
            {
                series = "BTS Series";
            }
            else if (_workParams._ProductSeries == 1)
            {
                series = "BTF Series";
            }
            else if (_workParams._ProductSeries == 2)
            {
                series = "BJ Series";
            }
            else if (_workParams._ProductSeries == 3)
            {
                series = "BEN Series";
            }
            else
                series = "BTS Series";
            pledSpotInspectionInfomation._InspectProductSeries = series;
            pledSpotInspectionInfomation._InspectProductModelName = _workParams._ProductModelName;
            pledSpotInspectionInfomation._InspectLedBlobHsize = mResultData.fOpticalSize[0];
            pledSpotInspectionInfomation._InspectLedBlobVsize = mResultData.fOpticalSize[1];
            pledSpotInspectionInfomation._InspectLedBlobBright = mResultData.fOpticalSpotImageBright;
            pledSpotInspectionInfomation._InspectLedOpticalEccentricity = mResultData.fOpticalEccentricity;
            pledSpotInspectionInfomation._InspectLedOpticalEmiterAngle = mResultData.fOpticalEmiterAngle * (180/Math.PI);
            pledSpotInspectionInfomation._InspectLedODFilterReduce = mResultData.fODFilterReduce;
            pledSpotInspectionInfomation._InspectLedND_FilterAngle = mResultData.fND_FilterAngle;
            pledSpotInspectionInfomation._InspectOperateMax_Distance = mResultData.fMaxOperateDistance;
            pledSpotInspectionInfomation._InspectOperateMin_Distance = mResultData.fMinOperateDistance;
            mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("수동 투광 LED 특성 검사 결과 , Spot1 Size :{0:000.000}mm, Spot2 Size :{1:000.000}mm, " +
                "이미지 밝기 :{2:000}pixel, 광원 편심 :{3:00.000}mm, 광 발산각 :{4:00.000}˚, 감쇄율 :{5:00.000}, ND필터 예측각도 :{6:000}˚ , 최대거리 ND필터 :{7:000}˚",
                mResultData.fOpticalSize[0], mResultData.fOpticalSize[1], mResultData.fOpticalSpotImageBright, mResultData.fOpticalEccentricity,(mResultData.fOpticalEmiterAngle * (180 / Math.PI)),
                mResultData.fODFilterReduce, mResultData.fND_FilterAngle, mResultData.fMaxOperateDistance));
        }
        public string GetRecipeNameToDB()
        {
            string retStr = string.Empty;
            string strsource = string.Empty;
            int strindex = 0, splitindex = 0;
            if (_JobWorkDbCtrl._rOrderDataList.Count == 1)
            {
                for (int i = 0; i < _JobWorkDbCtrl._rOrderDataList.Count; i++)
                {
                    if (_JobWorkDbCtrl._rOrderDataList[i].StatusType == 3)
                    {
                        strsource = _JobWorkDbCtrl._rOrderDataList[i].ItemName;
                        retStr = strsource;
                        //byte[] strbytes = new byte[strsource.Length];                        
                        //strbytes = Encoding.UTF8.GetBytes(strsource);
                        //for (int j = 0; j < strsource.Length; j++)
                        //{
                        //    if (strbytes[j] == '-')
                        //        strindex++;
                        //    if (strindex > 1)
                        //    {
                        //        splitindex = j;
                        //        break;
                        //    }                            
                        //}
                        //if (splitindex <= strsource.Length)
                        //    retStr = strsource.Substring(0, splitindex);
                        ////retStr = Encoding.UTF8.GetString(strbytes);

                    }
                }
                return retStr;
            }
            else
                return retStr;
        }

        private void barButtonItemHomming_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (!_bwMotionHome.IsBusy)
                {
                    _IsHommingFinished = false;
                    mRobotInformation.SetStatus(RobotInformation.RobotStatus.OperationReady, _IsHommingFinished);
                    _bwMotionHome.RunWorkerAsync(mRobotInformation);
                    AutoStartButtonLock();
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "Motion Homing 시작");
                    //if (_mMotionControlCommManager.IsOpen())
                    //{
                    //    if (MessageBox.Show("원점복귀를 진행을 합니다.", "원점복귀", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    //    {
                    //        byte[] SeData = new byte[8];
                    //        for (int i = 0; i < _mMotionControlCommManager.mDrvCtrl.DeviceIDCount; i++)
                    //        {
                    //            SeData = _mMotionControlCommManager.mDrvCtrl.HomeStartCommand((byte)_mMotionControlCommManager.mDrvCtrl.DrvID[i]);
                    //            _mMotionControlCommManager.SendData(SeData);
                    //        }
                    //        _IsHommingFinished = true;
                    //        mRobotInformation.SetStatus(RobotInformation.RobotStatus.OperationReady, _IsHommingFinished);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        private void barButtonItemReset_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_mMotionControlCommManager.IsOpen())
            {
                if (MessageBox.Show("알람 리셋을 진행을 합니다.", "알람 리셋", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    byte[] SeData = new byte[8];
                    for (int i = 0; i < _mMotionControlCommManager.mDrvCtrl.DeviceIDCount; i++)
                    {
                        SeData = _mMotionControlCommManager.mDrvCtrl.AlarmResetCommand((byte)_mMotionControlCommManager.mDrvCtrl.DrvID[i]);
                        _mMotionControlCommManager.SendData(SeData);
                    }
                    mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), "알람 리셋");
                }
            }
        }
        public void AutoStartButtonLock()
        {
            ribbonSystemPage.Enabled = false;
            ribbonPageGroupFile.Enabled = false;
            ribbonPageGroupImageFile.Enabled = false;
            ribbonPageGroupCamera.Enabled = false;
            ribbonPageGroupImageViewer.Enabled = false;
            ribbonPageGroupConnection.Enabled = false;
            //ribbonPageGroupMotionControl.Enabled = false;
            xtraTabControlMainSetup.Enabled = false;
        }
        public void AutoStartButtonRelease()
        {
            ribbonSystemPage.Enabled = true;
            ribbonPageGroupFile.Enabled = true;
            ribbonPageGroupImageFile.Enabled = true;
            ribbonPageGroupCamera.Enabled = true;
            ribbonPageGroupImageViewer.Enabled = true;
            ribbonPageGroupConnection.Enabled = true;
            //ribbonPageGroupMotionControl.Enabled = true;
            xtraTabControlMainSetup.Enabled = true;
        }
        private void FilterActuatorImageXFitSize()
        {
            if (_BaseXImage != null)
            {
                try
                {
                    float width = pictureEditActuatorX.ClientSize.Width * 100.0f / _BaseXImage.Width;
                    float height = (pictureEditActuatorX.ClientSize.Height - pictureEditActuatorX.ClientSize.Height * 0.01f) * 100.0f / _BaseXImage.Height;

                    float i = Math.Min(100.0f, Math.Min(width, height));

                    pictureEditActuatorX.Properties.ZoomPercent = i;
                    pictureEditActuatorX.HScrollBar.Value = 0;
                    pictureEditActuatorX.VScrollBar.Value = 0;
                    pictureEditActuatorX.Refresh();                    
                    //mLog.WriteLog(LogLevel.Info, LogClass.atPhoto.ToString(), string.Format("원본 Actuator 화면 맞춤: {0:0.0}%", pictureEditActuator.Properties.ZoomPercent));
                }
                catch (Exception)
                {
                    ;
                }
            }
        }
        private void FilterActuatorImageYFitSize()
        {
            if (_BaseYImage != null)
            {
                try
                {
                    float width = pictureEditActuatorY.ClientSize.Width * 100.0f / _BaseYImage.Width;
                    float height = (pictureEditActuatorY.ClientSize.Height - pictureEditActuatorY.ClientSize.Height * 0.01f) * 100.0f / _BaseYImage.Height;

                    float i = Math.Min(100.0f, Math.Min(width, height));

                    pictureEditActuatorY.Properties.ZoomPercent = i;
                    pictureEditActuatorY.HScrollBar.Value = 0;
                    pictureEditActuatorY.VScrollBar.Value = 0;
                    pictureEditActuatorY.Refresh();
                }
                catch (Exception)
                {
                    ;
                }
            }
        }
        private void FilterActuatorImageZFitSize()
        {
            if (_BaseZImage != null)
            {
                try
                {
                    float width = pictureEditActuatorZ.ClientSize.Width * 100.0f / _BaseZImage.Width;
                    float height = (pictureEditActuatorZ.ClientSize.Height - pictureEditActuatorZ.ClientSize.Height * 0.01f) * 100.0f / _BaseZImage.Height;

                    float i = Math.Min(100.0f, Math.Min(width, height));

                    pictureEditActuatorZ.Properties.ZoomPercent = i;
                    pictureEditActuatorZ.HScrollBar.Value = 0;
                    pictureEditActuatorZ.VScrollBar.Value = 0;
                    pictureEditActuatorZ.Refresh();
                }
                catch (Exception)
                {
                    ;
                }
            }
        }
        public Image ActuatorImageAnimationX(float pos)
        {
            Image tempimage = new Bitmap(_BaseXImage);
            Image tempact = new Bitmap(_ActuatorXImage);

            PointF ImagePos = new PointF(((750 - (tempact.Width / 2)) - (float)((pos - 13.4) * (700F / 773F))), 0);
            Graphics gp = Graphics.FromImage(tempimage);
            gp.DrawImage(tempact, ImagePos);
            gp.Save();
            gp.Dispose();
            return tempimage;
        }
        public Image ActuatorImageAnimationY(float pos)
        {
            Image tempimage = new Bitmap(_BaseYImage);
            Image tempact = new Bitmap(_ActuatorYImage);

            PointF ImagePos = new PointF(((360 - (tempact.Width / 2)) - (float)((pos - 0) * 195F / 50F)), 0);
            Graphics gp = Graphics.FromImage(tempimage);
            gp.DrawImage(tempact, ImagePos);
            gp.Save();
            gp.Dispose();
            return tempimage;
        }
        public Image ActuatorImageAnimationZ(float pos)
        {
            Image tempimage = new Bitmap(_BaseZImage);
            Image tempact = new Bitmap(_ActuatorZImage);

            PointF ImagePos = new PointF(0, ((440 - (tempact.Height / 2)) + (float)((pos - 0) * (190F / 52F))));
            Graphics gp = Graphics.FromImage(tempimage);
            gp.DrawImage(tempact, ImagePos);
            gp.Save();
            gp.Dispose();
            return tempimage;
        }
        private void UpdateGUI()
        {
            if (pictureEditActuatorX.InvokeRequired)
            {
                pictureEditActuatorX.Invoke(new MethodInvoker(delegate { pictureEditActuatorX.Image = ActuatorImageAnimationX((float)mRobotInformation.PositionX); FilterActuatorImageXFitSize(); }));
            }
            if (pictureEditActuatorY.InvokeRequired)
            {
                pictureEditActuatorY.Invoke(new MethodInvoker(delegate { pictureEditActuatorY.Image = ActuatorImageAnimationY((float)mRobotInformation.PositionY); FilterActuatorImageYFitSize(); }));
            }
            if (pictureEditActuatorZ.InvokeRequired)
            {
                pictureEditActuatorZ.Invoke(new MethodInvoker(delegate { pictureEditActuatorZ.Image = ActuatorImageAnimationZ((float)mRobotInformation.PositionZ); FilterActuatorImageZFitSize(); }));
            }
        }
        private void pictureEditActuatorX_Paint(object sender, PaintEventArgs e)
        {
            if (pictureEditActuatorX.Image != null)
            {
                float fScale = (float)(pictureEditActuatorX.Properties.ZoomPercent / 100.0f);
                Graphics gp = e.Graphics;
                string strpos = string.Empty;
                strpos = "Position X :" + string.Format("{0:000.00}", mRobotInformation.PositionX);
                //gp.DrawString(strpos, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(0, (pictureEditActuatorX.Image.Height - 30) * fScale));
                gp.DrawString(strpos, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(0, 10));
            }
        }

        private void pictureEditpictureEditActuatorY_Paint(object sender, PaintEventArgs e)
        {
            if (pictureEditActuatorY.Image != null)
            {
                float fScale = (float)(pictureEditActuatorY.Properties.ZoomPercent / 100.0f);
                Graphics gp = e.Graphics;
                string strpos = string.Empty;
                strpos = "Position Y :" + string.Format("{0:000.00}", mRobotInformation.PositionY);
                gp.DrawString(strpos, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(0, (pictureEditActuatorY.Image.Height - 30) * fScale));
            }
        }

        private void pictureEditActuatorZ_Paint(object sender, PaintEventArgs e)
        {
            if (pictureEditActuatorZ.Image != null)
            {
                float fScale = (float)(pictureEditActuatorZ.Properties.ZoomPercent / 100.0f);
                Graphics gp = e.Graphics;
                string strpos = string.Empty;
                strpos = "Position Z :" + string.Format("{0:000.00}", mRobotInformation.PositionZ);
                gp.DrawString(strpos, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), new PointF(0, (pictureEditActuatorZ.Image.Height - 30) * fScale));
            }
        }

        private void pictureEditDecenterResult_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.Bicubic;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pictureEditDecenterResult.ClientSize.Width;
            int height = pictureEditDecenterResult.ClientSize.Height;
            int passCount = 0;
            int failCount = 0;
            
            PointF fptCenter = new PointF(width / 2, height / 2);
            float fScale = height / 100f;
            float onePixelResolution = 0.1F;        // 100 pixel per 10 mm
            float threshold = 5.0F;
            float foptincalposy = 0.0f;
            float foptincalposz = 0.0f;

            Pen pen = new Pen(Brushes.White, 1f);
            pen.DashStyle = DashStyle.Dash;

            // 중심선 라인 그리기
            e.Graphics.DrawLine(pen, new Point(0, height / 2), new Point(width, height / 2));
            e.Graphics.DrawLine(pen, new Point(width / 2, 0), new Point(width / 2, height));

            // 중심에서 5mm 원 그리기
            e.Graphics.DrawEllipse(pen, (fptCenter.X - (threshold / onePixelResolution * fScale)), (fptCenter.Y - (threshold / onePixelResolution * fScale)), threshold * 2f / onePixelResolution * fScale, threshold * 2f / onePixelResolution * fScale);
            //e.Graphics.DrawEllipse(pen, (fptCenter.X), (fptCenter.Y), threshold * 2f / onePixelResolution * fScale, threshold * 2f / onePixelResolution * fScale);
            if (_InspectionResult)
            {
                for (int i = 0; i < _workParams.InspectionPositions.Count; i++)
                {
                    if (_workParams.InspectionPositions[i].ePositionType == INSPECTION_POSITION_MODE.POSITION_OPTICAL_SPOT_MODE)
                    {
                        PointF fDiff = new PointF( (((float)mResultData.fDecenterY - _workParams.InspectionPositions[i].PositionY)) * fScale / onePixelResolution,
                                                  (((float)mResultData.fDecenterZ - _workParams.InspectionPositions[i].PositionZ)) * fScale / onePixelResolution);
                        Pen greenPen = new Pen(Brushes.LightGreen, 2f );

                        e.Graphics.DrawLine(greenPen, new PointF(fptCenter.X + fDiff.X - 10, fptCenter.Y + fDiff.Y), new PointF(fptCenter.X + fDiff.X + 10, fptCenter.Y + fDiff.Y));
                        e.Graphics.DrawLine(greenPen, new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y - 10), new PointF(fptCenter.X + fDiff.X, fptCenter.Y + fDiff.Y + 10));
                        foptincalposy = _workParams.InspectionPositions[i].PositionY;
                        foptincalposz = _workParams.InspectionPositions[i].PositionZ;
                    }
                }
                e.Graphics.DrawString(string.Format("Optical Position Offset: {0:00.000},{1:00.000}", mResultData.fDecenterY - foptincalposy, mResultData.fDecenterZ - foptincalposz),
                    new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Underline),
                    Brushes.LightGreen, new PointF(10, height - 20));
            }

            /*
            e.Graphics.DrawString(string.Format("Pass:{0:00}, Fail:{1:00}", passCount, failCount),
                new Font(FontFamily.GenericSansSerif, 15f, FontStyle.Underline),
                Brushes.LightGreen, new PointF(10, 10));
            */
        }
        private void InitailProgramFormLanguage()
        {
            if (!_systemParams._SystemLanguageKoreaUse)
            {
                ribbonPageEquipementFunctions.Text = "Function";

                ribbonSystemPage.Text = "Set System";
                barButtonItemSystemFolderPathSetting.Caption = "Set Path";
                barButtonItemSystemEditor.Caption = "Set Parameter";
                barButtonItemWorkInfo.Caption = "Login Information";

                ribbonPageGroupFile.Text = "Recipe";
                barButtonItemRecipeOpen.Caption = "Load";
                barButtonItemRecipeEditorOpen.Caption = "Recipe Editor";
                barListItemRecipeOpen.Caption = "Open";

                barButtonItemImageOpen.Caption = "Open";
                barButtonItemImageSave.Caption = "Save";
                barCheckItemImageCrop.Caption = "Image Crop";
                barButtonItemCameraListRefresh.Caption = "Search Camera";
                barButtonItemSingleShot.Caption = "Single";
                barButtonItemContinueousShot.Caption = "Contiueous";
                barButtonItemCameraStop.Caption = "Stop";
                barButtonItemImageZoomIn.Caption = "ZoomIn";
                barButtonItemImageZoomOut.Caption = "ZoomOut";
                barButtonItemImageOriginSize.Caption = "OriginSize";
                barButtonItemFitSize.Caption = "FitSize";
                barCheckItemShowCenterMark.Caption = "CenterMark";

                ribbonPageGroupCamera.Text = "Camera";
                ribbonPageGroupImageViewer.Text = "ImageView";

                xtraTabPageCamera.Text = "Set Camera";
                categoryCamera.Properties.Caption = "Camera";
                rowCameraConnect.Properties.Caption = "Connect";
                rowCameraName.Properties.Caption = "Camera Name";
                rowCameraHResolution.Properties.Caption = "Horizon Pixel[pixel]";
                rowCameraVResolution.Properties.Caption = "Vertical Pixel[pixel]";
                rowCameraFrame.Properties.Caption = "Frame[fps]";
                rowCameraExposureTime.Properties.Caption = "Exposure Time[us]";
                rowCameraGain.Properties.Caption = "Gain";
                xtraTabPageImageProcess.Text = "Image Process";
                MenualOpticalInspectButton.Text = "Optical Inspection";
                categorySpotInpsect.Properties.Caption = "Optical Inspection";
                rowInspectSpotProductType.Properties.Caption = "Product Type";
                rowInspectSpotProductDistance.Properties.Caption = "Product Distance[mm]";
                rowInspectSpotInspectDistance.Properties.Caption = "Inspection Distance[mm]";
                rowInspectSpotCameraDistance.Properties.Caption = "Camera Moving Distance[mm]";
                rowInspectSpot1ImagePath.Properties.Caption = "Spot1 Image Path";
                rowInspectSpot1Image.Properties.Caption = "Spot1 Image";
                rowInspectSpot2ImagePath.Properties.Caption = "Spot2 Image Path";
                rowInspectSpot2Image.Properties.Caption = "Spot2 Image";
                rowInspectSpotThresholdH.Properties.Caption = "Horizon Threshold[0~255]";
                rowInspectSpotThresholdV.Properties.Caption = "Vertical Threshold[0~255]";
                rowInspectSpotBlobSizeMin.Properties.Caption = "Min Spot Size";
                rowInspectSpotBlobSizeMax.Properties.Caption = "Max Spot Size";
                rowInspectAlignmentDistance.Properties.Caption = "Optical Eccentricity Value[mm]";
                rowInspectDivergenceAngle.Properties.Caption = "DivergenceAngle Value[˚]";
                menualPatternMatchingButton.Text = "Pattern Matching";
                MenualInspectButton.Text = "Optical Inspection";
                categoryPatternMaching.Properties.Caption = "Pattern Matching";
                rowTempletePath.Properties.Caption = "Templete Path";
                rowTempleteImage.Properties.Caption = "Templete Image";
                rowrThresholdValue.Properties.Caption = "Threshold";
                rowResultPosition.Properties.Caption = "Result Position";
                categoryRecipe.Properties.Caption = "Optical Inspection";
                rowSpotImagePath.Properties.Caption = "Spot Image Path";
                rowSpotImage.Properties.Caption = "Spot Image";
                rowThresholdH.Properties.Caption = "Threshold Horizon[0~255]";
                rowThresholdV.Properties.Caption = "Threshold Vertical[0~255]";
                rowSpotBlobHSizeMin.Properties.Caption = "Min Spot Size[mm]";
                rowSpotBlobHSizeMax.Properties.Caption = "Max Spot Size[mm]";
                rowAlignmentDistance.Properties.Caption = "Optical Eccentricity Value[mm]";
                rowDivergenceAngle.Properties.Caption = "DivergenceAngle Value[˚]";

                ribbonPageGroupConnection.Text = "Communication";
                barButtonItemConnectAll.Caption = "Connection All";
                barButtonItemConnectionAiC.Caption = "AiC";
                barButtonItemConnectionRemeteIO.Caption = "Remote I/O";

                ribbonPageGroupMotionControl.Text = "Motion Control";
                barButtonItemHomming.Caption = "Homming";
                barButtonItemReset.Caption = "Alram Reset";

                ribbonPageGroupInspection.Text = "Inspection And Result";
                barCheckItemInspectionStart.Caption = "Start";
                barStaticItemInspectionStatus.Caption = "Status";
                barStaticItemInspectionTime.Caption = "Time:";
                barEditItemInspectionResult.EditValue = "Ready";
                barEditItemTotalInspectionCount.EditValue = "Total Count: 00000";
                barEditItemTotalFailCount.EditValue = "Fail Count: 00000";
                barButtonItemInitializeStatistics.Caption = "Initial Chart";

                dockPanelLogView.Text = "Log";
                gridColumn1.Caption = "Level";
                gridColumn2.Caption = "Time";
                gridColumn3.Caption = "Path";
                gridColumn4.Caption = "Message";

                dockPanelMainSetting.Text = "Setting";
                xtraTabPageMotionControl.Text = "Motion Control";
                xtraTabPageRemoteIO.Text = "Remote I/O";
                xtraTabPageInspectResult.Text = "Inspection Result";
                xtraTabPageStatistics.Text = "Chart";
            }
            else
            {
                ribbonPageEquipementFunctions.Text = "기능설정";

                ribbonSystemPage.Text = "시스템 설정";
                barButtonItemSystemFolderPathSetting.Caption = "경로설정";
                barButtonItemSystemEditor.Caption = "시스템 설정";
                barButtonItemWorkInfo.Caption = "로그인 정보";

                ribbonPageGroupFile.Text = "레시피";
                barButtonItemRecipeOpen.Caption = "불러오기";
                barButtonItemRecipeEditorOpen.Caption = "레시피 편집기";
                barListItemRecipeOpen.Caption = "레시피 선택";

                barButtonItemImageOpen.Caption = "불러오기";
                barButtonItemImageSave.Caption = "저장하기";
                barCheckItemImageCrop.Caption = "템플릿 자르기";
                barButtonItemCameraListRefresh.Caption = "카메라검색";
                barButtonItemSingleShot.Caption = "싱글샷";
                barButtonItemContinueousShot.Caption = "연속샷";
                barButtonItemCameraStop.Caption = "정지";
                barButtonItemImageZoomIn.Caption = "확대";
                barButtonItemImageZoomOut.Caption = "축소";
                barButtonItemImageOriginSize.Caption = "원래크기";
                barButtonItemFitSize.Caption = "화면 맞춤";
                barCheckItemShowCenterMark.Caption = "중심선 보기";

                ribbonPageGroupCamera.Text = "카메라";
                ribbonPageGroupImageViewer.Text = "화면보기";

                xtraTabPageCamera.Text = "카메라 설정";
                categoryCamera.Properties.Caption = "카메라";
                rowCameraConnect.Properties.Caption = "카메라 연결";
                rowCameraName.Properties.Caption = "카메라 이름";
                rowCameraHResolution.Properties.Caption = "카메라 가로해상도[pixel]";
                rowCameraVResolution.Properties.Caption = "카메라 세로해상도[pixel]";
                rowCameraFrame.Properties.Caption = "카메라 프레임[fps]";
                rowCameraExposureTime.Properties.Caption = "카메라 노출시간[us]";
                rowCameraGain.Properties.Caption = "카메라 게인";
                xtraTabPageImageProcess.Text = "이미지 처리";
                MenualOpticalInspectButton.Text = "투광 발산각 계산";
                categorySpotInpsect.Properties.Caption = "투광 검사";
                rowInspectSpotProductType.Properties.Caption = "투광 제품 타입";
                rowInspectSpotProductDistance.Properties.Caption = "제품 정격 거리[mm]";
                rowInspectSpotInspectDistance.Properties.Caption = "단축 검사 거리[mm]";
                rowInspectSpotCameraDistance.Properties.Caption = "카메라 이동거리[mm]";
                rowInspectSpot1ImagePath.Properties.Caption = "광원1 이미지 경로";
                rowInspectSpot1Image.Properties.Caption = "광원1 이미지";
                rowInspectSpot2ImagePath.Properties.Caption = "광원2 이미지 경로";
                rowInspectSpot2Image.Properties.Caption = "광원2 이미지";
                rowInspectSpotThresholdH.Properties.Caption = "수평 인식 임계값[0~255]";
                rowInspectSpotThresholdV.Properties.Caption = "수직 인식 임계값[0~255]";
                rowInspectSpotBlobSizeMin.Properties.Caption = "광원 최소 크기";
                rowInspectSpotBlobSizeMax.Properties.Caption = "광원 최대 크기";
                rowInspectAlignmentDistance.Properties.Caption = "편심 합격거리[mm]";
                rowInspectDivergenceAngle.Properties.Caption = "발산각 합격 각도";
                menualPatternMatchingButton.Text = "패턴 매칭";
                MenualInspectButton.Text = "광특성 검사";
                categoryPatternMaching.Properties.Caption = "패턴 매칭";
                rowTempletePath.Properties.Caption = "템플릿 경로";
                rowTempleteImage.Properties.Caption = "템플릿 이미지";
                rowrThresholdValue.Properties.Caption = "매칭 임계값";
                rowResultPosition.Properties.Caption = "결과 중심점";
                categoryRecipe.Properties.Caption = "광 특성 검사";
                rowSpotImagePath.Properties.Caption = "광원 이미지경로";
                rowSpotImage.Properties.Caption = "광원 이미지";
                rowThresholdH.Properties.Caption = "수평 인식 임계값[0~255]";
                rowThresholdV.Properties.Caption = "수직 인식 임계값[0~255]";
                rowSpotBlobHSizeMin.Properties.Caption = "광원 최소크기[mm]";
                rowSpotBlobHSizeMax.Properties.Caption = "광원 최대크기[mm]";
                rowAlignmentDistance.Properties.Caption = "편심 합격거리[mm]";
                rowDivergenceAngle.Properties.Caption = "발산각 합격각도[˚]";

                ribbonPageGroupConnection.Text = "통신 연결";
                barButtonItemConnectAll.Caption = "전체 연결";
                barButtonItemConnectionAiC.Caption = "AiC";
                barButtonItemConnectionRemeteIO.Caption = "Remote I/O";

                ribbonPageGroupMotionControl.Text = "모션 제어";
                barButtonItemHomming.Caption = "원점 복귀";
                barButtonItemReset.Caption = "알람 리셋";

                ribbonPageGroupInspection.Text = "검사 및 결과";
                barCheckItemInspectionStart.Caption = "검사 시작";
                barStaticItemInspectionStatus.Caption = "진행";
                barStaticItemInspectionTime.Caption = "검사 시간:";
                barEditItemInspectionResult.EditValue = "Ready";
                barEditItemTotalInspectionCount.EditValue = "총 검사 수: 00000";
                barEditItemTotalFailCount.EditValue = "불합격 수: 00000";
                barButtonItemInitializeStatistics.Caption = "통계 초기화";

                dockPanelLogView.Text = "로그";
                gridColumn1.Caption = "레벨";
                gridColumn2.Caption = "시간";
                gridColumn3.Caption = "위치";
                gridColumn4.Caption = "메세지";

                dockPanelMainSetting.Text = "주요 설정";
                xtraTabPageMotionControl.Text = "모션 제어";
                xtraTabPageRemoteIO.Text = "리모트 I/O";
                xtraTabPageInspectResult.Text = "검사 결과";
                xtraTabPageStatistics.Text = "통계";
            }
        }

    }
}