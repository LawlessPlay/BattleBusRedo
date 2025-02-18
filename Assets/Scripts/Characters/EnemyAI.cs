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
    

    private List<Vector3> linePositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        rangeFinder = new RangeFinder();
        pathFinder = new PathFinder();
        shapeParser = new ShapeParser();
        pathRenderer = new PathRenderer();
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
        Senario currentBestSenario = null;

        // Cache movement range tiles
        var movementRangeData = rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange);
        var movementRangeTiles = movementRangeData.Item1.Where(x => !x.activeCharacter).ToList();

        foreach (var movementRangeTile in movementRangeTiles)
        {
            // Cache spell actions for this tile
            var enemySpellAction = GetBestSpellAction(EnemySpell, movementRangeTile);
            var allySpellAction = GetBestSpellAction(AllySpell, movementRangeTile);
            var selfSpellAction = GetBestSpellAction(SelfSpell, movementRangeTile);

            var selectedSenario = GetWeightedRandomSpell(enemySpellAction, allySpellAction, selfSpellAction);

            if (currentBestSenario == null || selectedSenario.senarioValue > currentBestSenario.senarioValue)
            {
                currentBestSenario = selectedSenario;
                currentBestSenario.positionTile = movementRangeTile;
            }
        }

        CreateActionQueueFromSenario(currentBestSenario);
    }

    private void CreateActionQueueFromSenario(Senario currentBestSenario)
    {
        var actionQueue = new List<Action>();
        var path = pathFinder.FindPath(activeTile, currentBestSenario.positionTile,
            rangeFinder.GetTilesInRange(activeTile, characterClass.MoveRange).Item1);
        var nicePath = pathRenderer.GeneratePath(path, activeTile.transform.position, -1);

        actionQueue.Add(new Action(nicePath, Action.ActionType.Move, currentBestSenario.positionTile, this));

        if (currentBestSenario.targetTile != null)
        {
            Debug.Log("Casting: " + currentBestSenario.Ability.name + " by " + this.name + " at " + currentBestSenario.targetTile.activeCharacter + " with a value of " + currentBestSenario.senarioValue);
            var affectedCharacters = shapeParser.GetAbilityTileLocations(currentBestSenario.targetTile,
                    currentBestSenario.Ability.abilityShape,
                    activeTile.grid2DLocation, currentBestSenario.Ability.includeOrigin).Where(c => c.activeCharacter)
                .ToList();

            foreach (var character in affectedCharacters)
            {
                Debug.Log("Affecting: " + character.activeCharacter.name);
            }
            setTarget.Raise(currentBestSenario.targetTile.gameObject);
            actionQueue.Add(new Action(nicePath, Action.ActionType.Attack, currentBestSenario.targetTile, this,
                currentBestSenario.Ability));
        }

        this.actionQueue = actionQueue;
        actionQueue[0].StartAction();
    }

    public Senario GetBestSpellAction(Ability ability, OverlayTile overlayTile)
    {
        var inRangeTiles = rangeFinder.GetTilesInRange(overlayTile, ability.range).Item1;

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
                case Ability.AbilityTypes.Heal:
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
                            character.statsContainer.getStat(Stats.Health).statValue / 100 * 20)
                        {
                            value += 1000;
                        }

                        abilityValue += value;
                    }

                    break;
                case Ability.AbilityTypes.Damage:
                    characters = affectedTiles.Select(x => x.activeCharacter).Where(c => c != null).ToList();
                    var enemyCharacters = characters.Where(c => c.teamID != teamID).ToList();
                    var allyCharacters = characters.Where(c => c.teamID == teamID || activeTile == c.activeTile).ToList();

                    
                    foreach (var enemy in enemyCharacters)
                    {
                        abilityValue += ability.value;

                        //if character is close to dying, finish them.
                        if (enemy.statsContainer.getStat(Stats.CurrentHealth).statValue < ability.value)
                        {
                            abilityValue += 1000;
                        }
                    }
                    
                    foreach (var ally in allyCharacters)
                    {
                        Debug.Log("Will Hit: " + ally.gameObject.name);
                        abilityValue = Int32.MinValue;
                    }

                    break;
                case Ability.AbilityTypes.All:
                    characters = affectedTiles.Where(x => x.activeCharacter).Select(x => x.activeCharacter).ToList();

                    foreach (var character in characters)
                    {
                        var value = ability.value;
                        abilityValue = value;
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

        return new Senario(currentBestValue, ability, currentBestTile, overlayTile);
    }

    Senario GetWeightedRandomSpell(Senario EnemyAction, Senario AllyAction, Senario SelfAction)
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