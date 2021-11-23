using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadScript : MonoBehaviour
{
    [SerializeField] Text _keypadLabel, _keypadCode;
    SceneLoaderScript _sceneLoaderScript;
    PlayerScript _playerScript;
    bool _keypadIsShown;
    string _keypadText, _code, _targetRoom;
    int _codeIndex;

    void Start() { 
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        ToggleKeypad(_keypadIsShown);

        if (PlayerPrefs.HasKey("room_code")) {
            _code = PlayerPrefs.GetString("room_code");
            _codeIndex = PlayerPrefs.GetInt("room_code_index");
        }
        else {
            string randomStr = "0123456789";

            for (int i = 0; i < 10; i++) {
                _code += randomStr[Random.Range(0, randomStr.Length)];
            }

            _codeIndex = 0;

            PlayerPrefs.SetString("room_code", _code);
            PlayerPrefs.SetInt("room_code_index", _codeIndex);
        }

        UpdateCodeHint();
    }

    public void ToggleKeypad(bool status) {
        _keypadIsShown = status;
        _keypadText = "";
        _keypadLabel.text = "ENTER CODE";
        
        gameObject.SetActive(_keypadIsShown);
        _playerScript.LockMovement(_keypadIsShown);
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
        Debug.Log(_keypadText + " = " + _code);

        if (_keypadText.Equals(_code)) {
            if (_sceneLoaderScript.GetSceneName().Equals("Hallway")) {
                PlayerPrefs.SetInt("reset_player_post", 0);
                PlayerPrefs.SetString("prev_scene", _sceneLoaderScript.GetSceneName());
                PlayerPrefs.SetFloat("prev_player_x", _playerScript.transform.position.x);
                PlayerPrefs.SetFloat("prev_player_y", _playerScript.transform.position.y);
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

    public void UpdateCodeHint(){
        if (_keypadCode.text != null) {
            string fill = ""; 

            if (_codeIndex <= _code.Length - 1) {
                for (int i = 0; i < (_code.Length - _codeIndex); i++) { fill += "*"; }
            }

            _keypadCode.text = _code.Substring(0, _codeIndex) + fill;
        }
    }
}
