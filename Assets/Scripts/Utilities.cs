using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {
    public static string ReplaceClone(string hasClone)
    {
        return hasClone.Replace("(Clone)", "");
    }

}
