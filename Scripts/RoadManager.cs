using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoadManager : Singleton<RoadManager>
{

    GameObject[] loadedPieces;
    List<GameObject> roadPieces;
    [SerializeField]
    private int numberOfPieces = 10;
    //hardcode the first two raods  pieces to help with math calculations and making rode track
    [SerializeField]
    private string hardCodedPiece = "Straight60m";
    [SerializeField]
    private float roadSpeed = 20f;
    private Vector3 rotationPoint = Vector3.zero ; //the point in which we rotate our piece on 

    //Events and delegates
    public delegate void AddPieceHandler(GameObject roadpiece);

    //accesibility event delegateSignature eventname
    public event AddPieceHandler OnAddPieceEvent;

    private Transform beginLeft, beginRight, endLeft, endRight;
    void Start()
    {
        OnAddPieceEvent += nothing => { };

        loadedPieces = Resources.LoadAll<GameObject>("RoadPieces");
        //reference to a roadpiece in our file
        GameObject roadPiece = Resources.Load("RoadPieces/Straight50m") as GameObject;

        roadPieces = new List<GameObject>();

        //instantiate the first two pieces
        roadPieces.Add( Instantiate(Resources.Load("RoadPieces/" + hardCodedPiece) as GameObject));
        roadPieces.Add(Instantiate(Resources.Load("RoadPieces/" + hardCodedPiece) as GameObject));

        //to make the object show up in the scene. instantiate
        for (int i = 2; i < numberOfPieces; i++)
        {
            AddPiece();
        }
        //first road piece is parented to the second
        roadPieces[0].transform.parent = roadPieces[1].transform;
        //move the track of road past the first piece 
        float roadLength = (roadPieces[0].transform.Find("BeginLeft").position - roadPieces[0].transform.Find("EndLeft").position).magnitude;
        roadPieces[0].transform.Translate(0f, 0f, -roadLength, Space.World);

        //getting the four corners of the roadpieces taht are moving
        SetCurrentRoadPiece();
    }

    //new method called addpiece
    public void AddPiece()
    {
        //decides which piece to get from our loadedpieces array
        int randomIndex = Random.Range(0, loadedPieces.Length);
        roadPieces.Add(Instantiate(loadedPieces[randomIndex],
            roadPieces[roadPieces.Count - 1].transform.position,
            roadPieces[roadPieces.Count - 1].transform.rotation));

        //get a reference to the newest piece to the road track and the previous piece
        Transform newPiece = roadPieces[roadPieces.Count - 1].transform;
        Transform prePiece = roadPieces[roadPieces.Count - 2].transform;

        //Move the pieces into position using Displacement
         beginLeft = newPiece.Find("BeginLeft");
         beginRight = newPiece.Find("BeginRight");
         endLeft = prePiece.Find("EndLeft");
         endRight = prePiece.Find("EndRight");

        Vector3 beginEdge = beginRight.position - beginLeft.position;
        Vector3 endEdge = endRight.position - endLeft.position;

        //angle between the edges - so we can rotate by this angle to allign them to eachother
        float angle = Vector3.Angle(beginEdge, endEdge) * Mathf.Sign(Vector3.Cross(beginEdge, endEdge).y);

        //rotate piece using the angle and around the world/scene space
        newPiece.Rotate(0f, angle, 0f, Space.World);

        Vector3 discplacement = endLeft.position - beginLeft.position;
        //translate - move our new piece into allignment with the previous 
        newPiece.Translate(discplacement, Space.World);

        newPiece.parent = roadPieces[1].transform;


        //call upon the adaptive event to notify subscribers to perform action on the roadpieces
        OnAddPieceEvent(newPiece.gameObject);
    }

    void Update()
    {
        MoveRoadPieces(roadSpeed * Time.deltaTime);

        //step one, determine the parent road piece passes the origin on the x axis 
        if (endLeft.position.z < 0f || endRight.position.z < 0f)
        {
            //snap the current road pieces to the x axis 
            float resetDsistance = GetResetDistance();
            MoveRoadPieces(-resetDsistance);

            CycleRoadPieces();

            //realign roads after pieces get added and deleted
            MoveRoadPieces(resetDsistance);

            //determine if we have a straight piece
            if (roadPieces[1].CompareTag(Tags.straightPiece))
            {
                //re-align straigh pieces to the x axis with quaternion rotation 
                roadPieces[1].transform.rotation = new Quaternion(roadPieces[1].transform.rotation.x, 0f, 0f, roadPieces[1].transform.rotation.w);
                roadPieces[1].transform.position = new Vector3(0f, 0f, roadPieces[1].transform.position.z);
            }
        }

    }

    public void CycleRoadPieces()
    {
        //step 2, delete the first road piece in our list and within the scene
        Destroy(roadPieces[0]);
        roadPieces.RemoveAt(0);
        //step 3, add a new road piece to the end of our track
        AddPiece();
        //step 4, re-parent all the road piece to the newest second road piece in our list
        for (int i = roadPieces.Count - 1; i >= 0; i--)
        {
            //step 5, we have to unparent and reparent the road pieces backwards through the list
            roadPieces[i].transform.parent = null;
            roadPieces[i].transform.parent = roadPieces[1].transform;
        }
        //step 6, know which piece we are moving (begin left/right, end left/right)
        SetCurrentRoadPiece();
    }

    public void MoveRoadPieces(float distance)
    {
        if (roadPieces[1].CompareTag(Tags.straightPiece))
        {
            //moving the first index road piece backward
            roadPieces[1].transform.Translate(0f, 0f, -distance, Space.World);
        }

        else
        {
            //We have to rotate around the rotationpoint, what axis we rotate on and how fast
            //Calculate the angular velocity of the curve road piece
            float radius = Mathf.Abs(rotationPoint.x);
            //angular velocity = distance (radius) / angle 
            float angle = (((distance) / radius) * Mathf.Sign(roadPieces[1].transform.localScale.x)) * Mathf.Rad2Deg;

            roadPieces[1].transform.RotateAround(rotationPoint, Vector3.up, angle);
        }
    }

    //SetCurrentRoadPiece
    public void SetCurrentRoadPiece() 
    {
        beginLeft = roadPieces[1].transform.Find("BeginLeft");
        beginRight = roadPieces[1].transform.Find("BeginRight");
        endLeft = roadPieces[1].transform.Find("EndLeft");
        endRight = roadPieces[1].transform.Find("EndRight");
        //Find the rotationpoint of the road piece that we want to move
        rotationPoint = GetRotationPoint(beginLeft, beginRight, endLeft, endRight);
    }


    //create a new function that returns the rotationpoint of the road piece we are moving
    public Vector3 GetRotationPoint(Transform beginLeft, Transform beginRight, Transform endLeft, Transform endRight)
    {
        //Compute the edges of the road pieces
        Vector3 endEdge = endLeft.position - endRight.position;
        Vector3 beginEdge = beginLeft.position - beginRight.position;

        //calculate the square magnitude of the edges using the dot product
        float a = Vector3.Dot(beginEdge, beginEdge);
        float e = Vector3.Dot(endEdge, endEdge);
        //project beginedge onto endedge
        float b = Vector3.Dot(beginEdge, endEdge);

        float difference = a * e - b * b;

        //calculate a 3D vector between the beginning and end of the road piece
        Vector3 r = beginLeft.position - endLeft.position;

        //Calaculate the dot product in relation to the radius
        float c = Vector3.Dot(beginEdge, r);
        float f = Vector3.Dot(endEdge, r);

        //Calculate how much to extend the vector towards the rotationpoint
        float s = (b * f - c * e) / difference; //beginLeft
        float t = (a * f - c * b) / difference; //endEdge

        //Extand our vectors to go to the rotationpoint
        Vector3 rotationPointBegin = beginLeft.position + beginEdge * s;
        Vector3 rotationPointEnd = endLeft.position + endEdge * t;

        return (rotationPointBegin + rotationPointEnd) / 2f; 
    }

    private float GetResetDistance()
    {
        //check for tag either straight or curved
        if (roadPieces[1].CompareTag(Tags.straightPiece))
        {
            //return back the position of the straight piece on the z axis 
            return -endLeft.position.z; 
        }
        else
        {
            //calcute the end edge 
            Vector3 endEdge = endRight.position - endLeft.position;
            //calculate the endedge angle in relation to the global x axis 
            float angle = Vector3.Angle(Vector3.right, endEdge);
            //get the radius of our curved piece to form a partial circle
            float radius = Mathf.Abs(rotationPoint.x);

            return angle * Mathf.Deg2Rad * radius;  
        }
    }
}
