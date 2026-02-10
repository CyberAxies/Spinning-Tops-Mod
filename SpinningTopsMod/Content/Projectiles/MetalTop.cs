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
    public class MetalTop : ModProjectile
    {   
        // REMOVE THIS, Add your own texture in the same folder as the projectile, and give it the same name (must be .png)
        

        #region Initialization
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2; // Set the number of frames for the projectile sprite
            // Enable trail cache so DrawAfterimagesFromEdge has positions to render
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16; // number of afterimages
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // 0 = standard, 2 = rotated
        }
        // normally tile collisions and hit npc maxes are handled by penetrate,
        // but since we want custom behavior we handle them manually
        // especially since gliding along the ground normally triggers tile collisions
        int maxHits = 20; // Set the maximum number of NPCs to hit before destroying the projectile
        int maxBounces = 5; // Set the maximum number of tile bounces before destroying the projectile
        int ud;
        float gravity = 0.2f;
        float slamRange = 160f; // Range to trigger ogre slam
        public override void SetDefaults()
        {
            Projectile.alpha = 0; // Set to 0 so the sprite renders normally

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
            Projectile.localNPCHitCooldown = 30; // Set the cooldown for hitting NPC

            // Cashing numbers
            Projectile.ai[0] = 0; // Use ai[0] to count NPC hits
            Projectile.ai[1] = 0; // Use ai[1] to track the number of times AI() gets called
            Projectile.ai[2] = 0; // Use ai[2] to track the number of tile collisions
            ud = Projectile.extraUpdates + 1;

            // Behavior
            Projectile.friendly = true; // Can damage enemies
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.penetrate = -1; // Infinite block bounces
            Projectile.timeLeft = secondsToFrames(12, ud); // How long the projectile lasts before disappearing
            

            // Visuals
            Projectile.light = 0.3f; // Add a light effect to the projectile
        }
        #endregion
        #region Ai
        float distMax = 1600; // 100 tiles
        public override bool PreAI()
        {
            if(Projectile.ai[1]%FrequencyToFrames(4, ud) == 0)
            {
                Player player = Main.LocalPlayer;
                if(Vector2.DistanceSquared(Projectile.Center, player.Center) > distMax * distMax)
                {
                    Projectile.Kill();
                }
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
            if (Projectile.ai[1] == 1) { Projectile.velocity.Y *= 4.5f; }

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0) // Add dust every 10 frames
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WoodFurniture, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust

            }
            // Apply gravity
            
            Projectile.velocity.Y += gravity;
            if (Projectile.velocity.Y > 16f) { Projectile.velocity.Y = 16f; } // Terminal velocity      

            // Apply friction to slow down the projectile
            if (Projectile.ai[1] % FrequencyToFrames(8, ud) == 0) { Projectile.velocity.X *= 0.99f; }// Apply friction every 8 times per second
            
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
             // Ground slam effect, only if falling fast enough and hitting the ground
            if (oldVelocity.Y > 11f)
            {
               if(blockedByFloor(Projectile.velocity, oldVelocity)) // check if it hit the ground
                {
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.position);
                    
                    for (int i = 0; i < 22; i++)
                    {   
                        float offset = ScaleToRange(i, 0, 22, 0, 160);
                        Vector2 offsetVector = new Vector2(offset*Main.rand.NextFromList(-1, 1), 0); // Spread out horizontally
                        Vector2 randomSpeed = new Vector2(2*Main.rand.NextFloat()-1, -(i/2)*Main.rand.NextFloat()); // Move slow sideways, faster upwards

                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offsetVector, DustID.Dirt, randomSpeed);
                        dust.noGravity = false;
                        dust.scale = 2.5f;
                        
                    }

                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && npc.lifeMax > 5 && !npc.friendly && !npc.dontTakeDamage &&
                            Math.Abs(npc.Center.X - Projectile.Center.X) <= slamRange &&
                            Math.Abs(Projectile.Center.Y - npc.Center.Y) <= 80f &&
                            Projectile.Center.Y + 32> npc.Center.Y) // within horizontal range, if the npc is above the projectile
                        {
                            
                            NPC.HitInfo hitInfo = new NPC.HitInfo()
                            {
                                Damage = Projectile.damage * 2,
                                Knockback = 8f,
                                HitDirection = Math.Sign(npc.Center.X - Projectile.Center.X),
                            };
                            npc.StrikeNPC(hitInfo);
                            NetMessage.SendStrikeNPC(npc, hitInfo);
                            npc.immune[Projectile.owner] = 30; // Set immunity frames for the NPC to avoid multiple hits
                        }
                       
                    }
                }

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
                    // Projectile is heavy, so it's inertia means the counterforce from bouncing won't slow it down as much
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
                // Projectile is heavy, so it doesn't slow down on hitting NPCs (inertia)

                // Hit effects: 
                if(Projectile.ai[0] >= maxHits*0.5f)
                {
                    // Start to heat up the top visually when halfway to max hits
                    // This is handled in PreDraw()
                    target.AddBuff(BuffID.OnFire, secondsToFrames(4, ud)); // Set the target on fire for 4 seconds
                }
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
            Texture2D hotTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/MetalTop").Value;

            // get the glowmask texture
            Texture2D glowMask = Mod.Assets.Request<Texture2D>("Assets/Textures/MoltenGlowmask").Value;

            // Calculating frameHeight and current Y pos dependence of frame
            // If texture without animation frameHeight is always texture.Height and startY is always 0
            int frameHeight = texture.Height / Main.projFrames[Type];
            int startY = frameHeight * Projectile.frame;

            // Get this frame on texture
            //Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);
            Rectangle hotSourceRectangle = hotTexture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

            // Alternatively, you can skip defining frameHeight and startY and use this:
            // Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 hotOrigin = hotSourceRectangle.Size() / 2f;

            // If image isn't centered or symmetrical you can specify origin of the sprite
            // (0,0) for the upper-left corner
            //float offsetX = -16f;
            //origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            // If sprite is vertical
             float offsetY = 16f;
             origin.Y = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Height - offsetY : offsetY);
             hotOrigin.Y = (float)(Projectile.spriteDirection == 1 ? hotSourceRectangle.Height - offsetY : offsetY);


            // Applying lighting and draw current frame
            Color drawColor = Projectile.GetAlpha(lightColor);
            // The more hits, more opaque the hot texture becomes, which simulates heating up
            float alphaValue = MathHelper.Clamp(ScaleToRange(Projectile.ai[0], 0, maxHits*0.75f, 0, 1f), 0, 1f);
            Color Hotness = Color.White * alphaValue; // White tinted with alpha based on hits
            
            if(Projectile.velocity.Y > 10f){DrawAfterimagesCentered(Projectile, 1, lightColor, 2,texture, spacing: 1.5f); }

            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            
            Main.spriteBatch.Draw(hotTexture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                hotSourceRectangle, Hotness, Projectile.rotation, hotOrigin, Projectile.scale, spriteEffects, 0);
            
            Main.spriteBatch.Draw(glowMask,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, Hotness, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // It's important to return false, otherwise we also draw the original texture.
            return false;
        }
        #endregion
    }

}