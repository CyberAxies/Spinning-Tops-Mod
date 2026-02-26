using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static SpinningTopsMod.Utils;

namespace SpinningTopsMod.Content.Projectiles
{
    public class SnowTop : SpinningTopBase
    {

        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 20;
        protected override int MaxBounces => 20;
        protected override float DefaultScale => 0.375f;
        protected override int DefaultTimeLeftSeconds => 10;
        protected override int DefualtTextureFrames => 4;
        #endregion

        #region Initialization
        Projectile snowAura;
        public override void SetDefaults()
        {
            ApplyDefaultDefaults();
            // Since setdefualts is only called once, we can spawn the aura here
            snowAura = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                Projectile.Center, 
                Vector2.Zero, 
                ModContent.ProjectileType<SnowTop_Aura>(), 
                damage: 100, knockback: 0, Projectile.owner);
            
        }
        #endregion

        #region Ai
        public void updateAura(Projectile aura, Dust[] auraDust = null)
        {
            if(aura.active){
                aura.Center = Projectile.Center;
                // MAYBE: make dust ring rotate around the top?
                
            }
        }
        protected override void TopAI()
        {
            Projectile.ai[1]++; // Increment the ai[1] counter every frame
           
            updateAura(snowAura);

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Vector2.Zero, 100, default, 1.5f);
                dust.position = Projectile.Center;
                dust.noGravity = true;
                dust.velocity.Y = -1f;
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
            }

            if (Projectile.ai[1] % FrequencyToFrames(2, ud) == 0)
            {
                Dust snow = Dust.NewDustPerfect(Projectile.Center, DustID.Snow, Vector2.Zero, 100, default, 1.2f);
                snow.position = Projectile.Center;
                snow.velocity.Y = 2f;
            }

            base.TopAI(); // Call base for frame animation, gravity, friction, and step up
        }


        #endregion
        #region Kill
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            if (snowAura.active) snowAura.Kill();
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
            return base.OnTileCollide(oldVelocity);
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
                target.AddBuff(BuffID.Frostburn, secondsToFrames(5));
            }
        }
        #endregion
    }
}