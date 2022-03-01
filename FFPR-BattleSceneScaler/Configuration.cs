using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FFPR_BattleSceneScaler
{
    public class Configuration
    {
        private const string Group = "BattleSceneScaler";
        internal const string CTG_ID = "Battle Scene Scaler";
        private static Vector2[] Resolutions = { new Vector2(384, 216), new Vector2(320, 180) };
        public static Configuration Instance { get; set; }
        public ConfigEntry<int> ScalingEntry { get; set; }
        public ConfigEntry<int> EnemyScalingEntry { get; set; }
        public ConfigEntry<int> PlayerScalingEntry { get; set; }
        public ConfigEntry<int> BackgroundScalingEntry { get; set; }
        public ConfigEntry<bool> AspectEntry { get; set; }

        public Configuration(ConfigFile file)
        {
            Instance = this;
            AspectEntry = file.Bind(new ConfigDefinition(Group, "Align Aspect Ratio"), false, new ConfigDescription("Align the aspect ratio of the battle scene to that of the field scene.\n" +
                "The default resolution of the battle scene is 384x216, which is a different resolution than the field scene (320x180).\n" +
                "This creates a disparity in the size of pixels that can be frustrating to those using CRT shaders."));
            ScalingEntry = file.Bind(new ConfigDefinition(Group, "Scene Scale"), 1, new ConfigDescription("The integer value in which to scale the scene by.\n" +
                "This will scale all (non-enemy, player, or background) objects in the scene, as well as the image outputs of the scene."));
            PlayerScalingEntry = file.Bind(new ConfigDefinition(Group, "Player Scale"), 1, new ConfigDescription("The integer value in which to scale the player objects by.\n" +
                "If you are not using custom-scaled sprites, this should match the Scene Scale."));
            EnemyScalingEntry = file.Bind(new ConfigDefinition(Group, "Enemy Scale"), 1, new ConfigDescription("The integer value in which to scale the enemy objects by.\n" +
                "If you are not using custom-scaled sprites, this should match the Scene Scale."));
            BackgroundScalingEntry = file.Bind(new ConfigDefinition(Group, "Background Scale"), 1, new ConfigDescription("The integer value in which to scale the battle backgrounds by.\n" +
                "If you are not using custom-scaled backgrounds, this should match the Scene Scale."));

            AspectEntry.SettingChanged += ApplyChanges;
            ScalingEntry.SettingChanged += ApplyChanges;
            PlayerScalingEntry.SettingChanged += ApplyChanges;
            EnemyScalingEntry.SettingChanged += ApplyChanges;
            BackgroundScalingEntry.SettingChanged += ApplyChanges;
        }


        public bool AlignAspect => AspectEntry.Value;
        public int SceneScale => ScalingEntry.Value;
        public int EnemyScale => EnemyScalingEntry.Value;
        public int PlayerScale => PlayerScalingEntry.Value;
        public int BackgroundScale => BackgroundScalingEntry.Value;
        public void ApplyChanges(object sender, EventArgs e)
        {
            if (AlignAspect)
            {
                ModComponent.SetScale(Resolutions[1]);
            }
            else
            {
                ModComponent.SetScale(Resolutions[0]);
            }
        }
    }
}
