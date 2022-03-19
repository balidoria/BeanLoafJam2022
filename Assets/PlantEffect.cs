using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEffect : MonoBehaviour
{
    public int secondsBetweenCasting;

    public EffectOperator type;

    public EffectTarget target;

    public int effectAdjustment;

    private int timeSinceLastCasting = 0;
}
