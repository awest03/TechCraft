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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
#endregion

namespace NewTake.view.renderers
{
    public class SkyDomeRenderer : IRenderer
    {

        #region Fields
        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        #region Atmospheric settings
        
        
        //TODO accord with ThreadedWorldRenderer fog constants

        public const float FARPLANE = 220 * 4;
        public const int FOGNEAR = 200 * 4;
        public const int FOGFAR = 220 * 4;

        protected Vector3 SUNCOLOR = Color.White.ToVector3();

        protected Vector4 OVERHEADSUNCOLOR = Color.DarkBlue.ToVector4();
        protected Vector4 NIGHTCOLOR = Color.Black.ToVector4();

        protected Vector4 FOGCOLOR = Color.White.ToVector4();
        protected Vector4 HORIZONCOLOR = Color.White.ToVector4();

        protected Vector4 EVENINGTINT = Color.Red.ToVector4();
        protected Vector4 MORNINGTINT = Color.Gold.ToVector4();

        protected float CLOUDOVERCAST = 1.0f;

        protected const bool cloudsEnabled = true;

        #region SkyDome and Clouds
        // SkyDome
        protected Model skyDome;
        protected Matrix projectionMatrix;
        protected Texture2D cloudMap;
        protected Texture2D starMap;
        protected float rotationClouds;
        protected float rotationStars;

        // GPU generated clouds
        protected Texture2D cloudStaticMap;
        protected RenderTarget2D cloudsRenderTarget;
        protected Effect _perlinNoiseEffect;
        protected VertexPositionTexture[] fullScreenVertices;

        private float _tod;
        #endregion
        #endregion

        #endregion

        public SkyDomeRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            #region SkyDome and Clouds
            // SkyDome
            skyDome = content.Load<Model>("Models\\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = content.Load<Effect>("Effects\\SkyDome");
            cloudMap = content.Load<Texture2D>("Textures\\cloudMap");
            starMap = content.Load<Texture2D>("Textures\\newStars");

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);

            // GPU Generated Clouds
            if (cloudsEnabled)
            {
                _perlinNoiseEffect = content.Load<Effect>("Effects\\PerlinNoise");
                PresentationParameters pp = _graphicsDevice.PresentationParameters;
                //the mipmap does not work on some pc ( i5 laptops at least), with mipmap false it s fine 
                cloudsRenderTarget = new RenderTarget2D(_graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                cloudStaticMap = CreateStaticMap(32);
                fullScreenVertices = SetUpFullscreenVertices();
            }
            #endregion
        }

        #region Generate Clouds
        public virtual Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(_graphicsDevice, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        public virtual VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        public virtual void GeneratePerlinNoise(float time)
        {
            _graphicsDevice.SetRenderTarget(cloudsRenderTarget);
            //_graphicsDevice.Clear(Color.White);

            _perlinNoiseEffect.CurrentTechnique = _perlinNoiseEffect.Techniques["PerlinNoise"];
            _perlinNoiseEffect.Parameters["xTexture"].SetValue(cloudStaticMap);
            _perlinNoiseEffect.Parameters["xOvercast"].SetValue(CLOUDOVERCAST);
            _perlinNoiseEffect.Parameters["xTime"].SetValue(time / 1000.0f);

            foreach (EffectPass pass in _perlinNoiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }

            _graphicsDevice.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;
        }
        #endregion

        public void Stop()
        {
            //_running = false;
        }

        #region Update
        public void Update(GameTime gameTime)
        {
            if (cloudsEnabled)
            {
                // Generate the clouds
                float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                GeneratePerlinNoise(time);
            }
            //update of chunks is handled in chunkReGenBuildTask for this class
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime)
        {

            _graphicsDevice.RasterizerState = !_world._wireframed ? _world._normalRaster : _world._wireframedRaster;

            Matrix currentViewMatrix = _camera.View;

            _tod = _world.tod;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            rotationStars += 0.0001f;
            rotationClouds = 0;

            // Stars
            Matrix wStarMatrix = Matrix.CreateRotationY(rotationStars) * Matrix.CreateTranslation(0, -0.1f, 0) * Matrix.CreateScale(110) * Matrix.CreateTranslation(_camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wStarMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyStarDome"];

                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(starMap);
                    currentEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);
                    currentEffect.Parameters["SunColor"].SetValue(OVERHEADSUNCOLOR);
                    currentEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);

                    currentEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
                    currentEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);
                    currentEffect.Parameters["timeOfDay"].SetValue(_tod);
                }
                mesh.Draw();
            }

            // Clouds
            Matrix wMatrix = Matrix.CreateRotationY(rotationClouds) * Matrix.CreateTranslation(0, -0.1f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(_camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];

                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);
                    currentEffect.Parameters["SunColor"].SetValue(OVERHEADSUNCOLOR);
                    currentEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);

                    currentEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
                    currentEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);
                    currentEffect.Parameters["timeOfDay"].SetValue(_tod);
                }
                mesh.Draw();
            }

        }
        #endregion

    }
}
