using System;
using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class OverlayManagerV2 : MonoBehaviour
{
    private Dictionary<OverlayTile, List<Color>> ColoredTiles;
    public GameConfig gameConfig;

    private RangeFinder rangeFinder;
    private ShapeParser shapeParser;
    
    [FormerlySerializedAs("ActiveCharacter")] [SerializeField]
    private Entity activeCharacter;
    
    [FormerlySerializedAs("ActiveTile")] [SerializeField]
    private OverlayTile activeTile;
    
    [SerializeField]
    public Color AttackRangeColor;
    [SerializeField]
    public Color MoveRangeColor;
    [SerializeField]
    public Color BlockedTileColor;
        
    [SerializeField]
    public Color EnemyColor;
    [SerializeField]
    public Color AllyColor;
    [SerializeField]
    public Color SelfColor;
    
    private void Awake()
    {
        ColoredTiles = new Dictionary<OverlayTile, List<Color>>();
        MoveRangeColor = gameConfig.MoveRangeColor;
        AttackRangeColor = gameConfig.AttackRangeColor;
        BlockedTileColor = gameConfig.BlockedTileColor;
        EnemyColor = gameConfig.EnemyColor;
        AllyColor = gameConfig.AllyColor;
        SelfColor = gameConfig.SelfColor;
        
        rangeFinder = new RangeFinder();
        shapeParser = new ShapeParser();
    }

    public void UpdateTileColors(OverlayTile tile, Color color)
    {
        if (ColoredTiles.ContainsKey(tile))
        {
            ColoredTiles[tile].Add(color);
        }
        else
        {
            ColoredTiles.Add(tile, new List<Color>() { color });
        }
    }
    
    public void ShowTotalOverlay()
    {
        var movementTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange);
        var attackTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange + activeCharacter.characterClass.AttackRange);
        
        foreach (var tile in attackTilesToShow.Item1)
        {
            if (movementTilesToShow.Item1.Contains(tile))
            {
                UpdateTileColors(tile, MoveRangeColor);
            }
            else
            {
                UpdateTileColors(tile, AttackRangeColor);
            }

            ShowTile(tile);
        }

        ShowCharacterTiles(attackTilesToShow.Item2);
    }
    
    public void ShowMovementOverlay()
    {
        var movementTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange);
        var attackTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange + activeCharacter.characterClass.AttackRange);
        
        foreach (var tile in movementTilesToShow.Item1)
        {
            UpdateTileColors(tile, MoveRangeColor);
            ShowTile(tile);
        }

        ShowCharacterTiles(attackTilesToShow.Item2);
    }
    
    private void ShowAttackOverlay(OverlayTile originTile)
    {
        if (!activeCharacter) return;
        var attackTilesToShow = rangeFinder.GetTilesInRange(originTile, activeCharacter.SelfSpell.range, true, true);

        foreach (var tile in attackTilesToShow.Item1)
        {
            UpdateTileColors(tile, AttackRangeColor);
            ShowTile(tile);
        }

        
        ShowCharacterTiles(attackTilesToShow.Item2);
    }

    private void ShowCharacterTiles(List<OverlayTile> tiles)
    {
        foreach (var overlayTile in tiles)
        {
            if (overlayTile.activeCharacter == activeCharacter)
            {
                UpdateTileColors(overlayTile, SelfColor);
                continue;
            }

            UpdateTileColors(overlayTile, overlayTile.activeCharacter.teamID != activeCharacter.teamID ? EnemyColor : AllyColor);
            ShowTile(overlayTile);
        }
    }

    private void ShowTile(OverlayTile tile)
    {
        var tileColors = ColoredTiles[tile];
        tile.ShowTile(CombineColors(tileColors));
    }
    
    public static Color CombineColors(Color color1, Color color2)
    {
        // Blend the two colors equally
        return Color.Lerp(color1, color2, 0.5f);
    }
    
    public static Color CombineColors(List<Color> colors)
    {
        if (colors == null || colors.Count == 0)
        {
            throw new ArgumentException("The colors list must not be null or empty.");
        }

        // Start with the first color
        Color combinedColor = colors[0];

        // Gradually blend with the remaining colors
        for (var i = 1; i < colors.Count; i++)
        {
            var t = 1f / (i + 1); // Adjust interpolation factor
            combinedColor = Color.Lerp(combinedColor, colors[i], t);
        }

        return combinedColor;
    }

    public void SetActiveCharacter(Entity character)
    {
        activeCharacter = character;
    }

    public void SetActiveTile(OverlayTile activeTile)
    {
        this.activeTile = activeTile;
        
        if (!ColoredTiles.ContainsKey(activeTile) ||
            !ColoredTiles[activeTile].Contains(MoveRangeColor)) return;
        
       
        ClearColor(AllyColor);
    }

    public void ClearTiles()
    {
        foreach (var tile in ColoredTiles.Keys)
        {
            tile.HideTile();
        }
        
        ColoredTiles.Clear();
    }
    
    public void ClearColor(Color color)
    {
        foreach (var tile in ColoredTiles.Keys)
        {
            ColoredTiles[tile].Remove(color);

            if (ColoredTiles[tile].Count == 0)
            {
                tile.HideTile();
            }
            else
            {
                ShowTile(tile);
            }
        }
    }
    
    public void ClearTileColor(OverlayTile tile, Color color)
    {
        var tileColors = ColoredTiles[tile];

        foreach (var tileColor in tileColors.ToList().Where(tileColor => tileColor == color))
        {
            tileColors.Remove(tileColor);
        }
        
        ColoredTiles[tile] = tileColors;
    }

    public void DrawSpell(OverlayTile overlayTile, Ability ability)
    {
        var tiles = shapeParser.GetAbilityTileLocations(overlayTile, ability.abilityShape, activeCharacter.activeTile.grid2DLocation);

        foreach (var tile in tiles)
        {
            UpdateTileColors(tile, AllyColor);
            ShowTile(tile);
        }
    }
}
