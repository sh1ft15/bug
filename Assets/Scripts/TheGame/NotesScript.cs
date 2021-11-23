using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesScript : MonoBehaviour
{
    // [SerializeField] Button _backBtn, _homeBtn, _nextBtn;
    [SerializeField] Transform _pages;
    PlayerScript _playerScript;
    bool _notesIsShown;
    int _curPageIndex;

    void Start() {
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _curPageIndex = 0;

        ToggleNotes(_notesIsShown);
    }

    public void ToggleNotes(bool status) {
        _notesIsShown = status;
        gameObject.SetActive(_notesIsShown);
        _playerScript.LockMovement(_notesIsShown);
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

    public void GoToHome(){
        for(int i = 0; i < _pages.childCount; i++){ _pages.GetChild(i).gameObject.SetActive(false); }

        _curPageIndex = 0;
        _pages.GetChild(_curPageIndex).gameObject.SetActive(true);
    }

    public void GoToPage(int index){
        for(int i = 0; i < _pages.childCount; i++){ _pages.GetChild(i).gameObject.SetActive(false); }
        
        Transform page = _pages.GetChild(index);

        if (page != null) { 
            _curPageIndex = index;
            page.gameObject.SetActive(true); 
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
}
