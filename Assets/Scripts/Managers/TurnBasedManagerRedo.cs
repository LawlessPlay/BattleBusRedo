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
    public List<TurnOrderObject> turnCombatantList = new List<TurnOrderObject>();
    public List<TurnOrderObject> previewTurnCombatantList = new List<TurnOrderObject>();
    public TMP_Text speedOrderText;
    
    public GameEventGameObject setActiveCharacter;
    public Entity activeCharacter;

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
        speedOrderText.text = "";
        foreach (var turnObject in turnCombatantList)
        {
            speedOrderText.text += turnObject.character.name + " - Speed: " + turnObject.character.GetStat(Stats.Speed).statValue + " CurrentTickValue: " + turnObject.currentTickCount + " MaxTickCount: " + turnObject.tickMax;
            speedOrderText.text += "\n\n";
        }
    }

    public void SetTurnOrder()
    {
        turnCombatantList.Clear();
        turnCombatantList = CreateTurnOrder();
        previewTurnCombatantList = turnCombatantList;
        turnOrderDisplay.SetTurnOrderList(previewTurnCombatantList);
    }

    public List<TurnOrderObject> CreateTurnOrder()
    {
        var totalList = playerList.Concat(enemyList).ToList();
        var tempList = new List<TurnOrderObject>();

        foreach (var character in totalList)
        {
            tempList.Add(new TurnOrderObject(character, Constants.BaseCost));
        }
        
        tempList = tempList.OrderBy(x => x.currentTickCount).ToList();
        return tempList;
    }
    
    public void PreviewUpdateOrder(List<Entity> targets, float multiplier, float speedMultiplier)
    {
        var minTickValue = turnCombatantList[0].currentTickCount;
        
        foreach (var previewTurnCombatant in previewTurnCombatantList)
        {
            previewTurnCombatant.UpdateTickCount(minTickValue);
            if (targets.Contains(previewTurnCombatant.character))
            {
                previewTurnCombatant.RecalculateTickCount(speedMultiplier);
            }
        }
        
        previewTurnCombatantList.First(x => x.character == activeCharacter).ResetTickCount(multiplier);
        previewTurnCombatantList = previewTurnCombatantList.OrderBy(x => x.currentTickCount).ToList();
        
        var affectedCharacters = new List<Entity>();
        affectedCharacters.AddRange(targets);
        affectedCharacters.Add(activeCharacter);
        turnOrderDisplay.StartPreview(previewTurnCombatantList, affectedCharacters);
    }
    
    public void UndoPreview()
    {
        turnOrderDisplay.CancelPreview();
    }
    
    public void ConfirmPreview()
    {
        turnOrderDisplay.ConfirmPreview();
        turnCombatantList = previewTurnCombatantList;
    }
    
    public void EndTurn()
    {
        StartTurn();
    }


    public void StartTurn()
    {
        if(turnOrderDisplay.isPreviewMode)
            ConfirmPreview();
        else
        {
            var minTickValue = turnCombatantList[0].currentTickCount;
            foreach (var characters in turnCombatantList)
            {
                characters.UpdateTickCount(minTickValue);
            }
            turnCombatantList[0].ResetTickCount(1.2f);
            turnCombatantList = turnCombatantList.OrderBy(x => x.currentTickCount).ToList();
            turnOrderDisplay.SetTurnOrderList(turnCombatantList);
        }


        turnCombatantList[0].character.StartTurn();
        setActiveCharacter.Raise(turnCombatantList[0].character.gameObject);
        activeCharacter = turnCombatantList[0].character;
        //turnOrderDisplay.SetTurnOrderList(turnCombatantList);
    }

    public void HandleCharacterSpawning(GameObject character)
    {
        var newTurnOrderObject = new TurnOrderObject(character.GetComponent<Entity>(), Constants.BaseCost);
        turnCombatantList.Add(newTurnOrderObject);
        
        turnCombatantList = turnCombatantList.OrderBy(x => x.currentTickCount).ToList();
        
        previewTurnCombatantList = turnCombatantList;
        turnOrderDisplay.SetTurnOrderList(previewTurnCombatantList);
    }

    public void HandleCharacterDespawning(GameObject character)
    {
        turnCombatantList.RemoveAll(x => x.character == character.GetComponent<Entity>());
        turnCombatantList = turnCombatantList.OrderBy(x => x.currentTickCount).ToList();
        previewTurnCombatantList = turnCombatantList;
        turnOrderDisplay.SetTurnOrderList(previewTurnCombatantList);
    }
}

public class TurnOrderObject
{
    public Entity character;
    public float stepValue;
    public float tickMax;
    public float currentTickCount;
    public float baseTurnValue;

    public TurnOrderObject(Entity character, float baseTurnValue)
    {
       this.character = character;
       this.baseTurnValue = baseTurnValue;
       
       this.stepValue = (float)character.GetStat(Stats.Speed).statValue;
       this.tickMax = baseTurnValue / stepValue;
       this.currentTickCount = tickMax - 10f;
       
       if(this.currentTickCount < 0)
           this.currentTickCount = 0;
    }

    public void UpdateTickCount(float newTickCount)
    {
        this.currentTickCount -= newTickCount;
        
        if(this.currentTickCount < 0)
            this.currentTickCount = 0;
    }

    public void ResetTickCount(float multiplier)
    {
        this.currentTickCount = tickMax * multiplier;
    }

    public void RecalculateTickCount(float speedMultiplier)
    {
        this.stepValue = (float)character.GetStat(Stats.Speed).statValue * speedMultiplier;
        var newTickCount = (baseTurnValue / stepValue);
        var difference = Mathf.Abs(tickMax - newTickCount);
        this.currentTickCount -= difference;
        
        this.tickMax = newTickCount;
    }
}