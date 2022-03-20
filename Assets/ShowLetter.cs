using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLetter : MonoBehaviour
{
    private bool isOpen = false;
    public GameObject credits;

    // Start is called before the first frame update
  /*  void Awake(){
        letter = gameObject.GetComponent<SpriteRenderer>();
        showLetterButton = gameObject.GetComponent<Button>();
        playButton = gameObject.GetComponent<Button>();
    }*/

    public void ToggleCredits()
    {
        isOpen = !isOpen;
        credits.SetActive(isOpen);
    }
}
