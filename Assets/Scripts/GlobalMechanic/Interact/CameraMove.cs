using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using GUI_Game.InGame.PauseMenu;
using Menus;
using UnityEngine;

namespace GlobalMechanic.Interact
{
    public class CameraMove : MonoBehaviour
    {
        public float SpeedMult = 1;
        private bool _pressed;
        private Vector3 _startPos;

        private const float RANGE = 10;
        private float h;
        private float v;

        private Vector3 min;
        private Vector3 max;

        public static Camera cam;

        void Awake()
        {
            cam = gameObject.GetComponent<Camera>();
        }
        
        void Start()
        {
            var rect = Camera.main.pixelRect;
            h = rect.height;
            v = rect.width;

            var size = ChunkManager.StaticMapSize - 1;

            var chunk = ChunkManager.CurrentChunk;

            var first = chunk.GetGameObjectByIndex(new Vector3Int(0, 0, 0));
            var firstEnd = chunk.GetGameObjectByIndex(new Vector3Int(0, size, 0));
            var end = chunk.GetGameObjectByIndex(new Vector3Int(size, size, 0));
            var endfirst = chunk.GetGameObjectByIndex(new Vector3Int(size, 0, 0));

            min = first.transform.position;
            min.y = firstEnd.transform.position.y;

            max = end.transform.position;
            max.y = endfirst.transform.position.y;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (PauseMenu_HTML.IsPaused) return;
            var mousePosition = Input.mousePosition;

            //X coordinate
            var x = 0;
            if (mousePosition.x <= RANGE)
                x = -1;
            else if (mousePosition.x >= v - RANGE)
                x = 1;

            //Y coordinate
            var y = 0;
            if (mousePosition.y <= RANGE)
                y = -1;
            else if (mousePosition.y >= h - RANGE)
                y = 1;

            //Translate on move
            if ((x != 0 || y != 0) && !_pressed)
                Move(
                    new Vector3(x * 10 * SpeedMult * Time.deltaTime, y * 10 * SpeedMult * Time.deltaTime, 0));

            if (Input.GetButtonDown("Fire3"))
            {
                _pressed = true;
                _startPos = mousePosition;
            }
            else if (Input.GetButton("Fire3"))
            {
                var t = (_startPos - mousePosition);
                //if (t.x < 10 && t.y < 10)
                // {
                // _startPos = mousePosition;
                var fff = t * SpeedMult * Time.deltaTime;

                //Debug.Log(fff);
                if (Mathf.Abs(fff.x) >= 2f
                    || Mathf.Abs(fff.y) >= 2f)
                {
                    fff.x = 0f;
                    fff.y = 0f;
                }

                Move(fff);


                _startPos = mousePosition;
            }
            if (Input.GetButtonUp("Fire3"))
            {
                _pressed = false;
            }
        }


        public static void SetCameraAt(Vector3Int pos)
        {
            SetCameraAt(new Vector2Int(pos.x, pos.y));
        }

        public static void SetCameraAt(Vector2Int pos)
        {
            var pos3D = Util.Get3DPosByIndex(new Vector3Int(pos.x, pos.y, 1));
            pos3D.z = Camera.main.transform.position.z;
            cam.transform.position = pos3D;
        }

        void Move(Vector3 moveVector)
        {
            var nVector = transform.position + moveVector;
            if (nVector.x > min.x && nVector.y < min.y
                && nVector.x < max.x && nVector.y > max.y)
                transform.Translate(moveVector);
        }
    }
}