using System.Collections.Generic;
using GameUtils.Objects.Entities;
using GlobalMechanic.Interact;
using PowerUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI_Game.InGame.UnitBar
{
    public class UnitNumbers : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject textPrefab;


        public const float livingTime = 1f;
        private const float speed = 1f;
        private const int maxNumbersPerUnit = 10;

        private static List<TextMeshProUGUI> numbersMas = new List<TextMeshProUGUI>();

        private readonly List<TextMeshProUGUI> Fdel = new List<TextMeshProUGUI>();
        
        private static List<TextMeshProUGUI> forAddList = new List<TextMeshProUGUI>();
        
        private static Dictionary<TextMeshProUGUI, float> livingTextTime = new Dictionary<TextMeshProUGUI, float>();

        public static Dictionary<GameObject, GameObject> pointList = new Dictionary<GameObject, GameObject>();

        public static Dictionary<GameObject, List<TextMeshProUGUI>> unitNumbers =
            new Dictionary<GameObject, List<TextMeshProUGUI>>();

        public static Dictionary<GameObject, int> unitCurrNumber = new Dictionary<GameObject, int>();

        private static GameObject static_canvas;
        private static GameObject static_textPrefab;


        void Awake()
        {
            static_canvas = canvas;
            static_textPrefab = textPrefab;
        }

        private static TextMeshProUGUI FindFreeNumber(GameObject obj, GameObject canvas)
        {
            if (unitNumbers.ContainsKey(obj))
            {
                var curr = unitCurrNumber[obj];

                if (curr + 1 < maxNumbersPerUnit)
                    curr++;
                else
                    curr = 0;

                unitCurrNumber[obj] = curr;

                return unitNumbers[obj][curr];
            }
            var list = new List<TextMeshProUGUI>(maxNumbersPerUnit);
            unitNumbers.Add(obj, list);
            unitCurrNumber.Add(obj, 0);

            for (int i = 0; i < maxNumbersPerUnit; i++)
            {
                var newGO = Instantiate(static_textPrefab, canvas.transform);
                var textC = newGO.GetComponent<TextMeshProUGUI>();
                textC.text = "";
                list.Add(textC);
            }
            return unitNumbers[obj][0];
        }

        private static GameObject SetupCanvas(GameEntity ent)
        {
            var point = UnitBar_HTML.GetUnderPoint(ent);

            var canvas = Instantiate(static_canvas);
            canvas.transform.position = point;
            canvas.transform.SetParent(ent.transform);
            pointList.Add(ent.gameObject, canvas.gameObject);
            return canvas;
        }

        private const float extraSpace = 4.8f;
        private const float randomSpace = 0.4f;

        public static void AddNumber(GameEntity ent, int n)
        {
            if (ent == null || ent.gameObject == null) return;
            var canvas = pointList.ContainsKey(ent.gameObject) ? pointList[ent.gameObject] : SetupCanvas(ent);
            var textC = FindFreeNumber(ent.gameObject, canvas);

            var pos = textC.transform.localPosition;
            pos.x = Random.Range(extraSpace - randomSpace, extraSpace + randomSpace);
            pos.y = 0;
            textC.transform.localPosition = pos;

            var c = textC.color;
            c.a = 1;
            textC.color = c;
            textC.text = n.ToString();

            livingTextTime[textC] = 0;
            forAddList.Add(textC);
            //static_livingTextTime.Add(textC, 0);
        }

        private static float t = 0;
        private const float UpdateTime = 0.08f;

        private const float colorPerTime = livingTime / 255 * 10f;


        

        void Update()
        {
            t += Time.deltaTime;
            if (t < UpdateTime) return;
            t = 0;

            if (forAddList.Count > 0)
            {
                numbersMas.AddRange(forAddList);
                forAddList.Clear();
            }

            if (numbersMas.Count == 0) return;

            Fdel.Clear();

            for (int i = 0; i < numbersMas.Count; i++)
            {
                var obj = numbersMas[i];
                if (obj == null)
                {
                    numbersMas.RemoveAt(i);
                    continue;
                }
                //Positon
                var pos = obj.transform.position;
                pos.y += speed * Time.deltaTime;
                obj.transform.position = pos;
                //Color
                var c = obj.color;
                if (c.a <= 0) c.a = 0;
                else
                    c.a -= colorPerTime;
                obj.color = c;
                //Living time
                livingTextTime[obj] += Time.deltaTime;
                if (livingTextTime[obj] >= livingTime)
                    Fdel.Add(obj);
            }

            for (int i = 0; i < Fdel.Count; i++)
            {
                var l = Fdel[i];
                numbersMas.Remove(l);
                //livingTextTime.Remove(l);

                //Destroy(l);
            }
        }
    }
}