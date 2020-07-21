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

namespace TechCraftEngine.Managers
{
    public class BillboardContainer
    {        
        private VertexBuffer _vertexBuffer;
        private BoundingSphere _boundingSphere;
        private Texture2D _texture;

        private List<VertexPositionNormalTexture> _vertices;
    }

    public class BillboardManager : Manager
    {
        private VertexDeclaration _vertexDeclaration;
        private VertexPositionNormalTexture[] _quadVertices;

        public BillboardManager(TechCraftGame game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            //_vertexDeclaration = new VertexDeclaration(Game.GraphicsDevice, VertexPositionNormalTexture.VertexElements);
        }

        public override void LoadContent()
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }

}
