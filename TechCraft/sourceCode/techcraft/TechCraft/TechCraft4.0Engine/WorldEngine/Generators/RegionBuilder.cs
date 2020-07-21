using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine.Generators
{
    public interface IRegionBuilder
    {
        void build( Region chunk);
    }
}
