using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Unity.Collections;
using UnityEngine;

public class SpawnManager : Manager
{
    public static SpawnManager spawnManager;
    [SerializeField] public Dictionary<string, ThingDef> spawnableEnemiesDictionary = new();

    public float RNP_PointDistance = 5f;
    public float RNP_Max = 1.25f;
    public float RNP_Min = 1f;
    public bool SPW_Activated = true;
    public float SPW_DelayBetweenSpawns = 0.7f;
    public float SPW_FirstDelay = 2f;
    
    [SerializeField] private string EnemyTeamName = "enemies";
    [SerializeField] private string PlayerTeamName = "players";

    public override IEnumerator Kickstart()
    {
        spawnManager = this;
        InitializeSpawnables(AssetManager.assetLibrary.thingDefsDictionary);
        SpawnPlayer(AssetManager.assetLibrary.thingDefsDictionary);
        StartCoroutine(EnemySpawnTick());
        yield return this;
    }

    public void SpawnPlayer(Dictionary<string, XElement> definitionsList)
    {
        foreach (var definition in definitionsList)
        {
            XElement definitionType = null;
            definitionType = definition.Value.Element("spawnable")?.Element("faction");
            if (definitionType != null && definitionType.Value.Equals("player"))
            {
                Debug.Log("FHX: " + definition.Value);
                var player = (GameObj_Creature)PoolManager.poolManager.creaturesPool.HardSet(PrefabManager.prefabManager.GetPrefabOf("player"), InitializeDef(definition.Value), Vector2.zero, 0, PlayerTeamName);
                PlayerController.playerController.ownedCreature = player;
                StartCoroutine(PlayerController.playerController.TryFetchCreature());
                break;
            }
        }
    }

    public IEnumerator EnemySpawnTick()
    {
        yield return new WaitForSeconds(SPW_FirstDelay);

        while (SPW_Activated)
        {
            PoolManager.poolManager.creaturesPool.ObtainSlotForType(GetRandomEnemy(), (Vector2)GetRandomSpawnPosition(), 0, EnemyTeamName);
            yield return new WaitForSeconds(SPW_DelayBetweenSpawns);
        }
    }

    public ThingDef InitializeDef(XElement definition)
    {
        Debug.Log("[SPAWN MANAGER] Loading creatures to cache.. " + definition);
        var entity = YKUtility.FromXElement<ThingDef>(definition);

        return entity;
    }

    public void InitializeSpawnables(Dictionary<string, XElement> definitionsList)
    {
        foreach (var definition in definitionsList)
        {
            var definitionType = definition.Value.Element("spawnable")?.Element("faction");
            if (definitionType != null && definitionType.Value.Equals("enemy"))
            {
                spawnableEnemiesDictionary.Add(definition.Key, InitializeDef(definition.Value));
            }
        }
    }

    public ThingDef GetRandomEnemy()
    {
        return spawnableEnemiesDictionary[YKUtility.GetRandomIndex<ThingDef>(spawnableEnemiesDictionary)];
    }

    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = new(UnityEngine.Random.Range(RNP_Min, RNP_Max), UnityEngine.Random.Range(RNP_Min, RNP_Max));
        if (UnityEngine.Random.value > 0.5f) randomPoint = randomPoint * -1;
        randomPoint.z = RNP_PointDistance;
        Vector3 worldPoint = Camera.main.ViewportToWorldPoint(randomPoint);
        return worldPoint;
    }

}
