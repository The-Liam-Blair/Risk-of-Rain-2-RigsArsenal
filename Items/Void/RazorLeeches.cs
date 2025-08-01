﻿using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using R2API;
using R2API.Utils;
using RoR2;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items.VoidItems
{
    /// <summary>
    /// Razor Leeches - Void T2 (Void Uncommon) Item
    /// <para>Critically strike enemies to perforate them.</para>
    /// <para>Perforated enemies receive additional damage over time proportional to the damage they receive.</para>
    /// <para>Certain types of damage, including damage over time damage and environmental damage, are exempt.</para>
    /// <para>This item corrupts all Needle Rounds items.</para>
    /// </summary>
    public class RazorLeeches : Item
    {
        public override string Name => "Razor Leeches";
        public override string NameToken => "RAZORLEECHES";
        public override string PickupToken => "Perforate foes by critically striking. Perforated enemies receive additional damage over time from attacks.<style=cIsVoid> Corrupts all Needle Rounds.";
        public override string Description => $"Critically striking an enemy<style=cIsDamage> perforates</style> them for <style=cIsUtility>{baseDuration.Value}</style> <style=cStack>(+{baseDurationPerStack.Value} per stack)</style> seconds. <style=cIsDamage>Perforated</style> enemies receive <style=cIsUtility>{damageScalar.Value * 100f}%</style> of incoming damage as<style=cIsDamage> additional damage over time.</style> Also gain <style=cIsDamage>+5% critical strike chance</style>. <style=cIsVoid> Corrupts all Needle Rounds</style>.";
        public override string Lore => "<style=cMono>// MEDIC BAY 04 - DR. CROSS EXAMINATION REPORT #65 //</style>\n\nIn this sector we have seen a sharp rise of unusual wounds, similar to that produced by a small hand held firearm. Unlike such wounds however, which often fly straight with a clear entry and often exit point, these new wounds never have an exit point. A bit unusual but not alarming in of itself.\n\nAdditionally, these 'bullets' seem to turn and manoeuvre around inside the body. Not randomly either, they seek out vital organs, deftly loop around bone and muscle. We have managed to find what we believe is the 'bullet', a small puddle of purple, sticky liquid. The composition does not include any metals; in fact it is entirely made of biological matter.\n\nBreakthrough! This is a fully organic creature. From running simulations we predict that it may be similar to a large worm or leech with little to no higher brain function. When it reaches the entry point it burrows in and consumes the flesh in its path. It may have been bred for this purpose given that it dies shortly after entry, either creatures with an extremely limited lifespan or when exposed to an environment like the human body. Given that the entry point is most commonly chest-height with incidents involving this 'projectile', something or someone must be accelerating it to reach that height. It may be using the environment or another creature is working in tandem with this leech. Until we have a live account of this creature perform a hunt, we can only speculate.\n\nThey're shooting them, the leeches. Using them like ammunition. God watch over us.\n\n<style=cMono>// END OF REPORT //</style>";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("RazorLeeches.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("RazorLeeches.prefab");

        public override float minViewport => 0.8f;
        public override float maxViewport => 2f;

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "RazorLeechWound").buffDef;

        public override ItemDef pureItemDef => ItemList.Find(x => x.NameToken == "NEEDLEROUNDS").itemDef; // Needle Rounds

        private ConfigEntry<int> baseDuration;
        private ConfigEntry<int> baseDurationPerStack;
        public static ConfigEntry<float> damageScalar;

        public override void SetupHooks()
        {
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var attackerObj = damageReport.attacker;
                if (!attackerObj || !attackerObj.GetComponent<CharacterBody>()) { return; }

                var attacker = attackerObj.GetComponent<CharacterBody>();
                if (!attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);

                // Apply perforate (wounded) debuff to the victim if the attack was a crit and the attacker has the item.
                if (count > 0 && damageReport.damageInfo.crit)
                {
                    var victim = damageReport.victimBody;
                    if (!victim) { return; }

                    // Base durations: 2 seconds + 1 second per stack.
                    victim.AddTimedBuff(ItemBuffDef, baseDuration.Value + (count * baseDurationPerStack.Value), 1);
                }
            };
                

            // Item gives +5% flat crit chance, much like harvester scythe and predatory instincts.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (!self || !self.inventory || !self.isPlayerControlled) { return; }

                if(self.inventory.GetItemCount(itemDef) > 0)
                {
                    args.critAdd += 5f;
                }
            };
        }

        public override void AddConfigOptions()
        {
            baseDuration = configFile.Bind("Razor_Leeches Config", "baseDuration", 2, "The base duration of the wound effect.");
            baseDurationPerStack = configFile.Bind("Razor_Leeches Config", "baseDurationPerStack", 1, "The duration increase of the wound effect per stack.");
            damageScalar = configFile.Bind("Razor_Leeches Config", "damageScalar", 0.2f, "The percentage of damage dealt that is applied as damage over time to the perforated enemy (0.2 = 20% of damage dealt).");
        }
    }
}