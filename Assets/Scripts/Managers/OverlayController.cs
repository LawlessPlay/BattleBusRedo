using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    //Handles the colouring of tiles. 
    public class OverlayController : MonoBehaviour
    {
        private static OverlayController _instance;
        public static OverlayController Instance { get { return _instance; } }

        public Dictionary<Color, List<OverlayTile>> coloredTiles;
        public GameConfig gameConfig;

        //So all the other files don't need the gameConfig.
        public Color AttackRangeColor;
        public Color MoveRangeColor;
        public Color BlockedTileColor;
        
        public Color EnemyColor;
        public Color AllyColor;
        public Color SelfColor;
        
        public enum TileColors
        {
            MovementColor,
            AttackRangeColor,
            AttackColor
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            coloredTiles = new Dictionary<Color, List<OverlayTile>>();
            MoveRangeColor = gameConfig.MoveRangeColor;
            AttackRangeColor = gameConfig.AttackRangeColor;
            BlockedTileColor = gameConfig.BlockedTileColor;
            EnemyColor = gameConfig.EnemyColor;
            AllyColor = gameConfig.AllyColor;
            SelfColor = gameConfig.SelfColor;
        }

        //Remove colours from all tiles. 
        public void ClearTiles(Color? color = null)
        {
            if (color.HasValue)
            {
                if (coloredTiles.ContainsKey(color.Value))
                {
                    var tiles = coloredTiles[color.Value];
                    coloredTiles.Remove(color.Value);
                    foreach (var coloredTile in tiles)
                    {
                        coloredTile.HideTile();

                        foreach (var usedColors in coloredTiles.Keys)
                        {
                            foreach (var usedTile in coloredTiles[usedColors])
                            {
                                if (coloredTile.grid2DLocation == usedTile.grid2DLocation)
                                {
                                    coloredTile.ShowTile(usedColors);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in coloredTiles.Keys)
                {
                    foreach (var colouredTile in coloredTiles[item])
                    {
                        colouredTile.HideTile();
                    }
                }

                coloredTiles.Clear();
            }
        }

        //Color tiles to specific color
        public void ColorTiles(Color color, List<OverlayTile> overlayTiles, List<OverlayTile> characterTiles = null, Entity activeCharacter = null)
        {
            ClearTiles(color);
            foreach (var tile in overlayTiles)
            {
                tile.ShowTile(color);

                if (tile.isBlocked)
                    tile.ShowTile(BlockedTileColor);
            }

            if (characterTiles != null)
            {
                foreach (var tile in characterTiles)
                {
                    if (activeCharacter)
                    {
                        if (tile.activeCharacter == activeCharacter)
                        {
                            tile.ShowTile(SelfColor);
                        }
                        else if (tile.activeCharacter.teamID == activeCharacter.teamID)
                        {
                            tile.ShowTile(AllyColor);
                        }
                        else
                        {
                            tile.ShowTile(EnemyColor);
                        }
                    }
                    else
                    {
                        tile.ShowTile(EnemyColor);
                    }

                }
            }

            coloredTiles.Add(color, overlayTiles);
        }

        //Color only one tile. 
        public void ColorSingleTile(Color color, OverlayTile tile)
        {
            //ClearTiles(color);
            tile.ShowTile(color);

            if (tile.isBlocked)
                tile.ShowTile(BlockedTileColor);


            var list = new List<OverlayTile>();
            list.Add(tile);

            if (!coloredTiles.ContainsKey(color))
                coloredTiles.Add(color, list);
            else
                coloredTiles[color].AddRange(list);

        }
    }
}
