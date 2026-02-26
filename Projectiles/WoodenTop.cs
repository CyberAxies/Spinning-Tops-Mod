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
    public class WoodenTop : SpinningTopBase
    {
        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 5;
        protected override int MaxBounces => 20;
        protected override int DefaultLocalNPCHitCooldown => 10;
        protected override int DefaultTimeLeftSeconds => 10;
        protected override int DefualtTextureFrames => 2;
        protected override float DefualtArcHeightMult => 2.0f;
        protected override float MaxFallSpeed => 12f;
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
            base.TopAI();
            Projectile.ai[1]++; // Times the AI runs
        }
        #endregion

        #region  Collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {   
            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (blockedByWall(Projectile.velocity, oldVelocity))
            {
                Projectile.ai[2]++;
                if(Projectile.ai[2] >= MaxBounces)
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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= MaxHits)
                Projectile.Kill();
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.8f;
            }
        }
        #endregion
    }

}