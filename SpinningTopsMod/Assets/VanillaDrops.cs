using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace SpinningTopsMod.Content
{
    public class VanillaDrops : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Check the type of NPC killed
            if (npc.type == NPCID.Crimera || npc.type == NPCID.BloodCrawler ||
                npc.type == NPCID.BloodCrawlerWall || npc.type == NPCID.FaceMonster ||
                npc.type == NPCID.CrimsonGoldfish)
            {
                // Add a drop for the Bloodshed item 
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Bloodshed>(), 100, 1, 1));// 1/100 drop chance, dropping min 1 item and max 1 item
            }

            if (npc.type == NPCID.EaterofSouls || System.Array.IndexOf(new int[] {NPCID.DevourerBody, NPCID.DevourerHead, NPCID.DevourerTail}, npc.type) > -1)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Blight>(), 100, 1, 1));
            }
        }
    }
}