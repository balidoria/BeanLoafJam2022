using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEffect : MonoBehaviour
{
    [Tooltip("How many seconds between castings, x is lower range and y is higher. Please use 1 second for each as default.")]
    public Vector2Int SecondsBetweenCastingRange;

    [Tooltip("The target of my effect.")]
    public EffectTarget target;

    [Tooltip("The magnitude of my effect. Ex. 1.45 for %45 increase.")]
    public float effectAdjustment;

    [Tooltip("My range of effect.")]
    public int Range;

    // How long since I last casted.
    private float timeSinceLastCasting = 0;

    internal bool IsCastable = false;

    void Update()
    {
        timeSinceLastCasting += Time.deltaTime;
        if (timeSinceLastCasting >= SecondsBetweenCastingRange.y)
        {
            IsCastable = true;
        }
    }

    public void TryCast(BasePlant originPlant)
    {
        if (!IsCastable)
            return;

        var grid = GameManager.instance.GameGrid;
        var cellPosition = grid.WorldToCell(originPlant.transform.position);
        var plantsInRange = new List<BasePlant>();
        for (int i = 1; i <= Range; i++)
        {
            var plantsAffected = new HashSet<BasePlant>();

            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.up * 3 * Range, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.down * 3 * Range, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.left * 3 * Range, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.right * 3 * Range, Color.red, 1.0f);

            var hits = Physics2D.OverlapCircleAll(Vector2Int.FloorToInt(new Vector2(originPlant.transform.position.x, originPlant.transform.position.y))
            , 3 * Range);

            foreach (var plantHit in hits)
            {
                var plant = plantHit.GetComponentInChildren<BasePlant>();
                if (plant != null)
                {
                    castOnPlant(plant);
                }
            }
        }

        IsCastable = false;
        // Start next spell round with a range.
        System.Random rand = new System.Random();
        timeSinceLastCasting = rand.Next(SecondsBetweenCastingRange.y - SecondsBetweenCastingRange.x);
    }

    void castOnPlant(BasePlant plant)
    {
        if (target == EffectTarget.PLANTGROWTHRATE && !plant.ActiveEffects.Contains(EffectTarget.PLANTGROWTHRATE))
        {
            plant.waterNeedModifier = effectAdjustment;
            plant.ActiveEffects.Add(EffectTarget.PLANTGROWTHRATE);
        }

        if (target == EffectTarget.WEEDRESISTANCE && !plant.ActiveEffects.Contains(EffectTarget.WEEDRESISTANCE))
        {
            plant.weedednessModifier = effectAdjustment;
            plant.ActiveEffects.Add(EffectTarget.WEEDRESISTANCE);
        }

        if (target == EffectTarget.WATERPLANTS)
        {
            plant.WaterPlant();
        }
    }
}

public enum EffectTarget
{
    PLANTGROWTHRATE,
    WEEDRESISTANCE,
    WATERPLANTS
}