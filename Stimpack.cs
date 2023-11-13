using On.EntityStates.VoidSurvivor.Weapon;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems
{
    public class Stimpack : Item
    {

        public override string Name => "Stimpack";
        public override string NameToken => "STIMPACK";
        public override string PickupToken => "STIMPACK";
        public override string Description => "Increased movement speed when health is low.";
        public override string Lore => "starcraft refrence!?";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override void SetupHooks()
        {
            // onServerDamageDealt - Called when damage is inflicted on an entity, generates a damage report of the event.
            GlobalEventManager.onServerDamageDealt += (DamageReport damageReport) =>
            {
                // Fetch the player reference.
                var player = PlayerCharacterMasterController.instances[0];

                // Fetch number of stimpacks player has, skip if they have none.
                //var count = player.body.inventory.GetItemCount(); //todo: fill in once itemDef build section is initialised.
                var count = 1;
                if (count <= 0) { return; }

                // Fetch the victim of the attack, skip if not the player.
                // todo: generalize using team indexes (so enemies can use this item too).
                DebugLog.Log("Item exists in inventory.");
                var other = damageReport.victim.GetComponent<CharacterBody>();
                if (other == null || !other.isPlayerControlled) { return; }

                DebugLog.Log("Player detected.");


                // Find the player's current health. If it exceeds the listed percentage, give them the speed buff, scaled by item count.
                if (player.body.healthComponent.combinedHealthFraction < 0.6f)
                {

                    DebugLog.Log("Player has low health, applying buff.");


                    // Search for the cloak speed buff in the player's buff list.
                    // If it's not there, add it. If it is, 'refresh it'.
                    if (player.body.HasBuff(RoR2Content.Buffs.CloakSpeed))
                    {
                        player.body.ClearTimedBuffs(RoR2Content.Buffs.CloakSpeed);
                        DebugLog.Log("Buff refreshed!");
                    }
                    player.body.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, count * 2);
                    DebugLog.Log($"Buff applied for {count * 2} seconds!");
                }
            };
        }
    }
}
