using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine
{

    public class Block
    {
        public BlockType Type;
        public byte FaceInfo;

        public Block(BlockType type)
        {
            Type = type;
            FaceInfo = 0;
        }
    }
}
