using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    public class PathRenderer
    {
        private readonly float lineOffset;

        public PathRenderer(float lineOffset = 0.5f)
        {
            this.lineOffset = lineOffset;
        }

        public List<Vector3> GeneratePath(List<OverlayTile> path, Vector3 startTilePosition, float height)
        {
            // Step 1: Generate initial path positions
            var positions = new List<Vector3>(){startTilePosition};
            positions.AddRange(path.Select(x => x.transform.position).ToList());

            // Step 2: Determine turn nodes
            var turnNodes = GetTurnNodes(positions);

            // Step 3: Add arcs for smooth transitions
            var finalPath = new List<Vector3>();
            AddArcsToPath(turnNodes.Count > 0 ? turnNodes : positions, finalPath, height);

            return finalPath;
        }

        private List<Vector3> GenerateLinePositions(List<OverlayTile> path, Vector3 startTilePosition)
        {
            var positions = new List<Vector3> { AdjustPosition(startTilePosition, lineOffset) };
            positions.AddRange(path.Select(tile => AdjustPosition(tile.transform.position, lineOffset)));
            return positions;
        }

        private Vector3 AdjustPosition(Vector3 position, float yOffset)
        {
            return new Vector3(position.x, position.y + yOffset, position.z);
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

        private void AddArcsToPath(List<Vector3> positions, List<Vector3> finalPath, float arcHeight)
        {
            for (int i = 0; i < positions.Count - 1; i++)
            {
                var from = positions[i];
                var to = positions[i + 1];

                if (!Mathf.Approximately(from.y, to.y))
                {
                    AddArc(from, to, finalPath, arcHeight);
                }
                else
                {
                    finalPath.Add(from);
                }
            }

            // Ensure the last point is included
            finalPath.Add(positions.Last());
        }

        private void AddArc(Vector3 from, Vector3 to, List<Vector3> finalPath, float arcHeight = -1)
        {
            const int arcSegments = 10;
            arcHeight = arcHeight == -1 ? Mathf.Abs(from.y - to.y) : arcHeight;

            for (int j = 0; j < arcSegments; j++)
            {
                float t = j / (float)arcSegments;
                var interpolated = Vector3.Lerp(from, to, t);
                interpolated.y += arcHeight * Mathf.Sin(Mathf.PI * t);

                finalPath.Add(interpolated);
            }
        }
    }
}
