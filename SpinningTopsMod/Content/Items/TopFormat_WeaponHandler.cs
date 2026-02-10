using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static SpinningTopsMod.Utils;
using SpinningTopsMod;


namespace SpinningTopsMod.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class BlueprintTopHandler : ModItem
	{
        #region Initialization

        public override string Texture => "SpinningTopsMod/Assets/Textures/Top_glow";
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.SpinningTopsMod.hjson' file.'
        float maxMouse_distance = tilesToPixels(15); // How far the mouse can be to reach max shoot speed
		public override void SetDefaults()
		{
            // WEAPON STATS
			Item.damage = 8;
			Item.crit = 8 -4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance
			Item.knockBack = 3;
            Item.shootSpeed = 2.5f;
			Item.useTime = 60; // How long it takes to use the item (fire rate)
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Melee;

            // HITBOX HANDLER
			Item.width = 0;
			Item.height = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung

            // THE SINGLE MOST IMPORTANT LINE OF CODE
            Item.shoot = ModContent.ProjectileType<Projectiles.NobodyKnows>(); // Projectile this item shoots
            // MISCELLANEOUS STATS
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}
        #endregion
        #region shooting handler
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{   
            // Shoot slower when the mouse is closer to the player
			Vector2 dist = player.Center - Main.MouseWorld; // Get the direction from the player to the mouse cursor
			Vector2 direction = -Vector2.Normalize(dist); // Normalize the direction vector
			if (dist.Length() <= maxMouse_distance)
			{
				float scale = ScaleToRange(dist.Length(), 0f, maxMouse_distance, 0f, Item.shootSpeed); // Scale the shoot speed based on the distance to the mouse cursor
				velocity = direction * scale; // Set the velocity based on the direction and scaled speed
			}
			
            // Slightly offset the spawn position so the player can shoot down thru platforms
			position += Vector2.Normalize(velocity) * 16f; // Offset the position by 10 pixels in the direction of the velocity
			base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);

            // You can add more modifications to the shoot stats here, if needed
		}
        #endregion
        #region recipes
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 15);
			recipe.AddIngredient(ItemID.Cobweb, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
        #endregion
	}
}
