using BepInEx;
using System;
using UnityEngine.Networking;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Security.Cryptography;

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace Phase4Tweaks
{
    [BepInPlugin("com.Moffein.Phase4Tweaks", "Phase4Tweaks", "1.0.0")]
    public class Phase4TweaksPlugin : BaseUnityPlugin
    {
        private static BodyIndex brotherHurtIndex;

        private void Awake()
        {
            RoR2Application.onLoad += OnLoad;
            GameObject brotherHurtPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();
            brotherHurtPrefab.AddComponent<GuaranteedItemReturnComponent>();

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (!NetworkServer.active || damageInfo.rejected || !damageInfo.attacker || self.body.bodyIndex != brotherHurtIndex) return;

            GuaranteedItemReturnComponent g = self.GetComponent<GuaranteedItemReturnComponent>();
            if (!g || !g.ShouldGuaranteedSteal(damageInfo.attacker)) return;

            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!attackerBody || !attackerBody.inventory) return;

            ReturnStolenItemsOnGettingHit rsi = self.GetComponent<ReturnStolenItemsOnGettingHit>();
            if (rsi && rsi.itemStealController && rsi.itemStealController.ReclaimItemForInventory(attackerBody.inventory))
            {
                Debug.Log("Phase4Tweaks: Guaranteed item return for " + attackerBody);
                g.SetStealItemCooldown(damageInfo.attacker);
            }
        }

        private void OnLoad()
        {
            brotherHurtIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");
        }
    }
}
