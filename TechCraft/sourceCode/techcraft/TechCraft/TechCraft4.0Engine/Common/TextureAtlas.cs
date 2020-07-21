using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TechCraftEngine.Common
{
    public class TextureAtlas
    {
        public static RectangleF TextureCoords(int textureIndex, int atlasSize)
        {
            float bufferRatio = 0.0f;
            RectangleF r = new RectangleF();
            r.Y = (1.0f / atlasSize * (int)(textureIndex / atlasSize)) + ((bufferRatio) * (1.0f / atlasSize));
            r.X = (1.0f / atlasSize * (textureIndex % atlasSize)) + ((bufferRatio) * (1.0f / atlasSize));
            r.Width = (1f - 2f * bufferRatio) * 1.0f / atlasSize;
            r.Height = (1f - 2f * bufferRatio) * 1.0f / atlasSize;
            return r;
        }
    }
}
