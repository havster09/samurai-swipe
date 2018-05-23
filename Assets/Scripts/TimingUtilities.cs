using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class TimingUtilities : MonoBehaviour
    {
        public static TimingUtilities Instance;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }

        public void WaitFor(Action action, float duration, bool cancel = true)
        {
            StartCoroutine(WaitForCheck(action, duration, cancel));
        }

        public IEnumerator WaitForCheck(Action callback, float duration, bool cancel)
        {
            float start = Time.time;
            while (Time.time <= start + duration && cancel)
            {
                yield return new WaitForSeconds(.1f);
            }
            callback();
        }
    }
}