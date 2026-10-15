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
    class smokeParticleSystem : ParticleSystem
    {
        public smokeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/Cloud001";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(2);
            //settings.DurationRandomness = 2;

            settings.MinHorizontalVelocity = -700;
            settings.MaxHorizontalVelocity = 700;

            settings.MinVerticalVelocity = -700;
            settings.MaxVerticalVelocity = 700;

            // Create a wind effect by tilting the gravity vector sideways.
            //settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);

            //settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 2500;
            settings.MaxStartSize = 3000;

            settings.MinEndSize = 2500;
            settings.MaxEndSize = 3000;

            // Always overdraw 3D models so fragments appear behind the explosion cloud.
            settings.DepthStencilState = DepthStencilState.None;
        }
    }
}
