using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static TacticsToolkit.Ability;
using System.Collections;

namespace TacticsToolkit
{
    public class AbilityController : MonoBehaviour
    {
        public GameEventString disableAbility;
        public GameEventInt previewAction;

        public RangeFinder eventRangeController;

        private Entity activeCharacter;
        private List<OverlayTile> abilityRangeTiles;
        private List<OverlayTile> abilityAffectedTiles;
        private ShapeParser shapeParser;
        private AbilityContainer abilityContainer;

        //Action Completed is required for the CTB turn order. The turn order needs to be solidified on each action. Otherwise, not very important. 
        public GameEvent actionCompleted;
        // Start is called before the first frame update
        void Start()
        {
            eventRangeController = new RangeFinder();
            shapeParser = new ShapeParser();
            abilityRangeTiles = new List<OverlayTile>();
            abilityAffectedTiles = new List<OverlayTile>();
        }

        //Cast an ability
        private void CastAbility()
        {
            var inRangeCharacters = new List<Entity>();

            //get in range characters
            foreach (var tile in abilityAffectedTiles)
            {
                var targetCharacter = tile.activeCharacter;
                if (targetCharacter != null && CheckAbilityTargets(abilityContainer.ability.abilityType, targetCharacter) && targetCharacter.isAlive)
                {
                    inRangeCharacters.Add(targetCharacter);
                }
            }
            
            if (abilityContainer.ability.requiresTarget && inRangeCharacters.Count > 0 || !abilityContainer.ability.requiresTarget)
            {
                //attach effects
                foreach (var character in inRangeCharacters)
                {
                    foreach (var effect in abilityContainer.ability.effects)
                    {
                        if (effect.Duration == 0)
                            character.ApplySingleEffects(effect);
                        else
                            character.AttachEffect(effect);
                    }

                    //apply value
                    switch (abilityContainer.ability.abilityType)
                    {
                        case AbilityTypes.Heal:
                            character.HealEntity(abilityContainer.ability.value);
                            break;
                        case AbilityTypes.Damage:
                            character.TakeDamage(abilityContainer.ability.value);
                            break;
                        case AbilityTypes.All:
                            character.TakeDamage(abilityContainer.ability.value);
                            break;
                        default:
                            break;
                    }

                    foreach (var effect in abilityContainer.ability.effects)
                    {
                        if (effect.Duration == 0)
                            character.UndoEffect(effect);
                    }
                }

                abilityContainer.turnsSinceUsed = 0;
                activeCharacter.UpdateInitiative(Constants.AbilityCost);
                activeCharacter.UpdateMana(abilityContainer.ability.cost);
                disableAbility.Raise(abilityContainer.ability.Name);
                abilityContainer = null;
                OverlayController.Instance.ClearTiles(null);

                if (actionCompleted)
                    actionCompleted.Raise();

            }
        }
        //The event receiver for Casting an Ability. 
        public void CastAbilityCommand(EventCommand abilityCommand)
        {
            if (abilityCommand is CastAbilityCommand)
            {
                CastAbilityCommand command = (CastAbilityCommand)abilityCommand;
                CastAbilityParams castAbilityParams = command.StronglyTypedCommandParam();
                abilityAffectedTiles = castAbilityParams.affectedTiles;
                abilityContainer = castAbilityParams.abilityContainer;
                CastAbility();
            }
        }

        //Check if Abilities are targeting the right entities.
        private bool CheckAbilityTargets(AbilityTypes abilityType, Entity characterTarget)
        {
            if (abilityType == AbilityTypes.Damage)
            {
                return characterTarget.teamID != activeCharacter.teamID;
            }
            else if (abilityType == AbilityTypes.Heal)
            {
                return characterTarget.teamID == activeCharacter.teamID;
            }

            return true;
        }

        public void SetActiveCharacter(GameObject activeChar)
        {
            activeCharacter = activeChar.GetComponent<Entity>();
        }

        //Set the position the abilities origin.
        public void SetAbilityPosition(GameObject focusedOnTile)
        {
            var map = MapManager.Instance.map;
            OverlayTile tilePosition = focusedOnTile.GetComponent<OverlayTile>();
            if (abilityContainer != null)
            {
                activeCharacter.FaceGridLocation(tilePosition.grid2DLocation);

                foreach (var tile in abilityAffectedTiles)
                {
                    if (map.ContainsKey(tile.grid2DLocation))
                    {
                        var gridTile = map[tile.grid2DLocation];
                        gridTile.GetComponent<SpriteRenderer>().color = abilityRangeTiles.Contains<OverlayTile>(gridTile)
                            ? OverlayController.Instance.MoveRangeColor
                            : new Color(0, 0, 0, 0);
                    }
                }

                abilityAffectedTiles = new List<OverlayTile>();
                if (abilityRangeTiles.Contains(map[tilePosition.grid2DLocation]))
                {
                    abilityAffectedTiles = shapeParser.GetAbilityTileLocations(tilePosition, abilityContainer.ability.abilityShape, activeCharacter.activeTile.grid2DLocation, abilityContainer.ability.includeOrigin);

                    if (abilityContainer.ability.includeOrigin)
                        abilityAffectedTiles.Add(tilePosition);

                    OverlayController.Instance.ColorTiles(OverlayController.Instance.AttackRangeColor, abilityAffectedTiles);
                }
            }
        }

        //The event receiver for Casting an Ability. 
        public IEnumerator DelayedEnterAbilityMode(string abilityName)
        {
            yield return new WaitForFixedUpdate();

            OverlayController.Instance.ClearTiles(null);

            var abilityContainer = activeCharacter.abilitiesForUse.Find(x => x.ability.name == abilityName);
            if (abilityContainer.ability.cost <= activeCharacter.statsContainer.CurrentMana.statValue)
            {
                abilityRangeTiles = eventRangeController.GetTilesInRange(activeCharacter.activeTile, abilityContainer.ability.range, true).Item1;
                OverlayController.Instance.ColorTiles(OverlayController.Instance.MoveRangeColor, abilityRangeTiles);
                this.abilityContainer = abilityContainer;

                if (previewAction)
                    previewAction.Raise(Constants.AbilityCost);
            }
        }

        //Set ability casting mode. 
        public void AbilityModeEvent(string abilityName)
        {
            StartCoroutine(DelayedEnterAbilityMode(abilityName));
        }

        //Cancel ability casting mode. 
        public void CancelEventMode()
        {
            OverlayController.Instance.ClearTiles(null);
            abilityContainer = null;
        }

        public void ActionButtonPressed()
        {
            if (abilityContainer != null && abilityAffectedTiles.Count != 0)
                CastAbility();
        }
    }
}
