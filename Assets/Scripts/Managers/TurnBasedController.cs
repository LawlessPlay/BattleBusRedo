using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacticsToolkit
{
    public class TurnBasedController : MonoBehaviour
    {
        private List<Entity> teamA = new List<Entity>();
        private List<Entity> teamB = new List<Entity>();

        public TurnSorting turnSorting;

        public GameEventGameObject startNewCharacterTurn;
        public GameEventGameObjectList turnOrderSet;
        public GameEventGameObjectList turnPreviewSet;

        public List<TurnOrderPreviewObject> turnOrderPreview;
        public List<TurnOrderPreviewObject> currentTurnOrderPreview;

        public bool ignorePlayers = false;
        public bool ignoreEnemies = false;

        public int previewPoolCount = 10;
        private Entity activeCharacter;

        public enum TurnSorting
        {
            ConstantAttribute,
            CTB
        };

        void Start()
        {
            if (!ignorePlayers)
                teamA = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Entity>()).ToList();

            if (!ignoreEnemies)
                teamB = GameObject.FindGameObjectsWithTag("Enemy").Select(x => x.GetComponent<Entity>()).ToList();

            foreach (var item in teamA)
            {
                item.teamID = 1;
            }

            foreach (var item in teamB)
            {
                item.teamID = 2;
            }

            if (teamA.Count > 0 || teamB.Count > 0)
                SortTeamOrder(true);
        }

        private void SortTeamOrder(bool updateListSize = false)
        {
            if (teamA.Count == 0 && teamB.Count == 0) return;

            var combinedList = new List<Entity>();
            foreach (var team in new[] { teamA, teamB })
            {
                foreach (var entity in team)
                {
                    if (entity.isAlive)
                    {
                        combinedList.Add(entity);
                    }
                }
            }

            if (turnSorting == TurnSorting.ConstantAttribute)
            {
                turnOrderPreview = combinedList
                    .OrderBy(x => x.statsContainer.Speed.statValue)
                    .Select(x => new TurnOrderPreviewObject(x, x.initiativeValue))
                    .ToList();

                if (updateListSize)
                {
                    int characterCount = 1;
                    while (turnOrderPreview.Count < previewPoolCount)
                    {
                        foreach (var entity in combinedList)
                        {
                            turnOrderPreview.Add(new TurnOrderPreviewObject(entity, entity.initiativeValue * characterCount));
                        }
                        characterCount++;
                    }
                }
            }
            else if (turnSorting == TurnSorting.CTB)
            {
                turnOrderPreview = combinedList
                    .Select(x => new TurnOrderPreviewObject(x, x.initiativeValue + (Constants.BaseCost / x.GetStat(Stats.Speed).statValue)))
                    .ToList();

                int characterCount = 2;
                while (turnOrderPreview.Count < previewPoolCount)
                {
                    foreach (var entity in combinedList)
                    {
                        turnOrderPreview.Add(new TurnOrderPreviewObject(entity, entity.initiativeValue + ((Constants.BaseCost / entity.GetStat(Stats.Speed).statValue) * characterCount)));
                    }
                    characterCount++;
                }

                turnOrderPreview = turnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
            }

            activeCharacter = turnOrderPreview[0].character;
            currentTurnOrderPreview = turnOrderPreview;
            turnOrderSet.Raise(turnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void StartLevel()
        {
            if (HasAliveCharacters())
            {
                startNewCharacterTurn.Raise(activeCharacter.gameObject);
            }

            SortTeamOrder(true);
        }

        public void EndTurn()
        {
            if (turnOrderPreview.Count > 0)
            {
                FinaliseEndCharactersTurn();

                SortTeamOrder();

                foreach (var entity in turnOrderPreview)
                    entity.character.SetIsActive(false);

                if (HasAliveCharacters())
                {  
                    if (activeCharacter.isAlive)
                    {
                        startNewCharacterTurn.Raise(activeCharacter.gameObject);
                    }
                }
            }
        }

        private IEnumerator EndTurnWithWaits()
        {
            if (activeCharacter.isAlive)
            {
                activeCharacter.SetIsActive(true);
                activeCharacter.ApplyEffects();

                if (activeCharacter.isAlive)
                {
                    startNewCharacterTurn.Raise(activeCharacter.gameObject);
                }
                else
                {
                    EndTurn();
                }

                // Cache `abilitiesForUse` outside the loop for efficiency
                var abilities = activeCharacter.abilitiesForUse;
                foreach (var ability in abilities)
                {
                    ability.turnsSinceUsed++;
                }
            }
            else
            {
                EndTurn();
            }
            
            yield return null;
        }

        private bool HasAliveCharacters() => turnOrderPreview.Any(x => x.character.isAlive);

        private void FinaliseEndCharactersTurn()
        {
            if (activeCharacter.activeTile && activeCharacter.activeTile.tileData)
            {
                var tileEffect = activeCharacter.activeTile.tileData.effect;

                if (tileEffect != null)
                    activeCharacter.AttachEffect(tileEffect);
            }

            activeCharacter.UpdateInitiative(Constants.BaseCost);
        }

        IEnumerator DelayedSetActiveCharacter(Entity firstCharacter)
        {
            yield return new WaitForFixedUpdate();
            startNewCharacterTurn.Raise(firstCharacter.gameObject);
        }

        public void SpawnNewCharacter(GameObject character)
        {
            var newEntity = character.GetComponent<Entity>();
            teamA.Add(newEntity);
            SortTeamOrder(true);
        }

        public void UpdatePreviewForAction(int actionCost)
        {
            var updatedTurnOrderPreview = turnOrderPreview
                .Select(x => new TurnOrderPreviewObject(x.character, x.PreviewInitiativeValue))
                .ToList();

            var activeCharacters = updatedTurnOrderPreview
                .Where(x => x.character.name == activeCharacter.name)
                .ToList();

            for (int i = 1; i < activeCharacters.Count; i++)
            {
                activeCharacters[i].PreviewInitiativeValue += Mathf.RoundToInt(actionCost / activeCharacters[i].character.GetStat(Stats.Speed).statValue);
            }

            currentTurnOrderPreview = updatedTurnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
            turnPreviewSet.Raise(currentTurnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void UndoPreview()
        {
            turnOrderSet.Raise(turnOrderPreview.Select(x => x.character.gameObject).ToList());
        }

        public void ActionCompleted()
        {
            turnOrderPreview = currentTurnOrderPreview;
            turnOrderSet.Raise(currentTurnOrderPreview.Select(x => x.character.gameObject).ToList());
        }
    }

    public class TurnOrderPreviewObject
    {
        public Entity character;
        public int PreviewInitiativeValue;

        public TurnOrderPreviewObject(Entity character, int previewInitiativeValue)
        {
            this.character = character;
            PreviewInitiativeValue = previewInitiativeValue;
        }
    }
}
