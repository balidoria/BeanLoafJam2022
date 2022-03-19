using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantStage
{
    IMBABY,
    HALFWAYTHERE,
    FULLSIZE
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
    [Tooltip("How much I cost the player in the store.")]
    public int StorePrice;
    [Tooltip("How much the player gets when they sell me.")]
    public int SellPrice;

    private int OriginalSellPrice;

    [Tooltip("The effects I produce.")]
    public List<PlantEffect> Effects;

    // How long I have spent growing at the current stage of growth.
    private int growTimeInSeconds;

    [Tooltip("How often I need to be watered.")]
    public int WateringIntervalInSeconds;

    [Tooltip("How big I have grown.")]
    public PlantStage Size;

    [Tooltip("Am I growing, grown, thirsty, or dying?")]
    public PlantStatus Status;

    // How many seconds since I was last watered.
    private float secondsSinceLastWatered;

    [Tooltip("How many seconds until I die after I become thirsty.")]
    public float SecondsUntilDeathWhenThirsty;

    [Tooltip("How many seconds of time I need to spend growing to no longer be a baby.")]
    public float SecondsGrowingSmallToMidgrown;
    [Tooltip("How many seconds of time I need to spend growing to become completely grown.")]
    public float SecondsGrowingMediumToGrown;

    [Tooltip("How many seconds of time I need to spend completely grown before I begin to lose value.")]
    public float SecondsGrownToQualityDecay;

    [Tooltip("How many seconds of time I need to spend rotting before I lose half of my value.")]
    public float SecondsUntilLowestQuality;

    // How many seconds spent growing at the current PlantSize stage.
    private float secondsSpentGrowing = 0;

    // Have I been planted?
    internal bool IsPlanted = false;

    void Start()
    {
        // I don't need to be watered right away, they watered me at the store.
        secondsSinceLastWatered = 0;

        // Keep a record since our selling price can decay.
        OriginalSellPrice = SellPrice;
    }

    void Update()
    {
        if (!IsPlanted)
            return;

        // Update status to thirsty or dead if we need water.
        secondsSinceLastWatered += Time.deltaTime;
        if (secondsSinceLastWatered >= WateringIntervalInSeconds)
        {
            Status = PlantStatus.THIRSTY;
        }
        if (secondsSinceLastWatered >= SecondsUntilDeathWhenThirsty)
        {
            Status = PlantStatus.DEAD;
        }

        // Grow if we are growing.
        if (Status == PlantStatus.GROWING)
        {
            secondsSpentGrowing += Time.deltaTime;
            if (Size == PlantStage.IMBABY && secondsSpentGrowing >= SecondsGrowingSmallToMidgrown)
            {
                Size = PlantStage.HALFWAYTHERE;
                secondsSpentGrowing = 0;
            } else if (Size == PlantStage.HALFWAYTHERE && secondsSpentGrowing >= SecondsGrowingMediumToGrown)
            {
                secondsSpentGrowing = 0;
                Size = PlantStage.FULLSIZE;
                Status = PlantStatus.GROWN;
            }
        }

        // Decay in value if we've been Grown too long.
        if (Status == PlantStatus.GROWN)
        {
            secondsSpentGrowing += Time.deltaTime;
            if (secondsSpentGrowing >= SecondsGrownToQualityDecay)
            {
                SellPrice = OriginalSellPrice * (int)((secondsSpentGrowing - SecondsGrownToQualityDecay) / SecondsUntilLowestQuality);
            }
        }

        // Cast spells if we are ready.
        foreach (PlantEffect spell in Effects)
        {
            // TODO: Determine ranges.
            // spell.TryCast(this);
        }
    }

    private void OnMouseUpAsButton()
    {
        if ( GameManager.instance.plantBeingPlanted != null)
        {
            // No touching current plants until you're done planting!
            return;
        }

        if (Status == PlantStatus.DEAD)
        {
            RemovePlant();
        } else if (Status == PlantStatus.GROWN)
        {
            SellPlant();
        } else
        {
            WaterPlant();
        }
    }

    private void SellPlant()
    {
        GameManager.instance.PlayerSellPlant(this);
        RemovePlant();
    }

    private void WaterPlant()
    {
        // TODO: Formalize watering design.
        Debug.Log("Watered: " + this.ToString());
        secondsSinceLastWatered = 0;
        if (Size == PlantStage.FULLSIZE)
        {
            Status = PlantStatus.GROWN;
        } else if (Status != PlantStatus.DEAD)
        {
            Status = PlantStatus.GROWING;
        }
    }

    private void RemovePlant()
    {
        // Remove this plant from existence.
        Debug.Log("Remvoing: " + this.ToString());
        Destroy(gameObject);
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