using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;
using HarmonyLib;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Dissonant Edge - Lunar Item
    /// <para>All attacks deal increased damage to entities with a lower health percentage than the user.</para>
    /// <para>All attacks deal reduced damage to entities with a higher health percentage than the user.</para>
    /// </summary>
    public class DissonantEdge : Item
    {

        public override string Name => "Dissonant Edge";
        public override string NameToken => "DISSONANTEDGE";
        public override string PickupToken => "Increased damage to foes with a lower health percentage. <style=cIsHealth> Reduced damage to foes with a higher health percentage.</style>";
        public override string Description => $"<style=cIsDamage>All attacks</style> deal +{damageIncrease.Value*100f}% <style=cStack>(+{damageIncrease.Value*100f}% per stack)</style> increased damage if the target's <style=cIsUtility>current health percentage is lower than yours</style>. <style=cIsHealth>All attacks deal {damageDecrease.Value*100f}% <style=cDeath>REDUCED</style> damage if the target's current health percentage is higher than yours.</style>";
        public override string Lore => "<style=cLunarObjective>These creatures are imperfect. A flawed design. Unstable, temperamental, yet fragile.\n\nMy constructs are streamlined. Deadly, efficient, yet simple.\n\nWhy can't He see that? Is he so blinded by love? He is a fool. But one day He will see, and He will understand.</style>";

        public override ItemTier Tier => ItemTier.Lunar;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("DissonantEdge.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("DissonantEdge.prefab");

        public override float minViewport => 2f;
        public override float maxViewport => 3f;

        private static ConfigEntry<float> damageIncrease;
        private static ConfigEntry<float> damageDecrease;

        private static ItemDef staticItemDef;

        public override void CreateItem()
        {
            base.CreateItem();
            staticItemDef = itemDef;
        }

        /// <summary>
        /// Dissonent Edge implementation. Called via CharacterBody.TakeDamage as a Harmony prefix patch.
        /// </summary>
        public static bool TakeDamagePatch(RoR2.HealthComponent __instance, DamageInfo damageInfo)
        {
            // Skip invalid damage types (DOTs and damage that are neutral and don't have an attacker such as fall damage and void fog).
            if (!damageInfo.attacker ||
            damageInfo.damageType == DamageType.DoT || damageInfo.damageType == DamageType.NonLethal || damageInfo.damageType == DamageType.BypassBlock)
            {
                return true;
            }

            if ( !__instance  || !__instance.body || !__instance.body.inventory)
            {
                return true;
            }

            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!attacker || !attacker.inventory || !attacker.healthComponent)
            {
                return true;
            }

            var count = attacker.inventory.GetItemCount(staticItemDef);
            if (count <= 0)
            {
                return true;
            }

            var attackerHealth = attacker.healthComponent.combinedHealthFraction;
            var victimHealth = __instance.combinedHealthFraction;

            // Damage modifier of the attack increases by 10% per stack (With base values) if the attacker has more health than the victim.
            // Damage modifier of the attack decreases by 25% (With base values) if the attacker has less health than the victim, regardless of stack count.
            var damageScalar = 1f + (attackerHealth >= victimHealth ? damageIncrease.Value * count : -damageDecrease.Value);

            damageInfo.damage *= damageScalar;

            // Prevent negative and zero damage, the minimum damage is 1.
            if (damageInfo.damage <= 1f) { damageInfo.damage = 1f; }

            return true;
        }

        public override void AddConfigOptions()
        {
            damageIncrease = configFile.Bind("Dissonant_Edge Config", "damageIncrease", 0.1f, "The damage increase granted by this item per stack (0.1 = +10%).");
            damageDecrease = configFile.Bind("Dissonant_Edge Config", "damageDecrease", 0.25f, "The damage decrease inflicted by this item if under the health threshold. (0.25 = 25% reduced damage).");
        }
    }
}