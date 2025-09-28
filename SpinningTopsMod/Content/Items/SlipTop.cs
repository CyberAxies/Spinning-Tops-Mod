using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SpinningTopsMod.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class SlipTop : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.SpinningTopsMod.hjson' file.
		
		public override void SetDefaults()
		{

			Item.damage = 11;
			Item.crit = 4 -4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
			Item.knockBack = 2;
			Item.useTime = 70; // How long it takes to use the item (fire rate)
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Melee;
			Item.width = 0;
			Item.height = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.sellPrice(silver: 5);
			Item.shoot = ModContent.ProjectileType<Projectiles.SnowTop>();
			Item.shootSpeed = 3.5f;
			Item.rare = ItemRarityID.Blue; // Rarity matches the recipe
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		// Import the ScaleToRange function from Assets/Functions.cs
		// This function scales a value from one range to another
		
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


		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup("SpinningTopsMod:IceBlock", 30); // Use the custom recipe group for ice blocks
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceTorch, 10);
			recipe.AddIngredient(ItemID.SlimeBlock, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
