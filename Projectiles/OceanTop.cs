using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static SpinningTopsMod.Utils;

namespace SpinningTopsMod.Content.Projectiles
{
    public class OceanTop : SpinningTopBase
    {
        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 10;
        protected override int MaxBounces => 20;
        protected override int DefaultLocalNPCHitCooldown => 14 * ud;
        protected override int DefaultTimeLeftSeconds => 15;
        protected override int DefualtTextureFrames => 4;
        protected override int DefualtTicksUntilNextFrame => 15;
        #endregion

        private bool inWater = false;
        private bool dryAbove = false;

        #region Initialization
        public override void SetDefaults()
        {
            ApplyDefaultDefaults();
            Projectile.ignoreWater = true;
        }
        #endregion

        #region AI
        protected override void TopAI()
        {
            Projectile.ai[1]++;

            // Check if the projectile is in water
            inWater = Collision.DrownCollision(Projectile.position, Projectile.width, Projectile.height);
            Vector2 above = Projectile.Center + new Vector2(0, -16);
            dryAbove = !Collision.DrownCollision(above, Projectile.width, Projectile.height);

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Main.rand.NextVector2CircularEdge(1f, 1f), 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
            }

            // Custom gravity for water
            if (!inWater)
            {
                Projectile.velocity.Y += 0.1f;
                if (Projectile.velocity.Y > 10f) { Projectile.velocity.Y = 10f; }
            }
            else if (inWater && dryAbove)
            {
                Projectile.velocity.Y -= 0.1f * 5f;
            }
            else if (inWater && !dryAbove)
            {
                Projectile.velocity.Y += 0.1f * 0.25f;
            }

            // Apply friction when not in water
            if (Projectile.ai[1] % FrequencyToFrames(8, ud) == 0 && !inWater)
            {
                Projectile.velocity.X *= 0.98f;
            }

            base.TopAI();
        }

        public override void PostAI()
        {
            if (inWater && !dryAbove)
            {
                // Create bubble effects when fully submerged
                if (Projectile.ai[1] % FrequencyToFrames(6, ud) == 0)
                {
                    Dust dustBubble = Dust.NewDustPerfect(Projectile.Center, DustID.BubbleBlock, Main.rand.NextVector2CircularEdge(1f, 1f), 100, default, 1.2f);
                    dustBubble.velocity *= 0.5f;
                    Vector3 DustColor = dustBubble.color.ToVector3();
                    Lighting.AddLight(Projectile.position, DustColor);
                }

                if (Projectile.ai[1] % FrequencyToFrames(2, ud) == 0)
                {
                    Projectile bubbleProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Main.rand.NextVector2CircularEdge(2, 4),
                        ProjectileID.Bubble, 6, 3f, Projectile.owner);
                    bubbleProj.velocity.Y = -Math.Abs(bubbleProj.velocity.Y);
                }
            }
        }
        #endregion

        #region Collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (blockedByWall(Projectile.velocity, oldVelocity))
            {
                Projectile.ai[2]++;
                if (Projectile.ai[2] >= MaxBounces)
                {
                    Projectile.Kill();
                }
                else
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.8f;
                }
            }

            return false;
        }
        #endregion

        #region Hit NPC
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage -= 0.5f;

            if (inWater)
            {
                modifiers.FinalDamage += 5;
            }
            else if (target.wet || target.HasBuff(BuffID.Wet))
            {
                modifiers.SetCrit();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= MaxHits)
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.velocity.Y = -3.5f;
                target.AddBuff(BuffID.Wet, 300);
            }
        }
        #endregion

        #region Kill Effects
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WoodFurniture, Vector2.Zero, 100, default, 1.5f);
                dust.position = Projectile.Center;
                dust.velocity = new Vector2(1, 0).RotatedBy(i * (2 * Math.PI / 10)) * 2f;
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
            }
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        }
        #endregion

        #region Rendering
       
        #endregion
    }

}