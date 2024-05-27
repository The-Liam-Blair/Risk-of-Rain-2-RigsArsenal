using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IL.EntityStates;
using MoreItems.Buffs;
using On.RoR2.Projectile;
using R2API;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using ProjectileController = RoR2.Projectile.ProjectileController;
using static MoreItems.MoreItems;
using ProjectileGhostController = RoR2.Projectile.ProjectileGhostController;

namespace MoreItems.Items
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
        public override string Description => "<style=cIsDamage>10%</style> chance to fire<style=cIsDamage> 13 projectiles</style> with high spread. Each projectile inflicts <style=cIsDamage>25%</style> <style=cStack>(+25% per stack)</style> TOTAL damage.";
        public override string Lore => "<style=cMono>// UNKNOWN CHATTER RECORDED ONBOARD THE UES CONTACT LIGHT //</style>\n\n''Guns are so lame; heavy, cumbersome, generic, lacking style and flair.''\n\n''Who cares? It's a weapon, it has a purpose already, it does not need flair.''\n\n''Why couldn't it have flair? Picture this: Shooting baddies, left, right and centre, just by pointing at them. Like a superhero. Absolutely magic man. And compare that to carrying a heavy, uncomfortable steel rifle.''\n\n''What do you mean 'Like a superhero'? You just bolted a bunch of shotgun barrels onto a circular piece of leather, you've successfully made a gun thats more difficult to maintain and use in the name of 'style', not to mention that four shotguns firing at once is just plain overkill.''\n\n''It's just something you won't understand...''\n\n''Maybe...How do 4 masterkey shotguns fire a total of 13 pellets anyway? Are some of them malfunctioning?''\n\n''Its luck, y'know. My lucky number.''\n\n<style=cMono>// END OF CONVERSATION //</style>";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WristMountedShotgun.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WristMountedShotgun.prefab");
        private GameObject pellet;

        // Donut ring object that attaches to the player when the item is active to indicate range, much like the focus crystal.
        private GameObject rangeIndicator = null;

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

            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);

                if (!self|| !info.attacker || info.procCoefficient <= 0f || info.procChainMask.HasProc(ProcType.Missile)) { return; }

                var victim = self.body;
                var attacker = info.attacker.GetComponent<CharacterBody>();

                if (!victim || !attacker || !attacker.healthComponent || !victim.healthComponent) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                // Only triggers against entities 25 or fewer units away
                if (Vector3.Distance(victim.transform.position, attacker.transform.position) > 25f) { return; }

                // 10% (scaled by the attack's proc coefficient) chance to trigger the effect.
                //if (!Util.CheckRoll(0.1f * info.procCoefficient, attacker.master)) { return; }

                // Because they're projectiles, also performs a line of sight check so that the shot is actually able to hit.
                // (Not entirely consistent, might investigate a better solution down the line).
                if (Physics.Linecast(attacker.transform.position, victim.transform.position, 0)) { return; }

                int pelletCount = 13;
                float stackingDamageMultiplier = 0.25f * count; // 25% the attack's damage per pellet per stack.

                for (int i = 0; i < pelletCount; i++)
                {
                    ProcChainMask mask = info.procChainMask;
                    mask.AddProc(ProcType.Missile);

                    float damage = Util.OnHitProcDamage(info.damage * stackingDamageMultiplier, attacker.damage, info.procCoefficient);

                    // Each pellet has 25% of the attack's proc coefficient. This totals to a 3.25 proc coefficient factor more or less.
                    pellet.GetComponent<ProjectileController>().procCoefficient = info.procCoefficient * 0.25f;

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
                    projectileInfo.rotation = ApplySpread(projectileInfo.rotation, 2f);

                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(projectileInfo);
                }
            };

            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);

                rangeIndicator = null;
            };

            // Adds a visual range indicator when a player has the item.
            // todo: Config option to disable the indicator as it may be distracting.
            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);

                if (MoreItems.EnableShotgunMarker.Value && self.isPlayerControlled && self.inventory.GetItemCount(itemDef) > 0 && !rangeIndicator)
                {
                    // Uses the range indicator from the "NearbyDamageBonus" (focus crystal) item.
                    GameObject original = Resources.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
                    rangeIndicator = original.InstantiateClone("UnderBarrelShotgunRangeIndicator", true);

                    PrefabAPI.RegisterNetworkPrefab(rangeIndicator);

                    rangeIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject, null);

                    var donut = rangeIndicator.transform.GetChild(1); // 2nd child of the range indicator object controls the donut's visual properties.
                    donut.localScale = new Vector3(50f, 50f, 50f); // 25m radius to match the item's range.
                    donut.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0f, 0.29f, 0.06f)); // Green tint instead of red.
                }
            };
        }

        /// <summary>
        /// Randomize the angle of a given direction, to simulate spread.
        /// </summary>
        /// <param name="direction">The current, fixed aiming direction of a projectile.</param>
        /// <param name="spread">Spread intensity factor: Larger values produce larger angle values</param>
        /// <returns>The original angle offsetted randomly according to the spread intensity.</returns>
        private Quaternion ApplySpread(Quaternion direction, float spread)
        {
            // Randomize the spread angles in radians for yaw and pitch (In radians).
            float yawAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            float pitchAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // Calculate the spread offsets in the x and z directions (yaw and pitch) w.r.t the spread intensity.
            float xOffset = spread * Mathf.Cos(yawAngle);
            float zOffset = spread * Mathf.Sin(pitchAngle);

            // Apply the spread offsets to the aim direction
            Quaternion spreadYaw = Quaternion.Euler(0f, xOffset, 0f);
            Quaternion spreadPitch = Quaternion.Euler(zOffset, 0f, 0f);
            Quaternion spreadDirection = spreadYaw * spreadPitch * direction;

            return spreadDirection.normalized;
        }
    }
}