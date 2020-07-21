using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine.Generators
{
    public class MapTools
    {
        public static void Clear(BlockType[, ,] blocks, BlockType clearTo)
        {
            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.MAPHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                    {
                        blocks[x, y, z] = clearTo;
                    }
                }
            }
        }

        /// <summary>
        /// Get the interpolated points for the noise function
        /// </summary>
        /// <param name="noiseFn"></param>
        /// <returns></returns>
        public static double[,] SumNoiseFunctions(int width, int height, List<PerlinNoise2D> noiseFunctions)
        {
            double[,] summedValues = new double[width, height];

            // Sum each of the noise functions
            for (int i = 0; i < noiseFunctions.Count; i++)
            {
                double x_step = (float)width / (float)noiseFunctions[i].Frequency;
                double y_step = (float)height / (float)noiseFunctions[i].Frequency;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int a = (int)(x / x_step);
                        int b = a + 1;
                        int c = (int)(y / y_step);
                        int d = c + 1;

                        double intpl_val = noiseFunctions[i].getInterpolatedPoint(a, b, c, d, (x / x_step) - a, (y / y_step) - c);
                        summedValues[x,y] += intpl_val * noiseFunctions[i].Amplitude;
                    }
                }
            }
            return summedValues;
        }

        public static bool WithinMapBounds(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) return false;
            if (x > WorldSettings.MAPWIDTH - 1 || y > WorldSettings.MAPHEIGHT - 1 || z > WorldSettings.MAPLENGTH - 1) return false;
            return true;
        }
    }
}
