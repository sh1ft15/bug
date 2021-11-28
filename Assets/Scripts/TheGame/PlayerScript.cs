using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerScript : MonoBehaviour {
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] Animator animator;
    [SerializeField] Transform character;
    [SerializeField] float moveSpeed = 10f;
    // public float maxSpeed = 7f;
    SceneLoaderScript sceneLoaderScript;
    Vector2 direction;
    bool _moveLocked, facingRight = true;
    AudioScript audioScript;
    // UIScript uiScript;
    List<Sprite> items;
    Coroutine footstepCoroutine;
    
    void Start() { 
        sceneLoaderScript = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();
        audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();
        ResetInitPost();
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

        if (num > 0 && footstepCoroutine == null) {
            footstepCoroutine = StartCoroutine(CycleFootStep());
        }
    }

    void Flip() {
        facingRight = !facingRight;
        character.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    public void LockMovement(bool status) {
        _moveLocked = status;
        direction = Vector2.zero;
    }

    void ResetInitPost(){
        if (PlayerPrefs.GetInt("reset_player_post") > 0) {
            string sceneName = sceneLoaderScript.GetSceneName();
            float x = PlayerPrefs.GetFloat(sceneName + "_prev_player_x"),
                  y = PlayerPrefs.GetFloat(sceneName + "_prev_player_y");
                  
            transform.position = new Vector2(x, y);
            PlayerPrefs.DeleteKey("reset_player_post");
        }
    }

    IEnumerator CycleFootStep(){
        audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "footstep");
        yield return new WaitForSeconds(0.3f);
        footstepCoroutine = null;
    }
}