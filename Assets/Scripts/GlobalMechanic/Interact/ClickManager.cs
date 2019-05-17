using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.AbilityBar;
using GUI_Game.InGame.ErrorBar;
using GUI_Game.InGame.PauseMenu;
using GUI_Game.InGame.UnitBar;
using Menus;
using PowerUI;
using SpriteGlow;
using UnitsMechanic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Input = UnityEngine.Input;

namespace GlobalMechanic.Interact
{
    public class ClickManager : MonoBehaviour
    {
        public Material clickMat;
        private static bool _choosed;
        private static GameObject _choosedObj;
        private static Material static_chooseMat;

        private static Dictionary<GameObject, GameObject> chooseObjList = new Dictionary<GameObject, GameObject>();

        void Awake()
        {
            static_chooseMat = clickMat;
        }

        // Update is called once per frame
        void Update()
        {
            if (PauseMenu_HTML.IsPaused) return;
            if (Input.GetMouseButtonDown(0))
            {
                CastRay(0);
            }
            if (Input.GetMouseButtonDown(1))
            {
                CastRay(1);
            }
        }


        public static Vector3 GetPosForChooser(Vector3 objPos, Vector2 size, Vector3 scale)
        {
            var i = -0.5f;
            return new Vector3(objPos.x + (size.x * scale.x / 2) * i, objPos.y + (size.y * scale.y / 2),
                objPos.z - 3);
        }


        void CastRay(int key)
        {
            var p = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(p);

            var lay = -1;
            if (key == 1 && _choosed)
            {
                lay = LayerMask.GetMask("Background Image");
            }
            else
            {
                lay = LayerMask.GetMask("Background Image", "UnitBar");
            }


            //HUD CHECK
            var pDoc = InputPointer.AllRaw;
            foreach (var v in pDoc)
            {
                var element = UI.document.elementFromPoint(v.DocumentX, v.DocumentY);

                if (element == null
                    || (element.id != "MainBox"
                        && !element.id.Contains("List"))) return;
            }


            //USUAL RAYCAST
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, lay);

            if (hit.collider != null)
            {
                if (key == 0)
                {
                    LftClickedOnGameObject(hit.collider.gameObject);
                }
                else
                {
                    RghtClickedOnGameObject(hit.collider.gameObject);
                }
            }
            else
            {
                ClickedOnSpace();
            }
        }

        void LftClickedOnGameObject(GameObject obj)
        {
            var gameEntity = obj.GetComponent<AbstractGameObject>();

            if (gameEntity == null || (!FieldOfView.IsVisible(gameEntity) && ChunkManager.staticFogEnabled))
            {
                ClickedOnSpace();
                return;
            }


            if (_choosed)
            {
                var prevEnt = _choosedObj.GetComponent<GameEntity>();

                ClickedOnSpace();
                UnitBar_HTML.ClearUnitBar(prevEnt);
            }

            if (!GroupUtil.IsGround(gameEntity.Group))
            {
                ChooseUnit(gameEntity);
            }
            else
            {
                ClickedOnSpace();
            }
        }

        private static void AddChooser(GameEntity ent)
        {
            if (chooseObjList.ContainsKey(ent.gameObject)) return;

            var obj = new GameObject("ClickObj");
            obj.transform.localScale = ent.gameObject.transform.localScale;
            
            var sprite = ent.gameObject.GetComponent<SpriteRenderer>().sprite;
            var spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.material = static_chooseMat;
            obj.transform.SetParent(ent.gameObject.transform);
            obj.transform.localPosition = new Vector3(0, 0, -0.0001f);
            chooseObjList.Add(ent.gameObject, obj);
        }

        public static void ChooseUnit(AbstractGameObject obj)
        {
            AddChooser(obj);

            _choosedObj = obj.gameObject;
            _choosed = true;

            UnitBar_HTML.SetupUnitBar(obj);
            AbilityBar_HTML.SetupAbilityBar(obj);
        }

        static void changeColor(SpriteGlowEffect comp, Player owner)
        {
            if (owner == PlayersManager.Empty)
                comp.GlowColor = Color.white;
            else if (PlayersManager.GetMyPlayer() != owner)
                comp.GlowColor = Color.red;
            else
                comp.GlowColor = Color.green;
        }


