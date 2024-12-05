using System.Runtime.CompilerServices;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;
using static RoR2.MasterSpawnSlotController;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Chaos Rune - T3 (Legendary) Item
    /// <para>When applying a damaging debuff to an entity, chance to apply a random damaging debuff as well.</para>
    /// <para>Stacking increases the number of rolls, increasing overall chance and number of debuffs that can be applied at once.</para>
    /// <para>The damage of the debuff scales off of the attack that caused it.</para>
    /// </summary>
    public class ChaosRune : Item
    {
        public override string Name => "Chaos Rune";
        public override string NameToken => "CHAOSRUNE";
        public override string PickupToken => "Chance to inflict additional damaging debuffs when applying any damaging debuff.";
        public override string Description => "When applying a damaging debuff to an enemy, there is a <style=cIsDamage>33% chance</style><style=cStack> (+1 roll per stack)</style> to apply <style=cIsHealth>additional damaging debuffs</style> up to 1 <style=cStack>(+1 per 2 stacks)</style> more.";
        public override string Lore => "<style=cMono>// ARTIFACT RECOVERY NOTES: EXCAVATION SITE 165-A34 //</style>\n\nName: Runic Stone Carving\n\nSize: 20cm by 20cm by 3cm\n\nSite Notes: ''Weighty and shimmers a bright red hue. The miner that recovered this artifact was found an hour after contact in tremendous pain, dehydrated and collapsed, still holding onto the artifact. Artifact was additionally glowing incredibly brightly, and is allegedly scalding to the touch for some while bone-chillingly cold to others.\n\nDo NOT handle directly. Do NOT stare into it's glow. Do NOT listen to what it offers. Be not tempted.''\n\n<style=cMono>// END OF NOTES //";

        public override ItemTier Tier => ItemTier.Tier3;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => true; // Even though the AI could get this item, its only going to be useful if the enemy can inflict damaging DOTs
                                                   // naturally or is able to with another item, so its too niche.
        
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("ChaosRune.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("ChaosRune.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 1.8f;

        private bool hasRun = false;
        private DamageInfo damageInfo { get; set; }

        public override void SetupHooks()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += (On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) =>
            {
                orig(ref inflictDotInfo);

                if(hasRun) { return; }

                if(!inflictDotInfo.attackerObject || !inflictDotInfo.victimObject) { return; }

                var attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                var victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();

                if(!attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if(count <= 0) { return; }

                var maxSuccessfulRolls = 1 + Mathf.Floor(count * 0.5f); // Up to 1 successful roll, and another per 2 stacks.
                var currentSucessfulRolls = 0;
                
                var roll = 33; // 1/3 chance of a successful roll per stack.
                
                for(int i = 0; i < count; i++)
                {
                    if(currentSucessfulRolls >= maxSuccessfulRolls) { break; }

                    if (Util.CheckRoll(roll, attacker.master))
                    {
                        hasRun = true;
                        currentSucessfulRolls++;

                        // todo: some custom visual or audio effect maybe to indicate the item has triggered.

                        int DotIndex = UnityEngine.Random.Range(0, 4); // 4 DOTs: Bleed, Burn (Including ignition tank upgraded burn), Blight and Collapse.

                        switch (DotIndex)
                        {
                            case 0: // Bleed
                                InflictDot(attacker, victim, DotController.DotIndex.Bleed, attacker.damage, damageInfo.procCoefficient);
                                break;

                            case 1: // Burn
                                InflictDot(attacker, victim, DotController.DotIndex.Burn, attacker.damage, damageInfo.procCoefficient);
                                break;

                            case 2: // Blight
                                InflictDot(attacker, victim, DotController.DotIndex.Blight, attacker.damage, damageInfo.procCoefficient);
                                break;

                            case 3: // Collapse
                                InflictDot(attacker, victim, DotController.DotIndex.Fracture, attacker.damage, damageInfo.procCoefficient);
                                break;
                        }
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, DamageInfo, victim) =>
            {
                damageInfo = DamageInfo; // Store damage info for possible later use to get the attack's proc coefficient.
                hasRun = false; // Reset flag for triggering this item.
                orig(self, damageInfo, victim);
            };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("ChaosRune.prefab");

            var itemDisplay = display.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(display);

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0.06313F, 0.22037F, 0.09908F),
                    localAngles = new Vector3(45.34884F, 106.977F, 165.8645F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Base",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighR",
                    localPos = new Vector3(-0.12428F, 0.20452F, 0.04312F),
                    localAngles = new Vector3(22.53534F, 0.66831F, 188.5954F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighR",
                    localPos = new Vector3(-0.90804F, -0.68912F, -0.52958F),
                    localAngles = new Vector3(35.03824F, 0.44614F, 214.5975F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Stomach",
                    localPos = new Vector3(-0.35971F, 0.04198F, -0.042F),
                    localAngles = new Vector3(328.5733F, 176.5325F, 13.82045F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            return rules;
        }
    }
}