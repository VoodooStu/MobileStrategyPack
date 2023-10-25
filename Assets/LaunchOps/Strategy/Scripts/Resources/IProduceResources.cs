using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ResourceRate
{
    public ResourceType Type;
    public float Rate;

   
}

public interface IProduceResources
{
    public void RegisterProduction();
    public List<ResourceRate> CalculateProductionRate();

}
