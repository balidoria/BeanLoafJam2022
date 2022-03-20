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

    [Tooltip("How often I need to be watered, x is lower bound y is upper bound.")]
    public Vector2Int WateringIntervalInSeconds;

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

    [Tooltip("Base chance of getting weeds on a weed roll, out of 100.")]
    public int chanceOfWeededness;

    internal int weedednessModifier;

    internal bool hasWeeds = false;

    public SpriteRenderer ThirstNotification;
    public SpriteRenderer PlantBody;

    public SpriteRenderer Weeds;

    public Sprite SaplingSprite;
    public Sprite JuvenileSprite;
    public Sprite AdultSprite;
    public Sprite DeadSprite;

    void Start()
    {
        // I don't need to be watered right away, they watered me at the store.
        secondsSinceLastWatered = 0;
        ThirstNotification.enabled = false;
        Weeds.enabled = false;

        // Keep a record since our selling price can decay.
        OriginalSellPrice = SellPrice;

        PlantBody.sprite = SaplingSprite;

    }

    void Update()
    {
        if (!IsPlanted)
            return;

        // Update status to thirsty or dead if we need water.
        secondsSinceLastWatered += Time.deltaTime;
        System.Random rand = new System.Random();
        if (secondsSinceLastWatered >= WateringIntervalInSeconds.y && Status != PlantStatus.DEAD)
        {
            Status = PlantStatus.THIRSTY;
            ThirstNotification.enabled = true;
        }
        if (secondsSinceLastWatered - WateringIntervalInSeconds.y >= SecondsUntilDeathWhenThirsty)
        {
            Status = PlantStatus.DEAD;
            PlantBody.sprite = DeadSprite;
            ThirstNotification.enabled = false;
        }

        // Grow if we are growing.
        if (Status == PlantStatus.GROWING)
        {
            if (!hasWeeds)
                secondsSpentGrowing += Time.deltaTime;

            if (Size == PlantStage.IMBABY && secondsSpentGrowing >= SecondsGrowingSmallToMidgrown)
            {
                Size = PlantStage.HALFWAYTHERE;
                PlantBody.sprite = JuvenileSprite;
                secondsSpentGrowing = 0;
            } else if (Size == PlantStage.HALFWAYTHERE && secondsSpentGrowing >= SecondsGrowingMediumToGrown)
            {
                secondsSpentGrowing = 0;
                Size = PlantStage.FULLSIZE;
                Status = PlantStatus.GROWN;
                PlantBody.sprite = AdultSprite;
            }
        }

        // Decay in value if we've been Grown too long.
        if (Status == PlantStatus.GROWN)
        {
            secondsSpentGrowing += Time.deltaTime;
            if (secondsSpentGrowing >= SecondsGrownToQualityDecay)
            {
                SellPrice = OriginalSellPrice - (int)(OriginalSellPrice * 0.5f * ((secondsSpentGrowing - SecondsGrownToQualityDecay) / SecondsUntilLowestQuality));
            }
        }

        // Cast spells if we are ready.
        foreach (PlantEffect spell in Effects)
        {
            // TODO: Determine ranges.
            // spell.TryCast(this);
        }
    }

    internal void rollForWeeds()
    {
        System.Random rand = new System.Random();

        int roll = rand.Next(100);

        if (roll < chanceOfWeededness + weedednessModifier)
        {
            // Get weeded.
            hasWeeds = true;
            Weeds.enabled = true;
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
            if (!hasWeeds)
            {
                WaterPlant();
            } else 
            {
                RemoveWeeds();
            }
        }
    }

    private void RemoveWeeds()
    {
        Debug.Log("Deweeded " + this.ToString());
        hasWeeds = false;
        Weeds.enabled = false;
    }

    private void SellPlant()
    {
        GameManager.instance.PlayerSellPlant(this);
        RemovePlant();
    }

    private void WaterPlant()
    {
        Debug.Log("Watered: " + this.ToString());

        // Start next watering round with a range.
        System.Random rand = new System.Random();
        secondsSinceLastWatered = rand.Next(WateringIntervalInSeconds.y - WateringIntervalInSeconds.x);

        if (Size == PlantStage.FULLSIZE)
        {
            Status = PlantStatus.GROWN;
            PlantBody.sprite = AdultSprite;

        } else if (Status != PlantStatus.DEAD)
        {
            Status = PlantStatus.GROWING;
            PlantBody.sprite = Size == PlantStage.IMBABY ? SaplingSprite : JuvenileSprite;
        }
        ThirstNotification.enabled = false;
    }

    private void RemovePlant()
    {
        // Remove this plant from existence.
        Debug.Log("Remvoing: " + this.ToString());
        Destroy(gameObject);
    }
}