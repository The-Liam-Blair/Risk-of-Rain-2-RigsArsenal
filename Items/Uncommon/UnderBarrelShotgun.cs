using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL.EntityStates;
using RigsArsenal.Buffs;
using On.RoR2.Projectile;
using R2API;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using ProjectileController = RoR2.Projectile.ProjectileController;
using static RigsArsenal.RigsArsenal;
using ProjectileGhostController = RoR2.Projectile.ProjectileGhostController;
using BepInEx.Configuration;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Wrist-Mounted Shotgun - T2 (Uncommon) Item.
    /// <para>10% chace on hit to launch a spread of projectiles at close range, inflicting high damage.</para>
    /// <para>Equivalent to the ATG with slightly higher damage and much higher proc rate, it shares the same proc mask so are mutually exclusive.</para>
    /// <para>Close range attacks -> fire the shotgun. Long range attacks -> fire the ATG.</para>
    /// </summary>
    public class UnderBarrelShotgun : Item
    {
        public override string Name => "Wrist-Mounted Shotgun";
        public override string NameToken => "UNDERBARRELSHOTGUN";
        public override string PickupToken => "Chance to fire a cluster of projectiles with high spread.";
        public override string Description => $"<style=cIsDamage>{itemProcChance.Value}%</style> chance to fire<style=cIsDamage> {projectileCount.Value} projectiles</style> with high spread. Each projectile inflicts <style=cIsDamage>{projectileDamage.Value * 100f}%</style> <style=cStack>(+{projectileDamage.Value * 100f}% per stack)</style> TOTAL damage.";
        public override string Lore => "<style=cMono>// UNKNOWN CHATTER RECORDED ONBOARD THE UES CONTACT LIGHT //</style>\n\n''Guns are so lame; heavy, cumbersome, generic, lacking style and flair.''\n\n''Who cares? It's a weapon, it has a purpose already, it does not need flair.''\n\n''Why couldn't it have flair? Picture this: Shooting baddies, left, right and centre, just by pointing at them. Like a superhero. Absolutely magic man. And compare that to carrying a heavy, uncomfortable steel rifle.''\n\n''What do you mean 'Like a superhero'? You just bolted a bunch of underbarrel shotguns onto a crude plate of metal hammered into a half-pipe shape. You've successfully made a gun thats more difficult to maintain and use in the name of 'style', not to mention that four shotguns firing at once is just plain overkill.''\n\n''It's just something you won't understand...''\n\n''Maybe...How do 4 masterkey shotguns fire a total of 13 pellets anyway? Are some of them malfunctioning?''\n\n''Its luck, y'know. My lucky number.''\n\n<style=cMono>// END OF CONVERSATION //</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WristMountedShotgun.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WristMountedShotgun.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 2.5f;


        private GameObject pellet;

        // Donut ring object that attaches to the player when the item is active to indicate range, much like the focus crystal.
        private GameObject rangeIndicator = null;

        private ConfigEntry<float> itemProcChance;
        private ConfigEntry<int> itemRange;
        private ConfigEntry<int> projectileCount;
        private ConfigEntry<float> projectileDamage;
        private ConfigEntry<float> projectileProcRate;


        public override void Init()
        {
            base.Init();

            
            GameObject proj = Resources.Load<GameObject>("prefabs/projectiles/RailgunnerPistolProjectile");
            pellet = proj.InstantiateClone("UnderBarrelShotgunPellet", true);

            pellet.GetComponent<RoR2.Projectile.ProjectileDamage>().damageType = DamageType.AOE;
            pellet.GetComponent<RoR2.Projectile.ProjectileSteerTowardTarget>().enabled = false;
            pellet.GetComponent<RoR2.Projectile.ProjectileDirectionalTargetFinder>().enabled = false;

            GameObject aGhost = Resources.Load<GameObject>("prefabs/projectileghosts/RailgunnerPistolProjectileGhost");
            var ghost = aGhost.InstantiateClone("UnderBarrelShotgunPelletGhost", true);
            ghost.AddComponent<NetworkIdentity>();

            ghost.transform.GetChild(4).GetComponent<ParticleSystemRenderer>().enabled = false;

            var trail = ghost.transform.GetChild(0).GetComponent<TrailRenderer>();
            trail.widthMultiplier = 0.1f;

            var mat = trail.sharedMaterial;
            mat.SetColor("_TintColor", new Color(0f, 0.29f, 0.06f)); // Green

            pellet.GetComponent<ProjectileController>().ghostPrefab = ghost;

            PrefabAPI.RegisterNetworkPrefab(pellet);
            PrefabAPI.RegisterNetworkPrefab(ghost);

            ContentAddition.AddProjectile(pellet);
        }

        public override void SetupHooks()
        {
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var info = damageReport.damageInfo;
                var self = damageReport.victim;

                if (!info.attacker || info.procCoefficient <= 0f || info.procChainMask.HasProc(ProcType.Missile)) { return; }


                var victim = self.body;
                var attacker = info.attacker.GetComponent<CharacterBody>();

                if (!victim || !attacker || !attacker.healthComponent || !victim.healthComponent || !attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                // Only triggers against entities 35 metres away or less (At base values).
                if (Vector3.Distance(victim.transform.position, attacker.transform.position) > itemRange.Value) { return; }

                // 10% (scaled by the attack's proc coefficient) chance to trigger the effect.
                if (!Util.CheckRoll(itemProcChance.Value * info.procCoefficient, attacker.master)) { return; }

                int pelletCount = projectileCount.Value;
                float stackingDamageMultiplier = projectileDamage.Value * count; // 25% the attack's damage per pellet per stack. (With base values).

                ProcChainMask mask = info.procChainMask;
                mask.AddProc(ProcType.Missile);

                float damage = Util.OnHitProcDamage(info.damage * stackingDamageMultiplier, attacker.damage, info.procCoefficient);

                // Each pellet has 25% of the attack's proc coefficient. This totals to a 3.25 proc coefficient factor combined on average if every shot hits.
                // (With base values).
                pellet.GetComponent<ProjectileController>().procCoefficient = info.procCoefficient * projectileProcRate.Value;

                // todo: shotgun sound effect.

                for (int i = 0; i < pelletCount; i++)
                {
                    var projectileInfo = new FireProjectileInfo()
                    {
                        projectilePrefab = pellet,
                        position = attacker.transform.position,
                        procChainMask = mask,
                        target = victim.gameObject,
                        owner = attacker.gameObject,
                        damage = damage,
                        crit = info.crit,
                        force = 50f,
                        damageColorIndex = DamageColorIndex.Item,
                        speedOverride = -1f,
                        damageTypeOverride = DamageType.AOE,
                    };

                    projectileInfo.rotation = Util.QuaternionSafeLookRotation(projectileInfo.target.transform.position - projectileInfo.position);

                    // I know there is a utility function that can apply spread, it's either not suited for this purpose or I'm using it incorrectly.
                    // Regardless, a custom spread function has been made to make the projectiles shoot outwards like a shotgun.
                    projectileInfo.rotation = ApplySpread(projectileInfo.rotation, 4f);

                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(projectileInfo);
                }
            };

            // Adds a visual range indicator when a player has the item.
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);

                if (!RigsArsenal.EnableShotgunMarker.Value || !self.isPlayerControlled || !self.inventory
                    || self.inventory.GetItemCount(itemDef) <= 0) 
                {
                    if(rangeIndicator) {  GameObject.Destroy(rangeIndicator); rangeIndicator = null; }
                    return; 
                }

                // Uses a modified range indicator from the "NearbyDamageBonus" (focus crystal) item.
                GameObject original = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
                rangeIndicator = original.InstantiateClone("UnderBarrelShotgunRangeIndicator", true);

                PrefabAPI.RegisterNetworkPrefab(rangeIndicator);

                rangeIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject, null);

                var donut = rangeIndicator.transform.GetChild(1); // 2nd child of the range indicator object controls the donut's visual properties.
                donut.localScale = new Vector3(itemRange.Value * 2f, itemRange.Value * 2f, itemRange.Value * 2f); // Scaled by the item's range.
                donut.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0f, 0.03f, 0.3f)); // Blue tint instead of red.
            };
        }

        public override void AddConfigOptions()
        {
            itemProcChance = configFile.Bind("Wrist-Mounted_Shotgun Config", "itemProcChance", 10f, "The base proc chance of the item as a percentage.");
            itemRange = configFile.Bind("Wrist-Mounted_Shotgun Config", "itemRange", 35, "The maximum range which this item can trigger. Also scales the visual indicator range if enabled.");
            projectileCount = configFile.Bind("Wrist-Mounted_Shotgun Config", "projectileCount", 13, "The number of projectiles fired by the item.");
            projectileDamage = configFile.Bind("Wrist-Mounted_Shotgun Config", "projectileDamage", 0.25f, "The damage of each projectile (Scaled from the damage of the proc that triggered the item).");
            projectileProcRate = configFile.Bind("Wrist-Mounted_Shotgun Config", "projectileProcRate", 0.25f, "The proc coefficient of each projectile (Scaled from the proc coefficient of the attack that triggered the item)");

        }

        /// <summary>
        /// Randomize the angle of a given direction, to simulate spread.
        /// </summary>
        /// <param name="direction">The current, fixed aiming direction of a projectile.</param>
        /// <param name="spread">Spread intensity factor: Larger values produce larger angle values</param>
        /// <returns>The original angle offsetted randomly according to the spread intensity.</returns>
        private Quaternion ApplySpread(Quaternion direction, float spread)
        {
            // Randomize the spread angles in radians for yaw and pitch.
            float yawAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            float pitchAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // Calculate the spread offsets in the x and z directions (yaw and pitch) w.r.t the spread intensity.
            float xOffset = spread * Mathf.Cos(yawAngle);
            float zOffset = spread * Mathf.Sin(pitchAngle);

            // Apply the spread offsets to the aim direction.
            Quaternion spreadYaw = Quaternion.Euler(0f, xOffset, 0f);
            Quaternion spreadPitch = Quaternion.Euler(zOffset, 0f, 0f);
            Quaternion spreadDirection = spreadYaw * spreadPitch * direction;

            return spreadDirection.normalized;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("WristMountedShotgun.prefab");

            var itemDisplay = display.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(display);

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.02451F, 0.34024F, -0.00167F),
                    localAngles = new Vector3(315.5563F, 355.3339F, 81.05944F),
                    localScale = new Vector3(0.3F, 0.3F, 0.3F)
                }
            });

            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.01985F, 0.18101F, 0.02689F),
                    localAngles = new Vector3(316.665F, 246.1732F, 84.86661F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });

            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.01626F, 0.13756F, 0.01655F),
                    localAngles = new Vector3(304.7177F, 222.5652F, 91.26649F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });

            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.44827F, 2.64868F, -0.06426F),
                    localAngles = new Vector3(316.5246F, 20.30819F, 91.83681F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });

            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.00816F, 0.21112F, 0.0246F),
                    localAngles = new Vector3(321.7304F, 350.2334F, 81.78397F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });

            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.03421F, 0.22446F, -0.02513F),
                    localAngles = new Vector3(313.6138F, 199.0392F, 115.5678F),
                    localScale = new Vector3(0.25F, 0.25F, 0.25F)
                }
            });

            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.00072F, 0.21029F, -0.02355F),
                    localAngles = new Vector3(306.5188F, 285.6452F, 78.42603F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });

            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.00922F, -0.14646F, 0.37562F),
                    localAngles = new Vector3(318.8769F, 87.49858F, 90.94623F),
                    localScale = new Vector3(0.6F, 0.6F, 0.6F)
                }
            });

            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "MechHandR",
                    localPos = new Vector3(0.00949F, 0.2566F, 0.02934F),
                    localAngles = new Vector3(314.6177F, 107.579F, 106.8127F),
                    localScale = new Vector3(0.4F, 0.4F, 0.4F)
                }
            });

            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.15604F, 4.41402F, -0.00707F),
                    localAngles = new Vector3(316.8484F, 214.3637F, 99.96652F),
                    localScale = new Vector3(3F, 3F, 3F)
                }
            });

            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "HandL",
                    localPos = new Vector3(-0.0011F, 0.19803F, 0.0306F),
                    localAngles = new Vector3(315.4191F, 275.4706F, 83.80331F),
                    localScale = new Vector3(0.25F, 0.25F, 0.24F)
                }
            });

            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "LowerArmR",
                    localPos = new Vector3(-0.00057F, 0.16336F, -0.00884F),
                    localAngles = new Vector3(313.0686F, 290.9871F, 113.7543F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });

            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ForeArmL",
                    localPos = new Vector3(-0.04329F, 0.25132F, -0.00479F),
                    localAngles = new Vector3(316.7375F, 181.0666F, 63.56673F),
                    localScale = new Vector3(0.33F, 0.33F, 0.33F)
                }
            });

            return rules;
        }
    }
}