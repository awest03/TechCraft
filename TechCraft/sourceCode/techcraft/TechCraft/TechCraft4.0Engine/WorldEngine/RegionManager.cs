using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace TechCraftEngine.WorldEngine
{

    public class RegionManager
    {

        private Region _region;

        public RegionManager(Region region)
        {
            _region = region;
        }
        

        public void Flush(bool clear)
        {
#if !XBOX
            //Debug.WriteLine("Flushing " + GetFilename());
            FileStream fs = File.Open(GetFilename(_region.Position), FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            Save(writer);
            writer.Flush();
            fs.Close();
#endif
            if (clear)
            {
                _region.Clear();
            }
        }


        public void Load(Vector3i worldPosition)
        {
            string filename = GetFilename(worldPosition);


            //Debug.WriteLine("Loading " + filename);

#if XBOX
            if (_graphicsManager.ContentAvailable(filename))
            {
                Stream fs = TitleContainer.OpenStream("Content\\" + filename);
#else
            if (File.Exists(filename))
            {
                FileStream fs = File.Open(filename, FileMode.Open);
#endif
                BinaryReader reader = new BinaryReader(fs);
                Load(reader);
                reader.Close();
                fs.Close();
            }
            else
            {
                Debug.WriteLine("New " + filename);
                _region.Clear();

            }
        }

        public void Save(BinaryWriter writer)
        {
            //writer.Write(_regionPosition.X);
            //writer.Write(_regionPosition.Y);
            //writer.Write(_regionPosition.Z);
            //writer.Write(_size.X);
            //writer.Write(_size.Y);
            //writer.Write(_size.Z);

            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.REGIONHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                    {
                        writer.Write((byte)_region.Blocks[x, y, z]);
                    }
                }
            }
        }

        public void Load(BinaryReader reader)
        {
            //_regionPosition = new Vector3((int)reader.ReadDouble(), (int)reader.ReadDouble(), (int)reader.ReadDouble());
            //_size = new Vector3((int)reader.ReadDouble(), (int)reader.ReadDouble(), (int)reader.ReadDouble());
            for (int x = 0; x < WorldSettings.REGIONWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.REGIONHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.REGIONLENGTH; z++)
                    {
                        _region.Blocks[x, y, z] = (BlockType) reader.ReadByte();
                    }
                }
            }
            //_dirty = true;


            //Debug.WriteLine(string.Format("Loaded {0}-{1}-{2}", (int) _regionPosition.X, (int) _regionPosition.Y, (int) _regionPosition.Z));
        }


        public string GetFilename(Vector3i position)
        {
            return string.Format("{0}{1}-{2}-{3}", WorldSettings.LEVELFOLDER, position.x, position.y, position.z);
        }


    }
}
