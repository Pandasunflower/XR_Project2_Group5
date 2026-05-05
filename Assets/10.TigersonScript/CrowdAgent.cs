using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdAgent : MonoBehaviour
{
    public Animator animator;

    [HideInInspector]
    public float timer = 0f;

    [HideInInspector]
    public float updateInterval = 0.1f;
}
