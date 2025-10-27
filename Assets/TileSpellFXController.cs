using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class TileSpellFXController : MonoBehaviour
{
    public Animator fxAnimatorOne;
    public Animator fxAnimatorTwo;

    public void TriggerFXAnimation(AnimatorController controller)
    {
        fxAnimatorOne.runtimeAnimatorController = controller;
        fxAnimatorTwo.runtimeAnimatorController = controller;
        
        fxAnimatorOne.Play(0);
        fxAnimatorTwo.Play(0);
    }
}
