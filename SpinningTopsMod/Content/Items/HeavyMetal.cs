using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SpinningTopsMod.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class HeavyMetal : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.SpinningTopsMod.hjson' file.
		public override void SetDefaults()
		{
			
			Item.damage = 22; 
			Item.crit = 4 -4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
			Item.knockBack = 5;
			Item.useTime = 80; // How long it takes to use the item (fire rate)
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Melee;
			Item.width = 0;
			Item.height = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.sellPrice(silver: 10);
			Item.shoot = ModContent.ProjectileType<Projectiles.MetalTop>();
			Item.shootSpeed = 2f;
			Item.rare = ItemRarityID.Blue; // The rarity of the item matches prehardmode ores in the recipe
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Vector2 dist = player.Center - Main.MouseWorld; // Get the direction from the player to the mouse cursor
			Vector2 direction = -Vector2.Normalize(dist); // Normalize the direction vector
			float maxDistance = 16f * 15; // consider the distance of the mouse up to 15 blocks away
			if (dist.Length() <= maxDistance)
			{
				float scale = Utils.ScaleToRange(dist.Length(), 0f, maxDistance, 0f, Item.shootSpeed); // Scale the shoot speed based on the distance to the mouse cursor
				velocity = direction * scale; // Set the velocity based on the direction and scaled speed
			}

			position += Vector2.Normalize(velocity) * 16f; // Offset the position by 10 pixels in the direction of the velocity
			
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar , 13); // Use the defualt recipe group for Iron/Lead bars
			recipe.AddRecipeGroup("SpinningTopsMod:SilverBar", 11); // Use the custom recipe group for Silver/Tungsten bars
			recipe.AddRecipeGroup("SpinningTopsMod:GoldBar", 7); // Use the custom recipe group for Gold/Platinum bars
			recipe.AddIngredient(ItemID.Diamond, 1); // Rare gem, so you can't craft it early
			recipe.AddTile(TileID.Hellforge); // Need to explore hell to craft this item
			recipe.Register();
		}
	}
}
