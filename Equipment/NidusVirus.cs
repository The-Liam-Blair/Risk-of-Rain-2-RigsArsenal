﻿using BepInEx.Configuration;
using EntityStates.VoidRaidCrab;
using R2API;
using Rewired.ComponentControls.Data;
using Rewired.Utils;
using RigsArsenal.DOTs;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RigsArsenal.RigsArsenal;
using static RoR2.MasterSpawnSlotController;

namespace RigsArsenal.Equipments
{
    public class NidusVirus : Equipment
    {
        public override string Name => "Nidus Virus";

        public override string NameToken => "NIDUSVIRUS";

        public override string PickupToken => "Target an enemy to spread their debuffs to nearby enemies.";

        public override string Description => $"Target an enemy and spread their <style=cIsUtility>debuffs</style> to enemies up to <style=cIsUtility>{spreadRadius.Value} metres</style> away for <style=cIsUtility>{debuffDuration.Value} seconds</style>. <style=cIsHealth>Damaging debuffs</style> are duplicated to enemies for their normal duration.";
        public override string Lore => "''Toxicology team, report!''\n\n''Virus 'ND-1421' outbreak has been successfully contained.''\n\n''Casualties?''\n\n''Over the two month outbreak, approximately 500 million humans, about 95% of the planet's population, has died. Animal reports are still underway, but we estimate up to 80 thousand species are extinct or critically endangered.''\n\n''My god...''\n\n''Most of the local fauna were destroyed. Entire groves of flowers and forests, eradicated. This cannot happen again, ever. If a hostile agent managed to retrieve this virus-''\n\n''It will be sealed in the strongest container money can buy. This virus is one of the deadlest in the known universe and must be studied, to learn how it spreads, and for a cure to make sure an outbreak like this never happens again. Have it sent to the lab on Earth, as soon as possible''\n\n''Yes sir, on it.''";

        public override bool isLunar => false;

        public override float cooldown => equipCooldown.Value;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("NidusVirus.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("NidusVirus.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 1.8f;

        private ConfigEntry<int> equipCooldown;
        private ConfigEntry<int> spreadRadius;
        private ConfigEntry<int> debuffDuration;

        private GameObject targetIcon;
        private GameObject SpreadIndicator;

        // todo: Investigate NRE bug possibly from the spawned radial visual effect.

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

            var razorLeechBleedDOT = DOTList.Find(x => x.Name.Equals("RazorLeechBleed")); // Reference to the custom dot from Razor Leeches.

            // Get all entities in the victim's team.
            foreach (var entity in TeamComponent.GetTeamMembers(victimTeam))
            {
                if(entity.teamIndex == victimTeam)
                {
                    // If the entity is within the spread radius (50m by default), apply the recorded debuffs and dots to that entity.
                    if(Vector3.Distance(victim.healthComponent.body.corePosition, entity.body.corePosition) <= spreadRadius.Value)
                    {
                        // Skip if the entity was the victim itself to prevent them from receiving their own debuffs.
                        if(entity.body == victim.healthComponent.body) { continue; }

                        // Spread non-damaging debuffs.
                        foreach(var debuff in UniqueDebuffs)
                        {
                            entity.body.AddTimedBuff(debuff.Item1, debuffDuration.Value, debuff.Item2);
                        }

                        // Spread dots
                        foreach(var dot in UniqueDots)
                        {
                            if (dot.Item1.dotDef == razorLeechBleedDOT.dotDef)
                            {
                                RigsArsenal.InflictCustomDot(slot.characterBody, entity.body, razorLeechBleedDOT, slot.characterBody.damage);
                            }
                            else
                            {
                                // If the dot is not in the list of DOTs that can be spread, apply it as a normal buff.
                                // This will not apply the stack count, but will apply the dot with its original duration.
                                RigsArsenal.InflictDot(slot.characterBody, entity.body, dot.Item1.dotIndex, slot.characterBody.damage);
                            }
                        }
                    }
                }
            }

            // To prevent a NRE from spamming the item on a single target, resulting in multiple indicators with the old ones being de-referenced, lost
            // and not destroyed after expiring.
            if(SpreadIndicator)
            {
                UnityEngine.Object.Destroy(SpreadIndicator);
                SpreadIndicator = null;
            }

            // Uses a modified range indicator from the "NearbyDamageBonus" (focus crystal) item.
            GameObject original = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
            SpreadIndicator = original.InstantiateClone("NidusVirusSpreadIndicator", true);
            PrefabAPI.RegisterNetworkPrefab(SpreadIndicator);


            SpreadIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(victim.healthComponent.body.gameObject, null);

            var donut = SpreadIndicator.transform.GetChild(1); // 2nd child of the range indicator object controls the donut's visual properties.
            donut.localScale = new Vector3(5f, 5f, 5f);
            donut.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.45f, 0.56f, 0f)); // Blue tint instead of red.
            donut.GetComponent<MeshRenderer>().material.SetFloat("_SoftFactor", 0.1f); // Soften the edges of the donut.

