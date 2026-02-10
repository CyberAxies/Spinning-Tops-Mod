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
    public class Blueprint : SpinningTopBase
    {   
        #region Override and import Virtual parameters from the top Base
        // REMOVE THIS, Add your own texture in the same folder as the projectile, and give it the same name (must be .png)
        public override string Texture => "SpinningTopsMod/Assets/Textures/Top_glow";
        // Combat parameters
        protected override int MaxHits => base.MaxHits;
        protected override int MaxBounces => base.MaxBounces;
        protected override int DefaultLocalNPCHitCooldown => base.DefaultLocalNPCHitCooldown;
        protected override int DefaultTimeLeftSeconds => base.DefaultTimeLeftSeconds;
        // Physics parameters
        protected override float Gravity => base.Gravity;
        protected override float MaxFallSpeed => base.MaxFallSpeed;
        protected override float DefualtArcHeightMult => base.DefualtArcHeightMult;
        // Rendering Parameters (glowmask, etc. ) if needed:
        //protected override Texture2D GlowMask => base.GlowMask;
        #endregion

        #region Initialization
        // Set static defualts, etc can go here
        // Use DefualtTextureFrames for vertical frame count. Default is 4

        // normally tile collisions and hit npc maxes are handled by penetrate,
        // but since we want custom behavior we handle them manually
        // especially since gliding along the ground normally triggers tile collisions
        
        public override void SetDefaults()
        {
            // Apply base defaults; derived classes can override the protected properties on SpinningTopBase
            ApplyDefaultDefaults();

            // Override any additional defaults here if needed:
            //Projectile.coldDamage = true;
        }
        #endregion
        #region Ai

        // PreAI despawn check removed — handled by SpinningTopBase via `DespawnDistance`.
        /*  Can add stuff like public override void AI for other behaviours and stuff. PreAI, Postai, etc.
            The defualt cases are handled in the base class, so don't bloat your subclass with stuff that already got handled
            unless you wanna change something
        */
        protected override void TopAI()
        {
            Projectile.ai[1]++; // Increment the ai[1] counter every frame

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0) // Add dust every 10 frames
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WoodFurniture, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust
            }

            base.TopAI(); // Call base AI to handle frame animation and other common behavior. Override this method instead of AI() in derived classes for custom behavior.
        }

        #endregion
        #region  Collision
        // TileCollideStyle gets handled in the Base class
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {   
            // For normal tile collision effects, it's better to use ai[2] to count bounces
            //Projectile.ai[2]++;

            // If the projectile hits the left or right side of the tile...
            if (blockedByWall(Projectile.velocity, oldVelocity))
            {
                Projectile.ai[2]++;
                if(Projectile.ai[2] >= MaxBounces) // If it has bounced the maximum number of times, destroy the projectile
                {
                    Projectile.Kill();
                    // Again, you can add kill effects that only trigger when max bounces is reached, if desired
                }
                else
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.8f; // Reverse the X velocity and reduce speed
                }
                
            }
            //base.OnTileCollide(oldVelocity); Not sure if this is needed
            return base.OnTileCollide(oldVelocity); // passes it to Base class, returns false 
        }
        #endregion
        #region Hit NPC
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Increment the ai[0] counter when hitting an NPC
            Projectile.ai[0]++;
                if (Projectile.ai[0] >= MaxHits) // If it has hit max NPCs, destroy the projectile
                Projectile.Kill();
            // you can add kill effects that only trigger when max hits is reached, if desired
            // Generic kill effects go in OnKill()
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.8f; // Reverse the velocity

                // Hit effects: 
            }
        }
        #endregion
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            // Create a dust effect when the projectile is destroyed
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WoodFurniture, Vector2.Zero, 100, default, 1.5f);
                dust.position = Projectile.Center;
                dust.velocity = new Vector2(1, 0).RotatedBy(i * (2 * Math.PI / 10)) * 2f; // Spread the dust in a circular pattern
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust
            }
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        }
        #region Rendering
        // Some advanced drawing because the texture image isn't centered or symmetrical
        // If you don't want to manually draw you can use vanilla projectile rendering offsets
        // Here you can check it https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#horizontal-sprite-example
        public override bool PreDraw(ref Color lightColor)
        {
            base.PreDraw(ref lightColor);
            return false;

        }
        #endregion
    }

}