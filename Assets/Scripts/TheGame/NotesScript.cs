using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesScript : MonoBehaviour
{
    // [SerializeField] Button _backBtn, _homeBtn, _nextBtn;
    [SerializeField] Transform _pages, _icon;
    [SerializeField] Text _codeOnIcon, _codeOnNote;
    [SerializeField] bool _showIcon;
    PlayerScript _playerScript;
    AudioScript _audioScript;
    int _curPageIndex, _codeIndex;
    string _code;
    bool _notesIsShown;

    void Start() {
        GameObject audio = GameObject.Find("/Audio");
        GameObject player = GameObject.Find("/Player");

        if (player != null) { _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>(); }
        if (audio != null) { _audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>(); }
        
        _curPageIndex = 0;
        _icon.gameObject.SetActive(_showIcon);
        ToggleNotes(_notesIsShown, true);
        UpdateCodeHint(true);
    }

    public void OpenNote(){ ToggleNotes(true); }

    public void CloseNote(){ ToggleNotes(false); }

    public void ToggleNotes(bool status, bool init = false) {
        GoToHome();

        _notesIsShown = status;
        transform.Find("Dialog").gameObject.SetActive(_notesIsShown);

        if (!init) {
            if (_notesIsShown && _audioScript != null) { _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "open_notes"); }
            if (_playerScript != null) { _playerScript.LockMovement(_notesIsShown); }
        }
    }

    public void IteratePage(int dir){
        for(int i = 0; i < _pages.childCount; i++){ _pages.GetChild(i).gameObject.SetActive(false); }

        int nextPageIndex = Mathf.Max(Mathf.Min(_curPageIndex + dir, _pages.childCount - 1), 0);
        Transform page = _pages.GetChild(nextPageIndex);

        if (page != null) { 
            _curPageIndex = nextPageIndex;
            page.gameObject.SetActive(true);
        }
        else { GoToHome(); }
    }

    public void GoToHome(bool init = false){
        for(int i = 0; i < _pages.childCount; i++){ _pages.GetChild(i).gameObject.SetActive(false); }

        _curPageIndex = 0;
        _pages.GetChild(_curPageIndex).gameObject.SetActive(true);
    }

    public void GoToPage(int index){
        for(int i = 0; i < _pages.childCount; i++){ _pages.GetChild(i).gameObject.SetActive(false); }
        
        Transform page = _pages.GetChild(index);

        if (page != null) { 
            page.gameObject.SetActive(true); 
            _curPageIndex = index;
        }
        else { GoToHome(); }
    }

    public void HiliteKey(Image key){
        Color color = key.color;

        color.a = 1f;
        key.color = color;
    }

    public void DehiliteKey(Image key){
        Color color = key.color;

        color.a = .5f;
        key.color = color;
    }

    public void UpdateCodeHint(bool init = false){
        string fill = "",
               randomStr,
               tempCode; 

        if (init) {
            if (PlayerPrefs.HasKey("room_code")) {
                _code = PlayerPrefs.GetString("room_code");
                _codeIndex = PlayerPrefs.GetInt("room_code_index");
            }
            else {
                randomStr = "0123456789";

                for (int i = 0; i < 5; i++) {
                    _code += randomStr[Random.Range(0, randomStr.Length)];
                }

                _codeIndex = 0;

                PlayerPrefs.SetString("room_code", _code);
                PlayerPrefs.SetInt("room_code_index", _codeIndex);
            }
        }

        if (_codeIndex <= _code.Length - 1) {
            for (int i = 0; i < (_code.Length - _codeIndex); i++) { fill += "*"; }
        }

        tempCode = _code.Substring(0, _codeIndex) + fill;
        _codeOnNote.text = tempCode;

        if (_icon.gameObject.activeSelf) { _codeOnIcon.text = tempCode; }
    }

    public bool CheckCode(string code) {
        return _code.Equals(code);
    }
}
