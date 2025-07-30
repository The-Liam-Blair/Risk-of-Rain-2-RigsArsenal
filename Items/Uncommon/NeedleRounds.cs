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
    /// NeedleRounds - T2 (Uncommon) Item
    /// <para>Increases critical strike chance and damage slightly.</para>
    /// </summary>
    public class NeedleRounds : Item
    {
        public override string Name => "Needle Rounds";
        public override string NameToken => "NEEDLEROUNDS";
        public override string PickupToken => "Increases critical strike chance and critical strike damage slightly.";
        public override string Description => $"Gain <style=cIsDamage>+{critChanceGain.Value}%</style><style=cStack> (+{critChanceGain.Value}% per stack)</style> increased <style=cIsDamage>critical strike chance</style>. Also gain <style=cIsDamage>+{critDamageGain.Value * 100f}%</style><style=cStack> (+{critDamageGain.Value * 100f}% per stack)</style> increased <style=cIsDamage>critical strike damage</style>.";
        public override string Lore => "<style=cMono>// INTERCEPTED RADIO TRANSMISSIONS. //\n// PRINTING TRANSCRIPT... //</style>\n\nExperimental munitions MUST be banned today!\n\nSmall arms research has gone too far, and these so called 'Plated Rounds' harbour a sinister truth. They are designed to kill with brutal efficiency.\n\nShaped charges built into the tail end of the bullet detonate when the bullet has pierced it's target, sending forth huge bursts of shrapnel that decimate the target from the inside!\n\nWar is inevitable, but it does not always need to be a bloodbath. Incapacitation or disarming is always preferred, the less soldiers killed in battle, the better. And we already have methods to ensure that. Private military research into barbaric technologies like these prove that they are only interested in death without resolution.\n\nWe the people demand a system-wide ban on these munitions, and an immediate stop to any further research into similar technologies. For the sake of humanity. <style=cMono>\n\n// TRANSCRIPT FINISHED. //</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("NeedleRounds.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("NeedleRounds.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 2.75f;

        private ConfigEntry<int> critChanceGain;
        private ConfigEntry<float> critDamageGain;

        public override void SetupHooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if(!self || !self.inventory || self.inventory.GetItemCount(itemDef) <= 0) { return; }

                args.critAdd += critChanceGain.Value * self.inventory.GetItemCount(itemDef);
                args.critDamageMultAdd += critDamageGain.Value * self.inventory.GetItemCount(itemDef);
            };
        }

        public override void AddConfigOptions()
        {
            critChanceGain = configFile.Bind("Needle_Rounds Config", "critChanceGain", 15, "Critical hit chance per item stack.");
            critDamageGain = configFile.Bind("Needle_Rounds Config", "critDamageGain", 0.15f, "Critical hit damage per item stack.");
        }
    }
}