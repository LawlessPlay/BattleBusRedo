using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TacticsToolkit
{
    public class MapManager : MonoBehaviour
    {
        public OverlayTile overlayTilePrefab;
        public GameObject overlayContainer;
        public TileDataRuntimeSet tileTypeList;

        public Tilemap tilemap;

        private Entity activeCharacter;
        public Dictionary<TileBase, TileData> dataFromTiles = new();
        public Dictionary<Vector2Int, OverlayTile> map;

        public MapBounds mapBounds;

        public static MapManager Instance { get; private set; }

        public void Awake()
        {
            mapBounds = new MapBounds();
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
            SetMap();
        }

        public void Start()
        {
        }

        public virtual void SetMap()
        {
            if (tileTypeList)
                foreach (var tileData in tileTypeList.items)
                foreach (var item in tileData.baseTiles)
                    dataFromTiles.Add(item, tileData);

            tilemap = gameObject.GetComponentInChildren<Tilemap>();
            map = new Dictionary<Vector2Int, OverlayTile>();
            var bounds = tilemap.cellBounds;

            mapBounds = new MapBounds(bounds.xMax, bounds.yMax, bounds.xMin, bounds.yMin);

            //loop through the tilemap and create all the overlay tiles
            for (var z = bounds.max.z; z >= bounds.min.z; z--)
            for (var y = bounds.min.y; y < bounds.max.y; y++)
            for (var x = bounds.min.x; x < bounds.max.x; x++)
            {
                var tileLocation = new Vector3Int(x, y, z);
                var tileKey = new Vector2Int(x, y);
                if (tilemap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                {
                    var overlayTile = Instantiate(overlayTilePrefab, overlayContainer.transform);
                    var cellWorldPosition = tilemap.GetCellCenterWorld(tileLocation);
                    var baseTile = tilemap.GetTile(tileLocation);
                    overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y,
                        cellWorldPosition.z + 1);
                    overlayTile.GetComponent<SpriteRenderer>().sortingOrder =
                        tilemap.GetComponent<TilemapRenderer>().sortingOrder;
                    overlayTile.gridLocation = tileLocation;

                    if (dataFromTiles.ContainsKey(baseTile))
                    {
                        overlayTile.tileData = dataFromTiles[baseTile];
                        if (dataFromTiles[baseTile].type == TileTypes.NonTraversable)
                            overlayTile.isBlocked = true;
                    }

                    map.Add(tileKey, overlayTile);
                }
            }
        }

        public void SetActiveCharacter(GameObject activeCharacter)
        {
            this.activeCharacter = activeCharacter.GetComponent<Entity>();
        }

        //Get all tiles next to a tile
        public List<OverlayTile> GetNeighbourTiles(
            OverlayTile currentOverlayTile,
            List<OverlayTile> searchableTiles,
            bool ignoreObstacles = false,
            bool walkThroughAllies = true,
            int remainingRange = 10
        )
        {
            var tileToSearch = new Dictionary<Vector2Int, OverlayTile>();

            if (searchableTiles.Count > 0)
                foreach (var item in searchableTiles)
                    tileToSearch.Add(item.grid2DLocation, item);
            else
                tileToSearch = map;

            var neighbours = new List<OverlayTile>();
            if (currentOverlayTile)
                foreach (var direction in GetDirections())
                {
                    var locationToCheck = currentOverlayTile.grid2DLocation + direction;
                    ValidateNeighbour(currentOverlayTile, ignoreObstacles, walkThroughAllies, tileToSearch, neighbours,
                        locationToCheck, remainingRange);
                }

            return neighbours;
        }

        //Check the neighbouring tile is valid.
        private static void ValidateNeighbour(OverlayTile currentOverlayTile, bool ignoreObstacles,
            bool walkThroughAllies, Dictionary<Vector2Int, OverlayTile> tilesToSearch, List<OverlayTile> neighbours,
            Vector2Int locationToCheck, int remainingRange)
        {
            var canAccessLocation = false;

            if (tilesToSearch.ContainsKey(locationToCheck))
            {
                var tile = tilesToSearch[locationToCheck];
                var isBlocked = tile.isBlocked;
                var isActiveCharacter = tile.activeCharacter && Instance.activeCharacter;
                var isSameTeam = isActiveCharacter && tile.activeCharacter.teamID == Instance.activeCharacter.teamID;
                var canWalkThroughAllies = walkThroughAllies && isSameTeam;

                if (ignoreObstacles || (!isBlocked) || canWalkThroughAllies)
                    if (tile.GetMoveCost() <= remainingRange || ignoreObstacles)
                        canAccessLocation = true;

                if (!canAccessLocation) return;
                
                //artificial jump height. 
                if (tile.transform.position.y - currentOverlayTile.transform.position.y <= 2.5)
                    neighbours.Add(tilesToSearch[locationToCheck]);
            }
        }

        private IEnumerable<Vector2Int> GetDirections()
        {
            yield return Vector2Int.up;
            yield return Vector2Int.down;
            yield return Vector2Int.right;
            yield return Vector2Int.left;
        }

        //Hide all overlayTiles currently being shown.
        public void ClearTiles()
        {
            foreach (var item in map.Values) item.HideTile();
        }

        //Get a tile by world position. 
        public virtual OverlayTile GetOverlayByTransform(Vector3 position)
        {
            var gridLocation = tilemap.WorldToCell(position);
            if (map.ContainsKey(new Vector2Int(gridLocation.x, gridLocation.y)))
                return map[new Vector2Int(gridLocation.x, gridLocation.y)];

            return null;
        }

        //Get list of overlay tiles by grid positions. 
        public List<OverlayTile> GetOverlayTilesFromGridPositions(List<Vector2Int> positions)
        {
            var overlayTiles = new List<OverlayTile>();

            if (map != null)
                foreach (var item in positions)
                    overlayTiles.Add(map[item]);

            return overlayTiles;
        }

        //Get overlay tile by grid position. 
        public OverlayTile GetOverlayTileFromGridPosition(Vector2Int position)
        {
            return map[position];
        }
    }

    public class MapBounds
    {
        public int xMax;
        public int xMin;
        public int yMax;
        public int yMin;

        public MapBounds(int xMax, int yMax, int xMin, int yMin)
        {
            this.xMax = xMax;
            this.yMax = yMax;
            this.xMin = xMin;
            this.yMin = yMin;
        }

        public MapBounds()
        {
        }
    }
}