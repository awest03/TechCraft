using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine
{
    public enum BlockFaceDirection
    {
        XIncreasing = 1,
        XDecreasing = 2,
        YIncreasing = 4,
        YDecreasing = 8,
        ZIncreasing = 16,
        ZDecreasing = 32,
        MAXIMUM
    }
}
