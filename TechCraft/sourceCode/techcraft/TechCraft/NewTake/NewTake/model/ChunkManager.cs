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
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;

using NewTake.model;
#endregion

namespace NewTake.model.types
{

    public class ChunkManager : Dictionary2<Chunk>
    {

        private IChunkPersistence persistence;

        public ChunkManager(IChunkPersistence persistence)
        {
            this.persistence = persistence;
        }


        public override void Remove(uint x, uint z)
        {
            Chunk chunk = this[x, z];
            if (chunk == null) return;

            beforeRemove(chunk);

            Chunk removed;
            TryRemove(KeyFromCoords(x, z), out removed);

        }

        private void beforeRemove(Chunk chunk)
        {
            persistence.save(chunk);
        }

        public Chunk get(Vector3i index)
        {
            return this[index.X, index.Z];
        }

        /* public override Chunk this[uint x, uint z]
         {
             get
             {
                 Chunk chunk = base[x, z];
                 if (chunk == null)
                 {
                     Vector3i index = new Vector3i(x, 0, z);

                     chunk = whenNull(index);
                     base[x, z] = chunk; 
                 }
                 return chunk;
             }
             set
             {
                 base[x, z] = value;

             }
         }*/

        /*
 * The idea of loading directly whenever accessing a null chunk was cool but theres much more to do in the worldrenderer.generate method
 * 
 * Needs more thinking and surely some major refactoring. 
 * 
 */

        private Chunk whenNull(Vector3i index)
        {
            //return persistence.load(index);
            return null;
        }


        public Chunk load(Vector3i index)
        {
            return persistence.load(index);
        }


    }
}
