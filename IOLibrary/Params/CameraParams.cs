using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeManager
{
    public class CameraParams
    {
        public string FriendlyName { get; set; } = "None";
        public int HResolution { get; set; } = 1280;
        public int VResolution { get; set; } = 960;
        public int FrameRate { get; set; } = 10;

        public int ExposureTime { get; set; } = 7000;
        public int Gain { get; set; } = 300;
        public float OnePixelResolution { get; set; } = 0.01875F;

        public float ImageSensorHSize { get; set; } = 6.44f;
        public float ImageSensorVSize { get; set; } = 4.62f;
        public float LensFocusLength { get; set; } = 12.0f;
        public CameraParams()
        {

        }
    }
}
