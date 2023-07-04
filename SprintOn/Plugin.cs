using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using GYKHelper;
using UnityEngine;

namespace SprintOn;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "adisusioh31.gyk.SprintOn";
    private const string PluginName = "SprintOn";
    private const string PluginVer = "1.0.0";

    internal static ManualLogSource Log { get; set; }
    private static Harmony Harmony { get; set; }

    internal static ConfigEntry<bool> ModEnabled { get; set; }
    internal static ConfigEntry<float> SprintSpeed { get; set; }
    internal static ConfigEntry<float> EnergyConsumption { get; set; }
    internal static ConfigEntry<KeyboardShortcut> SprintKeyBind { get; set; }
    internal static ConfigEntry<string> SprintControllerButton { get; set; }


    private void Awake()
    {
        Log = Logger;
        Harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        ModEnabled = Config.Bind("1. General", "Enabled", true, new ConfigDescription($"Enable or disable {PluginName}", null, new ConfigurationManagerAttributes {Order = 10}));
        ModEnabled.SettingChanged += ApplyPatches;

        SprintSpeed = Config.Bind("2. Sprint", "Sprint Speed", 8.0f, new ConfigDescription("Sprint speed.", null, new ConfigurationManagerAttributes {Order = 9}));

        EnergyConsumption = Config.Bind("2. Sprint", "Enegy Consumption", 0.01f, new ConfigDescription("Energy consumption while sprint.", null, new ConfigurationManagerAttributes {Order = 8}));

        SprintKeyBind = Config.Bind("3. Control", "Sprint Key Bind", new KeyboardShortcut(KeyCode.LeftShift), new ConfigDescription("Key bind for sprint.", null, new ConfigurationManagerAttributes {Order = 6}));

        SprintControllerButton = Config.Bind("6. Controls", "Sprint Controller Button", Enum.GetName(typeof(GamePadButton), GamePadButton.LB), new ConfigDescription("Controller button for manually saving the game.", new AcceptableValueList<string>(Enum.GetNames(typeof(GamePadButton))), new ConfigurationManagerAttributes {Order = 5}));

        Patches._sprintingIsToggled = false;
        Patches._IsKeyPressed = false;
        Patches._IsPadButtonPressed = false;
    }


    private static void ApplyPatches(object sender, EventArgs eventArgs)
    {
        if (ModEnabled.Value)
        {
            Log.LogInfo($"Applying patches for {PluginName}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Log.LogInfo($"Removing patches for {PluginName}");
            Harmony.UnpatchSelf();
        }
    }

    private void Update()
    {
        if (!MainGame.game_started) return;

        if (SprintKeyBind.Value.IsDown())
        {
            if(Patches._IsKeyPressed == false)
            {
                Patches._IsKeyPressed = true;
            }
        }

        if (LazyInput.gamepad_active && ReInput.players.GetPlayer(0).GetButtonDown(Plugin.SprintControllerButton.Value))
        {
            if (Patches._IsPadButtonPressed == false)
            {
                Patches._IsPadButtonPressed = true;
            }
        }
    }
}
