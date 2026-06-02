using LookingGlass.ItemStatsNameSpace;
using RigsArsenal.Items;
using RigsArsenal.Items.VoidItems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RigsArsenal
{
    /// <summary>
    /// Class that initialises the Looking Glass stats display if that mod is present and enabled in the current modlist.
    /// </summary>
    internal class LookingGlassItemStats
    {
        public static bool enabled
        {
            get => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("droppod.lookingglass");
        }

        //   https://github.com/Wet-Boys/LookingGlass/blob/main/LookingGlass/ItemStats/ItemDefinitions.cs <-- this

        private static void RegisterItemStat<T>(string nameToken, Action<T, ItemStatsDef> setupItemStats) where T : Item
        {
            var item = RigsArsenal.ItemList.FirstOrDefault(i => i.NameToken == nameToken) as T;
            if (item == null) return;

            ItemStatsDef itemStats = new ItemStatsDef();
            setupItemStats(item, itemStats);
            ItemDefinitions.RegisterItemStatsDef(itemStats, item.itemDef.itemIndex);
        }

        public static void SetupStatsDisplays()
        {
            // Common

            RegisterItemStat<Items.WornOutStimpack>("WORNOUTSTIMPACK", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Regen Bonus: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Healing);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.FlatHealing);

                itemStats.descriptions.Add("Movement Speed Bonus: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(item.regenBonus.Value * stackCount);

                    values.Add(item.movementBonus.Value * stackCount);
                    return values;
                };
            });

            RegisterItemStat<Items.KineticBattery>("KINETICBATTERY", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Barrier: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Armor);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(item.barrierAmount.Value * stackCount);

                    return values;
                };
            });

            RegisterItemStat<Items.PrimordialChunk>("PRIMORDIALCHUNK", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Max Attack Speed Bonus: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(item.maxBuffStacks.Value * PrimordialChunk.atkSpeedBonus.Value * stackCount);

                    return values;
                };
            });


            // Uncommon

            RegisterItemStat<Items.BountyHunterBadge>("BOUNTYHUNTERBADGE", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Bonus Gold: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Gold);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    float fractionalBit = 1 / (1 + stackCount * item.goldIncrease);
                    float increasedGold = (1 - fractionalBit) * item.multiplier.Value;

                    values.Add(increasedGold);

                    return values;
                };
            });

            RegisterItemStat<Items.CoolantPack>("COOLANTPACK", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Damage Reduction: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Armor);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    float fractionalBit = 1 - (1 / (1 + stackCount * CoolantPack.damageReduction.Value));
                    float damageResist = fractionalBit;

                    values.Add(damageResist);

                    return values;
                };
            });

            RegisterItemStat<Items.NeedleRounds>("NEEDLEROUNDS", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Critical Chance Bonus: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.descriptions.Add("Critical Damage Bonus: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                itemStats.calculateValuesNew = (luck, stackCount, procChance) =>
                {
                    List<float> values = new();
                    values.Add(ProcChanceWithLuck(item.critChanceGain.Value * stackCount * 0.01f, Mathf.CeilToInt(luck)));
                    values.Add(item.critDamageGain.Value * stackCount);

                    return values;
                };
            });

            RegisterItemStat<Items.ReactiveArmourPlating>("REACTIVEARMOURPLATING", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Armour Gain When Hit: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Armor);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(ReactiveArmourPlating.armourPerStack.Value * stackCount);

                    return values;
                };
            });

            RegisterItemStat<Items.UnderBarrelShotgun>("UNDERBARRELSHOTGUN", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Pellet Damage Multiplier: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

                itemStats.descriptions.Add("Pellet Proc Chance: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);
                itemStats.calculateValuesNew = (luck, stackCount, procChance) =>
                {
                    List<float> values = new();
                    values.Add(item.projectileDamage.Value * stackCount);
                    values.Add(ProcChanceWithLuck(item.itemProcChance.Value * 0.01f * stackCount, Mathf.CeilToInt(luck)));


                    return values;
                };
            });


            // Legendary

            RegisterItemStat<Items.ChaosRune>("CHAOSRUNE", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Proc Attempts: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

                itemStats.descriptions.Add("Base Average Procs Per Attack: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);
                itemStats.calculateValuesNew = (luck, stackCount, procChance) =>
                {
                    List<float> values = new();
                    values.Add(item.rollsPerStack.Value * stackCount);
                    values.Add(ProcChanceWithLuck(item.rollsPerStack.Value * stackCount * item.procChance.Value * 0.01f, Mathf.CeilToInt(luck)));

                    return values;
                };
            });


            // Lunar

            RegisterItemStat<Items.DissonantEdge>("DISSONANTEDGE", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Damage Increase: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(DissonantEdge.damageIncrease.Value * stackCount);

                    return values;
                };
            });


            // Void

            RegisterItemStat<Items.VoidItems.UmbralPyre>("UMBRALPYRE", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Total Range: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

                itemStats.descriptions.Add("Total Burn Damage: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Damage);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Percentage);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(UmbralPyre.baseRange.Value + (UmbralPyre.rangePerStack.Value * stackCount));
                    values.Add(UmbralPyre.burnDamage.Value * stackCount);

                    return values;
                };
            });

            RegisterItemStat<Items.VoidItems.RazorLeeches>("RAZORLEECHES", (item, itemStats) =>
            {
                itemStats.descriptions.Add("Duration: ");
                itemStats.valueTypes.Add(ItemStatsDef.ValueType.Utility);
                itemStats.measurementUnits.Add(ItemStatsDef.MeasurementUnits.Number);

                itemStats.calculateValuesFlat = (stackCount) =>
                {
                    List<float> values = new();
                    values.Add(item.baseDuration.Value + (item.baseDurationPerStack.Value * stackCount));

                    return values;
                };
            });
        }

        private static float ProcChanceWithLuck(float baseChance, int luck)
        {
            float chance = baseChance;

            if (luck > 0)
                chance = Mathf.Floor(baseChance) + (1f - Mathf.Pow(1 - baseChance, luck + 1f));

            if (luck < 0)
                chance = Mathf.Floor(baseChance) + (Mathf.Pow(baseChance, Mathf.Abs(luck) + 1f));

            return chance;
        }
    }
}