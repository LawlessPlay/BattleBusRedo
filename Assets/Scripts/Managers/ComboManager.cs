using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public List<Combo> combos;
    private Dictionary<string, List<GameObject>> comboList = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, List<GameObject>> comboFlippedList = new Dictionary<string, List<GameObject>>();
    public Vector2 currentFacingDirection = Vector2.zero;
    public CharacterAnimationController animationController;
    public GameEvent attackUnit;
    public GameEvent endAttack;

    // Start is called before the first frame update
    void Start()
    {
        animationController = GetComponent<CharacterAnimationController>();

        if (animationController)
            currentFacingDirection = animationController.cameraDirection;

        foreach (Combo combo in combos)
        {
            List<GameObject> comboAttacks = new List<GameObject>();
            List<GameObject> comboFlippedAttacks = new List<GameObject>();
            for (int i = 0; i < combo.attacks.Length; i++)
            {
                comboAttacks.Add(Instantiate(combo.attacks[i], transform.parent));
                comboFlippedAttacks.Add(Instantiate(combo.flippedAttacks[i], transform.parent));
            }

            comboList.Add(combo.name, comboAttacks);
            comboFlippedList.Add(combo.name, comboFlippedAttacks);
        }
    }

    public void PlayAttackFX(string key)
    {
        if (combos.Count == 0)
        {
            attackUnit.Raise();
            endAttack.Raise();
        }
        else
        {
            attackUnit.Raise();
            endAttack.Raise();
            if (animationController)
            {
                var direction = animationController.cameraDirection;

                //right
                if (direction == new Vector2(0, -1) || direction == new Vector2(1, 0))
                {
                    comboList[key][0].GetComponent<Animator>().Play("AttackFX");
                }

                //Left
                if (direction == new Vector2(0, 1) || direction == new Vector2(-1, 0))
                {
                    comboFlippedList[key][0].GetComponent<Animator>().Play("AttackFX");
                }
            }
            else
            {
                comboList[key][0].GetComponent<Animator>().Play("AttackFX");
            }
        }

    }
}
