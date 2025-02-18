using System.Collections;
using UnityEngine;

    [CreateAssetMenu(fileName = "AttackFx", menuName = "ScriptableObjects/AttackFx", order = 1)]
    public class AttackFx : ScriptableObject
    {
        public GameObject[] attacks;
        public GameObject[] flippedAttacks;
        public AudioClip[] clips;
    }