using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Terraria.Utilities;

namespace SpinningTopsMod.Content.Projectiles
{
    public class Oil : ModProjectile
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Ale); // Clone the defaults from the Oil Slick projectile
            Projectile.damage = 1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damage)
        {
            // Inflict Oiled debuff for 10 seconds (600 ticks)
            target.AddBuff(BuffID.Oiled, 600);
        }

        public override void OnKill(int timeLeft)
        {
            int splashRadiusTiles = 10 * 16;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    // Calculate distance from projectile center to NPC center
                    float dx = Projectile.Center.X - npc.Center.X;
                    float dy = Projectile.Center.Y - npc.Center.Y;
                    float distSqr = dx * dx + dy * dy;

                    if (distSqr <= splashRadiusTiles * splashRadiusTiles)
                    {
                        // NPC is within the circular splash radius
                        npc.AddBuff(BuffID.Oiled, 600); // Inflict Oiled debuff for 10 seconds (600 ticks)
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item27, Projectile.position); // ice break sound, sounds like shattered glass cointainer for oil

            Vector2 startPosition = Main.rand.NextVector2Circular(3f, 3f);
            for (int i = 0; i < 3; i++)
            {
                Dust glassdust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, Scale: 1.5f);
                glassdust.velocity = 2 * startPosition.RotatedBy(MathHelper.TwoPi / 3 * i);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust oildust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, newColor: Color.Black, Scale: 2.5f);
                oildust.velocity = 4.5f * startPosition.RotatedBy(MathHelper.TwoPi / 10 * i);
                oildust.alpha = 180;
            }

            base.OnKill(timeLeft);
        }

    }
}