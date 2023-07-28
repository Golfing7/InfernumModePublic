using CalamityMod;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Dragonfolly
{
    public class LightningSuperchargeTelegraph : ModProjectile
    {
        public Vector2[] ChargePositions = new Vector2[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public NPC ThingToAttachTo => Main.npc.IndexInRange((int)Projectile.ai[1]) ? Main.npc[(int)Projectile.ai[1]] : null;

        public PrimitiveTrail TelegraphDrawer;

        public const int Lifetime = 60;

        public const float TelegraphFadeTime = 15f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charge Telegraph");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChargePositions.Length);
            for (int i = 0; i < ChargePositions.Length; i++)
                writer.WriteVector2(ChargePositions[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChargePositions = new Vector2[reader.ReadInt32()];
            for (int i = 0; i < ChargePositions.Length; i++)
                ChargePositions[i] = reader.ReadVector2();
        }

        public override void AI()
        {
            // Determine the relative opacities for each player based on their distance.
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }

            // Die if the thing to attach to disappears.
            if (ThingToAttachTo is null || !ThingToAttachTo.active)
            {
                Projectile.Kill();
                return;
            }

            // Determine opacity.
            Projectile.Opacity = Utils.GetLerpValue(0f, 6f, Projectile.timeLeft, true) * Utils.GetLerpValue(Lifetime, Lifetime - 6f, Projectile.timeLeft, true);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, Projectile.alpha);
        }

        public Color TelegraphPrimitiveColor(float completionRatio)
        {
            float opacity = Lerp(0.38f, 1.2f, Projectile.Opacity);
            opacity *= CalamityUtils.Convert01To010(completionRatio);
            opacity *= Lerp(0.9f, 0.2f, Projectile.ai[0] / (ChargePositions.Length - 1f));
            if (completionRatio > 0.95f)
                opacity = 0.0000001f;
            return Color.Red * opacity;
        }

        public float TelegraphPrimitiveWidth(float completionRatio)
        {
            return Projectile.Opacity * 15f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (TelegraphDrawer is null)
                TelegraphDrawer = new PrimitiveTrail(TelegraphPrimitiveWidth, TelegraphPrimitiveColor, PrimitiveTrail.RigidPointRetreivalFunction, GameShaders.Misc["CalamityMod:Flame"]);

            GameShaders.Misc["CalamityMod:Flame"].UseImage1("Images/Misc/Perlin");
            GameShaders.Misc["CalamityMod:Flame"].UseSaturation(0.36f);

            for (int i = ChargePositions.Length - 2; i >= 0; i--)
            {
                Vector2[] positions = new Vector2[2]
                {
                    ChargePositions[i],
                    ChargePositions[i + 1]
                };

                // Stand-in variable used to differentiate between the beams.
                // It is not used anywhere else.
                Projectile.ai[0] = i;

                TelegraphDrawer.Draw(positions, Projectile.Size * 0.5f - Main.screenPosition, 55);
            }
            return false;
        }
    }
}
