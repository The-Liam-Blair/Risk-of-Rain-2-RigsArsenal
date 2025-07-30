using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL.EntityStates;
using RigsArsenal.Buffs;
using UnityEngine;
using static RigsArsenal.RigsArsenal;
using R2API;
using BepInEx.Configuration;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Kinetic Battery - T1 (Common) Item.
    /// <para>Using your utility skill gives a small amount of barrier. Goes on cooldown for a few seconds after use.</para>
    /// <para>Enemies are unable to get this item. Barrier scales with %hp and item count.</para>
    /// </summary>
    public class KineticBattery : Item
    {
        public override string Name => "Kinetic Battery";
        public override string NameToken => "KINETICBATTERY";
        public override string PickupToken => "Gain barrier after using your utility skill.";
        public override string Description => $"After using a <style=cIsUtility>utility skill</style>, gain <style=cIsHealing>{barrierAmount.Value}</style> <style=cStack>(+{barrierAmount.Value} per stack)</style> <style=cIsHealing>barrier</style>. Goes on a <style=cIsUtility>{cooldown.Value:0.0} second cooldown</style> after use.";
        public override string Lore => "This little battery can hook directly into your shield system to provide a quick, temporary burst of shielding!\n\nIt's real strength however is that it can be recharged with kinetic energy! A burst of speed is enough for the battery to power your shields for a brief moment, perfect for when you need to quickly remove yourself from any situation safely.\n\nJust don't touch the glowing parts, or even the glass for that matter.";

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "KineticBatteryCooldown").buffDef;
        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("KineticBattery.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("KineticBattery.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 3f;

        private ConfigEntry<int> barrierAmount;
        private ConfigEntry<float> cooldown;

        public override void SetupHooks()
        {
            // When the player uses their utility skill, give a barrier and apply the cooldown buff.
            On.RoR2.CharacterBody.OnSkillActivated += (orig, self, skill) =>
            {
                orig(self, skill);

                if (!self || !self.isPlayerControlled) { return; } // Prevents NRE from enemies calling this method (On self.skillLocator.utility.hasExecutedSuccessfully).

                if (self.GetBuffCount(ItemBuffDef) <= 0
                && self.skillLocator.utility.hasExecutedSuccessfully
                && self.skillLocator.FindSkillSlot(skill) == SkillSlot.Utility)
                {
                    var count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var finalBarrierValue = barrierAmount.Value * count; // 35 (Base) barrier per stack.
                        self.healthComponent.AddBarrier(finalBarrierValue);
                        self.AddTimedBuff(ItemBuffDef, cooldown.Value); // 3 second cooldown (Base).
                    }
                }
                
            };
        }

        public override void AddConfigOptions()
        {
            barrierAmount = configFile.Bind("Kinetic_Battery Config", "barrierAmount", 35, "The barrier given by this item.");
            cooldown = configFile.Bind("Kinetic_Battery Config", "cooldown", 3.0f, "The cooldown duration of the item.");
        }
    }
}
