using System.Collections.Generic;
using System.Linq;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.PauseMenu;
using Json;
using PowerUI;
using UnityEngine;

namespace GUI_Game.InGame.UnitBar
{
    public class UnitBar_HTML : MonoBehaviour
    {
        public TextAsset UnitBarHTML;

        public static Dictionary<GameEntity, GameObject> pointList = new Dictionary<GameEntity, GameObject>();
        public static Dictionary<GameEntity, WorldUI> UIDict = new Dictionary<GameEntity, WorldUI>();

        private const float value = 170f;

        private const float smallValue = 100f;


        public const int BarHeight = 400, BarWeight = 600;

        private static TextAsset StaticUnitBarHTML;

        void Start()
        {
            StaticUnitBarHTML = UnitBarHTML;
        }

        void Update()
        {
            if (PauseMenu_HTML.IsPaused) return;

            if (ProgressUnitBar.ProgressBars.Count == 0) return;
            var BarsKV = ProgressUnitBar.ProgressBars.ToArray();
            foreach (var KV in BarsKV)
            {
                var ent = KV.Key;
                var bar = KV.Value;

                WorldUI ui;
                if (!UIDict.ContainsKey(ent))
                {
                    ui = SetupUnitBar(ent);
                    SetEnable(ui, false);
                }
                else
                {
                    ui = UIDict[ent];
                }

                if (bar.name == ProgressUnitBar.ProgressName.None)
                {
                    ui.document.Run("SetProgressBarEnable", false);

                    continue;
                }
                var progress = bar.time / bar.needTime * 100f;
                if (progress >= 99f) progress = 100f;

                bar.time += Time.deltaTime;

                //var specName = ProgressUnitBar.GetTextForProgressName(bar.name);
                //var barText = LanguageTextManager.GetTextByName(specName);


                ui.document.Run("UpdateProgressBar", progress);
            }
        }


        private static void SetEnable(WorldUI ui, bool enable)
        {
            if (ui != null)
                ui.document.Run("SetUnitBarEnable", enable);
        }


        public static Vector3 GetUnderPoint(GameEntity ent)
        {
            var objPos = ent.transform.position;
            var rend = ent.GetComponent<Collider2D>();
            return ClickManager.GetPosForChooser(objPos, rend.bounds.size, ent.transform.localScale);
        }

        public static GameObject SetupPoint(GameEntity ent)
        {
            if (pointList.ContainsKey(ent))
                return pointList[ent];
            var objPos = ent.transform.position;
            var rend = ent.GetComponent<Collider2D>();
            var posR = ClickManager.GetPosForChooser(objPos, rend.bounds.size, ent.transform.localScale);

            var pos = new Vector3(objPos.x,
                posR.y + BarHeight / value
                , objPos.z * 10);

            var point = new GameObject("Point");

            point.transform.position = pos;

            point.transform.SetParent(ent.transform);


            pointList[ent] = point;
            return point;
        }

        public static WorldUI SetupUnitBar(GameEntity ent)
        {
            WorldUI unitBar;
            if (UIDict.ContainsKey(ent) && UIDict[ent].gameObject != null)
            {
                unitBar = UIDict[ent];
                SetEnable(unitBar, true);
                if (unitBar.Expires)
                    unitBar.CancelExpiry();
                return unitBar;
            }

            //UI 
            unitBar = new WorldUI(BarWeight, BarHeight);
            UIDict[ent] = unitBar;

            //Return or Create
            var point = SetupPoint(ent);

            //Attach to point and setup HTML
            unitBar.ParentToOrigin(point.transform);
            unitBar.document.innerHTML = StaticUnitBarHTML.text;

            //Unit special info
            var name = LanguageTextManager.GetTextByName(ent.OriginalName);
            unitBar.document.Run("SetupUnitName", name);

            var unit = ent as AbstractGameObject;
            if (unit == null) return unitBar;
            if (!ClickManager.IsChoosed(unit.gameObject))
                unitBar.SetExpiry(5f);
            UpdateInfo(unit);
            return unitBar;
        }


        public static bool IsUnitBarOpened(GameEntity ent)
        {
            return UIDict.ContainsKey(ent);
        }

        public static void UpdateInfo(GameEntity ent)
        {
            if (ent == null) return;
            WorldUI ui = null;
            if (UIDict.ContainsKey(ent))
                ui = UIDict[ent];

            if (ui == null || ui.gameObject == null)
                ui = SetupUnitBar(ent);


            var unit = ent as AbstractGameObject;
            if (unit == null) return;

            //Debug.Log("UpdateInfo: " +unit+" HP:"+unit.UpgradedStats.Hp);

            //UnitBar
            ui.document.Run("UpdateHearts", unit.GetCurrentHp(), unit.GetMaxHp());
            //Main ui
            if (ClickManager.GetChoosedEnt() != ent) return;
            UI.document.Run("UpdateHP_count", unit.GetCurrentHp());
            UI.document.Run("UpdateHP_max", unit.GetMaxHp());
        }

        //Destroy point and UI
        public static void ClearUnitBar(GameEntity ent)
        {
            if (!UIDict.ContainsKey(ent)) return;
            SetEnable(UIDict[ent], false);
        }
    }
}