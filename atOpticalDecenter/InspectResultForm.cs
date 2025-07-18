﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PhotoProduct;
namespace atOpticalDecenter
{
    public partial class InspectResultForm : Form
    {
        public InspectResultForm()
        {
            InitializeComponent();
        }
        public InspectResultForm(string series, string model, InspectResultData result, bool usingkorealanguage)
        {
            InitializeComponent();
            if (!usingkorealanguage)
            {
                categoryInspectionResult.Properties.Caption = "Inspection Result";
                rowProductSeries.Properties.Caption = "Product Series";
                rowProductModelName.Properties.Caption = "Product Model Name";
                rowLedSpot1BlobSize.Properties.Caption = "Spot 1 Size[mm]";
                rowLedSpot2BlobSize.Properties.Caption = "Spot 2 Size[mm]";
                categoryOpticalEccentricAngle.Properties.Caption = "Spot Eccentric Angle";
                rowOpticalEccentricAngle.Properties.Caption = "Eccentric Angle[˚]";
                rowOpticalEccentricHorizonAngle.Properties.Caption = "Eccentric Horizon Angle[˚]";
                rowOpticalEccentricVirticalAngle.Properties.Caption = "Eccentric Virtical Angle[˚]";
                rowInspectResult.Properties.Caption = "Inspection Result";
            }
            else
            {
                categoryInspectionResult.Properties.Caption = "투광 검사 결과";
                rowProductSeries.Properties.Caption = "제품 시리즈";
                rowProductModelName.Properties.Caption = "제품 모델명";
                rowLedSpot1BlobSize.Properties.Caption = "Spot 1 크기[mm]";
                rowLedSpot2BlobSize.Properties.Caption = "Spot 2 크기[mm]";
                categoryOpticalEccentricAngle.Properties.Caption = "광 편심각도";
                rowOpticalEccentricAngle.Properties.Caption = "편심 각도[˚]";
                rowOpticalEccentricHorizonAngle.Properties.Caption = "편심 수평 각도[˚]";
                rowOpticalEccentricVirticalAngle.Properties.Caption = "편심 수직 각도[˚]";
                rowInspectResult.Properties.Caption = "투광 검사 결과";
            }
            rowProductSeries.Properties.Value = series;
            rowProductModelName.Properties.Value = model;
            rowLedSpot1BlobSize.Properties.Value = Math.Round(result.fOpticalSize[0], 3);
            rowLedSpot2BlobSize.Properties.Value = Math.Round(result.fOpticalSize[1], 3);
            rowOpticalEccentricAngle.Properties.Value = Math.Round(result.fOpticalEccentricAngle, 3);
            rowOpticalEccentricHorizonAngle.Properties.Value = Math.Round(result.fOpticalEccentricAngle_H, 3);
            rowOpticalEccentricVirticalAngle.Properties.Value = Math.Round(result.fOpticalEccentricAngle_V, 3);
            if (result.bTotalResult)
                rowInspectResult.Properties.Value = "Pass";
            else
                rowInspectResult.Properties.Value = "Fail";
        }
    }
}
