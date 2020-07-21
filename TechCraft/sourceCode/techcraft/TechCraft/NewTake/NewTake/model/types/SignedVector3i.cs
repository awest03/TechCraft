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

using Microsoft.Xna.Framework;
using NewTake.model;
#endregion

namespace NewTake
{

    public struct SignedVector3i
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public SignedVector3i(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public SignedVector3i(Vector3 vector3)
        {
            X = (int)vector3.X;
            Y = (int)vector3.Y;
            Z = (int)vector3.Z;
        }

        public override bool Equals(object obj)
        {
            if (obj is SignedVector3i)
            {
                SignedVector3i other = (SignedVector3i)obj;
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(SignedVector3i a, SignedVector3i b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(SignedVector3i a, SignedVector3i b)
        {
            return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public static SignedVector3i operator +(SignedVector3i a, SignedVector3i b)
        {
            return new SignedVector3i(a.X + b.X ,a.Y +b.Y ,a.Z + b.Z);
        }

        public static SignedVector3i operator *(SignedVector3i v, byte b)
        {
            return new SignedVector3i(v.X * b, v.Y * b, v.Z * b);
        }

        public static int DistanceSquared(SignedVector3i value1, SignedVector3i value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;
            int z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

        public override int GetHashCode()
        {
            //TODO check this hashcode impl           
            return (int)(X ^ Y ^ Z);
        }

        public override string ToString()
        {
            return ("SignedVector3i (" + X + "," + Y + "," + Z + ")");
        }

        public Vector3 asVector3()
        {
            return new Vector3(X, Y, Z);
        }

        

    }
}
