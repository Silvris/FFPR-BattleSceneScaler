using HarmonyLib;
using Last.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FFPR_BattleSceneScaler
{
    [HarmonyPatch(typeof(BattlePlayerEntity),nameof(BattlePlayerEntity.Init))]
    public class BattlePlayerEntity_Init
    {
        public static void Postfix(BattlePlayerEntity __instance)
        {
            GameObject playerObject = __instance.gameObject;
            if (playerObject != null) playerObject.transform.localPosition = new Vector3(playerObject.transform.localPosition.x * Configuration.Instance.PlayerScale, playerObject.transform.localPosition.y * Configuration.Instance.PlayerScale, playerObject.transform.localPosition.z);
        }
    }
    [HarmonyPatch(typeof(BattleEnemyEntity), nameof(BattleEnemyEntity.Init))]
    public class BattleEnemyEntity_Init
    {
        public static void Postfix(BattleEnemyEntity __instance)
        {
            GameObject enemyObject = __instance.gameObject;
            if (enemyObject != null) enemyObject.transform.localPosition = new Vector3(enemyObject.transform.localPosition.x * Configuration.Instance.PlayerScale, enemyObject.transform.localPosition.y * Configuration.Instance.PlayerScale, enemyObject.transform.localPosition.z);
        }
    }
}
