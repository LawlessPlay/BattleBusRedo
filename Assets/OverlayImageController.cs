using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayImageController : MonoBehaviour
{
    public GameObject MovementImage;
    public GameObject AttackImage;
    public GameObject SupportImage;
    public GameObject HighlightImage;


    public void EnableMovementImage()
    {
        MovementImage.SetActive(true);
    }
    
    public void EnableAttackImage()
    {
        AttackImage.SetActive(true);
    }    
    
    public void EnableSupportImage()
    {
        SupportImage.SetActive(true);
    }
    
    public void EnableHighlightImage()
    {
        HighlightImage.SetActive(true);
    }
    
    public void DisableAll()
    {
        MovementImage.SetActive(false);
        AttackImage.SetActive(false);
        SupportImage.SetActive(false);
        HighlightImage.SetActive(false);
    }
    
    public void DisableMovementImage()
    {
        MovementImage.SetActive(false);
    }
    
    public void DisableAttackImage()
    {
        AttackImage.SetActive(false);
    }    
    
    public void DisableSupportImage()
    {
        SupportImage.SetActive(false);
    }
}
