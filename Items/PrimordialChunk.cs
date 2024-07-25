using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static MoreItems.MoreItems;

namespace MoreItems.Items
{
    /// <summary>
    /// NeedleRounds - T1 (Common) Item
    /// <para>While charging the teleporter, slowly gain stacks.</para>
    /// <para>Each stack increases attack speed. Stacks are slowly lost when no longer charging the teleporter.</para>
    /// </summary>
    public class PrimordialChunk : Item
    {
        public override string Name => "Primordial Chunk";
        public override string NameToken => "PRIMORDIALCHUNK";
        public override string PickupToken => "Gain increasing attack speed while charging a teleporter.";
        public override string Description => "Gain <style=cIsDamage>9% attack speed</style> every 2 seconds <style=cIsUtility>while charging a teleporter</style>. Maximum cap of 27% <style=cStack>(+27% per stack)</style> attack speed.";
        public override string Lore => "''The teleporters manipulate space and time to transport beings across vast distances. This chunk here can't teleport us anymore, but it still exhibits some latent, dormant energy.\n\nTry bringing it near an active teleporter and see what happens when the dormant energy is reactivated. Its temporal energy in theory could warp space and time in a small radius around you, literally speeding up time locally. \n\nOf course its only a theory, but thats why we have lab rats like you, right?\n\nGood luck.''";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;
        
        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("PrimordialChunk.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("PrimordialChunk.prefab");

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "PrimordialChunkAttackSpeed").buffDef;


        private float buffDuration = 3f;

        public override void SetupHooks()
        {
            On.RoR2.TeleporterInteraction.FixedUpdate += (orig, self) =>
            {
                orig(self);

                // Only run if the teleporter is charging.
                if (!self.isCharging) { return; }

                var zone = self.holdoutZoneController;
                ReadOnlyCollection<TeamComponent> playerTeam = TeamComponent.GetTeamMembers(TeamIndex.Player);

                // For each player..
                foreach(var player in playerTeam)
                {
                    // If a player is within the charging radius of the teleporter...
                    if(HoldoutZoneController.IsBodyInChargingRadius(zone, zone.transform.position, zone.currentRadius * zone.currentRadius, player.body))
                    {
                        // Give them the item's buff if they don't have it.
                        var itemCount = player.body.inventory.GetItemCount(itemDef);

                        if (!player.body.HasBuff(ItemBuffDef))
                        {
                            player.body.AddTimedBuff(ItemBuffDef, buffDuration, itemCount * 3);
                        }

                        // If the player has the buff already and the last (set) of buffs have a remaining timer of 1 second or less (2+ seconds have passed)
                        // refresh the timers of all previous stacks of the buff, and add another stack afterwards.
                        else if (player.body.timedBuffs.Find(x => x.buffIndex == ItemBuffDef.buffIndex).timer <= 1f)
                        {
                            var buffStacks = player.body.timedBuffs.Where(x => x.buffIndex == ItemBuffDef.buffIndex);

                            foreach(var buff in buffStacks)
                            {
                                buff.timer = buffDuration;
                            }

                            player.body.AddTimedBuff(ItemBuffDef, buffDuration, itemCount * 3);
                        }
                    }
                }
            };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("NeedleRounds.prefab");

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