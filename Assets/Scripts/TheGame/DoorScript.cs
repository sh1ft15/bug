using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorScript : MonoBehaviour
{
    [SerializeField] string _roomName;
    [SerializeField] GameObject _guide;
    [SerializeField] Text _roomLabel;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Sprite _normalDoor, _keypadDoor;
    [SerializeField] bool _hasKeypad;
    SceneLoaderScript _sceneLoaderScript;
    // EventManagerScript eventManagerScript;
    // AudioScript audioScript;
    PlayerScript _playerScript;
    KeypadScript _keypadScript;
    bool _isInRadius;
    string _keypadText;

    void Start() { 
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _keypadScript = GameObject.Find("/Canvas").transform.Find("Keypad").GetComponent<KeypadScript>();
        // eventManagerScript = GameObject.Find("EventManager").GetComponent<EventManagerScript>();
        // audioScript = GameObject.Find("Audio").GetComponent<AudioScript>();
        _guide.gameObject.SetActive(false);
        _roomLabel.text = _roomName.Replace("Room", "");
        _renderer.sprite = _hasKeypad ? _keypadDoor : _normalDoor;
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.E) && _isInRadius) { 
            if (_hasKeypad) { _keypadScript.ToggleKeypad(true); }
            else { ToggleRoom(); }  
        }
    }

    public void ToggleRoom() {
        // bool status = eventManagerScript.CheckEvent(transform);

        // if (status) {
            // StorePlayerState();
            _sceneLoaderScript.LoadScene(_roomName);

            // if (transform.GetComponent<SpriteRenderer>().sprite != null) {
            //     audioScript.PlayAudio(null, "door_opening");
            // }
        // }
    }

    void OnTriggerEnter2D(Collider2D col){
        switch(col.tag){
            case "Player": 
            // _player = col.transform.root;
            _isInRadius = true;
            _guide.gameObject.SetActive(true);
            break;
        }
    }

    void OnTriggerExit2D(Collider2D col){
        switch(col.tag){
            case "Player": 
            // _player = null;
            _isInRadius = false; 
            _guide.gameObject.SetActive(false);
            break;
        }
    }

    // void StorePlayerState(){
    //     if (player != null) {
    //         PlayerScript script = player.GetComponent<PlayerScript>();
    //         string curSceneName = sceneLoaderScript.GetSceneName(),
    //                itemsStr = "";

    //         foreach(Sprite item in script.GetItems()){ 
    //             if (item != null) { itemsStr += item.name + "|"; }
    //         }

    //         PlayerPrefs.SetInt("player_topless", script.IsTopless()? 1 : 0);
    //         PlayerPrefs.SetFloat(curSceneName + "_player_post_x", player.position.x);
    //         PlayerPrefs.SetFloat(curSceneName + "_player_post_y", player.position.y);
    //         PlayerPrefs.SetString("player_items", itemsStr);
    //         PlayerPrefs.Save();
    //     }
    // }
}
