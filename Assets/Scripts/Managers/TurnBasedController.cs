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
        
        public TurnOrderDisplayTwo turnOrderDisplay;

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
                item.teamID = Entity.TeamType.Player;
            }

            foreach (var item in teamB)
            {
                item.teamID = Entity.TeamType.Enemy;
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
                    .Select(x => new TurnOrderPreviewObject(x, x.initiativeValue,0))
                    .ToList();

                if (updateListSize)
                {
                    int characterCount = 1;
                    while (turnOrderPreview.Count < previewPoolCount)
                    {
                        foreach (var entity in combinedList)
                        {
                            turnOrderPreview.Add(new TurnOrderPreviewObject(entity, entity.initiativeValue * characterCount,0));
                        }
                        characterCount++;
                    }
                }
            }
            else if (turnSorting == TurnSorting.CTB)
            {
                turnOrderPreview = combinedList
                    .Select(x => new TurnOrderPreviewObject(x, x.initiativeValue + (Constants.BaseCost / x.GetStat(Stats.Speed).statValue),0))
                    .ToList();

                int characterCount = 2;
                while (turnOrderPreview.Count < previewPoolCount)
                {
                    foreach (var entity in combinedList)
                    {
                        turnOrderPreview.Add(new TurnOrderPreviewObject(entity, entity.initiativeValue + ((Constants.BaseCost / entity.GetStat(Stats.Speed).statValue) * characterCount),0));
                    }
                    characterCount++;
                }

                turnOrderPreview = turnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
            }

            activeCharacter = turnOrderPreview[0].character;
            currentTurnOrderPreview = turnOrderPreview;
            
            turnOrderSet.Raise(turnOrderPreview.Select(x => x.character.gameObject).ToList());
            
            turnOrderDisplay.SetTurnOrderList(turnOrderPreview);
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
                turnOrderDisplay.PopOffFirst(DoInsert);
                
                
            }
        }
        public void DoInsert()
        {
            //turnOrderDisplay.InsertAt(turnOrderPreview.Count - 1, activeCharacter);
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
                .Select(x => new TurnOrderPreviewObject(x.character, x.PreviewInitiativeValue,0))
                .ToList();

            var activeCharacters = updatedTurnOrderPreview
                .Where(x => x.character.name == activeCharacter.name)
                .ToList();

            for (int i = 1; i < activeCharacters.Count; i++)
            {
                activeCharacters[i].PreviewInitiativeValue += Mathf.RoundToInt(actionCost / activeCharacters[i].character.GetStat(Stats.Speed).statValue);
            }
            
            currentTurnOrderPreview = updatedTurnOrderPreview.OrderBy(x => x.PreviewInitiativeValue).ToList();
            turnOrderDisplay.StartPreview(currentTurnOrderPreview, activeCharacter);
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
        public float PreviewInitiativeValue;
        public float DefaultPreviewInitiativeValue;
        public int index;

        public TurnOrderPreviewObject(Entity character, float previewInitiativeValue, int i)
        {
            this.character = character;
            this.PreviewInitiativeValue = previewInitiativeValue;
            this.DefaultPreviewInitiativeValue = previewInitiativeValue;
            this.index = i;
        }

        public void ModifyTurnOrderPreview(int turnOrderPreview)
        {
            PreviewInitiativeValue += turnOrderPreview;
        }

        public void ResetPreview()
        {
            PreviewInitiativeValue = DefaultPreviewInitiativeValue;
        }
    }
}
