using System;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Filters = Terraria.Graphics.Effects.Filters;



namespace SpinningTopsMod.Content.Projectiles
{
    // This is a basic projectile template.
    // Please see tModLoader's ExampleMod for every other example:

    public class Elemental_Hallowed : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2; // Set the number of frames for the projectile sprite
  
        }

        public override void SetDefaults()
        {
            Projectile.width = 16; // The width of the projectile hitbox
            Projectile.height = 16; // The height of the projectile hitbox
            Projectile.friendly = true; // Can damage enemies
            Projectile.penetrate = -1; // Infinite block bounces
            Projectile.scale = 0.75f; // Downscale sprite from 32x32 to 16x16
            Projectile.ai[0] = 0; // Use ai[0] to count NPC hits
            Projectile.ai[1] = 0; // Use ai[1] to track the number of times AI() gets called
            Projectile.ai[2] = 0; // Use ai[3] to track the number of tile collisions
            Projectile.timeLeft = 7 * 60 * 2; // How long the projectile lasts before disappearing
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.aiStyle = 0; // The AI style of the projectile, 0 means no AI
            Projectile.extraUpdates = 1; // How many extra updates the projectile gets per frame
            DrawOffsetX = -12; // Center the sprite horizontally
            DrawOriginOffsetY = -16; // Center the sprite vertically
            Projectile.usesLocalNPCImmunity = true; // Use the static NPC immunity system
            Projectile.localNPCHitCooldown = 21; // Set the cooldown for hitting NPC
            Projectile.tileCollide = true; // Enable tile collision
            Projectile.light = 0.3f; // Add a light effect to the projectile
        }
        int maxHits = 17; // Set the maximum number of NPCs to hit before destroying the projectile
        Projectile over = null;
        Vector2 spawn;
        int gravityDelay = 1; // Delay before gravity starts affecting the projectile
        public override void AI()
        {
            Projectile.ai[1]++; // Increment the ai[1] counter every frame
            if (gravityDelay > 0) {gravityDelay--; }
            // Animate the projectile sprite
            // Loop through the 2 animation frames, spending 15 ticks on each
            // Projectile.frame — index of current frame
            if (++Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                SoundEngine.PlaySound(SoundID.Item7, Projectile.position);

                // Or more compactly Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

        
            
            // Add a passive secondary projectile spawn on timer
            if (Projectile.ai[1] % (127) == 0) // spawn projectile every ~0.5 second
            {
                Projectile magicBolt = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                 Main.rand.NextVector2Circular(5f, 5f), ProjectileID.FairyQueenMagicItemShot, (int)(Projectile.damage / 3f), 1.7f, Projectile.owner);
                magicBolt.scale = 0.25f;
                magicBolt.ignoreWater = true;
                magicBolt.tileCollide = false;
                magicBolt.timeLeft = 60;
                magicBolt.alpha = 60 / magicBolt.timeLeft * 127; // Make the projectile semi-transparent near spawning time
                magicBolt.ArmorPenetration = 10;

                int distMinSqr = 16 * 60 * 16 * 60;
                foreach (NPC target in Main.npc)
                {
                    if (target.CanBeChasedBy(Projectile))
                    {
                        int distSqr = (int)Vector2.DistanceSquared(Projectile.Center, target.Center);
                        if (distSqr < distMinSqr)
                        {
                            distMinSqr = distSqr;
                            Vector2 rr = Main.rand.NextVector2CircularEdge(1f, 1f) * 16 * 20;
                            spawn = target.Center + rr;
                            if (!Collision.SolidCollision(spawn, 16, 16))
                            {
                                Vector2 dir2Enemy = Vector2.Normalize(target.Center - spawn + // Calculate direction from spawn point to target, leading the shot based on target velocity
                                target.velocity * 2.5f * (target.velocity / (Projectile.velocity + new Vector2(0.01f, 0.01f))));

                                Vector2 dir2Spawn = Vector2.Normalize(spawn - (target.Center-rr)); // Direction from the main projectile to the spawn point
                                over = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center - rr, // add a projectile that tavels from the main projectile to the spawn point
                                 dir2Spawn * 12f, ProjectileID.HallowStar, (int)(Projectile.damage), 1.7f, Projectile.owner);
                                over.scale = 0.5f;
                                over.tileCollide = false;
                                
                               
                                Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, Velocity: Vector2.Zero,
                                Alpha: 220, Scale: 3.7f, newColor: Color.LightCyan); // Add a dust effect at the spawn point
                                Lighting.AddLight(Projectile.position, new Vector3(2f, 0.4f, 1.3f)); // Add light at the spawn point

                                Projectile.position = spawn; // Move the main projectile to the spawn point
                                gravityDelay = 60; // Reset gravity delay after teleporting
                                Projectile.velocity = dir2Enemy * 12f; // Set the velocity of the main projectile towards the target
                                Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, Velocity: Vector2.Zero,
                                Alpha: 220, Scale: 3.7f, newColor: Color.LightCyan); // Add a dust effect at the spawn point
                                Lighting.AddLight(Projectile.position, new Vector3(2f, 0.4f, 1.3f)); // Add light at the spawn point

                            }
                        }

                    }
                }

            }
            
             if (over != null ? Utils.areClose(over.Center, spawn, 16f):false) 
             {
                 over.Kill(); 
                 Dust.NewDustPerfect(Projectile.Center, DustID.TreasureSparkle, Velocity: Vector2.Zero,
                 Alpha: 150, Scale: 1.7f, newColor: Color.LightPink); // Add a dust effect on kill
             }


            // Apply gravity
            if (gravityDelay == 0)
            {
                Projectile.velocity.Y += 0.1f;
                if (Projectile.velocity.Y > 8f)
                {
                    Projectile.velocity.Y = 8f;
                }
                // Apply friction to slow down the projectile
                if (Projectile.ai[1] % 15 == 0) // Apply friction every 4 times per second
                {
                    Projectile.velocity.X *= 0.99f; // Reduce X velocity by 5% every 5 frames
                }
                if (Projectile.ai[1] == 1)
                {
                    Projectile.velocity.Y *= 1.5f; // Increase the Y velocity on the first frame
                }
            }
            // Step up logic to handle steps and hammered tiles
            float stepSpeed = 1f;
            Projectile.gfxOffY = 0;
            Collision.StepUp(ref Projectile.position,
             ref Projectile.velocity,
              Projectile.width,
               Projectile.height,
               ref stepSpeed,
                ref Projectile.gfxOffY, 1);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false; // Prevent falling through platforms
            // Use the tModLoader API method directly in AI 
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {


            // Increment the tile collision counter
            Projectile.ai[2]++;
            if (Projectile.ai[2] % (15 * 2) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f; // Reverse the X velocity and reduce speed
            }
            return false; // Prevent the projectile from being destroyed on tile collision
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Increment the ai[0] counter when hitting an NPC
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= maxHits) // If it has hit 5 NPCs, destroy the projectile
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.4f; // Reverse the velocity and reduce speed

            }
        }

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

        // Some advanced drawing because the texture image isn't centered or symmetrical
        // If you don't want to manually draw you can use vanilla projectile rendering offsets
        // Here you can check it https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#horizontal-sprite-example
        public override bool PreDraw(ref Color lightColor)
        {
            // SpriteEffects helps to flip texture horizontally and vertically
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.frameCounter >= 15)
            {
                Projectile.frameCounter = 0;
                spriteEffects = SpriteEffects.FlipHorizontally;
            }



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
            //float offsetX = 0f;
            //origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            // If sprite is vertical
            // float offsetY = 20f;
            // origin.Y = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Height - offsetY : offsetY);


            // Applying lighting and draw current frame
            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            // It's important to return false, otherwise we also draw the original texture.
            return false;
        }

    }

   
    
}