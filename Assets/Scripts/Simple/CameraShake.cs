using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TacticsToolkit
{
    //Generic CameraShake script
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake instance;

        private Vector3 _originalPos;
        private float _timeAtCurrentFrame;
        private float _timeAtLastFrame;
        private float _fakeDelta;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            // Calculate a fake delta time, so we can Shake while game is paused.
            _timeAtCurrentFrame = Time.realtimeSinceStartup;
            _fakeDelta = _timeAtCurrentFrame - _timeAtLastFrame;
            _timeAtLastFrame = _timeAtCurrentFrame;
        }

        public static void Shake(float duration, float amount)
        {
            instance._originalPos = instance.gameObject.transform.localPosition;
            instance.StopAllCoroutines();
            instance.StartCoroutine(instance.cShake(duration, amount));
        }

        public IEnumerator cShake(float duration, float amount)
        {
            float elapsedTime = 0;

            // Rotate gradually over time
            while (elapsedTime < duration)
            {
                transform.localPosition = _originalPos + Random.insideUnitSphere * amount;

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = _originalPos;
        }
    }
}
