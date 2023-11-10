using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf;
using Unity.MLAgents;

/// <summary>
/// Manages a match between two Boxer agents during training
/// </summary>

/*
 Changes Done:
 * Added call of OnEpisodeBegin in MatchReset()
 * Removed the part for avoiding ring boundary
 */

public class TrainingMatch : MonoBehaviour
{

    // References to the two boxers
    public Boxer m_BoxerA; //Red
    public Boxer m_BoxerB; //Blue
    public GameObject winPrefab;
    public Rewards m_rewards;
    

    // Use this for initialization
    public void Start()
    {
        m_BoxerA.OnEpisodeBegin();
        m_BoxerB.OnEpisodeBegin();
    }

    // Update is called once per frame
    public void Update()
    {
        // Check for knock out
        if (m_BoxerA.IsKnockedOut() && m_BoxerB.IsKnockedOut())
        {
            // Tie
            // Punish both agents and reset the match
            Debug.Log("Its a Tie.");
            GameObject canvas = Instantiate(winPrefab, Vector3.zero, Quaternion.identity);

            TextMeshProUGUI textMesh = canvas.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = "It's a tie";
            Destroy(canvas, 2.0f);
            m_BoxerA.AddReward(-m_rewards.m_KnockoutReward);
            m_BoxerB.AddReward(-m_rewards.m_KnockoutReward);

            m_BoxerA.EndEpisode();
            m_BoxerB.EndEpisode();

            MatchReset();
        }
        else if (!m_BoxerA.IsKnockedOut() && m_BoxerB.IsKnockedOut())
        {
            // Boxer A Win
            // Reward red, punish blue, reset
            Debug.Log("Red wins.");
            GameObject canvas = Instantiate(winPrefab, Vector3.zero, Quaternion.identity);

            TextMeshProUGUI textMesh = canvas.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = "<color=red>Red wins</color>";
            Destroy(canvas, 2.0f);

            m_BoxerA.AddReward(m_rewards.m_KnockoutReward);
            m_BoxerB.AddReward(-m_rewards.m_KnockoutReward);

            m_BoxerA.EndEpisode();
            m_BoxerB.EndEpisode();

            MatchReset();
        }
        else if (m_BoxerA.IsKnockedOut() && !m_BoxerB.IsKnockedOut())
        {
            // Boxer B Win
            // Reward blue, punish red, reset
            Debug.Log("Blue wins.");
            GameObject canvas = Instantiate(winPrefab, Vector3.zero, Quaternion.identity);

            TextMeshProUGUI textMesh = canvas.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = "<color=blue>Blue wins</color>";

            Destroy(canvas, 2.0f);

            m_BoxerA.AddReward(-m_rewards.m_KnockoutReward);
            m_BoxerB.AddReward(m_rewards.m_KnockoutReward);

            m_BoxerA.EndEpisode();
            m_BoxerB.EndEpisode();

            MatchReset();
        }

        else if (m_BoxerA.transform.position.y > 2f || m_BoxerB.transform.position.y > 2f)
        {

            m_BoxerA.AddReward(-m_rewards.m_KnockoutReward);
            m_BoxerB.AddReward(-m_rewards.m_KnockoutReward);

            m_BoxerA.EndEpisode();
            m_BoxerB.EndEpisode();

            MatchReset();
        }

        else if (m_BoxerA.transform.position.y < -2f || m_BoxerB.transform.position.y < -2f)
        {
            m_BoxerA.AddReward(-m_rewards.m_KnockoutReward);
            m_BoxerB.AddReward(-m_rewards.m_KnockoutReward);

            m_BoxerA.EndEpisode();
            m_BoxerB.EndEpisode();

            MatchReset();
        }

        //else if (Mathf.Abs(m_BoxerA.transform.position.x) >= 4f || Mathf.Abs(m_BoxerA.transform.position.z) >= 4f ||
        //    Mathf.Abs(m_BoxerB.transform.position.x) >= 4f || Mathf.Abs(m_BoxerB.transform.position.z) >= 4f)
        //{
        //    m_BoxerA.AddReward(-m_Academy.m_KnockoutReward);
        //    m_BoxerB.AddReward(-m_Academy.m_KnockoutReward);

        //    m_BoxerA.EndEpisode();
        //    m_BoxerB.EndEpisode();

        //    MatchReset();
        //}
    }

    public virtual void MatchReset()
    {
        // Place agents back to their starting spots

        m_BoxerA.OnEpisodeBegin();
        m_BoxerB.OnEpisodeBegin();
    }
}