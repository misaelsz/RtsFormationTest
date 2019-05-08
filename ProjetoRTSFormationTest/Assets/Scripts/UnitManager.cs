using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;
    //gameObject
    public GameObject tileTestCorner;
    public GameObject tileTestEdges;
    //vector3
    public Vector3 originalRightClickPosition;
    public Vector3 newTemp;
    public Vector3 recalculatedTarget; 
    //lists
    public List<GameObject> movingUnits = new List<GameObject>();
    public List<GameObject> units = new List<GameObject>();
    public List<Vector3> occupiedNodes = new List<Vector3>();
    public List<Vector3> listOfVectors = new List<Vector3>();
    //bools
    public bool buildMode;
    public bool foundClosestFreeNode; 
    //FSMs 
    public enum UnitMovement
    {
        rightClickTargetNode,
        calculateMoveArea, 
        createFormation,
        clearLists
    }
    public UnitMovement unitMovement; 

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //when you add a tag to a gameObject, unity creates an array of the all the objects that have that tag
        //FindGameObjectsWithTag is not the same as FindGameObjectWithTag, and it is used to directly access the tagged object array. 
        //here I use this array of objects with the tag 'unit' to make a list of all the unit's in the game at start. 
        //I want a list as opposed an array and units will be added and die during the game (i.e. the number of units will change -not good for arrays). 
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Unit").Length; i++)
        {
            units.Add(GameObject.FindGameObjectsWithTag("Unit")[i]);
        }
    }

    private void Update()
    {
        UnitMovementFSM();
    }

    private void UnitMovementFSM()
    {
        switch(unitMovement)
        {
            case UnitMovement.rightClickTargetNode:
                RightClickTargetNode();
                break;
            case UnitMovement.calculateMoveArea:
                CreateWalkalbeArea(originalRightClickPosition);
                break; 
            case UnitMovement.createFormation:
                FindTargetForSelectedUnits();
                break;
            case UnitMovement.clearLists:
                listOfVectors.Clear();
                unitMovement = UnitMovement.rightClickTargetNode; 
                break; 
        }
    }

    private void RightClickTargetNode()
    {
        if(SelectionManager.instance.currentlySelectedUnits.Count > 0)
        {
            //if units are selected (if the currently selected units list is greater than 0), and if the right mouse button 
            //has been clicked and we are not placing a building....
            if(Input.GetMouseButtonDown(1) && !buildMode)
            {
                //...then send out a ray
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.gameObject.name == "Ground")
                    {
                        //if the ray hits the ground, then take the vector3 of that hit location, 
                        //find the node that corresponds to that vector3, get the node vector3
                        //store it in tempTarget for use in future methods. 
                        originalRightClickPosition = Grid.instance.NodeFromWorldPoint(hit.point).nodeWorldPosition;
                        //remove all selected units from moving units if they are already in that list.
                        //failure to do this will result in a very large moving units list over time. 
                        RemoveFromTargetsAndMoveingUnits();
                        //go to the calculateMoveArea state in the UnitMovement finite state machine. 
                        unitMovement = UnitMovement.calculateMoveArea;
                    }
                }
            }
        }
    }

    private void FindTargetForSelectedUnits()
    {
        List<GameObject> selectedUnits = SelectionManager.instance.currentlySelectedUnits;
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            Vector3 newTarget = originalRightClickPosition;

            if(!Grid.instance.NodeFromWorldPoint(newTarget).walkable || occupiedNodes.Contains(newTarget))
            {
                Debug.Log("need to find another node");
                newTarget = FindClosestAvailableNode(listOfVectors);
                occupiedNodes.Remove(selectedUnits[i].GetComponent<Unit>().target);
                occupiedNodes.Add(newTarget);
                selectedUnits[i].GetComponent<Unit>().target = newTarget;
                selectedUnits[i].GetComponent<Unit>().moveFSM = Unit.MoveFSM.findPosition;
            }
            else if(Grid.instance.NodeFromWorldPoint(newTarget).walkable && !occupiedNodes.Contains(newTarget))
            {
                Debug.Log("no need to find another node"); 
                occupiedNodes.Remove(selectedUnits[i].GetComponent<Unit>().target);
                occupiedNodes.Add(newTarget);
                selectedUnits[i].GetComponent<Unit>().target = newTarget;
                selectedUnits[i].GetComponent<Unit>().moveFSM = Unit.MoveFSM.findPosition;
            }
        }
        unitMovement = UnitMovement.clearLists; 
    }

    private void CreateWalkalbeArea(Vector3 startingPos)
    {
        Vector3 topLeft = new Vector3();
        Vector3 bottomLeft = new Vector3();
        Vector3 topRight = new Vector3();
        Vector3 bottomRight = new Vector3();
        int cornerIncrementer = 1;
        int sideIncrementer = 1;

        for (int i = 0; i < 4; i ++)
        {
            topLeft.x = (startingPos.x - cornerIncrementer);
            topLeft.z = (startingPos.z + cornerIncrementer);
            topLeft.y = .01f; 
       
            listOfVectors.Add(topLeft); 

            EdgeMaker(sideIncrementer, topLeft, 1, 0);
            EdgeMaker(sideIncrementer, topLeft, 0, -1);

            bottomLeft.x = (startingPos.x - cornerIncrementer);
            bottomLeft.z = (startingPos.z - cornerIncrementer);
            bottomLeft.y = .01f;
 
            listOfVectors.Add(bottomLeft);

            topRight.x = (startingPos.x + cornerIncrementer);
            topRight.z = (startingPos.z + cornerIncrementer);
            topRight.y = .01f;

            listOfVectors.Add(topRight);

            bottomRight.x = (startingPos.x + cornerIncrementer);
            bottomRight.z = (startingPos.z - cornerIncrementer);
            bottomRight.y = .01f;
      
            listOfVectors.Add(bottomRight);

            EdgeMaker(sideIncrementer, bottomRight, -1, 0);
            EdgeMaker(sideIncrementer, bottomRight, 0, 1);

            cornerIncrementer++;
            sideIncrementer += 2;

            //FindClosestAvailableNode(listOfVectors);

            GameObject topLeftTile = Instantiate(tileTestCorner);
            topLeftTile.transform.position = topLeft;
            GameObject bottomLeftTile = Instantiate(tileTestCorner);
            bottomLeftTile.transform.position = bottomLeft;
            GameObject topRightTile = Instantiate(tileTestCorner);
            topRightTile.transform.position = topRight;
            GameObject bottomRightTile = Instantiate(tileTestCorner);
            bottomRightTile.transform.position = bottomRight;

            /*if (foundClosestFreeNode)
            {
                Debug.Log("Found free node"); 
                listOfVectors.Clear();
                foundClosestFreeNode = false;
                return recalculatedTarget; 
            }*/
        }

        unitMovement = UnitMovement.createFormation; 

    }

    private void EdgeMaker(int sideIncrementer, Vector3 corner, int x, int z)
    {
        for (int tl = 0; tl < sideIncrementer; tl++)
        {
            if (tl == 0)
            {
                Vector3 temp = corner;
                temp.x += x;
                temp.z += z;
               GameObject topLeftHorz = Instantiate(tileTestEdges);
               topLeftHorz.transform.position = temp;
                newTemp = temp;
                listOfVectors.Add(newTemp); 
            }
            else
            {
                Vector3 thirdTemp = newTemp;
                thirdTemp.x += x;
                thirdTemp.z += z;
               GameObject topLeftHorz = Instantiate(tileTestEdges);
              topLeftHorz.transform.position = thirdTemp;
                newTemp = thirdTemp;
                listOfVectors.Add(newTemp);
            }
        }
    }

    private Vector3 FindClosestAvailableNode(List<Vector3> listOfVectors)
    {
        for (int i = 0; i < listOfVectors.Count; i++)
        {
            if (Grid.instance.NodeFromWorldPoint(listOfVectors[i]).walkable && !occupiedNodes.Contains(listOfVectors[i]))
            {
               // foundClosestFreeNode = true;
               // recalculatedTarget = listOfVectors[i];
                return listOfVectors[i];
            }
        }
        return listOfVectors[0]; 

            /*if((listOfVectors[i].x < Grid.instance.gridSizeX && listOfVectors[i].z < Grid.instance.gridSizeY) && (listOfVectors[i].x >= 0 && listOfVectors[i].z >= 0))
            {
                if(Grid.instance.NodeFromWorldPoint(listOfVectors[i]).walkable && !targets.Contains(listOfVectors[i]))
                {
                    foundClosestFreeNode = true; 
                    recalculatedTarget = listOfVectors[i];
                    break;
                }
            }*/
    }

    private void RemoveFromTargetsAndMoveingUnits()
    {
        if(SelectionManager.instance.currentlySelectedUnits.Count > 0)
        {
            for(int i = 0; i < SelectionManager.instance.currentlySelectedUnits.Count; i++)
            {
                SelectionManager.instance.currentlySelectedUnits[i].GetComponent<Unit>().RemoveUnitFromUnitManagerMovingUnitsList(); 
            }
        }
    }





}
