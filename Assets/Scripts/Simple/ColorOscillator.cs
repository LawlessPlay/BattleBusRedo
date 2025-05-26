using UnityEngine;
using UnityEngine.UI;

namespace TacticsToolkit
{
    //Generic color shifter
    public class ColorOscillator : MonoBehaviour
    {
        public Color color1;
        public Color color2;

        public float speed;

        public bool isUI = false;
        // Update is called once per frame
        void Update()
        {
            if (!isUI)
                GetComponent<SpriteRenderer>().material.color = Color.Lerp(color1, color2, Mathf.PingPong(Time.time * speed, 1));
            else
                GetComponent<Image>().color = Color.Lerp(color1, color2, Mathf.PingPong(Time.time * speed, 1));
        }
    }
}
