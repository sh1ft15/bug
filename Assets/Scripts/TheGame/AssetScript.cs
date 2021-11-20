using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetScript : MonoBehaviour
{
    [SerializeField] GameObject _guide;
    [SerializeField] Animator _animator;
    [SerializeField] CharObj character;
    [SerializeField] bool _bugged;
    SceneLoaderScript _sceneLoaderScript;
    Transform _player;
    bool _isInRadius;

    void Start(){
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _player = GameObject.Find("Player").transform;
        _animator.Play(character.name + "_idle", 0);
        _animator.SetBool("bugged", _bugged);
        _guide.gameObject.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.E) && _isInRadius) { 
            ToggleCombatSystem();
        }
    }

    void OnTriggerEnter2D(Collider2D col){
        if (_bugged) {
            switch(col.tag){
                case "Player": 
                _isInRadius = true;
                _guide.gameObject.SetActive(true); 
                break;
            }
        }
        
    }

    void OnTriggerExit2D(Collider2D col){
        if (_guide.gameObject.activeSelf) {
            switch(col.tag){
                case "Player": 
                _isInRadius = false;
                _guide.gameObject.SetActive(false); 
                break;
            }
        }
    }

    void ToggleCombatSystem(){
        PlayerPrefs.SetString("start_character", "testbot");
        PlayerPrefs.SetString("end_character", character.name);
        PlayerPrefs.SetString("prev_scene", _sceneLoaderScript.GetSceneName());
        PlayerPrefs.SetFloat("prev_player_x", _player.transform.position.x);
        PlayerPrefs.SetFloat("prev_player_y", _player.transform.position.y);
        _sceneLoaderScript.LoadScene("PongCombatSys");
    }
}
