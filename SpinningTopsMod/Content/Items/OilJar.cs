using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpinningTopsMod.Content.Items
{
    public class OilJar : ModItem
    {
        public override void SetDefaults()
        {   
            Item.damage = 1;
			Item.crit = 0 -4; // Player has a base crit chance of 4, so we subtract it to get the actual crit chance (Let projectile handle crits this time)
			Item.knockBack = 5f;
			Item.useTime = 20; // How long it takes to use the item (fire rate)
			Item.useAnimation = 20; // How long the item is used for (animation time)
			Item.DamageType = DamageClass.Generic;
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.buyPrice(copper: 80);
			Item.shoot = ModContent.ProjectileType<Projectiles.Oil>();
			Item.shootSpeed = 8f;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.scale = 0.375f;
			Item.consumable = true; // Reduces stack count by 1 when thrown
			Item.maxStack = 999;
			Item.ResearchUnlockCount = 99;
        }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(10);
			recipe.AddIngredient(ItemID.Bottle, 1);
			recipe.AddIngredient(ItemID.Gel, 10);
			recipe.Register();

			Recipe extracted = CreateRecipe(100);
			extracted.AddIngredient(ItemID.Bottle, 1);
			extracted.AddIngredient(ItemID.Gel, 20);
			extracted.AddTile(TileID.Solidifier);
			extracted.Register();
		}
		
    }
}