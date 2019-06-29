using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingPoolHandler : MonoBehaviour
{
    [SerializeField]
    private List<GenericBuilding> _SpawnedBuildings, _ReserveBuildings;

    [SerializeField]
    private List<BuildingPrefClass> _PrefabList;

    [SerializeField]
    private Transform _PoolTransform;

    private void Awake()
    {
        Assert.IsNotNull(_PoolTransform);
        Assert.IsNotNull(_PrefabList);
        Assert.IsNotNull(_ReserveBuildings);
    }

    public GenericBuilding RequestGenericBuilding(BuildingType type)
    {
        var buildingObj = _ReserveBuildings.Find(_ => _.BuildingType == type);

        if (buildingObj == null)
        {
            var tempObj = Instantiate(_PrefabList.Find(_ => _.BuildingType == type).Prefab, _PoolTransform);
            tempObj.SetActive(false);
            buildingObj = tempObj.GetComponent<GenericBuilding>();
        }
        else
        {
            _ReserveBuildings.Remove(buildingObj);
        }
        _SpawnedBuildings.Add(buildingObj);
        return buildingObj;
    }

    public void ReturnToPool(GenericBuilding bldng)
    {
        bldng.transform.position = new Vector3(-100, 0, 0);
        _ReserveBuildings.Add(bldng);
        bldng.gameObject.SetActive(false);
    }
}