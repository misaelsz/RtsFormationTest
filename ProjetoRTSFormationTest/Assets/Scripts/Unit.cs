using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour
{
    public Vector2 screenPos;
    public bool onScreen = false;
    public bool selected = false;

	public Vector3 target;
	float speed = 5f;
	public Vector3[] path;
	int targetIndex;

    private Animator animator;
    private int walkSpeedId;

    public bool stopMoving;
    private Grid grid;

    public Renderer renderer; 

    public Vector3 currentWaypoint; 

    public enum MoveFSM
    {
        empty,
        findPosition,
        recalculatePath,
        move,
        turnToFace,
        interact,
        moveToTarget
    }

    public MoveFSM moveFSM; 

	void Start()
    {
        animator = this.GetComponent<Animator>();
        walkSpeedId = Animator.StringToHash("WalkSpeed");
        grid = GameObject.FindGameObjectWithTag("A*").GetComponent<Grid>();
        renderer = transform.Find("Human").GetComponent<Renderer>(); 
    }

    void Update()
    {         

        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject() && 
            UnitManager.instance.buildMode == false)
        {
            GetInteraction();
        }
        MoveStates();
    }

    public void MoveStates()
    {
        switch (moveFSM)
        {
            case MoveFSM.empty:

                break; 
            case MoveFSM.findPosition:
                {
                    RemoveUnitFromUnitManagerMovingUnitsList();
                    PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                    moveFSM = MoveFSM.move; 
                }
                break;
            case MoveFSM.recalculatePath:
                {
                    Node targetNode = grid.NodeFromWorldPoint(target);
                    if (targetNode.walkable == false)
                    {
                        stopMoving = false;
                        FindClosestWalkableNode(targetNode);
                        moveFSM = MoveFSM.move;
                    }
                    else if (targetNode.walkable == true)
                    {
                        stopMoving = false;
                        PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                        moveFSM = MoveFSM.move;
                    }
                }
                break;
            case MoveFSM.move:
                Move();
                break;
            case MoveFSM.turnToFace:
                //TurnToFace();
                break;
            case MoveFSM.interact:
                // if(currentInteractable != null)
                //currentInteractable.GetComponent<Interactable>().Interact(this.gameObject);
                break;
            case MoveFSM.moveToTarget:
                MoveToTarget(); 
                break;
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
		if (pathSuccessful)
        {
			path = newPath;
			targetIndex = 0;
            //RemoveUnitFromUnitManagerMovingUnitsList();
            UnitManager.instance.movingUnits.Add(this.gameObject);
            StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
            moveFSM = MoveFSM.move; 
		}
	}

    private void FindClosestWalkableNode(Node originalNode)
    {
        Node comparisonNode = grid.grid[0, 0];
        Node incrementedNode = originalNode;
        for (int x = 0; x < incrementedNode.gridX; x++)
        {
            // Debug.Log("x: " + incrementedNode.gridX + " incremented node - 1: " + (incrementedNode.gridX - 1));
            incrementedNode = grid.grid[incrementedNode.gridX - 1, incrementedNode.gridY];

            if (incrementedNode.walkable == true)
            {
                comparisonNode = incrementedNode;
                target = comparisonNode.nodeWorldPosition;
                PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                moveFSM = MoveFSM.move;
                break;
            }
        }

    }

    public void MoveToTarget()
    {
        if(transform.position != target)
        {
            transform.rotation = Quaternion.LookRotation(target - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
        else if(transform.position == target)
        {
            animator.SetFloat(walkSpeedId, 0f);
            moveFSM = MoveFSM.move;
        }
    }

    public void Move()
    {

    }


	IEnumerator FollowPath()
    {
        currentWaypoint = path[0];
		while (true)
        {
			if (transform.position == currentWaypoint)
            {
				targetIndex ++;
				if (targetIndex >= path.Length)
                {
                    // stopMoving = true;
                    // currentWaypoint = new Vector3(999,999,999);
                    moveFSM = MoveFSM.moveToTarget; 
                    yield break;
				} 
                currentWaypoint = path[targetIndex];           
            }
            stopMoving = false;
            animator.SetFloat(walkSpeedId, 3f);
            transform.rotation = Quaternion.LookRotation(currentWaypoint - transform.position); 
            transform.position = Vector3.MoveTowards(transform.position,currentWaypoint,speed * Time.deltaTime);
			yield return null;

		}
	}

    private void GetInteraction()
    {
       /* Ray interactionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit interactionInfo;
        if (Physics.Raycast(interactionRay, out interactionInfo, Mathf.Infinity))
        {
            if(interactionInfo.collider.gameObject.name == "Ground")
            {
                if (grid.NodeFromWorldPoint(interactionInfo.point).nodeWorldPosition != currentWaypoint)
                {
                    target = interactionInfo.point;
                    RemoveUnitFromUnitManagerMovingUnitsList();
                    PathRequestManager.RequestPath(transform.position, target, OnPathFound);
                }
            }
        }*/
    }



    public void OnDrawGizmos()
    {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i ++) {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				}
				else {
					Gizmos.DrawLine(path[i-1],path[i]);
				}
			}
		}
	}

    public void RemoveUnitFromUnitManagerMovingUnitsList()
    {
        if (UnitManager.instance.movingUnits.Count > 0)
        {
            for (int i = 0; i < UnitManager.instance.movingUnits.Count; i++)
            {
                if (this.gameObject == UnitManager.instance.movingUnits[i])
                {
                    UnitManager.instance.movingUnits.Remove(UnitManager.instance.movingUnits[i]);
                }
            }
        }
      /*  if(UnitManager.instance.targets.Count > 0)
        {
            for (int i = 0; i < UnitManager.instance.targets.Count; i++)
            {
                if (target == UnitManager.instance.targets[i])
                {
                    Debug.Log("removed target");
                    UnitManager.instance.targets.Remove(target);
                }
            }
        }*/
    }
}
