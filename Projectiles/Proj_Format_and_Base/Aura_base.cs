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
using rail;
using Humanizer;

namespace SpinningTopsMod.Content.Projectiles
{
    public abstract class Aura : ModProjectile
    {
        // ----- Configurable defaults, virtual parameters to be overridden by each aura ----- //

        #region Virtual Parameters
        #region Behavior params
        protected virtual int MaxHits => -1; // Aura hits limit, -1 for infinite
        protected virtual bool IDStaticNPCHitImmunity => true; // Uses static, rather than local
        protected virtual int NPCHitCooldown => 6; 
        protected virtual int defaultWidth => (int)tilesToPixels(10);
        protected virtual int defaultHeight => (int)tilesToPixels(10);
        protected virtual float defaultKnockback => 0f;
        protected virtual int defaultTimeLeft => -1; // -1 for infinite, kill manually
        protected virtual int defaultPierce => -1; // -1 for infinite
        protected virtual bool defaultFriendly => true;
        protected virtual bool defaultHostile => false;
        protected virtual bool defualtTileCollide => false;
        protected virtual float radius => Math.Max(Projectile.width, Projectile.height) / 2f; // for circle, if elipse- override AI
        #endregion
        #region Rendering params
        /// <summary>
        /// Gets the Aura texture from assets. NOTICE: Texture is grayscale, the color can be changed by changing the color parameter in the spritebatch.draw in the PostDraw method. This is done to allow for more customization and dynamic coloring of the aura without needing multiple textures.
        /// </summary>
        /// <typeparam name="Texture2D"></typeparam>
        /// <returns></returns>
        protected virtual Texture2D AuraTexture => ModContent.Request<Texture2D>("SpinningTopsMod/Assets/Textures/AuraCircle").Value;
        protected virtual bool renderInFrontOfTiles => false; // hidden by tiles
        /// <summary>
        /// The default alpha of the proj, 0 for fully opaque, 1 for fully transparent
        /// </summary> 
        protected virtual float defaultAlpha => 0.95f; 
        /// <summary>
        /// Alpha scaled to 0-255 for drawing
        /// </summary>
        /// <returns> Returns the defaultAlpha scaled to 0-255</returns>
        protected virtual int alphaScaled => (int)ScaleToRange(defaultAlpha, 0, 1, 0, 255);
        protected virtual float defaultLight => 0;
        protected virtual Color? lightColor => null; // If null, uses default white light
        // Default color to tint the grayscale aura texture. Override to change per-class default.
        protected virtual Color? DefaultAuraColor => null;
        // Per-instance override: set this on the projectile instance to change its color at runtime.
        protected Color? auraColor = null;
        /// <summary>
        /// Scale adjustment multiplier for the aura texture. Use this if the rendered circle doesn't fit the hitbox correctly.
        /// Default 1.0 scales the texture width to match the hitbox width.
        /// </summary>
        protected virtual float TextureScaleMultiplier => 1f;
        // Fade settings (in ticks). Override to change per-class defaults.
        protected virtual int FadeInDuration => 30;
        protected virtual int FadeOutDuration => 30;
        // Internal fade state
        int fadeTimer = 0;
        bool isFadingOut = false;
        bool hasFadedIn = false;
        // Brightness pulsing (sinusoidal)
        protected virtual bool EnableBrightnessPulse => true;
        protected virtual float BrightnessPulseFrequency => 0.5f; // Hz
        protected virtual float BrightnessPulseAmplitude => 0.25f; // relative to default light (±25%)
        protected virtual float BrightnessPulsePhase => 0f;
        float brightnessTimer = 0f;
        #endregion
        #endregion
        #region Initialization
        public override void SetDefaults()
        {
            Projectile.width = defaultWidth;
            Projectile.height = defaultHeight;
            Projectile.timeLeft = defaultTimeLeft;
            Projectile.penetrate = defaultPierce;
            Projectile.friendly = defaultFriendly;
            Projectile.hostile = defaultHostile;
            Projectile.tileCollide = defualtTileCollide;
            // start fully transparent and fade into the configured alpha
            Projectile.alpha = 255;
            // init pulse params
            brightnessTimer = 0f;
            Projectile.light = defaultLight;
            Projectile.ai[0] = 0; // hit counter
            // Scale the projectile's sprite to match its hitbox size
            try
            {
                Texture2D tex = AuraTexture;
                if (tex != null && tex.Width > 0)
                    Projectile.scale = (Projectile.width / (float)tex.Width) * TextureScaleMultiplier;
            }
            catch
            {
            }
            if (IDStaticNPCHitImmunity)
            {
                Projectile.usesIDStaticNPCImmunity = IDStaticNPCHitImmunity;
                Projectile.idStaticNPCHitCooldown = NPCHitCooldown;
            }
            else
            {
                Projectile.usesLocalNPCImmunity = !IDStaticNPCHitImmunity;
                Projectile.localNPCHitCooldown = NPCHitCooldown;
            }
        }
        #endregion
        #region AI

