using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [SerializeField, HideInInspector] private Vector3[] controlPoints = new Vector3[0]; 



    public Vector3 computePoint(float t)
    {
        return Vector3.zero;
    }
}
