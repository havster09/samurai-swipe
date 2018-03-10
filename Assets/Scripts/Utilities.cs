using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {
    public static string ReplaceClone(string hasClone)
    {
        return hasClone.Replace("(Clone)", "");
    }

    public static IEnumerator FadeOut(SpriteRenderer renderer, float duration)
    {
        float start = Time.time;
        while (Time.time <= start + duration)
        {
            Color color = renderer.color;
            color.a = 1f - Mathf.Clamp01((Time.time - start) / duration);
            renderer.color = color;
            yield return new WaitForEndOfFrame();
        }
    }
}

