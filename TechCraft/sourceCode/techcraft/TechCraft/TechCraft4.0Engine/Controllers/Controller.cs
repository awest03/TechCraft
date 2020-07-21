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

namespace TechCraftEngine.Controllers
{
    public abstract class Controller
    {
        private TechCraftGame _game;

        public Controller(TechCraftGame game)
        {
            _game = game;
        }

        public TechCraftGame Game
        {
            get { return _game; }
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update(GameTime gameTime)
        {

        }
    }
}
