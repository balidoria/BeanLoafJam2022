using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    // TEMP DEBUG
    private KeyCode[] keyCodes = {
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };
     // TEMP END

    void Start()
    {
        
    }

    async void Update()
    {
        // TEMP DEBUG
        for(int i = 0 ; i < keyCodes.Length; i ++ )
        {
            if(Input.GetKeyDown(keyCodes[i]))
            {
                try
                {
                    PlayerBuyPlant(plantsForSale[i]);
                } catch (System.IndexOutOfRangeException e) {
                    Debug.Log(e.ToString() + ": No plant on sale for index: " + i);
                }
            }
        }
        // TEMP END
    }

    public void PlayerBuyPlant(BasePlant plant)
    {
        if (GameManager.instance.playerMoney >= plant.storePrice)
        {
            GameManager.instance.playerMoney -= plant.storePrice;
            GameManager.instance.plantBeingPlanted = plant;

            // TODO: Show player they paid for the plant.
            Debug.Log("Bought " + plant.ToString() + " for " + plant.storePrice);
        } else
        {
            // TODO: Show player they can't afford the plant.
            Debug.Log("Too poor to buy " + plant.ToString() + " for " + plant.storePrice);
        }
    }
 
    public List<BasePlant> plantsForSale;
}

