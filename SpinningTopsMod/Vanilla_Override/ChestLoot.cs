using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SpinningTopsMod.Content // Content tag to access the mod's custom items
{
    public class ChestLoot : ModSystem
    {
        public override void PostWorldGen()
        {
            // Loop through all chests in the world
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null) continue;

                // Check if the chest is a dungeon chest
                int chestTileType = Main.tile[chest.x, chest.y].TileType;
                if (chestTileType == TileID.Containers)
                {
                    int style = Main.tile[chest.x, chest.y].TileFrameX / 36;
                    // Dungeon chest styles: 1 = Locked Gold, 2 = Locked Shadow, 3 = Locked Blue, 4 = Locked Green, 5 = Locked Pink
                    if (style == 1 || style == 3 || style == 4 || style == 5)
                    {
                        // 20% chance to add 1 Rich Mahogany
                        if (WorldGen.genRand.NextFloat() < 0.20f)
                        {
                            for (int slot = 0; slot < Chest.maxItems; slot++)
                            {
                                if (chest.item[slot].type == ItemID.None)
                                {
                                    chest.item[slot].SetDefaults(ItemID.RichMahogany); // Replace with mod item later
                                    chest.item[slot].stack = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}