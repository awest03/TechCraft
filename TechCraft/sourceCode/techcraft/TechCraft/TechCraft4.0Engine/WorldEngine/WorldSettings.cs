using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using TechCraftEngine;

namespace TechCraftEngine.WorldEngine
{
    public class WorldSettings
    {
        private const float TEXTURESIZE = 256f;

        public const int SEED = 56;
        
        public const int MAPWIDTH = 16*13;
        public const int MAPHEIGHT = 128;
        public const int MAPLENGTH = 16*13;
        public const int FOGNEAR = 90 * 4;
        public const int FOGFAR = 140 * 4;
        public const int FARPLANE = 140 * 4;

        public const int REGIONWIDTH = 16;
        public const int REGIONHEIGHT = 128;
        public const int REGIONLENGTH = 16;

        public const int SNOWLINE = 110;
        public const int SEALEVEL = 50;
     
        public const byte MAXLIGHT = 10;
        public const byte MINLIGHT = 1;
        public const float SIDESHADOWS = 0.2f;
        public const int WATERFLOWDISTANCE = 10;
        public const int LAVAFLOWDISTANCE = 10;

        public const int MAXPROJECTILES = 500;
        public const float PROJECTILEGRAVITY = -10f;

        public const float PLAYERGRAVITY = -15f;
        public const float PLAYERMOVESPEED = 3.5f;
        public const float PLAYERJUMPVELOCITY = 6f;
        public const float PLAYERSWIMVELOCITY = 4f;

        public const int MISSILEEXPLOSIONRADIUS = 2;
        public const int MAXGAMERS = 16;
        public const int TEXTUREATLASSIZE = 8;

        private Texture2D _textureSet;
        private VertexDeclaration _vertexDeclaration;
        private TechCraftGame _game;

        public const String LEVELFOLDER = "c:\\";

        public WorldSettings(TechCraftGame game, Texture2D textureSet)
        {
            _textureSet = textureSet;
            _game = game;
            //_vertexDeclaration = new VertexDeclaration(_game.GraphicsDevice, VertexPositionTextureShade.VertexElements);
            _vertexDeclaration = new VertexDeclaration( VertexPositionTextureShade.VertexElements);
        
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return _vertexDeclaration; }
        }

        public Texture2D TextureSet
        {
            get { return _textureSet; }
        }

        public Vector2 GetTextureCoords(int x, int y)
        {
            return new Vector2((TEXTURESIZE / 8f) * x, (TEXTURESIZE / 8f) * y);
        }
    }
}
