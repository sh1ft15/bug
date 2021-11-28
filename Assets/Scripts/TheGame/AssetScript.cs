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
    AudioScript _audioScript;
    DialogScript _dialogScript;
    Transform _player;
    bool _isInRadius;

    void Start(){
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _dialogScript = GameObject.Find("/Canvas").transform.Find("Dialog").GetComponent<DialogScript>();
        _audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();
        _player = GameObject.Find("Player").transform;
        _animator.Play(character.name + "_idle", 0);
        _animator.SetBool("bugged", _bugged);
        _guide.gameObject.SetActive(false);

        string prefKey ="{" + GetAssetID() + "}_has_been_fixed";

        if (_bugged && PlayerPrefs.HasKey(prefKey)) {
            SetBugged(!(PlayerPrefs.GetInt(prefKey) > 0));
        }

        if (character.name.Equals("player")) {
            if (PlayerPrefs.HasKey("bug_has_been_fixed")) {
                gameObject.SetActive(PlayerPrefs.GetInt("bug_has_been_fixed") > 0);
            }
            else { gameObject.SetActive(false); }
        }
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.E) && _isInRadius) { 
            ToggleCombatSystem();
        }
    }

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "Player": 
            _dialogScript.ToggleDialogByKey(character.name);
            
            if (_bugged) {
                _isInRadius = true;
                _guide.gameObject.SetActive(true); 
                CheckFixed();
            }
            break;
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
        if (_bugged) {
            string sceneName = _sceneLoaderScript.GetSceneName();

            PlayerPrefs.SetString("start_character", "testbot");
            PlayerPrefs.SetString("end_character", character.name);
            PlayerPrefs.SetString("prev_scene", sceneName);
            PlayerPrefs.SetFloat(sceneName + "_prev_player_x", _player.transform.position.x);
            PlayerPrefs.SetFloat(sceneName + "_prev_player_y", _player.transform.position.y);
            _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "enter_combat");
            _sceneLoaderScript.LoadScene("Combat");
        }
    }

    public void SetBugged(bool status){
        _bugged = status;
        _animator.SetBool("bugged", _bugged);

        if (!_bugged && _guide.gameObject.activeSelf) { _guide.gameObject.SetActive(false); }
    }

    public void CheckFixed(){
        if (PlayerPrefs.HasKey("prev_combat_result")) {
            int combatResult = PlayerPrefs.GetInt("prev_combat_result");
            bool playerWin = combatResult > 0;

            SetBugged(!playerWin);
            PlayerPrefs.DeleteKey("prev_combat_result");
            PlayerPrefs.SetInt("{" + GetAssetID() + "}_has_been_fixed", combatResult);

            if (character.name.Equals("bug")) { 
                PlayerPrefs.SetInt(character.name + "_has_been_fixed", combatResult);
            }
        }
    }

    string GetAssetID(){
        string sceneName = _sceneLoaderScript.GetSceneName();
        Vector2 post = transform.position;

        return sceneName + "-" + character.name + "-" + post.x.ToString("0.0") + "-" + post.y.ToString("0.0");   
    }
}
