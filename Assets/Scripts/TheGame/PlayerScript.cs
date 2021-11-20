using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] Animator animator;
    [SerializeField] Transform character;
    [SerializeField] float moveSpeed = 10f;
    // public float maxSpeed = 7f;
    SceneLoaderScript sceneLoaderScript;
    Vector2 direction;
    bool _moveLocked, facingRight = true;

    // EventManagerScript eventManagerScript;
    // AudioScript audioScript;
    // UIScript uiScript;
    List<Sprite> items;
    Coroutine footstepCoroutine;
    
    void Start() { 
        sceneLoaderScript = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();

        // eventManagerScript = GameObject.Find("EventManager").GetComponent<EventManagerScript>();
        // audioScript = GameObject.Find("Audio").GetComponent<AudioScript>();
        // uiScript = GameObject.Find("UI").GetComponent<UIScript>();

        // if (PlayerPrefs.HasKey("player_topless")) { 
        //     topless = PlayerPrefs.GetInt("player_topless") == 1; 
        // }
        
    }

    // Update is called once per frame
    void Update() {
        if (_moveLocked) { return; }
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
    }

    void FixedUpdate() {
        moveCharacter(direction.x);
    }

    void moveCharacter(float horizontal) {
        float num;

        rbody.velocity = direction * moveSpeed;
        num = Mathf.Abs(rbody.velocity.x);

        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight)) { Flip(); }

        animator.SetFloat("horizontal", num);

        // if (num > 0 && footstepCoroutine == null) {
        //     footstepCoroutine = StartCoroutine(CycleFootStep());
        // }
    }

    void Flip() {
        facingRight = !facingRight;
        character.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    public void LockMovement(bool status) {
        _moveLocked = status;
    }

    void ResetInitPost(){
        if (PlayerPrefs.GetInt("reset_player_post") > 0) {
            float x = PlayerPrefs.GetFloat("prev_player_x"),
                  y = PlayerPrefs.GetFloat("prev_player_y");
                  
            transform.position = new Vector2(x, y);
        }
    }

    // IEnumerator CycleFootStep(){
    //     audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "footstep");
    //     yield return new WaitForSeconds(0.3f);
    //     footstepCoroutine = null;
    // }
}