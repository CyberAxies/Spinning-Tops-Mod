using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using SpinningTopsMod;
using static SpinningTopsMod.Utils;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace SpinningTopsMod.Content.Projectiles
{
    public abstract class SpinningTopBase : ModProjectile
    {
        // ---- Configurable defaults (override in derived classes) ----

        #region Default properties (override these in derived classes as needed)
        #region Projectile defaults (override these in derived classes as needed)
        protected virtual float DespawnDistance => 1600f; // 100 tiles
        protected virtual int MaxHits => 15;
        protected virtual int MaxBounces => 10;
        protected virtual int DefaultWidth => 16;
        protected virtual int DefaultHeight => 16;
        protected virtual bool DefaultTileCollide => true;
        protected virtual float DefaultScale => 0.75f;
        protected virtual float DefaultDrawOriginOffsetX => -14f;
        protected virtual int DefaultExtraUpdates => 1;
        protected virtual bool DefaultUsesLocalNPCImmunity => true;
        protected virtual int DefaultLocalNPCHitCooldown => 18;
        protected virtual int DefaultAlpha => 0;
        protected virtual float DefaultLight => 0.3f;
        protected virtual int DefaultTimeLeftSeconds => 10;
        protected virtual int DefualtPierce => -1;
        protected virtual bool DefualtFriendly => true;
        protected virtual bool DefualtHostile => false;
        protected virtual DamageClass DefualtDamageClass => DamageClass.Melee;
        protected virtual SoundStyle DefualtFrameChangeSound => SoundID.Item7;

        protected virtual float DefualtArcHeightMult => 3f;
        #endregion
        #region Physics defaults (override per-top if needed)
        // Physics defaults (override per-top if needed)
        protected virtual float Gravity => 0.1f;
        protected virtual float MaxFallSpeed => 8f;
        protected virtual float frictionFactor => 0.99f; // Multiplier applied to horizontal velocity every friction application
        // How many final updates to wait between collision checks. 2 => ~30 checks/sec (at 60fps).
        protected virtual int CollisionCheckInterval => 2;
        // Extra radius to use for the cheap distance precheck before doing a hitbox intersection test.
        protected virtual float CollisionCheckRadius => 96f;
        // Whether to ignore tops owned by the same player (preserves previous behavior)
        protected virtual bool SkipSameOwner => false;

        // Reusable neighbor buffer to avoid per-call allocations
        static readonly List<int> neighborBuffer = new List<int>(32);

        // Helper field populated by ApplyDefaultDefaults
        protected int ud; // extraUpdates + 1
        #endregion
        #region Textures and drawing defaults (override these in derived classes as needed)
        // Draw culling margin in pixels. Tops outside the screen +/- this margin will not be drawn.
        protected virtual int DrawCullMargin => 128;

         protected virtual short KillDustType => DustID.WoodFurniture;
        protected virtual int DefualtTicksUntilNextFrame => 10;
        protected virtual int DefualtTextureFrames => 4;
        protected virtual Texture2D TopTexture => TextureAssets.Projectile[Projectile.type].Value;
        protected virtual Texture2D GlowMask => null; // Set this to a glowmask texture if you want one (must be same size as the projectile texture)
        protected virtual SpriteEffects renderingSpriteEffect => SpriteEffects.None;
        #endregion
        #endregion
        #region Common AI and collision logic (override TopAI and OnTopCollide for custom behavior)
        /// <summary>
        /// Returns true if the projectile is inside the screen rectangle expanded by <paramref name="margin"/> pixels.
        /// </summary>
        protected bool IsOnScreenWithMargin(int margin)
        {
            Rectangle screenRect = new Rectangle((int)(Main.screenPosition.X - margin), (int)(Main.screenPosition.Y - margin), Main.screenWidth + margin * 2, Main.screenHeight + margin * 2);
            return Projectile.Hitbox.Intersects(screenRect);
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[Projectile.type] = DefualtTextureFrames; // Set the number of frames for the projectile sprite
        }

        /// <summary>
        /// Apply the standard projectile defaults defined by this base class.
        /// Call this from derived `SetDefaults()` if you want the common defaults applied.
        /// </summary>
        protected void ApplyDefaultDefaults()
        {
            Projectile.alpha = DefaultAlpha;
            Projectile.width = DefaultWidth;
            Projectile.height = DefaultHeight;
            Projectile.tileCollide = DefaultTileCollide;
            Projectile.scale = DefaultScale;
            DrawOriginOffsetX = DefaultDrawOriginOffsetX;
            Projectile.extraUpdates = DefaultExtraUpdates;
            Projectile.usesLocalNPCImmunity = DefaultUsesLocalNPCImmunity;
            Projectile.localNPCHitCooldown = DefaultLocalNPCHitCooldown;
            ud = Projectile.extraUpdates + 1;
            Projectile.timeLeft = secondsToFrames(DefaultTimeLeftSeconds, ud);
            Projectile.light = DefaultLight;
            Projectile.penetrate = DefualtPierce;
            Projectile.friendly = DefualtFriendly;
            Projectile.hostile = DefualtHostile;
            Projectile.DamageType = DefualtDamageClass;
        }

        public override void AI()
        {
            // Check distance despawn
            if (Projectile.Distance(Main.player[Projectile.owner].Center) > DespawnDistance)
            {
                Projectile.Kill();
                return;
            }

            // Ensure this top is registered in the packed registry and keep it updated
            float radius = Math.Max(Projectile.width, Projectile.height) * 0.5f;
            TopRegistry.Register(Projectile, radius);
            TopRegistry.UpdateFromProjectile(Projectile);

            // Check collision with other tops (throttled/optimized)
            CheckTopCollision();

            // Call the derived class's custom AI
            TopAI();
        }

        
        protected virtual void TopAI()
        {
            // Default shared AI for simple tops. Derived classes can override entirely
            // or call base.TopAI() and extend.

            // Animate frames
            if (++Projectile.frameCounter >= DefualtTicksUntilNextFrame)
            {
                Projectile.frameCounter = 0;
                SoundEngine.PlaySound(DefualtFrameChangeSound, Projectile.position);
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
            
            // Hack to get a higher arc trajectory, only triggers once
            if (Projectile.ai[1] == 1) { Projectile.velocity.Y *= DefualtArcHeightMult; }

            // Gravity
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxFallSpeed) Projectile.velocity.Y = MaxFallSpeed;

            // Friction
            if (Projectile.ai[1] % FrequencyToFrames(8, ud) == 0) Projectile.velocity.X *= frictionFactor;

            // Step up logic
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
        #region  Collision (Top-Top and Top-Tile)

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {   
            fallThrough = false; // Don't fall thru platforms
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        protected virtual void CheckTopCollision()
        {
            // Only run on the final extra update to avoid duplicate checks when extraUpdates > 0
            if (!Utils.FinalExtraUpdate(Projectile))
                return;

            // Throttle checks across frames to reduce CPU usage
            Projectile.localAI[0] += 1f;
            if (((int)Projectile.localAI[0]) % CollisionCheckInterval != 0)
                return;

            // Look up this projectile's packed index in the registry
            if (!TopRegistry.TryGetIndex(Projectile.whoAmI, out int myIdx))
                return;

            // Query nearby packed indices into a reusable buffer
            float searchRadius = CollisionCheckRadius + Math.Max(Projectile.width, Projectile.height) * 0.5f;
            TopRegistry.QueryNearbyIndicesNonAlloc(myIdx, searchRadius, neighborBuffer);

            // Iterate neighbors and run precise checks against live projectiles
            for (int i = 0; i < neighborBuffer.Count; i++)
            {
                int packedIdx = neighborBuffer[i];
                if (packedIdx < 0 || packedIdx >= TopRegistry.Count) continue;
                int otherProjId = TopRegistry.projIds[packedIdx];
                if (otherProjId < 0 || otherProjId >= Main.maxProjectiles) continue;

                Projectile other = Main.projectile[otherProjId];
                if (other == null || !other.active) continue;
                if (other.identity == Projectile.identity) continue;
                if (SkipSameOwner && other.owner == Projectile.owner) continue;

                // Precise hitbox test
                if (Projectile.Hitbox.Intersects(other.Hitbox))
                {
                    OnTopCollide(other);
                }
            }
        }

        protected virtual void OnTopCollide(Projectile other)
        {
            // Override this in derived classes to handle top collisions
            Projectile.velocity = -Projectile.oldVelocity;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {

            if (Projectile.ai[1] % FrequencyToFrames(3, ud) == 0)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }
            
            return false;
        }
        #endregion
        #region Kill
        public override void OnKill(int timeLeft)
        {
            // Remove from packed registry when the projectile dies
            TopRegistry.Unregister(Projectile.whoAmI);

            // Spawn some dust on kill
            DustExplosion(Projectile.Center, KillDustType, 12, 0.5f, 2.5f);

            base.OnKill(timeLeft);
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


            // Applying lighting and draw current frame
            Color drawColor = Projectile.GetAlpha(lightColor);
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
                    drawColor, 
                    Projectile.rotation, 
                    origin, 
                    Projectile.scale, 
                    spriteEffects, 
                    0);
                }
            }
            // It's important to return false, otherwise we also draw the original texture.
            return false;
        }
        #endregion
    }
}