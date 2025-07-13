using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ProximityBossScaling {
    public class ConfigToggle : ModConfig {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Enabled")]
        [DefaultValue(true)]
        public bool Toggle;
    }
}