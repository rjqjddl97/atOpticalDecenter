using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeManager
{
    public class CalibrationParams
    {
        public const double YAxis_X_Referense_Pos = -48.39;
        public const double YAxis_Y_Referense_Pos = 100;
        public const double YAxis_Z_Referense_Pos = 42.53;
        public const double YAxis_X_Delta_D_X = -0.001213;
        public const double YAxis_X_OFFSET_X = 0;
        public const double YAxis_X_Delta_D_Z = -0.003775;        
        public const double YAxis_X_OFFSET_Z = -0.15448;
        public const double YAxis_Z_Delta_D_X = 0.009253;
        public const double YAxis_Z_OFFSET_X = -0.24929;
        public const double YAxis_Z_Delta_D_Z = -0.000425;        
        public const double YAxis_Z_OFFSET_Z = 0;        
        public const double YAxis_Y_Delta_D_X = -0.0082;
        public const double YAxis_Y_Delta_D_Z = -0.012;
        public const double YAxis_Y_Delta_Offset_X = 0.81999;
        public const double YAxis_Y_Delta_Offset_Z = 1.1999;

        public bool _CoordinateSwitchEnable { get; set; } = false;
        public float _imagetoSystemXcoordi { get; set; } = 1;
        public float _imagetoSystemYcoordi { get; set; } = 1;
        public bool _CoordinateCalibrationActive { get; set; } = false;

        public double _Position_Reference_X { get; set; } = 0D;
        public double _Position_Reference_Y { get; set; } = 0D;
        public double _Position_Reference_Z { get; set; } = 0D;

        public double _Position_1_X { get; set; } = 0D;
        public double _Position_1_Y { get; set; } = 0D;
        public double _Position_1_Z { get; set; } = 0D;

        public double _Position_2_X { get; set; } = 0D;
        public double _Position_2_Y { get; set; } = 0D;
        public double _Position_2_Z { get; set; } = 0D;

        public double _X_diff { get; set; } = 0D;
        public double _Y_diff { get; set; } = 0D;
        public double _Z_diff { get; set; } = 0D;

        public double _Offset_X { get; set; } = 0D;
        public double _Offset_Y { get; set; } = 0D;
        public double _Offset_Z { get; set; } = 0D;



        public class Position
        {
            public double X { get; set; } = 0;
            public double Y { get; set; } = 0;            
            public double Z { get; set; } = 0;
            public double R { get; set; } = 0;
        }
        public void SetupCalibrationParameter(Position p1, Position p2)
        {
            _Position_1_X = p1.X;
            _Position_1_Y = p1.Y;
            _Position_1_Z = p1.Z;

            _Position_2_X = p2.X;
            _Position_2_Y = p2.Y;
            _Position_2_Z = p2.Z;

            _X_diff = _Position_2_X - _Position_1_X;
            _Y_diff = _Position_2_Y - _Position_1_Y;
            _Z_diff = _Position_2_Z - _Position_1_Z;
        }
        public Position GetVectorOffset(double posx)
        {
            Position offset = new Position();
            double VectorX = 0;
            if (_X_diff != 0)
                VectorX = (posx - _Position_1_X) / _X_diff;
            else
                VectorX = 0;

            offset.X = 0;
            offset.Y = ((VectorX * _Y_diff) + _Position_1_Y) - _Position_Reference_Y;
            offset.Z = ((VectorX * _Z_diff) + _Position_1_Z) - _Position_Reference_Z;
            return offset;
        }

        public Position ForwardTransitionPosition(Position PresentPos)
        {
            Position DeltaPosition = new Position();
            Position offset = new Position();

            if (_CoordinateCalibrationActive)
            {
                offset = GetVectorOffset(PresentPos.X);

                DeltaPosition.X = PresentPos.X + offset.X;
                DeltaPosition.Y = PresentPos.Y + offset.Y;
                DeltaPosition.Z = PresentPos.Z + offset.Z;
            }
            else
            {
                DeltaPosition.X = PresentPos.X;
                DeltaPosition.Y = PresentPos.Y;
                DeltaPosition.Z = PresentPos.Z;
            }

            return DeltaPosition;
        }
        public Position InverseTransitionPosition(Position PresentPos)
        {
            Position DeltaPosition = new Position();
            Position offset = new Position();

            if (_CoordinateCalibrationActive)
            {
                offset = GetVectorOffset(PresentPos.X);

                DeltaPosition.X = PresentPos.X - offset.X;
                DeltaPosition.Y = PresentPos.Y - offset.Y;
                DeltaPosition.Z = PresentPos.Z - offset.Z;

            }
            else
            {
                DeltaPosition.X = PresentPos.X;
                DeltaPosition.Y = PresentPos.Y;
                DeltaPosition.Z = PresentPos.Z;
            }

            return DeltaPosition;
        } 
    }
}
