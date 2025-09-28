using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;


namespace SpinningTopsMod.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class ShreddingWind : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.SpinningTopsMod.hjson' file.'
		public override void SetDefaults()
		{

			Item.damage = 1;
			Item.crit = 0 -4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
			Item.knockBack = 3;
			Item.useTime = 60; // How long it takes to use the item (fire rate)
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Melee;
			Item.width = 0;
			Item.height = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.sellPrice(copper: 50);
			Item.shoot = ModContent.ProjectileType<Projectiles.N_edgeTOP>();
			Item.shootSpeed = 2.5f;
			Item.rare = ItemRarityID.Orange;
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
				float scale = Functions.ScaleToRange(dist.Length(), 0f, maxDistance, 0f, Item.shootSpeed); // Scale the shoot speed based on the distance to the mouse cursor
				velocity = direction * scale; // Set the velocity based on the direction and scaled speed
			}
			
			position += Vector2.Normalize(velocity) * 16f; // Offset the position by 10 pixels in the direction of the velocity
			base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
		}

		public override void AddRecipes()
		{
			Recipe recipe1 = CreateRecipe();
			recipe1.AddIngredient<Dungeoneer>(1);
			recipe1.AddIngredient<WildSpore>(1);
			recipe1.AddIngredient<Caldera>(1);
			recipe1.AddIngredient<Blight>(1);
			recipe1.AddTile(TileID.Anvils);
			recipe1.AddCondition(Condition.DownedSkeletron);
			recipe1.Register();

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient<Dungeoneer>(1);
			recipe2.AddIngredient<WildSpore>(1);
			recipe2.AddIngredient<Caldera>(1);
			recipe2.AddIngredient<Bloodshed>(1);
			recipe2.AddTile(TileID.Anvils);
			recipe2.AddCondition(Condition.DownedSkeletron);
			recipe2.Register();
		}
	}
}
