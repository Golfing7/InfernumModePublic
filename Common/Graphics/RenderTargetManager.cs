﻿using InfernumMode.Common.Graphics.ScreenEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Common.Graphics
{
    public class RenderTargetManager : ModSystem
    {
        internal static List<ManagedRenderTarget> ManagedTargets = new();

        public delegate void RenderTargetUpdateDelegate();

        public static event RenderTargetUpdateDelegate RenderTargetUpdateLoopEvent;

        /// <summary>
        /// How many frames should pass since a target was last accessed before automatically disposing of it.
        /// </summary>
        public const int TimeBeforeAutoDispose = 600;

        internal static void ResetTargetSizes(Vector2 obj)
        {
            foreach (ManagedRenderTarget target in ManagedTargets)
            {
                // Don't attempt to recreate targets that are already initialized or shouldn't be recreated.
                if (target is null || target.IsDisposed || target.WaitingForFirstInitialization)
                    continue;

                ScreenSaturationBlurSystem.DrawActionQueue.Enqueue(() =>
                {
                    target.Recreate((int)obj.X, (int)obj.Y);
                });
            }
        }

        internal static void DisposeOfTargets()
        {
            if (ManagedTargets is null)
                return;

            Main.QueueMainThreadAction(() =>
            {
                foreach (ManagedRenderTarget target in ManagedTargets)
                    target?.Dispose();
                ManagedTargets.Clear();
            });
        }

        public static RenderTarget2D CreateScreenSizedTarget(int screenWidth, int screenHeight) =>
            new(Main.instance.GraphicsDevice, screenWidth, screenHeight, true, SurfaceFormat.Color, DepthFormat.Depth24, 2, RenderTargetUsage.DiscardContents);

        public override void OnModLoad()
        {
            ManagedTargets = new();
            Main.OnPreDraw += HandleTargetUpdateLoop;
            Main.OnResolutionChanged += ResetTargetSizes;
        }

        public override void OnModUnload()
        {
            DisposeOfTargets();
            Main.OnPreDraw -= HandleTargetUpdateLoop;
        }

        private void HandleTargetUpdateLoop(GameTime obj)
        {
            // Auto dispose of targets that havent been used in a while, to stop them hogging GPU memory.
            if (ManagedTargets != null)
            {
                foreach (ManagedRenderTarget target in ManagedTargets)
                {
                    if (target == null || target.IsDisposed || !target.ShouldAutoDispose)
                        continue;

                    if (target.TimeSinceLastAccessed >= TimeBeforeAutoDispose)
                        target.Dispose();
                    else
                        target.TimeSinceLastAccessed++;
                }
            }
            RenderTargetUpdateLoopEvent?.Invoke();
        }
    }
}
