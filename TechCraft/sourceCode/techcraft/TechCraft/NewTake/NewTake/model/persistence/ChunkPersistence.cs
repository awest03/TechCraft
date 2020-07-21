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
using System.IO;
#endregion

namespace NewTake.model
{
    public class ChunkPersistence : IChunkPersistence
    {
        private const String LEVELFOLDER = "c:\\techcraft";

        private readonly World world;

        public ChunkPersistence(World world)
        {
            this.world = world;

            if (!Directory.Exists(LEVELFOLDER))
            {
                Directory.CreateDirectory(LEVELFOLDER);
            }
        }

        #region save
        public void save(Chunk chunk)
        {
            //Debug.WriteLine("saving " + GetFilename(chunk.Position));
            FileStream fs = File.Open(GetFilename(chunk.Position), FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            Save(chunk, writer);
            writer.Flush();
            writer.Close();
            fs.Close();
        }
        #endregion

        #region load
        public Chunk load(Vector3i index)
        {
            Vector3i position = new Vector3i(index.X * Chunk.SIZE.X, index.Y * Chunk.SIZE.Y, index.Z * Chunk.SIZE.Z);
            string filename = GetFilename(position);

            if (File.Exists(filename))
            {
                //Debug.WriteLine("Loading " + filename);
                FileStream fs = File.Open(filename, FileMode.Open);

                BinaryReader reader = new BinaryReader(fs);
                Chunk chunk = Load(position, reader);
                reader.Close();
                fs.Close();
                //chunk.generated = true;
                chunk.State = ChunkState.AwaitingBuild;
                return chunk;
            }
            else
            {
                //Debug.WriteLine("New " + filename);
                return null;
            }
        }
        #endregion

        #region Private Save
        private void Save(Chunk chunk, BinaryWriter writer)
        {

            byte[] array = new byte[chunk.Blocks.Length];

            for (int i = 0; i < chunk.Blocks.Length; i++)
            {
                array[i] = (byte)chunk.Blocks[i].Type;
            }
            writer.Write(array);
        }
        #endregion

        #region Private Load
        private Chunk Load(Vector3i worldPosition, BinaryReader reader)
        {
            //index from position
            Vector3i index = new Vector3i(worldPosition.X / Chunk.SIZE.X, worldPosition.Y / Chunk.SIZE.Y, worldPosition.Z / Chunk.SIZE.Z);

            Chunk chunk = new Chunk(world, index);

            byte[] array = reader.ReadBytes(chunk.Blocks.Length);

            for (int i = 0; i < chunk.Blocks.Length; i++)
            {
                chunk.Blocks[i].Type = (BlockType)array[i];
            }

            return chunk;
        }
        #endregion

        private string GetFilename(Vector3i position)
        {
            return string.Format("{0}\\{1}-{2}-{3}", LEVELFOLDER, position.X, position.Y, position.Z);
        }
    }
}
