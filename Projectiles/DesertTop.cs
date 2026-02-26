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
    public class DesertTop : SpinningTopBase
    {
        #region Override and import Virtual parameters from the top Base
        protected override int MaxHits => 10;
        protected override int MaxBounces => 20;
        protected override float Gravity => 0.1f;
        protected override float MaxFallSpeed => 8f;
        protected override int DefaultLocalNPCHitCooldown => 25;
        protected override int DefaultTimeLeftSeconds => 15;
        protected override int DefualtTextureFrames => 4;
        protected override Texture2D TopTexture => base.TopTexture;
        #endregion

        private bool sandy = false;
        private float originalXVelocity = 0.1f;

        #region Initialization
        public override string Texture => "SpinningTopsMod/Content/Projectiles/DesertTop";

        public override void SetDefaults()
        {
            ApplyDefaultDefaults();
        }
        #endregion

        #region AI
        protected override void TopAI()
        {
            Projectile.ai[1]++;
            if(Projectile.ai[1] == 1) originalXVelocity = Projectile.velocity.X;
            // Add a dust effect to the projectile 
            if (Projectile.ai[1] % FrequencyToFrames(4, ud) == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Sand, Vector2.Zero, 100, default, 1.5f);
                Vector3 DustColor = dust.color.ToVector3();
                Lighting.AddLight(Projectile.position, DustColor);
            }
            // Check if projectile is on sand
            Point tilePos = Projectile.Center.ToTileCoordinates() + new Point(0, 1);
            Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
            if (TileID.Sets.isDesertBiomeSand[tile.TileType])
            {
                sandy = true;
                Projectile.velocity.X = originalXVelocity * 2f;
            }
            else
            {
                sandy = false;
                if (Math.Abs(Projectile.velocity.X) > Math.Abs(originalXVelocity))
                {
                    Projectile.velocity.X = originalXVelocity;
                }
                else
                {
                    originalXVelocity = Projectile.velocity.X;
                }
            }

            if(Projectile.ai[1] % FrequencyToFrames(1, ud) == 0)
            {
                if(sandy)
                {
                    Projectile spike = Projectile.NewProjectileDirect(Entity.GetSource_FromThis(),
                        Projectile.Center,
                        new Vector2(0, -2.5f),
                        ProjectileID.RollingCactusSpike,
                        Projectile.damage/2,
                        2f,
                        Projectile.owner
                    );
                    spike.hostile = false;
                    spike.friendly = true;
                }
            }

            base.TopAI();
        }

    
        #endregion

        #region Collision
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (blockedByWall(Projectile.velocity, oldVelocity))
            {
                Projectile.ai[2]++;
                if (Projectile.ai[2] >= MaxBounces)
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
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.velocity = -Projectile.velocity * 0.8f;

                if (sandy)
                {   /// TODO: Use utils Tripleproj instead
                    for (int i = -1; i < 2; i++)
                    {
                        Projectile spike1 = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(i * 2, -2.75f),
                            ProjectileID.RollingCactusSpike,
                            Projectile.damage / 2,
                            Projectile.knockBack,
                            Projectile.owner);
                        spike1.hostile = false;
                        spike1.friendly = true;
                        spike1.ArmorPenetration = 5;
                        spike1.penetrate = 2;
                        spike1.usesIDStaticNPCImmunity = true;
                        spike1.idStaticNPCHitCooldown = 30;
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
                    spike2.hostile = false;
                    spike2.friendly = true;
                    spike2.ArmorPenetration = 5;
                    spike2.penetrate = 2;
                    spike2.usesIDStaticNPCImmunity = true;
                    spike2.idStaticNPCHitCooldown = 20;
                }

                Player player = Main.LocalPlayer;
                player.AddBuff(BuffID.Thorns, 300);
            }
        }
        #endregion

        #region Kill Effects
        public override void OnKill(int timeLeft)
        {
            DustExplosion(Projectile.Center, DustID.t_Cactus, 12, 0.5f, 1.5f);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            Projectile[] spike2 = projTripleShot(Projectile.GetSource_FromThis(), 
                Projectile.Center, 
                ProjectileID.RollingCactusSpike, 
                new Vector2(0,1), 
                1.75f, 
                Projectile.damage/2, 
                knockBack: 2f, spreadAngle: 30f);
            for (int i = 0; i <= 2; i++)
            {
                spike2[i].hostile = false;
                spike2[i].friendly = true;
                spike2[i].ArmorPenetration = 5;
                spike2[i].penetrate = 2;
            }
        }
        #endregion

        #region Rendering
        public override bool PreDraw(ref Color lightColor)
        {
            base.PreDraw(ref lightColor);

            return false;
        }
        #endregion
    }

}