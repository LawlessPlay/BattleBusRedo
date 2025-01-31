using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TacticsToolkit;

public class CharacterTurnManager : MonoBehaviour
{
    [SerializeField] private bool hasMoved;
    [SerializeField] private bool hasUsedAbility;

    [SerializeField] private Entity activeCharacter;
    [SerializeField] private OverlayTile activeTile;

    [SerializeField] private LineRenderer lineRenderer;

    private bool isActive;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
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

    public void SetActiveTile(GameObject tile)
    {
        if (activeCharacter == null) return;

        OverlayManagerV2.Instance.SetActiveTile(tile.GetComponent<OverlayTile>());
        activeTile = tile.GetComponent<OverlayTile>();
        activeCharacter.SetActiveTile(activeTile);
    }

    public void TriggerActiveAction()
    {
        activeCharacter.TriggerNextAction();
    }
}
