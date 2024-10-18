/*using System.Collections;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Defective Assembly Arm- T2 (Uncommon) Item
    /// <para>At the start of each stage, fabricate a random basic drone that expires after progressing a certain amount of stages.</para>
    /// <para>There is a chance for the assembly arm to instead fabricate a random powerful drone instead of a basic drone.</para>
    /// <para>Enemies are unable to get this item.</para>
    /// </summary>
    public class DefectiveAssemblyArm : Item
    {
        public override string Name => "Defective Assembly Arm";
        public override string NameToken => "DEFECTIVEASSEMBLYARM";
        public override string PickupToken => "Fabricates a random drone with a limited lifetime at the start of each stage.";
        public override string Description => "<style=cIsUtility>Fabricate</style> a random drone at the start of each stage. <style=cIsUtility>Fabricated</style> drones expire after completing <style=cIsDamage>3</style><style=cStack> (+1 per stack)</style><style=cIsDamage> stages</style>. There is a <style=cIsDamage>20%</style><style=cStack> (+5% per stack)</style> chance to instead fabricate a <style=cIsUtility>powerful</style> drone.";
        public override string Lore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => null;
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("DefectiveAssemblyArm.prefab");

        // Drone1 - Gunner drone.
        // Drone2 - Healing drone.
        // EmergencyDrone - Emergency drone.
        // FlameDrone - Incinerator drone.
        // DroneMissile - Missile drone.
        // This item cannot spawn drones from "The Backup" equipment, equipment drones, or "mega" (TC-280 Prototype) drones.

        // Item 1: Drone prefab name.
        // Item 2: Drone's name token.
        private System.Tuple<string, string>[] drones = new System.Tuple<string, string>[] 
        {
            new System.Tuple<string, string>("Drone1Master", "FABRICATEDDRONE1_NAME"),
            new System.Tuple<string, string>("Drone2Master", "FABRICATEDDRONE2_NAME"),
            new System.Tuple<string, string>("EmergencyDroneMaster", "FABRICATEDEMERGENCYDRONE_NAME"),
            new System.Tuple<string, string>("FlameDroneMaster", "FABRICATEDFLAMEDRONE_NAME"),
            new System.Tuple<string, string>("DroneMissileMaster", "FABRICATEDMISSILEDONE_NAME") 
        };


        public override void InitLang()
        {
            base.InitLang();

            // Add Fabricated Drone Name Tokens
            // (To easily differentiate between a normal drone and a fabricated drone by its card name).
            LanguageAPI.Add("FABRICATEDDRONE1_NAME", "Fabricated Gunner Drone");
            LanguageAPI.Add("FABRICATEDDRONE2_NAME", "Fabricated Healing Drone");
            LanguageAPI.Add("FABRICATEDEMERGENCYDRONE_NAME", "Fabricated Emergency Drone");
            LanguageAPI.Add("FABRICATEDFLAMEDRONE_NAME", "Fabricated Incinerator Drone");
            LanguageAPI.Add("FABRICATEDMISSILEDONE_NAME", "Fabricated Missile Drone");
        }

        // todo: exhaustive testing.
        public override void SetupHooks()
        {
            On.RoR2.Run.AdvanceStage += (orig, self, stage) =>
            {
                orig(self, stage);
                
                if(!Run.instance) { return; }

                var players = PlayerCharacterMasterController.instances;

                // Checks for drone spawn conditions after a delay as the player bodies are not instantiated immediately, causing a null reference error.
                self.StartCoroutine(AddDrone(players));
            };

            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);

                var count = self.inventory.GetItemCount(itemDef);

                if(count <= 0 && self.master.GetComponent<DroneTimers>())
                {
                    self.master.GetComponent<DroneTimers>().DestroyAllDrones();
                    GameObject.Destroy(self.master.GetComponent<DroneTimers>());
                }
            };
        }

        // Fabricates drones for all players that have the item.
        IEnumerator AddDrone(System.Collections.ObjectModel.ReadOnlyCollection<PlayerCharacterMasterController> players)
        {
            // Short delay to allow the players to spawn into the next stage.
            yield return new WaitForSeconds(1f);

            foreach(PlayerCharacterMasterController player in players)
            {
                if (!player.master || !player.master.inventory || !player.body) { continue; }

                var count = player.master.inventory.GetItemCount(itemDef);

                if (count > 0)
                {
                    // Drone timers keep a reference of all the drones made by the Defective Assembly Arm, and their lifetimes.
                    // The component is only given if the player has the item.
                    if (!player.master.GetComponent<DroneTimers>())
                    {
                        player.master.gameObject.AddComponent<DroneTimers>();
                    }

                    var droneTimer = player.master.GetComponent<DroneTimers>();

                    // Saves the index of the drone to be spawned, so a reference to the drone prefab and its custom name token can be obtained later on.
                    int droneIndex;

                    var roll = Random.Range(0, 100) + ((count - 1) * 5); // Roll is randomised but minimum increases by 5 per item stack.
                                                                         // At 17 stacks, strong drones will always spawn. (Roll is always 80 or higher).

                    // Rolling under 80 spawns common drones (healing drone and gunner drone).
                    if (roll < 80)
                    {
                        droneIndex = Random.Range(0, 2);
                    }

                    // Rolling 80 or higher spawns strong drones (incinerator drone, missile drone and emergency drone).
                    else
                    {
                        droneIndex = Random.Range(2, drones.Length);
                    }


                    // Decrement all current Defective Assembly Arm drones' lifetimes by one stage.
                    droneTimer.DecreaseDroneLifeTimes();

                    // Instantiate the new drone and give it to the player with the item.
                    CharacterMaster summonedDrone = new MasterSummon()
                    {
                        masterPrefab = MasterCatalog.FindMasterPrefab(drones[droneIndex].Item1),
                        position = player.body.corePosition,
                        rotation = player.body.transform.rotation,
                        summonerBodyObject = player.body.gameObject,
                        ignoreTeamMemberLimit = false,
                        teamIndexOverride = player.master.teamIndex
                    }.Perform();

                    summonedDrone.GetBody().baseNameToken = drones[droneIndex].Item2;

                    // Component that overrides the drone's death behaviour to prevent it from dropping into the "broken" interactable state.
                    // Stops duplication of drones (Fabricated drone dies -> Wreckage is purchased -> Spawns as a normal, permament drone).
                    summonedDrone.gameObject.AddComponent<DontDropInteractableOnDeath>();

                    // Add the drone to the list with lifetime of 2 stages + 1 stage per item stack.
                    droneTimer.AddDrone(summonedDrone.minionOwnership, 2 + count, drones[droneIndex].Item2);
                }
            }
        }


        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("ReactiveArmourPlating.prefab");

            var itemDisplay = display.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(display);

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0.06313F, 0.22037F, 0.09908F),
                    localAngles = new Vector3(45.34884F, 106.977F, 165.8645F),
                    localScale = new Vector3(0F, 0F, 0F)
                }
            });

            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Base",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "ThighL",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
                }
            });

            return rules;
        }
    }

    // Handles drone lifetimes and destroys them when their time is up.
    public class DroneTimers : MonoBehaviour
    {
        public struct droneLifeTimes
        {
            public MinionOwnership drone { get; set; } // Reference to the drone.
            public int timeLeft { get; set; } // The number of stage transitions remaining until the drone is destroyed.

            public string nameToken { get; set; } // The drone's name token.
        }

        // List of active fabricated drones.
        private List<droneLifeTimes> drones = new List<droneLifeTimes>();

        // List of drones identified for removal during DecreaseDroneLifeTimes.
        private List<int> dronesToRemove = new List<int>();

        public void AddDrone(MinionOwnership drone, int time, string name)
        {
            drones.Add(new droneLifeTimes { drone = drone, timeLeft = time, nameToken = name });
        }

        // Decrease Defective Assembly Arm drones' lifetimes by one stage (Called on stage transition). If lifetime reaches zero, the drone is destroyed.
        public void DecreaseDroneLifeTimes()
        {
            if(drones.Count <= 0) { return; }

            dronesToRemove.Clear();

            for (int i = 0; i < drones.Count; i++)
            {
                // If the drone was already destroyed, remove it from the list.
                if (!drones[i].drone)
                {  
                    dronesToRemove.Add(i);
                }

                // Decrease each drone's lifetime by one stage.
                drones[i] = new droneLifeTimes { drone = drones[i].drone, timeLeft = drones[i].timeLeft - 1, nameToken = drones[i].nameToken };

                // Destroy the drone if its lifetime reaches zero.
                if (drones[i].timeLeft <= 0)
                {
                    StartCoroutine(RemoveExpiredDroneDelayed(drones[i]));
                    dronesToRemove.Add(i);
                }

                // Update the drone's name token again (As this resets after entering a new stage).
                drones[i].drone.GetComponent<CharacterMaster>().GetBody().baseNameToken = drones[i].nameToken;
            }

            // Remove drones from the list after the for loop above is completed so the list is not modified during iteration.
            if (dronesToRemove.Count > 0)
            {
                for (int i = 0; i < dronesToRemove.Count; i++)
                {
                    drones.RemoveAt(dronesToRemove[i]);
                }
            }
        }

        public int GetDroneCount()
        {
            return drones.Count;
        }

        // Upon losing the Defective Assembly Arm item, the drones are lost with it.
        public void DestroyAllDrones()
        {
            for (int i = 0; i < drones.Count; i++)
            {
                drones[i].drone.GetComponent<CharacterMaster>().TrueKill();
            }
            drones.Clear();

        }

        // Destroys the drone after a 1 second delay. The delay is included as it allows the player to see which drone has been destroyed, since
        // without the delay the destruction fully occurs before the player loads into the next stage.
        IEnumerator RemoveExpiredDroneDelayed(droneLifeTimes drone)
        {
            yield return new WaitForSeconds(2f);

            drone.drone.GetComponent<CharacterMaster>().TrueKill();
        }
    }

    // Gives fabricated drones this component as an indicator that they should not drop into the "broken" interactable state upon death to prevent
    // drone duplication (allowing a fabricated drone to spawn, be destroyed, then re-purchased as a permament drone).
    public class DontDropInteractableOnDeath() : MonoBehaviour
    {
        private void Start()
        {
            // Overrides the implementation of OnImpactServer for these drones, which handles placing the broken drone
            // interactable object.
            On.EntityStates.Drone.DeathState.OnImpactServer += (orig, self, contactPoint) =>
            {};
        }
    }
}
*/