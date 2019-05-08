using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildButton : MonoBehaviour, IPointerClickHandler
{
    public BuildingPlacement buildingPlacement; 

    private void Start()
    {
        buildingPlacement = GameObject.FindGameObjectWithTag("Overseer").GetComponent<BuildingPlacement>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(buildingPlacement.currentBuilding == null)
        {
            for(int i = 0; i < buildingPlacement.buildings.Count; i++)
            {
                if (gameObject.name == buildingPlacement.buildings[i].name)
                {
                    buildingPlacement.currentBuilding = Instantiate(buildingPlacement.buildings[i]);
                    UnitManager.instance.buildMode = true; 
                }
            }
        }        
    }
}