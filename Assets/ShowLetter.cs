using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLetter : MonoBehaviour
{
    private bool showLetter = false;
    public SpriteRenderer letter;
    public SpriteRenderer title;
    public GameObject showLetterButton;
    public GameObject playButton;

    // Start is called before the first frame update
  /*  void Awake(){
        letter = gameObject.GetComponent<SpriteRenderer>();
        showLetterButton = gameObject.GetComponent<Button>();
        playButton = gameObject.GetComponent<Button>();
    }*/

    public void ShowTheLetter()
    {
        if(!showLetter){
            letter.enabled = true;
            title.enabled = false;
            showLetter = true;
            showLetterButton.SetActive(false);
            playButton.SetActive(true);

        }
    }
}
