using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageLibrary;
using RecipeManager;

namespace PhotoProduct
{
    public class InspectResultData
    {
        public const double fPsiValue = 0.017505828;         // ψ(psi) 상수값
        public const double fRhoValue_Transmitter = 0.1;     // ρ(rho) , 반사율값
        public const double fRhoValue_Reflector = 0.9;     // ρ(rho) , 반사율값
        public const double fPLValue = 1.0;                  // PL, 광 세기 값
        public const double fDrValue = 0.04;                 // 투광LED 지름값[cm]
        public const double fEtaValue = 0.5;                 // η(eta), 수광부 효율값 
        public const double RADIAN_TO_DEGREE = 180 / 3.141592;
        public const double DEGREE_TO_RADIAN = 3.141592 / 180;
        public const double BASE_FILTER_ANGLE = 0;
        public const double ND_FILTER_RANGE = 310;
        public const double ND_FILTER_ZERO_ANNGLE = 50;
        public const double ND_FILTER_REDUCE_RATE = 4.0;
        public const double TA_RANGE_DISTANCE = 100000.0;      // 100M

        public double fPixelResolution = 0;
        public Blob mFirstLedSpot = new Blob();
        public Blob mFinalLedSpot = new Blob();
        public double[] _ImageHist_H = null;
        public double[] _ImageHist_V = null;
        public double fOpticalEccentricity = 0;             // 
        public double fOpticalEmiterAngle = 0;              // 투광 발산각
        public double fND_FilterAngle = 0;                  // ND 필터 각도
        public double fImageSensorSize_H = 0;
        public double fImageSensorSize_V = 0;
        public double fLensFocusLength = 0;
        public double fImageFOV = 0;
        public double fFixelResolution = 0;
        public double fCameraWorkDistance = 150;
        public double fRealOpticDistance = 1000;
        public double fShortOpticDistance = 500;
        public double fODFilterReduce = 0;
        public double fMinOperateDistance = 0;
        public double fMaxOperateDistance = 0;
        public double fOpticalSpotImageBright = 0;
        public double fOpticalTA = 0;
        public double fGaussianDistribution = 0;
        public double fAlbedo = 0;

        public double fOpticalCenterPositionX = 0;
        public double fOpticalCenterPositionY = 0;
        public double fOpticalCenterPositionZ = 0;

        public double fMeasureP1X = 0;
        public double fMeasureP1Y = 0;
        public double fMeasureP1Z = 0;

        public double fMeasureP2X = 0;
        public double fMeasureP2Y = 0;
        public double fMeasureP2Z = 0;

        public double fLineVectorX = 0;
        public double fLineVectorY = 0;
        public double fLineVectorZ = 0;

        public double fDecenterX = 0;
        public double fDecenterY = 0;
        public double fDecenterZ = 0;


        public double[] fOpticalAngle = new double[2];
        public double[] fOpticalSize = new double[2];
        public double[] fOpticalReducttionQuantity = new double[2];     // PD에 도달한 수광량
        public double[] OD_Define = new double[Convert.ToInt32(ND_FILTER_RANGE)];

        public bool bTotalResult = false;
        public bool bOpticalInspect = false;
        public bool bCalibration_White = false;
        public bool bCalibration_Black = false;
        public bool bCalibrationNomal = false;
        public bool bNomalOutputInspect = false;
        public bool bReversePowerInspect = false;
        public bool bOverCurrentInspect = false;
        public bool bOutputTransition_OutputInspect = false;
        public bool bOutputTransition_OverCurInspect = false;
        public bool bOutputTransition_ConsumerCurInspect = false;

        public event Action<InspectResultData> UpdatedDataEvent;

