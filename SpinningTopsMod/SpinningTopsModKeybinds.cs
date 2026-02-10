using Terraria.ModLoader;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace SpinningTopsMod
{
    public class SpinningTopsModKeybinds : ModSystem
    {
        public static ModKeybind ElementalTopReverseCycle { get; private set; }

        // Load and register default keybinds
        public override void Load()
        {
            ElementalTopReverseCycle = KeybindLoader.RegisterKeybind(Mod, "CycleModesReverse", Keys.LeftAlt);
        }

        // Unload after we're done
        public override void Unload()
        {
            ElementalTopReverseCycle = null;
        }
    }
}