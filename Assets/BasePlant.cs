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

    public ParticleSystem plantTransition;
    public ParticleSystem waterPlant;
    public ParticleSystem growthComplete;
    public ParticleSystem plantDeath;
    public ParticleSystem clearPlot;
    public ParticleSystem money;

    public AudioClip plantTransitionGrowth;
    public AudioClip plantSpecialSound;
    public AudioClip water;
    public AudioClip plantReady;
    public AudioClip plantPlaced;
    public AudioClip plantDied;
    public AudioClip digUpPlant;
    public AudioClip deweed;
    public AudioClip sellPlant;
    public AudioClip thirsty;

    public AudioSource audioSource;
    bool planted = false;
    public bool specialAudioPlant;
    bool iswatered = false;




    void Start()
    {
        // I don't need to be watered right away, they watered me at the store.
        secondsSinceLastWatered = 0;
        ThirstNotification.enabled = false;
        Weeds.enabled = false;

        // Keep a record since our selling price can decay.
        OriginalSellPrice = SellPrice;

        PlantBody.sprite = SaplingSprite;

        audioSource = FindObjectOfType<AudioSource>();
        

    }

    void Update()
    {
        if (!IsPlanted)
            return;

        //sound and effect code    
        if(!planted){
                audioSource.PlayOneShot(plantPlaced,0.3f);
                Instantiate(clearPlot, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
                clearPlot.Play();
                planted = true;
            }
            

        // Update status to thirsty or dead if we need water.
        secondsSinceLastWatered += Time.deltaTime;
        System.Random rand = new System.Random();
        if (secondsSinceLastWatered >= WateringIntervalInSeconds.y && Status != PlantStatus.DEAD && Status != PlantStatus.GROWN)
        {
            if(!iswatered){
                iswatered = true;
                audioSource.PlayOneShot(thirsty,0.3f);
            }
            Status = PlantStatus.THIRSTY;
            ThirstNotification.enabled = true;
        }
        if (secondsSinceLastWatered - WateringIntervalInSeconds.y >= SecondsUntilDeathWhenThirsty)
        {
            audioSource.PlayOneShot(plantDied,0.3f);
            Instantiate(plantDeath, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
            plantDeath.Play();

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
                 //Particle Effects and Sound
                audioSource.PlayOneShot(plantTransitionGrowth,0.3f);
                Instantiate(plantTransition, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
                plantTransition.Play();

                Size = PlantStage.HALFWAYTHERE;
                PlantBody.sprite = JuvenileSprite;
                secondsSpentGrowing = 0;

               
            } else if (Size == PlantStage.HALFWAYTHERE && secondsSpentGrowing >= SecondsGrowingMediumToGrown)
            {
                 //Particle Effects and Sound
                audioSource.PlayOneShot(plantReady,0.3f);
                Instantiate(growthComplete, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
                growthComplete.Play();

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
        audioSource.PlayOneShot(deweed,0.3f);
        Debug.Log("Deweeded " + this.ToString());
        hasWeeds = false;
        Weeds.enabled = false;
    }

    private void SellPlant()
    {
        //effects and sounds
        audioSource.PlayOneShot(sellPlant,0.3f);
        Instantiate(money, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
        money.Play();

        GameManager.instance.PlayerSellPlant(this);
        RemovePlant();
    }

    private void WaterPlant()
    {
        audioSource.PlayOneShot(water,0.3f);
        Instantiate(waterPlant, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
        waterPlant.Play();
        iswatered = false;
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
        audioSource.PlayOneShot(digUpPlant,0.3f);
        Instantiate(clearPlot, new Vector3(this.transform.position.x,this.transform.position.y,0f), Quaternion.identity);
        clearPlot.Play();

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