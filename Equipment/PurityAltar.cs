using RigsArsenal.Buffs;
using RigsArsenal.Items;
using R2API;
using Rewired.ComponentControls.Data;
using Rewired.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RigsArsenal.RigsArsenal;
using static RoR2.MasterSpawnSlotController;
using BepInEx.Configuration;

namespace RigsArsenal.Equipments
{
    public class PurityAltar : Equipment
    {
        public override string Name => "Altar of Purity";

        public override string NameToken => "PURITYALTAR";

        public override string PickupToken => "Restore a random broken or depleted item.<style=cIsHealth> Sacrifices a random item per restoration.</style>";

        public override string Description => "Restore a <style=cIsUtility>single random</style> consumed item or <style=cLunarObjective>cure a single tonic affliction.</style> <style=cIsHealth>A common item is sacrificed per restoration.</style> <style=cDeath>A legendary item is sacrificed for legendary-tier items.</style>";
        public override string Lore => "<style=cLunarObjective>Peer into the abyss. Be tempted. Curiosity will take hold. Allow it.\n\nGaze into the abyss. It offers great power. To turn back time. To undue mistakes.\n\nStare into the abyss. For with each glance, I grow in power. And you grow weaker.\n\nI am coming for you. With your stolen strength.</style>";

        public override bool isLunar => true;

        public override float cooldown => equipCooldown.Value;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("PurityAltar.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("PurityAltar.prefab");

        public override float minViewport => 3f;
        public override float maxViewport => 5f;

        private ConfigEntry<int> equipCooldown;
        private ConfigEntry<bool> angryMithrix;


        private List<Tuple<ItemDef, ItemDef>> validItems;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            foreach (Tuple<ItemDef, ItemDef> item in validItems)
            {
                // If the player has a valid item that can be restored or cleansed...
                if (slot.characterBody.inventory.GetItemCount(item.Item1) > 0)
                {
                    // Dios Best Fried or its void counterpart: Tier 3 (Legendary) sacrifice. Otherwise, Tier 1 (Common).
                    ItemTier sacrificeTier;

                    if (item.Item1.itemIndex == ItemCatalog.FindItemIndex("ExtraLifeConsumed") ||
                        item.Item1.itemIndex == ItemCatalog.FindItemIndex("ExtraVoidLifeConsumed"))
                    {
                        sacrificeTier = ItemTier.Tier3;
                    }
                    else
                    {
                        sacrificeTier = ItemTier.Tier1;
                    }

                    // Sacrifice a random item with the chosen rarity.
                    // If this fails (No item was valid for sacrifice for the current consumed item), try the next consumed item.
                    if (!DestroyRandomItem(sacrificeTier, slot.characterBody)) { continue; }

                    // Remove the consumed item.
                    slot.characterBody.inventory.RemoveItem(item.Item1, 1);

                    // (Don't give a restored item if a tonic affliction stack was removed as it has no item counterpart).
                    if(item.Item1.itemIndex == ItemCatalog.FindItemIndex("TonicAffliction"))
                    {
                        return true;
                    }

                    // Give the player the restored item.
                    slot.characterBody.inventory.GiveItem(item.Item2.itemIndex, 1);

                    // Broadcast to the player the item they restored, with the message being the opposite of the broken item message
                    // (Broken item icon transformed into normal item icon counterpart).
                    CharacterMasterNotificationQueue.SendTransformNotification(slot.characterBody.master,
                    item.Item1.itemIndex,
                    item.Item2.itemIndex,
                    CharacterMasterNotificationQueue.TransformationType.Default);
                    return true;
                }
            }   

            return false;
        }

        private bool DestroyRandomItem(ItemTier sacrificeTier, CharacterBody self)
        {
            // Also records the void counterpart of the chosen rarity, as those can also be sacrificed.
            var voidSacrificeTier = (sacrificeTier == ItemTier.Tier1) ? ItemTier.VoidTier1 : ItemTier.VoidTier3;

            // Return false if there are no items available to sacrifice for the chosen rarity.
            if(self.inventory.GetTotalItemCountOfTier(sacrificeTier) <= 0 && self.inventory.GetTotalItemCountOfTier(voidSacrificeTier) <= 0)
            {
                return false;
            }

            // Get all items in the player's inventory.
            List<ItemIndex> ValidItems = new List<ItemIndex>();

            foreach(ItemIndex item in self.inventory.itemAcquisitionOrder)
            {
                // Get the item's tier.
                ItemTier tier = ItemCatalog.GetItemDef(item).tier;
                if( tier == sacrificeTier || tier == voidSacrificeTier)
                {
                    ValidItems.Add(item);
                }
            }

            // Destroy a single valid item randomly.
            var deadItem = ValidItems[UnityEngine.Random.Range(0, ValidItems.Count)];
            self.inventory.RemoveItem(deadItem, 1);

            // Modifying the item conversion notification, indicate to the player what item was destroyed, with additional flavour text.
            var consumeItem = ItemList.Find(x => x.NameToken == "PURITYALTARCONSUME").itemDef;

            // Pass in the tier for bonus flavour text for legendary items.
            var mithrixResponse = SetPurityAltarConsumeFlavourText(ItemCatalog.GetItemDef(deadItem).tier);

            consumeItem.pickupToken = mithrixResponse;

            // For UI mods like looking glass, they can replace the short description (pickuptoken) with the long description
            // (descriptiontoken) so this ensures that no matter what settings are used, the same flavour text is displayed.
            consumeItem.descriptionToken = mithrixResponse;

            CharacterMasterNotificationQueue.SendTransformNotification(self.master,
            deadItem, 
            consumeItem.itemIndex,
            CharacterMasterNotificationQueue.TransformationType.Default);

            return true;
        }

