using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using UnityEngine;

namespace UnitsMechanic
{
    public class SpecialActions
    {
        public static GameItem SpawnItem(string itemName, Vector3Int pos, Player owner)
        {
            if (owner == null)
                owner = PlayersManager.Empty;
            GameEntity ent = null;
            var chunk = ChunkManager.CurrentChunk;
            if (ChunkUtil.IsAnyEntity(chunk.ChunkNumber, pos))
            {
                ent = chunk.GetGameObjectByIndex(pos);
            }
            
            if (!SecondaryGroundLvL.IsEmptyPos(chunk.ChunkNumber, pos))
                SecondaryGroundLvL.GetGroundEnt(chunk.ChunkNumber, pos).KillSelf();

            var item = chunk.SetupItem(itemName, pos, owner);
            SecondaryGroundLvL.SetGroundEnt(chunk.ChunkNumber, pos, item);

            chunk.SetIndex(pos, -1);
            chunk.SetObjectAtPos(pos, null);

            if (ent != null)
            {
                chunk.SetIndex(pos, ent.PrefabIndex);
                chunk.SetObjectAtPos(pos, ent);
            }

            return item;
        }
    }
}