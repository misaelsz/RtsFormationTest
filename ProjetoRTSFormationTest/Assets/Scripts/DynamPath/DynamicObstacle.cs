using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DynamicObstacle : MonoBehaviour
{
    public List<Node> occupiedNodes = new List<Node>();
    public List<Transform> buildingTransforms = new List<Transform>();

    private void Start()
    {
        foreach(Transform child in transform)
        {
            buildingTransforms.Add(child); 
        }
    }
}
