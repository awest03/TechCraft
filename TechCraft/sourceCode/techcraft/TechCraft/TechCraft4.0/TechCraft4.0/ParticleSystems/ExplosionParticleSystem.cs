using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TechCraftEngine;
using TechCraftEngine.Particles;

namespace TechCraft.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(TechCraftGame game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "explosion";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -5f;
            settings.MaxRotateSpeed = 5f;

            settings.MinStartSize = 0.1f;
            settings.MaxStartSize = 0.1f;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 5;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
