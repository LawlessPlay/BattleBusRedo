using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    //A tile object that is used for spawning characters on a specific location.
    public class SpawnTile : MonoBehaviour
    {
        public Vector3Int gridLocation = new Vector3Int(-100, -100, -100);
        public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }

        private void Update()
        {
            if (gridLocation == new Vector3Int(-100, -100, -100))
                GetGridLocation();
        }

        public void GetGridLocation()
        {
            var closestTile = MapManager.Instance.GetOverlayByTransform(transform.position);
            gridLocation = closestTile.gridLocation;
        }
    }
}