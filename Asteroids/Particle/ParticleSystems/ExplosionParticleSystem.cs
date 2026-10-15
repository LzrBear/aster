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
    class explosionParticleSystem : ParticleSystem
    {
        public explosionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/Particle004";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1.2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(0.9f, 0.5019608f, 0.0f);
            settings.MaxColor = new Color(1.1f, 0.5019608f, 0.0f);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5000;
            settings.MaxStartSize = 6000;

            settings.MinEndSize = 10000;
            settings.MaxEndSize = 12000;

            settings.BlendState = BlendState.Additive;

            // Always overdraw 3D models so fragments appear behind the explosion cloud.
            settings.DepthStencilState = DepthStencilState.None;
        }
    }
}
