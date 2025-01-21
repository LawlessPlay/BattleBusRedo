using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TacticsToolkit;

public class CharacterTurnManager : MonoBehaviour
{
    [SerializeField] private bool hasMoved;
    [SerializeField] private bool hasUsedAbility;

    [SerializeField] private OverlayManagerV2 overlayManager;
    [SerializeField] private Entity activeCharacter;
    [SerializeField] private OverlayTile activeTile;
    [SerializeField] private PathFinder pathFinder;
    [SerializeField] private RangeFinder rangeFinder;
    [SerializeField] private List<Action> actionQueue;

    [SerializeField] private LineRenderer lineRenderer;

    public GameObject nodePrefab; // Assign your prefab in the Inspector
    private List<GameObject> instantiatedNodes;

    public GameEvent endTurn;
    public float lineOffset = 0.5f;

    private bool isActive;
    private List<Vector3> currentPath;

    private void Start()
    {
        overlayManager = GetComponent<OverlayManagerV2>();
        lineRenderer = GetComponent<LineRenderer>();

        rangeFinder = new RangeFinder();
        pathFinder = new PathFinder();

        instantiatedNodes = new List<GameObject>();
        actionQueue = new List<Action>();
        currentPath = new List<Vector3>();
    }


    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && actionQueue.Count > 0)
        {
            overlayManager.ClearTiles();
            lineRenderer.positionCount = 0;
            isActive = true;
            actionQueue[0].StartAction();
        }

        HandleInProgressAction();
        CleanupFinishedActions();
    }

    private void HandleInProgressAction()
    {
        if (actionQueue != null)
        {
            var inProgressAction = actionQueue.FirstOrDefault(x => x.State == Action.ActionState.InProgress && x.Type == Action.ActionType.Move);
            inProgressAction?.DoAction();
        }
    }

    private void CleanupFinishedActions()
    {
        if (!actionQueue.Any(x => x.State == Action.ActionState.Finished)) return;

        actionQueue.RemoveAll(x => x.State == Action.ActionState.Finished);

        if (actionQueue.Count > 0)
        {
            actionQueue[0].StartAction();
        }
        else
        {
            isActive = false;
            endTurn.Raise();
        }
    }

    public void StartTurn()
    {
        //overlayManager.ShowTotalOverlay();
        activeCharacter.SetRenderers(lineRenderer, overlayManager);
        
        actionQueue = activeCharacter.StartTurn();
    }

    public void SetActiveCharacter(GameObject character)
    {
        activeCharacter = character.GetComponent<Entity>();
        overlayManager.SetActiveCharacter(activeCharacter);
        StartTurn();
    }

    public void SetActiveTile(GameObject tile)
    {
        if (activeCharacter == null || actionQueue.Any(x => x.State == Action.ActionState.InProgress)) return;

        actionQueue.Clear();
        overlayManager.SetActiveTile(tile.GetComponent<OverlayTile>());

        activeTile = tile.GetComponent<OverlayTile>();
        
        actionQueue = activeCharacter.SetActiveTile(activeTile);
    }

    private void HandleActiveTileWithCharacter()
    {
        var movementTiles = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange + activeCharacter.SelfSpell.range);
        var path = pathFinder.FindPath(activeCharacter.activeTile, activeTile, movementTiles.Item1);

        if (activeTile.activeCharacter == activeCharacter)
        {
            DisplaySpell(path, activeCharacter.SelfSpell);
        }
        else if (activeTile.activeCharacter.teamID == activeCharacter.teamID)
        {
            
            DisplaySpell(path, activeCharacter.AllySpell);
        }
        else
        {
            DisplaySpell(path, activeCharacter.EnemySpell);
        }
    }

    private void HandleActiveTileWithoutCharacter()
    {
        //overlayManager.SetActiveTile(activeTile);

        if (isActive) return;

        var movementTiles = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange);
        var path = pathFinder.FindPath(activeCharacter.activeTile, activeTile, movementTiles.Item1);

        if (path.Count <= 0) return;

        var pathPositions = GeneratePathPositions(path);
        var turnNodes = GetTurnNodes(pathPositions);

        UpdateLineRenderer(turnNodes.Count > 0 ? turnNodes : pathPositions);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, AdjustPosition(activeTile.transform.position, lineOffset));

        UpdateCurrentPath();

        actionQueue.Add(new Action(currentPath, Action.ActionType.Move, activeTile, activeCharacter));
    }

    private void UpdateCurrentPath()
    {
        currentPath.Clear();

        for (var i = 0; i < lineRenderer.positionCount; i++)
        {
            var position = lineRenderer.GetPosition(i);
            currentPath.Add(new Vector3(position.x, position.y - lineOffset, position.z));
        }
    }

    private void DisplaySpell(List<OverlayTile> path, Ability ability)
    {
        overlayManager.DrawSpell(activeTile, ability);
        if (ability.range <= path.Count)
        {
            path.RemoveRange(path.Count - ability.range + 1, ability.range - 1);

            var pathPositions = GeneratePathPositions(path);

            UpdateLineRenderer(pathPositions);
            UpdateCurrentPath();

            lineRenderer.positionCount--;

            var posCount = lineRenderer.positionCount;
            
            var lastPosition = new Vector3(currentPath.Last().x, currentPath.Last().y + lineOffset, currentPath.Last().z);
            AddArcToLineRenderer(lastPosition, activeTile.transform.position, ref posCount, 2);

            actionQueue.Add(new Action(currentPath, Action.ActionType.Move, MapManager.Instance.GetOverlayByTransform(currentPath.Last()), activeCharacter));
            actionQueue.Add(new Action(currentPath, Action.ActionType.Attack, activeTile, activeCharacter, ability));
        }
        else
        {
            ResetLineRenderer();

            var posCount = lineRenderer.positionCount;
            AddArcToLineRenderer(activeCharacter.activeTile.transform.position, activeTile.transform.position, ref posCount, 2);

            actionQueue.Add(new Action(currentPath, Action.ActionType.Attack, activeTile, activeCharacter, ability));
        }
    }

    private void ResetLineRenderer()
    {
        currentPath.Clear();
        lineRenderer.positionCount = 0;
    }

    private List<Vector3> GeneratePathPositions(List<OverlayTile> path)
    {
        var positions = new List<Vector3> { AdjustPosition(activeCharacter.activeTile.transform.position, lineOffset) };
        positions.AddRange(path.Select(tile => AdjustPosition(tile.transform.position, lineOffset)));
        return positions;
    }

    private Vector3 AdjustPosition(Vector3 position, float yOffset)
    {
        return new Vector3(position.x, position.y + yOffset, position.z);
    }

    private void UpdateLineRenderer(List<Vector3> positions)
    {
        lineRenderer.positionCount = 0;
        var lineCount = 0;

        for (var i = 0; i < positions.Count - 1; i++)
        {
            var from = positions[i];
            var to = positions[i + 1];

            if (!Mathf.Approximately(from.y, to.y))
            {
                AddArcToLineRenderer(from, to, ref lineCount);
            }
            else
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineCount++, from);
            }
        }
    }

    private void AddArcToLineRenderer(Vector3 from, Vector3 to, ref int lineCount, float arcHeight = -1)
    {
        const int arcSegments = 10;
        arcHeight = arcHeight == -1 ? Mathf.Abs(from.y - to.y) : arcHeight;

        lineRenderer.positionCount += arcSegments;

        for (int j = 0; j < arcSegments; j++)
        {
            float t = j / (float)arcSegments;
            var interpolated = Vector3.Lerp(from, to, t);
            interpolated.y += arcHeight * Mathf.Sin(Mathf.PI * t);

            lineRenderer.SetPosition(lineCount++, interpolated);
        }
    }

    private static List<Vector3> GetTurnNodes(List<Vector3> positions)
    {
        if (positions == null || positions.Count < 3) return new List<Vector3>();

        var turnNodes = new List<Vector3> { positions[0] };

        for (int i = 1; i < positions.Count - 1; i++)
        {
            var prevDirection = (positions[i] - positions[i - 1]).normalized;
            var nextDirection = (positions[i + 1] - positions[i]).normalized;

            if (!prevDirection.Equals(nextDirection))
            {
                turnNodes.Add(positions[i]);
            }
        }

        turnNodes.Add(positions[^1]);
        return turnNodes;
    }

    public void AddPrefabsToLineRenderer(List<Vector3> positions)
    {
        foreach (var node in instantiatedNodes)
        {
            Destroy(node);
        }
        instantiatedNodes.Clear();

        foreach (var position in positions)
        {
            var nodeInstance = Instantiate(nodePrefab, position, Quaternion.identity);
            instantiatedNodes.Add(nodeInstance);
        }
    }

    public void TriggerActiveAction()
    {
        var activeAction = actionQueue.FirstOrDefault(x => x.State == Action.ActionState.InProgress && x.Type == Action.ActionType.Attack);
        activeAction?.DoAction();
    }
}
