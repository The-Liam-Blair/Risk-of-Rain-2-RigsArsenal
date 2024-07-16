using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.MasterSpawnSlotController;

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

        public override bool AIBlackList => false;

        public override Sprite Icon => null;

        public override GameObject Model => null;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            slot.UpdateTargets(RoR2Content.Equipment.Lightning.equipmentIndex, true);
            HurtBox victim = slot.currentTarget.hurtBox;

            if(victim)
            {
                DebugLog.Log($"Nidus Virus: Targeted {victim.healthComponent.body.name}");

                // Non-damaging debuffs. First item: Type. Second item: Stack count.
                List<Tuple<BuffDef, int>> UniqueDebuffs = new List<Tuple<BuffDef, int>>();

                // Damage over time (Dot) debuffs. First item: Type. Second item: Stack count. Third item: Damage value.
               List<Tuple<DotController.DotStack, int, float>> UniqueDots = new List<Tuple<DotController.DotStack, int, float>>();


                var buffs = victim.healthComponent.body.timedBuffs;

                foreach (var buff in buffs)
                {
                    BuffDef buffDef = BuffCatalog.GetBuffDef(buff.buffIndex);
                    DebugLog.Log($"Nidus Virus: Found {buffDef.name} on {victim.healthComponent.body.name}");

                    if (buffDef.isDebuff)
                    {
                        DebugLog.Log($"Nidus Virus: Found debuff {buffDef.name} on {victim.healthComponent.body.name}");
                        UniqueDebuffs.Add(new Tuple<BuffDef, int>(buffDef, victim.healthComponent.body.GetBuffCount(buffDef)));
                    }
                }

                DebugLog.Log("Nidus Virus: Checking for dots.");
                var dots = DotController.FindDotController(victim.healthComponent.body.gameObject);

                if (dots != null)
                {
                    foreach (var dot in dots.dotStackList)
                    {
                        DebugLog.Log($"Nidus Virus: Found {dot.dotDef} on {victim.healthComponent.body.name}");
                        BuffDef buffDef = dot.dotDef.associatedBuff;

                        DebugLog.Log($"Nidus Virus: Found debuff {buffDef.name} on {victim.healthComponent.body.name}");
                        UniqueDots.Add(new Tuple<DotController.DotStack, int, float>(dot, victim.healthComponent.body.GetBuffCount(buffDef), dot.damage));

                    }
                }

                var victimTeam = victim.healthComponent.body.teamComponent.teamIndex;
                DebugLog.Log($"Nidus Virus: Victim team is {victimTeam}");

                foreach(var entity in TeamComponent.GetTeamMembers(victimTeam))
                {
                    DebugLog.Log($"Nidus Virus: Found {entity.name} on the same team as {victim.healthComponent.body.name}");
                    if(entity.teamIndex == victimTeam)
                    {
                        if(Vector3.Distance(victim.healthComponent.body.corePosition, entity.body.corePosition) <= 200)
                        {
                            if(entity.body == victim.healthComponent.body)
                            {
                                continue;
                            }
                            DebugLog.Log($"Nidus Virus: {entity.name} is within range of {victim.healthComponent.body.name}");
                            foreach(var debuff in UniqueDebuffs)
                            {
                                entity.body.AddTimedBuff(debuff.Item1, 5f, debuff.Item2);
                                DebugLog.Log($"Nidus Virus: Spread {debuff.Item1.name} to {entity.name}");
                                DebugLog.Log($"Buff stats: {debuff.Item2} stacks.");
                            }

                            foreach(var dot in UniqueDots)
                            {
                                DebugLog.Log($"Nidus Virus: Inflicting {dot.Item1.dotDef.associatedBuff.name} on {entity.name}");
                                DebugLog.Log($"DOT type is {dot.Item1.dotIndex} or {dot.Item1}");
                                DebugLog.Log($"DOT damage is {dot.Item1.damage}");
                                DebugLog.Log($"DOT stack count is {dot.Item2}");

                                /*
                                InflictDotInfo clonedDot = new InflictDotInfo()
                                {
                                    attackerObject = slot.characterBody.gameObject,
                                    victimObject = entity.body.gameObject,
                                    totalDamage = slot.characterBody.damage * 2f,
                                    dotIndex = dot.Item1.dotIndex
                                };
                                DotController.InflictDot(ref clonedDot);
                                */

                                MoreItems.InflictDot(slot.characterBody, entity.body, dot.Item1.dotIndex, slot.characterBody.damage);
                            }
                        }
                    }
                } 

                slot.InvalidateCurrentTarget();
                return true;
            }

            return false;
        }

        public override void SetupHooks()
        {
            // Hook onto the update event.
            On.RoR2.RoR2Application.Update += (orig, self) =>
            {
                orig(self);

                // todo: add visual target indicator.
            };  
        }
    }


}
