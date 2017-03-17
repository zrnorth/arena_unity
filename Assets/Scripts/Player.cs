using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float speed;

    Rigidbody2D rb2D;

	void Awake () {
        rb2D = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate () {
        ProcessPlayerInput();
	}

    void ProcessPlayerInput() {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(horiz, vert);
        rb2D.velocity = movement * speed;
    }
}
