using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameObj_Creature : GameObj, IDamageable
{
    public XpGained onXpGain;
    [SerializeField] private Vector2 lastMovementVector;

    private BehaviourHandler<GameObj_Creature> behaviourHandler = null;
    [SerializeField] private float health = 100;
    [SerializeField] private float maxhealth = 100;
    [SerializeField] private float hitColorSpeed = 0.1f;
    [SerializeField] private float xp = 40;
    [SerializeField] private GameObject healthBar;
    public int killCount = 0;
    public CreatureState currentState;
    public GameObj_Creature leader;

    public override void Spawned()
    {
        base.Spawned();
        healthBar = UIManager.uiManager.SpawnObjectAtWorldCanvas(PrefabManager.prefabManager.GetPrefabOf("healthBar"));
        healthBar.GetComponent<HealthBar>().AttachObj(this);
        healthBar.GetComponent<IBootable>().Boot();
    }

    public override void Update()
    {
        MoveObject(lastMovementVector, Time.deltaTime);
        rareUpdateTimeCounter += Time.deltaTime;
        if (rareUpdateTimeCounter > rareUpdateTickTime)
        {
            rareUpdateTimeCounter = 0;
            RareTick();
        }
        foreach (var behaviour in installedBehaviours)
        {
            behaviour?.Tick(null, Time.deltaTime);
        }
    }

    public void UpdateCharacterMovement(Vector2 axis)
    {
        lastMovementVector = axis;
    }

    public override void Possess<GameObj_Creature>(ThingDef entity, string faction)
    {
        onXpGain?.RemoveAllListeners();
        base.Possess<GameObj_Creature>(entity, faction);
        movementSpeed = float.Parse(possessedThing.FindStatByName("MovementSpeed").Value, CultureInfo.InvariantCulture);
        health = float.Parse(possessedThing.FindStatByName("Health").Value, CultureInfo.InvariantCulture);
        maxhealth = health;
        PossessEquipments(entity, faction);
        killCount = 0;

        onHealthChange.Invoke(new HealthInfo()
        {
            currentHealth = health,
            damageTaken = 0,
            changeMax = true
        });

        onActivationChange?.Invoke(true);
    }


    public virtual void PossessEquipments(ThingDef entity, string faction)
    {
        if (entity == null || entity.equipmentNames == null || entity.equipmentNames == Array.Empty<string>()) return;
        foreach (var equipment in entity.equipmentNames)
        {
            if (equipment == null || equipment == string.Empty) continue;
            var thingDef = AssetManager.assetLibrary.thingDefsDictionary.FirstOrDefault(x => x.Key == equipment).Value;
            var prefab = PrefabManager.prefabManager.GetPrefabOf("equipment");
            var spawnedObj = Instantiate(prefab).GetComponent<GameObj>();
            spawnedObj.transform.parent = gameObject.transform;
            spawnedObj.Possess<GameObj_Shooter>(YKUtility.FromXElement<ThingDef>(thingDef), faction);
            GameObj_Shooter shooter = (GameObj_Shooter)spawnedObj;
            shooter.onProjectileHit += OnHitToEnemy;
        }
    }

    public void OnHitToEnemy(HitResult result)
    {
        if (result.killed)
        {
            Debug.Log("[XP] Target killed.. Got XP.");
            float xpToGrant = result.hitTarget.GetComponent<IDamageable>().GetXP();
            onXpGain.Invoke(xpToGrant);
        }
    }

    public void Heal(float amount, bool bypassMax){
        health += amount;

        if(health > maxhealth && !bypassMax) {
            health = maxhealth;
        }


        onHealthChange?.Invoke(new HealthInfo()
        {
            currentHealth = health,
            damageTaken = -amount
        });
    }

    public bool TryDamage(float amount, out bool endedUpKilling)
    {
        health -= amount;

        var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDamageTakenSounds);
        AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);


        if (!IsHealthDepleted()) StartCoroutine(HitColorChange());
        onHealthChange?.Invoke(new HealthInfo()
        {
            currentHealth = health,
            damageTaken = amount
        });

        endedUpKilling = TryDestroy();
        return true;
    }

    public IEnumerator HitColorChange()
    {
        ownedSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitColorSpeed);
        ownedSpriteRenderer.color = Color.black;
        yield return new WaitForSeconds(hitColorSpeed);
        ownedSpriteRenderer.color = Color.white;
    }

    public bool TryDestroy()
    {
        if (IsHealthDepleted())
        {
            var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDeathSounds);
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);

            gameObject.SetActive(false);
            ownedSpriteRenderer.color = Color.white;
            onActivationChange?.Invoke(false);
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetHealth()
    {
        return health;
    }

    public bool IsHealthDepleted()
    {
        return health <= 0;
    }

    public float GetXP()
    {
        return xp;
    }

    public enum CreatureState
    {
        Idle,
        Moving,
        OnAction,
        Dead
    }

    [System.Serializable]
    public class XpGained : UnityEvent<float>
    {
    }
}

