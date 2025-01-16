using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class PlayNextAttack : MonoBehaviour
{
    public Animator nextAttack;
    public GameEvent attackUnit;
    public GameEvent endAttack;

    public void PlayAttack()
    {
        if (nextAttack)
        {
            nextAttack.Play("AttackFX");

            if(attackUnit)
                attackUnit.Raise();
        } else
        {
            //StopAttacking
            endAttack.Raise();
        }
    }
}
