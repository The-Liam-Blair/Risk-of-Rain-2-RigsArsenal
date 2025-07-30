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
using UnityEngine.XR;
using BepInEx.Configuration;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
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
        public override string Description => $"Gain <style=cIsDamage>{atkSpeedBonus.Value*100f}% attack speed</style> every second <style=cIsUtility>while charging a teleporter</style>. Maximum cap of {(atkSpeedBonus.Value * 100f) * maxBuffStacks.Value}% <style=cStack>(+{(atkSpeedBonus.Value * 100f) * maxBuffStacks.Value}% per stack)</style> attack speed.";
        public override string Lore => "''The teleporters manipulate space and time to transport beings across vast distances. This chunk from the primordial teleporter can't teleport us anymore, but it still exhibits some latent, dormant energy.\n\nTry bringing it near an active teleporter and see what happens when the dormant energy is reactivated. Its temporal energy in theory could warp space and time in a small radius around you, literally speeding up time locally. \n\nOf course its only a theory, but thats why we have lab rats like you, right?\n\nGood luck.''";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;
        
        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("PrimordialChunk.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("PrimordialChunk.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 2f;

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "PrimordialChunkAttackSpeed").buffDef;

        public static ConfigEntry<float> atkSpeedBonus;
        private ConfigEntry<int> maxBuffStacks;
        private ConfigEntry<float> buffDuration;

        public override void SetupHooks()
        {
            On.RoR2.HoldoutZoneController.Update += (orig, self) =>
            {
                orig(self);

                if(!self.isAnyoneCharging) { return; }

                ReadOnlyCollection<TeamComponent> playerTeam = TeamComponent.GetTeamMembers(TeamIndex.Player);

                // For each player..
                foreach (var player in playerTeam)
                {
                    // If a player is within the charging radius of the teleporter...
                    if (HoldoutZoneController.IsBodyInChargingRadius(self, self.transform.position, self.currentRadius * self.currentRadius, player.body))
                    {
                        // Give them the item's buff if they don't have it.
                        var itemCount = player.body.inventory.GetItemCount(itemDef);

                        if (!player.body.HasBuff(ItemBuffDef))
                        {
                            player.body.AddTimedBuff(ItemBuffDef, buffDuration.Value, itemCount * maxBuffStacks.Value);
                        }

                        // If the player has the buff already and the last (set) of buffs have a remaining timer of (timer - 1) seconds...
                        // refresh the timers of all previous stacks of the buff, and add another stack afterwards.
                        else if (player.body.timedBuffs.Find(x => x.buffIndex == ItemBuffDef.buffIndex).timer <= buffDuration.Value - 1f)
                        {
                            var buffStacks = player.body.timedBuffs.Where(x => x.buffIndex == ItemBuffDef.buffIndex);

                            foreach (var buff in buffStacks)
                            {
                                buff.timer = buffDuration.Value;
                            }

                            player.body.AddTimedBuff(ItemBuffDef, buffDuration.Value, itemCount * maxBuffStacks.Value);
                        }
                    }
                }
            };
        }

        public override void AddConfigOptions()
        {
            atkSpeedBonus = configFile.Bind("Primordial_Chunk Config", "atkSpeedBonus", 0.09f, "The attack speed bonus per buff stack. (0.09 = +9%)");
            maxBuffStacks = configFile.Bind("Primordial_Chunk Config", "maxBuffStacks", 3, "The maximum number of buff stacks per item stack.");
            buffDuration = configFile.Bind("Primordial_Chunk Config", "buffDuration", 5f, "The duration of the buff in seconds.");
        }
    }
}