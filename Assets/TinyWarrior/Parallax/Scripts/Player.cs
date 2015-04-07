using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public float pos = 0f;
    Animator animator;
    float speed = 0f;
    float speedfactor = 1f;
    int direction = 0;
  //TODO a lot of code in this file should be removed, but is necessary to maintain the model framework for now.
	void Start () {
        pos = 0f;
        speed = 0f;

        // print("added speed");
animator = GetComponent<Animator>();
        animator.SetFloat ("Speed",10);
	}
 
    void AddSpeed(float delta)
    {
        speed = Mathf.Clamp( speed + delta, -1f, 1f);
        animator.SetFloat("Speed", speed*speedfactor);
    print("added speed");
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
        animator.SetFloat("Speed", speed*speedfactor);
    print("setted speed");

    }
    void OnAttack()
    {
        animator.SetTrigger("Attack");
    }
    void OnJump()
    {
        animator.SetTrigger("Jump");
    }
    void OnRight()
    {
        direction = 1;
    }
    void OnLeft()
    {
        direction = -1;
    }
    void OnStop()
    {
        direction = 0;
        SetSpeed(0f);
    }

    void UpdateMovement()
    {
  /*      if (direction != 0) AddSpeed(direction * 0.01f);

        pos += speed * Time.deltaTime;*/
    }

	void Update () {
//        UpdateMovement();
	}
}
