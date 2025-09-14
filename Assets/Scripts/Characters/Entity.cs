using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TacticsToolkit
{
    //Parent Class for Characters and Enemys
    public class Entity : MonoBehaviour
    {
        [Header("Character Specific")]
        public List<AbilityContainer> abilitiesForUse;

        [Header("Level")]
        public int level;
        public int experience = 0;
        public int requiredExperience = 0;

        [Header("General")]
        public TeamType teamID = 0;
        [HideInInspector]
        public OverlayTile activeTile;
        public CharacterClass characterClass;
        [HideInInspector]
        public CharacterStats statsContainer;
        [HideInInspector]
        public int initiativeValue;

        public Sprite portrait;

        [HideInInspector]
        public bool isAlive = true;

        [HideInInspector]
        public bool isActive;

        public GameObject isActiveIndicator;

        public GameEvent endTurn;
        public GameEventGameObject dieEvent;
        private HealthBarManager healthBarManager;
        [HideInInspector]
        public int previousTurnCost = -1;

        private bool isTargetted = false;

        public GameConfig gameConfig;

        private int initiativeBase = 1000;
        private float i;

        public Vector2 facingDirection = new Vector2(1, 0);

        public bool isRanged = false;
        public Projectile projectile;

        [HideInInspector]
        public SpriteRenderer myRenderer;
        public Vector3 spriteOffset = Vector3.zero;
        private Vector3 initialSpritePosition = Vector3.zero;

        public Ability EnemySpell;
        public Ability AllySpell;
        public Ability SelfSpell;
        
        public bool isActing = false;

        public GameEventGameObject triggerProjectile;
        public GameEventGameObject setTarget;
        
        
        protected bool hasAttacked = false;
        protected bool hasMoved = false;
        
        public List<Action> actionQueue = new List<Action>();

        public enum TeamType
        {
            Player = 0,
            Ally = 1,
            Enemy = 2
        }
        
        private void Awake()
        {
            SpawnCharacter();
        }

        public void SpawnCharacter()
        {
            SetAbilityList();
            SetStats();
            requiredExperience = gameConfig.GetRequiredExp(level);
            healthBarManager = GetComponent<HealthBarManager>();
            myRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            initiativeValue = Mathf.RoundToInt(initiativeBase / GetStat(Stats.Speed).statValue);
            initialSpritePosition = myRenderer.transform.position;
        }

        //Setup the statsContainer and scale up the stats based on level. 
        public void SetStats()
        {
            if (statsContainer == null)
            {
                statsContainer = ScriptableObject.CreateInstance<CharacterStats>();
                statsContainer.Health = new Stat(Stats.Health, characterClass.Health.baseStatValue, this);
                statsContainer.Mana = new Stat(Stats.Mana, characterClass.Mana.baseStatValue, this);
                statsContainer.Strenght = new Stat(Stats.Strenght, characterClass.Strenght.baseStatValue, this);
                statsContainer.Endurance = new Stat(Stats.Endurance, characterClass.Endurance.baseStatValue, this);
                statsContainer.Speed = new Stat(Stats.Speed, characterClass.Speed.baseStatValue, this);
                statsContainer.Intelligence = new Stat(Stats.Intelligence, characterClass.Intelligence.baseStatValue, this);
                statsContainer.MoveRange = new Stat(Stats.MoveRange, characterClass.MoveRange, this);
                statsContainer.AttackRange = new Stat(Stats.AttackRange, characterClass.AttackRange, this);
                statsContainer.CurrentHealth = new Stat(Stats.CurrentHealth, characterClass.Health.baseStatValue, this);
                statsContainer.CurrentMana = new Stat(Stats.CurrentMana, characterClass.Mana.baseStatValue, this);
                for (int i = 0; i < level; i++)
                {
                    LevelUpStats();
                }
            }

        }

        // Update is called once per frame
        public void Update()
        {
            if (isTargetted)
            {
                //Just a Color Lerp for when a character is targetted for an attack. 
                i += Time.deltaTime * 0.5f;
                myRenderer.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 0.5f, 0, 1), Mathf.PingPong(i * 2, 1));
            }

            myRenderer.transform.position = initialSpritePosition + spriteOffset;
        }

        public void UpdateInitialSpritePosition(Vector3 spritePosition) => initialSpritePosition = spritePosition;

        public void UpdateOffset(Vector3 newSpriteOffset) => spriteOffset = newSpriteOffset;

        //Get's all the available abilities from the characters class. 
        public void SetAbilityList()
        {
            abilitiesForUse = new List<AbilityContainer>();
            foreach (var ability in characterClass.abilities)
            {
                if (level >= ability.requiredLevel)
                    abilitiesForUse.Add(new AbilityContainer(ability));
            }
        }

        //Scale up attributes based on a weighted random. 
        public void LevelUpStats()
        {
            float v = Random.Range(0f, 1f);
            statsContainer.Health.ChangeStatValue(statsContainer.Health.statValue + Mathf.RoundToInt(characterClass.Health.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Mana.ChangeStatValue(statsContainer.Mana.statValue + Mathf.RoundToInt(characterClass.Mana.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Strenght.ChangeStatValue(statsContainer.Strenght.statValue + Mathf.RoundToInt(characterClass.Strenght.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Endurance.ChangeStatValue(statsContainer.Endurance.statValue + Mathf.RoundToInt(characterClass.Endurance.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Speed.ChangeStatValue(statsContainer.Speed.statValue + Mathf.RoundToInt(characterClass.Speed.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Intelligence.ChangeStatValue(statsContainer.Intelligence.statValue + Mathf.RoundToInt(characterClass.Intelligence.baseStatModifier.Evaluate(v) * 10));

            statsContainer.CurrentHealth.ChangeStatValue(statsContainer.Health.statValue);
            statsContainer.CurrentMana.ChangeStatValue(statsContainer.Mana.statValue);
        }

        //Scale down attributes based on a weighted random. 
        public void LevelDownStats()
        {
            float v = Random.Range(0f, 1f);
            statsContainer.Health.ChangeStatValue(statsContainer.Health.statValue - Mathf.RoundToInt(characterClass.Health.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Mana.ChangeStatValue(statsContainer.Mana.statValue - Mathf.RoundToInt(characterClass.Mana.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Strenght.ChangeStatValue(statsContainer.Strenght.statValue - Mathf.RoundToInt(characterClass.Strenght.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Endurance.ChangeStatValue(statsContainer.Endurance.statValue - Mathf.RoundToInt(characterClass.Endurance.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Speed.ChangeStatValue(statsContainer.Speed.statValue - Mathf.RoundToInt(characterClass.Speed.baseStatModifier.Evaluate(v) * 10));
            v = Random.Range(0f, 1f);
            statsContainer.Intelligence.ChangeStatValue(statsContainer.Intelligence.statValue - Mathf.RoundToInt(characterClass.Intelligence.baseStatModifier.Evaluate(v) * 10));

            statsContainer.CurrentHealth.ChangeStatValue(statsContainer.Health.statValue);
            statsContainer.CurrentMana.ChangeStatValue(statsContainer.Mana.statValue);
        }

        //Level up stats and get the new required experience for the next level. 
        public void LevelUpCharacter()
        {
            level++;
            LevelUpStats();
            requiredExperience = gameConfig.GetRequiredExp(level);
        }

        public void IncreaseExp(int value)
        {
            experience += value;

            while (experience >= requiredExperience)
            {
                experience -= requiredExperience;
                LevelUpCharacter();
            }
        }

        //Level down stats and get the new required experience for the next level. 
        public void LevelDownCharacter()
        {
            level--;
            LevelDownStats();
            requiredExperience = gameConfig.GetRequiredExp(level);
        }

        //Update the characters initiative after the perform an action. This is used for Dynamic Turn Order. 
        public void UpdateInitiative(int turnValue)
        {
            initiativeValue += Mathf.RoundToInt(turnValue / GetStat(Stats.Speed).statValue);
            previousTurnCost = turnValue;
        }

        //Entity is being targets for an attack. 
        public void SetTargeted(bool focused = false)
        {
            isTargetted = focused;

            if (isAlive)
            {
                if (isTargetted)
                {
                    myRenderer.color = new Color(1, 0, 0, 1);
                }
                else
                {
                    myRenderer.color = new Color(1, 1, 1, 1);
                }
            }
        }

        //Take damage from an attack or ability. 
        public void TakeDamage(int damage, bool ignoreDefence = false)
        {
            int damageToTake = ignoreDefence ? damage : CalculateDamage(damage);

            if (damageToTake > 0)
            {
                statsContainer.CurrentHealth.statValue -= damageToTake;
                StartCoroutine(ShowDamage());
                healthBarManager.UpdateCharacterUI();
                if (GetStat(Stats.CurrentHealth).statValue <= 0)
                {
                    isAlive = false;
                    StartCoroutine(Die());
                    UnlinkCharacterToTile();

                    if (isActive)
                        endTurn.Raise();
                }
            }
        }

        private IEnumerator ShowDamage()
        {
            myRenderer.color = new Color(1, 0f, 0, 1);

            yield return new WaitForSeconds(0.25f);
            myRenderer.color = new Color(1, 0.5f, 0.5f, 1);

            yield return new WaitForSeconds(0.25f);
            myRenderer.color = new Color(1, 1, 1, 1);
        }
        
        private IEnumerator ShowHeal()
        {
            myRenderer.color = new Color(0, 1f, 0, 1);

            yield return new WaitForSeconds(0.25f);
            myRenderer.color = new Color(1, 1f, 0.5f, 1);

            yield return new WaitForSeconds(0.25f);
            myRenderer.color = new Color(1, 1, 1, 1);
        }

        public void HealEntity(int value)
        {
            statsContainer.CurrentHealth.statValue += value;
            
            if(statsContainer.CurrentHealth.statValue > statsContainer.Health.statValue)
                statsContainer.CurrentHealth.statValue = statsContainer.Health.statValue;
            
            StartCoroutine(ShowHeal());
            healthBarManager.UpdateCharacterUI();
        }

        //basic example if using a defencive stat
        public int CalculateDamage(int damage)
        {
            var endurance = (float)GetStat(Stats.Endurance).statValue;
            float percentage = ((endurance / (float)damage) * 100) / 2;

            percentage = percentage > 75 ? 75 : percentage;

            int damageToTake = damage - Mathf.CeilToInt((float)(percentage / 100f) * (float)damage);
            return damageToTake;
        }

        //Get a perticular stat object. 
        public Stat GetStat(Stats statName)
        {
            switch (statName)
            {
                case Stats.Health:
                    return statsContainer.Health;
                case Stats.Mana:
                    return statsContainer.Mana;
                case Stats.Strenght:
                    return statsContainer.Strenght;
                case Stats.Endurance:
                    return statsContainer.Endurance;
                case Stats.Speed:
                    return statsContainer.Speed;
                case Stats.Intelligence:
                    return statsContainer.Intelligence;
                case Stats.MoveRange:
                    return statsContainer.MoveRange;
                case Stats.CurrentHealth:
                    return statsContainer.CurrentHealth;
                case Stats.CurrentMana:
                    return statsContainer.CurrentMana;
                case Stats.AttackRange:
                    return statsContainer.AttackRange;
                default:
                    return statsContainer.Health;
            }
        }

        //What happens when a character dies. 
        public IEnumerator Die()
        {
            dieEvent.Raise(gameObject);
            
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
            yield return null;
        }

        //Change characters mana
        public void UpdateMana(int value) => statsContainer.CurrentMana.statValue -= value;

        //Attach an effect to the Entity from a tile or ability. 
        public void AttachEffect(ScriptableEffect scriptableEffect)
        {
            if (scriptableEffect)
            {
                var statToEffect = GetStat(scriptableEffect.statKey);

                if (statToEffect.statMods.FindIndex(x => x.statModName == scriptableEffect.name) != -1)
                {
                    int modIndex = statToEffect.statMods.FindIndex(x => x.statModName == scriptableEffect.name);
                    statToEffect.statMods[modIndex] = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name, scriptableEffect.description);
                }
                else
                    statToEffect.statMods.Add(new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name, scriptableEffect.description));
            }
        }

        //Effects that don't have a duration can just be applied straight away. 
        public void ApplySingleEffects(ScriptableEffect scriptableEffect)
        {
            if (scriptableEffect)
            {
                var statMod = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value,
                    scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name,
                    scriptableEffect.description);
                Stat value = statsContainer.getStat(scriptableEffect.GetStatKey());
                value.ApplySingleStatMod(statMod);
                healthBarManager.UpdateCharacterUI();
            }
        }

        //Effects that don't have a duration should be manually removed. 
        public void UndoEffect(ScriptableEffect scriptableEffect)
        {
            var statMod = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name, scriptableEffect.description);
            Stat value = statsContainer.getStat(scriptableEffect.GetStatKey());
            value.UndoStatMod(statMod);
            healthBarManager.UpdateCharacterUI();
        }


        //Apply all the currently attached effects. Happens when a new turn begins. 
        public void ApplyEffects()
        {
            var fields = typeof(CharacterStats).GetFields();

            foreach (var item in fields)
            {
                var type = item.FieldType;
                Stat value = (Stat)item.GetValue(statsContainer);

                value.ApplyStatMods();
                value.TickStatMods();
            }

            healthBarManager.UpdateCharacterUI();
        }

        public List<StatModifier> GetStatModifiers()
        {
            List<StatModifier> mods = new List<StatModifier>();
            var fields = typeof(CharacterStats).GetFields();
            foreach (var item in fields)
            {
                var type = item.FieldType;
                Stat value = (Stat)item.GetValue(statsContainer);

                mods.AddRange(value.GetStatMods());
            }

            return mods;
        }

        //Gets Entities ability. 
        public AbilityContainer GetAbilityByName(string abilityName)
        {
            return abilitiesForUse.Find(x => x.ability.Name == abilityName);
        }

        public virtual void StartTurn()
        {
            SetIsActive(true);
            
            myRenderer.material.SetFloat("_Enabled", 1);
            ApplyEffects();
        }
        public virtual void CharacterMoved()
        {
        }

        //When an Entity moves, link it to the tiles it's standing on. 
        public void LinkCharacterToTile(OverlayTile tile)
        {
            UnlinkCharacterToTile();
            tile.activeCharacter = this;
            activeTile = tile;
            activeTile.isBlocked = true;
        }

        //Unlink an entity from a previous tile it was standing on. 
        public void UnlinkCharacterToTile()
        {
            if (activeTile)
            {
                activeTile.activeCharacter = null;
                activeTile.isBlocked = false;
                activeTile = null;
            }
        }

        public void UpdateFacingDirection(Vector2 direction) {
            var xAbs = Mathf.Abs(direction.x);
            var yAbs = Mathf.Abs(direction.y);

            if (yAbs >= xAbs && direction.y > 0)
            {
                facingDirection = new Vector2(0, 1);
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    180,
                    transform.eulerAngles.z
                );
            }
            if (yAbs >= xAbs && direction.y < 0)
            {
                facingDirection = new Vector2(0, -1);
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    0,
                    transform.eulerAngles.z
                );
            }

            if (xAbs >= yAbs && direction.x > 0)
            {
                facingDirection = new Vector2(1, 0);
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    270,
                    transform.eulerAngles.z
                );
            }

            if (xAbs >= yAbs && direction.x < 0)
            {
                facingDirection = new Vector2(-1, 0);
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    90,
                    transform.eulerAngles.z
                );
            }
        }


        public void FaceGridLocation(Vector2Int targetGridLocation)
        {
            var currentGridlocation = activeTile.grid2DLocation;

            var direction = targetGridLocation - currentGridlocation;
            UpdateFacingDirection(direction);   
        }

        public void SetIsActive(bool active)
        {
            isActive = active;

            if(isActiveIndicator)
                isActiveIndicator.SetActive(active);
        }

        public virtual void SetActiveTile(OverlayTile tile)
        {
        }
        
        
        public virtual void SetRenderers(LineRenderer lineRenderer)
        {
        }
        
        public virtual void TriggerNextAction()
        {
        }


        public virtual void ActionButtonPressed()
        {
        }

        public virtual void TriggerAction()
        {
            if (actionQueue.Count > 0)
            {
                if(actionQueue[0].Ability.abilityFX)
                    triggerProjectile.Raise(actionQueue[0].Ability.abilityFX);
                else
                    actionQueue[0].DoAction();
            }
        }
        
        public void EndTurn()
        {
            SetIsActive(false);
            myRenderer.material.SetFloat("_Enabled", 0);
            endTurn.Raise();
        }
    }
}
