using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : Singleton<ObstacleManager>
{

    GameObject[] loadedObstacles;

    //unused or leftover distance from a previous road
    float leftoverDistance = 0;

    [SerializeField]
    private float distanceInterval = 30f; //the psace between obstacles


    // Start is called before the first frame update
    void Awake()
    {
        //load all obstacles gameobject prefabs from the resourses folder
        loadedObstacles = Resources.LoadAll<GameObject>("Obstacles");

        //obstacle manager is listening/subscribe to the roadmanager event called onaddpieceevent
        RoadManager.Instance.OnAddPieceEvent += PlaceObstacles;
    }

    public void PlaceObstacles(GameObject piece)
    {
        Debug.Log("New Road Piece Had Beed Added .... Placing Obstacles");

        //get the child gameobject of the road piece
        Transform beginLeft = piece.transform.Find("BeginLeft");
        Transform beginRight = piece.transform.Find("BeginRight");
        Transform EndLeft = piece.transform.Find("EndLeft");
        Transform EndRight = piece.transform.Find("EndRight");

        //calculate road length
        float roadLength;
        Vector3 rotationPoint = Vector3.zero;
        float radius = 0f;

        if (piece.CompareTag(Tags.straightPiece))
        {
            roadLength = Vector3.Distance(beginLeft.position, EndLeft.position);
        }
        else
        {
            //get radius from our rotationo point
            rotationPoint = RoadManager.Instance.GetRotationPoint(beginLeft, beginRight, EndLeft, EndRight);

            radius = Vector3.Distance(piece.transform.position, rotationPoint);

            //calculate angle between edges of road
            float angle = Vector3.Angle(beginLeft.position - beginRight.position, EndLeft.position - EndRight.position);

            roadLength = radius * angle * Mathf.Deg2Rad; 

        }

        float halfRoadLength = roadLength / 2f;

        float currentDistance = distanceInterval - halfRoadLength - leftoverDistance;

        //if the road piece we're trying to add obstacle onto is to small 
        if (currentDistance >= halfRoadLength)
        {
            leftoverDistance += roadLength; 
        }

        //start placing obstacles every distanceInterval 
        for (; currentDistance < halfRoadLength; currentDistance += distanceInterval) 
        {
            //Create a container for the rows every distanceInterval
            GameObject obstacleRow = new GameObject("ObstacleRow");
            //set position and rotation of this row in relation to the piece we want it on
            obstacleRow.transform.position = piece.transform.position;
            obstacleRow.transform.rotation = piece.transform.rotation;
            //compensate for the roadpiece modle's rotation
            obstacleRow.transform.Rotate(90f, 0f, 0f);
            //parent the row onto the road piece so it follows itt in the scene
            obstacleRow.transform.parent = piece.transform;
            Debug.Log("check");

            //check how many of the same type of the obstacle we have, don't want walls to block in every lane
            int sameObstacleCount = 0;
            int numberOfLanes = InputController.Instance.numberOfLanes;


            for (int i = numberOfLanes/-2; i <= numberOfLanes/2; i++)
            {
                //choose a random obstacle based on the prefabs
                int randomObstacle = Random.Range(0, loadedObstacles.Length);

                if (loadedObstacles[randomObstacle].CompareTag(Tags.wall)) { 
                    //To check if the number of wall is the same or equal to the number of lanes
                    //so that there cant be all 3 kanes being walls
                    if(++sameObstacleCount >= numberOfLanes)
                    {
                        //skip over a few obstacles in the loadedobstacle array\
                        randomObstacle += 3;
                        //to avoid a indexarnayoutofbounds exception, was remainder to keep index between loadedobstacle array
                        randomObstacle %= loadedObstacles.Length;
                    }
                }

                //instantiate the obstacle prefab onto the row per lane
                GameObject obstacle = Instantiate(loadedObstacles[randomObstacle], obstacleRow.transform.position, obstacleRow.transform.rotation, obstacleRow.transform);

                //Move obstacle to the correct lane on the x axis
                obstacle.transform.transform.Translate(Vector3.right * i * InputController.Instance.laneWidth, Space.Self);

            }
            
            if (piece.CompareTag(Tags.straightPiece))
            {
                //translate or slide obstacle to their correct location
                obstacleRow.transform.Translate(0f, 0f, currentDistance);
            }
            else
            {
                float angle = (currentDistance / radius) * Mathf.Rad2Deg;
                //we need to rotate the obstacle row to align with the curvature of the roads
                obstacleRow.transform.RotateAround(rotationPoint, Vector3.up, angle * Mathf.Sign(piece.transform.localScale.x));

            }

            leftoverDistance = halfRoadLength - currentDistance;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
