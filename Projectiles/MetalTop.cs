using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static SpinningTopsMod.Utils;

namespace SpinningTopsMod.Content.Projectiles
{
    public class MetalTop : SpinningTopBase
    {   
        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 22;
        protected override int MaxBounces => 5;
        protected override float Gravity => 0.2f;
        protected override float MaxFallSpeed => 16f;
        protected override float topMass => 10f; // The normal mass is 1f
        protected override int DefaultLocalNPCHitCooldown => 30;
        protected override int DefaultTimeLeftSeconds => 12;
        protected override int DefualtTextureFrames => 2;
        protected override short KillDustType =>DustID.Iron;

        #endregion

        #region Initialization
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            // Enable trail cache so DrawAfterimagesFromEdge has positions to render
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16; // number of afterimages
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // 0 = standard, 2 = rotated
        }

        public override void SetDefaults()
        {
            ApplyDefaultDefaults();
        }
        #endregion

        #region Ai
        protected override void TopAI()
        {
            Projectile.ai[1]++;

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WoodFurniture, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
            }

            base.TopAI();
        }
        #endregion

        #region  Collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {   
            if (Projectile.ai[1] % FrequencyToFrames(2, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (Projectile.oldVelocity.Y > 10f) // if projectile moving down fast...
            {
                if(blockedByFloor(Projectile.velocity, oldVelocity)) // ... and hits the ground
                {
                    Projectile.ai[2]++;
                    if(Projectile.ai[2] >= MaxBounces) {Projectile.Kill();}
                    else
                    {
                       // Shockwave : Strike NPCs in a wide horizontal area when hitting ground fast
                        float shockWidth = tilesToPixels(20); // 10 tiles on each side
                        float shockHeight = tilesToPixels(6); // 3 above and 3 below 
                        
                        foreach (NPC foe in allNPCsInRectangle(Projectile.Bottom, 
                                new Vector2(shockWidth, shockHeight), true) )
                        {
                            int dir = foe.Center.X < Projectile.Center.X ? -1 : 1;
                            foe.SimpleStrikeNPC(Projectile.damage, dir, knockBack: 15.5f);
                        }

                        dustSlamOutwards(Projectile.Bottom, DustID.Dirt, 32, -2.5f, -7.5f, 160, dustScale: 1.5f);
                        dustPuffAtEdges(Projectile.Bottom, DustID.Smoke, 2, 32, dustScale: 2f);
                        dustPuffAtEdges(Projectile.Bottom, DustID.Smoke, 2, 160, dustScale: 6f);
                        SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, Projectile.position);
                        
                    }
                }
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
                Projectile.velocity = Projectile.velocity * 0.8f; // Keep going, but slower
                if(Projectile.ai[0] >= MaxHits*0.5f) // This will make more sense when you see the rendering
                {
                   // Add a hot spark effect
                   DustExplosion(target.Center, DustID.MinecartSpark, 7, 0.5f, 2f, dustScale: 3.5f);
                   // Burn the target
                   target.AddBuff(BuffID.OnFire, secondsToFrames(5));
                }
            }
        }
        #endregion
        #region Rendering
        public override bool PreDraw(ref Color lightColor)
        {
            // SpriteEffects helps to flip texture horizontally and vertically
            // Can be used for simple animation effects, instead of drawing frames by hand
            SpriteEffects spriteEffects = renderingSpriteEffect;

            // Calculating frameHeight and current Y pos dependence of frame
            // If texture without animation frameHeight is always texture.Height and startY is always 0
            int frameHeight = TopTexture.Height / Main.projFrames[Type];
            int startY = frameHeight * Projectile.frame;

            // Get this frame on texture
            Rectangle sourceRectangle = TopTexture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);
            // Alternatively, you can skip defining frameHeight and startY and use this:
            // Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

            Vector2 origin = sourceRectangle.Size() / 2f;

            // If image isn't centered or symmetrical you can specify origin of the sprite
            // (0,0) for the upper-left corner
            //float offsetX = 0f;
            //origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            // If sprite is vertical
             float offsetY = 16f;
             origin.Y = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Height - offsetY : offsetY);

            // Hot texture variant
            Texture2D hotTex = ModContent.Request<Texture2D>("SpinningTopsMod/Assets/Textures/MetalTop").Value;

            // Applying lighting and draw current frame
            Color drawColor = Projectile.GetAlpha(lightColor);
            // Interpolate between 0 and 255 alpha based on hits
            float hitProgress = ScaleToRange(Projectile.ai[0], 0, MaxHits, 0, 255);
            Color glowColor = Color.White;
                glowColor.A = (byte)hitProgress;
            // Apply culling
            if(IsOnScreenWithMargin(DrawCullMargin))
            {
                Main.spriteBatch.Draw(TopTexture,
                    Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                    sourceRectangle, 
                    drawColor, 
                    Projectile.rotation, 
                    origin, 
                    Projectile.scale, 
                    spriteEffects, 
                    0);
                // Draw glowmask if applicable
                if (GlowMask != null)
                {
                    Main.spriteBatch.Draw(GlowMask,
                    Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                    sourceRectangle, 
                    glowColor, 
                    Projectile.rotation, 
                    origin, 
                    Projectile.scale, 
                    spriteEffects, 
                    1);
                }
                Main.spriteBatch.Draw(hotTex,
                    Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                    sourceRectangle, 
                    Color.White * (hitProgress/255), // Make hot texture more visible as we get closer to max hits
                    Projectile.rotation, 
                    origin, 
                    Projectile.scale, 
                    spriteEffects, 
                    1);

                if(Projectile.velocity.Y>11f)
                DrawAfterimagesCentered(Projectile, 1, lightColor);
            }
            
            
            
            return false;
        }
        #endregion
    }
}
        
