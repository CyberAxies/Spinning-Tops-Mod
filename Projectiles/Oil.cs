using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Terraria.Utilities;
using static SpinningTopsMod.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace SpinningTopsMod.Content.Projectiles
{
    public class Oil : ModProjectile
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Ale); // Clone the defaults from the Oil Slick projectile
            Projectile.damage = 1;
            Projectile.scale = 0.5f;
            DrawOriginOffsetX = 16;
            DrawOriginOffsetY = -10;

            Projectile.ai[0] = 0;
        }

        public override void AI()
        {   

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damage)
        {
            // Inflict Oiled debuff for 10 seconds (600 ticks)
            target.AddBuff(BuffID.Oiled, 600);
        }
         int splashRadiusTiles = 10 * 16;
        public override void OnKill(int timeLeft)
        {

            foreach(NPC target in Utils.AllNPCsWithinRangeOfPoint(Projectile.Center, splashRadiusTiles))
            {
                target.AddBuff(BuffID.Oiled, 600); // Inflict Oiled debuff for 10 seconds (600 ticks)
            }
           
           
            // Visual and sound effects upon projectile death
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position); // ice break sound, sounds like shattered glass cointainer for oil

            Vector2 startPosition = Main.rand.NextVector2Circular(3f, 3f);
            for (int i = 0; i < 3; i++)
            {
                Dust glassdust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, Scale: 1.5f);
                glassdust.velocity = 2 * startPosition.RotatedBy(MathHelper.TwoPi / 3 * i);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust oildust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, newColor: Color.Black, Scale: 2.5f);
                oildust.velocity = 4.5f * startPosition.RotatedBy(MathHelper.TwoPi / 10 * i);
                oildust.alpha = 180;
            }

            base.OnKill(timeLeft);
        }

        Vector2 originOfRotation;
        public override bool PreDraw(ref Color lightColor)
        {
           Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
           float OriginX = texture.Width /3f;
           float originY = texture.Height*2f / 3f;
           originOfRotation = new Vector2(OriginX, originY);

        // Draw the projectile with the specified origin of rotation
            Main.EntitySpriteDraw(
                texture, 
                Projectile.Center - Main.screenPosition,
                null, 
                lightColor, 
                Projectile.rotation, 
                originOfRotation, 
                Projectile.scale, 
                SpriteEffects.None, 
                0);
        
            
            return false; // Return false to prevent the default drawing
        }

    }
}