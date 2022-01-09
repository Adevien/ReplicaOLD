using Replica.Runtime;
using System;
using UnityEngine;

public class CD : NetworkBehaviour
{
    public string Name;

    [NetVar(nameof(OnHealthChanged))] public float Health { get; set; }

    public void OnHealthChanged(float value)
    {
        Debug.Log($"[{Name}] Health Changed");
    }
}