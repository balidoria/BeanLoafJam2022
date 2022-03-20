using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Coin : MonoBehaviour
{
    public Transform CoinTarget;
    public AnimationCurve movementCurvex;
    public AnimationCurve movementCurvey;

    public float duration = 1;
    [SerializeField]
    private float currentTime = 0;
    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
      
        if (currentTime < duration)
        {

            Vector3 newPosition = new Vector3(
                Mathf.Lerp(startPos.x, CoinTarget.position.x, movementCurvex.Evaluate(currentTime / duration)),
                Mathf.Lerp(startPos.y, CoinTarget.position.y, movementCurvey.Evaluate(currentTime / duration)),
                0);
            this.transform.position = newPosition;
            currentTime += Time.deltaTime;
        }
        else if (currentTime > duration)
        {
            Destroy(this.gameObject);
        }
    }
}
