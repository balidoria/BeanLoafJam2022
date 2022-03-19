using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEffect : MonoBehaviour
{
    [Tooltip("How many seconds between castings.")]
    public float secondsBetweenCasting;

    [Tooltip("The sort of effect I have.")]
    public EffectOperator type;

    [Tooltip("The target of my effect.")]
    public EffectTarget target;

    [Tooltip("The magnitude of my effect.")]
    public int effectAdjustment;

    // How long since I last casted.
    private float timeSinceLastCasting = 0;

    internal bool IsCastable = false;

    void Update()
    {
        timeSinceLastCasting += Time.deltaTime;
        if (timeSinceLastCasting >= secondsBetweenCasting)
        {
            IsCastable = true;
        }
    }

    public void TryCast(BasePlant originPlant)
    {
        if (!IsCastable)
            return;

        // TODO: Cast the effect based on the given plant origin and range.
        // ex. foreach (BasePlant in plantsInRange) do the thing.

    }
}
