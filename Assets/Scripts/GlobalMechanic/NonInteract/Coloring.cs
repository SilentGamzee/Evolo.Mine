using System.Linq;
using FOW.Script;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using UnitsMechanic;
using UnitsMechanic.Groups.SpecialGroups;
using UnityEngine;

namespace GlobalMechanic.NonInteract
{
    public class Coloring
    {
        public static void RecolorObject(Vector3Int pos)
        {
            RecolorObject(pos, false);
        }

        public static void RecolorObject(Vector3Int pos, bool trueCall)
        {
            var ent = ChunkManager.CurrentChunk.GetGameObjectByIndex(pos);
            RecolorObject(ent, trueCall);
        }

        public static void RecolorObject(GameEntity ent, bool trueCall)
        {
            if (ent == null || ((Recolor.IsHided(ent) && !trueCall) && ChunkManager.staticFogEnabled)) return;


            var mod = 3f;
            var upper = ChunkUtil.GetUpper(ent.CurrentPos);
            if ((ChunkUtil.IsAnyEntity(ent.ChunkNumber, upper) ||
                 !SecondaryGroundLvL.IsEmptyPos(ent.ChunkNumber, upper))
                && ent.CurrentPos.z + 1 != ChunkManager.MaxGroundsLvls)
                mod += 2.5f;


            var i = 1f / ((ent.CurrentPos.z + mod) * 0.3f);
            var color = new Color(i, i, i);

            if (ChunkUtil.IsEntity(ent.ChunkNumber, upper) &&
                ent.CurrentPos.z + 1 != ChunkManager.MaxGroundsLvls)
            {
                var chunk = ChunkManager.GetChunkByNum(ent.ChunkNumber);
                var upperEnt = chunk.GetGameObjectByIndex(upper);
                if (upperEnt != null)
                {
                    if (TreeGroup.IsTreeGroup(upperEnt.Group))
                    {
                        color.r = 0.5f;
                        color.b = 0.7f;
                        color.g = 0.3f;
                    }
                    else if (upperEnt.Group == "rock")
                    {
                        color.r = 0.3f;
                        color.b = 0.7f;
                        color.g = 0.2f;
                    }
                    else if (upperEnt is GameUnit)
                    {
                        var unit = upperEnt as GameUnit;
                        if (unit.IsEnemy(PlayersManager.GetMyPlayer()))
                        {
                            color.r = 1;
                            color.b = 0.3f;
                            color.g = 0.3f;
                        }
                        else
                        {
                            color.r = 0.1f;
                            color.b = 0.1f;
                            color.g = 0.6f;
                        }
                    }
                    else
                    {
                        color.r = 0.1f;
                        color.b = 0.1f;
                        color.g = 0.6f;
                    }
                }
            }

            if (!GroupUtil.isCreatureGroup(ent.Group) &&
                !GroupUtil.isBuilding(ent.Group))
            {
                var spriteRenderer = ent.GetComponent<SpriteRenderer>();
                spriteRenderer.color = color;
            }
        }

        public static void RecolorAll()
        {
            var chunk = ChunkManager.CurrentChunk;
            for (var z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (var x = 0; x < chunk.MapSize; x++)
                {
                    for (var y = 0; y < chunk.MapSize; y++)
                    {
                        var ent = chunk.GetGameObjectByIndex(new Vector3Int(x, y, z));
                        RecolorObject(ent, false);
                    }
                }
            }
        }
    }
}