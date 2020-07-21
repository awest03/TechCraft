using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TechCraftEngine.WorldEngine
{
    [Serializable]
    public struct VertexPositionTextureShade : IVertexType
    {
        Vector3 _position;
        Vector2 _textureCoordinate;
        float _shade;
       
        public static readonly VertexElement[] VertexElements = new VertexElement[]
        { 
           /* new VertexElement(0,0,VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0,sizeof(float)*5,VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1)               
        */
        
            new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3,VertexElementFormat.Vector2,  VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*5,VertexElementFormat.Single,  VertexElementUsage.TextureCoordinate, 1)               
        
          
             };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

       
   
        public VertexPositionTextureShade(Vector3 position, Vector2 textureCoordinate, float shade)
        {
            _position = position;
            _textureCoordinate = textureCoordinate;
            _shade = shade;
           // _vertexDeclaration = new VertexDeclaration(VertexPositionTextureShade.VertexElements);
        }

        public Vector3 Position { get { return _position; } set { _position = value; } }
        public Vector2 TextureCoordinate { get { return _textureCoordinate; } set { _textureCoordinate = value; } }
        public float Shade { get { return _shade; } set { _shade = value; } }
        public static int SizeInBytes { get { return sizeof(float) * 6; } }
    }
}
