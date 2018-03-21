using UnityEngine;
using System.Collections;

public class NpcHeroAttributesComponent : MonoBehaviour
{
    public int health;
    public int brave;

    public NpcHeroAttributesComponent()
    {
        health = 100;
        brave = 100;
    }
}