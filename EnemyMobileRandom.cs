﻿using Unity.FPS.Game;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyControllerRandom))]
    public class EnemyMobileRandom : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Follow,
            Attack,
        }

        public Animator Animator;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        public AudioClip OnDetectSfx;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        public AIState AiState { get; private set; }
        EnemyControllerRandom m_EnemyController;
        AudioSource m_AudioSource;

        MapManager m_MapManager;

        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamaged";

        List<GameObject> m_ConnectedFriends = new List<GameObject>();

        public float AngularSpeedOfRaycast = 1f;
        public float RangeOfRaycast = 10f;
        float raycastAngleForSearchFriend;

        Actor m_actor;
        Collider[] m_SelfColliders;

        GameObject RayViewer;

        GameObject Body;

        void Start()
        {
            m_EnemyController = GetComponent<EnemyControllerRandom>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyControllerRandom, EnemyMobile>(m_EnemyController, this,
                gameObject);

            m_EnemyController.onAttack += OnAttack;
            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;
            // m_EnemyController.SetPathDestinationToClosestNode();
            m_EnemyController.onDamaged += OnDamaged;

            // Start patrolling
            AiState = AIState.Patrol;

            // adding a audio source to play the movement sound on it
            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);
            m_AudioSource.clip = MovementSound;
            m_AudioSource.Play();
            m_MapManager = GetComponent<MapManager>();
            // DebugUtility.HandleErrorIfNullGetComponent<MapManager, EnemyMobile>(m_MapManager, this, gameObject);
            raycastAngleForSearchFriend = 0f;
            m_actor = GetComponent<Actor>();
            m_SelfColliders = GetComponentsInChildren<Collider>();
            RayViewer = gameObject.transform.Find("RayViewer").gameObject;
            Body = gameObject.transform.Find("HitBox").gameObject;
        }

        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            float moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / m_EnemyController.NavMeshAgent.speed);

            SearchFriendsByRaycast();
        }

        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (AiState)
            {
                case AIState.Follow:
                    // Transition to attack when there is a line of sight to the target
                    if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        // m_EnemyController.SetNavDestination(m_MapManager.randomWP);
                        Debug.Log("to Attack");
                    }

                    break;
                case AIState.Attack:
                    // Transition to follow when no longer a target in attack range
                    if (!m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Follow;
                        Debug.Log("to Follow");
                        m_MapManager.ClearCentor();
                    }

                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (AiState)
            {
                case AIState.Patrol:
                    // m_EnemyController.UpdatePathDestination();
                    m_EnemyController.SetNavDestination(m_MapManager.randomWP);
                    break;
                case AIState.Follow:
                    m_MapManager.SetCentor(m_EnemyController.KnownDetectedTarget.transform.position, 3);
                    m_EnemyController.SetNavDestination(m_MapManager.randomWP);
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientWeaponsTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    m_MapManager.SetCentor(transform.position, 3);
                    m_EnemyController.SetNavDestination(m_MapManager.randomWP);
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.TryAtack(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        void SearchFriendsByRaycast()
        {
            raycastAngleForSearchFriend += AngularSpeedOfRaycast * Time.deltaTime;
            if (raycastAngleForSearchFriend > Mathf.PI){
                raycastAngleForSearchFriend -= 2 * Mathf.PI;
            }
            if (raycastAngleForSearchFriend < -Mathf.PI){
                raycastAngleForSearchFriend += 2 * Mathf.PI;
            }
            Vector3 ray = Quaternion.AngleAxis(raycastAngleForSearchFriend * 180 / Mathf.PI, Vector3.up) * Vector3.forward * RangeOfRaycast;
            RayViewer.transform.position = Body.transform.position + ray;
            RaycastHit[] hits = Physics.RaycastAll(Body.transform.position, 
                                        ray,
                                        RangeOfRaycast,  
                                        -1, 
                                        QueryTriggerInteraction.Ignore);
            foreach (var hit in hits){
                Actor hitActor = hit.collider.GetComponentInParent<Actor>();
                if (hitActor != null && hitActor.Affiliation == m_actor.Affiliation){
                    Debug.Log(string.Format("Find Friends {0}", hitActor.transform.root.gameObject.name));
                }
            }
        }

        void OnAttack()
        {
            Animator.SetTrigger(k_AnimAttackParameter);
        }

        void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Play();
            }

            if (OnDetectSfx)
            {
                AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
            }

            Animator.SetBool(k_AnimAlertedParameter, true);
        }

        void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            for (int i = 0; i < OnDetectVfx.Length; i++)
            {
                OnDetectVfx[i].Stop();
            }

            Animator.SetBool(k_AnimAlertedParameter, false);
        }

        void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }

            Animator.SetTrigger(k_AnimOnDamagedParameter);
        }
    }
}