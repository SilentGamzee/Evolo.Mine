using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.NonInteract;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace FOW.Script
{
    public class Recolor : MonoBehaviour
    {
        public Color nonVisionColor;
        public Color semiVisionColor;

        private static Color static_nonVisionColor;
        private static Color static_semiVisionColor;

        void Awake()
        {
            static_nonVisionColor = nonVisionColor;
            static_semiVisionColor = semiVisionColor;
        }

        public static bool IsHided(GameEntity ent)
        {
            var targets = FieldOfView.visibleTargets.ToList();
            return !targets.Contains(ent.transform);
        }

        public static bool IsVisionBefore(GameEntity ent)
        {
            var targets = FieldOfView.visibleBefore.ToList();
            return targets.Contains(ent.transform);
        }

        public static bool IsIgnored(GameEntity ent)
        {
            return GroupUtil.IsNeutral(ent.Group);
        }

        static void HidingColor(GameEntity ent, GameEntity eUP)
        {
            var check = false;
            if (eUP == null)
            {
                var friends =
                    AI_Calculation.GetGoodNeighbors
                        (ChunkManager.CurrentChunk.ChunkNumber, ent.CurrentPos);

                foreach (var friend in friends)
                {
                    var fUP = ChunkManager.CurrentChunk.GetGameObjectByIndex
                        (ChunkUtil.GetUpper(friend));
                    if (fUP != null && !IsHided(fUP))
                        check = true;
                }
            }
            else
            {
                check = !IsHided(eUP);
            }
            if (!check)
            {
                var spD = ent.GetComponent<SpriteRenderer>();
                spD.color = static_nonVisionColor;
            }
            else
            {
                Coloring.RecolorObject(ent, true);

                Coloring.RecolorObject(ChunkUtil.GetUpper(ent.CurrentPos), true);

                Coloring.RecolorObject(ChunkUtil.GetDovvner(ent.CurrentPos), true);
            }
        }

        static void SemiColor(GameEntity ent, GameEntity eUP)
        {
            if (IsHided(eUP))
            {
                var spUp = eUP.GetComponent<SpriteRenderer>();
                spUp.color = static_semiVisionColor;

                var sp = ent.GetComponent<SpriteRenderer>();
                sp.color = static_semiVisionColor;
            }
            else
            {
                Coloring.RecolorObject(ent, true);
                Coloring.RecolorObject(eUP, true);
            }
        }

        static void OnGroundEntity(GameEntity ent)
        {
            var eUP = ChunkManager.CurrentChunk.GetGameObjectByIndex
                (ChunkUtil.GetUpper(ent.CurrentPos));
            if (eUP != null && IsIgnored(eUP) && IsVisionBefore(eUP))
                SemiColor(ent, eUP);
            else
                HidingColor(ent, eUP);
        }

        public static void OnFogChange()
        {
            if (!ChunkManager.staticFogEnabled || ChunkManager.CurrentChunk == null) return;
            var ents = ChunkManager.CurrentChunk.Ground;

            for (int x = 0; x < ents[0].Length; x++)
            {
                for (int y = 0; y < ents[0][0].Length; y++)
                {
                    var ent = ents[0][x][y];
                    var item = SecondaryGroundLvL.GetGroundEnt(ChunkManager.CurrentChunk.ChunkNumber,
                        new Vector3Int(x, y, 1));
                    if (item != null)
                    {
                        Coloring.RecolorObject(item, true);
                        Coloring.RecolorObject(new Vector3Int(x, y, 0), true);
                    }
                    else if (ent != null)
                        OnGroundEntity(ent);
                }
            }
        }
    }
}