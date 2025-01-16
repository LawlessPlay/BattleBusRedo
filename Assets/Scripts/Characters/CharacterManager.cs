
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace TacticsToolkit 
{
    
    //Script for a playable character.
    public class CharacterManager : Entity
    { 
        private OverlayManagerV2 overlayManager;
        private PathFinder pathFinder;
        private PathRenderer pathRenderer;
        private RangeFinder rangeFinder;
        private LineRenderer lineRenderer;
        private float lineOffset = 0.5f;
        
        private List<Vector3> linePositions = new List<Vector3>();
        
        public void Start()
        {
            rangeFinder = new RangeFinder();
            pathFinder = new PathFinder();
            pathRenderer = new PathRenderer(lineOffset);
        }

        public override void SetRenderers(LineRenderer lineRenderer, OverlayManagerV2 overlayManager)
        {
            if(this.lineRenderer == null)
                this.lineRenderer = lineRenderer;
        
            if(this.overlayManager == null)
                this.overlayManager = overlayManager;
        }

        public override List<Action> SetActiveTile(OverlayTile targetTile)
        {
            return targetTile.activeCharacter != null ? HandleActiveTileWithCharacter(targetTile) : HandleActiveTileWithoutCharacter(targetTile);
        }
        
        private List<Action> HandleActiveTileWithCharacter(OverlayTile targetTile)
        {
            var movementTiles = rangeFinder.GetTilesInRange(targetTile, characterClass.MoveRange + SelfSpell.range);
            var path = pathFinder.FindPath(activeTile, targetTile, movementTiles.Item1);

            var actionQueue = new List<Action>();
            if (targetTile.activeCharacter == this)
            {
                actionQueue = DisplaySpell(path, SelfSpell, targetTile);
            }
            else if (targetTile.activeCharacter.teamID == teamID)
            {
            
                actionQueue = DisplaySpell(path, AllySpell, targetTile);
            }
            else
            {
                actionQueue = DisplaySpell(path, EnemySpell, targetTile);
            }
            
            return actionQueue;
        }

        private List<Action> HandleActiveTileWithoutCharacter(OverlayTile targetTile)
        {
            var actionQueue = new List<Action>();

            var movementTiles = rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange);
            var path = pathFinder.FindPath(activeTile, targetTile, movementTiles.Item1);

            if (path.Count <= 0) return actionQueue;

            var finalPath = pathRenderer.GeneratePath(path, activeTile.transform.position, -1);

            var renderedPath = AdjustPath(finalPath);
            lineRenderer.positionCount = renderedPath.Count;
            
            for (int i = 0; i < renderedPath.Count; i++)
            {
                lineRenderer.SetPosition(i, renderedPath[i]);
            }
            
            actionQueue.Add(new Action(finalPath, Action.ActionType.Move, targetTile, this));
            return actionQueue;
        }
        private List<Vector3> AdjustPath(List<Vector3> positions)
        {
            var newPositions = new List<Vector3>();
            for (int i = 0; i < positions.Count; i++)
            {
                newPositions.Add(new Vector3(positions[i].x, positions[i].y + lineOffset, positions[i].z));
            }

            return newPositions;
        }
        
        private void UpdateCurrentPath()
        {
            linePositions.Clear();

            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                var position = lineRenderer.GetPosition(i);
                linePositions.Add(new Vector3(position.x, position.y - lineOffset, position.z));
            }
        }
        
        private List<Action> DisplaySpell(List<OverlayTile> path, Ability ability, OverlayTile targetTile)
        {
            var actionQueue = new List<Action>();
            overlayManager.DrawSpell(targetTile, ability);
            if (ability.range <= path.Count)
            {
                path.RemoveRange(path.Count - ability.range, ability.range);
                var finalPath = pathRenderer.GeneratePath(path, activeTile.transform.position, -1);

                var renderedPath = AdjustPath(finalPath);
                lineRenderer.positionCount = renderedPath.Count;
            
                for (int i = 0; i < renderedPath.Count; i++)
                {
                    lineRenderer.SetPosition(i, renderedPath[i]);
                }
                
                lineRenderer.positionCount--;

                var posCount = lineRenderer.positionCount;
                var lastPosition = new Vector3(finalPath.Last().x, finalPath.Last().y + lineOffset, finalPath.Last().z);
                AddArcToLineRenderer(lastPosition, targetTile.transform.position, ref posCount, 2);

                actionQueue.Add(new Action(finalPath, Action.ActionType.Move, MapManager.Instance.GetOverlayByTransform(finalPath.Last()), this));
                actionQueue.Add(new Action(finalPath, Action.ActionType.Attack, targetTile, this, ability));
            }
            else
            {
                ResetLineRenderer();

                var posCount = lineRenderer.positionCount;
                AddArcToLineRenderer(activeTile.transform.position, targetTile.transform.position, ref posCount, 2);

                actionQueue.Add(new Action(linePositions, Action.ActionType.Attack, targetTile, this, ability));
            }
            
            return actionQueue;
        }
        
        
    private void ResetLineRenderer()
    {
        linePositions.Clear();
        lineRenderer.positionCount = 0;
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
    }
}