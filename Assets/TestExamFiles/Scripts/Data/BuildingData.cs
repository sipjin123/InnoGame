using System.Collections.Generic;
using UnityEngine;

public class BuildingData : ScriptableObject
{
    public BuildingType BuildingType;
    public List<ResourceCostClass> ResourceCost;
    public List<ResourceCostClass> ResourceGathered;
    public int ProductionTime;
    public int BuildTime;
    public bool AutoProduce;
    public bool Decoration;
}