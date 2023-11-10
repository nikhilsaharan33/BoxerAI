using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.ObjectModel;
using TMPro;


/*
 Changes Done:
 * Move forward Reward = 0.1f (from 0.45f)
 * Now each punch gets a reward of 0.3f even if its not successful (having a successful punch gets +2f reward on top of it)
 * Added an observation: The opponent's ActionState
 */

public class Boxer : Agent
{
    public enum AgentColor
    {
        RED = 0,
        BLUE
    };

    // Enumeration of actions the boxer can take
    public enum ActionState
    {
        IDLE = 0,
        BLOCK,
        PUNCH_LEFT,
        PUNCH_RIGHT
    };

    public AgentColor m_Color;
    private string m_ColorTag;

    // Maximum amount of damage that can be taken before a knockout
    public const float MAX_LIFE = 20f;
    // Current amount of damage that can be taken before knockout
    public float m_Life = 20f;
    // Damage that can be done to opponents
    public float m_Strength = 0.5f;
    // Damage absorbed when blocking
    public float m_Defense = 0.5f;
    // How fast can the boxer move forward, backwards, and laterally
    public float m_MoveSpeed = 5f;


    // What action is the boxer currently performing
    [HideInInspector]
    public ActionState m_ActionState = ActionState.IDLE;
    // Reference to opponent
    public Boxer m_Opponent = null;
    // Start position of this boxer
    public Transform m_StartPosition;
    // Is this agent controlled by the player?
    public bool m_PlayerControlled = false;

    public SphereCollider m_HeadCollider;
    public SphereCollider m_LeftHandHitbox;
    public SphereCollider m_RightHandHitbox;

    private Animator m_Anim;
    private Rigidbody m_Rbody;
    private float[] m_ActionVector;

    public TrainingMatch m_Match;

    public LayerMask m_LayerMask;

    [SerializeField] HealthBar m_healthBar;

    public TextMeshProUGUI m_Event;
    // Start is called before the first frame update
    public void Start()
    {
        m_ActionState = ActionState.IDLE;
        m_Anim = GetComponent<Animator>();
        m_Rbody = GetComponent<Rigidbody>();
        m_healthBar = GetComponentInChildren<HealthBar>();
        m_healthBar.UpdateHealthBar(m_Life, MAX_LIFE);
        string[] tags = { "Red", "Blue" };

        m_ColorTag = tags[(int)m_Color];
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        MoveAgent(m_ActionVector);
        AttackDefend(m_ActionVector);

        if (m_ActionState == ActionState.BLOCK)
        {
            Block();
            float animationCompletePerc = m_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime /
                    m_Anim.GetCurrentAnimatorStateInfo(0).length;
            if (animationCompletePerc >= 0.7f)
            {
                m_HeadCollider.enabled = false;
            }
        }
        else
        {
            m_HeadCollider.enabled = true;
        }
    }

    public void MoveAgent(float[] vectorAction)
    {
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Vector3 moveDir = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        var map = new Dictionary<int, int>
        {
            {0, -1},
            {1, 0},
            {2, 1}
        };

        if (vectorAction[1] > 0)
        {
            AddReward(m_Match.m_rewards.m_MoveToOppReward);
        }
        else if (vectorAction[1] < 0)
        {
            AddReward(-m_Match.m_rewards.m_MoveToOppReward);
        }
        moveDir += transform.forward * map[(int)vectorAction[1]];
        moveDir += transform.right * map[(int)vectorAction[0]];

        transform.LookAt(m_Opponent.transform);

        transform.position += m_MoveSpeed * Time.deltaTime * moveDir;
    }

    // For actually doing the actions in ActionState {IDLE, BLOCK, PUNCH_LEFT, PUNCH_RIGHT}
    public void AttackDefend(float[] vectorAction)
    {
        int action = (int)vectorAction[2];

        // If the agent is on BLOCK state, only do something when recieved action is PUNCH_(LEFT or RIGHT), else do the received action
        if (m_ActionState == ActionState.BLOCK)
        {
            if (action != (int)ActionState.IDLE && action != (int)ActionState.BLOCK)
            {
                m_ActionState = (ActionState)action;
                m_Anim.SetInteger("ActionState", action);
            }
        }
        else
        {
            m_ActionState = (ActionState)action;
            m_Anim.SetInteger("ActionState", action);
        }


        switch (action)
        {
            case 1:
                m_Anim.SetBool("Blocking", true);
                m_Anim.SetBool("Block", true);
                break;
            case 2:
                PunchOpponent(m_LeftHandHitbox);
                break;
            case 3:
                PunchOpponent(m_RightHandHitbox);
                break;
        }
    }
    private float[] ConvertToFloatArray(ActionSegment<int> actionSegment)
    {
        float[] result = new float[actionSegment.Length];

        for (int i = 0; i < actionSegment.Length; i++)
        {
            result[i] = (float)actionSegment[i];
        }

        return result;
    }

