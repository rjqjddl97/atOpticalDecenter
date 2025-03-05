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
using DevExpress.XtraCharts;
using RecipeManager;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using CustomPages;
using Basler;
using LogLibrary;
using ImageLibrary;
using PhotoProduct;
using atOpticalDecenter;
using atOpticalDecenter.Functions.StepHandler;

namespace atOpticalDecenter
{
    public partial class atOpticalDecenter
    {
        DataTable _dtOpticalInspect = new DataTable();       

        public bool _bRadarChartUpdateEnable { get; set; } = false;
        private void InitializeChartOpticalInspect()
        {
            try
            {
                //InitializeChartInsptecResult();
                //InitializeChartDistance();
                InitializeChartAngle();
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), ex.Message);
            }
        }
        private void UpdateChartInspectionAngle(float angle)
        {
            int index = 0;
            if ((angle >= -1f) && (angle <= 1f))
                index = 0;
            else if ((angle >= -2f) && (angle <= 2f))
                index = 1;
            else if ((angle >= -3f) && (angle <= 3f))
                index = 2;
            else if ((angle >= -4f) && (angle <= 4f))
                index = 3;
            else if ((angle >= -5f) && (angle <= 5f))
                index = 4;
            else if ((angle >= -6f) && (angle <= 6f))
                index = 5;
            else if ((angle >= -7f) && (angle <= 7f))
                index = 6;
            else if ((angle >= -8f) && (angle <= 8f))
                index = 7;
            else if ((angle >= -9f) && (angle <= 9f))
                index = 8;
            else
                index = 9;

            int count = Convert.ToInt32(_statistics.Statistics[index]) + 1;
            _statistics.Statistics[index] = count;

            double[] data = new double[1];
            data[0] = count;

            chartControlInspectionAngle.Series[0].Points[index].Values = data;
        }
        private void InitializeChartAngle()
        {
            try
            {
                chartControlInspectionAngle.Series[0].Points.Clear();
                chartControlInspectionAngle.Series[0].Name = "Angle Histogram";
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±1˚", Convert.ToInt32(_statistics.Statistics[0])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±2˚", Convert.ToInt32(_statistics.Statistics[1])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±3˚", Convert.ToInt32(_statistics.Statistics[2])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±4˚", Convert.ToInt32(_statistics.Statistics[3])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±5˚", Convert.ToInt32(_statistics.Statistics[4])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±6˚", Convert.ToInt32(_statistics.Statistics[5])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±7˚", Convert.ToInt32(_statistics.Statistics[6])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±8˚", Convert.ToInt32(_statistics.Statistics[7])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±9˚", Convert.ToInt32(_statistics.Statistics[8])));
                chartControlInspectionAngle.Series[0].Points.Add(new DevExpress.XtraCharts.SeriesPoint("±10˚", Convert.ToInt32(_statistics.Statistics[9])));

                chartControlInspectionAngle.RefreshData();                
            }
            catch (Exception ex)
            {
                mLog.WriteLog(LogLevel.Fatal, LogClass.atPhoto.ToString(), string.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace));
            }
        }
        private void UpdateStaticsData()
        {
            _statistics.TotalCount++;
            if (mResultData.bTotalResult == true)
            {
                _statistics.PassCount++;
                barEditItemInspectionResult.EditValue = "PASS";
                repositoryItemTextEditInspectionResult.Appearance.ForeColor = System.Drawing.Color.LimeGreen;
            }
            else
            {
                _statistics.FailCount++;
                barEditItemInspectionResult.EditValue = "FAIL";
                repositoryItemTextEditInspectionResult.Appearance.ForeColor = System.Drawing.Color.Red;
            }
            barEditItemTotalInspectionCount.EditValue = string.Format("총 검사 수:{0:00000}", _statistics.TotalCount);
            barEditItemTotalFailCount.EditValue = string.Format("불합격 수:{0:00000}", _statistics.FailCount);
            string strStatistics = string.Format(@"{0}\{1}", global::atOpticalDecenter.Properties.Settings.Default.strSystemFolderPath, SystemDirectoryParams.StatisticsFileName);            
            UpdateChartInspectionAngle((float)(mResultData.fOpticalEmiterAngle * (180 / Math.PI)));
            RecipeFileIO.WriteInspectionStatisticsFile(strStatistics, _statistics);
        }
    }
}
