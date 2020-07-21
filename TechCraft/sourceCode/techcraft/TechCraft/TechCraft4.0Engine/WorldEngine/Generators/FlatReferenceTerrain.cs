using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine.Generators
{

    class FlatReferenceTerrain : IRegionBuilder
    {
        public void build(Region chunk)
        {

            int sizeY = WorldSettings.REGIONHEIGHT;
            int sizeX = WorldSettings.REGIONWIDTH;
            int sizeZ = WorldSettings.REGIONLENGTH;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    for (int z = 0; z < sizeZ; z++)
                    {
                        BlockType t;

                        if (y < sizeY / 4)
                            t = BlockType.Lava;
                        /*
                         * else if (y == (sizeY / 2) - 1) // test caves visibility t
                         * = Type.empty;
                         */
                        else if (y < sizeY / 2)
                            t = BlockType.Rock;
                        else if (y == sizeY / 2)
                        {
                            t = BlockType.Grass;
                        }
                        else
                        {
                            if (y == sizeY / 2 + 1 && (x == 0 || x == sizeX - 1 || z == 0 || z == sizeZ - 1))
                                t = BlockType.Brick;
                            else
                                t = BlockType.None;
                        }

                      
                        chunk.AddBlock(x, y, z, t);
                    }
                }
            }

        }
    }
}
