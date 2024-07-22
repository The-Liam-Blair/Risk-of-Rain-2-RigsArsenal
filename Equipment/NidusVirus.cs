using Rewired.ComponentControls.Data;
using Rewired.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.MasterSpawnSlotController;
using static MoreItems.MoreItems;
using R2API;

namespace MoreItems.Equipments
{
    public class NidusVirus : Equipment
    {
        public override string Name => "Nidus Virus";

        public override string NameToken => "NIDUSVIRUS";

        public override string PickupToken => "Target enemy's debuffs are spread to nearby enemies.";

        public override string Description => "Target an enemy and spread their <style=cIsUtility>debuffs</style> to enemies up to <style=cIsUtility>50 units</style> away for <style=cIsUtility>5 seconds</style>. <style=cIsHealth>Damaging debuffs</style> are duplicated to enemies for their normal duration.";
        public override string Lore => "''Toxicology team, report!''\n\n''Virus 'ND-1421' outbreak has been successfully contained.''\n\n''Casualties?''\n\n''Over the two month outbreak, approximately 500 million humans, about 95% of the planet's population, has died. Animal reports are still underway, but we estimate up to 80 thousand species are extinct or critically endangered.''\n\n''My god...''\n\n''Most of the local fauna were destroyed. Entire groves of flowers and forests, eradicated. This cannot happen again, ever. If a hostile agent managed to retrieve this virus-''\n\n''It will be sealed in the strongest container money can buy. This virus is one of the deadlest in the known universe and must be studied, to learn how it spreads, and for a cure to make sure an outbreak like this never happens again. Have it sent to the lab on Earth, as soon as possible''\n\n''Yes sir, on it.''";

        public override bool isLunar => false;

        public override float cooldown => 35f;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("NidusVirus.png");

        public override GameObject Model => MainAssets.LoadAsset<GameObject>("NidusVirus.prefab");

        private int spreadRadius = 50;
        private float debuffDefaultDuration = 5f;
        private Indicator targetIndicator = null;

        private GameObject targetIcon;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            // Special debuffs this CAN spread:
            // - Ruin stacks from essence of heresy. (Repeated use does not increment the stack count).
            // - Lunar root from hooks of heresy's explosion.
            // - Effigy of grief's cripple.
            //
            // - Death mark.
            // - Tar.
            // - Pulverise buildup stacks AND pulverised from shattering justice. (Spread will overwrite the current stack count, not add to it).
            //
            // - Rex's weak debuff.
            // - Rex's fruiting debuff and on-death followup.
            // - Acrid's blight and poison.
            // - Bandit hemorrhage.


            // Special debuffs this CAN NOT spread:
            // - Hellfire tincture.            
            // - Symbiotic Scorpion's armour shred.
            // - Void fog.
            // - Rex's entangle.
            // - Mercenary's expose.


            // todo: May need more testing, sometimes it feels like the effect fails, but it may be due to it acquiring the wrong target.
            //       Aka MAKE THAT VISUAL INDICATOR!!

            HurtBox victim = slot.currentTarget.hurtBox;

            if (!victim || !victim.healthComponent) { return false; }

            // Non-damaging debuffs. First item: Type. Second item: Stack count.
            List<Tuple<BuffDef, int>> UniqueDebuffs = new List<Tuple<BuffDef, int>>();

            // Damage over time (Dot) debuffs. First item: Type. Second item: Stack count.
            List<Tuple<DotController.DotStack, int>> UniqueDots = new List<Tuple<DotController.DotStack, int>>();

            // Find all buffs on the victim.
            var buffs = victim.healthComponent.body.timedBuffs;

            if (buffs != null)
            {
                // Loop through each buff, and record all debuffs.
                foreach (var buff in buffs)
                {
                    BuffDef buffDef = BuffCatalog.GetBuffDef(buff.buffIndex);

                    if (buffDef.isDebuff)
                    {
                        UniqueDebuffs.Add(new Tuple<BuffDef, int>(buffDef, victim.healthComponent.body.GetBuffCount(buffDef)));
                    }
                }
            }

            // Find all dots on the victim.
            var dots = DotController.FindDotController(victim.healthComponent.body.gameObject);

            if (dots != null)
            {
                // Loop through each dot, and record all dots.
                foreach (var dot in dots.dotStackList)
                {
                    BuffDef buffDef = dot.dotDef.associatedBuff;

                    UniqueDots.Add(new Tuple<DotController.DotStack, int>(dot, victim.healthComponent.body.GetBuffCount(buffDef)));
                }
            }

            // Stop execution if no debuffs or dots were found.
            if(!UniqueDebuffs.Any() && !UniqueDots.Any())
            {
                return false;
            }

            // Find the team the victim belongs to.
            var victimTeam = victim.healthComponent.body.teamComponent.teamIndex;
                
            // Get all entities in the victim's team.
            foreach(var entity in TeamComponent.GetTeamMembers(victimTeam))
            {
                if(entity.teamIndex == victimTeam)
                {
                    // If the entity is within the spread radius, apply the recorded debuffs and dots to that entity.
                    if(Vector3.Distance(victim.healthComponent.body.corePosition, entity.body.corePosition) <= spreadRadius)
                    {
                        // Skip if the entity was the victim itself to prevent them from receiving their own debuffs.
                        if(entity.body == victim.healthComponent.body) { continue; }

                        // Spread non-damaging debuffs.
                        foreach(var debuff in UniqueDebuffs)
                        {
                            entity.body.AddTimedBuff(debuff.Item1, debuffDefaultDuration, debuff.Item2);
                        }

                        // Spread dots
                        foreach(var dot in UniqueDots)
                        {
                            MoreItems.InflictDot(slot.characterBody, entity.body, dot.Item1.dotIndex, slot.characterBody.damage);
                        }
                    }
                }
            } 

            // todo: add visual spread effect (some way of showing the equipment activating successfully, and its radius of effect).

            slot.InvalidateCurrentTarget();

            return true;
        }

        public override void SetupHooks()
        {
            On.RoR2.EquipmentSlot.UpdateTargets += (orig, self, equipmentIndex, isEquipmentActivation) =>
            {
                if (!EquipmentSlot || equipmentIndex != EquipmentSlot.equipmentIndex) 
                {
                    orig(self, equipmentIndex, isEquipmentActivation);
                    return;
                }

                self.ConfigureTargetFinderForEnemies();

                if (!targetIcon)
                {
                    targetIcon = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator"), "NidusIndicator", false);
                }

                self.targetFinder.candidatesEnumerable = from candidate in self.targetFinder.candidatesEnumerable select candidate;
                HurtBox entity = self.targetFinder.GetResults().FirstOrDefault();
                self.currentTarget = new EquipmentSlot.UserTargetInfo(entity);


                bool hasTransform = self.currentTarget.transformToIndicateAt;

                if (self.currentTarget.transformToIndicateAt)
                {
                    EquipmentSlot.UserTargetInfo currentTarget = self.currentTarget;

                    if(currentTarget.hurtBox && currentTarget.hurtBox.healthComponent)
                    {
                        self.targetIndicator.visualizerPrefab = targetIcon;
                    }

                }

                self.targetIndicator.targetTransform = hasTransform ? self.currentTarget.transformToIndicateAt : null;
                self.targetIndicator.active = hasTransform && self.stock > 0;

                return;

                // todo: figure out how to make the indicator disappear when the focus on the current target is lost.
            };
        }
    }


}
