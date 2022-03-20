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
    [Tooltip("How much the player gets when they sell me at my WORST.")]
    public int BottomSellPrice = 10;

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

    [Tooltip("How forward do particles need to be offset in the Z position to be seen?")]
    public float particleZOffset = 3;

    // How many seconds spent growing at the current PlantSize stage.
    private float secondsSpentGrowing = 0;

    // Have I been planted?
    internal bool IsPlanted = false;

    [Tooltip("Base chance of getting weeds on a weed roll, out of 100.")]
    public int chanceOfWeededness;

    internal float weedednessModifier = 1.0f;

    internal float waterNeedModifier = 1.0f;

    internal bool hasWeeds = false;

    public SpriteRenderer ThirstNotification;
    public SpriteRenderer PlantBody;
    public SpriteRenderer Weeds;
    public SpriteRenderer SaplingSpriteRenderer;
    public SpriteRenderer JuvenileSpriteRenderer;
    public GameObject AdultSpriteRenderer;
    public SpriteRenderer DeadSpriteRenderer;

    internal List<EffectTarget> ActiveEffects = new List<EffectTarget>();
    private float timeBetweenEffectResets = 5;
    private float timeSinceEffectReset = 0.0f;
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
    bool IsWatered = false;


    bool areSpells;

    void Start()
    {
        // I don't need to be watered right away, they watered me at the store.
        secondsSinceLastWatered = 0;
        ThirstNotification.enabled = false;
        Weeds.enabled = false;

        // Keep a record since our selling price can decay.
        OriginalSellPrice = SellPrice;

        ClearBodySprites();
        SaplingSpriteRenderer.enabled = true;

        audioSource = FindObjectOfType<AudioSource>();
        
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
    }

    void Update()
    {
        if (!IsPlanted)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int gridPosition = GameManager.instance.GameGrid.WorldToCell(GameManager.instance.MainCamera.ScreenToWorldPoint(Input.mousePosition));
            gridPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
            var worldPos = GameManager.instance.GameGrid.GetCellCenterWorld(gridPosition);
            if (!GameManager.instance.TileEmpty(worldPos))
            {
                var hits = Physics2D.OverlapCircleAll(Vector2Int.FloorToInt(new Vector2(worldPos.x, worldPos.y)), 1);
                foreach (var plantHit in hits)
                {
                    var plant = plantHit.GetComponentInChildren<BasePlant>();
                    if (plant != null && plant == this)
                        OnClicked();
                }
                
            }
        }

        // Update our current buffs.
        timeSinceEffectReset += Time.deltaTime;
        if (timeSinceEffectReset > timeBetweenEffectResets)
        {
            timeBetweenEffectResets = 0;
            weedednessModifier = 1.0f;
            waterNeedModifier = 1.0f;
            ActiveEffects.Clear();
        }

        //sound and effect code    
        if(!planted)
        {
            audioSource.PlayOneShot(plantPlaced,0.3f);
            var cp = Instantiate(clearPlot, new Vector3(this.transform.position.x,this.transform.position.y,-particleZOffset), Quaternion.identity);
            cp.transform.parent = this.transform;
            clearPlot.Play();
            planted = true;
        }

        // Update status to thirsty or dead if we need water.
        if (secondsSinceLastWatered >= WateringIntervalInSeconds.y * waterNeedModifier && Status != PlantStatus.DEAD && Status != PlantStatus.GROWN)
        {
            if(!IsWatered)
            {
                IsWatered = true;
                audioSource.PlayOneShot(thirsty,0.3f);
            }
            Status = PlantStatus.THIRSTY;
            ThirstNotification.enabled = true;
        }
        if (secondsSinceLastWatered - WateringIntervalInSeconds.y * waterNeedModifier >= SecondsUntilDeathWhenThirsty && Status != PlantStatus.DEAD)
        {
            audioSource.PlayOneShot(plantDied,0.3f);
            var death = Instantiate(plantDeath, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
            death.transform.parent = this.transform;
            plantDeath.Play();

            Status = PlantStatus.DEAD;
            ClearBodySprites();
            DeadSpriteRenderer.enabled = true;
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
                var transition = Instantiate(plantTransition, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
                transition.transform.parent = this.transform;
                plantTransition.Play();

                Size = PlantStage.HALFWAYTHERE;
                ClearBodySprites();
                JuvenileSpriteRenderer.enabled = true;
                secondsSpentGrowing = 0;

               
            } else if (Size == PlantStage.HALFWAYTHERE && secondsSpentGrowing >= SecondsGrowingMediumToGrown)
            {
                 //Particle Effects and Sound
                audioSource.PlayOneShot(plantReady,0.3f);
                var grown = Instantiate(growthComplete, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
                grown.transform.parent = this.transform;
                growthComplete.Play();

                secondsSpentGrowing = 0;
                Size = PlantStage.FULLSIZE;
                Status = PlantStatus.GROWN;
                Debug.Log("I am a FULL BOY!");
                ClearBodySprites();
                AdultSpriteRenderer.SetActive(true);
            }
        }

        // Decay in value if we've been Grown too long, stopping if sell value hits zero.
        if (Status == PlantStatus.GROWN && SellPrice > BottomSellPrice)
        {
            secondsSpentGrowing += Time.deltaTime;
            if (secondsSpentGrowing >= SecondsGrownToQualityDecay)
            {
                SellPrice = Mathf.Clamp(OriginalSellPrice - (int)(OriginalSellPrice * 0.5f * ((secondsSpentGrowing - SecondsGrownToQualityDecay) / SecondsUntilLowestQuality)), BottomSellPrice, int.MaxValue);
            }
        }

 
             // Cast spells if we are ready.
             foreach (PlantEffect spell in Effects)
             {
                 if(spell!= null){
                    spell.TryCast(this);
                 }
                 
             }
       
    }

    internal void rollForWeeds()
    {
        System.Random rand = new System.Random();

        int roll = rand.Next(100);

        if (roll < chanceOfWeededness * weedednessModifier)
        {
            // Get weeded.
            hasWeeds = true;
            Weeds.enabled = true;
        }
    }

    private void OnClicked()
    {
        if (GameManager.instance.plantBeingPlanted != null)
        {
            // No touching current plants until you're done planting!
            return;
        }

        if (Status == PlantStatus.DEAD)
        {
            RemovePlant();
            return;
        }

        if (Status == PlantStatus.GROWN)
        {
            SellPlant();
            return;
        } 

        if (!hasWeeds)
        {
            WaterPlant();
        } else 
        {
            RemoveWeeds();
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
        var moneyPartlices = Instantiate(money, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
        moneyPartlices.transform.parent = this.transform;
        money.Play();

        GameManager.instance.PlayerSellPlant(this);
        RemovePlant();
    }

    internal void WaterPlant()
    {
        audioSource.PlayOneShot(water,0.3f);
        var waterParticles = Instantiate(waterPlant, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
        waterParticles.transform.parent = this.transform;
        waterPlant.Play();
        IsWatered = false;
        Debug.Log("Watered: " + this.ToString());

        // Start next watering round with a range.
        System.Random rand = new System.Random();
        secondsSinceLastWatered = rand.Next(WateringIntervalInSeconds.y - WateringIntervalInSeconds.x);

        if (Size == PlantStage.FULLSIZE)
        {
            Debug.Log("I am a FULL BOY!");
            Status = PlantStatus.GROWN;
            ClearBodySprites();
            AdultSpriteRenderer.SetActive(true);

        } else if (Status != PlantStatus.DEAD)
        {
            Status = PlantStatus.GROWING;
            ClearBodySprites();
            if (Size == PlantStage.IMBABY)
            {
                SaplingSpriteRenderer.enabled = true;
            } else
            {
                JuvenileSpriteRenderer.enabled = true;
            }
        }
        ThirstNotification.enabled = false;
    }

    private void RemovePlant()
    {
        audioSource.PlayOneShot(digUpPlant,0.3f);
        var clear = Instantiate(clearPlot, new Vector3(this.transform.position.x,this.transform.position.y, -particleZOffset), Quaternion.identity);
        clear.transform.parent = this.transform;
        clearPlot.Play();

        // tell gamemanager we are without this plant
        GameManager.instance.numOfActivePlants--;

        // Remove this plant from existence.
        Debug.Log("Remvoing: " + this.ToString());
        Destroy(gameObject);
    }

    private void ClearBodySprites()
    {
        SaplingSpriteRenderer.enabled = false;
        JuvenileSpriteRenderer.enabled = false;
        AdultSpriteRenderer.SetActive(false);
    }
}
