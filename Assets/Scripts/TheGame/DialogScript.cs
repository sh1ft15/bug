using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogScript : MonoBehaviour
{
    [SerializeField] Text _dialogText;
    Dictionary<string, List<string>> _dialogScripts;
    Dictionary<string, bool> _seenScripts;
    List<string> _scriptTexts;
    SceneLoaderScript _sceneLoaderScript;
    PlayerScript _playerScript;
    Coroutine _typeTextCoroutine;
    bool _dialogIsShown;
    int _curIndex;

    void Awake(){
        GameObject player = GameObject.Find("/Player");

        if (player != null) { _playerScript = player.GetComponent<PlayerScript>(); }

        _sceneLoaderScript = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();
        
        _dialogScripts = new Dictionary<string, List<string>>();
        _dialogScripts["Hallway"] = new List<string>(){
            "This is the hallway, here the player can access to various test rooms. The player play as a 'test bot'.",
            "The idea is for the player to search and fix for the bugged assets.",
            "The area is kinda empty because its development start on the 2nd half of game jam."
        };
        _dialogScripts["Combat"] = new List<string>(){
            "This is the combat arena where I spent most of the development time.",
            "At first I envisioned idea to be super cool but it end up being super bland.",
            "The enemy AI is also kinda dumb. You can click the 'Notes' button to see the mechanic of the game."
        };
        _dialogScripts["Room101"] = new List<string>(){
            "This room supposedly to be the tutorial area with the first bugged asset.",
            "However, the doors to the other rooms are clearly accessible so I'm not sure whether the player will access this room first."
        };
        _dialogScripts["door"] = new List<string>(){
            "This is supposed to act as a fake door for comedic relief or clever twist for the game",
            "However, the asset sprite is too small so it doesn't work that well."
        };
        _dialogScripts["trashcan"] = new List<string>(){
            "I failed to notice sprite size of this 'Trash Can' during asset developement. So, it's as tall as the player"
        };
        _dialogScripts["couch"] = new List<string>(){
            "The angle in which the couch is positioned is kinda off. The same goes for the TV sprite."
        };
        _dialogScripts["bug"] = new List<string>(){
            "'The Bug' is supposedly to be the 'origin' of the bugged assets. Moving away from the player view from time to time and leaving along the bugged assets.",
            "However, the idea is quickly scraped off as it's taking to much time to just finish the game. Now it just act as any other bugged assets."
        };
        _dialogScripts["player"] = new List<string>(){
            "I envisioned 'The Player' to be the final antagonist where it will try to take over the game.",
            "This idea is also being abandoned because I got no clue how to do it."
        };

        _seenScripts = new Dictionary<string, bool>();

        foreach(string key in _dialogScripts.Keys) {
            string prefKey = "player_has_read_{" + key + "}";
            bool hasSeen = false;

            if (PlayerPrefs.HasKey(prefKey)) {
                hasSeen = PlayerPrefs.GetInt(prefKey) > 0;
            }
            
            _seenScripts[key] = hasSeen;
        }

        _dialogIsShown = false;
        transform.Find("Object").gameObject.SetActive(_dialogIsShown);
    }

    void Start(){
        if (_sceneLoaderScript != null) {
            string sceneName = _sceneLoaderScript.GetSceneName();

            ToggleDialogByKey(sceneName);
        }
    }

    public void SetDialogScripts(string key) {
        if (_dialogScripts.ContainsKey(key)) {
            _curIndex = 0;
            _scriptTexts = _dialogScripts[key];
            TriggerTypeText(GetPageLabel() + _scriptTexts[0]);
        }
        else { _scriptTexts = new List<string>(); }
    }

    public void ToggleDialog(bool status) {
        if (status && _scriptTexts.Count > 0) { _dialogIsShown = status; } 
        else { _dialogIsShown = false; }

        transform.Find("Object").gameObject.SetActive(_dialogIsShown);

        if (_playerScript != null) { _playerScript.LockMovement(_dialogIsShown); }
    }

    public void IterateDialog(int dir) {
        if (_scriptTexts.Count > 0) {
            int tempIndex = Mathf.Max(Mathf.Min(_curIndex + dir, _scriptTexts.Count - 1), 0);

            if (tempIndex != _curIndex) {
                _curIndex = tempIndex;
                TriggerTypeText(GetPageLabel() + _scriptTexts[_curIndex]);
            }
        }
    }

    public void ToggleDialogByKey(string key){
        if (_dialogScripts.ContainsKey(key)) {
            if (_seenScripts.ContainsKey(key) && !_seenScripts[key]) {
                SetDialogScripts(key);
                ToggleDialog(true);

                // has seen
                _seenScripts[key] = true;
                PlayerPrefs.SetInt("player_has_read_{" + key + "}", 1); 
            }
        }
    }

    public void ImmediateTypeText(){
        if (_typeTextCoroutine != null) { StopCoroutine(_typeTextCoroutine); }
        
        _dialogText.text = GetPageLabel() + _scriptTexts[_curIndex];
    }

    public bool DialogIsShown() { return _dialogIsShown; }

    void TriggerTypeText(string str){
        if (_typeTextCoroutine != null) { StopCoroutine(_typeTextCoroutine); }

        _dialogText.text = "";
        _typeTextCoroutine = StartCoroutine(TypeText(str, _dialogText));
    }

    string GetPageLabel(){
        return "[" + (_curIndex + 1) + "/" + (_scriptTexts.Count) + "] ";
    }

    IEnumerator TypeText(string str, Text label){
        float time = 0;
        int charIndex = 0;

        while(charIndex < str.Length) {
            time += Time.deltaTime * 30;
            charIndex = Mathf.FloorToInt(time);
            charIndex = Mathf.Clamp(charIndex, 0, str.Length);
            label.text = str.Substring(0, charIndex);

            yield return null;
        }

        label.text = str;
    }
}
