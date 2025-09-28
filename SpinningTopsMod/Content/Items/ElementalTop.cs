using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography.Pkcs;
using System;
using SpinningTopsMod.Content.Projectiles;


namespace SpinningTopsMod.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class ElementalTop : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.SpinningTopsMod.hjson' file.'
		public enum BiomeMode
		{
			Sky,
			Forest,
			Snow,
			Desert,
			Evil,
			Ocean,
			Jungle,
			Hallowed
		}

		BiomeMode mode; // Stores the current mode
		public void ApplyModeStats()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Pink;
			Item.autoReuse = true;
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Melee;
			Item.SetNameOverride($"Elemental Top ({mode})");

			switch (mode)
			{

				case BiomeMode.Evil:
					Item.damage = 34;
					Item.crit = 16 - 4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
					Item.knockBack = 5f;
					Item.useTime = 60; // How long it takes to use the item (fire rate)
					Item.width = 32;
					Item.height = 32;
					Item.shoot = ModContent.ProjectileType<Projectiles.Elemental_Evil>();
					Item.shootSpeed = 7.5f;
					Item.UseSound = SoundID.Item1;
					break;
				case BiomeMode.Sky:
					Item.damage = 1;
					Item.crit = 16 - 4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
					Item.knockBack = 5f;
					Item.useTime = 60; // How long it takes to use the item (fire rate)
					Item.width = 32;
					Item.height = 32;
					Item.shoot = ModContent.ProjectileType<Projectiles.WoodenTop>();
					Item.shootSpeed = 7.5f;
					Item.UseSound = SoundID.Item1;
					break;
			}
		}
		
		public override void SetDefaults()
		{
			mode = BiomeMode.Forest; // Default mode
			ApplyModeStats(); // Apply the stats for the default mode
		}


		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Vector2 dist = player.Center - Main.MouseWorld; // Get the direction from the player to the mouse cursor
			Vector2 direction = -Vector2.Normalize(dist); // Normalize the direction vector
			float maxDistance = 16f * 15; // consider the distance of the mouse up to 15 blocks away
			if (dist.Length() <= maxDistance)
			{
				float scale = Functions.ScaleToRange(dist.Length(), 0f, maxDistance, 0f, Item.shootSpeed); // Scale the shoot speed based on the distance to the mouse cursor
				velocity = direction * scale; // Set the velocity based on the direction and scaled speed
			}

			position += Vector2.Normalize(velocity) * 16f; // Offset the position by 10 pixels in the direction of the velocity
			base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
		}

		private int switchCooldown = 0; // Cooldown timer for switching modes
		public override void HoldItem(Player player)
		{
			if (switchCooldown > 0){switchCooldown--;} // Decrease the cooldown timer if it's above 0
			
				
			if (Main.mouseRight && Main.mouseRightRelease // Check if the ployer right-clicked with the item held
			&& switchCooldown == 0 // and the switch cooldown is 0
			&& !(Main.playerInventory || Main.ingameOptionsWindow))  // and no menu is open
			{
				switchCooldown = 10; // Set the cooldown to 10 frames (0.178 seconds at 60 FPS)

				if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) ||
				Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt))
				{
					mode--; // Cycle to the previous mode
					if ((int)mode < 0)
					{
						mode = (BiomeMode)(Enum.GetNames(typeof(BiomeMode)).Length - 1); // Wrap around to the last mode (length starts at 1, so we subtract 1 to get the last index)
					}
				}
				else
				{
					mode++; // Cycle to the next mode
					if ((int)mode >= Enum.GetValues(typeof(BiomeMode)).Length)
					{
						mode = 0; // Wrap around to the first mode
					}
				}

				Color BiomeColor = mode switch
				{
					BiomeMode.Sky => Color.SkyBlue,
					BiomeMode.Forest => Color.ForestGreen,
					BiomeMode.Snow => Color.LightCyan,
					BiomeMode.Desert => Color.Goldenrod,
					BiomeMode.Evil => Color.Purple,
					BiomeMode.Ocean => Color.Teal,
					BiomeMode.Jungle => Color.DarkSeaGreen,
					BiomeMode.Hallowed => Color.LightPink,
					_ => Color.White
				};

				AdvancedPopupRequest request = new()
				{
					Text = $"Switched to {mode} mode",
					Color = BiomeColor,
					DurationInFrames = 60, // Display for 1 second (60 frames)
					Velocity = new Vector2(0, -0.5f), // Float upwards
				};

				Vector2 popupPosition = player.Top + new Vector2(0, -16); // Position above the player

				PopupText.NewText(request, popupPosition);

				ApplyModeStats(); // Apply the stats for the new mode
			}

		}

		public override void AddRecipes()
		{
			Recipe recipeCorr = CreateRecipe();
			recipeCorr.AddIngredient(ItemID.Pearlwood, 15); // Hallowed items
			recipeCorr.AddIngredient(ItemID.PixieDust, 3);
			recipeCorr.AddIngredient(ItemID.Feather, 15); // Sky items
			recipeCorr.AddIngredient(ItemID.Cloud, 5);
			recipeCorr.AddIngredient(ItemID.SoulofFlight, 3);
			recipeCorr.AddIngredient(ItemID.FallenStar, 1); // shoutout space biome (no content lol)
			recipeCorr.AddIngredient<WoodenTop>(1); // basic top for forest rep
			recipeCorr.AddIngredient<WildSpore>(1); // Jungle rep
			recipeCorr.AddIngredient<Waterwalker>(1); // Ocean rep
			recipeCorr.AddIngredient<SlipTop>(1); // Snow rep
			recipeCorr.AddIngredient<OnPoint>(1); // Desert rep
			recipeCorr.AddIngredient<Blight>(1);
			recipeCorr.AddTile(TileID.Anvils);
			recipeCorr.Register();

			Recipe recipeCrim = CreateRecipe();
			recipeCrim.AddIngredient(ItemID.Pearlwood, 15); // Hallowed items
			recipeCrim.AddIngredient(ItemID.PixieDust, 3);
			recipeCrim.AddIngredient(ItemID.Feather, 15); // Sky items
			recipeCrim.AddIngredient(ItemID.Cloud, 5);
			recipeCrim.AddIngredient(ItemID.SoulofFlight, 3);
			recipeCrim.AddIngredient(ItemID.FallenStar, 1); // shoutout space biome (no content lol)
			recipeCrim.AddIngredient<WoodenTop>(1); // basic top for forest rep
			recipeCrim.AddIngredient<WildSpore>(1); // Jungle rep
			recipeCrim.AddIngredient<Waterwalker>(1); // Ocean rep
			recipeCrim.AddIngredient<SlipTop>(1); // Snow rep
			recipeCrim.AddIngredient<OnPoint>(1); // Desert rep
			recipeCrim.AddIngredient<Bloodshed>(1);
			recipeCrim.AddTile(TileID.Anvils);
			recipeCrim.Register();
		}
	}
}
