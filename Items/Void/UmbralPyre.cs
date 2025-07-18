using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Items;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;
using static UnityEngine.UI.Image;

namespace RigsArsenal.Items.VoidItems
{
    /// <summary>
    /// Umbral Pyre - Void T1 (Void Common) Item
    /// <para>Once per second, apply a damaging flaming explosion centred on the user.</para>
    /// <para>Stacking increases the burn damage and slightly increases the radius of the explosion.</para>
    /// <para>This item corrupts all Gasoline items.</para>
    /// </summary>
    public class UmbralPyre : Item
    {
        public override string Name => "Umbral Pyre";
        public override string NameToken => "UMBRALPYRE";
        public override string PickupToken => "Damage and burn all nearby enemies once per second. <style=cIsVoid>Corrupts all Gasoline</style>.";
        public override string Description => $"<style=cDeath>Burn</style> all enemies within <style=cIsUtility>{baseRange.Value + rangePerStack.Value}m</style> <style=cStack>(+{rangePerStack.Value}m per stack)</style> for <style=cIsDamage>{explosionDamage.Value*100f}%</style> base damage, and <style=cIsDamage>ignite</style> them for <style=cIsDamage>{burnDamage.Value*100f}%</style> <style=cStack>(+{burnDamage.Value*100f}% per stack)</style> base damage. Occurs <style=cIsUtility>{explosionsPerSecond.Value}</style> per second. <style=cIsVoid>Corrupts all Gasoline</style>.";
        public override string Lore => "<style=cMono>========================================\r\n====   MyBabel Machine Translator   ====\r\n====     [Version 15.01.3.000 ]   ======\r\n========================================\r\nTraining... <1000000000 cycles>\r\nTraining... <1000000000 cycles>\r\nTraining... <4054309 cycles>\r\nComplete!\r\nDisplay result? Y/N\r\nY\r\n========================================</style>\r\n\r\n<style=cIsVoid>Imperfect design. Uncontrolled. Prone to friendly fire. Human design, of course.\r\n\r\nImprove. Remove the redundant elements. The base form enables its power, unsuppressed.\r\n\r\nIt knows. Friend and foe. Teach and inform, it will listen, and it will only hurt the opposition.\r\n\r\nReplicate. The air holds enough mass to enable replication. It will continue unhindered.\r\n\r\nHuman design, made perfect.\r\n\r\nGo now, and spread our message. Knowledge through disintegration.</style>";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("UmbralPyre.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("UmbralPyre.prefab");

        public override float minViewport => 0.66f;
        public override float maxViewport => 1.33f;

        public static ConfigEntry<float> explosionDamage;
        public static ConfigEntry<float> burnDamage;
        public static ConfigEntry<int> baseRange;
        public static ConfigEntry<int> rangePerStack;
        public static ConfigEntry<int> explosionsPerSecond;

        public override ItemDef pureItemDef => RoR2Content.Items.IgniteOnKill; // Gasoline

        /*
        public override void SetupHooks()
        {         
            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                orig(self);

                if(!Run.instance || !self || !self.inventory) { return; }

                var count = self.inventory.GetItemCount(itemDef);

                // If the player has no stacks of this item, remove the timer component (if one exists).
                if (count <= 0) 
                {
                    if (timer)
                    {
                        GameObject.Destroy(timer);
                        timer = null;
                        timerStart = 1f;
                    }
                    return; 
                }

                if(!timer)
                {
                    DebugLog.Log($"Timer start ({timerStart}) set to {1f/explosionsPerSecond.Value}.");
                    // Component that holds a timer to track the cooldown of each explosion.
                    timer = self.gameObject.AddComponent<PyreTimer>();
                    timerStart = 1f / explosionsPerSecond.Value;
                    DebugLog.Log($"Umbral Pyre timer set to {timerStart} seconds per explosion.");
                }

                if (timer.timer <= 0f)
                {
                    DebugLog.Log($"Umbral Pyre explosion triggered with seconds = {timer.timer}.");
                    timer.timer = timerStart;

                    DebugLog.Log($"Resetting timer to {timer.timer} seconds.");

                    // 8m + 1m per stack (Base values).
                    var radius = baseRange.Value + (count * rangePerStack.Value);

                    // Damage of each "explosion" is 100% of the user's damage (Base values).
                    var explosiveDamage = self.damage * explosionDamage.Value;

                    // Damage of the DOT is 75% (+75% per stack) of the user's damage (Base values).
                    var DOTDamage = self.damage * burnDamage.Value * count;

                    var pos = self.corePosition;

                    // Borrowing the implementation of the igniteOnKill (gasoline) proc activation function:

                    // Get all non-friendly entities within the calculated radius.
                    SphereSearch igniteOnKillSphereSearch = new SphereSearch();

                    GlobalEventManager.igniteOnKillSphereSearch.origin = pos;
                    GlobalEventManager.igniteOnKillSphereSearch.mask = LayerIndex.entityPrecise.mask;
                    GlobalEventManager.igniteOnKillSphereSearch.radius = radius;
                    GlobalEventManager.igniteOnKillSphereSearch.RefreshCandidates();
                    GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(self.teamComponent.teamIndex));
                    GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                    GlobalEventManager.igniteOnKillSphereSearch.OrderCandidatesByDistance();
                    GlobalEventManager.igniteOnKillSphereSearch.GetHurtBoxes(GlobalEventManager.igniteOnKillHurtBoxBuffer);
                    GlobalEventManager.igniteOnKillSphereSearch.ClearCandidates();

                    // For each entity found, apply the burn DOT.
                    for (int i = 0; i < GlobalEventManager.igniteOnKillHurtBoxBuffer.Count; i++)
                    {
                        HurtBox hurtBox = GlobalEventManager.igniteOnKillHurtBoxBuffer[i];
                        if (hurtBox.healthComponent)
                        {
                            RigsArsenal.InflictDot(self, hurtBox.healthComponent.body, DotController.DotIndex.Burn, DOTDamage);
                        }
                    }

                    // Generate the explosion damage.
                    new BlastAttack
                    {
                        radius = radius,
                        baseDamage = explosiveDamage,
                        procCoefficient = 0f,
                        crit = Util.CheckRoll(self.crit, self.master),
                        damageColorIndex = DamageColorIndex.Item,
                        attackerFiltering = AttackerFiltering.Default,
                        falloffModel = BlastAttack.FalloffModel.None,
                        attacker = self.gameObject,
                        teamIndex = self.teamComponent.teamIndex,
                        position = pos
                    }.Fire();

                    // Create the visual effect of the explosion.
                    // Explosion vfx is not created if its disabled in the config or if no targets were hit.
                    // TODO: Custom visual effects.
                    if (RigsArsenal.EnableUmbralPyreVFX.Value && GlobalEventManager.igniteOnKillHurtBoxBuffer.Count > 0)
                    {
                        EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
                        {
                            origin = pos,
                            scale = radius,
                            rotation = Util.QuaternionSafeLookRotation(Vector3.up)
                        }, true);
                    }

                    GlobalEventManager.igniteOnKillHurtBoxBuffer.Clear();
                }
            };
                
        }
        */

        public override void AddConfigOptions()
        {
            explosionDamage = configFile.Bind("Umbral_Pyre Config", "explosionDamage", 1f, "Explosion damage scalar (1.0 = 100% of the user's damage).");
            burnDamage = configFile.Bind("Umbral_Pyre Config", "burnDamage", 0.75f, "Burn DOT scalar applied by this item per stack.");
            baseRange = configFile.Bind("Umbral_Pyre Config", "baseRange", 7, "The base radius of the explosion (in meters).");
            rangePerStack = configFile.Bind("Umbral_Pyre Config", "rangePerStack", 1, "The radius increase of the explosion per stack.");
            explosionsPerSecond = configFile.Bind("Umbral_Pyre Config", "explosionsPerSecond", 1, "The number of explosions this item will trigger per second.");
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

    /// <summary>
    /// Item body behaviour that controls the "explosions" of the Umbral Pyre item. Essentially the implementation of the item.
    /// </summary>
    public class UmbralPyreItemBehaviour : BaseItemBodyBehavior
    {
        private float timer; // Current timer
        private float timerStart; // Reset value.

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return RigsArsenal.ItemList.Find(x => x.NameToken == "UMBRALPYRE").itemDef;
        }

        // Timer reset value scaled from the user-defined explosions per second value.
        private void OnEnable()
        {
            timerStart = 1f / UmbralPyre.explosionsPerSecond.Value;
        }

        private void FixedUpdate()
        {
            if (!body || !body.inventory || !Run.instance) { return; }

            timer -= Time.fixedDeltaTime;

            if (timer <= 0f)
            {
                timer = timerStart;
                var item = RigsArsenal.ItemList.Find(x => x.NameToken == "UMBRALPYRE") as UmbralPyre;

                // Item stats (With base values):
                // Radius: 7m + 1m per stack
                // Explosive damage: 100% of the user's damage
                // Burn DOT damage: 75% of the user's damage + 75% per stack
                var radius = UmbralPyre.baseRange.Value + (UmbralPyre.rangePerStack.Value * stack);
                var explosionDamage = body.damage * UmbralPyre.explosionDamage.Value;
                var DOTDamage = body.damage * UmbralPyre.burnDamage.Value * stack;

                DebugLog.Log($"Damage: {explosionDamage}, DOT Damage: {DOTDamage}, Radius: {radius}");


                // Borrowing the implementation of the igniteOnKill (gasoline) proc activation function:

                // Get all non-friendly entities within the calculated radius.
                SphereSearch igniteOnKillSphereSearch = new SphereSearch();

                GlobalEventManager.igniteOnKillSphereSearch.origin = body.corePosition;
                GlobalEventManager.igniteOnKillSphereSearch.mask = LayerIndex.entityPrecise.mask;
                GlobalEventManager.igniteOnKillSphereSearch.radius = radius;
                GlobalEventManager.igniteOnKillSphereSearch.RefreshCandidates();
                GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex));
                GlobalEventManager.igniteOnKillSphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                GlobalEventManager.igniteOnKillSphereSearch.OrderCandidatesByDistance();
                GlobalEventManager.igniteOnKillSphereSearch.GetHurtBoxes(GlobalEventManager.igniteOnKillHurtBoxBuffer);
                GlobalEventManager.igniteOnKillSphereSearch.ClearCandidates();

                // For each entity found, apply the burn DOT.
                for (int i = 0; i < GlobalEventManager.igniteOnKillHurtBoxBuffer.Count; i++)
                {
                    HurtBox hurtBox = GlobalEventManager.igniteOnKillHurtBoxBuffer[i];
                    if (hurtBox.healthComponent)
                    {
                        RigsArsenal.InflictDot(body, hurtBox.healthComponent.body, DotController.DotIndex.Burn, DOTDamage);
                    }
                }

                // Generate the explosion damage.
                new BlastAttack
                {
                    radius = radius,
                    baseDamage = explosionDamage,
                    procCoefficient = 0f,
                    crit = Util.CheckRoll(body.crit, body.master),
                    damageColorIndex = DamageColorIndex.Item,
                    attackerFiltering = AttackerFiltering.Default,
                    falloffModel = BlastAttack.FalloffModel.None,
                    attacker = body.gameObject,
                    teamIndex = body.teamComponent.teamIndex,
                    position = body.corePosition
                }.Fire();

                // Create the visual effect of the explosion.
                // Explosion vfx is not created if its disabled in the config or if no targets were hit.
                // TODO: Custom visual effects.
                if (RigsArsenal.EnableUmbralPyreVFX.Value && GlobalEventManager.igniteOnKillHurtBoxBuffer.Count > 0)
                {
                    EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
                    {
                        origin = body.corePosition,
                        scale = radius,
                        rotation = Util.QuaternionSafeLookRotation(Vector3.up)
                    }, true);
                }

                GlobalEventManager.igniteOnKillHurtBoxBuffer.Clear();
            }
        }
    }
}