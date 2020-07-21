using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.terrain
{

    public interface IChunkBuilder
    {
        void build( Chunk chunk);
    }

}
