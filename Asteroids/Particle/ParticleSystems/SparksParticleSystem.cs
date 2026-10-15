#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Particle3DSample
{
    /// <summary>
    /// Custom particle system for creating a small ball of fire.
    /// </summary>
    class sparksParticleSystem : ParticleSystem
    {
        public sparksParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            Random rand = new Random();
            settings.TextureName = "Textures/Particle005";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1f;

            settings.MinHorizontalVelocity = -2500;
            settings.MaxHorizontalVelocity = 2500;

            settings.MinVerticalVelocity = -2500;
            settings.MaxVerticalVelocity = 2500;

            // Create a wind effect by tilting the gravity vector sideways.
            //settings.Gravity = new Vector3((float)rand.NextDouble() * 10000, (float)rand.NextDouble() * 10000, (float)rand.NextDouble() * 10000);

            settings.MinColor = new Color(1f, 0.8784314f, 0.7529412f);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 150;
            settings.MaxStartSize = 350;

            settings.MinEndSize = 150;
            settings.MaxEndSize = 350;

            settings.BlendState = BlendState.Additive;
        }
    }
}