        public override void SetupHooks()
        {
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);

                // Item 1: Consumed Item. Item 2: Item to restore (Before consumption).
                validItems = new List<Tuple<ItemDef, ItemDef>>
                {
                    new Tuple<ItemDef, ItemDef>(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExtraLifeConsumed")), ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExtraLife"))), // Dio's Best Friend.
                    new Tuple<ItemDef, ItemDef>(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExtraLifeVoidConsumed")), ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ExtraLifeVoid"))), // Pluripotent Larva.
                    new Tuple<ItemDef, ItemDef>(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("FragileDamageBonusConsumed")), ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("FragileDamageBonus"))), // Delicate Watch.
                    new Tuple<ItemDef, ItemDef>(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("HealingPotionConsumed")), ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("HealingPotion"))), // Power Elixir.
                    new Tuple<ItemDef, ItemDef>(ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("TonicAffliction")), null) // Spinel Tonic. (Has no item counterpart).
                };
            };
        }

        public override void AddConfigOptions()
        {
            equipCooldown = configFile.Bind("Altar_Of_Purity Config", "equipCooldown", 45, "Cooldown for this equipment.");
            angryMithrix = configFile.Bind("Altar_Of_Purity Config", "angryMithrix", false, "When enabled, Mithrix will use more non-vulgar aggressive and sarcastic taunts when sacrificing items. Disable for more generic sacrifice dialogue.");
        }

        private string SetPurityAltarConsumeFlavourText(ItemTier tier)
        {
            string[] MithrixResponses;

            // Angry mithrix is true: Use more aggressive/sarcastic taunts as well as a couple game references.
            if (angryMithrix.Value)
            {
                // If the sacrificed item was a legendary, instead use a more tailored set of responses to really salt the wound.
                if (tier == ItemTier.Tier3 || tier == ItemTier.VoidTier3)
                {
                    MithrixResponses = new string[]
                    {
                        "<style=cIsUtility>You call that a sacrifice? I call it a tribute.</style>",
                        "<style=cIsUtility>An offering worthy of a king.</style>",
                        "<style=cIsUtility>What a waste of a legendary item.</style>",
                        "<style=cIsUtility>One does not pawn divinity and escape mockery.</style>"
                    };
                }
                else
                {
                    MithrixResponses = new string[]
                    {
                    "<style=cLunarObjective>Oops... was that one important?</style>",
                    "<style=cLunarObjective>One step forward, ten steps back.</style>",
                    "<style=cLunarObjective>Two words my friend: No refunds!</style>",
                    "<style=cLunarObjective>I'll always take items you don't need.</style>",
                    "<style=cDeath>I see you..!</style>",
                    "<style=cLunarObjective>Come for a fight? Oh, should have dressed for a funeral!</style>",
                    "<style=cLunarObjective>Did you really think I would let you keep that?</style>",
                    "<style=cLunarObjective>What are you without your baubles and trinkets?</style>"
                    };
                }
            }

            // Angry Mithrix is false: Use more neutral taunts.
            else
            {
                MithrixResponses = new string[]
                {
                "<style=cLunarObjective>You only grow my power.</style>",
                "<style=cLunarObjective>I draw closer.</style>",
                "<style=cLunarObjective>My strength grows.</style>",
                "<style=cDeath>Behind you.</style>",
                "<style=cLunarObjective>You will perish.</style>",
                "<style=cLunarObjective>I am your end.</style>", // DHUUMS GAZE FALLS ON ME
                "<style=cLunarObjective>Surrender in body and spirit.</style>",
                "<style=cLunarObjective>Yes, stare into the altar... let it take you.</style>"
                };
            }

            var result = MithrixResponses[UnityEngine.Random.Range(0, MithrixResponses.Length)];
            return result;
        }   
    }
}