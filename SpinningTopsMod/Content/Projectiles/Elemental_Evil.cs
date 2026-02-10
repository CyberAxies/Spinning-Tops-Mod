using System;
using System.Threading;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace SpinningTopsMod.Content.Projectiles
{
    // This is a basic projectile template.
    // Please see tModLoader's ExampleMod for every other example:

    public class Elemental_Evil : ModProjectile
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
            Projectile.timeLeft = (int)6.6666666f * 60 * 2; // How long the projectile lasts before disappearing
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.aiStyle = 0; // The AI style of the projectile, 0 means no AI
            Projectile.extraUpdates = 1; // How many extra updates the projectile gets per frame
            DrawOffsetX = -12; // Center the sprite horizontally
            DrawOriginOffsetY = -16; // Center the sprite vertically
            Projectile.usesLocalNPCImmunity = true; // Use the static NPC immunity system
            Projectile.localNPCHitCooldown = 16; // Set the cooldown for hitting NPC
            Projectile.tileCollide = true; // Enable tile collision
            Projectile.light = 0.3f; // Add a light effect to the projectile
        }
        int maxHits = 66; // Set the maximum number of NPCs to hit before destroying the projectile
                          // GoingDown is used to track if the projectile is moving downwards
                          // This is used to control the tile collision behavior on platforms


        public override void AI()
        {
            Projectile.ai[1]++; // Increment the ai[1] counter every frame
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

            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % (15 * 2) == 0) // Add dust every 10 frames
            {
                Dust dust1 = Dust.NewDustPerfect(Projectile.Center, DustID.Corruption, Vector2.Zero, 100, default, 1.5f);
                Vector3 Dust1Color = dust1.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, Dust1Color * 1.5f); // Add light to the dust

                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, DustID.Crimson, Vector2.Zero, 100, default, 1.5f);
                Vector3 Dust2Color = dust2.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, Dust2Color * 1.5f); // Add light to the dust
            }

            if (Projectile.ai[0] == 0) { Projectile.ai[0] += Main.rand.NextFromList(1, 2); }


            // Apply gravity
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

            // Step up logic to handle steps and hammered tiles
            float stepSpeed = 1f;
            Projectile.gfxOffY = -1f;
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
            if (Projectile.ai[2] % (15*2) == 0)
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

        private Vector2 up = new Vector2(0, -1);
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

                // Spawn projectile on hit
                
                if (Projectile.ai[0] % 2 == 0) // on even hits
                {
                    for (int i = 0; i < 3; i++) // spawn 3 ichor sprays
                    {

                        Projectile ichorSpray = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                        target.Center, Vector2.Zero, ProjectileID.IchorSplash, (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);
                        ichorSpray.velocity = 3 * up.RotatedBy((i - 2 * Math.Clamp(i - 1, 0f, 1f) * i) / 2f);
                        ichorSpray.ArmorPenetration = 15; // Ichor ignores 15 defense
                    }

                    target.AddBuff(BuffID.Ichor, 60 * 10); // Apply ichor debuff for 10 seconds (adds 15 defense reduction)
                }

                else if (Projectile.ai[0] % 2 == 1)// on odd hits
                {
                    for (int i = 0; i < 3; i++) // spawn 3 cursed flames
                    {


                        Projectile cursedFlame = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                        target.Center, Vector2.Zero, ProjectileID.CursedFlameFriendly, (int)(Projectile.damage * 0.2f), 0f, Projectile.owner);
                        cursedFlame.velocity = 6 * up.RotatedBy(i - 2 * Math.Clamp(i - 1, 0f, 1f) * i);
                    }

                    target.AddBuff(BuffID.CursedInferno, 60 * 3); // Apply cursed flames debuff for 3 seconds (deals 24 dps)
                }

                float temp = 16 * 60 * 16 * 60; // 16 pixels = 1 tile                
                Vector2 direction = Vector2.Zero;
               
                foreach (NPC opps in Main.npc) // itterate through all NPCs
                {
                    if (opps.active && !opps.friendly && opps.whoAmI != target.whoAmI && !opps.dontTakeDamage) // If the NPC is active, not friendly, and not the one we just hit
                    {
                        float sqrDist = (opps.Center - Projectile.Center).LengthSquared(); // Get the squared distance to avoid a square root calculation
                         if (sqrDist < temp) // If the NPC is within 100 tiles (or closer than the closest found so far)
                         {
                            temp = sqrDist; // Update the closest distance
                                                // Reset velocity before retargeting
                                                // If velocity is very low, set a default direction

                             direction = Vector2.Normalize(opps.Center - Projectile.Center
                             + opps.velocity * (opps.velocity / (Projectile.velocity + new Vector2(0.1f,0.1f)))); // Get the direction to the closest NPC


                         }
                    }
                 }
                

                if (direction != Vector2.Zero) { Projectile.velocity = direction * Projectile.velocity.Length() * 0.9f;} // If a valid target was found, adjust velocity towards it
                else if (direction == Vector2.Zero){ Projectile.velocity *= -0.8f; } // If no valid target was found, just slow down and reverse the projectile
                
            }
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