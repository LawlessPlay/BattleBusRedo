using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : Entity
{
    private OverlayManagerV2 overlayManager;
    private PathFinder pathFinder;
    private PathRenderer pathRenderer;
    private RangeFinder rangeFinder;
    private ShapeParser shapeParser;
    private LineRenderer lineRenderer;
    private float lineOffset = 0.5f;
    
    
    const float LowHealthThreshold = 0.20f; // 20%
    const int LowHealthSaveBonus = 150;
    const int LowHealthExecuteBonus = 150;
    const int DamageInflation = 2; // if you want it

    private List<Vector3> linePositions = new List<Vector3>();

    struct ScoreWeights {
        public int DamageWeight;         // e.g., 1
        public int HealWeight;           // e.g., 1
        public int ExecuteBonus;         // e.g., 150
        public int SaveAllyBonus;        // e.g., 150
        public int DistanceToEnemyWeight;// e.g., +1 per step closer (or negative for kiting)
        public int DistanceToAllyWeight; // e.g., prefer staying near allies?
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rangeFinder = new RangeFinder();
        pathFinder = new PathFinder();
        shapeParser = new ShapeParser();
        pathRenderer = new PathRenderer();

        teamID = TeamType.Enemy;
    }

    void Update()
    {
        if (actionQueue.Count > 0)
        {
            if(actionQueue[0].State == Action.ActionState.InProgress && actionQueue[0].Type == Action.ActionType.Move)
                actionQueue[0].DoAction();
            
            if (actionQueue[0].State == Action.ActionState.Finished)
            {
                actionQueue.RemoveAt(0);
                if (actionQueue.Count > 0)
                {
                    OverlayManagerV2.Instance.DrawSpell(actionQueue[0].Target, actionQueue[0].Ability);
                    actionQueue[0].StartAction();
                }
                else
                {
                    EndTurn();
                }
            }
        }
    }
    
    public override void StartTurn()
    {
        StartCoroutine(StartTurnRoutine());
    }
    
    IEnumerator StartTurnRoutine()
    {
        base.StartTurn();
        yield return new WaitForSeconds(0.5f);
        Scenario currentBestScenario = null;
        List<Scenario> historicalScenarios = new List<Scenario>();

        OverlayTile originalActiveTile = activeTile;
        
        List<Scenario> originSpells = new List<Scenario>();
            
        // Cache spell actions for this tile
        originSpells.Add(GetBestSpellAction(EnemySpell, originalActiveTile));
        originSpells.Add(GetBestSpellAction(AllySpell, originalActiveTile));
        originSpells.Add(GetBestSpellAction(SelfSpell, originalActiveTile));
        
        // Cache movement range tiles
        var movementRangeData = rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange);
        var movementRangeTiles = movementRangeData.Item1.Where(x => !x.activeCharacter).ToList();

        foreach (var movementRangeTile in movementRangeTiles.Where(movementRangeTile => !movementRangeTile.activeCharacter))
        {
            activeTile = movementRangeTile;
            movementRangeTile.activeCharacter = this;
            
            var bestSpell = GetBestScenarioFromTile(movementRangeTile);
            
            if (currentBestScenario == null || bestSpell.senarioValue > currentBestScenario.senarioValue)
            {
                currentBestScenario = bestSpell;
                currentBestScenario.positionTile = movementRangeTile;
                historicalScenarios.Add(currentBestScenario);
            }
            
            movementRangeTile.activeCharacter = null;
        }

        activeTile = originalActiveTile;
        CreateActionQueueFromSenario(currentBestScenario);
    }

    public Scenario GetBestScenarioFromTile(OverlayTile tile)
    {
        activeTile = tile;
        tile.activeCharacter = this;
            
        List<Scenario> spells = new List<Scenario>();
            
        // Cache spell actions for this tile
        spells.Add(GetBestSpellAction(EnemySpell, tile));
        spells.Add(GetBestSpellAction(AllySpell, tile));
        spells.Add(GetBestSpellAction(SelfSpell, tile));
        
        var bestSpell = spells.OrderByDescending(x => x.senarioValue).First();
        var threatValue = MapManager.Instance.GetTileThreatLevel(false, this, tile);
        var distanceFromPlayer = MapManager.Instance.GetClosestCharacterDistance(true, this, tile);

        //bestSpell.senarioValue += threatValue;
        bestSpell.senarioValue -= distanceFromPlayer * 1.25f;
        return bestSpell;
    }

    private void CreateActionQueueFromSenario(Scenario currentBestScenario)
    {
        var actionQueue = new List<Action>();
        var path = pathFinder.FindPath(activeTile, currentBestScenario.positionTile,
            rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange).Item1);
        var nicePath = pathRenderer.GeneratePath(path, activeTile.transform.position, -1);

        actionQueue.Add(new Action(nicePath, Action.ActionType.Move, currentBestScenario.positionTile, this));

        if (currentBestScenario.targetTile != null)
        {
            Debug.Log("Casting: " + currentBestScenario.Ability.name + " by " + this.name + " at " + currentBestScenario.targetTile.activeCharacter + " with a value of " + currentBestScenario.senarioValue);
            var affectedCharacters = shapeParser.GetAbilityTileLocations(currentBestScenario.targetTile,
                    currentBestScenario.Ability.abilityShape,
                    activeTile.grid2DLocation, currentBestScenario.Ability.includeOrigin).Where(c => c.activeCharacter)
                .ToList();

            foreach (var character in affectedCharacters)
            {
                Debug.Log("Affecting: " + character.activeCharacter.name);
            }
            setTarget.Raise(currentBestScenario.targetTile.gameObject);
            actionQueue.Add(new Action(nicePath, Action.ActionType.Attack, currentBestScenario.targetTile, this,
                currentBestScenario.Ability));
        }

        this.actionQueue = actionQueue;
        
        var turnValue = actionQueue.Sum(x => x.InitiativeValue);
        UpdateInitiative(turnValue);
        actionQueue[0].StartAction();
    }

    public Scenario GetBestSpellAction(Ability ability, OverlayTile overlayTile)
    {
        var twoCollections = rangeFinder.GetTilesInRange(overlayTile, ability.range, true);
        var inRangeTiles = twoCollections.Item1;
        inRangeTiles.AddRange(twoCollections.Item2);
        
        OverlayTile currentBestTile = null;
        var currentBestValue = 0f;

        foreach (var inRangeTile in inRangeTiles)
        {
            int abilityValue = 0;
            var affectedTiles = shapeParser.GetAbilityTileLocations(inRangeTile, ability.abilityShape,
                activeTile.grid2DLocation, ability.includeOrigin);
            var characters = new List<Entity>();

            switch (ability.abilityType)
            {
                case Ability.AbilityTypes.Ally:
                    characters = affectedTiles.Where(x => x.activeCharacter && x.activeCharacter.teamID == teamID)
                        .Select(x => x.activeCharacter).ToList();

                    foreach (var character in characters)
                    {
                        var characterHealth = character.statsContainer.getStat(Stats.CurrentHealth).statValue;
                        var characterMissingHealth = character.statsContainer.getStat(Stats.Health).statValue -
                                                     characterHealth;
                        var value = 0;
                        if (ability.value < characterMissingHealth)
                            value = ability.value;
                        else
                            value = characterMissingHealth;

                        //if character is close to dying try to save it.
                        if (character.statsContainer.getStat(Stats.CurrentHealth).statValue <=
                            character.statsContainer.getStat(Stats.Health).statValue * LowHealthThreshold )
                        {
                            value += LowHealthSaveBonus ;
                        }

                        abilityValue += value;
                    }

                    break;
                case Ability.AbilityTypes.Enemy:
                    characters = affectedTiles.Select(x => x.activeCharacter).Where(c => c != null).ToList();
                    var enemyCharacters = characters.Where(c => c.teamID != teamID).ToList();
                    var allyCharacters = characters.Where(c => c.teamID == teamID).ToList();
                    
                    foreach (var enemy in enemyCharacters)
                    {
                        abilityValue += ability.value;

                        //if character is close to dying, finish them.
                        if (enemy.statsContainer.getStat(Stats.CurrentHealth).statValue < enemy.statsContainer.getStat(Stats.Health).statValue * LowHealthThreshold )
                        {
                            abilityValue += LowHealthExecuteBonus ;
                        }
                    }
                    
                    const int allyHitPenalty = 250; // tune this
                    if (allyCharacters.Count > 0) continue; // skip tile entirely
                        abilityValue -= allyHitPenalty * allyCharacters.Count;
                    
                    //damage inflation
                    abilityValue = Mathf.RoundToInt(abilityValue * DamageInflation );
                    
                    break;
                case Ability.AbilityTypes.All:
                    characters = affectedTiles.Where(x => x.activeCharacter).Select(x => x.activeCharacter).ToList();

                    foreach (var character in characters)
                    {
                        var value = ability.value;
                        abilityValue += value;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (abilityValue > currentBestValue)
            {
                currentBestValue = abilityValue;
                currentBestTile = inRangeTile;
            }
        }

        return new Scenario(currentBestValue, ability, currentBestTile, overlayTile);
    }

    Scenario GetWeightedRandomSpell(Scenario EnemyAction, Scenario AllyAction, Scenario SelfAction)
    {
        // Step 1: Calculate total weight
        float totalWeight = 0f;

        totalWeight += EnemyAction.senarioValue;
        totalWeight += AllyAction.senarioValue;
        totalWeight += SelfAction.senarioValue;

        // Step 2: Generate random number
        float randomValue = Random.Range(0f, totalWeight);

        // Step 3: Select based on weight
        float cumulativeWeight = 0f;

        cumulativeWeight += EnemyAction.senarioValue;
        if (randomValue <= cumulativeWeight)
        {
            return EnemyAction;
        }

        cumulativeWeight += AllyAction.senarioValue;
        if (randomValue <= cumulativeWeight)
        {
            return AllyAction;
        }

        cumulativeWeight += SelfAction.senarioValue;
        if (randomValue <= cumulativeWeight)
        {
            return SelfAction;
        }

        // Fallback (shouldn't happen if weights are set correctly)
        return null;
    }

    public override void TriggerNextAction()
    {
        actionQueue[0].DoAction();
    }
}