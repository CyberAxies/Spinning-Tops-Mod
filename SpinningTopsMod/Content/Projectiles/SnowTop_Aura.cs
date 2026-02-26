using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static SpinningTopsMod.Utils;

namespace SpinningTopsMod.Content.Projectiles
{
    class SnowTop_Aura : Aura
    {
        #region Override virtual params
        protected override int NPCHitCooldown => 12; // frames
        protected override float radius => 200f;
        protected override int defaultWidth => (int)radius * 2;
        protected override int defaultHeight => (int)radius * 2;
        protected override Color? DefaultAuraColor => Color.SkyBlue;
        protected override bool renderInFrontOfTiles => true;
        public override string Texture => "SpinningTopsMod/Assets/Textures/AuraCircle";
        protected override float defaultAlpha => 0.96f;
        protected override float defaultLight => 0.1f;
        protected override bool EnableBrightnessPulse => true;
        protected override float BrightnessPulseAmplitude => 0.03f; // as a fraction of the defualt
        protected override bool IDStaticNPCHitImmunity => false;
        
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.timeLeft = secondsToFrames(120);
        }
        #endregion

        #region AI
        protected override void applyAuraEffects(NPC target)
        {
            target.AddBuff(BuffID.Slow, 2);
        }
        #endregion
        #region HIT
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(Vector2.Distance(target.Center, Projectile.Center)<= radius)
            {
             base.OnHitNPC(target, hit, damageDone);
            }
            
        }
        #endregion
    }
}