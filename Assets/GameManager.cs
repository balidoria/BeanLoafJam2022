using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("I'm a singleton, the most overused pattern of all time.")]
    public static GameManager instance;

    [Tooltip("The plant store.")]
    public StoreManager storeManager;

    [Tooltip("How much money the player has.")]
    [SerializeField] internal int playerMoney;

    [Tooltip("How much money the player starts the game with.")]
    [SerializeField] int playerStartingMoney;

    [Tooltip("How much money the player needs to win the game.")]    
    [SerializeField] internal int goalMoney;

    [Tooltip("Do not set! keeps track of the number of plants.")]
    public int numOfActivePlants = 0;

    [Header("Money Counter Animation")]
    public float moneyAnimDuration;
    public AnimationCurve moneyAnimCurve;

    public Grid GameGrid;
    public Tilemap GameTileMap;
    public Tilemap UITileMap;
    public Tile UIHighlightTile;
    public TMP_Text UIMoneyText;
    public Slider UIFundraiser;
    public GameObject UICanvas;
    public GameObject UIWin;
    public GameObject UILose;
    public AudioSource AudioObject;
    public AudioClip WinningSound;
    private Vector3Int UIHighlightTilePosition = Vector3Int.zero;
    bool ended = false;

    // The plant we can currently plant.
    internal BasePlant plantBeingPlanted = null;

    public Camera MainCamera;

    [Tooltip("How often we roll a plant for getting Weeds.")]
    public float SecondsBetweenWeedRolls;

    private float secondsSinceLastWeedRoll;
    void Awake() {
        instance = this;
    }

    void Start()
    {
        playerMoney = playerStartingMoney;
        UIMoneyText.text = playerMoney.ToString();
        numOfActivePlants = 0;

        UIWin.SetActive(false);
        UILose.SetActive(false);
    }

    void Update()
    {
        // Ending the game.
        if (!ended)
        {
            if (playerMoney >= goalMoney)
            {
                PlayerWins();
            }
            if (playerMoney < 10 && numOfActivePlants == 0)
            {
                PlayerLose();
            }
        }


        // Cursor.
        Vector3Int gridPosition = GameGrid.WorldToCell(MainCamera.ScreenToWorldPoint(Input.mousePosition));
        gridPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
        if (GameTileMap.HasTile(gridPosition))
        {
            UITileMap.SetTile(UIHighlightTilePosition, null);
            UITileMap.SetTile(gridPosition, UIHighlightTile);
            UIHighlightTilePosition = gridPosition;
        } else
        {
            UITileMap.SetTile(UIHighlightTilePosition, null);
            UIHighlightTilePosition = Vector3Int.zero;
        }

        // Planting plants.
        if (plantBeingPlanted != null)
        {
            var c2d = plantBeingPlanted.GetComponent<Collider2D>();
            c2d.enabled = false;                // make the plant sit on the mouse pos
            plantBeingPlanted.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Vector3.forward * 10);
            plantBeingPlanted.SaplingSpriteRenderer.transform.localScale = Vector3.one * 2;

            if (GameTileMap.HasTile(gridPosition) && TileEmpty(GameGrid.GetCellCenterWorld(gridPosition)))
            {
                if (Input.GetMouseButtonUp(0))
                {
                    plantBeingPlanted.transform.position = GameGrid.GetCellCenterWorld(gridPosition);
                    plantBeingPlanted.IsPlanted = true;
                    plantBeingPlanted.SaplingSpriteRenderer.transform.localScale = Vector3.one;
                    storeManager.PlantPlanted(plantBeingPlanted);
                    plantBeingPlanted.Status = PlantStatus.GROWING;
                    c2d.enabled = true;
                    var renderers = plantBeingPlanted.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var sr in renderers)
                    {
                        Color tmp = sr.color;
                        tmp.a = 1.0f;
                        sr.color = tmp;
                    }
                    // keep track of all planted plants.
                    numOfActivePlants++;
                    
                    plantBeingPlanted = null;
                }
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(plantBeingPlanted.gameObject);
                plantBeingPlanted = null;
            }
        }

        // Weeds.
        secondsSinceLastWeedRoll += Time.deltaTime;
        if (secondsSinceLastWeedRoll >= SecondsBetweenWeedRolls)
        {
            secondsSinceLastWeedRoll = 0;
            var plants = GameObject.FindObjectsOfType<BasePlant>();
            List<BasePlant> weedablePlants = new List<BasePlant>();
            foreach (var plant in plants)
            {
                if (plant.Status != PlantStatus.DEAD && plant.IsPlanted)
                {
                    weedablePlants.Add(plant);
                }
            }
            if (weedablePlants.Count > 0)
            {
                System.Random rand = new System.Random();
                var target = weedablePlants[rand.Next(weedablePlants.Count)];
                target.rollForWeeds();
            }
        }
    }

    public BasePlant GetPlantOnTile(Vector3Int gridPositionWorld)
    {
        var hit = Physics2D.OverlapCircle(Vector2Int.FloorToInt(new Vector2(gridPositionWorld.x, gridPositionWorld.y)), 1);
        if (hit == null)
            return null;

        return hit.GetComponentInChildren<BasePlant>();
    }

    public bool TileEmpty(Vector3 gridPositionWorld)
    {
        var hit = Physics2D.OverlapCircle(Vector2Int.FloorToInt(new Vector2(gridPositionWorld.x, gridPositionWorld.y)), 1);
        if (hit != null && hit.GetComponentInChildren<BasePlant>() != null)
        {
            return false;
        }

        return true;
    }

    public void PlayerSellPlant(BasePlant plant)
    {
        if (plant.Status == PlantStatus.GROWN)
        {
            //GameManager.instance.playerMoney += plant.SellPrice;
            UpdatePlayerMoney(plant.SellPrice);
            Debug.Log("Sold " + plant.ToString() + " for " + plant.SellPrice);
        } else
        {
            Debug.Log("Tried to sell " + plant.ToString() + " but it was not fully grown!");
        }
    }

    void PlayerWins()
    {
        ended = true;
        UICanvas.SetActive(false);
        UIWin.SetActive(true);
        AudioObject.PlayOneShot(WinningSound);
    }

    void PlayerLose()
    {
        ended = true;
        UICanvas.SetActive(false);
        UILose.SetActive(true);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(1);
    }

    public void UpdatePlayerMoney(int amount)
    {
        StopAllCoroutines();
        int start = playerMoney;
        int end = playerMoney + amount;

        playerMoney += amount;

        StartCoroutine(DoUpdatePlayerMoney(start, end));
    }

    IEnumerator DoUpdatePlayerMoney(int start, int end)
    {
        UIMoneyText.text = start.ToString();
        float timer = 0;
        if (start < end)
        {
            while (timer < 1)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0;
        }

        while (timer < moneyAnimDuration)
        {
            int temp = (int)Mathf.Lerp(start, end, moneyAnimCurve.Evaluate(timer / moneyAnimDuration));
            UIMoneyText.text = temp.ToString();
            UIFundraiser.value = temp;
            timer += Time.deltaTime;
            yield return null;
        }
        UIMoneyText.text = end.ToString();
    }

}