    // actionvector is like {MOVE_RIGHT, MOVE_FORWARD, ACTION}. MOVE_RIGHT would be like -1,0,1 (abstractly) similarly forward, and ACTION will be like 0,1,2,3.
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        m_ActionVector = ConvertToFloatArray(actionBuffers.DiscreteActions);
        AddReward(-0.01f);
    }

    public override void OnEpisodeBegin()
    {
        m_ActionState = ActionState.IDLE;
        m_Anim = GetComponent<Animator>();
        m_Rbody = GetComponent<Rigidbody>();
        

        string[] tags = { "Red", "Blue" };

        m_ColorTag = tags[(int)m_Color];


        m_Life = MAX_LIFE;

        m_ActionVector = new float[3];
        m_Anim.SetInteger("ActionState", (int)ActionState.IDLE);

        ReturnToCorner();
        m_healthBar.UpdateHealthBar(m_Life, MAX_LIFE);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Moving Right or Left
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 0;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else { discreteActionsOut[0] = 1; }

        // Moving Up or Down
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[1] = 0;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[1] = 2;
        }
        else { discreteActionsOut[1] = 1; }

        // Only one attack/defensive action may be passed at a time
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[2] = (int)ActionState.BLOCK;
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[2] = (int)ActionState.PUNCH_LEFT;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[2] = (int)ActionState.PUNCH_RIGHT;
            }
            else
            {
                discreteActionsOut[2] = (int)ActionState.IDLE;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Note: Observation vector should have size (10).

        // Boxer Position
        sensor.AddObservation(transform.position);
        // Boxer Stats
        sensor.AddObservation(m_Life);
        sensor.AddObservation(m_Strength);
        sensor.AddObservation(m_Defense);
        // Opponent Position
        sensor.AddObservation(m_Opponent.transform.position);
        // Opponent Stats (Partially observable)
        sensor.AddObservation(m_Opponent.m_Life);
        // May add new observation such as ActionState of the opponent
        sensor.AddObservation((int)m_Opponent.m_ActionState);
    }

    public bool IsKnockedOut()
    {
        return m_Life <= 0;
    }

    public void ReturnToCorner()
    {
        // Reset Positioning
        transform.position = m_StartPosition.position;
        transform.LookAt(m_Opponent.transform);
    }

    private void Block()
    {
        // Check if the opponent hands are punching this boxer
        Collider[] cols = Physics.OverlapSphere(m_RightHandHitbox.bounds.center, m_RightHandHitbox.radius, m_LayerMask);
        foreach (Collider c in cols)
        {
            if (c.transform.parent == transform)
                continue;
            if (m_Opponent.m_ActionState == ActionState.PUNCH_LEFT ||
                m_Opponent.m_ActionState == ActionState.PUNCH_RIGHT)
            {
                m_Event.text = transform.name + " blocked punch";
                Debug.Log(transform.name + " blocked punch");
                AddReward(m_Match.m_rewards.m_SuccessfulBlock);
            }
        }
    }

    private void PunchOpponent(SphereCollider col)
    {
        Collider[] cols = Physics.OverlapSphere(col.bounds.center, col.radius, m_LayerMask);

        foreach (Collider c in cols)
        {
            if (c.transform.parent == transform)
                continue;

            // Check the name of the bodypart hit and the action state of the opponent
            if (m_Opponent.m_ActionState == ActionState.BLOCK)
            {
                // Get the percantage of block animation that has elapsed
                float animationCompletePerc = m_Anim.GetCurrentAnimatorStateInfo(0).normalizedTime /
                    m_Anim.GetCurrentAnimatorStateInfo(0).length;

                if (c.CompareTag("Hand") && animationCompletePerc >= 0.7f)
                {
                    m_Event.text = transform.name + "'s punch was blocked.";
                    Debug.Log(transform.name + "'s punch was blocked.");
                    AddReward(m_Match.m_rewards.m_PunchBlocked);
                }
                else if (c.CompareTag("Head"))
                {
                    m_Event.text = transform.name + " landed punch";
                    Debug.Log(transform.name + " landed punch");
                    AddReward(m_Match.m_rewards.m_SuccessfulPunch);
                    // Apply damage to opponent
                    m_Opponent.m_Life -= Mathf.Max(0, (m_Strength - m_Opponent.m_Defense));
                    m_Opponent.m_healthBar.UpdateHealthBar(m_Opponent.m_Life, MAX_LIFE);
                    m_Opponent.AddReward(-m_Match.m_rewards.m_SuccessfulPunch);
                }
            }
            else
            {
                if (c.CompareTag("Head"))
                {

                    m_Event.text = transform.name + " landed punch";
                    Debug.Log(transform.name + " landed punch");
                    AddReward(m_Match.m_rewards.m_SuccessfulPunch);
                    // Apply damage to opponent
                    m_Opponent.m_Life -= Mathf.Max(1, (m_Strength - m_Opponent.m_Defense));
                    m_Opponent.m_healthBar.UpdateHealthBar(m_Opponent.m_Life, MAX_LIFE);
                    m_Opponent.AddReward(-m_Match.m_rewards.m_SuccessfulPunch);
                }
            }
        }
    }
}

