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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NewTake.controllers;

using NewTake.model;
using NewTake.view.blocks;
using NewTake.model.types;
#endregion

namespace NewTake.view
{
    /*render player and his hands / tools / attached parts */
    public class PlayerRenderer
    {

        #region Fields
        public readonly Player player;
        private readonly Viewport viewport;
        public readonly FirstPersonCamera camera;
        private readonly FirstPersonCameraController cameraController;

        private Vector3 lookVector;

        private MouseState previousMouseState;

        private readonly GraphicsDevice GraphicsDevice;

        private PlayerPhysics physics;

        // SelectionBlock
        public Model SelectionBlock;
        BasicEffect _selectionBlockEffect;
        Texture2D SelectionBlockTexture;
        public bool freeCam;
        #endregion

        public PlayerRenderer(GraphicsDevice graphicsDevice, Player player)
        {
            this.GraphicsDevice = graphicsDevice;
            this.player = player;
            this.viewport = graphicsDevice.Viewport;
            this.camera = new FirstPersonCamera(viewport);
            this.cameraController = new FirstPersonCameraController(camera);
            physics = new PlayerPhysics(this);
        }

        public void Initialize()
        {

            camera.Initialize();
            camera.Position = new Vector3(World.origin * Chunk.SIZE.X, Chunk.SIZE.Y, World.origin * Chunk.SIZE.Z);
            player.position = camera.Position;
            camera.LookAt(Vector3.Down);

            cameraController.Initialize();

            // SelectionBlock
            _selectionBlockEffect = new BasicEffect(GraphicsDevice);
        }

        public void LoadContent(ContentManager content)
        {
            // SelectionBlock
            SelectionBlock = content.Load<Model>("Models\\SelectionBlock");
            SelectionBlockTexture = content.Load<Texture2D>("Textures\\SelectionBlock");
        }

        #region SelectionBlock
        public void RenderSelectionBlock(GameTime gameTime)
        {

            GraphicsDevice.BlendState = BlendState.NonPremultiplied; // allows any transparent pixels in original PNG to draw transparent

            if (!player.currentSelection.HasValue)
            {
                return;
            }

            //TODO why the +0.5f for rendering slection block ?
            Vector3 position = player.currentSelection.Value.position.asVector3() + new Vector3(0.5f, 0.5f, 0.5f);

            Matrix matrix_a, matrix_b;
            Matrix identity = Matrix.Identity;                       // setup the matrix prior to translation and scaling  
            Matrix.CreateTranslation(ref position, out matrix_a);    // translate the position a half block in each direction
            Matrix.CreateScale((float)0.51f, out matrix_b);          // scales the selection box slightly larger than the targetted block

            identity = Matrix.Multiply(matrix_b, matrix_a);          // the final position of the block

            // set up the World, View and Projection
            _selectionBlockEffect.World = identity;
            _selectionBlockEffect.View = camera.View;
            _selectionBlockEffect.Projection = camera.Projection;
            _selectionBlockEffect.Texture = SelectionBlockTexture;
            _selectionBlockEffect.TextureEnabled = true;

            // apply the effect
            foreach (EffectPass pass in _selectionBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DrawSelectionBlockMesh(GraphicsDevice, SelectionBlock.Meshes[0], _selectionBlockEffect);
            }

        }

        private void DrawSelectionBlockMesh(GraphicsDevice graphicsdevice, ModelMesh mesh, Effect effect)
        {
            int count = mesh.MeshParts.Count;
            for (int i = 0; i < count; i++)
            {
                ModelMeshPart parts = mesh.MeshParts[i];
                if (parts.NumVertices > 0)
                {
                    GraphicsDevice.Indices = parts.IndexBuffer;
                    //TODO better use DrawUserIndexedPrimitives for fully dynamic content
                    GraphicsDevice.SetVertexBuffer(parts.VertexBuffer);
                    graphicsdevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, parts.NumVertices, parts.StartIndex, parts.PrimitiveCount);
                }
            }
        }

        private float setPlayerSelectedBlock(bool waterSelectable)
        {
            for (float x = 0.5f; x < 8f; x += 0.1f)
            {
                Vector3 targetPoint = camera.Position + (lookVector * x);

                Block block = player.world.BlockAt(targetPoint);

                if (block.Type != BlockType.None && (waterSelectable || block.Type != BlockType.Water))
                {
                    player.currentSelection = new PositionedBlock(new Vector3i(targetPoint), block);
                    return x;
                }
                else
                {
                    player.currentSelection = null;
                    player.currentSelectedAdjacent = null;
                }
            }
            return 0;
        }

        private void setPlayerAdjacentSelectedBlock(float xStart)
        {
            for (float x = xStart; x > 0.7f; x -= 0.1f)
            {
                Vector3 targetPoint = camera.Position + (lookVector * x);
                Block block = player.world.BlockAt(targetPoint);

                //TODO smelly - check we really iterate here, and parametrize the type.none
                if (player.world.BlockAt(targetPoint).Type == BlockType.None)
                {
                    player.currentSelectedAdjacent = new PositionedBlock(new Vector3i(targetPoint), block);
                    break;
                }
            }
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            Matrix previousView = camera.View;

            if (freeCam)
            {
                cameraController.ProcessInput(gameTime);
                player.position = camera.Position;
            }

            cameraController.Update(gameTime);

            camera.Update(gameTime);

            //Do not change methods order, its not very clean but works fine
            if (!freeCam)
                physics.move(gameTime);

            //do not do this each tick
            if (!previousView.Equals(camera.View))
            {
                lookVector = camera.LookVector;
                lookVector.Normalize();

                bool waterSelectable = false;
                float x = setPlayerSelectedBlock(waterSelectable);
                if (x != 0) // x==0 is equivalent to payer.currentSelection == null
                {
                    setPlayerAdjacentSelectedBlock(x);
                }

            }

            MouseState mouseState = Mouse.GetState();

            int scrollWheelDelta = previousMouseState.ScrollWheelValue - mouseState.ScrollWheelValue;

            if (mouseState.RightButton == ButtonState.Pressed
             && previousMouseState.RightButton != ButtonState.Pressed)
            {
                player.RightTool.Use();                
            }
            
            if (mouseState.LeftButton == ButtonState.Pressed
             && previousMouseState.LeftButton != ButtonState.Pressed)
            {
                player.LeftTool.Use();                
            }

            player.RightTool.switchType(scrollWheelDelta);
            player.LeftTool.switchType(scrollWheelDelta);

            previousMouseState = Mouse.GetState();
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime)
        {
            //TODO draw the player / 3rd person /  tools

            if (player.currentSelection.HasValue)
            {
                RenderSelectionBlock(gameTime);
            }
        }
        #endregion

    }
}
