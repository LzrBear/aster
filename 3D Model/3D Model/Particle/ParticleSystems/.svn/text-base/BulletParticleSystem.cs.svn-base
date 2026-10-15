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
    class bulletParticleSystem : ParticleSystem
    {
        public bulletParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/Particle004";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(0.6);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(0f, 0f, 1f);;
            settings.MaxColor = new Color(0.5019608f, 0f, 0.5019608f);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 2000;
            settings.MaxStartSize = 2400;

            settings.MinEndSize = 200;
            settings.MaxEndSize = 400;

            settings.BlendState = BlendState.Additive;
        }
    }
}
