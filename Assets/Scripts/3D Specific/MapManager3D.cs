using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace TacticsToolkit
{
    public class MapManager3D : MapManager
    {
        public TileDataRuntimeSet tileData3D;
        public GameObject enemyList;


        public bool MapSet = false;
        new void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            SetMap();
        }

        public override void SetMap()
        {
            if (MapSet)
            {
                return;
            }

            Tilemap[] childTilemaps = gameObject.GetComponentsInChildren<Tilemap>();
            map = new Dictionary<Vector2Int, OverlayTile>();

            mapBounds = new MapBounds();
            foreach (var tilemap in childTilemaps)
            {
                if(tilemap.GetComponentsInChildren<ScaleUp>().Any(x => !x.isReady))
                {
                    return;
                }

                foreach (Transform child in tilemap.transform)
                {
                    var gridLocation = tilemap.WorldToCell(child.position);
                    var tileKey = new Vector2Int(gridLocation.x, gridLocation.y);
                    var meshBounds = child.gameObject.GetComponent<MeshRenderer>().bounds;
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);

                    foreach (var tileData in tileTypeList.items)
                    {
                        foreach (var material in tileData.Tiles3D)
                        {
                            if (material == child.GetComponent<MeshRenderer>().sharedMaterial)
                            {
                                overlayTile.tileData = tileData;
                            }
                        }
                    }

                    overlayTile.transform.position = new Vector3(child.position.x,
                        child.position.y + meshBounds.extents.y + 0.0001f, child.position.z);
                    overlayTile.gridLocation = gridLocation;
                    SetMapBounds(gridLocation);
                    
                    if (!map.ContainsKey(tileKey))
                    {
                        map.Add(tileKey, overlayTile);
                    }
                    else
                    {
                        var tileToDestroy = map[tileKey];
                        if (overlayTile.gridLocation.z > tileToDestroy.gridLocation.z)
                        {
                            Destroy(tileToDestroy.gameObject);
                            map.Remove(tileKey);
                            map.Add(tileKey, overlayTile);
                        }
                        else
                        {
                            Destroy(overlayTile.gameObject);
                        }
                    }
                }
            }

            if(enemyList)
                PositionEnemies();


            MapSet = true;
        }

        void PositionEnemies()
        {
            int minX = 0;
            int minY = 0;
            int maxX = 0;
            int maxY = 0;

            maxX = map.Keys.Select(x => x.x).Max();
            maxY = map.Keys.Select(x => x.y).Max();

            minX = map.Keys.Select(x => x.x).Min();
            minY = map.Keys.Select(x => x.y).Min();

            foreach (Transform enemy in enemyList.transform)
            {
                int newX = Random.Range(minX, maxX);
                int newY = Random.Range(minY, maxY);

                while (map[new Vector2Int(newX, newY)].isBlocked)
                {
                    newX = Random.Range(minX, maxX);
                    newY = Random.Range(minY, maxY);
                }

                enemy.position = map[new Vector2Int(newX, newY)].transform.position;
                enemy.GetComponent<PositionOnGrid>().PositionEntityOnGrid();
                enemy.GetComponent<Entity>().LinkCharacterToTile(map[new Vector2Int(newX, newY)]);
            }
        }

        private void SetMapBounds(Vector3Int gridLocation)
        {
            if (mapBounds.yMin > gridLocation.y)
                mapBounds.yMin = gridLocation.y;

            if (mapBounds.yMax < gridLocation.y)
                mapBounds.yMax = gridLocation.y;

            if (mapBounds.xMin > gridLocation.x)
                mapBounds.xMin = gridLocation.x;

            if (mapBounds.xMax > gridLocation.x)
                mapBounds.xMax = gridLocation.x;
        }
    }
}