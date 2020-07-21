using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine
{
    public struct Flow
    {
        public int X;
        public int Y;
        public int Z;
        public int Count;

        public Flow(int x, int y, int z, int count)
        {
            X = x;
            Y = y;
            Z = z;
            Count = count;
        }
    }
}