        public InspectResultData()
        {
            fOpticalSize[0] = 0;
            fOpticalSize[1] = 0;
            fOpticalSpotImageBright = 0;
            fOpticalEccentricity = 0;
            fOpticalEmiterAngle = 0;
            fODFilterReduce = 0;
            fND_FilterAngle = 0;
            fMaxOperateDistance = 0;
            fMinOperateDistance = 0;            
        }
        public double ImageResolution
        {
            get { return fPixelResolution; }
            set { fPixelResolution = value; }
        }
        public double WorkDistance
        {
            get { return fCameraWorkDistance; }
            set { fCameraWorkDistance = value; }
        }
        public double RealInspectDistance
        {
            get { return fRealOpticDistance; }
            set { fRealOpticDistance = value; }
        }
        public double ShortInspectDistance
        {
            get { return fShortOpticDistance; }
            set { fShortOpticDistance = value; }
        }
        public void UpdateData()
        {
            ;
        }
        public void ClearData()
        {

        }
        public void InspectParameterInitial(double fDistance, double fShortDistance, int Pixel_H, int Pixel_V, double fPixelPerUnit)
        {
            double f0 = ND_FILTER_REDUCE_RATE / ND_FILTER_RANGE;
            for (int i = 0; i < ND_FILTER_RANGE; i++)
            {
                if (i == 0)
                {
                    OD_Define[i] = f0;
                }
                else
                {
                    OD_Define[i] = f0 * (i + 1);
                }
            }
            RealInspectDistance = fDistance;
            ShortInspectDistance = fShortDistance;
            fPixelResolution = fPixelPerUnit;
            SetAngleFOV(Pixel_H, Pixel_V);
        }
        public void InspectInitialImageBuffer(int Pixel_H, int Pixel_V)
        {
            _ImageHist_H = new double[Pixel_H];
            _ImageHist_V = new double[Pixel_V];
        }
        public void SetAngleFOV(int Pixel_H, int Pixel_V)
        {
            fImageFOV = (Math.Atan((fImageSensorSize_H / 2f) / (fLensFocusLength)) * (RADIAN_TO_DEGREE)) / (Pixel_H / 2f);
        }
        public void CalculateLedBlob(int PointIndex)
        {
            if (PointIndex == 0)
            {
                //fOpticalAngle[PointIndex] = ((mFirstLedSpot.Width + mFirstLedSpot.Height) / 2f) * fImageFOV;                                // P1 빔발산각                
                fOpticalSize[PointIndex] = ((mFirstLedSpot.Width + mFirstLedSpot.Height) / 2f) * fPixelResolution;            // 빔 크기 계산.                
            }
            else
            {
                //fOpticalAngle[PointIndex] = ((mFinalLedSpot.Width + mFinalLedSpot.Height) / 2f) * fImageFOV;                                // P1 빔발산각                
                fOpticalSize[PointIndex] = ((mFinalLedSpot.Width + mFinalLedSpot.Height) / 2f) * fPixelResolution;            // 빔 크기 계산.
            }
            //fOpticalSize[PointIndex] = Math.Tan((fOpticalAngle[PointIndex] / 2f) * DEGREE_TO_RADIAN) * WorkDistance * 2f;            // 빔 크기 계산.
            
        }
        public void CalculateOpticalInspect(int iProductType)
        {
            // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
            //fOpticalEmiterAngle = Math.Atan(((fOpticalSize[1] - fOpticalSize[0]) / 2f) / WorkDistance) * 2f * RADIAN_TO_DEGREE;      // 발산각 계산.
            fOpticalEmiterAngle = Math.Atan(((fOpticalSize[1] - fOpticalSize[0]) / 2f) / WorkDistance) * 2f;      // 발산각 계산.
            CalcOpticalReduceQuantity(iProductType);
            if (iProductType == 6)
            {
                fODFilterReduce = 0;
                fND_FilterAngle = 0;
            }
            else
            {
                fODFilterReduce = -Math.Log10(fOpticalReducttionQuantity[0] / fOpticalReducttionQuantity[1]);
                fND_FilterAngle = FindODFilterAngle(fODFilterReduce, iProductType) + ND_FILTER_ZERO_ANNGLE;
            }
        }
        public bool CalculateOpticalDecenter()
        {
            if ((fMeasureP2X - fMeasureP1X) == 0) return false;
            //if ((fMeasureP2Y - fMeasureP1Y) == 0) return false;
            //if ((fMeasureP2Z - fMeasureP1Z) == 0) return false;

            fLineVectorX = (fMeasureP2X - fMeasureP1X);
            fLineVectorY = (fMeasureP2Y - fMeasureP1Y);
            fLineVectorZ = (fMeasureP2Z - fMeasureP1Z);
            //fOpticalEccentricity = 0;            
            fDecenterX = 0;
            fDecenterY = ((-fMeasureP1X / fLineVectorX) * fLineVectorY) + fMeasureP1Y;
            fDecenterZ = ((-fMeasureP1X / fLineVectorX) * fLineVectorZ) + fMeasureP1Z;


            fOpticalEccentricity = Math.Sqrt(Math.Pow((fDecenterY - fMeasureP1Y), 2) + Math.Pow((fDecenterZ - fMeasureP1Z), 2));
            return true;
        }
        public void CalcOpticalReduceQuantity(int producttype)
        {
            // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
            if (producttype == 0)
            {
                MirrorOpticalReduceQuantity();
            }
            else if ((producttype == 1) || (producttype == 2) || (producttype == 3) || (producttype == 4))
            {
                DiffuseOpticalReduceQuantity();
            }
            else if (producttype == 5)
            {
                TransmitterOpticalReduceQuantity();
            }
            else
            {
                fOpticalReducttionQuantity[0] = fOpticalReducttionQuantity[1] = 1f;
            }
        }
        public void MirrorOpticalReduceQuantity()
        {
            //Pr = PL * ρ(fRhoValue) * DR^2 * η(fEtaValue) * GaussianDistribution / (4 * ψ^2(fPsiValue) * r^2 )
            //fOpticalReducttionQuantity[0] = ((fPLValue * fEtaValue * (fDrValue * fDrValue)) / ((16 * fRealOpticDistance * fRealOpticDistance * fRealOpticDistance* fRealOpticDistance))) * (1 - Math.Exp((-2) / ((fOpticalEmiterAngle * fPsiValue) * (fOpticalEmiterAngle * fPsiValue))));
            //fOpticalReducttionQuantity[1] = ((fPLValue * fEtaValue * (fDrValue * fDrValue)) / ((16 * fShortOpticDistance * fShortOpticDistance * fShortOpticDistance * fShortOpticDistance))) * (1 - Math.Exp((-2) / ((fOpticalEmiterAngle * fPsiValue) * (fOpticalEmiterAngle * fPsiValue))));
            fGaussianDistribution = (1 - Math.Exp((-2) / (fOpticalEmiterAngle * fOpticalEmiterAngle)));
            fAlbedo = fRhoValue_Reflector;
            fOpticalReducttionQuantity[0] = ((fPLValue * fAlbedo * fEtaValue * (fDrValue * fDrValue)) / ((4 * fOpticalEmiterAngle * fOpticalEmiterAngle * fRealOpticDistance * fRealOpticDistance))) * fGaussianDistribution;
            fOpticalReducttionQuantity[1] = ((fPLValue * fAlbedo * fEtaValue * (fDrValue * fDrValue)) / ((4 * fOpticalEmiterAngle * fOpticalEmiterAngle * fShortOpticDistance * fShortOpticDistance))) * fGaussianDistribution;
        }
        public void TransmitterOpticalReduceQuantity()
        {
            //Pr = PL * Ta * DR^2 * η(fEtaValue) * GaussianDistribution / (ψ^2(fPsiValue) * r^2 )
            fOpticalTA = Math.Exp(-1*0.1*(fRealOpticDistance / TA_RANGE_DISTANCE));
            fAlbedo = fRhoValue_Transmitter;
            fGaussianDistribution = (1 - Math.Exp((-2) / (fOpticalEmiterAngle * fOpticalEmiterAngle)));            
            fOpticalReducttionQuantity[0] = ((fPLValue * fOpticalTA * fEtaValue * (fDrValue * fDrValue)) / ((fOpticalEmiterAngle * fOpticalEmiterAngle) * (fRealOpticDistance * fRealOpticDistance))) * fGaussianDistribution;
            fOpticalReducttionQuantity[1] = ((fPLValue * fOpticalTA * fEtaValue * (fDrValue * fDrValue)) / ((fOpticalEmiterAngle * fOpticalEmiterAngle) * (fShortOpticDistance * fShortOpticDistance))) * fGaussianDistribution;
        }
        public void DiffuseOpticalReduceQuantity()
        {
            //Pr = 3 * PL * ρ(fRhoValue) * DR^2 * η(fEtaValue) * GaussianDistribution * cosθ / (2 * r^3 )            
            fGaussianDistribution = (1 - Math.Exp((-2) / (fOpticalEmiterAngle * fOpticalEmiterAngle)));
            fAlbedo = fRhoValue_Reflector;
            fOpticalReducttionQuantity[0] = ((3 * fAlbedo * fEtaValue * (fDrValue * fDrValue) * fPLValue) / (2 * (fRealOpticDistance * fRealOpticDistance * fRealOpticDistance))) * fGaussianDistribution;
            fOpticalReducttionQuantity[1] = ((3 * fAlbedo * fEtaValue * (fDrValue * fDrValue) * fPLValue) / (2 * (fShortOpticDistance * fShortOpticDistance * fShortOpticDistance))) * fGaussianDistribution;
        }
        public double FindODFilterAngle(double OpticalReduce, int prodtype)
        {
            // 0: 미러반사형, 1: 한정거리반사, 2: 확산반사, 3: BGS반사, 4: 협시계반사, 5: 투광, 6: 수광
            int i,index = 0;
/*
            if ((prodtype == 0) || (prodtype == 1) || (prodtype == 2) || (prodtype == 3) || (prodtype == 4))
            {
                OpticalReduce = OpticalReduce / 2f;
            }
*/
            //OpticalReduce = OpticalReduce / 2f;
            for (i = 0; i < ND_FILTER_RANGE; i++)
            {
                if (OpticalReduce < OD_Define[i])
                {
                    index = i;
                    break;
                }
            }
            return (index + BASE_FILTER_ANGLE + 1);
        }
    }
}
