
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpinningTopsMod.Assets
{
    // This class is used to define custom recipe groups for the mod.
    // Recipe groups allow you to use multiple items in a recipe that can be interchangeable.
    // For example, you can create a recipe group for any Silver/Tungsten bar.
    // This allows players to use either Silver or Tungsten bars in the same recipe.
    // You can also create recipe groups for other items like Gold/Platinum bars, Copper/Tin bars, etc.


    public class RecipeGroups : ModSystem
    {
        public override void AddRecipeGroups()
        {
            // Add a custom recipe group for any Silver/Tungsten bar
            RecipeGroup SilverBar = new RecipeGroup(() => "Any Silver Bar", new int[] { ItemID.SilverBar, ItemID.TungstenBar });
            RecipeGroup.RegisterGroup("SpinningTopsMod:SilverBar", SilverBar);

            // Add a custom recipe group for any Gold/Platinum bar
            RecipeGroup GoldBar = new RecipeGroup(() => "Any Gold Bar", new int[] { ItemID.GoldBar, ItemID.PlatinumBar });
            RecipeGroup.RegisterGroup("SpinningTopsMod:GoldBar", GoldBar);

            // Add a custom recipe group for any Copper/Tin bar
            RecipeGroup CopperBar = new RecipeGroup(() => "Any Copper Bar", new int[] { ItemID.CopperBar, ItemID.TinBar });
            RecipeGroup.RegisterGroup("SpinningTopsMod:CopperBar", CopperBar);

            // Add a custom recipe group for any ice block
            RecipeGroup IceBlock = new RecipeGroup(() => "Any Ice Block", new int[] { ItemID.IceBlock, ItemID.RedIceBlock,
                ItemID.PinkIceBlock, ItemID.PurpleIceBlock });
            RecipeGroup.RegisterGroup("SpinningTopsMod:IceBlock", IceBlock);

            // Add a custom recipe group for any seashell
            RecipeGroup Seashell = new RecipeGroup(() => "Any Seashell", new int[] { ItemID.Seashell, ItemID.TulipShell,
                ItemID.JunoniaShell, ItemID.LightningWhelkShell});
            RecipeGroup.RegisterGroup("SpinningTopsMod:Seashell", Seashell);
        }
    }
}
