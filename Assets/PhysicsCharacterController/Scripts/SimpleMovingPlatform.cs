using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovingPlatform : MonoBehaviour
{
    public float duration;
    public float moveSpeed;
    private float timer;
    public Vector3 direction;
    public Rigidbody rgbody;


    //THIS IS A VERY PLACEHOLDER-Y VERY UNSIGHTLY MOVING PLATFORM. DON'T USE IT. IT'S JUST HERE TO GIVE YOU BASIC TOOLS TO UNDERSTAND HOW THIS WORKS.
    //THE CHARACTER CONTROLLER IS COMPATIBLE WITH ANY MOVING OBJECT THAT USES A RIGIDBODY AND MOVES USING IT.
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < duration * 2)
            rgbody.MovePosition(rgbody.position+(direction * moveSpeed * (timer > duration? -1 : 1)));
        else
            timer = 0;
    }
}
