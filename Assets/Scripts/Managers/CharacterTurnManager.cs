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

    private bool isActive;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        activeCharacters = new List<Entity>();
        activeCharacters = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Entity>()).ToList();
        activeCharacters.AddRange(GameObject.FindGameObjectsWithTag("Enemy").Select(x => x.GetComponent<Entity>()).ToList());
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
