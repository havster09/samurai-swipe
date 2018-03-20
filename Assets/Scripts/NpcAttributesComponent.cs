using UnityEngine;
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
        braveCount = 100;
    }
}