using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine
{
    public class BlockInformation
    {
        public static BlockTexture GetTexture(BlockType blockType, BlockFaceDirection faceDir)
        {
            return GetTexture(blockType, faceDir, BlockType.None);
        }

        public static uint GetCost(BlockType type)
        {
            return 1;
        }

        public static bool IsSolidBlock(BlockType type)
        {
            if (type == BlockType.Water || type == BlockType.None) return false;
            return true;
        }

        public static bool IsLightEmittingBlock(BlockType type)
        {
            if (type == BlockType.Lava) return true;
            return false;
        }

        public static bool IsLightTransparentBlock(BlockType type)
        {
            if (type == BlockType.None || type == BlockType.Water) return true;
            return false;
        }

        public static bool IsDiggable(BlockType type)
        {
            if (type == BlockType.Water) return false;
            return true;
        }

        public static bool IsModelBlock(BlockType type)
        {
            //if (type == BlockType.Water || type == BlockType.Leaves) return true;
            return false;
        }

        /// <summary>
        /// Return the appropriate texture to render a given face of a block
        /// </summary>
        /// <param name="blockType"></param>
        /// <param name="faceDir"></param>
        /// <param name="blockAbove">Reserved for blocks which behave differently if certain blocks are above them</param>
        /// <returns></returns>
        public static BlockTexture GetTexture(BlockType blockType, BlockFaceDirection faceDir, BlockType blockAbove)
        {
            switch (blockType)
            {
                case BlockType.Brick:
                    return BlockTexture.Brick;
                case BlockType.Dirt:
                    return BlockTexture.Dirt;
                case BlockType.Gold:
                    return BlockTexture.Gold;
                case BlockType.Grass:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing:
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.ZIncreasing:
                        case BlockFaceDirection.ZDecreasing:
                            return BlockTexture.GrassSide;
                        case BlockFaceDirection.YIncreasing:
                            return BlockTexture.GrassTop;
                        case BlockFaceDirection.YDecreasing:
                            return BlockTexture.Dirt;
                        default :
                            return BlockTexture.Rock;
                    }
                case BlockType.Iron:
                    return BlockTexture.Iron;
                case BlockType.Lava:
                    return BlockTexture.Lava;
                case BlockType.Leaves:
                    return BlockTexture.Leaves;
                case BlockType.Gravel:
                    return BlockTexture.Gravel;
                case BlockType.Rock:
                    return BlockTexture.Rock;
                case BlockType.Sand:
                    return BlockTexture.Sand;
                case BlockType.Snow:
                    return BlockTexture.Snow;
                case BlockType.Tree:
                    switch (faceDir)
                    {
                        case BlockFaceDirection.XIncreasing:
                        case BlockFaceDirection.XDecreasing:
                        case BlockFaceDirection.ZIncreasing:
                        case BlockFaceDirection.ZDecreasing:
                            return BlockTexture.TreeSide;
                        case BlockFaceDirection.YIncreasing:
                        case BlockFaceDirection.YDecreasing:
                            return BlockTexture.TreeTop;
                        default:
                            return BlockTexture.Rock;
                    }
                case BlockType.Water:
                    return BlockTexture.Water;
                default:
                    return BlockTexture.Rock;

            }
        }
    }
}
