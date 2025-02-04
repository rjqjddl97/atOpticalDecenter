using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomPages
{
    public partial class LedSpotInspectionInfomation : UserControl
    {
        public double _InspectLedBlobHsize { get { return Convert.ToDouble(rowLedSpotBlobInspectHSize.Properties.Value); } set { rowLedSpotBlobInspectHSize.Properties.Value = value; } }
        public double _InspectLedBlobVsize { get { return Convert.ToDouble(rowLedSpotBlobInspectVSize.Properties.Value); } set { rowLedSpotBlobInspectVSize.Properties.Value = value; } }

        public string _InspectProductModelName { get { return rowProductModelName.Properties.Value.ToString(); } set { rowProductModelName.Properties.Value = value; } }
        public string _InspectProductSeries { get { return rowProductSeries.Properties.Value.ToString(); } set { rowProductSeries.Properties.Value = value; } }
        public double _InspectLedBlobBright { get { return Convert.ToDouble(rowLedSpotImageBright.Properties.Value); } set { rowLedSpotImageBright.Properties.Value = value; } }
        public double _InspectLedOpticalEccentricity { get { return Convert.ToDouble(rowAlignmentDistance.Properties.Value); } set { rowAlignmentDistance.Properties.Value = value; } }
        public double _InspectLedOpticalEmiterAngle { get { return Convert.ToDouble(rowDivergenceAngle.Properties.Value); } set { rowDivergenceAngle.Properties.Value = value; } }
        public double _InspectLedODFilterReduce { get { return Convert.ToDouble(rowNDReduceRatio.Properties.Value); } set { rowNDReduceRatio.Properties.Value = value; } }
        public double _InspectLedND_FilterAngle { get { return Convert.ToDouble(rowNDFilterAngle.Properties.Value); } set { rowNDFilterAngle.Properties.Value = value; } }
        public double _InspectOperateMax_Distance { get { return Convert.ToDouble(rowOperateMaxDistance.Properties.Value); } set { rowOperateMaxDistance.Properties.Value = value; } }
        public double _InspectOperateMin_Distance { get { return Convert.ToDouble(rowOperateMinDistance.Properties.Value); } set { rowOperateMinDistance.Properties.Value = value; } }
        public bool _InspectOpticalResult { get { return Convert.ToBoolean(rowInspectResult.Properties.Value); } set { rowInspectResult.Properties.Value = value; } }
        public LedSpotInspectionInfomation()
        {
            InitializeComponent();
        }
    }
}
