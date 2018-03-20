﻿using UnityEngine;
using System.Collections;

/**
 * Holds resources for the Agent.
 */
public class NpcAttributesComponent : MonoBehaviour
{
    public int killCount;
    public int attackCount;
    public int braveCount;

    public NpcAttributesComponent()
    {
        reset();
    }

    void OnEnable()
    {
        reset();
    }

    private void reset()
    {
        killCount = 0;
        attackCount = 0;
        braveCount = 0;
    }
}