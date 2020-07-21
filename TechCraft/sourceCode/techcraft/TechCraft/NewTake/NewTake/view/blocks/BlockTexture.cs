#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{

    public enum BlockTexture
    {
        MossyCobble,
        Dirt,
        Obsidian,
        GrassSide,
        GrassTop,
        Gravel,
        Lava,
        SolidLeaves,
        Cobble,
        Rock,
        Sand,
        Snow,
        TreeTop,
        TreeSide,
        Water,
        Leaves,
        LongGrass,
        RedFlower,
        Stone,
        Brick,
        MAXIMUM
    }

    public static class TextureHelper
    {
        public const int TEXTUREATLASSIZE = 16;

        public static Dictionary<int, Vector2[]> UVMappings;

        static TextureHelper()
        {
            BuildUVMappings();
        }


        private static Dictionary<int, Vector2[]> BuildUVMappings()
        {
            UVMappings = new Dictionary<int, Vector2[]>();
            for (int i = 0; i < (int)BlockTexture.MAXIMUM; i++)
            {
                UVMappings.Add((i * 6), TextureHelper.GetUVMapping(i, BlockFaceDirection.XIncreasing));
                UVMappings.Add((i * 6) + 1, TextureHelper.GetUVMapping(i, BlockFaceDirection.XDecreasing));
                UVMappings.Add((i * 6) + 2, TextureHelper.GetUVMapping(i, BlockFaceDirection.YIncreasing));
                UVMappings.Add((i * 6) + 3, TextureHelper.GetUVMapping(i, BlockFaceDirection.YDecreasing));
                UVMappings.Add((i * 6) + 4, TextureHelper.GetUVMapping(i, BlockFaceDirection.ZIncreasing));
                UVMappings.Add((i * 6) + 5, TextureHelper.GetUVMapping(i, BlockFaceDirection.ZDecreasing));
            }
            return UVMappings;
        }

        #region GetUVMapping
        public static Vector2[] GetUVMapping(int texture, BlockFaceDirection faceDir)
        {
            int textureIndex = texture;
            // Assumes a texture atlas of 8x8 textures

            int y = textureIndex / TEXTUREATLASSIZE;
            int x = textureIndex % TEXTUREATLASSIZE;

            float ofs = 1f / ((float)TEXTUREATLASSIZE);

            float yOfs = y * ofs;
            float xOfs = x * ofs;

            //ofs -= 0.01f;

            Vector2[] UVList = new Vector2[6];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.XDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.YIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[1] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.YDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.ZIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.ZDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
            }
            return UVList;
        }
        #endregion

    }

}
