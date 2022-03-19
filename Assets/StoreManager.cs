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

    void Update()
    {
        // TEMP DEBUG
        for(int i = 0 ; i < keyCodes.Length; i ++ )
        {
            if(Input.GetKeyDown(keyCodes[i]))
            {
                try
                {
                    PlayerBuyPlant(plantsForSale[i]);
                } catch (System.Exception e) 
                {
                    Debug.Log(e.ToString() + ": No plant on sale for index: " + i);
                }
            }
        }
        // TEMP END
    }

    public void PlayerBuyPlant(BasePlant plant)
    {
        if (GameManager.instance.playerMoney >= plant.StorePrice)
        {
            GameManager.instance.playerMoney -= plant.StorePrice;
            GameManager.instance.plantBeingPlanted = Instantiate(plant);
            var sr = GameManager.instance.plantBeingPlanted.GetComponent<SpriteRenderer>();
            Color tmp = sr.color;
            tmp.a = 0.25f;
            GameManager.instance.plantBeingPlanted.GetComponent<SpriteRenderer>().color = tmp;

            // TODO: Show player they paid for the plant.
            Debug.Log("Bought " + plant.ToString() + " for " + plant.StorePrice);
        } else
        {
            // TODO: Show player they can't afford the plant.
            Debug.Log("Too poor to buy " + plant.ToString() + " for " + plant.StorePrice);
        }
    }
 
    [Tooltip("The plants for sale in the store.")]
    public List<BasePlant> plantsForSale;
}

