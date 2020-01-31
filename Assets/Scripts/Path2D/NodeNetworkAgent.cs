using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Path2D;
using Persistance;
using CustomAttributes;

[System.Serializable]
public class NodeNetworkAgent
{
#pragma warning disable 649
    [SerializeField]
    private Vector2 _size;
    [SerializeField][HideInInspector]
    private LayerMask _networkLayerMask;
    public TerrainType[] WalkableTerrainTypes;
#pragma warning restore 649
    public Vector2 Size { get { return _size; } }
    
    public LayerMask NetworkLayerMask { get { return _networkLayerMask; } }

    [SerializeField]
    private SerializableDictionary<int, int> _walkableTerrainTypesDictionary = new SerializableDictionary<int, int>();
    public SerializableDictionary<int, int> WalkableTerrainTypesDictionary { get { return _walkableTerrainTypesDictionary; } }

    public void UpdateLayerMask()
    {
        _walkableTerrainTypesDictionary = new SerializableDictionary<int, int>();
        _networkLayerMask = 0;
        foreach (var terrainType in WalkableTerrainTypes)
        {

            if ((terrainType.TerrainMask == (1 << NodeNetwork.UnwalkableLayer | terrainType.TerrainMask) ||
                (terrainType.TerrainMask == (1 << NodeNetwork.CustomLayer | terrainType.TerrainMask))) && terrainType.TerrainMask > 0)
            {
                Debug.LogError("Cannot add unwalkable or custom layer to walkable terrain.");
                terrainType.TerrainMask = 0;
            }
            else if (!_walkableTerrainTypesDictionary.Has(terrainType.TerrainMask.value))
            {
                _walkableTerrainTypesDictionary.Add(terrainType.TerrainMask.value, terrainType.TerrainPenalty);
                _networkLayerMask |= (1 << terrainType.TerrainMask);
            }
        }

        _networkLayerMask |= 1 << NodeNetwork.UnwalkableLayer;
    }
}
