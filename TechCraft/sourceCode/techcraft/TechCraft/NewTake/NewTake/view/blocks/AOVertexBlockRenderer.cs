#region license

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
using System.Diagnostics;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{

    class AOVertexBlockRenderer
    {

        public List<VertexPositionDualTexture> vertexList;
        private readonly World world;

        public AOVertexBlockRenderer(World world)
        {
            this.world = world;
            Clear();
        }

        public void Clear()
        {
            vertexList = new List<VertexPositionDualTexture>();
        }

        #region BuildFaceVertices
        public void BuildFaceVertices(Vector3i blockPosition, Vector3i chunkRelativePosition, BlockFaceDirection faceDir, BlockType blockType, float aoTL, float aoTR, float aoBL, float aoBR)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

            int faceIndex = 0;
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    faceIndex = 0;
                    break;
                case BlockFaceDirection.XDecreasing:
                    faceIndex = 1;
                    break;
                case BlockFaceDirection.YIncreasing:
                    faceIndex = 2;
                    break;
                case BlockFaceDirection.YDecreasing:
                    faceIndex = 3;
                    break;
                case BlockFaceDirection.ZIncreasing:
                    faceIndex = 4;
                    break;
                case BlockFaceDirection.ZDecreasing:
                    faceIndex = 5;
                    break;
            }

            int crackStage = 0;


            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];
            Vector2[] CrackUVList = TextureHelper.CrackMappings[crackStage * 6 + faceIndex];

            float light = 2;//TODO light hardcoded to 2

            Vector2 aoTilePosition = new Vector2(0, 0);

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(1, 0, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[2], CrackUVList[2], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[3], CrackUVList[3], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[4], CrackUVList[4], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(1, 0, 0), light, UVList[5], CrackUVList[5], aoBL);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(-1, 0, 0), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[2], CrackUVList[2], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[3], CrackUVList[3], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[4], CrackUVList[4], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), light, UVList[5], CrackUVList[5], aoBR);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[0], CrackUVList[0], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 1, 0), light, UVList[1], CrackUVList[1], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[2], CrackUVList[2], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[3], CrackUVList[3], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[4], CrackUVList[4], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 1, 0), light, UVList[5], CrackUVList[5], aoTL);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, -1, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[1], CrackUVList[1], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[2], CrackUVList[2], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[3], CrackUVList[3], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[4], CrackUVList[4], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, -1, 0), light, UVList[5], CrackUVList[5], aoTR);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 0, 1), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), light, UVList[2], CrackUVList[2], aoBL);

                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), light, UVList[3], CrackUVList[3], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), light, UVList[4], CrackUVList[4], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, 0, 1), light, UVList[5], CrackUVList[5], aoBR);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 0, -1), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[2], CrackUVList[2], aoBR);

                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[3], CrackUVList[3], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[4], CrackUVList[4], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, 0, -1), light, UVList[5], CrackUVList[5], aoBL);
                    }
                    break;
            }
        }
        #endregion

        private void AddVertex(Vector3i blockPosition, Vector3i chunkRelativePosition, Vector3 vertexAdd, Vector3 normal, float light, Vector2 uv1, Vector2 uv2, float aoWeight)
        {
            int x = (int)(chunkRelativePosition.X + vertexAdd.X);
            int y = (int)(chunkRelativePosition.Y + vertexAdd.Y);
            int z = (int)(chunkRelativePosition.Z + vertexAdd.Z);

            vertexList.Add(new VertexPositionDualTexture(blockPosition.asVector3() + vertexAdd, uv1, uv2, aoWeight));
            //indexList.Add(vertexIndex);
            //vertexInfo[x, y, z].Index = vertexIndex;
            //vertexInfo[x, y, z].Count++;
            //vertexIndex++;  
        }

    }
}
