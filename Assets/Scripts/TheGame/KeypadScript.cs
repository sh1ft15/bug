using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadScript : MonoBehaviour
{
    [SerializeField] Text _keypadLabel;
    SceneLoaderScript _sceneLoaderScript;
    PlayerScript _playerScript;
    NotesScript _notesScript;
    bool _keypadIsShown;
    string _keypadText, _code, _targetRoom;
    int _codeIndex;

    void Start() { 
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _notesScript = GameObject.Find("/Canvas").transform.Find("Notes").GetComponent<NotesScript>();
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        ToggleKeypad(_keypadIsShown, true);
    }

    public void ToggleKeypad(bool status, bool init = false) {
        _keypadIsShown = status;
        _keypadText = "";
        _keypadLabel.text = "ENTER CODE";
        
        gameObject.SetActive(_keypadIsShown);

        if (!init) { _playerScript.LockMovement(_keypadIsShown); }
    }

    public void CloseKeypad(){
        ToggleKeypad(false);
    }

    public void AddKeypadCode(string text){
        if (_keypadText.Length < 12) { 
            _keypadText += text;
            _keypadLabel.text = _keypadText;
        }
    }

    public void RemoveKeypadCode(){
        if (_keypadText.Length > 0) { 
            _keypadText = _keypadText.Remove(_keypadText.Length - 1, 1);
            _keypadLabel.text = _keypadText;
        }
    }

    public void ClearKeypadCode(){
        if (_keypadText.Length > 0) { 
            _keypadText = "";
            _keypadLabel.text = _keypadText;
        }
    }

    public void SubmitKeypadCode() {
        // Debug.Log(_keypadText + " = " + _code);

        if (_notesScript.CheckCode(_keypadText)) {
            if (_sceneLoaderScript.GetSceneName().Equals("Hallway")) {
                string sceneName = _sceneLoaderScript.GetSceneName();

                PlayerPrefs.SetInt("reset_player_post", 0);
                PlayerPrefs.SetString("prev_scene", sceneName);
                PlayerPrefs.SetFloat(sceneName + "_prev_player_x", _playerScript.transform.position.x);
                PlayerPrefs.SetFloat(sceneName + "_prev_player_y", _playerScript.transform.position.y);
            }
            else { PlayerPrefs.SetInt("reset_player_post", 1); }

             _sceneLoaderScript.LoadScene(_targetRoom);
        }
        // invalid code
        else { 
            _keypadText = "";
            _keypadLabel.text = "INVALID CODE";
        }
    }

    public void HiliteKeypadKey(Image key){
        Color color = key.color;

        color.a = .5f;
        key.color = color;
    }

    public void DehiliteKeypadKey(Image key){
        Color color = key.color;

        color.a = 0;
        key.color = color;
    }

    public void SetTargetRoom(string room){
        _targetRoom = room;
    }

    // public void SetCode(string code){
    //     _code = code;
    // }
}
