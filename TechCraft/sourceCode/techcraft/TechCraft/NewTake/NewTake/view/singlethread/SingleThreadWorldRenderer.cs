using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;

namespace NewTake.view
{
    class SingleThreadWorldRenderer : WorldRenderer
    {

        public SingleThreadWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) : 
            base (graphicsDevice,  camera,  world) { }

        protected override void postConstruct(){
            //I just extracted the thread stuff here in base class
        }

        public override void initRendererAction(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            ChunkRenderer cRenderer = new SolidBoundsChunkRenderer(GraphicsDevice, world, chunk);
            ChunkRenderers.Add(chunk.Index,cRenderer);
        }

        #region Update
        public override void Update(GameTime gameTime)
        {

            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;    // This is the chunk in which the camera currently resides

            // Loop through all possible chunks around the camera in both X and Z directions
            for (uint j = cx - (World.VIEW_DISTANCE_FAR_X + 1); j < cx + (World.VIEW_DISTANCE_FAR_X + 1); j++)
            {
                for (uint l = cz - (World.VIEW_DISTANCE_FAR_Z + 1); l < cz + (World.VIEW_DISTANCE_FAR_Z + 1); l++)
                {
                    int distancecx = (int)(cx - j);        // The distance from the camera to the chunk in the X direction
                    int distancecz = (int)(cz - l);        // The distance from the camera to the chunk in the Z direction

                    if (distancecx < 0) distancecx = 0 - distancecx;        // If the distance is negative (behind the camera) make it positive
                    if (distancecz < 0) distancecz = 0 - distancecz;        // If the distance is negative (behind the camera) make it positive

                    // Remove Chunks
                    if ((distancecx > World.VIEW_DISTANCE_NEAR_X) || (distancecz > World.VIEW_DISTANCE_NEAR_Z))
                    {
                        if ((world.viewableChunks[j, l] != null)) // Chunk is created, therefore remove
                        {
                            Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                            Chunk chunk = world.viewableChunks[j, l];
                            chunk.visible = false;
                            world.viewableChunks[j, l] = null;
                            ChunkRenderers.Remove(newIndex);
                            //Debug.WriteLine("Removed chunk at {0},{1},{2}", chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
                        }
                        else
                        {
                            //    Debug.WriteLine("[Remove] chunk not found at at {0},0,{1}", j, l);
                        }
                    }
                    // Build Chunks
                    else if ((distancecx > World.VIEW_CHUNKS_X) || (distancecz > World.VIEW_CHUNKS_Z))
                    {
                        if (world.viewableChunks[j, l] == null) // Chunk is not created, therefore create
                        {
                            Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                            Chunk toAdd = new Chunk(newIndex);
                            world.viewableChunks[newIndex.X, newIndex.Z] = toAdd;
                            world.builder.build(toAdd);
                            //Debug.WriteLine("Built chunk at {0},{1},{2}", toAdd.Position.X, toAdd.Position.Y, toAdd.Position.Z);
                        }
                    }
                    // Build Vertices
                    else
                    {
                        Chunk chunk = world.viewableChunks[j, l];

                        if ((!chunk.built) && (chunk.generated)) // Chunk is generated but vertices not built. Therefore build the vertices
                        {
                            Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                            initRendererAction(newIndex);
                            chunk.built = true;
                            //Debug.WriteLine("Built vertices at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                        }


                    }

                }
            }



            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                if (chunkRenderer.isInView(viewFrustum))
                {
                    chunkRenderer.update(gameTime);
                }
            }

        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            //currently a copy paste of base class but currently only :)

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            GraphicsDevice.Clear(Color.SkyBlue);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.BlendState = BlendState.NonPremultiplied; // Allows transparency in leaves
            GraphicsDevice.BlendState = BlendState.Opaque;        // Removes transparency in leaves
            
            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["AmbientIntensity"].SetValue(0.6f);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
                {
                    if (chunkRenderer.isInView(viewFrustum))
                    {
                       chunkRenderer.draw(gameTime);
                    }
                }
            }

        }
        #endregion

    }
}
