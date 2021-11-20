using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadScript : MonoBehaviour
{
    [SerializeField] Text _keypadLabel;
    SceneLoaderScript _sceneLoaderScript;
    PlayerScript _playerScript;
    bool _keypadIsShown;
    string _keypadText, _targetRoom;

    void Start() { 
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        ToggleKeypad(_keypadIsShown);
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

    public void SetTargetRoom(){

    }
}
