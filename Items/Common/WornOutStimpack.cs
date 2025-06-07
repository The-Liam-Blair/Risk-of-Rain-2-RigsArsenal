using System.Runtime.CompilerServices;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using BepInEx.Configuration;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
{
    /// <summary>
    /// WornOutStimpack - T1 (Common) Item
    /// <para>If the player takes damage while at or under 50% health, gain a speed and health regeneration boost.</para>
    /// <para>Item count increases boost intensity. Duration is infinite while under the health threshold.</para>
    /// </summary>
    public class WornOutStimpack : Item
    {

        public override string Name => "Worn-Out Stimpack";
        public override string NameToken => "WORNOUTSTIMPACK";
        public override string PickupToken => "Increased movement speed and health regeneration. Effects double at low health.";
        public override string Description => $"Gain <style=cIsUtility>{movementBonus.Value*100}%</style><style=cStack> (+{movementBonus.Value*100}% per stack)</style> <style=cIsUtility>movement speed</style> and <style=cIsHealing>+{regenBonus.Value} <style=cStack>(+{regenBonus.Value} per stack)</style> health regeneration</style>. While at or under <style=cIsHealth>50% health</style>, effects are <style=cIsUtility>doubled</style>.";
        public override string Lore => "<style=cMono>// INTERCEPTED TRANSMISSIONS FROM KOPRULU QUADRANT, SECTOR 19 //</style>\n\n''Looks like you've used this here Stimpack one too many times. Save it for when you REALLY need that extra kick.''\n\n''But sir... I need to heal up before the next fight.''\n\n''Nonsense. You will always recover slowly, no matter the situation. But sometimes, you'll fight enemies so strong that the healing isn't enough to offset their damage. That's where the secondary use of your Stimpack becomes clear: the adrenaline, the speed.''\n\n''I don't understand sir.''\n\n''Big strong foes are slow and lumber about. With a speed advantage, you can easily avoid their fire and come out unscathed. Outsmart your foes, and you will always come out victorious.''\n\n<style=cMono>// END OF TRANSMISSION //</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] {ItemTag.Healing};
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WornOutStimpack.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WornOutStimpack.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 3f;

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "StimpackHealStrong").buffDef;

        private ConfigEntry<float> movementBonus;
        private ConfigEntry<float> regenBonus;

        public override void SetupHooks()
        {
            // If entity takes damage that drops their health below the low health threshold, add the buff.
            // Buff is refreshed if it's currently active.
            /*
            On.RoR2.HealthComponent.TakeDamage +=
                (orig, self, damageInfo) =>
                {
                    orig(self, damageInfo);
                    if(!self) { return; }

                    var body = self.body;
                    if (!body || !body.inventory) { return; }

                    var count = body.inventory.GetItemCount(itemDef);
                    if(count <= 0) { return; }

                    if(body.healthComponent.health >= body.healthComponent.fullHealth * 0.5f) { return; }

                    // Add buff for 5 second duration. If buff already exists, refresh the duration.
                    if (body.GetBuffCount(ItemBuffDef) <= 0)
                    {
                        body.AddTimedBuff(ItemBuffDef, 5f);
                    }
                    else
                    {
                        body.ClearTimedBuffs(ItemBuffDef);
                        body.AddTimedBuff(ItemBuffDef, 5f);
                    }
                };
            */

            // todo: investigate when characterBody.recalculatestats fires, perform buff application logic in there instead of checking all instances of damage and healing?
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var self = damageReport.victimBody;
                if (!self || !self.inventory) { return; }

                var count = self.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                if (self.healthComponent.health >= self.healthComponent.fullHealth * 0.5f) { return; }

                // Add buff for 5 second duration. If buff already exists, refresh the duration.
                if (self.GetBuffCount(ItemBuffDef) <= 0)
                {
                    self.AddTimedBuff(ItemBuffDef, 5f);
                }
                else
                {
                    self.ClearTimedBuffs(ItemBuffDef);
                    self.AddTimedBuff(ItemBuffDef, 5f);
                }
            };

            // If an entity receives healing, has the buff, and is currently under the low health threshold, the buff
            // is applied or refreshed.
            On.RoR2.HealthComponent.Heal += (orig, self, amount, mask, regen) =>
            {
                if(!self) { return orig(self, amount, mask, regen); }

                var body = self.body;
                if (!body || !body.inventory) { return orig(self, amount, mask, regen); }

                var count = body.inventory.GetItemCount(itemDef);
                if (count <= 0) { return orig(self, amount, mask, regen); }

                if (body.healthComponent.health >= body.healthComponent.fullHealth * 0.5f) { return orig(self, amount, mask, regen); }

                if (body.GetBuffCount(ItemBuffDef) <= 0)
                {
                    body.AddTimedBuff(ItemBuffDef, 5f);
                }
                else
                {
                    body.ClearTimedBuffs(ItemBuffDef);
                    body.AddTimedBuff(ItemBuffDef, 5f);
                }

                return orig(self, amount, mask, regen);
            };

            // Item increases movement speed by 10% and health regen by 0.5 per stack. (Base values)
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (!self || !self.inventory) { return; }

                var count = self.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                float moveBuff = movementBonus.Value * count;
                float regenBuff = regenBonus.Value * count;

                // Double bonuses if the user is under the low health threshold.
                if (self.HasBuff(ItemBuffDef))
                {
                    moveBuff *= 2;
                    regenBuff *= 2;
                }

                args.moveSpeedMultAdd += moveBuff;
                args.baseRegenAdd += regenBuff;
            };
        }

        public override void AddConfigOptions()
        {
            movementBonus = configFile.Bind("Worn-Out_Stimpack Config", "movementBonus", 0.1f, "The movement speed bonus granted by this item. (0.1 = +10%).");
            regenBonus = configFile.Bind("Worn-Out_Stimpack Config", "regenBonus", 0.5f, "The health regeneration bonus granted by this item.");
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.9F, 0.9F, 0.9F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
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
                    localScale = new Vector3(1F, 1F, 1F)
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
                    localScale = new Vector3(0.18F, 0.18F, 0.18F)
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
                    localScale = new Vector3(0.16F, 0.16F, 0.16F)
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
                    localScale = new Vector3(0.166F, 0.166F, 0.166F)
                }
            });

            return rules;
        }
    }
}