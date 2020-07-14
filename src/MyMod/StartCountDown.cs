using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MyMod
{
    public class StartCountdown : UnityEngine.MonoBehaviour
    {
        int time, a;
        float x;
        public bool count;
        public string timeDisp;

        void Start()
        {
            time = 3;
            count = false;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (count)
            {
                timeDisp = time.ToString();
                GameObject.Find("StartCounter").GetComponent<Text>().text = timeDisp;
                x += Time.deltaTime;
                a = (int)x;
                print(a);
                switch (a)
                {
                    case 0: GameObject.Find("StartCounter").GetComponent<Text>().text = "3"; break;
                    case 1: GameObject.Find("StartCounter").GetComponent<Text>().text = "2"; break;
                    case 2: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 3: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 4: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 5: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 6: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 7: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 8: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 9: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 10: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 11: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 12: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 13: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 14: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 15: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 16: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 17: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 18: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 19: GameObject.Find("StartCounter").GetComponent<Text>().text = "1"; break;
                    case 20:
                        GameObject.Find("StartCounter").GetComponent<Text>().enabled = false;
                        count = false; break;
                }
            }
        }
    }
}