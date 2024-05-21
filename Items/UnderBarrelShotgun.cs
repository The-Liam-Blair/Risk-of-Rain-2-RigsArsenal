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
    /// Reactive Armour Plating - T2 (Uncommon) Item.
    /// <para>Gain a temporary buff that increases armour when taking damage.</para>
    /// <para>Buff duration is low, does not prevent the damage prior to activation, but can be refreshed repeatedly if hit repeatedly.</para>
    /// </summary>
    public class UnderBarrelShotgun : Item
    {
        public override string Name => "Under-Barrel Shotgun";
        public override string NameToken => "UNDERBARRELSHOTGUN";
        public override string PickupToken => "Chance to fire a cluster of projectiles with high spread.";
        public override string Description => "<style=cIsDamage>10%</style> chance to fire<style=cIsDamage> 10 projectiles</style> with high spread. Each projectile inflicts <style=cIsDamage>15%</style> <style=cStack>(+15% per stack)</style> TOTAL damage.";
        public override string Lore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => null;
        public override GameObject Model => null;
        private GameObject pellet;

        public override void Init()
        {
            base.Init();

            DebugLog.Log("Initializing Under-Barrel Shotgun pellet...");

            // temp until custom models are made.
            GameObject proj = Resources.Load<GameObject>("prefabs/projectiles/RailgunnerPistolProjectile");
            pellet = proj.InstantiateClone("UnderBarrelShotgunPellet", true);

            pellet.GetComponent<RoR2.Projectile.ProjectileDamage>().damageType = DamageType.AOE;
            pellet.GetComponent<RoR2.Projectile.ProjectileSteerTowardTarget>().enabled = false;
            pellet.GetComponent<RoR2.Projectile.ProjectileDirectionalTargetFinder>().enabled = false;

            PrefabAPI.RegisterNetworkPrefab(pellet);

            ContentAddition.AddProjectile(pellet);

            DebugLog.Log("Finished initializing Under-Barrel Shotgun Pellet.");
        }

        public override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);

                var victim = self.body;
                if (!victim || !victim.inventory|| !info.attacker) { return; }

                var attacker = info.attacker.GetComponent<CharacterBody>();


                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                if(info.procChainMask.HasProc(ProcType.Missile)) { return; }

                for (int i = 0; i < 10; i++)
                {
                    ProcChainMask mask = info.procChainMask;
                    mask.AddProc(ProcType.Missile);

                    var projectileInfo = new FireProjectileInfo()
                    {
                        projectilePrefab = pellet,
                        position = attacker.transform.position,
                        procChainMask = mask,
                        target = victim.gameObject,
                        owner = attacker.gameObject,
                        damage = 0f,
                        crit = info.crit,
                        force = 0f,
                        damageColorIndex = DamageColorIndex.Item,
                        speedOverride = -1f,
                        damageTypeOverride = DamageType.AOE,
                    };

                   // Temp workaround: Idea is to eventually bring in a custom projectile through the asset bundle or find a better way to
                   // manipulate an existing projectile into one for this item, ideally during initialisation. Right now, this just removes
                   // some unwanted inherited visuals.
                   // var ghost = pellet.GetComponent<RoR2.Projectile.ProjectileController>().ghost.gameObject;
                   // ghost.transform.GetChild(4).GetComponent<ParticleSystemRenderer>().enabled = false; 

                    projectileInfo.rotation = Util.QuaternionSafeLookRotation(projectileInfo.target.transform.position - projectileInfo.position);

                    // I know there is a utility function that can apply spread, it's either not suited for this purpose or I'm using it incorrectly.
                    // Regardless, a custom spread function has been made to make the projectiles shoot outwards like a shotgun.
                    projectileInfo.rotation = ApplySpread(projectileInfo.rotation, 2f);

                    RoR2.Projectile.ProjectileManager.instance.FireProjectile(projectileInfo);
                }
            };
        }

        private Quaternion ApplySpread(Quaternion direction, float spread)
        {
            // Randomize the spread angles in radians for yaw and pitch
            float yawAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            float pitchAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // Calculate the spread offsets in the x and z directions (yaw and pitch)
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