using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoProduct
{
    public class Enums
    {
        public enum ProductSeries
        {
            BTS,
            BTF,
            BJ,
            BJP,
            BEN,
        }
        public enum ProductType
        {
            Mirror = 0,
            Convergent,
            Diffuse,
            BGS,
            MicroSpot,
            Transmitter,
            Receiver,            
        }
        public enum OperatingMode
        {
            LightOn,
            DarkOn,
        }
        public enum OutputType
        {
            NPN,
            PNP,
        }
        public enum DetectMeterial
        {
            None,
            Mirror,
            WhitePaper,
            BlackPaper,
            Glass,
        }
        public enum InspectionStep
        {
            Idle,
            CheckStatus,
            // < Check Recipe Model >
            CheckModel,
            // < Optical Led Inspection >
            MoveInspect1Pos,
            InspectLEDSpot1,
            MoveInspect2Pos,
            InspectLedSpot2,
            CalcurateData,
            MoveProductInspectPos,
            CheckShortDistanceInspect,
            SetNDFilter,
            ReleaseNDFilter,
            // <InspectionMove>
            MoveInspectPos,
            // <Reverse Power Inspection>
            ReversePowerOn,
            WaitReverseTime,
            ReversePowerOff,
            // <InspectionMove>
            ErrorOccured,
        }

    }
}
