using System.Collections.Generic;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.ManagersAndSystems.Quests;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.UnitBar;
using UnitsMechanic;
using UnitsMechanic.Groups.SpecialGroups;
using UnityEngine;

namespace ItemMechanic
{
    public class ItemMain
    {
        private const float NeedToPickupTime = 3f;

        public static void Update(GameUnit unit)
        {
            if (unit.State != EventManager.InProgressEvents.Stay)
            {
                ProgressUnitBar.RemoveProgressBar(unit);
                unit.StayOnBuilding = false;
                return;
            }


            //Dropping at ground
            if (unit.pickUped != null)
            {
                //if item on ground and not stacking return
                if (!SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, unit.CurrentPos))
                {
                    var gEntName = SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, unit.CurrentPos).OriginalName;
                    if (ItemGroup.GetStackResult(unit.pickUped.OriginalName, gEntName).Length == 0)
                    {
                        return;
                    }
                }


                //Item on ground
                if (!SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, unit.CurrentPos))
                {
                    var progressName = ProgressUnitBar.ProgressName.Stucking;

                    ProgressUnitBar.Setup(unit, progressName, NeedToPickupTime);
                    //Time to pickup
                    if (!ProgressUnitBar.IsReady(unit) ||
                        !ProgressUnitBar.IsThisProgressName(unit, progressName)) return;


                    var onGround = SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, unit.CurrentPos);
                    var stack = ItemGroup.GetStackResult(unit.pickUped.OriginalName, onGround.OriginalName);
                    if (stack.Length > 0)
                    {
                        var item = SpecialActions.SpawnItem(stack, unit.CurrentPos, unit.Owner);

                        ItemEvents.OnDropItem(unit.pickUped, unit);

                        unit.pickUped.KillSelf();
                        onGround.KillSelf();

                        unit.pickUped = null;
                        SecondaryGroundLvL.SetGroundEnt(unit.ChunkNumber, unit.CurrentPos, item);

                        QuestManager.OnItemStuck(item);

                        var chunk = ChunkManager.GetChunkByNum(unit.ChunkNumber);
                        chunk.SetIndex(unit.CurrentPos, unit.PrefabIndex);
                        chunk.SetObjectAtPos(unit.CurrentPos, unit);
                    }
                }
                //Ground empty
                else
                {
                    var progressName = ProgressUnitBar.ProgressName.Dropping;
                    ProgressUnitBar.Setup(unit, progressName, NeedToPickupTime);
                    //Time to pickup
                    if (!ProgressUnitBar.IsReady(unit) ||
                        !ProgressUnitBar.IsThisProgressName(unit, progressName)) return;

                    var prev3DPos = Util.Get3DPosByIndex(unit.pickUped.CurrentPos);


                    var pos = unit.pickUped.transform.position - prev3DPos;
                    pos.z = 0;

                    var d = unit.CurrentPos;
                    var get3DPosByIndex = Util.Get3DPosByIndex(d);
                    get3DPosByIndex.z -= d.z / 150f;


                    unit.pickUped.CurrentPos = unit.CurrentPos;
                    unit.pickUped.transform.position = get3DPosByIndex + pos;

                    unit.pickUped.transform.Translate(0, 0, 0.01f);

                    var rend = unit.pickUped.GetComponent<SpriteRenderer>();
                    rend.enabled = true;


                    SecondaryGroundLvL.SetGroundEnt(unit.ChunkNumber, unit.CurrentPos, unit.pickUped);

                    ItemEvents.OnDropItem(unit.pickUped, unit);

                    unit.pickUped = null;
                }

                ProgressUnitBar.RemoveProgressBar(unit);
            }
            //Pickuping from ground
            else if (
                unit.StayOnBuilding || !SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, unit.CurrentPos))
            {
                var progressName = ProgressUnitBar.ProgressName.Pickuping;
                var item = SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, unit.CurrentPos);
                var objGroup = item.Group;
                if (objGroup.ToLower() != "item") return;
                if (item.SoloEvolution) return;

                unit.StayOnBuilding = true;


                ProgressUnitBar.Setup(unit, progressName, NeedToPickupTime);

                if (!ProgressUnitBar.IsReady(unit) ||
                    !ProgressUnitBar.IsThisProgressName(unit, progressName)) return;

                unit.StayOnBuilding = false;
                ProgressUnitBar.RemoveProgressBar(unit, progressName);

                if (item.Destroyed) return;
                var rend = item.GetComponent<SpriteRenderer>();
                rend.enabled = false;

                unit.pickUped = item;
                ItemEvents.OnPickup(unit.pickUped, unit);

                SecondaryGroundLvL.RemovePos(unit.ChunkNumber, unit.CurrentPos);
            }
        }
    }
}