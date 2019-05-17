using System.Collections.Generic;
using System.Linq;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using ItemMechanic;
using UnitsMechanic.Groups.SpecialGroups;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public abstract class Group
    {
        private Vector3Int _lairPos = Vector3Int.zero;


        public virtual bool IsTimeForTree(GameUnit unit)
        {
            return IsSetUpedLairPos(unit);
        }

        public virtual bool IsTimeForAngry(GameUnit unit)
        {
            return false;
        }

        public virtual bool IsTimeForEvoCrossing(GameUnit unit)
        {
            var groupEnts = CreatureGroupManager.GetAllInGroup(unit.Group);
            foreach (var entInGroup in groupEnts)
            {
                if (unit == entInGroup) continue;
                if (UnitEvolution.IsCanBeStacked(unit.OriginalName, entInGroup.OriginalName)) return true;
            }
            return false;
        }

        public virtual bool IsTimeForReproduction(GameUnit unit)
        {
            var lairPos = ChunkUtil.GetUpper(unit.GroupObj.GetLairPos());
            return IsSetUpedLairPos(unit)
                   && !SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, lairPos)
                   && SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, lairPos).OriginalName == "building_lair";
        }

        public virtual bool IsSetUpedLairPos(GameUnit unit)
        {
            return unit.GroupObj.GetLairPos() != Vector3Int.zero;
        }

        public virtual void SetUpLairPos(Vector3Int pos)
        {
            _lairPos = pos;
        }

        public virtual Vector3Int GetLairPos()
        {
            return _lairPos;
        }

        public virtual void OnDeath(GameUnit ent, AbstractGameObject from)
        {
        }

        public virtual bool IsTimeForPickupItem(GameUnit unit)
        {
            if (SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, unit.CurrentPos)) return false;
            var item = SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, unit.CurrentPos);

            return item != null
                   && item.CanBePickuped
                   && !item.SoloEvolution
                   && item.Group == "item"
                   && unit.pickUped == null
                   && TreeGroup.IsPickableTreeLog(item)
                   && unit.GroupObj.GetLairPos() != ChunkUtil.GetDovvner(unit.CurrentPos);
        }

        public virtual bool IsTimeForDeliverItem(GameUnit unit)
        {
            var lairPos = ChunkUtil.GetUpper(unit.GroupObj.GetLairPos());

            if (unit.pickUped == null || !unit.GroupObj.IsSetUpedLairPos(unit) ||
                (ChunkUtil.IsAnyEntity(unit.ChunkNumber, lairPos) && unit.CurrentPos != lairPos)) return false;
            if (!SecondaryGroundLvL.IsEmptyPos(unit.ChunkNumber, lairPos))
            {
                return ItemGroup.IsCanBeStacked(unit.pickUped.OriginalName,
                    SecondaryGroundLvL.GetGroundEnt(unit.ChunkNumber, lairPos).OriginalName);
            }
            return true;
        }
    }
}