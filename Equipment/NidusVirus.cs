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

namespace MoreItems.Equipments
{
    public class NidusVirus : Equipment
    {
        public override string Name => "Nidus Virus";

        public override string NameToken => "NIDUSVIRUS";

        public override string PickupToken => "Target enemy's debuffs are spread to nearby enemies.";

        public override string Description => "Target an enemy and spread their <style=cIsUtility>debuffs</style> to enemies up to <style=cIsUtility>50 units</style> away for <style=cIsUtility>5 seconds</style>. <style=cIsHealth>Damaging debuffs</style> inflict <style=cIsHealth>double damage</style>.";
        public override string Lore => "";

        public override bool isLunar => false;

        public override float cooldown => 1f;

        public override Sprite Icon => null;

        public override GameObject Model => MainAssets.LoadAsset<GameObject>("NidusVirus.prefab");

        private int spreadRadius = 200;
        private float debuffDefaultDuration = 5f;
        private Indicator targetIndicator = null;

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

            if (victim)
            {
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
                        DebugLog.Log($"damage of {dot} is {dot.damage} and buffDef is {buffDef}");

                    }
                }

                // Stop execution if no debuffs or dots were found.
                if(!UniqueDebuffs.Any() && !UniqueDots.Any())
                {
                    DebugLog.Log($"Nidus Virus: {victim.healthComponent.body.name} has no debuffs to spread.");
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

            return false;
        }

        public override void SetupHooks()
        {
            // Hook onto the update event.
            On.RoR2.RoR2Application.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if (Run.instance && EquipmentSlot && equipmentDef.equipmentIndex == EquipmentSlot.equipmentIndex)
                {
                    EquipmentSlot.UpdateTargets(RoR2Content.Equipment.Lightning.equipmentIndex, true);
                    EquipmentSlot.targetIndicator = new Indicator(EquipmentSlot.gameObject, null);

                    // todo: figure out how to make the indicator disappear when the focus on the current target is lost.


                }
            };
        }
    }


}
