using System;
using UnityEngine;

public abstract class SpacerAttribute : PropertyAttribute
{
    protected float Size;

    protected SpacerAttribute() => throw new NotImplementedException();
}