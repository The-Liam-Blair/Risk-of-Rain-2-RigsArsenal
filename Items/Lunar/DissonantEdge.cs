using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;
using HarmonyLib;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Dissonant Edge - Lunar Item
    /// <para>All attacks deal increased damage to entities with a lower health percentage than the user.</para>
    /// <para>All attacks deal reduced damage to entities with a higher health percentage than the user.</para>
    /// </summary>
    public class DissonantEdge : Item
    {

        public override string Name => "Dissonant Edge";
        public override string NameToken => "DISSONANTEDGE";
        public override string PickupToken => "Increased damage to foes with a lower health percentage. <style=cIsHealth> Reduced damage to foes with a higher health percentage.</style>";
        public override string Description => $"<style=cIsDamage>All attacks</style> deal +{damageIncrease.Value*100f}% <style=cStack>(+{damageIncrease.Value*100f}% per stack)</style> more damage if the target's <style=cIsUtility>current health percentage is lower than yours</style>. <style=cIsHealth>All attacks deal {damageDecrease.Value*100f}% <style=cDeath>REDUCED</style> damage if the target's current health percentage is higher than yours.</style>";
        public override string Lore => "<style=cLunarObjective>These creatures are imperfect. A flawed design. Unstable, temperamental, yet fragile.\n\nMy constructs are streamlined. Deadly, efficient, yet simple.\n\nWhy can't He see that? Is he so blinded by love? He is a fool. But one day He will see, and He will understand.</style>";

        public override ItemTier Tier => ItemTier.Lunar;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("DissonantEdge.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("DissonantEdge.prefab");

        public override float minViewport => 2f;
        public override float maxViewport => 3f;

        private static ConfigEntry<float> damageIncrease;
        private static ConfigEntry<float> damageDecrease;

        private static ItemDef staticItemDef;

        public override void CreateItem()
        {
            base.CreateItem();
            staticItemDef = itemDef;
        }

        public override void SetupHooks()
        {
            /*
            On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
            {
                if (!damageInfo.attacker || 
                damageInfo.damageType == DamageType.DoT || damageInfo.damageType == DamageType.NonLethal || damageInfo.damageType == DamageType.BypassBlock) 
                {
                    orig(self, damageInfo); 
                    return; 
                }

                if (!self || !self.body || !self.body.inventory) 
                { 
                    orig(self, damageInfo); 
                    return; 
                }

                var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                if (!attacker || !attacker.inventory || !attacker.healthComponent)
                {
                    orig(self, damageInfo); 
                    return; 
                }

                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) 
                {
                    orig(self, damageInfo); 
                    return; 
                }

                var attackerHealth = attacker.healthComponent.combinedHealthFraction;
                var victimHealth = self.combinedHealthFraction;

                // Damage modifier of the attack increases by 10% per stack (With base values) if the attacker has more health than the victim.
                // Damage modifier of the attack decreases by 25% (With base values) if the attacker has less health than the victim, regardless of stack count.
                var damageScalar = 1f + (attackerHealth >= victimHealth ? damageIncrease.Value * count : -damageDecrease.Value);

                damageInfo.damage *= damageScalar;

                // Prevent negative and zero damage, the minimum damage is 1.
                if(damageInfo.damage <= 1f) { damageInfo.damage = 1f; }

                orig(self, damageInfo);
            };
            */

        }

        public static bool DissonantEdgeScaleDamage(RoR2.HealthComponent __instance, DamageInfo damageInfo)
        {
            if (!damageInfo.attacker ||
            damageInfo.damageType == DamageType.DoT || damageInfo.damageType == DamageType.NonLethal || damageInfo.damageType == DamageType.BypassBlock)
            {
                return true;
            }

            if ( !__instance  || !__instance.body || !__instance.body.inventory)
            {
                return true;
            }

            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!attacker || !attacker.inventory || !attacker.healthComponent)
            {
                return true;
            }

            var count = attacker.inventory.GetItemCount(staticItemDef);
            if (count <= 0)
            {
                return true;
            }

            var attackerHealth = attacker.healthComponent.combinedHealthFraction;
            var victimHealth = __instance.combinedHealthFraction;

            // Damage modifier of the attack increases by 10% per stack (With base values) if the attacker has more health than the victim.
            // Damage modifier of the attack decreases by 25% (With base values) if the attacker has less health than the victim, regardless of stack count.
            var damageScalar = 1f + (attackerHealth >= victimHealth ? damageIncrease.Value * count : -damageDecrease.Value);

            damageInfo.damage *= damageScalar;

            // Prevent negative and zero damage, the minimum damage is 1.
            if (damageInfo.damage <= 1f) { damageInfo.damage = 1f; }

            return true;
        }

        public override void AddConfigOptions()
        {
            damageIncrease = configFile.Bind("Dissonant_Edge Config", "damageIncrease", 0.1f, "The damage increase granted by this item per stack (0.1 = +10%).");
            damageDecrease = configFile.Bind("Dissonant_Edge Config", "damageDecrease", 0.25f, "The damage decrease inflicted by this item if under the health threshold. (0.25 = 25% reduced damage).");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("WornOutStimpack.prefab");

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
                    localPos = new Vector3(0.1173F, 0.15272F, -0.00641F),
                    localAngles = new Vector3(41.80785F, 191.3737F, 184.3928F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighR",
                    localPos = new Vector3(-0.0298F, 0.2107F, 0.07734F),
                    localAngles = new Vector3(20.04926F, 47.88525F, 178.8776F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ExtraCalfL",
                    localPos = new Vector3(-0.88015F, 0.89902F, 0.05056F),
                    localAngles = new Vector3(325.1124F, 179.5492F, 5.23199F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
           {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "UpperArmL",
                    localPos = new Vector3(0.09373F, 0.03316F, 0.01377F),
                    localAngles = new Vector3(28.99673F, 162.3914F, 172.891F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
           });

            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighR",
                    localPos = new Vector3(-0.08462F, 0.36293F, 0.1016F),
                    localAngles = new Vector3(37.80873F, 12.43991F, 179.8899F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighR",
                    localPos = new Vector3(-0.13209F, 0.20849F, -0.02851F),
                    localAngles = new Vector3(43.4352F, 350.5829F, 195.3876F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "FlowerBase",
                    localPos = new Vector3(-0.49788F, 0.79773F, -0.57944F),
                    localAngles = new Vector3(351.3225F, 156.2976F, 35.23988F),
                    localScale = new Vector3(0F, 0F, 0F)
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
                    localPos = new Vector3(-1.18375F, 0.75609F, -0.29474F),
                    localAngles = new Vector3(26.76363F, 329.0323F, 182.0514F),
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
                    childName = "Pelvis",
                    localPos = new Vector3(0.21541F, 0.20836F, -0.00426F),
                    localAngles = new Vector3(43.55844F, 204.8219F, 212.8925F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "UpperArmR",
                    localPos = new Vector3(0.2064F, 0.06196F, 0.11423F),
                    localAngles = new Vector3(48.37803F, 145.3048F, 172.1066F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            return rules;
        }
    }
}