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
        public bool JigCalibrationPatternMatching(Bitmap sourceimg, Bitmap tmepleteimg , int _iThreshold, int _iMatching)
        {            
            try
            {                

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
