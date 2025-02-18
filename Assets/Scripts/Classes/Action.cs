using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

    public class Action
    {
        public enum ActionState
        {
            NotStarted,
            InProgress,
            Finished,
        }
        public enum ActionType
        {
            Move,
            Attack
        }

        public List<Vector3> Path;
        public Entity Entity;
        public ActionType Type;
        public ActionState State;
        public OverlayTile Target;
        public Ability Ability;

        private bool isMoving;
        private ShapeParser ShapeParser;
        
        public Action(List<Vector3> path, ActionType actionType, OverlayTile target, Entity entity, Ability ability = null)
        {
            this.Path = path;
            this.Type = actionType;
            this.Target = target;
            this.Ability = ability;
            this.Entity = entity;
            
            State = ActionState.NotStarted;
            ShapeParser = new ShapeParser();
            isMoving = false;
        }

        public void StartAction()
        {
            State = ActionState.InProgress;
            Entity.UpdateFacingDirection(new Vector2(Target.transform.position.x - Entity.transform.position.x, Target.transform.position.z - Entity.transform.position.z));
            
            switch (Type)
            {
                case ActionType.Move:
                    //Trigger movement animation
                    break;
                case ActionType.Attack:
                    //Trigger attack animation
                    Entity.GetComponentInChildren<Animator>().Play("Attack");
                    break;
                default:
                    break;
            }
        }
        
        public void DoAction()
        {
            switch (Type)
            {
                case ActionType.Move:
                    //Do movement
                    isMoving = true;
                    MoveAlongPath();
                    break;
                case ActionType.Attack:
                    //do attack
                    CastAbility();
                    break;
                default:
                    break;
            }
        }
        
        private void MoveAlongPath()
        {
            var step = 10f * Time.deltaTime;
            Entity.transform.position = Vector3.MoveTowards(Entity.transform.position, Path[0], step);
            Entity.UpdateInitialSpritePosition(Entity.transform.position);

            if (!(Vector3.Distance(Entity.transform.position, Path[0]) < 0.0001f)) return;

        
            switch (Path.Count)
            {        
                //second last tile
                case >= 2:
                    Entity.UpdateFacingDirection(new Vector2(Path[1].x - Path[0].x, Path[1].z - Path[0].z));
                    break;
                //last tile
                case 1:
                    PositionCharacterOnTile(Entity, Path[0]);
                    State = ActionState.Finished;
                    isMoving = false;
                    break;
            }
        
            Path.RemoveAt(0);
        }
        
        private static void PositionCharacterOnTile(Entity character, Vector3 tile)
        {
            var overlayTile = MapManager.Instance.GetOverlayByTransform(tile);
            character.transform.position = overlayTile.transform.position;
            character.UpdateInitialSpritePosition(overlayTile.transform.position);
            character.LinkCharacterToTile(overlayTile);
        }

        private void CastAbility()
        {
            var inRangeCharacters = new List<Entity>();
            var abilityAffectedTiles = ShapeParser.GetAbilityTileLocations(Target, Ability.abilityShape, Entity.activeTile.grid2DLocation, false);
            
            if (Ability.includeOrigin)
                abilityAffectedTiles.Add(Target);
            
            //get in range characters
            foreach (var tile in abilityAffectedTiles)
            {
                var targetCharacter = tile.activeCharacter;
                if (targetCharacter &&  targetCharacter.isAlive)
                {
                    inRangeCharacters.Add(targetCharacter);
                }
            }
            
            if (Ability.requiresTarget && inRangeCharacters.Count > 0 || !Ability.requiresTarget)
            {
                //attach effects
                foreach (var character in inRangeCharacters)
                {
                    foreach (var effect in Ability.effects)
                    {
                        if (effect.Duration == 0)
                            character.ApplySingleEffects(effect);
                        else
                            character.AttachEffect(effect);
                    }

                    //apply value
                    switch (Ability.abilityType)
                    {
                        case Ability.AbilityTypes.Heal:
                            character.HealEntity(Ability.value);
                            break;
                        case Ability.AbilityTypes.Damage:
                            character.TakeDamage(Ability.value);
                            break;
                        case Ability.AbilityTypes.All:
                            character.TakeDamage(Ability.value);
                            break;
                        default:
                            break;
                    }

                    foreach (var effect in Ability.effects)
                    {
                        if (effect.Duration == 0)
                            character.UndoEffect(effect);
                    }
                }
                
                Entity.UpdateInitiative(Constants.AbilityCost);
            }
            State = ActionState.Finished;
        }
        
        private bool CheckAbilityTargets(Ability.AbilityTypes abilityType, Entity characterTarget)
        {
            if (abilityType == Ability.AbilityTypes.Damage)
            {
                return characterTarget.teamID != Entity.teamID;
            }
            else if (abilityType == Ability.AbilityTypes.Heal)
            {
                return characterTarget.teamID == Entity.teamID;
            }

            return true;
        }

    }