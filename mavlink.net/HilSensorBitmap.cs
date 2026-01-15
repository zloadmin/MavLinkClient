namespace MavLinkNet
{
    public static class HilSensorBitmap
    {
        public static UInt32 Generate(UasHilSensor prev, UasHilSensor curr)
        {
            UInt32 b = 0;

            if (FloatChanged(prev.Xacc, curr.Xacc)) b |= 1u << 0;
            if (FloatChanged(prev.Yacc, curr.Yacc)) b |= 1u << 1;
            if (FloatChanged(prev.Zacc, curr.Zacc)) b |= 1u << 2;

            if (FloatChanged(prev.Xgyro, curr.Xgyro)) b |= 1u << 3;
            if (FloatChanged(prev.Ygyro, curr.Ygyro)) b |= 1u << 4;
            if (FloatChanged(prev.Zgyro, curr.Zgyro)) b |= 1u << 5;

            if (FloatChanged(prev.Xmag, curr.Xmag)) b |= 1u << 6;
            if (FloatChanged(prev.Ymag, curr.Ymag)) b |= 1u << 7;
            if (FloatChanged(prev.Zmag, curr.Zmag)) b |= 1u << 8;

            if (FloatChanged(prev.AbsPressure, curr.AbsPressure)) b |= 1u << 9;
            if (FloatChanged(prev.DiffPressure, curr.DiffPressure)) b |= 1u << 10;
            if (FloatChanged(prev.PressureAlt, curr.PressureAlt)) b |= 1u << 11;
            if (FloatChanged(prev.Temperature, curr.Temperature)) b |= 1u << 12;

            // PX4 REQUIREMENT: bitmap must not be zero
            if (b == 0)
                b = 0x1FFF; // Gazebo behavior fallback

            return b;
        }

        private static bool FloatChanged(float a, float b)
        {
            if (float.IsNaN(a) && float.IsNaN(b)) return false;
            if (float.IsNaN(a) || float.IsNaN(b)) return true;
            if (float.IsInfinity(a) || float.IsInfinity(b)) return a != b;

            return Math.Abs(a - b) > 1e-5f;
        }
    }

}