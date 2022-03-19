using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public StoreManager storeManager;

    public int playerMoney;

    [SerializeField] int playerStartingMoney;
    
    public int goalMoney;

    internal BasePlant plantBeingPlanted;

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


    }

    public void PlayerSellPlant(BasePlant plant)
    {
        if (plant.status == PlantStatus.GROWN)
        {
            GameManager.instance.playerMoney += plant.sellPrice;
            // TODO: Remove the plant from the player garden.
        } else
        {
            // TODO: No sale!
            Debug.Log("Tried to sell " + plant.ToString() + " but it is not fully grown");
        }
    }


}