            victim.StartCoroutine(DestroyIndicator(1f, SpreadIndicator));
            
            slot.InvalidateCurrentTarget();

            return true;
        }

        public override void SetupHooks()
        {
            On.RoR2.EquipmentSlot.UpdateTargets += (orig, self, equipmentIndex, isEquipmentActivation) =>
            {
                // Skip if not the equipment def for nidus virus. Also includes complementary NRE catcher :)
                if (!EquipmentSlot || equipmentIndex != equipmentDef.equipmentIndex) 
                {
                    orig(self, equipmentIndex, isEquipmentActivation);
                    return;
                }

                // Setup targetting to search for enemies, load the target icon (if null) and set the target to an aimed entity, if there is one.
                self.ConfigureTargetFinderForEnemies();

                if (!targetIcon)
                {
                    targetIcon = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/LightningIndicator"), "NidusIndicator", false);
                }

                //self.targetFinder.candidatesEnumerable = (List<BullseyeSearch.CandidateInfo>)(from candidate in self.targetFinder.candidatesEnumerable select candidate);
                //self.targetFinder.candidatesEnumerable = (from candidate in self.targetFinder.candidatesEnumerable select candidate).ToList();
                HurtBox entity = self.targetFinder.GetResults().FirstOrDefault();

                if (entity)
                {
                    self.currentTarget = new EquipmentSlot.UserTargetInfo(entity);
                }


                // If there is a target, setup the aim indicator, and record the target's transform and hurtbox.
                bool hasTransform = self.currentTarget.transformToIndicateAt;

                if (hasTransform)
                {
                    EquipmentSlot.UserTargetInfo currentTarget = self.currentTarget;

                    if(currentTarget.hurtBox && currentTarget.hurtBox.healthComponent)
                    {
                        self.targetIndicator.visualizerPrefab = targetIcon;
                    }

                }
                self.targetIndicator.targetTransform = hasTransform ? self.currentTarget.transformToIndicateAt : null;

                // Cancel the targetting if the equipment is on cooldown.
                // todo: ?? stack trace says an error is encountered on this line based on a null ref exception when setting the renderer to active.
                // should maybe fix this but I'm not sure how the error is occuring.
                if (self.targetIndicator.visualizerPrefab)
                {
                    self.targetIndicator.active = hasTransform && self.stock > 0;
                }


                return;
            };
        }

        public override void AddConfigOptions()
        {
            equipCooldown = configFile.Bind("Nidus_Virus Config", "equipCooldown", 35, "Cooldown for this equipment.");
            spreadRadius = configFile.Bind("Nidus_Virus Config", "spreadRadius", 50, "The spread radius in metres.");
            debuffDuration = configFile.Bind("Nidus_Virus Config", "debuffDuration", 5, "Duration of all non-DOTs spread to enemies.");
        }


        public IEnumerator DestroyIndicator(float duration, GameObject spreadIndicator)
        {
            // Expands the donut for the duration of the spread effect dyamically over time, up to the maximum radius of 50 units. (By default).
            // Only acts as a visual indicator for the range, the spread effect is instant across the maximum radius.

            var donut = spreadIndicator.transform.GetChild(1);

            Vector3 originalScale = donut.localScale;
            float counter = 0f;

            // Stop if the indicator was destroyed.
            while (counter <= duration)
            {
                counter += Time.deltaTime;

                if(!spreadIndicator)
                {
                    yield break;
                }
                donut.localScale = Vector3.Lerp(originalScale, new Vector3(spreadRadius.Value * 2, spreadRadius.Value * 2, spreadRadius.Value * 2), counter / duration);
                yield return null;
            }

            if(counter >= duration && spreadIndicator)
            {
                UnityEngine.Object.Destroy(spreadIndicator);
                spreadIndicator = null;
                yield break;
            }
        }
    }


}
