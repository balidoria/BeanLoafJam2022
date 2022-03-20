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

    [Tooltip("The plants for sale in the store.")]
    public List<BasePlant> plantsForSale;


    public void BuyPlant(int plantNum)
    {
        // check to see if the index num of the requested plant exists
        if (plantNum < plantsForSale.Count)
            PlayerBuyPlant(plantsForSale[plantNum]);        
    }

    public void PlayerBuyPlant(BasePlant plant)
    {
        // check if we got cash to burn
        if (GameManager.instance.playerMoney >= plant.StorePrice && GameManager.instance.numOfActivePlants < 25)
        {
            // make sure we clean up any 'pending planting' plants
            if (GameManager.instance.plantBeingPlanted != null)
            {
                Destroy(GameManager.instance.plantBeingPlanted.gameObject);
                GameManager.instance.plantBeingPlanted = null;
            }

            //GameManager.instance.playerMoney -= plant.StorePrice;
            GameManager.instance.plantBeingPlanted = Instantiate(plant);
            var renderers = GameManager.instance.plantBeingPlanted.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers)
            {
                Color tmp = sr.color;
                tmp.a = 0.5f;
                sr.color = tmp;
            }

            // TODO: Show player they paid for the plant.
            Debug.Log("Bought " + plant.ToString() + " for " + plant.StorePrice);
        } else
        {
            // TODO: Show player they can't afford the plant.

            Debug.Log("Too poor to buy " + plant.ToString() + " for " + plant.StorePrice);
        }
    }

    public void PlantPlanted(BasePlant plant)
    {
        GameManager.instance.playerMoney -= plant.StorePrice;
    }
}
