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
using UnityEngine.Networking;
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
        public override string Description => $"<style=cDeath>Burn</style> all enemies within <style=cIsUtility>{baseRange.Value + rangePerStack.Value}m</style> <style=cStack>(+{rangePerStack.Value}m per stack)</style> for <style=cIsDamage>{explosionDamage.Value*100f}%</style> base damage, and <style=cIsDamage>ignite</style> them for <style=cIsDamage>{burnDamage.Value*100f}%</style> <style=cStack>(+{burnDamage.Value*100f}% per stack)</style> base damage. Triggers <style=cIsUtility>{explosionsPerSecond.Value}</style> " + (explosionsPerSecond.Value == 1 ? "time" : "times") + " per second. <style=cIsVoid>Corrupts all Gasoline</style>.";
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

        public static GameObject flameVFX;

        public override void Init()
        {
            base.Init();

            var explosionTex = MainAssets.LoadAsset<Texture2D>("ExplosionTexRamp2.png");

            // Grab a copy of the gasoline explosion vfx for modification for umbral pyre.
            var origFlameVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/IgniteOnKill/IgniteExplosionVFX.prefab").WaitForCompletion();
            flameVFX = origFlameVFX.InstantiateClone("UmbralPyreExplosionVFX", true);

            var psRenderer = flameVFX.transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
            var explosionMat = Object.Instantiate(psRenderer.sharedMaterial);

            explosionMat.SetFloat("_Boost", 15f); // Set brightness boost to 15.
            explosionMat.SetFloat("_InvFade", 0f); // Set Soft Factor to 0.
            explosionMat.SetFloat("_AlphaBoost", 2f); // Set Alpha Boost to 2.
            explosionMat.SetFloat("_AlphaBias", 0.5f); // Set Alpha Bias to 0.5.
            explosionMat.SetTexture("_RemapTex", explosionTex); // Override Color Remap Ramp to custom explosion vfx.

            psRenderer.sharedMaterial = explosionMat; // Assign the new material to the particle system renderer.

            flameVFX.AddComponent<NetworkIdentity>();

            ContentAddition.AddEffect(flameVFX);
        }

        /// Implementation uses an item body behaviour [<see cref="UmbralPyreItemBehaviour">] instead of a hook
        /*
        public override void SetupHooks()
        {}
        */

        public override void AddConfigOptions()
        {
            explosionDamage = configFile.Bind("Umbral_Pyre Config", "explosionDamage", 1f, "Explosion damage scalar (1.0 = 100% of the user's damage).");
            burnDamage = configFile.Bind("Umbral_Pyre Config", "burnDamage", 0.75f, "Burn DOT scalar applied by this item per stack.");
            baseRange = configFile.Bind("Umbral_Pyre Config", "baseRange", 7, "The base radius of the explosion (in meters).");
            rangePerStack = configFile.Bind("Umbral_Pyre Config", "rangePerStack", 1, "The radius increase of the explosion per stack.");
            explosionsPerSecond = configFile.Bind("Umbral_Pyre Config", "explosionsPerSecond", 1, "The number of explosions this item will trigger per second.");
        }
    }

    /// <summary>
    /// Item body behaviour that controls the "explosions" of the Umbral Pyre item. The implementation of the item.
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

                // Item stats (With base values):
                // Radius: 7m + 1m per stack
                // Explosive damage: 100% of the user's damage
                // Burn DOT damage: 75% of the user's damage + 75% per stack
                var radius = UmbralPyre.baseRange.Value + (UmbralPyre.rangePerStack.Value * stack);
                var explosionDamage = body.damage * UmbralPyre.explosionDamage.Value;
                float DOTDamage = body.damage * UmbralPyre.burnDamage.Value * stack;


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
                if (RigsArsenal.EnableUmbralPyreVFX.Value && GlobalEventManager.igniteOnKillHurtBoxBuffer.Count > 0)
                {
                    EffectManager.SpawnEffect(UmbralPyre.flameVFX, new EffectData
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