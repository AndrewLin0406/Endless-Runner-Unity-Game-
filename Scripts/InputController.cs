using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : Singleton<InputController>
{
    //Our inputs controls we created inside unity - reference to inputactions
    PlayerInputActions inputActions;
    //variables that store the inpurts read from the input actions 
    float horizontal = 0f; //+ve right, -ve left, 0 nore movement
    bool jump = false;
    bool slide = false;

    float hNew = 0f, hPrev = 0f;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float initialVelocity = 5f;
    [SerializeField] float gravity = -9.81f;
    //gravity displacement : y = Gt2/2 + Vi t
    //lane variable 
    public int numberOfLanes { get; private set; } = 3;
    int CurrentLane = 0, prevLane = 0;

    public float laneWidth { get; private set; } = 7.5f;
    Coroutine currentLaneChange;

    //we have input to do actions with but a coroutine is busy, store into a buffer to do when coroutine is finished 
    int directionBuffer = 0;
    //keep track of how many currentlanechange coroutines are being called in the stack
    int laneChangeStackCalls = 0;

    //get reference to the animator on the gameobject robot
    Animator anim;
    //reference to the animator on the gameobject robot
    int jumpParameter, slideParameter;

    State currentState = State.Run;

    void Awake()
    {
        anim = GetComponent<Animator>();
        jumpParameter = Animator.StringToHash("Jump");
        slideParameter = Animator.StringToHash("Slide");

        //call coroutine
        //StartCoroutine(TestCorqutine());
        laneWidth /= numberOfLanes;

        //Set the player to be at the origin of the world scene 
        transform.position = Vector3.zero;


        inputActions = new PlayerInputActions();

        //key/controller events - KeyUp and KeyDown - Action Perofrmed and Canceled 

        //lambda expressions let us assign annonymous methods/functions without having to formally declare 
        inputActions.Player.Movement.performed += ReadHorizontal;

        inputActions.Player.Jump.performed += context => jump = context.ReadValue<float>() > 0f;
        inputActions.Player.Jump.canceled += context => jump = context.ReadValue<float>() > 0f;

        inputActions.Player.Slide.performed += context => slide = context.ReadValue<float>() > 0f;
        inputActions.Player.Slide.canceled += context => slide = context.ReadValue<float>() > 0f;
    }

    public void ReadHorizontal(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<float>();
    }

    // Update is called once per frame
    void Update()
    {
        hNew = horizontal;
        float hDelta = hNew - hPrev;

        if(Mathf.Abs(hDelta) > 0f && Mathf.Abs(hNew) > 0f && currentState != State.Slide)
        {
            MovePlayer((int)hNew);
        }
        if (jump && currentState != State.Jump)
        {
            StartCoroutine(Jump());
        }
        if (slide && currentState == State.Run)
        {
            anim.SetTrigger(slideParameter);
            currentState = State.Slide;
        }
        //reset the input variables 
        jump = false;
        slide = false;
        horizontal = 0f;
    }

    public void MovePlayer(int Direction)
    {
        
        //if theres a couroutine running
        if (currentLaneChange != null)
        {
            //if the current lane and the direction the player is going is not the preveious lane the player was at
            if (CurrentLane + Direction != prevLane)
            {
                //store the input into the buffer
                directionBuffer = Direction; 
                return;
            }
            StopCoroutine(currentLaneChange);
        }
        prevLane = CurrentLane;

        CurrentLane = Mathf.Clamp(CurrentLane + Direction, numberOfLanes / -2, numberOfLanes / 2);
        //MOve the player into the new lane by changing the transform.position
        currentLaneChange = StartCoroutine(LaneChange());

        //all inputs are done so buffer empties to 0
        directionBuffer = 0;
    }

    public void FinishSlide()
    {
        currentState = State.Run;
    }
    public void PauseEditor()
    {
        UnityEditor.EditorApplication.isPaused = true;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    IEnumerator TestCorqutine()
    {
        Debug.Log("Please Wait 4 Seconds");
        //yield return halts progess of this routine for a time n seconds 
        yield return new WaitForSeconds(4);

        Debug.Log("Thanks For Waiting, Wait 3 seconds");
        yield return new WaitForSeconds(3);

        Debug.Log("Have some factorials");
        for (int i = 0; i < 10; i++)
        {
            int factorial = i;
            for (int l = i - 1; l > 1; l--)
            {
                factorial *= l;
            }
            Debug.Log(factorial);
            //resume execution of code on next frame 
            yield return null;
        }

        Debug.Log("Complete");
        
    }

    IEnumerator Jump()  
    {
        currentState = State.Jump;

        //set the animator controller parameter jump to true
        anim.SetBool(jumpParameter, true);

        //calculate the total time of the jump
        float tFinal = (2 * initialVelocity) / -gravity;
        //calculate a transition time - as we're jumping in the air, 
        float tLand = tFinal - 0.125f;

        float t = Time.deltaTime;   

        for (; t < tLand; t += Time.deltaTime)
        {
            float yPos = gravity * (t * t) / 2f + initialVelocity * t;
            Helpers.SetY(transform, yPos);
            yield return null;
        }

        anim.SetBool(jumpParameter, false);

        for (; t < tFinal; t += Time.deltaTime)
        {
            float yPos = gravity * (t * t) / 2f + initialVelocity * t;
            Helpers.SetY(transform, yPos);
            yield return null;
        }
        //ensure the player's robot position is back at 0
        Helpers.SetY(transform, 0f);
        currentState = State.Run;

        
    }

    IEnumerator LaneChange()
    {
        //where we're coming from
        Vector3 fromPosition = Vector3.right * prevLane * laneWidth;
        //where we are going
        Vector3 toPosition = Vector3.right * CurrentLane * laneWidth; 

        float t = (laneWidth - Vector3.Distance(transform.position.x * Vector3.right, toPosition)) / laneWidth;

        //gradually move the player into the new lane using linear interpolation
        for (; t < 1; t += moveSpeed * Time.deltaTime / laneWidth)
        {
            //move between lanes and preserve our y posistion value
            transform.position = Vector3.Lerp(fromPosition + transform.position.y * Vector3.up, 
                toPosition + transform.position.y * Vector3.up, t);
            yield return null; 
        }
         
        //ensure that the player is in the middle of the lane
        transform.position = toPosition + transform.position.y * Vector3.up;

        //when the coroutine is complete, the variable that contains it (currentlanechange) reset
        currentLaneChange = null;

        //check to see if we have any input in the buffer
        //prevent more than 2 calls on the stack of this coroutine 
        if (directionBuffer != 0 && ++laneChangeStackCalls < 2)
        {
            //call move player based on the input in the buffer
            MovePlayer(directionBuffer);
            directionBuffer = 0;
            
        }
        //all coroutines are done and off the callstack, the variable here reflects this 
        laneChangeStackCalls = 0;
    }
}
