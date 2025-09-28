using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpinningTopsMod.Content.Projectiles
{
    // This is a basic projectile template.
    // Please see tModLoader's ExampleMod for every other example:

    public class DungeonTop : ModProjectile
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
            Projectile.timeLeft = 20 * 60 * 2; // How long the projectile lasts before disappearing
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.aiStyle = 0; // The AI style of the projectile, 0 means no AI
            Projectile.extraUpdates = 1; // How many extra updates the projectile gets per frame
            DrawOffsetX = -12; // Center the sprite horizontally
            DrawOriginOffsetY = -16; // Center the sprite vertically
            Projectile.usesLocalNPCImmunity = true; // Use the static NPC immunity system
            Projectile.localNPCHitCooldown = 10; // Set the cooldown for hitting NPC
            Projectile.tileCollide = true; // Enable tile collision
            Projectile.light = 0.3f; // Add a light effect to the projectile
        }
        int maxHits = 5; // Set the maximum number of NPCs to hit before destroying the projectile
        Projectile swish; // Reference to the Muramasa projectile
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
            if (Projectile.ai[1] % 37 == 0) // Add dust every 10 frames
            {
                Vector2 rand2d = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 2f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 172, rand2d, 100, default, 1.5f); // Dust ID 172 is for Dungeon weapons
                dust.noGravity = true; // Make the dust not affected by gravity
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust
            }

            if (Projectile.ai[1] % 53 == 0)
            {
                // Create a small water stream effect at a 45-degree angle facing right and up
                Projectile Str_A = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(3f, -3f), ProjectileID.WaterStream,
                Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                // Create another water stream effect at a 45-degree angle facing left and up
                Projectile Str_B = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-3f, -3f), ProjectileID.WaterStream,
                Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                Str_A.ArmorPenetration = 15; // Set armor penetration for the water stream
                Str_B.ArmorPenetration = 15; // Set armor penetration for the water stream
                
                                    
            }


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
                Projectile.velocity.Y *= 2.5f; // Increase the Y velocity on the first frame
            }

            if (Projectile.ai[1] % (30*2) == 0)
            {
                Vector2 Player2Proj = Projectile.Center - Main.player[Projectile.owner].Center; // Get the distance from the player
                float distSqr = Player2Proj.X * Player2Proj.X + Player2Proj.Y * Player2Proj.Y; // Calculate the distance squared
                if (distSqr > 1600 * 1600) { Projectile.Kill(); } // If the distance is greater than 1600 pixels (100 tiles), kill the projectile
            }

            if (swish != null && swish.type == ProjectileID.Muramasa)
            {
                swish.velocity = new Vector2((float)Math.Cos(Projectile.ai[1] * MathHelper.TwoPi / 120f) * 4.5f,
                (float)Math.Sin(Projectile.ai[1] * MathHelper.TwoPi / 120f) * 3f); // Set the velocity to make it move in an arc
                swish.rotation = swish.velocity.ToRotation() + MathHelper.PiOver2; // Rotate the projectile to face the direction of movement
            }

            // Step up logic to handle steps and hammered tiles
            float stepSpeed = Projectile.velocity.X;
            float gfxOffY = 0f;
            Collision.StepUp(ref Projectile.position,
             ref Projectile.velocity,
              Projectile.width,
               Projectile.height,
               ref stepSpeed,
                ref gfxOffY);
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
                Projectile.velocity = -Projectile.velocity * 0.8f; // Reverse the velocity and reduce speed
            }

            Vector2 rand2d = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * 4f;

            swish = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center - rand2d,
            Vector2.Zero, ProjectileID.Muramasa, 0, Projectile.knockBack, Projectile.owner); // Create a Muramasa projectile at the NPC's position with a slight random offset
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