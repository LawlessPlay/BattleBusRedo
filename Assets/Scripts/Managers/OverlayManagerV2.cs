using System;
using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class OverlayManagerV2 : MonoBehaviour
{
    private Dictionary<OverlayTile, List<Color>> _inRangeColoredTiles;
    private Dictionary<OverlayTile, List<Color>> _abilityColoredTiles;
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
    
    
    public static OverlayManagerV2 Instance { get; private set; }
    
    private void Awake()
    {
        _inRangeColoredTiles = new Dictionary<OverlayTile, List<Color>>();
        _abilityColoredTiles = new Dictionary<OverlayTile, List<Color>>();
        MoveRangeColor = gameConfig.MoveRangeColor;
        AttackRangeColor = gameConfig.AttackRangeColor;
        BlockedTileColor = gameConfig.BlockedTileColor;
        EnemyColor = gameConfig.EnemyColor;
        AllyColor = gameConfig.AllyColor;
        SelfColor = gameConfig.SelfColor;
        
        rangeFinder = new RangeFinder();
        shapeParser = new ShapeParser();
        
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void UpdateTileColors(OverlayTile tile, Color color)
    {
        if (_inRangeColoredTiles.ContainsKey(tile))
        {
            _inRangeColoredTiles[tile].Add(color);
        }
        else
        {
            _inRangeColoredTiles.Add(tile, new List<Color>() { color });
        }
    }
    
    public void UpdateAbilityTileColors(OverlayTile tile, Color color)
    {
        if (_abilityColoredTiles.ContainsKey(tile))
        {
            _abilityColoredTiles[tile].Add(color);
        }
        else
        {
            _abilityColoredTiles.Add(tile, new List<Color>() { color });
        }
    }
    
    public void ShowTotalOverlay()
    {
        var movementTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange);
        var enemySpellTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange + activeCharacter.EnemySpell.range);
        var allySpellTilesToShow = rangeFinder.GetTilesInRange(activeCharacter.activeTile, activeCharacter.characterClass.MoveRange + activeCharacter.AllySpell.range);

        var combinedTiles = new HashSet<OverlayTile>(enemySpellTilesToShow.Item1);
        combinedTiles.UnionWith(allySpellTilesToShow.Item1); 
        
        foreach (var tile in combinedTiles)
        {
            if (movementTilesToShow.Item1.Contains(tile))
            {
                UpdateTileColors(tile, MoveRangeColor);
            }
            else
            {
                if (allySpellTilesToShow.Item1.Contains(tile))
                {
                    UpdateTileColors(tile, AllyColor);
                }
                if (enemySpellTilesToShow.Item1.Contains(tile))
                {
                    UpdateTileColors(tile, AttackRangeColor);
                }
            }

            ShowTile(tile);
        }
        
        ShowCharacterTiles(enemySpellTilesToShow.Item2.Where(x => x.activeCharacter.teamID != activeCharacter.teamID).ToList());
        ShowCharacterTiles(allySpellTilesToShow.Item2.Where(x => x.activeCharacter.teamID == activeCharacter.teamID).ToList());
    }

    private void ShowCharacterTiles(List<OverlayTile> tiles)
    {
        ClearTiles(tiles);
        foreach (var overlayTile in tiles)
        {
            if (overlayTile.activeCharacter == activeCharacter)
            {
                UpdateTileColors(overlayTile, SelfColor);
                ShowTile(overlayTile);
                continue;
            }

            UpdateTileColors(overlayTile, overlayTile.activeCharacter.teamID != activeCharacter.teamID ? EnemyColor : AllyColor);
            ShowTile(overlayTile);
        }
    }

    private void ShowTile(OverlayTile tile)
    {
        var tempTileColors = new List<Color>();
        
        if(_inRangeColoredTiles.ContainsKey(tile))
            tempTileColors.AddRange(_inRangeColoredTiles[tile]);
        
        if(_abilityColoredTiles.ContainsKey(tile))
            tempTileColors.AddRange(_abilityColoredTiles[tile]);
        
        
        tile.ShowTile(CombineColors(tempTileColors));
    }
    
    public static Color CombineColors(List<Color> colors)
    {
        if (colors == null || colors.Count == 0)
        {
            return default;
        }

        float totalWeight = colors.Count;
        Color combinedColor = new Color(0, 0, 0); // Start with black or any neutral color.

        foreach (var color in colors)
        {
            combinedColor += color / totalWeight; // Evenly distribute the contribution of each color
        }

        return combinedColor;
    }

    public void SetActiveCharacter(Entity character)
    {
        activeCharacter = character;
    }

    public void SetActiveTile(OverlayTile activeTile)
    {
        ClearAbilityTiles();
        
        this.activeTile = activeTile;

        if (activeTile.activeCharacter)
        {
            
        }
    }

    public void ClearTiles()
    {
        foreach (var tile in _inRangeColoredTiles.Keys)
        {
            tile.HideTile();
        }
        
        _inRangeColoredTiles.Clear();
    }
    
    public void ClearTile(OverlayTile tile)
    {
        tile.HideTile();
        _inRangeColoredTiles.Remove(tile);
    }
    public void ClearTiles(List<OverlayTile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.HideTile();
            _inRangeColoredTiles.Remove(tile);
        }
    }
    
    public void ClearAbilityTiles()
    {
        var tempTilesToUpdate = new List<OverlayTile>();
        tempTilesToUpdate.AddRange(_abilityColoredTiles.Keys);
        _abilityColoredTiles.Clear();
        foreach (var tile in tempTilesToUpdate)
        {
            tile.HideTile();
            ShowTile(tile);
        }
    }
    
    public void ClearColor(Color color)
    {
        foreach (var tile in _inRangeColoredTiles.Keys)
        {
            _inRangeColoredTiles[tile].Remove(color);

            if (_inRangeColoredTiles[tile].Count == 0)
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
        var tileColors = _inRangeColoredTiles[tile];

        tileColors.RemoveAll(c => c == color);
        
        _inRangeColoredTiles[tile] = tileColors;
    }

    public void DrawSpell(OverlayTile overlayTile, Ability ability)
    {
        var tiles = shapeParser.GetAbilityTileLocations(overlayTile, ability.abilityShape, activeCharacter.activeTile.grid2DLocation);

        foreach (var tile in tiles)
        {
            UpdateAbilityTileColors(tile, AllyColor);
            ShowTile(tile);
        }
    }
}
