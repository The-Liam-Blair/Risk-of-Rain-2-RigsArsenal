using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL.EntityStates;
using MoreItems.Buffs;
using UnityEngine;
using static MoreItems.MoreItems;
using R2API;

namespace MoreItems.Items
{
    /// <summary>
    /// Reactive Armour Plating - T2 (Uncommon) Item.
    /// <para>Gain a temporary buff that increases armour when taking damage.</para>
    /// <para>Buff duration is low, does not prevent the damage prior to activation, but can be refreshed repeatedly if hit repeatedly.</para>
    /// </summary>
    public class ReactiveArmourPlating : Item
    {
        public override string Name => "Reactive Armour Plating";
        public override string NameToken => "REACTIVEARMOURPLATING";
        public override string PickupToken => "Briefly gain armour when hit.";
        public override string Description => "Gain <style=cIsDamage>20 permanent armour.</style> Briefly gain <style=cIsDamage>20</style> <style=cStack>(+20 per stack)</style> <style=cIsDamage>armour</style> after receiving damage.";
        public override string Lore => "<style=cMono>// ARTIFACT SCAVENGER TEAM - UPRISING AT HAEDRON MINING TOWN AFTERMATH - CONVERSATION EXCERPT //</style>\n\n''...Wielded by the leader of the rebels, this plating, ripped straight from the hull of a light interceptor ship with a handle crudely welded to the back, weighs approximately 650 kilos, and is designed to withstand low grade ship weapon fire. Nanobots tucked inside the plating constantly repair the plate and reinforce it to adapt to the current weapon fire it's receiving for maximum durability.''\n\n''Wow! How was he even able to lift that, let alone use it as a shield?''\n\n''He figured that if it could withstand ship weaponry, it'd be able to deflect any weaponry carried by the town guards. With that thing, he was almost invincible.''\n\n''In that case, how did they manage to put him down?''\n\n''They shot him from behind. Never saw it coming.''\n\n<style=cMono>// END OF EXCERPT //";

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "ReactiveArmourPlatingBuff").buffDef;
        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("ReactiveArmourPlating.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("ReactiveArmourPlating.prefab");

        public override void SetupHooks()
        {
            // On damage taken, give the entity the relevant reactive armour plating buff that increases armour.
            On.RoR2.HealthComponent.TakeDamage +=
                (orig, self, damageInfo) =>
                {
                    orig(self, damageInfo);

                    var body = self.body;
                    if (body.inventory == null) { return; }

                    if(damageInfo.damageType is DamageType.OutOfBounds or DamageType.FallDamage) { return; }

                    var count = body.inventory.GetItemCount(itemDef);
                    if (count <= 0) { return; }

                    if (body.HasBuff(ItemBuffDef))
                    {
                        body.ClearTimedBuffs(ItemBuffDef);
                        body.AddTimedBuff(ItemBuffDef, 3f);
                    }
                    else
                    {
                        body.AddTimedBuff(ItemBuffDef, 3f);
                    }
                };

            // If the entity has at least 1 stack of this item, they passively gain 20 armour.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (self && self.inventory && self.inventory.GetItemCount(itemDef) > 0)
                {
                    args.armorAdd += 20;
                }
            };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("ReactiveArmourPlating.prefab");

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
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
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

            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
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
