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
    public class SnowParticleSystem : ParticleSystem
    {
        public SnowParticleSystem(TechCraftGame game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "snowflake";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(4f);

            settings.DurationRandomness = 0;

            settings.MinHorizontalVelocity = -0.1f;
            settings.MaxHorizontalVelocity = 0.1f;

            settings.MinVerticalVelocity = -0.1f;
            settings.MaxVerticalVelocity = -0.5f;

            // Set gravity upside down, so the flames will 'fall' upward.
            //settings.Gravity = new Vector3(0, 0.5f, 0);

            //settings.MinColor = new Color(255, 255, 255, 20);
            //settings.MaxColor = new Color(255, 255, 255, 60);

            settings.MaxRotateSpeed = 0f;
            settings.MinRotateSpeed = 0f;

            settings.MinStartSize = 0.2f;
            settings.MaxStartSize = 0.2f;

            settings.MinEndSize = 0.2f;
            settings.MaxEndSize = 0.2f;

            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
        }
    }
}
