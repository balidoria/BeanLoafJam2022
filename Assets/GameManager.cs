using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    public Grid GameGrid;
    public Tilemap GameTileMap;
    public Tilemap UITileMap;
    public Tile UIHighlightTile;
    private Vector3Int UIHighlightTilePosition = Vector3Int.zero;

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
    }

    void Update()
    {
        // Ending the game.
        if (playerMoney >= goalMoney)
        {
            // TODO: End and win game.
        }
        if (playerMoney <= 0)
        {
            // TODO: End and lose game.
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
            c2d.enabled = false;

            if (GameTileMap.HasTile(gridPosition) && TileEmpty(GameGrid.GetCellCenterWorld(gridPosition)))
            {
                plantBeingPlanted.transform.position = GameGrid.GetCellCenterWorld(gridPosition);
                if (Input.GetMouseButtonUp(0))
                {
                    plantBeingPlanted.IsPlanted = true;
                    plantBeingPlanted.Status = PlantStatus.GROWING;
                    c2d.enabled = true;
                    var renderers = plantBeingPlanted.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var sr in renderers)
                    {
                        Color tmp = sr.color;
                        tmp.a = 1.0f;
                        sr.color = tmp;
                    }
                    
                    
                    plantBeingPlanted = null;
                }
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

    private BasePlant GetPlantOnTile(Vector3Int gridPositionWorld)
    {
        var hit = Physics2D.OverlapCircle(Vector2Int.FloorToInt(new Vector2(gridPositionWorld.x, gridPositionWorld.y)), 1);
        if (hit == null)
            return null;

        return hit.GetComponentInChildren<BasePlant>();
    }

    private bool TileEmpty(Vector3 gridPositionWorld)
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
            GameManager.instance.playerMoney += plant.SellPrice;
            Debug.Log("Sold " + plant.ToString() + " for " + plant.SellPrice);
        } else
        {
            // TODO: No sale!
            Debug.Log("Tried to sell " + plant.ToString() + " but it was not fully grown!");
        }
    }


}
