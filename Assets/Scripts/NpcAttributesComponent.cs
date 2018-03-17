using UnityEngine;
using System.Collections;

public class NpcAttributesComponent : MonoBehaviour
{
    public int numTools; // for mining ore and chopping logs
    public int numLogs; // makes firewood
    public int numFirewood; // what we want to make
    public int numOre; // makes tools
    public int health;
    public int brave;

    public NpcAttributesComponent()
    {
        health = 100;
        brave = 100;
    }
}