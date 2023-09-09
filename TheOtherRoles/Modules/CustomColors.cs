using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using AmongUs.Data.Legacy;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Modules {
    public class CustomColors {
        protected static Dictionary<int, string> ColorStrings = new Dictionary<int, string>();
        public static List<int> lighterColors = new List<int>(){ 3, 4, 5, 7, 10, 11, 13, 14, 17 };
        public static uint pickableColors = (uint)Palette.ColorNames.Length;

        private static readonly List<int> ORDER = new List<int>() { 7, 14, 5, 33, 4, 
                                                                    30, 0, 19, 27, 3,
                                                                    17, 25, 18, 13, 23,
                                                                    8, 32, 1, 21, 31,
                                                                    10, 34, 15, 28, 22,
                                                                    29, 11, 2, 26, 16,
                                                                    20, 24, 9, 12, 6 };
        public static void Load() {
            List<StringNames> longlist = Enumerable.ToList<StringNames>(Palette.ColorNames);
            List<Color32> colorlist = Enumerable.ToList<Color32>(Palette.PlayerColors);
            List<Color32> shadowlist = Enumerable.ToList<Color32>(Palette.ShadowColors);

            List<CustomColor> colors = new List<CustomColor>();

            /* Custom Colors */
            colors.Add(new CustomColor { longname = "Navy",
                                        color = new Color32(0, 0, 128, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                                        shadow = new Color32(0, 0, 96, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Gold",
                                        color = new Color32(255, 192, 0, byte.MaxValue), 
                                        shadow = new Color32(192, 144, 0, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Olive",
                                        color = new Color32(154, 140, 61, byte.MaxValue), 
                                        shadow = new Color32(104, 95, 40, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Juniper",
                                        color = new Color32(0, 64, 0, byte.MaxValue), 
                                        shadow = new Color32(0, 48, 0, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Mint", 
                                        color = new Color32(111, 192, 156, byte.MaxValue), 
                                        shadow = new Color32(65, 148, 111, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Lavender",
                                        color = new Color32(173, 126, 201, byte.MaxValue), 
                                        shadow = new Color32(131, 58, 203, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Iris",
                                        color = new Color32(255, 80, 255, byte.MaxValue), 
                                        shadow = new Color32(192, 40, 192, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Peach",
                                        color = new Color32(255, 164, 119, byte.MaxValue), 
                                        shadow = new Color32(238, 128, 100, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Viridian",
                                        color = new Color32(64, 128, 112, byte.MaxValue), 
                                        shadow = new Color32(32, 96, 80, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Amaranth",
                                        color = new Color32(229, 43, 80, byte.MaxValue), 
                                        shadow = new Color32(189, 3, 40, byte.MaxValue),
                                        isLighterColor = true });
            colors.Add(new CustomColor { longname = "Sky", 
                                        color = new Color32(128, 128, 255, byte.MaxValue), 
                                        shadow = new Color32(96, 96, 192, byte.MaxValue),
                                        isLighterColor = false });
            colors.Add(new CustomColor { longname = "Lemon",
                                        color = new Color32(255, 255, 128, byte.MaxValue), 
                                        shadow = new Color32(192, 192, 96, byte.MaxValue), 
                                        isLighterColor = true });

            pickableColors += (uint)colors.Count; // Colors to show in Tab
            /** Hidden Colors **/     
                    
            /** Add Colors **/
            int id = 50000;
            foreach (CustomColor cc in colors) {
                longlist.Add((StringNames)id);
                CustomColors.ColorStrings[id++] = cc.longname;
                colorlist.Add(cc.color);
                shadowlist.Add(cc.shadow);
                if (cc.isLighterColor)
                    lighterColors.Add(colorlist.Count - 1);
            }

            Palette.ColorNames = longlist.ToArray();
            Palette.PlayerColors = colorlist.ToArray();
            Palette.ShadowColors = shadowlist.ToArray();
        }

        protected internal struct CustomColor {
            public string longname;
            public Color32 color;
            public Color32 shadow;
            public bool isLighterColor;
        }

        [HarmonyPatch]
        public static class CustomColorPatches {
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
            private class ColorStringPatch {
                [HarmonyPriority(Priority.Last)]
                public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name) {
                    if ((int)name >= 50000) {
                        string text = CustomColors.ColorStrings[(int)name];
                        if (text != null) {
                            __result = text;
                            return false;
                        }
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
            private static class PlayerTabEnablePatch {
                public static void Postfix(PlayerTab __instance) { // Replace instead
                    Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                    int cols = 5; // TODO: Design an algorithm to dynamically position chips to optimally fill space
                    for (int i = 0; i < ORDER.Count; i++) {
                        int pos = ORDER[i];
                        if (pos < 0 || pos > chips.Length)
                            continue;
                        ColorChip chip = chips[pos];
                        int row = i / cols, col = i % cols; // Dynamically do the positioning
                        chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f), chip.transform.localPosition.z);
                        chip.transform.localScale *= 0.78f;
                    }
                    for (int j = ORDER.Count; j < chips.Length; j++) { // If number isn't in order, hide it
                        ColorChip chip = chips[j];
                        chip.transform.localScale *= 0f; 
                        chip.enabled = false;
                        chip.Button.enabled = false;
                        chip.Button.OnClick.RemoveAllListeners();
                    }
                }
            }
            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
            private static class LoadPlayerPrefsPatch { // Fix Potential issues with broken colors
                private static bool needsPatch = false;
                public static void Prefix([HarmonyArgument(0)] bool overrideLoad) {
                    if (!LegacySaveManager.loaded || overrideLoad)
                        needsPatch = true;
                }
                public static void Postfix() {
                    if (!needsPatch) return;
                    LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                    needsPatch = false;
                }
            }
            [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
            private static class PlayerControlCheckColorPatch {
                private static bool isTaken(PlayerControl player, uint color) {
                    foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                        if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                            return true;
                    return false;
                }
                public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor) { // Fix incorrect color assignment
                    uint color = (uint)bodyColor;
                   if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length) {
                        int num = 0;
                        while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color))) {
                            color = (color + 1) % CustomColors.pickableColors;
                        }
                    }
                    __instance.RpcSetColor((byte)color);
                    return false;
                }
            }
        }
    }
}
