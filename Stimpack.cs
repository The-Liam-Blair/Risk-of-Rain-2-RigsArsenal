using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems
{
    public class Stimpack : ItemAbstract
    {

        public override string Name => "Stimpack";
        public override string NameToken => "STIMPACK";
        public override string PickupToken => "STIMPACK";
        public override string Description => "Increased movement speed when health is low.";
        public override string Lore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;


        public void Awake()
        {
            DebugLog.Log("Initialising stimpack...");

            DebugLog.Log("Stimpack initialised.");
        }

        public override void SetupHooks()
        {
            GlobalEventManager.onServerDamageDealt += GEM_onServerDamageDealt;
        }

        private void GEM_onServerDamageDealt(DamageReport damageReport)
        {
            // todo: this stuff
        }
    }
}
