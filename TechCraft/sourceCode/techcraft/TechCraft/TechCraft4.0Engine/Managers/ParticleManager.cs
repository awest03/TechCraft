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

using TechCraftEngine.Particles;

namespace TechCraftEngine.Managers
{
    public class ParticleManager : Manager
    {
        private List<ParticleEmitter> _particleEmitters;
        private List<ParticleSystem> _particleSystems;

        public ParticleManager(TechCraftGame game)
            : base(game)
        {
            _particleEmitters = new List<ParticleEmitter>();
            _particleSystems = new List<ParticleSystem>();
        }

        public List<ParticleSystem> ParticleSystems
        {
            get { return _particleSystems; }
        }

        public List<ParticleEmitter> ParticleEmitters
        {
            get { return _particleEmitters; }
        }

        public override void Initialize()
        {           
        }

        public override void LoadContent()
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            for (int x = 0; x < _particleEmitters.Count; x++)
            {
                _particleEmitters[x].Update(gameTime, _particleEmitters[x].Position);
            }
            for (int x = 0; x < _particleSystems.Count;x++)
            {
                _particleSystems[x].SetCamera(Game.Camera.View, Game.Camera.Projection);
                _particleSystems[x].Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            for (int x = 0; x < _particleSystems.Count; x++)
            {
                _particleSystems[x].Draw(gameTime);
            }
        }
    }
}
