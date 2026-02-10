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
    public class OceanTop : ModProjectile
    {   
        // REMOVE THIS, Add your own texture in the same folder as the projectile, and give it the same name (must be .png)
       // public override string Texture => "SpinningTopsMod/Assets/Textures/Top_glow";

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
        int ud; // Holds the number of updates per frame (extraUpdates + 1)
        float gravity = 0.1f; // Gravity applied to the projectile
        float maxFallSpeed = 10f; // Terminal velocity
        float distMax = 1600; // 100 tiles; max distance from player before despawn
        bool inWater = false; // Track if the projectile is in water
        bool dryAbove = false; // Track if the projectile is on the water surface
        public override void SetDefaults()
        {
            Projectile.alpha = 0; // Opacity of the projectile, 0 is fully visible, 255 is fully transparent

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
            Projectile.localNPCHitCooldown = 18; // Set the cooldown for hitting NPC

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
            Projectile.ignoreWater = true; // Ignore water physics
            
            // Visuals
            Projectile.light = 0.3f; // Add a light effect to the projectile
        }
        #endregion
        #region Ai

        public override bool PreAI()
        {
            Player player = Main.LocalPlayer;
            if(Vector2.DistanceSquared(Projectile.Center, player.Center) > distMax * distMax)
            {
                Projectile.Kill();
            }

           // Check if the projectile is in water
                inWater = Collision.DrownCollision(Projectile.position, Projectile.width, Projectile.height);
                Vector2 above = Projectile.Center + new Vector2(0, -16);
                dryAbove = !Collision.DrownCollision(above, Projectile.width, Projectile.height);

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
             if (Projectile.ai[1] == 1) { Projectile.velocity.Y *= 2f; }

            // Add a dust effect to the projectile 
                if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0) // Add dust every 10 frames
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Main.rand.NextVector2CircularEdge(1f,1f), 100, default, 1.5f);
                    Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                    Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust

                }
            // Apply gravity

                if (!inWater)
                {   // Normal gravity when not in water
                    Projectile.velocity.Y += gravity;
                    if (Projectile.velocity.Y > maxFallSpeed) { Projectile.velocity.Y = maxFallSpeed; }  
                }
                else if (inWater && dryAbove) // Water surface
                {Projectile.velocity.Y -= gravity * 5f;} // Glide along surface
                else if(inWater && !dryAbove)
                { Projectile.velocity.Y += gravity * 0.25f; } // Sink slowly when fully submerged

            // Apply friction to slow down the projectile
                if (Projectile.ai[1] % FrequencyToFrames(8, ud) == 0 && !inWater) { Projectile.velocity.X *= 0.98f; }// Apply friction every 8 times per second
            
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
            if(inWater && !dryAbove)
            {
                // Create bubble effects when fully submerged
                if (Projectile.ai[1] % FrequencyToFrames(6, ud) == 0) // Add bubbles 6 times per second
                {
                    Dust dustBubble = Dust.NewDustPerfect(Projectile.Center, DustID.BubbleBlock, Main.rand.NextVector2CircularEdge(1f, 1f), 100, default, 1.2f);
                    dustBubble.velocity *= 0.5f; // Slow down the bubble rise
                    Vector3 DustColor = dustBubble.color.ToVector3(); // Get the color of the dust
                    Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust
                }
    
                if(Projectile.ai[1] % FrequencyToFrames(2, ud) == 0)
                {
                    Projectile bubbleProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), 
                        Projectile.Center, 
                        Main.rand.NextVector2CircularEdge(2, 4), 
                        ProjectileID.Bubble, 6, 3f, Projectile.owner);
                    bubbleProj.velocity.Y = -Math.Abs(bubbleProj.velocity.Y); // Ensure bubble moves upwards
                }
                
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage -= 0.5f; // Crits deal 50% bonus damage, instead of 100%

            if(inWater){modifiers.FinalDamage *= 2f;} // Increase damage in water
            else if(target.wet || target.HasBuff(BuffID.Wet)){modifiers.SetCrit();} // Gaurentee Crit (doesn't stack with in water bonus)
        }
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
                Projectile.velocity.Y =  -2.5f; // Jump up slightly upon hitting an NPC

                // Hit effects: 
                target.AddBuff(BuffID.Wet, 300); // Inflict Wet debuff for 5 seconds (300 ticks)
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
            Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

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