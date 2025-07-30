using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Coolant Pack - T2 (Uncommon) Item
    /// <para>Incoming damaging debuffs deal reduced damage.</para>
    /// <para>Hyperbolic scaling so that 100% damage reduction cannot be reached.</para>
    /// </summary>
    public class CoolantPack : Item
    {
        public override string Name => "Coolant Pack";
        public override string NameToken => "COOLANTPACK";
        public override string PickupToken => "Incoming damaging debuffs inflict less damage";
        public override string Description => $"All incoming <style=cIsHealth>damaging debuffs</style> inflict {damageReduction.Value * 100f}% <style=cStack>(+{damageReduction.Value * 100f}% per stack)</style> <style=cIsDamage>reduced damage</style>.";
        public override string Lore => "Introducing MediFreeze(tm)! Your omni-purpose solution to all burns, aches and ailments.\n\nCut your finger? Seal that wound up with MediFreeze!\nBurned Yourself? MediFreeze it away!\nBruised your head? MediFreeze it into oblivion!\n\nMediFreeze, resolving your ailments one chill at a time. Buy now today at your nearest wholesaler.\n\nLEGAL DISCLAIMER: Medical trials pending. May contain toxic materials hazardous to biological life. May contain corrosive materials. Wear protective equipment before handling. Seek medical attention immediately if it comes into direct contact with skin, eyes or other sensitive areas.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("CoolantPack.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("CoolantPack.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 2f;

        private static ConfigEntry<float> damageReduction;

        private static ItemDef staticItemDef;

        public override void CreateItem()
        {
            base.CreateItem();
            staticItemDef = itemDef;
        }

        /// <summary>
        /// Coolant Pack implementation. Called via CharacterBody.TakeDamage as a Harmony prefix patch.
        /// </summary>
        public static bool TakeDamagePatch(RoR2.HealthComponent __instance, DamageInfo damageInfo)
        {
            if (!__instance) { return true; }

            var body = __instance.body;
            if (!body || !body.inventory) { return true; }

            var count = body.inventory.GetItemCount(staticItemDef);

            if (damageInfo.damageType == DamageType.DoT && count > 0)
            {
                var fractionalBit = 1 - (1 / (1 + count * damageReduction.Value)); // Hyperbolic Scaling, approaching 100% damage reduction.
                damageInfo.damage *= 1 - fractionalBit;

                if (damageInfo.damage < 0) { damageInfo.damage = 0; } // Catches negative damage.
            }

            return true;
        }

        public override void AddConfigOptions()
        {
            damageReduction = configFile.Bind("Coolant_Pack Config", "damageReduction", 0.15f, "Scales down the damage of incoming DOTs per stack. Does not change the hyperbolic approach value of 1.0 (100% damage reduction).");
        }
    }
}