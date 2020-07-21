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
    /// Custom particle system for creating a flame effect.
    /// </summary>
    public class BazookaSmokeParticleSystem : ParticleSystem
    {
        public BazookaSmokeParticleSystem(TechCraftGame game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "bazookasmoke";

            settings.MaxParticles = 3000;

            settings.Duration = TimeSpan.FromSeconds(0.2f);

            settings.DurationRandomness = 0;

            settings.MinHorizontalVelocity = 0f;
            settings.MaxHorizontalVelocity = 0f;

            settings.MinVerticalVelocity = 0f;
            settings.MaxVerticalVelocity = 0f;

            // Set gravity upside down, so the flames will 'fall' upward.
            //settings.Gravity = new Vector3(0, 0.5f, 0);
            //settings.MinColor = new Color(255, 255, 255, 20);
            //settings.MaxColor = new Color(255, 255, 255, 60);

            settings.MaxRotateSpeed = 0.1f;
            settings.MinRotateSpeed = 1f;

            settings.MinStartSize = 3f;
            settings.MaxStartSize = 3f;

            settings.MinEndSize = 1f;
            settings.MaxEndSize = 1f;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;

            //settings.MinColor = new Color(64, 96, 128, 255);
            //settings.MaxColor = new Color(255, 255, 255, 128);
        }
    }
}
