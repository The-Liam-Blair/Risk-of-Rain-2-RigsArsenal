using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MoreItems.Equipments
{
    public class NidusVirus : Equipment
    {
        public override string Name => "Nidus Virus";

        public override string NameToken => "NIDUSVIRUS";

        public override string PickupToken => "Target enemy's debuffs are spread to nearby enemies.";

        public override string Description => "Target an enemy to spread their <style=cIsUtility>debuffs</style> to enemies up to <style=cIsUtility>20 units</style> away at <style=cIsHealth>100% effectiveness</style>.";
        public override string Lore => "";

        public override bool isLunar => false;

        public override float cooldown => 3;

        public override bool AIBlackList => false;

        public override Sprite Icon => null;

        public override GameObject Model => null;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            DebugLog.Log("LMAO");
            return true;
        }

        public override void SetupHooks()
        {

        }
    }


}
