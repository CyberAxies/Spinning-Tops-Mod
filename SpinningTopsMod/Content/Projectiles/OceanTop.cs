using System;
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

    public class OceanTop : ModProjectile
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
            Projectile.timeLeft = 15 * 60 * 2; // How long the projectile lasts before disappearing
            Projectile.DamageType = DamageClass.Melee; // The damage type of the projectile
            Projectile.aiStyle = 0; // The AI style of the projectile, 0 means no AI
            Projectile.extraUpdates = 1; // How many extra updates the projectile gets per frame
            DrawOffsetX = -12; // Center the sprite horizontally
            DrawOriginOffsetY = -16; // Center the sprite vertically
            Projectile.usesLocalNPCImmunity = true; // Use the static NPC immunity system
            Projectile.localNPCHitCooldown = 17; // Set the cooldown for hitting NPC
            Projectile.tileCollide = true; // Enable tile collision
            Projectile.light = 0.3f; // Add a light effect to the projectile
            Projectile.ignoreWater = true; // Ignore water

        }
        int maxHits = 10; // Set the maximum number of NPCs to hit before destroying the projectile
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
            if (Projectile.ai[1] % 15  == 0) // Add dust every 10 frames
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Vector2.Zero, 100, default, 1.5f);
                dust.position = Projectile.Center;
                Vector3 DustColor = dust.color.ToVector3(); // Get the color of the dust
                Lighting.AddLight(Projectile.position, DustColor); // Add light to the dust
            }


            // Check if the projectile is in water and if it is on the water surface
            // This is used to imatate the water walking effect
            bool inWater = Collision.DrownCollision(Projectile.position, Projectile.width, Projectile.height);
            Vector2 above = Projectile.position + new Vector2(0, -16);
            bool onSurface = !Collision.DrownCollision(above, Projectile.width, Projectile.height);

            // If the projectile is in water and on the surface, it will not fall thru the water
            // If it is in water but not on the surface, it will fall thru the water but at a slower rate
            // If it is not in water, it will fall normally
            if (inWater && onSurface)
            {
                Projectile.velocity.Y = -0.5f; // Glide on the water surface
                Projectile.damage = 26;
            }
            else if (inWater)
            {
                Projectile.velocity.Y += 0.025f; // Apply a smaller gravity to the projectile
                if (Projectile.velocity.Y > 1.3f) { Projectile.velocity.Y = 1.3f; } // Limit the downward speed to prevent it from falling too fast
                Projectile.damage = 28;
            }
            else
            {
                Projectile.velocity.Y += 0.2f; // Apply normal gravity to the projectile
                if (Projectile.velocity.Y > 10f) { Projectile.velocity.Y = 10f; } // Limit the downward speed to prevent it from falling too fast
                Projectile.damage = 16; // Set the damage to normal when not in water
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

            if (Projectile.ai[1] % (30*2) == 0)
            {
                Vector2 Player2Proj = Projectile.Center - Main.player[Projectile.owner].Center; // Get the distance from the player
                float distSqr = Player2Proj.X * Player2Proj.X + Player2Proj.Y * Player2Proj.Y; // Calculate the distance squared
                if (distSqr > 1600 * 1600) { Projectile.Kill(); } // If the distance is greater than 1600 pixels (100 tiles), kill the projectile
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
            if (Projectile.ai[2] % (17*2) == 0)
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
                Projectile.velocity.X *= 0.8f; // Reverse the velocity and reduce speed
                Projectile.velocity.Y -= 6f; // Add a small upward force to the projectile
            }

            // Water splash effect
            for (int i = 0; i < 5; i++)
            { 
                // First take the index and convert it to a poriton of a circle, eg. index 5 is 1/5 of the circle
                float turn = (1 / 5f) * MathHelper.TwoPi * i; // Multiply by 2π to convert from rotations- eg. 1/5 of a cricle- to radians- eg. 1.2566 radians-
                Vector2 velocity= new Vector2((float)Math.Cos(0.44 + turn), (float)Math.Sin(0.44 + turn)) * 2f; // Set the velocity based on the rotation angle

                Dust Splash = Dust.NewDustPerfect(target.Center + velocity, DustID.Water, velocity, 100, default, 2.5f);
                Splash.noGravity = true; // Set the dust to not be affected by gravity                
            }

            // Do the rest normaly
            base.OnHitNPC(target, hit, damageDone);
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