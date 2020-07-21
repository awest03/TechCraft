#region License

//  TechCraft - http//techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows

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

using Microsoft.Xna.Framework;
#endregion

namespace NewTake.model
{

    public enum Cardinal
    {
        N, S, E, W, NE, NW, SE, SW
    }

    public static class Cardinals
    {
        //TODO N is +1
        public static SignedVector3i N = new SignedVector3i(0, 0, -1);
        public static SignedVector3i NE = new SignedVector3i(+1, 0, -1);
        public static SignedVector3i E = new SignedVector3i(+1, 0, 0);
        public static SignedVector3i SE = new SignedVector3i(+1, 0, +1);
        public static SignedVector3i S = new SignedVector3i(0, 0, +1);
        public static SignedVector3i SW = new SignedVector3i(-1, 0, +1);
        public static SignedVector3i W = new SignedVector3i(-1, 0, 0);
        public static SignedVector3i NW = new SignedVector3i(-1, 0, -1);

        public static SignedVector3i VectorFrom(Cardinal c)
        {
            switch (c)
            {
                case Cardinal.N: return N;
                case Cardinal.NE: return NE;
                case Cardinal.E: return E;
                case Cardinal.SE: return SE;
                case Cardinal.S: return S;
                case Cardinal.SW: return SW;
                case Cardinal.W: return W;
                case Cardinal.NW: return NW;
            }
            throw new NotImplementedException("unknown cardinal direction" + c);
        }

        public static SignedVector3i OppositeVectorFrom(Cardinal c)
        {
            switch (c)
            {
                case Cardinal.N: return S;
                case Cardinal.NE: return SW;
                case Cardinal.E: return W;
                case Cardinal.SE: return NW;
                case Cardinal.S: return N;
                case Cardinal.SW: return NE;
                case Cardinal.W: return E;
                case Cardinal.NW: return SE;
                default:
                    break;
            }
            throw new NotImplementedException("unknown cardinal direction " + c);
        }

        public static Cardinal CardinalFrom(int x, int z)
        {
            SignedVector3i v = new SignedVector3i(x, 0, z);
            return CardinalFrom(v);
        }

        public static Cardinal CardinalFrom(SignedVector3i v)
        {

            if (v == N) return Cardinal.N;
            if (v == NE) return Cardinal.NE;
            if (v == E) return Cardinal.E;
            if (v == SE) return Cardinal.SE;
            if (v == S) return Cardinal.S;
            if (v == SW) return Cardinal.SW;
            if (v == W) return Cardinal.W;
            if (v == NW) return Cardinal.NW;

            throw new NotImplementedException("vector " + v + " does not map to a cardinal direction");
        }

        public static Cardinal[] Adjacents(Cardinal from) {
            switch (from)
            {
                case Cardinal.N: return new Cardinal[] {Cardinal.E,Cardinal.W};
                case Cardinal.NE: return new Cardinal[] { Cardinal.S, Cardinal.W, Cardinal.E, Cardinal.N };
                case Cardinal.E: return new Cardinal[] { Cardinal.N, Cardinal.S };
                case Cardinal.SE: return new Cardinal[] { Cardinal.N, Cardinal.W, Cardinal.E, Cardinal.S };
                case Cardinal.S: return new Cardinal[] { Cardinal.E, Cardinal.W };
                case Cardinal.SW: return new Cardinal[] { Cardinal.N, Cardinal.E, Cardinal.W, Cardinal.S };
                case Cardinal.W: return new Cardinal[] { Cardinal.N, Cardinal.S };
                case Cardinal.NW: return new Cardinal[] { Cardinal.S, Cardinal.E, Cardinal.N, Cardinal.W };
                default:
                    break;
            }
            throw new NotImplementedException("unknown cardinal direction " + from);
            
        }

    }

}