using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TechCraftEngine;
using TechCraftEngine.Common;

namespace TechCraftEngine.Rendering
{
    public class CubeRenderer
    {
        private TechCraftGame _game;
        //private VertexDeclaration _vertexDeclaration;
        private VertexBuffer _vertexBuffer;
        private List<VertexPositionColor> _vertexList;
        private Effect _effect;
        private Vector3[] _cubeNormals = {
                                             new Vector3(0,0,1),
                                             new Vector3(0,0,-1),
                                             new Vector3(1,0,0),
                                             new Vector3(-1,0,0),
                                             new Vector3(0,1,0),
                                             new Vector3(0,-1,0)
                                         };
        public CubeRenderer(TechCraftGame game)
        {
            _game = game;
            _vertexList = new List<VertexPositionColor>();
            //_vertexDeclaration = new VertexDeclaration( VertexPositionColor.VertexElements);
        }

        public void LoadContent()
        {
            _effect = _game.Content.Load<Effect>("Effects\\CubeEffect");
        }

        public void AddCube(Vector3 position, float size, Color color)
        {
            for (int x = 0; x < _cubeNormals.Length; x++)
            {
                Vector3 normal = _cubeNormals[x];

                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

                _vertexList.Add(new VertexPositionColor(position + ((normal - side1 - side2) * size / 2), color));
                _vertexList.Add(new VertexPositionColor(position + ((normal - side1 + side2) * size / 2), color));
                _vertexList.Add(new VertexPositionColor(position + ((normal + side1 + side2) * size / 2), color));
                _vertexList.Add(new VertexPositionColor(position + ((normal - side1 - side2) * size / 2), color));
                _vertexList.Add(new VertexPositionColor(position + ((normal + side1 + side2) * size / 2), color));
                _vertexList.Add(new VertexPositionColor(position + ((normal + side1 - side2) * size / 2), color));
            }
        }

        public void Clear()
        {
            _vertexList.Clear();
        }

        public void Build()
        {
            //_vertexBuffer = new VertexBuffer(_game.GraphicsDevice, _vertexList.Count * VertexPositionColor.SizeInBytes, BufferUsage.WriteOnly);
            _vertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor), _vertexList.Count, BufferUsage.WriteOnly);

            _vertexBuffer.SetData<VertexPositionColor>(_vertexList.ToArray());
        }

        public void Draw(GameTime gameTime)
        {
            if (_vertexList.Count > 0)
            {
                //_vertexBuffer = new VertexBuffer(_game.GraphicsDevice, _vertexList.Count * VertexPositionColor.SizeInBytes, BufferUsage.WriteOnly);
                _vertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPositionColor), _vertexList.Count, BufferUsage.WriteOnly);



                _vertexBuffer.SetData<VertexPositionColor>(_vertexList.ToArray());


                //_game.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
                
                /*_game.GraphicsDevice.RenderState.CullMode = CullMode.None;
                _game.GraphicsDevice.RenderState.DepthBufferEnable = true;
                _game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                _game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                */

                _game.GraphicsDevice.BlendState = BlendState.Opaque;
                _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                _effect.Parameters["World"].SetValue(Matrix.Identity);
                _effect.Parameters["View"].SetValue(_game.Camera.View);
                _effect.Parameters["Projection"].SetValue(_game.Camera.Projection);

                //effect.Begin();
                //_effect.CurrentTechnique.Passes[0].Begin();
                _effect.CurrentTechnique.Passes[0].Apply();

                //_game.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionColor.SizeInBytes);
                _game.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                
                //_game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, (_vertexBuffer.SizeInBytes / VertexPositionColor.SizeInBytes) / 3);
                _game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3);

                //_effect.CurrentTechnique.Passes[0].End();
                //_effect.End();
            }
        }
    }
}
