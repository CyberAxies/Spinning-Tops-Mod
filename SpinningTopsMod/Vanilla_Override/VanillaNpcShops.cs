using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpinningTopsMod.GlobalNPCs
{
    public class ShopNPC : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if(shop.NpcType == NPCID.Merchant)
            {
                shop.Add(ModContent.ItemType<Content.Items.OilJar>());
            }
        }        
        
    }
}