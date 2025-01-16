using System.Collections;
using UnityEngine;

    [CreateAssetMenu(fileName = "Combo", menuName = "ScriptableObjects/Combo", order = 1)]
    public class Combo : ScriptableObject
    {
        public GameObject[] attacks;
        public GameObject[] flippedAttacks;
    }