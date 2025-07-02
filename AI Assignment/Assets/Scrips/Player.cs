using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float speed = 0.025f;
    public LayerMask wall;
    public List<Transform> enemyPatrolPoints;
    public List<float> distanceFromPP;
    Vector3 movement;
    bool isColliding = false;
    public float pHealth;
    public float pStamina;
 
    
    private void Start()
    {
        pHealth = 50f;
        pStamina = 100f;
    }
    void Update()
    {


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        movement = new Vector3(x * speed, 0, z * speed);

        // Call the new method to move the player with collision detection
        MoveWithCollisions(movement);

        // calculating distance between player and patrol points 
        for (int i = 0; i < enemyPatrolPoints.Count; i++)
        {
            distanceFromPP[i] = Vector3.Distance(transform.position, enemyPatrolPoints[i].transform.position);
        }

        if(pHealth < 0) 
        {
            this.gameObject.SetActive(false);
        }
    }

    void MoveWithCollisions(Vector3 movement)
    {
        // Get the player's current position
        Vector3 currentPosition = transform.position;

        // Move the player's position by the movement vector
        Vector3 newPosition = currentPosition + movement;

        // Check for collisions between the player's current and new positions
        RaycastHit hitInfo;
        if (Physics.Linecast(currentPosition, newPosition, out hitInfo, wall))
        {
            // If there is a collision, don't move the player and set isColliding to true
            isColliding = true;
            return;
        }

        // If there is no collision, move the player and set isColliding to false
        isColliding = false;
        transform.position = newPosition;
    }

}

