using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static SpinningTopsMod.Utils;

namespace SpinningTopsMod.Content.Projectiles
{
    public class DesertTop : ModProjectile
    {   
        // REMOVE THIS, Add your own texture in the same folder as the projectile, and give it the same name (must be .png)
        public override string Texture => "SpinningTopsMod/Content/Projectiles/DesertTop";

        #region Initialization
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // Set the number of frames for the projectile sprite
        }
        // normally tile collisions and hit npc maxes are handled by penetrate,
        // but since we want custom behavior we handle them manually
        // especially since gliding along the ground normally triggers tile collisions
        int maxHits = 10; // Set the maximum number of NPCs to hit before destroying the projectile
        int maxBounces = 20; // Set the maximum number of tile bounces before destroying the projectile
        int ud;
        bool sandy = false;
        float originalXVelocity = 0.1f;
        public override void SetDefaults()
        {
            Projectile.alpha = 255; // REMOVE THIS, it makes the sprite not render

            // Hitbox and collision
            Projectile.width = 16; // The width of the projectile hitbox
            Projectile.height = 16; // The height of the projectile hitbox
            Projectile.tileCollide = true; // Enable tile collision

            // Drawing Sprite
            Projectile.scale = 0.75f; // Downscale sprite
            DrawOriginOffsetX = -14f;

            // AI 
            Projectile.aiStyle = 0; // The AI style of the projectile, 0 means no AI
            Projectile.extraUpdates = 1; // How many extra updates the projectile gets per frame
            Projectile.usesLocalNPCImmunity = true; // Use the static NPC immunity system
            Projectile.localNPCHitCooldown = 25; // Set the cooldown for hitting NPC

            // Cashing numbers
            Projectile.ai[0] = 0; // Use ai[0] to count NPC hits
            Projectile.ai[1] = 0; // Use ai[1] to track the number of times AI() gets called
            Projectile.ai[2] = 0; // Use ai[2] to track the number of tile collisions
            ud = Projectile.extraUpdates + 1;

            // Behavior
            Projectile.friendly = true; // Can damage enemies
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.penetrate = -1; // Infinite block bounces
            Projectile.timeLeft = secondsToFrames(15, ud); // How long the projectile lasts before disappearing
            
            // Visuals
            Projectile.light = 0.3f; // Add a light effect to the projectile
            Projectile.alpha = 0; // Make the projectile fully visible
        }
        #endregion
        #region Ai

        public override bool PreAI()
        {
            float distMax = 1600; // 100 tiles
            Player player = Main.LocalPlayer;
            if(Vector2.DistanceSquared(Projectile.Center, player.Center) > distMax * distMax)
            {
                Projectile.Kill();
            }

            return true;
        }

        public override void AI()
        {
            Projectile.ai[1]++; // Increment the ai[1] counter every frame

            // Animate the projectile sprite
            // Loop through the animation frames, spending 10 ticks on each
            // Projectile.frame — index of current frame
            if (++Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                SoundEngine.PlaySound(SoundID.Item7, Projectile.position);

                // Or more compactly Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // Hack to get a higher arc trajectory, only triggers once
            if (Projectile.ai[1] == 1) { Projectile.velocity.Y *= 2.5f; originalXVelocity = Projectile.velocity.X;}

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0) // Add dust every 10 frames
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Sand, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust

            }
            // Apply gravity
            float gravity = 0.1f;
            Projectile.velocity.Y += gravity;
            if (Projectile.velocity.Y > 8f) { Projectile.velocity.Y = 8f; } // Terminal velocity      

            // Apply friction to slow down the projectile
            if (Projectile.ai[1] % FrequencyToFrames(8, ud) == 0) { Projectile.velocity.X *= 0.97f; }// Apply friction every 8 times per second
            
