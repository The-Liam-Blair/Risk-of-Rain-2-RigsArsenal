using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreItems.Buffs;
using UnityEngine;
using static MoreItems.MoreItems;

namespace MoreItems.Items
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
        public override string PickupToken => "Gain barrier when you use your utility skill.";
        public override string Description => "After using your <style=cIsUtility>utility skill</style>, gain <style=cIsHealing>10</style> plus an additional <style=cIsHealing>5%</style> <style=cStack>(+5% per stack)</style> <style=cIsHealing>barrier</style>. Goes on a <style=cIsUtility>5 second cooldown</style> after use.";
        public override string Lore => "This little battery can hook directly into your shield system to provide a quick, temporary burst of shielding!\n\nIt's real strength however is that it can recharge itself through quick motion! A burst of speed is enough for the battery to power your shields for a brief moment, perfect for when you need to quickly remove yourself from any situation safely.\n\nJust don't touch the glowing parts, or even the bars for that matter.";

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "KineticBatteryCooldown").buffDef;
        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("KineticBattery.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("KineticBattery.prefab");

        public override void SetupHooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += (orig, self, skill) =>
            {
                orig(self, skill);

                if (!self.isPlayerControlled) { return; } // Prevents NRE from enemies calling this method (On self.skillLocator.utility.hasExecutedSuccessfully).

                DebugLog.Log($"Buff name: {ItemBuffDef.name}, {ItemBuffDef.buffColor}, {ItemBuffDef}");
                DebugLog.Log($"Buff count: {self.GetBuffCount(ItemBuffDef)}");
                if (self.GetBuffCount(ItemBuffDef) <= 0 && self.skillLocator.utility.hasExecutedSuccessfully)
                {
                    var count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        var finalBarrierValue = 10 + (self.maxHealth * 0.05f * count); // 10 + 5% max health per stack.
                        self.healthComponent.AddBarrier(finalBarrierValue);
                        self.AddTimedBuff(ItemBuffDef, 5f);
                    }
                }
            };
        }
    }
}
