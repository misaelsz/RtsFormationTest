using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSense : MonoBehaviour
{
    private BuildingPlacement buildingPlacement; 

    private void Start()
    {
        buildingPlacement = GameObject.FindGameObjectWithTag("Overseer").GetComponent<BuildingPlacement>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Unwalkable") || col.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            buildingPlacement.hitBuilding = true; 
        }
    }

    private void OnTriggerStray(Collider  col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Unwalkable") || col.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            buildingPlacement.hitBuilding = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Unwalkable") || col.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            buildingPlacement.hitBuilding = false; 
        }
    }
}
