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
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WornOutStimpack.prefab");

        private GameObject pellet;

        public override void Init()
        {
            base.Init();


            DebugLog.Log("Initializing Under-Barrel Shotgun pellet...");
            // temp until custom models are made.
            pellet = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/Thermite"), "UnderBarrelShotgunPellet", true);

            pellet.AddComponent<ProjectileGhostController>();

            var projectileController = pellet.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = Model;
            pellet.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
            pellet.GetComponent<RoR2.Projectile.ProjectileDamage>().damageType = DamageType.Generic;
            pellet.GetComponent<RoR2.Projectile.ProjectileDamage>().damage = 1f;
           
            PrefabAPI.RegisterNetworkPrefab(pellet);

            ContentAddition.AddProjectile(pellet);

            DebugLog.Log("Finished initializing Under-Barrel Shotgun Pellet.");
        }

        public override void SetupHooks()
        {
            // todo: null reference on projectile manager.Start(). Unable to determine the cause through debugging.
            // todo: Revise projectile spawning and preconditions for spawning. Research proper method of spawning projectiles (This is probably wildly incorrect).
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);


                var victim = self.body;
                if (!victim || !victim.inventory|| !info.attacker) { return; }

                var attacker = info.attacker.GetComponent<CharacterBody>();


                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                DebugLog.Log("Firing projectile...");


                var projectileInfo = new FireProjectileInfo();

                projectileInfo.owner = attacker.gameObject;
                projectileInfo.projectilePrefab = pellet;
                projectileInfo.speedOverride = 100f;
                projectileInfo.damage = 1f;
                projectileInfo.damageTypeOverride = null;
                projectileInfo.damageColorIndex = DamageColorIndex.Default;
                projectileInfo.procChainMask = default;

                RoR2.Projectile.ProjectileManager.instance.FireProjectile(projectileInfo);

                DebugLog.Log("Finished firing projectile.");
            };
        }
    }
}