            // Step up logic to handle steps and hammered tiles
            float stepSpeed = 1f;
            Projectile.gfxOffY = 0;
            Collision.StepUp(
                ref Projectile.position,
                ref Projectile.velocity,
                Projectile.width,
                Projectile.height,
                ref stepSpeed,
                ref Projectile.gfxOffY,
                1);
        }
        
        public override void PostAI()
        {     
    
            Point tilePos = Projectile.Center.ToTileCoordinates() + new Point(0, 1);
            Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
            if (TileID.Sets.isDesertBiomeSand[tile.TileType])
            {
                sandy = true;
                Projectile.velocity.X = originalXVelocity * 2f; // Maintain original X velocity on sand
            }
            else
            {
                sandy = false; 
                if(Math.Abs(Projectile.velocity.X) > Math.Abs(originalXVelocity))
                {
                    Projectile.velocity.X = originalXVelocity; // Restore original X velocity when not on sand
                }
                else{originalXVelocity = Projectile.velocity.X;}
            }

        }
        #endregion
        #region  Collision
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false; // Prevent the projectile from falling through platforms
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {   
            // For normal tile collision effects, it's better to use ai[2] to count bounces
            //Projectile.ai[2]++;

            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            // If the projectile hits the left or right side of the tile...
            if (blockedByWall(Projectile.velocity, oldVelocity))
            {
                Projectile.ai[2]++;
                if(Projectile.ai[2] >= maxBounces) // If it has bounced the maximum number of times, destroy the projectile
                {
                    Projectile.Kill();
                    // Again, you can add kill effects that only trigger when max bounces is reached, if desired
                    
                }
                else
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.8f; // Reverse the X velocity and reduce speed
                }
                
            }

            return false; // Prevent the projectile from being destroyed on tile collision
        }
        #endregion
        #region Hit NPC
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Increment the ai[0] counter when hitting an NPC
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= maxHits) // If it has hit 5 NPCs, destroy the projectile
                Projectile.Kill();
            // you can add kill effects that only trigger when max hits is reached, if desired
            // Generic kill effects go in OnKill()
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.8f; // Reverse the velocity

                // Hit effects: 
               
                if(sandy)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        Projectile spike1 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(i * 2, -2.75f),
                            ProjectileID.RollingCactusSpike,
                            Projectile.damage / 2,
                            Projectile.knockBack,
                            Projectile.owner);
                        spike1.hostile = false; // Make the spike friendly
                        spike1.friendly = true;
                        spike1.ArmorPenetration = 5;
                        spike1.penetrate = 2;
                    }
                }
                else
                {
                    Projectile spike2 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(-Projectile.velocity.X, -2.75f),
                            ProjectileID.RollingCactusSpike,
                            Projectile.damage / 2,
                            Projectile.knockBack,
                            Projectile.owner);
                        spike2.hostile = false; // Make the spike friendly
                        spike2.friendly = true;
                        spike2.ArmorPenetration = 5;
                        spike2.penetrate = 2;
                }
                 

                Player player = Main.LocalPlayer;
                player.AddBuff(BuffID.Thorns, 300); // Give the player the Thorns buff for 5 seconds (300 ticks)

                
                
            }
        }

        
        #endregion
        public override void OnKill(int timeLeft)
        {
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
            
            for (int i = -1; i < 2; i++)
                    {
                        Projectile spike2 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(i*-1.75f, -2.75f),
                            ProjectileID.RollingCactusSpike,
                            Projectile.damage / 2,
                            Projectile.knockBack,
                            Projectile.owner);
                        spike2.hostile = false; // Make the spike friendly
                        spike2.friendly = true;
                        spike2.ArmorPenetration = 5;
                        spike2.penetrate = 2;
                    }
        }
        #region Rendering
        // Some advanced drawing because the texture image isn't centered or symmetrical
        // If you don't want to manually draw you can use vanilla projectile rendering offsets
        // Here you can check it https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#horizontal-sprite-example
         public override bool PreDraw(ref Color lightColor)
        {
            // SpriteEffects helps to flip texture horizontally and vertically
            // Can be used for simple animation effects, instead of drawing frames by hand
            SpriteEffects spriteEffects = SpriteEffects.None;
           


            // Getting texture of projectile
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            // get the glowmask texture
            Texture2D glowMask = Mod.Assets.Request<Texture2D>("Assets/Textures/Top_glow").Value;

            // Calculating frameHeight and current Y pos dependence of frame
            // If texture without animation frameHeight is always texture.Height and startY is always 0
            int frameHeight = texture.Height / Main.projFrames[Type];
            int startY = frameHeight * Projectile.frame;

            // Get this frame on texture
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);

            // Alternatively, you can skip defining frameHeight and startY and use this:
            // Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

            Vector2 origin = sourceRectangle.Size() / 2f;

            // If image isn't centered or symmetrical you can specify origin of the sprite
            // (0,0) for the upper-left corner
            //float offsetX = -16f;
            //origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            // If sprite is vertical
             float offsetY = 16f;
             origin.Y = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Height - offsetY : offsetY);


            // Applying lighting and draw current frame
            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // It's important to return false, otherwise we also draw the original texture.
            return false;
        }
        #endregion
    }

}