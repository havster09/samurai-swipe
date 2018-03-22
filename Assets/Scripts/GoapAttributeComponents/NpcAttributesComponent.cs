using UnityEngine;
using System.Collections;

/**
 * Holds resources for the Agent.
 */
public class NpcAttributesComponent : MonoBehaviour
{
    public int killCount;
    public int attackCount;
    public int brave;
    public int health;

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
        brave = 0;
        health = 100;
    }
}