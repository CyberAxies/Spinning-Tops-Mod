using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace SpinningTopsMod
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class SpinningTopsMod : Mod
	{
		public override void Load()
		{
			// Code to run when the mod is loaded
			if (Main.netMode != NetmodeID.Server)
			{
				// Client-side only code
				////////////////////////////////////////////////
					
			}
		}
	}
}

