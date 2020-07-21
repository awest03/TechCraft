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

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NewTake.model;
using NewTake.view.blocks;
using NewTake.profiling;
#endregion

namespace NewTake.view
{
    public class LightingChunkProcessor : IChunkProcessor
    {
        private const int MAX_SUN_VALUE = 16;
        private Random r = new Random();

        public void ProcessChunk(Chunk chunk)
        {
            ClearLighting(chunk);
            FillLighting(chunk);
        }

        #region ClearLighting
        private void ClearLighting(Chunk chunk)
        {
            try
            {
                byte sunValue = MAX_SUN_VALUE;

                for (byte x = 0; x < Chunk.SIZE.X; x++)
                {
                    for (byte z = 0; z < Chunk.SIZE.Z; z++)
                    {
                        int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                        bool inShade = false;
                        //for (byte y = Chunk.MAX.Y; y > 0; y--)
                        for (byte y = Chunk.MAX.Y; y > chunk.lowestNoneBlock.Y; y--)
                        {
                            if (chunk.Blocks[offset + y].Type != BlockType.None) inShade = true;
                            if (!inShade)
                            {
                                chunk.Blocks[offset + y].Sun = sunValue;
                            }
                            else
                            {
                                chunk.Blocks[offset + y].Sun = 0;
                            }

                            if (chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                            {
                                chunk.Blocks[offset + y].R = (byte)r.Next(17);
                                chunk.Blocks[offset + y].G = (byte)r.Next(17);
                                chunk.Blocks[offset + y].B = (byte)r.Next(17);
                            }
                            else
                            {
                                chunk.Blocks[offset + y].R = 0;
                                chunk.Blocks[offset + y].G = 0;
                                chunk.Blocks[offset + y].B = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ClearLighting Exception");
            }
        }
        #endregion

        #region PropogateLight

        private byte Attenuate(byte light)
        {
            return (byte)((light * 9) / 10);
        }
        private void PropogateLightSun(Chunk chunk, byte x, byte y, byte z, byte light)
        {
                int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
                if (chunk.Blocks[offset].Type != BlockType.None && chunk.Blocks[offset].Type != BlockType.Water) return;
                if (chunk.Blocks[offset].Sun >= light) return;
                chunk.Blocks[offset].Sun = light;

                if (light > 1)
                {
                    light = Attenuate(light);

                    // Propogate light within this chunk
                    if (x > 0) PropogateLightSun(chunk, (byte)(x - 1), y, z, light);
                    if (x < Chunk.MAX.X) PropogateLightSun(chunk, (byte)(x + 1), y, z, light);
                    if (y > 0) PropogateLightSun(chunk, x, (byte)(y - 1), z, light);
                    if (y < Chunk.MAX.Y) PropogateLightSun(chunk, x, (byte)(y + 1), z, light);
                    if (z > 0) PropogateLightSun(chunk, x, y, (byte)(z - 1), light);
                    if (z < Chunk.MAX.Z) PropogateLightSun(chunk, x, y, (byte)(z + 1), light);

                    //if (chunk.E == null || chunk.W == null || chunk.S == null || chunk.N == null)
                    //{
                    //    throw new Exception("LIGHTING ISSUE");
                    //}

                    if (chunk.E != null && x == 0) PropogateLightSun(chunk.E, (byte)(Chunk.MAX.X), y, z, light);
                    if (chunk.W != null && (x == Chunk.MAX.X)) PropogateLightSun(chunk.W, 0, y, z, light);
                    if (chunk.S != null && z == 0) PropogateLightSun(chunk.S, x, y, (byte)(Chunk.MAX.Z), light);
                    if (chunk.N != null && (z == Chunk.MAX.Z)) PropogateLightSun(chunk.N, x, y, 0, light);
                }
        }

        private void PropogateLightR(Chunk chunk, byte x, byte y, byte z, byte lightR)
        {
            try
            {
                int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
                if (chunk.Blocks[offset].Type != BlockType.None && chunk.Blocks[offset].Type != BlockType.Water) return;
                if (chunk.Blocks[offset].R >= lightR) return;
                chunk.Blocks[offset].R = lightR;
                if (chunk.State > ChunkState.Lighting) chunk.State = ChunkState.AwaitingBuild;
                if (lightR > 1)
                {
                    lightR = Attenuate(lightR);

                    if (x > 0) PropogateLightR(chunk, (byte)(x - 1), y, z, lightR);
                    if (x < Chunk.MAX.X) PropogateLightR(chunk, (byte)(x + 1), y, z, lightR);
                    if (y > 0) PropogateLightR(chunk, x, (byte)(y - 1), z, lightR);
                    if (y < Chunk.MAX.Y) PropogateLightR(chunk, x, (byte)(y + 1), z, lightR);
                    if (z > 0) PropogateLightR(chunk, x, y, (byte)(z - 1), lightR);
                    if (z < Chunk.MAX.Z) PropogateLightR(chunk, x, y, (byte)(z + 1), lightR);

                    if (chunk.E != null && x == 0) PropogateLightR(chunk.E, (byte)(Chunk.MAX.X), y, z, lightR);
                    if (chunk.W != null && (x == Chunk.MAX.X)) PropogateLightR(chunk.W, 0, y, z, lightR);
                    if (chunk.S != null && z == 0) PropogateLightR(chunk.S, x, y, (byte)(Chunk.MAX.Z), lightR);
                    if (chunk.N != null && (z == Chunk.MAX.Z)) PropogateLightR(chunk.N, x, y, 0, lightR);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("PropogateLightR Exception");
            }
        }

        private void PropogateLightG(Chunk chunk, byte x, byte y, byte z, byte lightG)
        {
            try
            {
                int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
                if (chunk.Blocks[offset].Type != BlockType.None && chunk.Blocks[offset].Type != BlockType.Water) return;
                if (chunk.Blocks[offset].G >= lightG) return;
                chunk.Blocks[offset].G = lightG;

                if (lightG > 1)
                {
                    lightG = Attenuate(lightG);
                    if (x > 0) PropogateLightG(chunk, (byte)(x - 1), y, z, lightG);
                    if (x < Chunk.MAX.X) PropogateLightG(chunk, (byte)(x + 1), y, z, lightG);
                    if (y > 0) PropogateLightG(chunk, x, (byte)(y - 1), z, lightG);
                    if (y < Chunk.MAX.Y) PropogateLightG(chunk, x, (byte)(y + 1), z, lightG);
                    if (z > 0) PropogateLightG(chunk, x, y, (byte)(z - 1), lightG);
                    if (z < Chunk.MAX.Z) PropogateLightG(chunk, x, y, (byte)(z + 1), lightG);

                    if (chunk.E != null && x == 0) PropogateLightG(chunk.E, (byte)(Chunk.MAX.X), y, z, lightG);
                    if (chunk.W != null && (x == Chunk.MAX.X)) PropogateLightG(chunk.W, 0, y, z, lightG);
                    if (chunk.S != null && z == 0) PropogateLightG(chunk.S, x, y, (byte)(Chunk.MAX.Z), lightG);
                    if (chunk.N != null && (z == Chunk.MAX.Z)) PropogateLightG(chunk.N, x, y, 0, lightG);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("PropogateLightG Exception");
            }
        }

        private void PropogateLightB(Chunk chunk, byte x, byte y, byte z, byte lightB)
        {
            try
            {
                int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
                if (chunk.Blocks[offset].Type != BlockType.None && chunk.Blocks[offset].Type != BlockType.Water) return;
                if (chunk.Blocks[offset].B >= lightB) return;
                chunk.Blocks[offset].B = lightB;

                if (lightB > 1)
                {
                    lightB = Attenuate(lightB);

                    if (x > 0) PropogateLightB(chunk, (byte)(x - 1), y, z, lightB);
                    if (x < Chunk.MAX.X) PropogateLightB(chunk, (byte)(x + 1), y, z, lightB);
                    if (y > 0) PropogateLightB(chunk, x, (byte)(y - 1), z, lightB);
                    if (y < Chunk.MAX.Y) PropogateLightB(chunk, x, (byte)(y + 1), z, lightB);
                    if (z > 0) PropogateLightB(chunk, x, y, (byte)(z - 1), lightB);
                    if (z < Chunk.MAX.Z) PropogateLightB(chunk, x, y, (byte)(z + 1), lightB);

                    if (chunk.E != null && x == 0) PropogateLightB(chunk.E, (byte)(Chunk.MAX.X), y, z, lightB);
                    if (chunk.W != null && (x == Chunk.MAX.X)) PropogateLightB(chunk.W, 0, y, z, lightB);
                    if (chunk.S != null && z == 0) PropogateLightB(chunk.S, x, y, (byte)(Chunk.MAX.Z), lightB);
                    if (chunk.N != null && (z == Chunk.MAX.Z)) PropogateLightB(chunk.N, x, y, 0, lightB);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("PropogateLightB Exception");
            }
        }
        #endregion

        #region FillLighting
        private void FillLighting(Chunk chunk)
        {
            FillLightingSun(chunk);
            FillLightingR(chunk);
            FillLightingG(chunk);
            FillLightingB(chunk);
        }

        private void FillLightingSun(Chunk chunk)
        {

            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    //for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    for (byte y = chunk.lowestNoneBlock.Y; y < Chunk.SIZE.Y; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None)
                        {
                            // Sunlight
                            if (chunk.Blocks[offset + y].Sun > 1)
                            {
                                byte light = Attenuate(chunk.Blocks[offset + y].Sun);

                                if (x > 0) PropogateLightSun(chunk, (byte)(x - 1), y, z, light);
                                if (x < Chunk.MAX.X) PropogateLightSun(chunk, (byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightSun(chunk, x, (byte)(y - 1), z, light);
                                if (y < Chunk.MAX.Y) PropogateLightSun(chunk, x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightSun(chunk, x, y, (byte)(z - 1), light);
                                if (z < Chunk.MAX.Z) PropogateLightSun(chunk, x, y, (byte)(z + 1), light);
                            }

                            // Pull in light from neighbours
                            if (chunk.E!=null && x == 0) PropogateLightSun(chunk, x, y, z, chunk.E.BlockAt(Chunk.MAX.X, y, z).Sun);
                            if (chunk.W!=null && x == Chunk.MAX.X) PropogateLightSun(chunk, x, y, z, chunk.W.BlockAt(0, y, z).Sun);
                            if (chunk.S!=null && z == 0) PropogateLightSun(chunk, x, y, z, chunk.S.BlockAt(x, y, Chunk.MAX.Z).Sun);
                            if (chunk.N!=null && z == Chunk.MAX.Z) PropogateLightSun(chunk, x, y, z, chunk.N.BlockAt(x, y, 0).Sun);
                        }
                    }
                }
            }

        }

        private void FillLightingR(Chunk chunk)
        {
            try
            {
                for (byte x = 0; x < Chunk.SIZE.X; x++)
                {
                    for (byte z = 0; z < Chunk.SIZE.Z; z++)
                    {
                        int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                        //for (byte y = 0; y < Chunk.SIZE.Y; y++)
                        for (byte y = chunk.lowestNoneBlock.Y; y < Chunk.SIZE.Y; y++)
                        {
                            if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                            {
                                // Local light R
                                if (chunk.Blocks[offset + y].R > 1)
                                {
                                    byte light = Attenuate(chunk.Blocks[offset + y].R);

                                    if (x > 0) PropogateLightR(chunk, (byte)(x - 1), y, z, light);
                                    if (x < Chunk.MAX.X) PropogateLightR(chunk, (byte)(x + 1), y, z, light);
                                    if (y > 0) PropogateLightR(chunk, x, (byte)(y - 1), z, light);
                                    if (y < Chunk.MAX.Y) PropogateLightR(chunk, x, (byte)(y + 1), z, light);
                                    if (z > 0) PropogateLightR(chunk, x, y, (byte)(z - 1), light);
                                    if (z < Chunk.MAX.Z) PropogateLightR(chunk, x, y, (byte)(z + 1), light);
                                }

                                // Pull in light from neighbours
                                if (chunk.E!=null && x == 0) PropogateLightR(chunk, x, y, z, chunk.E.BlockAt(Chunk.MAX.X, y, z).R);
                                if (chunk.W!=null && x == Chunk.MAX.X) PropogateLightR(chunk, x, y, z, chunk.W.BlockAt(0, y, z).R);
                                if (chunk.S!=null && z == 0) PropogateLightR(chunk, x, y, z, chunk.S.BlockAt(x, y, Chunk.MAX.Z).R);
                                if (chunk.N!=null && z == Chunk.MAX.Z) PropogateLightR(chunk, x, y, z, chunk.N.BlockAt(x, y, 0).R);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine("FillLightingR Exception");
            }
        }

        private void FillLightingG(Chunk chunk)
        {
            try
            {
                for (byte x = 0; x < Chunk.SIZE.X; x++)
                {
                    for (byte z = 0; z < Chunk.SIZE.Z; z++)
                    {
                        int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                        //for (byte y = 0; y < Chunk.SIZE.Y; y++)
                        for (byte y = chunk.lowestNoneBlock.Y; y < Chunk.SIZE.Y; y++)
                        {
                            if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                            {
                                // Local light G
                                if (chunk.Blocks[offset + y].G > 1)
                                {
                                    byte light = Attenuate(chunk.Blocks[offset + y].G);
                                    if (x > 0) PropogateLightG(chunk, (byte)(x - 1), y, z, light);
                                    if (x < Chunk.MAX.X) PropogateLightG(chunk, (byte)(x + 1), y, z, light);
                                    if (y > 0) PropogateLightG(chunk, x, (byte)(y - 1), z, light);
                                    if (y < Chunk.MAX.Y) PropogateLightG(chunk, x, (byte)(y + 1), z, light);
                                    if (z > 0) PropogateLightG(chunk, x, y, (byte)(z - 1), light);
                                    if (z < Chunk.MAX.Z) PropogateLightG(chunk, x, y, (byte)(z + 1), light);
                                }

                                // Pull in light from neighbours
                                if (chunk.E!=null && x == 0) PropogateLightG(chunk, x, y, z, chunk.E.BlockAt(Chunk.MAX.X, y, z).G);
                                if (chunk.W!=null && x == Chunk.MAX.X) PropogateLightG(chunk, x, y, z, chunk.W.BlockAt(0, y, z).G);
                                if (chunk.S!=null && z == 0) PropogateLightG(chunk, x, y, z, chunk.S.BlockAt(x, y, Chunk.MAX.Z).G);
                                if (chunk.N!=null && z == Chunk.MAX.Z) PropogateLightG(chunk, x, y, z, chunk.N.BlockAt(x, y, 0).G);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine("FillLightingG Exception");
            }
        }

        private void FillLightingB(Chunk chunk)
        {
            try
            {
                for (byte x = 0; x < Chunk.SIZE.X; x++)
                {
                    for (byte z = 0; z < Chunk.SIZE.Z; z++)
                    {
                        int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                        //for (byte y = 0; y < Chunk.SIZE.Y; y++)
                        for (byte y = chunk.lowestNoneBlock.Y; y < Chunk.SIZE.Y; y++)
                        {
                            if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.Tree || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                            {
                                // Local light B
                                if (chunk.Blocks[offset + y].B > 1)
                                {
                                    byte light = Attenuate(chunk.Blocks[offset + y].B);
                                    if (x > 0) PropogateLightB(chunk, (byte)(x - 1), y, z, light);
                                    if (x < Chunk.MAX.X) PropogateLightB(chunk, (byte)(x + 1), y, z, light);
                                    if (y > 0) PropogateLightB(chunk, x, (byte)(y - 1), z, light);
                                    if (y < Chunk.MAX.Y) PropogateLightB(chunk, x, (byte)(y + 1), z, light);
                                    if (z > 0) PropogateLightB(chunk, x, y, (byte)(z - 1), light);
                                    if (z < Chunk.MAX.Z) PropogateLightB(chunk, x, y, (byte)(z + 1), light);
                                }

                                // Pull in light from neighbours
                                if (chunk.E!=null && x == 0) PropogateLightB(chunk, x, y, z, chunk.E.BlockAt(Chunk.MAX.X, y, z).B);
                                if (chunk.W!=null && x == Chunk.MAX.X) PropogateLightB(chunk, x, y, z, chunk.W.BlockAt(0, y, z).B);
                                if (chunk.S!=null && z == 0) PropogateLightB(chunk, x, y, z, chunk.S.BlockAt(x, y, Chunk.MAX.Z).B);
                                if (chunk.N!=null && z == Chunk.MAX.Z) PropogateLightB(chunk, x, y, z, chunk.N.BlockAt(x, y, 0).B);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine("FillLightingB Exception");
            }
        }
        #endregion

    }
}