        public override void AI()
        {
            // DO NOT USE FOR HITTING OR DEALING DAMAGE, use OnHitNPC instead
            foreach (NPC target in Utils.AllNPCsWithinRangeOfPoint(Projectile.Center, radius))
            {
                applyAuraEffects(target);
            
            }
            // Moved hit logic to OnHitNPC to use the built in iframes
            // add a slow fade in and out effect to make the aura look smoother when it spawns and despawns

            // Handle fade-in
            if (!hasFadedIn)
            {
                fadeTimer++;
                float t = Math.Min(1f, fadeTimer / (float)Math.Max(1, FadeInDuration));
                int target = alphaScaled;
                Projectile.alpha = (int)MathHelper.Lerp(255f, target, Sinusoidal(t));
                if (t >= 1f)
                {
                    hasFadedIn = true;
                    fadeTimer = 0;
                }
            }

            // Handle fade-out
            if (isFadingOut)
            {
                fadeTimer++;
                float t = Math.Min(1f, fadeTimer / (float)Math.Max(1, FadeOutDuration));
                int target = alphaScaled;
                Projectile.alpha = (int)MathHelper.Lerp(target, 255f, Sinusoidal(t));
                if (t >= 1f)
                {
                    Projectile.Kill();
                }
            }
            // Add a sinusoidal pulsing to the light and alpha to make the aura look more dynamic and magical
            // Brightness pulsing
            if (EnableBrightnessPulse)
            {
                brightnessTimer += 1f / 60f;
                float freq = BrightnessPulseFrequency;
                float amp = BrightnessPulseAmplitude;
                float phase = BrightnessPulsePhase;
                float sinv = (float)Math.Sin((brightnessTimer + phase) * twoPi * freq);
                float sinScaled = (sinv + 1f) / 2f; // scale to 0-1
                // vary around default light
                float baseLight = defaultLight;
                float mult = 1f + amp * sinv;
                float newLight = baseLight * mult;
                if (newLight < 0f) newLight = 0f;
                Projectile.light = newLight;
                // vary alpha in sync with brightness for extra effect
                int baseAlpha = alphaScaled;
                float alphaChange = amp * baseAlpha * sinScaled;
                int newAlpha = baseAlpha - (int)(alphaChange);
                newAlpha = Math.Clamp(newAlpha, 0, 255);
                Projectile.alpha = newAlpha;
            }

        }

        protected virtual void applyAuraEffects(NPC target)
        {
            // Override for effects
            
        }
        #endregion
        #region NPC hits

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {   
            // if it's in the circle
            
        }

        // Start fading out; will call Kill() when fade completes.
        protected void StartFadeOut()
        {
            if (isFadingOut)
                return;
            isFadingOut = true;
            fadeTimer = 0;
            // ensure projectile doesn't auto-expire while fading
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, Math.Max(FadeInDuration + 2, FadeOutDuration + 2));
        }
        #endregion
        #region Drawing
     
        public override bool PreDraw(ref Color lightColor)
        {
            if (renderInFrontOfTiles)
            {
                Texture2D tex = AuraTexture;
                if (tex != null)
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    Color baseColor = (auraColor ?? DefaultAuraColor) ?? Color.White;
                    float visible = 1f - Projectile.alpha / 255f;
                    visible = MathHelper.Clamp(visible, 0f, 1f);
                    Color drawColor = new Color((byte)(baseColor.R * visible), (byte)(baseColor.G * visible), (byte)(baseColor.B * visible), (byte)(255f * visible));
                    Main.spriteBatch.Draw(tex, drawPos, null, drawColor, Projectile.rotation, new Vector2(tex.Width / 2f, tex.Height / 2f), Projectile.scale, SpriteEffects.None, layerDepth: 0.9f);
                }
            }
            else
            {
                // TODO: Make it draw behind tiles
            }

            return false;
        }
        

        #endregion
    }
}