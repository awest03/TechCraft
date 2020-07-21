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

    class LightingVertexBlockRenderer
    {
        public List<short> indexList;
        public List<VertexPositionTextureLight> vertexList;
        private readonly World world;
        private short index;

        public LightingVertexBlockRenderer(World world)
        {
            this.world = world;
            Clear();
        }

        public void Clear()
        {
            vertexList = new List<VertexPositionTextureLight>();
            indexList = new List<short>();
            index = 0;
        }

        public void BuildPlantVertices(Vector3i blockPosition, Vector3i chunkRelativePosition, BlockType blockType, float sunLight, Color localLight)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType);
            Vector2[] UVList;

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XIncreasing];
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLight, localLight);
            AddIndex(0, 1, 2, 2, 1, 3);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.XDecreasing];
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0.5f, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLight, localLight);
            AddIndex(0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZIncreasing];
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.5f), new Vector3(0, 0, 1), UVList[0], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.5f), new Vector3(0, 0, 1), UVList[1], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.5f), new Vector3(0, 0, 1), UVList[5], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.5f), new Vector3(0, 0, 1), UVList[2], sunLight, localLight);
            AddIndex(0, 1, 3, 0, 3, 2);

            UVList = TextureHelper.UVMappings[(int)texture * 6 + (int)BlockFaceDirection.ZDecreasing];
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0.5f), new Vector3(0, 0, -1), UVList[0], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0.5f), new Vector3(0, 0, -1), UVList[1], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0.5f), new Vector3(0, 0, -1), UVList[2], sunLight, localLight);
            AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0.5f), new Vector3(0, 0, -1), UVList[5], sunLight, localLight);
            AddIndex(0, 1, 2, 2, 1, 3);
        }

        #region BuildFaceVertices
        public void BuildFaceVertices(Vector3i blockPosition, Vector3i chunkRelativePosition, BlockFaceDirection faceDir, BlockType blockType, float sunLightTL, float sunLightTR, float sunLightBL, float sunLightBR, Color localLightTL, Color localLightTR, Color localLightBL, Color localLightBR)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

            int faceIndex = (int)faceDir;

            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 1, 2, 2, 1, 3);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLightBL, localLightBL);
                        AddIndex(0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        //BL,BR,TR,BL,TR,TL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), UVList[4], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 1, 0), UVList[5], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 1, 0), UVList[1], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), UVList[3], sunLightBL, localLightBL);
                        AddIndex(3, 2, 0, 3, 0, 1);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        //TR,BR,TL,TL,BR,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, -1, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), UVList[2], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), UVList[4], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, -1, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 2, 1, 1, 2, 3);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 0, 1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, 0, 1), UVList[5], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), UVList[2], sunLightBL, localLightBL);
                        AddIndex(0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 0, -1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), UVList[2], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, 0, -1), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 1, 2, 2, 1, 3);
                    }
                    break;
            }
        }
        #endregion

        private void AddVertex(Vector3i blockPosition, Vector3i chunkRelativePosition, Vector3 vertexAdd, Vector3 normal, Vector2 uv1, float sunLight, Color localLight)
        {
            vertexList.Add(new VertexPositionTextureLight(blockPosition.asVector3() + vertexAdd, uv1, sunLight, localLight.ToVector3()));
        }

        private void AddIndex(short i1, short i2, short i3, short i4, short i5, short i6)
        {
            indexList.Add((short)(index + i1));
            indexList.Add((short)(index + i2));
            indexList.Add((short)(index + i3));
            indexList.Add((short)(index + i4));
            indexList.Add((short)(index + i5));
            indexList.Add((short)(index + i6));
            index += 4;
        }
    }
}
