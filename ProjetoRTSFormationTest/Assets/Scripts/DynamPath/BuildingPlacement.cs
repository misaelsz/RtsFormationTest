using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingPlacement : MonoBehaviour
{
    public List<GameObject> buildings = new List<GameObject>();
    public GameObject currentBuilding;

    public bool hitBuilding;

    private Grid grid;

    private void Start()
    {
        grid = GameObject.FindGameObjectWithTag("A*").GetComponent<Grid>();
    }

    private void Update()
    {
        Ray interactionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit interactionInfo;
        if (Physics.Raycast(interactionRay, out interactionInfo, Mathf.Infinity))
        {
            if (currentBuilding != null)
            {
                Node nodeToChange = grid.NodeFromWorldPoint(interactionInfo.point);
                Vector3 moveTransform = nodeToChange.nodeWorldPosition;
                currentBuilding.transform.position = moveTransform;

                if(Input.GetKeyUp(KeyCode.R))
                  currentBuilding.transform.Rotate(currentBuilding.transform.rotation.x, currentBuilding.transform.rotation.y + 90, currentBuilding.transform.rotation.z); 

                if (Input.GetMouseButtonUp(0) && hitBuilding == false && !EventSystem.current.IsPointerOverGameObject())
                {
                    UnitManager.instance.buildMode = false; 
                    Destroy(currentBuilding.GetComponent<Rigidbody>());
                    currentBuilding.GetComponent<Collider>().isTrigger = true;
                    for (int i = 0; i < currentBuilding.GetComponent<DynamicObstacle>().buildingTransforms.Count; i++)
                    {
                        Vector3 posToScan = currentBuilding.GetComponent<DynamicObstacle>().buildingTransforms[i].position;
                        Node nodeToScan = grid.NodeFromWorldPoint(posToScan);
                        nodeToScan.walkable = false;
                        currentBuilding.GetComponent<DynamicObstacle>().occupiedNodes.Add(nodeToScan);
                        Destroy(currentBuilding.GetComponent<DynamicObstacle>().buildingTransforms[i].gameObject);
                    }
                    if (UnitManager.instance.movingUnits.Count > 0)
                    {
                        Debug.Log("Number of units in unit manager: " + UnitManager.instance.movingUnits.Count);
                        for (int i = 0; i < UnitManager.instance.movingUnits.Count; i++)
                        {
                            for (int y = 0; y < UnitManager.instance.movingUnits[i].GetComponent<Unit>().path.Length; y++)
                            {
                                for (int z = 0; z < currentBuilding.GetComponent<DynamicObstacle>().occupiedNodes.Count; z++)
                                {
                                    if (UnitManager.instance.movingUnits[i].GetComponent<Unit>().path[y] ==
                                        currentBuilding.GetComponent<DynamicObstacle>().occupiedNodes[z].nodeWorldPosition)
                                    {
                                        UnitManager.instance.movingUnits[i].GetComponent<Unit>().stopMoving = true;
                                        UnitManager.instance.movingUnits[i].GetComponent<Unit>().moveFSM = Unit.MoveFSM.recalculatePath;
                                        break;
                                    }
                                }
                            }
                        }
   

                    }
                    GameObject building = currentBuilding;
                    building.name = "Building";
                    building.layer = LayerMask.NameToLayer("Unwalkable");
                    building.transform.position = currentBuilding.transform.position;
                    currentBuilding = null;

                }
            }

            if (interactionInfo.collider.transform.gameObject.name == "Building" && Input.GetKeyDown(KeyCode.B))
            {
                GameObject buildingToDestroy = interactionInfo.collider.transform.gameObject;

                for (int i = 0; i < buildingToDestroy.GetComponent<DynamicObstacle>().occupiedNodes.Count; i++)
                {
                    buildingToDestroy.GetComponent<DynamicObstacle>().occupiedNodes[i].walkable = true;

                }
                Destroy(buildingToDestroy);
            }
        }
    }
}



