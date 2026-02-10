using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria; 
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Utilities;

using SpinningTopsMod.Content.Projectiles;

namespace SpinningTopsMod.Projectiles
{
    public partial class GlobalProjectileOverride : GlobalProjectile
    {
       public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        
       
    }
}
