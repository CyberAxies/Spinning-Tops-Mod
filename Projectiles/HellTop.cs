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
    public class HellTop : SpinningTopBase
    {
        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 15;
        protected override int MaxBounces => 10;
        protected override int DefaultLocalNPCHitCooldown => 20;
        protected override float Gravity => 0.1f;
        protected override float MaxFallSpeed => 9.82f;
        protected override int DefaultTimeLeftSeconds => 30;
        protected override int DefualtTextureFrames => 4;
        protected override int DefualtTicksUntilNextFrame => 20;
        protected override float DefualtArcHeightMult => 1.5f;
        #endregion

        #region Initialization
        public override void SetDefaults()
        {
            ApplyDefaultDefaults();
        }
        #endregion

        #region AI
        protected override void TopAI()
        {
            Projectile.ai[1]++;

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Lava, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
                dust.noGravity = false;
            }

            base.TopAI();
        }
        #endregion

        #region Collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            Projectile.ai[2]++;
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            if (Projectile.ai[2] >= MaxBounces)
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.velocity.Y = oldVelocity.Y > 1 ? -oldVelocity.Y * 0.8f: -2f;

                if (blockedByWall(Projectile.velocity, oldVelocity))
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.8f;
                }
            }

            // Spawn fire projectile on collision
            Projectile fire = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                Projectile.Bottom,
                Vector2.Zero,
                ProjectileID.GreekFire1,
                Projectile.damage,
                0f,
                Projectile.owner);
            fire.timeLeft = secondsToFrames(2.5f);
            fire.friendly = true;
            fire.hostile = false;
            fire.scale = 0.75f;
            fire.usesIDStaticNPCImmunity = true;
            fire.idStaticNPCHitCooldown = 20;

            return false;
        }
        #endregion

        #region Hit NPC
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= MaxHits)
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.8f;
                target.AddBuff(BuffID.OnFire, secondsToFrames(5));
                target.AddBuff(BuffID.Burning, secondsToFrames(1));
                SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
            }
        }
        #endregion

        #region Kill Effects
        public override void OnKill(int timeLeft)
        {
            DustExplosion(Projectile.Center, DustID.Obsidian, 10, 0.5f, 3f, dustScale: 1.5f);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        }
        #endregion

        #region Rendering
        public override bool PreDraw(ref Color lightColor)
        {
            base.PreDraw(ref lightColor);

            return false;
        }
        #endregion
    }

}