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
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true; // Prevents the item from doing damage when swung
			Item.value = Item.sellPrice(copper: 35);
			Item.shoot = ModContent.ProjectileType<Projectiles.Oil>();
			Item.shootSpeed = 6f;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
        }
    }
}