        void RghtClickedOnGameObject(GameObject obj)
        {
            if (_choosed)
            {
                var from = _choosedObj.GetComponent<GameUnit>();
                if (from == null || !from.IsMy()) return;
                var to = obj.GetComponent<GameEntity>();

                if (to == null) return;

                if (!GroupUtil.isCreatureGroup(from.Group)) return;


                if (!FieldOfView.IsVisible(to) && !GroupUtil.IsGround(to.Group))
                {
                    ClickedOnSpace();
                    return;
                }

                Vector3Int f;
                if (from.MovingTo != null)
                {
                    f = from.MovingTo.Index;
                }
                else
                {
                    f = from.CurrentPos;
                    f.z--;
                }

                //Cant move ? 
                if (from.UpgradedStats.MoveSpeed <= 0f)
                {
                    SimpleOrderManager.CancelOrders(from);
                    ErrorBar_HTML.SetupError("This unit can`t move!");
                    return;
                }

                //Attack
                var t = to.CurrentPos;
                var buildingCheck = false;
                if (ChunkUtil.IsAnyEntity(from.ChunkNumber, ChunkUtil.GetUpper(t)))
                {
                    var underGround = ChunkManager.CurrentChunk.GetGameObjectByIndex(ChunkUtil.GetUpper(t));
                    var underEnt = underGround.GetComponent<GameEntity>();
                    if (SecondaryGroundLvL.isSecondaryGroup(underEnt.Group))
                        buildingCheck = true;

                    if (!GroupUtil.IsGround(underEnt.Group) && underEnt.Owner != PlayersManager.GetMyPlayer())
                    {
                        var underP = underEnt.CurrentPos;
                        underP.z++;

                        if (!ChunkManager.CurrentChunk.IsMapPos(underP)
                            || !ChunkUtil.IsAnyEntity(from.ChunkNumber, ChunkUtil.GetUpper(underEnt.CurrentPos)))
                        {
                            if (from.UpgradedStats.Dmg > 0)
                            {
                                SimpleOrderManager.AttackToIndex(from, t);
                            }
                            else if (from.UpgradedStats.Dmg == 0)
                            {
                                ErrorBar_HTML.SetupError("This unit can`t attack!");
                            }
                            return;
                        }
                    }
                }


                //Cancel or Move
                if ((ChunkUtil.IsAnyEntity(from.ChunkNumber, ChunkUtil.GetUpper(t))
                     || !PathCalcManager.IsReaching(f, t)) && !buildingCheck)
                {
                    ErrorBar_HTML.SetupError("Can`t reach that place!");
                    SimpleOrderManager.CancelOrders(from);
                }
                else if (ChunkUtil.IsCanStayHere(from.ChunkNumber, t) || buildingCheck)
                {
                    SimpleOrderManager.MoveToIndex(from, t);
                }
            }
            else
            {
                var clicked = obj.GetComponent<GameEntity>();
                if (clicked == null) return;

                var pos = clicked.CurrentPos;

                if (FlagManager.IsFlagAtPos(pos))
                    FlagManager.RemoveFlag(pos);
                else
                    FlagManager.SetupFlag(pos);
            }
        }

        void RemoveChoosedGUI()
        {
            if (_choosed)
            {
                var ent = _choosedObj.GetComponent<GameEntity>();
                UnitBar_HTML.ClearUnitBar(ent);
                AbilityBar_HTML.HideAbilityBar();
                // GUI_HpBar.RemoveHP(ent as AbstractGameObject);
                // GUI_UnitBar.RemoveBar(ent as AbstractGameObject);
            }
        }

        void ClickedOnSpace()
        {
            RemoveChoosedGUI();
            RemoveChosing();
        }

        public static bool IsChoosed(AbstractGameObject obj)
        {
            return obj != null && IsChoosed(obj.gameObject);
        }

        public static bool IsChoosed(GameObject obj)
        {
            return _choosed && _choosedObj.Equals(obj);
        }

        public static void RemoveChosing(GameObject obj)
        {
            if (IsChoosed(obj)) RemoveChosing();
        }

        private static void RemoveChosing()
        {
            _choosed = false;
            if (_choosedObj == null) return;
            if (chooseObjList.ContainsKey(_choosedObj))
            {
                var obj = chooseObjList[_choosedObj];
                Destroy(obj);
                chooseObjList.Remove(_choosedObj);
            }
            AbilityBar_HTML.HideAbilityBar();

            _choosedObj = null;
        }

        public static GameEntity GetChoosedEnt()
        {
            return _choosedObj != null ? _choosedObj.GetComponent<GameEntity>() : null;
        }
    }
}