using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;
using TMPro;
using UnityEngine;

public class TurnBasedManagerRedo : MonoBehaviour
{
    public List<Entity> playerList = new List<Entity>();
    public List<Entity> enemyList = new List<Entity>();
    public List<TurnOrderPreviewObject> turnList = new List<TurnOrderPreviewObject>();
    public TMP_Text speedOrderText;
    
    public GameEventGameObject setActiveCharacter;
    

    private Dictionary<Entity, List<TurnOrderPreviewObject>> turnOrderPreviewObjects =
        new Dictionary<Entity, List<TurnOrderPreviewObject>>();

    public TurnOrderDisplayTwo turnOrderDisplay;
    
    // Start is called before the first frame update
    void Start()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Entity>()).ToList();
        enemyList = GameObject.FindGameObjectsWithTag("Enemy").Select(x => x.GetComponent<Entity>()).ToList();
        
        SetTurnOrder();
    }

    void Update()
    {
        var totalList = playerList.Concat(enemyList).ToList();
        
        totalList = totalList.Distinct().OrderByDescending(x => x.GetStat(Stats.Speed).statValue).ToList();
        speedOrderText.text = "";
        foreach (var character in totalList)
        {
            speedOrderText.text += character.name + ": " + character.GetStat(Stats.Speed).statValue;
            speedOrderText.text += "\n";
        }
    }

    public void SetTurnOrder()
    {
        turnList.Clear();
        turnList = CreateTurnOrder();
        turnOrderDisplay.SetTurnOrderList(turnList);
    }

    public List<TurnOrderPreviewObject> CreateTurnOrder()
    {
        var totalList = playerList.Concat(enemyList).ToList();
        var tempList = new List<TurnOrderPreviewObject>();
        var tempDictionary = new Dictionary<Entity, List<TurnOrderPreviewObject>>();


        foreach (var character in totalList)
        {
            for (int i = 1; i < 6; i++)
            {
                if(!tempDictionary.ContainsKey(character))
                    tempDictionary.Add(character, new List<TurnOrderPreviewObject>(){new TurnOrderPreviewObject(character, (( Constants.BaseCost / (float)character.GetStat(Stats.Speed).statValue)) * i, i)});
                else
                {
                    tempDictionary[character].Add(new TurnOrderPreviewObject(character, ((Constants.BaseCost / (float)character.GetStat(Stats.Speed).statValue)) * i, i));
                }
            }  
        }
        
        foreach (var values in tempDictionary.Values)
        {
            tempList.AddRange(values);
        }
        
        return tempList.OrderByDescending(x => x.PreviewInitiativeValue).ToList();
    }

    public void PreviewUpdateOrder(GameObject character, int entryCount)
    {
        int currentCount = 0;
        for (int i = 0; i < turnList.Count; i++)
        {
            if (turnList[i].character.gameObject == character)
            {
                var entity = character.GetComponent<Entity>();
                var speed = entity.GetStat(Stats.Speed);

               
                turnList[i].PreviewInitiativeValue = (Constants.BaseCost / (float)speed.statValue) * turnList[i].index;

                // Move this turn upward if needed
                RepositionTurnEntry(turnList, i);
                currentCount++;
            }

            if (currentCount >= entryCount)
            {
                break;
            }
        }

        turnOrderDisplay.StartPreview(turnList, character.GetComponent<Entity>());
    }

    private void RepositionTurnEntry(List<TurnOrderPreviewObject> list, int oldIndex)
    {
        var entry = list[oldIndex];
        list.RemoveAt(oldIndex);

        int newIndex = list.FindIndex(x => x.PreviewInitiativeValue > entry.PreviewInitiativeValue);
        if (newIndex == -1) list.Add(entry); // Goes to the end


        else list.Insert(newIndex, entry);
    }

    
    public void UndoPreview()
    {
        turnOrderDisplay.CancelPreview();
    }
    
    public void ConfirmPreview()
    {
        turnOrderDisplay.ConfirmPreview();
        SetTurnOrder();
    }

    
    public void EndTurn()
    {
        if (turnList.Count == 0) return;

        var finishedTurn = turnList[0];
        turnList.RemoveAt(0);

        var entity = finishedTurn.character;
        var speed = entity.GetStat(Stats.Speed).statValue;
        var baseThreshold = Constants.BaseCost;

        // Create a new turn entry using next index
        int maxIndex = turnList
            .Where(t => t.character == entity)
            .Select(t => t.index)
            .DefaultIfEmpty(0)
            .Max();

        int newIndex = maxIndex + 1;

        float newInitiativeValue = ((float)baseThreshold * (float)newIndex) / (float)speed;
        var nextTurn = new TurnOrderPreviewObject(entity, newInitiativeValue, newIndex);

        // Insert into the correct position
        int insertIndex = turnList.FindIndex(x => x.PreviewInitiativeValue > newInitiativeValue);
        if (insertIndex == -1)
            turnList.Add(nextTurn);
        else
            turnList.Insert(insertIndex, nextTurn);

        // Animate the pop-off and begin the next turn when ready
        turnOrderDisplay.PopOffFirst(StartTurn);
    }


    public void StartTurn()
    {
        turnList.First().character.StartTurn();
        setActiveCharacter.Raise(turnList.First().character.gameObject);
        turnOrderDisplay.SetTurnOrderList(turnList);
    }

    public void HandleCharacterSpawning(GameObject character)
    {
        var currentCharacter = character.GetComponent<Entity>();
        var newCharacterPreviewList = new List<TurnOrderPreviewObject>();
        for (int i = 1; i < 6; i++)
        {
            newCharacterPreviewList.Add(new TurnOrderPreviewObject(currentCharacter, ((Constants.BaseCost / (float)currentCharacter.GetStat(Stats.Speed).statValue)) * i, i));
        }  
        
        turnList.AddRange(newCharacterPreviewList);
        playerList.Add(currentCharacter);
        turnList = turnList.OrderBy(x => x.PreviewInitiativeValue).ToList();
        turnOrderDisplay.SetTurnOrderList(turnList);
    }

    public void HandleCharacterDespawning(GameObject character)
    {
        var currentCharacter = character.GetComponent<Entity>();
        turnList.RemoveAll(x => x.character == currentCharacter);
        turnOrderDisplay.SetTurnOrderList(turnList);
    }

    public void HandleTurnOrderPreview(int turnCost)
    {
        
    }
}