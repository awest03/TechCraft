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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using NewTake.view.blocks;
using NewTake.model;
using NewTake;
using NewTake.controllers;
#endregion

namespace NewTake.view
{
    public class HudRenderer
    {

        #region Fields

        #region minimap
        // Minimap
        SpriteBatch _spriteBatchmap;
        Texture2D MinimapTex;
        Color MinimapBGCol = new Color(150, 150, 150, 150);
        Color[] maptexture = new Color[Chunk.SIZE.X * Chunk.SIZE.Z];
        Rectangle MinimapBGRect = new Rectangle(650, 20, 64, 64);
        Rectangle BlockPos = new Rectangle(0, 0, 8, 8);
        #endregion

        GraphicsDevice GraphicsDevice;
        public readonly FirstPersonCamera _camera;
        public readonly World world;

        public bool showMinimap = false;

        // Crosshair
        private Texture2D _crosshairTexture;
        private SpriteBatch _spriteBatch;

        #endregion

        public HudRenderer(GraphicsDevice device, World world, FirstPersonCamera camera)
        {
            this.GraphicsDevice = device;
            this._camera = camera;
            this.world = world;
        }

        #region Initialize
        public void Initialize()
        {
            // Used for crosshair sprite/texture at the moment
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            #region Minimap
            _spriteBatchmap = new SpriteBatch(GraphicsDevice);
            MinimapTex = new Texture2D(GraphicsDevice, 1, 1);
            Color[] texcol = new Color[1];
            MinimapTex.GetData(texcol);
            texcol[0] = Color.White;
            MinimapTex.SetData(texcol);
            #endregion

        }
        #endregion

        public void LoadContent(ContentManager Content)
        {
            // Crosshair
            _crosshairTexture = Content.Load<Texture2D>("Textures\\crosshair");
        }

        #region generateMinimapTexture
        public void generateMinimapTexture()
        {
            uint x = (uint)_camera.Position.X;
            uint z = (uint)_camera.Position.Z;

            uint cx = x / Chunk.SIZE.X;
            uint cz = z / Chunk.SIZE.Z;

            Chunk chunk = world.Chunks[cx, cz];

            for (int xx = 0; xx < Chunk.SIZE.X; xx++)
            {
                for (int zz = 0; zz < Chunk.SIZE.Z; zz++)
                {
                    int offset = xx * Chunk.FlattenOffset + zz * Chunk.SIZE.Y;
                    for (int y = Chunk.MAX.Y; y > 0; y--)
                    {
                        BlockType blockcheck = chunk.Blocks[offset + y].Type;
                        if (blockcheck != BlockType.None)
                        {
                            int index = xx * (Chunk.SIZE.X) + zz;
                            switch (blockcheck)
                            {
                                case BlockType.Grass:
                                    maptexture[index] = new Color(0, y, 0);
                                    break;
                                case BlockType.Dirt:
                                    maptexture[index] = Color.Khaki;
                                    break;
                                case BlockType.Snow:
                                    maptexture[index] = new Color(y, y, y);
                                    break;
                                case BlockType.Sand:
                                    maptexture[index] = new Color(193 + (y / 2), 154 + (y / 2), 107 + (y / 2));
                                    break;
                                case BlockType.Water:
                                    maptexture[index] = new Color(0, 0, y + 64);
                                    break;
                                case BlockType.Leaves:
                                    maptexture[index] = new Color(0, 128, 0);
                                    break;
                                default:
                                    maptexture[index] = new Color(0, 0, 0);
                                    break;
                            }
                            y = 0;
                        }
                    }
                }
            }
        }
        #endregion

        #region Draw
        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Draw the crosshair
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(_crosshairTexture, new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - 10,
                (GraphicsDevice.Viewport.Height / 2) - 10), Color.White);
            _spriteBatch.End();

            #region minimap
            if (showMinimap)
            {
                generateMinimapTexture();
                _spriteBatchmap.Begin();
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        BlockPos.X = i * 8 + 650;
                        BlockPos.Y = j * 8 + 20;
                        _spriteBatchmap.Draw(MinimapTex, BlockPos, this.maptexture[i * Chunk.SIZE.X + j]);
                    }
                }
                _spriteBatchmap.End();
            }
            #endregion

        }
        #endregion

    }
}
