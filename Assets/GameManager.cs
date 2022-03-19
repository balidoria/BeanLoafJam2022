using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // The plant we can currently plant.
    internal BasePlant plantBeingPlanted = null;

    public Camera MainCamera;

    void Awake() {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (playerMoney >= goalMoney)
        {
            // TODO: End and win game.
        }

        if (plantBeingPlanted != null)
        {
            Vector3Int point = GameGrid.WorldToCell(MainCamera.ScreenToWorldPoint(Input.mousePosition));
            plantBeingPlanted.transform.position = GameGrid.GetCellCenterWorld(point);

            if (Input.GetMouseButtonUp(0))
            {
                // Place plant if valid grid position.
                // TODO: Check valid grid position.

                plantBeingPlanted = null;
            }
        }
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
