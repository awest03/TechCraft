using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model;

namespace NewTake.view
{
    public interface IChunkProcessor
    {
        void ProcessChunk(Chunk chunk);
    }
}
