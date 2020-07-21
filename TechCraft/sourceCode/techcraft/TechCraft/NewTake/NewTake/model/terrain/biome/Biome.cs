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

namespace NewTake.model.terrain.biome
{

    #region enum BiomeType
    public enum BiomeType : byte
    {

        None = 0,
    
        //  Tundra 
        Tundra_Alpine = 1,
        Tundra_Artic = 2,
    
        //  Grassland
        Grassland_Savanna = 3,
        Grassland_Temperate = 4,
    
        //  Forest
        Forest_Tropical = 5,
        Forest_Temperate = 6,
        Forest_Taiga = 7,
    
        //  Desert
        Desert_Subtropical = 8,
        Desert_Semiarid = 9,
        Desert_Coastal = 10,
        Desert_Cold = 11,
    
        //  Marine
        Marine_Ocean = 12,
        Marine_CoralReef = 13,
        Marine_Estuary = 14,
    
        //  Freshwater
        Freshwater_Lake = 15,
        Freshwater_River = 16,
        Freshwater_Wetland = 17,
    
        Custom = 18,
        MAXIMUM = 19

    }
    #endregion

    public class Biome
    {

        public byte temperature_lowest      { get; set; }
        public byte temperature_highest     { get; set; }

        public byte rainfall_lowest         { get; set; }
        public byte rainfall_highest        { get; set; }

        public BlockType treetype           { get; set; }
        public BlockType topgroundblocktype { get; set; }
        public BlockType watertype          { get; set; }


        public Biome()
        {

        }
    } 
}
