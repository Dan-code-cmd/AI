 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class StateMachine : MonoBehaviour
{
    public AnimationCurve distanceToPlayerF;
    public AnimationCurve enemyHealthF;
    public AnimationCurve playerHealthF;
    public AnimationCurve enemyStaminaF;
    public AnimationCurve playerStaminaF;
    public AnimationCurve fustrationF;
    public AnimationCurve smellTimerF;
    public state currentState;
    public List<Transform> patrolPoints;
    private int currentPatrolIndex;
    private NavMeshAgent enemy;
    public Enemy GetEnemy;
    public Player GetPlayer;
    Vector3 furthestPoint;
    float retreatTimer = 10;
    float healTimer = 0;
    Vector3 lastKnownLocation;
    bool reachedPosition;
    Vector3 difference;
    float  investigateTimer = 10;
    float radiusTimer = 1;
    float attackTimer = 1;
    public float sampledDistance;
    public float SampledEnemyHealth;
    public float sampledEnemyStamina;
    public float sampledPlayerHealth;
    public float sampledPlayerStamina;
    public float howLikelyToRetreat;
    public float howLikelyToSmell;
    public float sampledSmellTimer;
    public float howLikelyToSprint;
    public float sampledFustration;
    [SerializeField] private Material myMat;
    public enum state 
    {
        patrol,
        hunt,
        smell,
        attack,
        retreat,
        recover,
        birth,
        protect,
        click,
        investigate
    }
    private void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
        currentPatrolIndex = 0;
        GoToNextPatrolPoint();
        furthestPoint = new Vector3();
        reachedPosition = true;
        difference = new Vector3(0.1f, 0.1f, 0.1f);
    }
    float enemyHealthMap, playerHealthMap, enemyStaminaMap, playerStaminaMap, distanceBetweenMap ,fustrationMap, smellTimerMap;
    void Update()
    {
        // calculating distance between player and monster
        GetEnemy.distanceFromPlayer = Vector3.Distance(GetEnemy.transform.position,GetPlayer.transform.position);
        distanceBetweenMap = Remap(GetEnemy.distanceFromPlayer, 0f, 65f, 0f,1f);
        smellTimerMap = Remap(GetEnemy.smellTimer, 0, 15f, 0f, 1f);
        enemyHealthMap = Remap(GetEnemy.mHealth, 0f, 100f, 0f, 1f);
        playerHealthMap = Remap(GetPlayer.pHealth, 0f, 50f, 0f, 1f);
        enemyStaminaMap = Remap(GetEnemy.eStamina, 0f, 100f, 0f, 1f);
        playerStaminaMap = Remap(GetPlayer.pStamina, 0f, 100f, 0f, 1f);
        fustrationMap = Remap(GetEnemy.fustration, 0f, 100f, 0f, 1f);

        //curves
        sampledDistance = distanceToPlayerF.Evaluate(distanceBetweenMap);
        SampledEnemyHealth = enemyHealthF.Evaluate(enemyHealthMap);
        sampledPlayerHealth = playerHealthF.Evaluate(playerHealthMap);
        sampledEnemyStamina = enemyStaminaF.Evaluate(enemyStaminaMap);
        sampledPlayerStamina = playerStaminaF.Evaluate(playerStaminaMap);
        sampledFustration = fustrationF.Evaluate(fustrationMap);
        sampledSmellTimer = smellTimerF.Evaluate(smellTimerMap);
        //fuzzy logic

        //how likely enemy will retreat == how low health AND how close the player is
        howLikelyToRetreat = Mathf.Min(sampledDistance,SampledEnemyHealth);
        //how likely enemy will try and smell to find player == how long since last time enemy smelt AND how healthy the enemy is OR how fustrated enemy is
        howLikelyToSmell = Mathf.Min(SampledEnemyHealth, sampledSmellTimer);
        //how likely enemy will sprint and how fast == how much stamina enemy has AND how low the players health is AND how much stamina player has OR the distance between the player and the monster 
        howLikelyToSprint = Mathf.Min(sampledEnemyStamina, sampledPlayerHealth, sampledPlayerStamina);
        //howLikelyToSprint = Mathf.Min(howLikelyToSprint, sampledPlayerStamina);
        //howLikelyToSprint = Mathf.Max(howLikelyToSprint, sampledDistance);

        // de-fuzz

        if(howLikelyToSprint > 0.7f)
        {
            Sprint();
        }
        if(howLikelyToSmell > 0.7f)
        {
            currentState = state.smell;
        }

        

        // switch statement to control which state is activated
        switch (currentState)
        {
            case state.patrol:
                patrol();
                break;
            case state.hunt:
                hunt();
                break;
            case state.smell:
                smell();
                break;
            case state.attack:
                attack();
                break;
            case state.retreat:
                retreat();
                break;
            case state.recover:
                recover();
                break;
            case state.birth:
                birth();
                break;
            case state.protect:
                protect();
                break;
            case state.investigate:
                investigate();
                break;
            default:
                Debug.LogError("Invalid state: " + currentState);
                break;
        }
        if (currentState == state.attack) 
        {
            myMat.color = Color.red;
        }
        else if(currentState == state.recover) 
        {
            myMat.color = Color.green;
        }
        else
        {
            myMat.color = Color.black;
        }
    }
    void Sprint() 
    {
        enemy.speed = 25f;
        Debug.Log("springing");
        GetEnemy.isSprinting = true;
        
    }
    public void StopSprint()
    {
        enemy.speed = 20f;
        GetEnemy.isSprinting = false;
    }
    void patrol() 
    {
        //random movement
        if (!enemy.pathPending && enemy.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }
    void hunt()
    {
        //try find players last known position
        if (GetEnemy.canSmellPlayer == true) 
        {
            lastKnownLocation = GetPlayer.transform.position;
            enemy.SetDestination(lastKnownLocation);
            
        }

        else 
        {
            GetEnemy.canSmellPlayer = false;
            reachedPosition = false;
            currentState = StateMachine.state.investigate;
        }

    }
    void investigate() 
    {
        enemy.SetDestination(lastKnownLocation);
        Vector3 upper;
        Vector3 lower;
        upper = lastKnownLocation + difference;
        lower = lastKnownLocation - difference;
        if (reachedPosition == false) 
        {
            if (GetEnemy.transform.position.x <= upper.x && GetEnemy.transform.position.z <= upper.z && GetEnemy.transform.position.x >= lower.x && GetEnemy.transform.position.z >= lower.z)
            {
                currentState = StateMachine.state.patrol;
                reachedPosition = true;
            }
            if (investigateTimer > 0) 
            {
                investigateTimer -= Time.deltaTime;
            }
            else if (investigateTimer <= 0) 
            {
                investigateTimer = 10;
                currentState = StateMachine.state.patrol;
                reachedPosition = true;
            }
        }
       
       
    }
    void smell()
    {
        if (radiusTimer > 0) 
        {
            GetEnemy.GetComponent<SphereCollider>().radius = 15;
            radiusTimer -= Time.deltaTime;
        }
        else 
        {
            radiusTimer = 1;
            GetEnemy.smellTimer = 15;
            GetEnemy.GetComponent<SphereCollider>().radius = 5;
        }

        //increase sense raduis to try find player
    }
    void attack()
    {
        //attack player
        if (attackTimer > 0)
        {
            
            attackTimer -= Time.deltaTime;
        }
        else 
        {
            GetPlayer.pHealth -= 20.0f;
            attackTimer = 1;
        }
    }
    void retreat()
    {
        if(retreatTimer > 0)
        {
            //hide from player
            for (int i = 0; i < GetPlayer.enemyPatrolPoints.Count; i++)
            {
                if (Vector3.Distance(GetPlayer.transform.position, GetPlayer.enemyPatrolPoints[i].transform.position) > Vector3.Distance(GetPlayer.transform.position, furthestPoint))
                {
                    furthestPoint = GetPlayer.enemyPatrolPoints[i].transform.position;
                }
            }
            enemy.SetDestination(furthestPoint);
            //Debug.Log(furthestPoint);
            retreatTimer -= Time.deltaTime;
        }
        else 
        {
            currentState = StateMachine.state.recover;
        }
  
    }
    void birth() 
    {
        //sets eggs to spawn more monsters
    }
    void protect()
    {
        // patrols around the eggs to try to stop player from breaking them
    }
    void recover()
    {
        //heal 
        if (GetEnemy.mHealth < 100)
        {
            if (healTimer < 0)
            {
                //Debug.Log("recovering");
                GetEnemy.mHealth += 5;
                healTimer = 3;
            }
            else
                healTimer -= Time.deltaTime;
           
        }
        else 
        {
            retreatTimer = 10;
            currentState = state.patrol;
        }
            
       
        //Debug.Log(healTimer);

    }
    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0)
            return;
        enemy.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }
    float Remap(float value, float from1,float to1,float from2, float to2) 
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
