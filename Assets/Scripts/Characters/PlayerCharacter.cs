using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit 
{
    // Script for a playable character.
    public class CharacterManager : Entity
    { 
        private PathFinder pathFinder;
        private PathRenderer pathRenderer;
        private RangeFinder rangeFinder;
        private LineRenderer lineRenderer;
        private const float LineOffset = 0.5f;
        private List<Vector3> linePositions = new();

        public int ActionInitiativeValue = 0;
        
        private void Start()
        {
            rangeFinder = new RangeFinder();
            pathFinder = new PathFinder();
            pathRenderer = new PathRenderer(LineOffset);

            teamID = TeamType.Player;
        }

        private void Update()
        {
            if (actionQueue.Count == 0) return;
            
            
            if (actionQueue[0].State == Action.ActionState.InProgress && actionQueue[0].Type == Action.ActionType.Move)
                actionQueue[0].DoAction();
            
            if (actionQueue[0].State != Action.ActionState.Finished) return;
            
            actionQueue.RemoveAt(0);
            if (actionQueue.Count > 0)
            {
                hasAttacked = true;
                actionQueue[0].StartAction();
            }
            else
            {
                isActing = false;
                if (hasMoved && hasAttacked)
                {
                    EndTurn();
                    return;
                }

                if (hasMoved)
                {
                    OverlayManagerV2.Instance.ShowAttackOverlay();
                }
                
                
                if (hasAttacked)
                {
                    OverlayManagerV2.Instance.ShowMovementOverlay();
                }
            }
        }

        public override void ActionButtonPressed()
        {
            if (actionQueue.Count == 0) return;
            
            isActing = true;
            lineRenderer.positionCount = 0;
            OverlayManagerV2.Instance.ClearTiles();
            actionQueue[0].StartAction();

            if (actionQueue[0].Type == Action.ActionType.Move)
            {
                hasMoved = true;
            }
                
            if (actionQueue[0].Type == Action.ActionType.Attack)
            {
                hasAttacked = true;
            }
        }

        public override void SetRenderers(LineRenderer lineRenderer)
        {
            this.lineRenderer ??= lineRenderer;
        }

        public override void SetActiveTile(OverlayTile targetTile)
        {
            if (isActing) return;
            
            actionQueue = targetTile.activeCharacter != null 
                ? HandleActiveTileWithCharacter(targetTile) 
                : HandleActiveTileWithoutCharacter(targetTile);
        }

        private List<Action> HandleActiveTileWithCharacter(OverlayTile targetTile)
        {
            if (hasAttacked)
                return new List<Action>();
                
            setTarget.Raise(targetTile.gameObject);
            
            if (targetTile.activeCharacter == this)
                return DisplaySpell(new List<OverlayTile>(), SelfSpell, targetTile);
            
            if (targetTile.activeCharacter.teamID == teamID)
            {
                var path = pathFinder.FindPath(activeTile, targetTile,new List<OverlayTile>());
                return DisplaySpell(path, AllySpell, targetTile);
            }
            
            var enemyPath = pathFinder.FindPath(activeTile, targetTile,new List<OverlayTile>());
            var movementPath = enemyPath.Take(GetStat(Stats.MoveRange).statValue).ToList();
            return DisplaySpell(movementPath, EnemySpell, targetTile);
        }

        private List<Action> HandleActiveTileWithoutCharacter(OverlayTile targetTile)
        {
            setTarget.Raise(targetTile.gameObject);
            switch (hasMoved)
            {
                case false:
                {
                    var movementTiles = rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange);
                    var path = pathFinder.FindPath(activeTile, targetTile, movementTiles.Item1);

                    if (path.Count == 0) return new List<Action>();

                    var finalPath = pathRenderer.GeneratePath(path, activeTile.transform.position, -1);
                    RenderPath(finalPath);

                    return new List<Action> { new(finalPath, Action.ActionType.Move, targetTile, this) };
                }
                case true:
                {
                    return DisplaySpell(new List<OverlayTile>(), EnemySpell, targetTile);
                }
                default:
                    return new List<Action>();
            }
        }

        private void RenderPath(List<Vector3> path)
        {
            var adjustedPath = AdjustPath(path);
            lineRenderer.positionCount = adjustedPath.Count;
            
            for (int i = 0; i < adjustedPath.Count; i++)
            {
                lineRenderer.SetPosition(i, adjustedPath[i]);
            }
        }

        private List<Vector3> AdjustPath(List<Vector3> positions)
        {
            return positions.Select(position => new Vector3(position.x, position.y + LineOffset, position.z)).ToList();
        }

        private List<Action> DisplaySpell(List<OverlayTile> movementPath, Ability ability, OverlayTile targetTile)
        {
            var tempActionQueue = new List<Action>();
            OverlayManagerV2.Instance.DrawSpell(targetTile, ability);

            if(hasMoved)
                movementPath = new List<OverlayTile>();


            //if starting tile is in range, attack
            if (pathFinder.GetManhattenDistance(activeTile, targetTile, true) <= ability.range)
            {
                ResetLineRenderer();
                AddArcToLineRenderer(activeTile.transform.position, targetTile.transform.position, 2);
                tempActionQueue.Add(new Action(linePositions, Action.ActionType.Attack, targetTile, this, ability));
                return tempActionQueue;
            }

            for (int i = 0; i < movementPath.Count; i++)
            {
                  if (pathFinder.GetManhattenDistance(movementPath[i],targetTile, true) < ability.range)
                  {
                      movementPath = movementPath.Take(i).ToList();
                      var finalPath = pathRenderer.GeneratePath(movementPath, activeTile.transform.position, -1);
                      RenderPath(finalPath);
                      lineRenderer.positionCount--;
                
                      var lastPosition = new Vector3(finalPath.Last().x, finalPath.Last().y + LineOffset, finalPath.Last().z);
                      AddArcToLineRenderer(lastPosition, targetTile.transform.position, 2);
                
                      tempActionQueue.Add(new Action(finalPath, Action.ActionType.Move, MapManager.Instance.GetOverlayByTransform(finalPath.Last()), this));
                      tempActionQueue.Add(new Action(finalPath, Action.ActionType.Attack, targetTile, this, ability));
                      return tempActionQueue;
                  }
            }

            return tempActionQueue;
        }

        private void ResetLineRenderer()
        {
            linePositions.Clear();
            lineRenderer.positionCount = 0;
        }

        private void AddArcToLineRenderer(Vector3 from, Vector3 to, float arcHeight = -1)
        {
            const int ArcSegments = 10;
            arcHeight = arcHeight == -1 ? Mathf.Abs(from.y - to.y) : arcHeight;

            lineRenderer.positionCount += ArcSegments;
            
            for (int j = 0; j < ArcSegments; j++)
            {
                float t = j / (float)ArcSegments;
                var interpolated = Vector3.Lerp(from, to, t);
                interpolated.y += arcHeight * Mathf.Sin(Mathf.PI * t);
                lineRenderer.SetPosition(lineRenderer.positionCount - ArcSegments + j, interpolated);
            }
        }
        
        public override void TriggerNextAction()
        {
            if (actionQueue.Count <= 0) return;
                actionQueue[0].DoAction();
        }
        
        public override void StartTurn()
        {
            StartCoroutine(StartTurnRoutine());
        }

        IEnumerator StartTurnRoutine()
        {
            base.StartTurn();
            yield return new WaitForSeconds(1);
            OverlayManagerV2.Instance.ShowTotalOverlay();
        }
    }
}
