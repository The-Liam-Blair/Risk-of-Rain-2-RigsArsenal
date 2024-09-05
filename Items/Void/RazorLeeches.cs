using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using R2API;
using R2API.Utils;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static MoreItems.MoreItems;

namespace MoreItems.Items.VoidItems
{
    /// <summary>
    /// Razor Leeches - Void T2 (Void Uncommon) Item
    /// <para>Critically strike enemies to perforate them.</para>
    /// <para>Perforated enemies have a chance of receiving damage over time from incoming attacks.</para>
    /// <para>Certain types of damage, including damage over time damage and environmental damage, are exempt.</para>
    /// <para>This item corrupts all Needle Rounds items.</para>
    /// </summary>
    public class RazorLeeches : Item
    {
        public override string Name => "Razor Leeches";
        public override string NameToken => "RAZORLEECHES";
        public override string PickupToken => "Perforate foes by critically striking. Perforated enemies have a chance to receive additional damage over time from attacks.<style=cIsVoid> Corrupts all Needle Rounds.";
        public override string Description => "Critically striking an enemy<style=cIsDamage> perforates</style> them for <style=cIsUtility>2</style> <style=cStack>(+1 per stack)</style> seconds. <style=cIsDamage>Perforated</style> enemies have a <style=cIsUtility>25%</style> chance to receive <style=cIsUtility>50%</style> of <style=cIsDamage>ANY</style> incoming damage as <style=cIsDamage>additional damage over time</style>. <style=cIsVoid> Corrupts all Needle Rounds</style>.";
        public override string Lore => "";

        // TODO:
        // - Debuff Icon.
        // - (Optional?) Damage oer time bleed/splatter effect per damage application.
        // - Model (And Sprite).
        // - Sound effect?

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => null;
        public override GameObject Model => null;

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "RazorLeechWound").buffDef;

        public override ItemDef pureItemDef => ItemList.Find(x => x.NameToken == "NEEDLEROUNDS").itemDef; // Needle Rounds

        public override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);

                if (!self) { return; }

                var attackerObj = info.attacker;
                if (!attackerObj || !attackerObj.GetComponent<CharacterBody>()) { return; }

                var attacker = attackerObj.GetComponent<CharacterBody>();
                if (!attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);

                // Apply perforate (wounded) debuff to the victim if the attack was a crit and the attacker has the item.
                if (count > 0 && info.crit)
                {
                    var victim = self.body;
                    if (!victim) { return; }

                    victim.AddTimedBuff(ItemBuffDef, 2f + count, 1);
                }
            };

            // Item gives +5% flat crit chance, much like harvester scythe and predatory instincts.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (!self || !self.inventory || !self.isPlayerControlled) { return; }

                if(self.inventory.GetItemCount(itemDef) > 0)
                {
                    args.critAdd += 5f;
                }
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