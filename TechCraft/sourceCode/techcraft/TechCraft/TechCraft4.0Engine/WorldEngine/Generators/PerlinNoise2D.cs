using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine.Generators
{
    /// <summary>
    /// 2D Perlin Noise function
    /// </summary>
    public class PerlinNoise2D
    {
        private double[,] noiseValues;
        private float amplitude = 1;    // Max amplitude of the function
        private int frequency = 1;      // Frequency of the function

        /// <summary>
        /// Constructor
        /// </summary>
        public PerlinNoise2D(int freq, float _amp)
        {
            Random rand = new Random(System.Environment.TickCount);
            noiseValues = new double[freq, freq];
            amplitude = _amp;
            frequency = freq;

            // Generate our noise values
            for (int i = 0; i < freq; i++)
            {
                for (int k = 0; k < freq; k++)
                {
                    noiseValues[i, k] = rand.NextDouble();
                }
            }
        }

        /// <summary>
        /// Get the interpolated point from the noise graph using cosine interpolation
        /// </summary>
        /// <returns></returns>
        public double getInterpolatedPoint(int _xa, int _xb, int _ya, int _yb, double x, double y)
        {
            double i1 = interpolate(
                noiseValues[_xa % Frequency, _ya % frequency],
                noiseValues[_xb % Frequency, _ya % frequency]
                , x);

            double i2 = interpolate(
                noiseValues[_xa % Frequency, _yb % frequency],
                noiseValues[_xb % Frequency, _yb % frequency]
                , x);

            return interpolate(i1, i2, y);
        }

        /// <summary>
        /// Get the interpolated point from the noise graph using cosine interpolation
        /// </summary>
        /// <returns></returns>
        private double interpolate(double a, double b, double x)
        {
            double ft = x * Math.PI;
            double f = (1 - Math.Cos(ft)) * .5;

            // Returns a Y value between 0 and 1
            return a * (1 - f) + b * f;
        }

        #region Accessors/Mutators
        public float Amplitude { get { return amplitude; } }
        public int Frequency { get { return frequency; } }
        #endregion
    }
}
