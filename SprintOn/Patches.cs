using BepInEx.Logging;
using HarmonyLib;
using LazyBearGames.Preloader;
using Rewired;
using System.Reflection;
using System;
using UnityEngine;

namespace SprintOn
{
    [HarmonyPatch]
    public static class Patches
    {
        internal static bool _sprintingIsToggled { get; set; }
        internal static bool _IsKeyPressed { get; set; }
        internal static bool _IsPadButtonPressed { get; set; }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MovementComponent), nameof(MovementComponent.UpdateMovement))]
        public static void MovementComponentUpdatePatch(MovementComponent __instance)
        {
            if (__instance.wgo.is_player)
            {
                if (_IsKeyPressed == true || _IsPadButtonPressed == true)
                {
                    Patches.SetSpeed(__instance, _sprintingIsToggled ? LazyConsts.PLAYER_SPEED : Plugin.SprintSpeed.Value);
                    _sprintingIsToggled = _sprintingIsToggled != true;
                    _IsKeyPressed = false;
                    _IsPadButtonPressed = false;
                }
                if (_sprintingIsToggled && !Patches.DrainEnergy(__instance))
                {
                    Patches.SetSpeed(__instance, LazyConsts.PLAYER_SPEED);
                    _sprintingIsToggled = false;
                    return;
                }
            }
        }

        private static bool DrainEnergy(MovementComponent instance)
        {
            if ((double)Math.Abs(LazyInput.GetDirection().x) > 0.2 || (double)Math.Abs(LazyInput.GetDirection().y) > 0.2)
            {
                return instance.wgo.components.character.player.TrySpendEnergy(Plugin.EnergyConsumption.Value);
            }
            return MainGame.me.player.energy >= Plugin.EnergyConsumption.Value;
        }

        private static void SetSpeed(MovementComponent instance, float targetSpeed)
        {
            if (instance.wgo.data.GetParam("speed", 0f) != targetSpeed)
            {
                instance.SetSpeed(targetSpeed);
            }
        }
    }
}
