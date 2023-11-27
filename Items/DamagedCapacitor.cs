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
    public class DamagedCapacitor : Item
    {
        public override string Name => "Damaged Capacitor";
        public override string NameToken => "DAMAGEDCAPACITOR";
        public override string PickupToken => "Gain barrier when you use your utility skill.";
        public override string Description => "After using your <style=cIsUtility>utility skill</style>, gain <style=cIsHealing>10</style> plus an additional <style=cIsHealing>5%</style> <style=cStack>(+5% per stack)</style> <style=cIsHealing>barrier</style>. Goes on a <style=cIsUtility>5 second cooldown</style> after use.";
        public override string Lore => "This little capacitor is capable of powering your shields from dawn 'til dusk! Unfortunately, it looks like it took a bit of damage.\n\nSave it for when your in a pinch and need to hightail it out of there.\n\nJust don't touch the glowing part, or even the bars for that matter.";

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "DamagedCapacitorCooldown").buffDef;
        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("DamagedCapacitor.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("DamagedCapacitor.prefab");

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
