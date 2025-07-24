using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TacticsToolkit;

public class CharacterTurnManager : MonoBehaviour
{
    
    public SpawnProjectilesScript spawnProjectiles;
    [SerializeField] private bool hasMoved;
    [SerializeField] private bool hasUsedAbility;
    [SerializeField] private List<Entity> activeCharacters;

    [SerializeField] private Entity activeCharacter;
    [SerializeField] private OverlayTile activeTile;

    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private GameObject confirmationUI;
    [SerializeField] private GameEventInt previewTurnOrder;
    
    public TurnBasedManagerRedo turnBasedManagerRedo;
    
        
    private bool isActive;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        activeCharacters = new List<Entity>();
        activeCharacters = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Entity>()).ToList();
        activeCharacters.AddRange(GameObject.FindGameObjectsWithTag("Enemy").Select(x => x.GetComponent<Entity>()).ToList());
    }
    
    public void ActiveButtonPressed()
    {
        if (activeCharacter && !activeCharacter.isActing && activeCharacter.actionQueue.Count > 0 && confirmationUI.activeSelf == false)
        {
            //check if updating speed. 
            foreach (var action in activeCharacter.actionQueue)
            {
                if (action.Type == Action.ActionType.Attack)
                {
                    foreach (var effect in action.Ability.effects)
                    {
                        if (effect.statKey == Stats.Speed)
                        {
                            action.Target.activeCharacter.ApplySingleEffects(effect);
                            turnBasedManagerRedo.PreviewUpdateOrder(action.Target.activeCharacter.gameObject, effect.Duration);
                        }
                    }
                }
            }
            
            confirmationUI.SetActive(true);
        }
    }

    public void ConfirmAction()
    {
        if (activeCharacter && confirmationUI.activeSelf == true)
        {
            confirmationUI.SetActive(false);
            activeCharacter.ActionButtonPressed();
            //turnBasedManagerRedo.ConfirmPreview();
        }
    }
    
    public void DeclineAction()
    {
        if (activeCharacter && confirmationUI.activeSelf == true)
        {
            confirmationUI.SetActive(false);
            turnBasedManagerRedo.UndoPreview();
        }
    }
    
    public void StartTurn()
    {
        activeCharacter.SetRenderers(lineRenderer);
        activeCharacter.StartTurn();
    }

    public void SetActiveCharacter(GameObject character)
    {
        activeCharacter = character.GetComponent<Entity>();
        OverlayManagerV2.Instance.SetActiveCharacter(activeCharacter);
        StartTurn();
    }

    public void SpawnCharacter(GameObject character)
    {
        activeCharacters.Add(character.GetComponent<Entity>());
    }

    public void SetActiveTile(GameObject tile)
    {
        foreach (var character in activeCharacters)
        {
           character.GetComponent<HealthBarManager>().HidePreview();
        }
        
        if (activeCharacter == null) return;

        OverlayManagerV2.Instance.SetActiveTile(tile.GetComponent<OverlayTile>());
        activeTile = tile.GetComponent<OverlayTile>();
        activeCharacter.SetActiveTile(activeTile);
    }

    public void TriggerActiveAction()
    {
        activeCharacter.TriggerNextAction();
    }
    
    public void TriggerAction()
    {
        activeCharacter.TriggerAction();
    }
}
