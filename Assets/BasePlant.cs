using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantSize
{
    SMOL,
    MEDIUM,
    LARGE
}

public enum PlantStatus
{
    GROWING,
    THIRSTY,
    DEAD,
    GROWN
}

public class BasePlant : MonoBehaviour
{
    public int storePrice;
    public int sellPrice;

    public List<PlantEffect> effects;

    public bool isTree;

    public int growTimeInSeconds;

    public int wateringIntervalInSeconds;

    public PlantSize size;

    public PlantStatus status;

    private float secondsSinceLastWatered;

    public float secondsUntilDeathWhenThirsty;

    public float secondsGrowingSmallToMedium;
    public float secondsGrowingMediumToLarge;

    private float secondsSpentGrowing = 0;

    void Start()
    {
        secondsSinceLastWatered = wateringIntervalInSeconds;
    }

    void Update()
    {
        // Are we thirsty or even dead?
        secondsSinceLastWatered -= Time.deltaTime;
        if (secondsSinceLastWatered <= 0)
        {
            status = PlantStatus.THIRSTY;
        }
        if (secondsSinceLastWatered >= secondsUntilDeathWhenThirsty)
        {
            status = PlantStatus.DEAD;
        }

        // Grow if we are growing.
        if (status == PlantStatus.GROWING)
        {
            secondsSpentGrowing += Time.deltaTime;
            if (size == PlantSize.SMOL && secondsSpentGrowing >= secondsGrowingSmallToMedium)
            {
                size = PlantSize.MEDIUM;
                secondsSpentGrowing = 0;
            } else if (size == PlantSize.MEDIUM && secondsSpentGrowing >= secondsGrowingMediumToLarge)
            {
                size = PlantSize.LARGE;
                status = PlantStatus.GROWN;
            }
        }

        // Cast spells if we are ready.
        foreach (PlantEffect spell in effects)
        {
            
        }
    }

    private void OnMouseUpAsButton()
    {    
        if (status == PlantStatus.DEAD)
        {
            RemoveDeadPlant();
        } else
        {
            WaterPlant();
        }
    }

    private void WaterPlant()
    {
        // TODO: Formalize watering design.
        Debug.Log("Watered: " + this.ToString());
        secondsSinceLastWatered = wateringIntervalInSeconds;
        if (size == PlantSize.LARGE)
        {
            status = PlantStatus.GROWN;
        } else if (status != PlantStatus.DEAD)
        {
            status = PlantStatus.GROWING;
        }
    }

    private void RemoveDeadPlant()
    {
        // Remove this plant from existence.
        Debug.Log("Remvoing: " + this.ToString());
        Destroy(this);
    }
}

public enum EffectOperator
{
    ADDORSUBTRACT,
    MULTIPLY
}

public enum EffectTarget
{
    GROWSPEED,
    WATERNEED,
    BANKACCOUNT
}