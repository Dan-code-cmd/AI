using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    public float mHealth;
    public StateMachine stateMachine;
    public float smelldistance;
    public float distanceFromPlayer;
    public bool canSmellPlayer;
    Collider player;
    public bool hunting;
    public float smellTimer = 15;
    public float eStamina;
    public float fustration;
    public bool isSprinting;
    void Start()
    {
        mHealth = 100f;
        canSmellPlayer = false;
        hunting = false;
        eStamina = 100f;
        fustration = 0f;
        isSprinting = false;
    }

    // Update is called once per frame
    void Update()
    {


        if (mHealth == 100 && canSmellPlayer != true && stateMachine.currentState != StateMachine.state.hunt && stateMachine.currentState != StateMachine.state.investigate)
        {
            stateMachine.currentState = StateMachine.state.patrol;
        }
        
        if (mHealth < 50 && stateMachine.currentState != StateMachine.state.recover)
        {
            stateMachine.currentState = StateMachine.state.retreat;
        }
        if (canSmellPlayer == true && mHealth == 100)
        {
            stateMachine.currentState = StateMachine.state.hunt;
        }
        if (distanceFromPlayer <= 4.0f)
        {
            stateMachine.currentState = StateMachine.state.attack;
        }
        if(isSprinting == true) 
        {
            eStamina -= 50 * Time.deltaTime;
        }
        if (!isSprinting && eStamina < 100) 
        {
            eStamina += 50 * Time.deltaTime;

        }
        if (eStamina < 0) 
        {
            stateMachine.StopSprint();
        }
        if (smellTimer > 0)
        {
            smellTimer -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            mHealth -= 20;
        }
       
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player") 
        {
            canSmellPlayer = true;
        }
        

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canSmellPlayer = false;
        }
       

    }

}

