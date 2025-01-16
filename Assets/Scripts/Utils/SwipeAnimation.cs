using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeAnimation : MonoBehaviour
{
   public void AnimatiedSwipe()
    {
        Debug.Log("Swipe");
        gameObject.GetComponent<SpriteRenderer>().sprite = null;
        gameObject.SetActive(false);
    }
}
