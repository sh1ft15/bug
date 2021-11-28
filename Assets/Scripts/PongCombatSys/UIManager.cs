using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] float _maxHealth;
    [SerializeField] Transform _healthBar, _curTile, _specialTilesOnPath, _curTurn, _curSpecialTile, _blueChar, _redChar;
    [SerializeField] TileCursor _tileCursor;
    [SerializeField] Button _resetPathBtn;
    [SerializeField] LineRenderer _lineSpecialTile;
    [SerializeField] Text _timeLimitText, _turnTimeLimitText;
    float _blueHealth, _redHealth;
    GridManager _gridManager;
    BallManager _ballManager;
    SceneLoaderScript _sceneLoaderScript;
    AudioScript _audioScript;
    EnemyAI _enemyAI;
    Collider2D _objOnCursor;
    Coroutine _iterateCurrentTileCoroutine;
    bool _onMouseHold, _isPaused, _playerTurn;
    float _turnTimeLimit, _timeLimit;

    void Start() {
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();
        _gridManager = GameObject.Find("/GridManager").GetComponent<GridManager>();
        _ballManager = GameObject.Find("/BallManager").GetComponent<BallManager>();
        _enemyAI = GameObject.Find("/EnemyAI").GetComponent<EnemyAI>();
        _playerTurn = true;
        _turnTimeLimit = 0;
        _timeLimitText.enabled = false;
        _enemyAI.SetExists(true);

        _audioScript.PlayMusic("bgm");
        UpdateTurn(true);
        UpdateHealth(_maxHealth / 2);
    }

    void Update(){
        if (_curSpecialTile.gameObject.activeSelf) { CheckCurSpecialTile(); }

        if (_turnTimeLimit > 0) { 
            _turnTimeLimit -= Time.deltaTime;
            UpdateTurnTimeLimit(); 
        }

        if (_playerTurn) {
            if (Input.GetMouseButtonDown(0)) {
                if (!_onMouseHold) { _onMouseHold = true; }

                _objOnCursor = GetObjectOnMouse();

                if (_objOnCursor != null) {
                    switch(_objOnCursor.transform.tag){
                        case "Tile":
                            Tile tile = _objOnCursor.GetComponent<Tile>();

                            if (tile != null && _gridManager.IsSpecialTile(tile)) {
                                _tileCursor.ToggleCursor(true);
                                _tileCursor.SetOriginTile(tile);
                            }
                        break;
                    }
                }
            }

            if (Input.GetMouseButton(0)) {
                _tileCursor.transform.position = GetMousePost();
            }

            if (Input.GetMouseButtonUp(0)) {
                if (_onMouseHold) { _onMouseHold = false; }
                
                if (_objOnCursor != null) {
                    switch(_objOnCursor.transform.tag){
                        case "Tile":
                            Tile targetTile = _tileCursor.GetTargetTile(),
                                originTile = _tileCursor.GetOriginTile(),
                                tile = _objOnCursor.GetComponent<Tile>();

                            if (tile != null) {
                                if (targetTile != null && originTile != null && _tileCursor.OnTarget()) {
                                    // target tile is a path and not a special tile
                                    // origin tile must not be from the path
                                    if (targetTile.IsPath() && !_gridManager.IsSpecialTile(targetTile) && !_gridManager.IsSpecialTileOnPath(originTile)) {
                                        targetTile.SetHost(originTile.GetHost());
                                        _gridManager.AddSpecialTile(targetTile);
                                        _gridManager.RemoveSpecialTile(originTile);
                                        _gridManager.MoveCurSpecialTileOnPath();
                                        _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "place_tile");
                                        UpdateTurn();
                                    }
                                    // add warning here
                                    else { _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "cannot_place_tile"); }
                                }
                                else if (_gridManager.IsFirstTile(tile) && !_ballManager.HasBall()) { 
                                    _ballManager.SpawnBall(_gridManager.GetMainTiles()); 
                                    _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "place_tile");
                                    UpdateTurn();
                                }
                            }
                        break;
                    }
                }

                _tileCursor.ToggleCursor(false);
            }
        }
        else { 
            if (_tileCursor.gameObject.activeSelf) { _tileCursor.ToggleCursor(false); }

            if (Input.GetMouseButtonUp(0)) { _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "cannot_place_tile"); }  
        }
    }

    Collider2D GetObjectOnMouse(){
        Vector2 mousePos2D = GetMousePost();
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);  

        return hit.collider ?? null;
    }

    Vector2 GetMousePost() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        return new Vector2(mousePos.x, mousePos.y);
    }

    public void SetHost(int index, HostObj host) {
        Transform tile = _specialTilesOnPath.GetChild(index);
        
        if (tile != null) {
            tile.GetComponent<Image>().sprite = host.baseSprite;
            
        }
    }

    public void TriggerCurrentTile(int index) {
        if (_iterateCurrentTileCoroutine != null) { StopCoroutine(_iterateCurrentTileCoroutine); }

        _iterateCurrentTileCoroutine = StartCoroutine(IterateCurrentTile(index));
    }

    // public bool IsPlayerTurn() { return _playerTurn; }

    void UpdateTurnTimeLimit(){
        float minutes = Mathf.FloorToInt(_turnTimeLimit / 60),
              seconds = Mathf.FloorToInt(_turnTimeLimit % 60);

        _turnTimeLimitText.text = string.Format("{0:00} : {1:00}", Mathf.Max(minutes, 0), Mathf.Max(seconds, 0));

        if (_turnTimeLimit <= 0) { UpdateTurn(); }
    }

    IEnumerator IterateCurrentTile(int index){
        List<float> xPosts = new List<float>{-70, 0, 70};
        Vector2 curPost = _curTile.GetComponent<RectTransform>().anchoredPosition,
                post = curPost;
        Transform tile = _specialTilesOnPath.GetChild(index);
        
        ToggleCurSpecialTile(null);

        if (index != -1 && tile != null) {
            yield return new WaitForSeconds(.5f);

            post.x = xPosts[index];
            _curTile.Find("Tile").gameObject.SetActive(false);

            while(Vector2.Distance(curPost, post) > .1f){
                curPost = Vector2.Lerp(curPost, post, Time.deltaTime * 10);
                _curTile.GetComponent<RectTransform>().anchoredPosition = curPost;

                yield return null;
            }

            SetCurrentTileSprite(tile.GetComponent<Image>().sprite);
            _curTile.Find("Tile").gameObject.SetActive(true);
            _curTile.GetComponent<RectTransform>().anchoredPosition = post;
        }

        CheckCurSpecialTile();

        _iterateCurrentTileCoroutine = null;
    }

    public void CheckCurSpecialTile(){
        Tile curSpecialTile = _gridManager.GetCurSpecialTileOnPath();

        if (curSpecialTile != null) { ToggleCurSpecialTile(curSpecialTile); }
        else { ToggleCurSpecialTile(null); }
    }

    public void SetCurrentTileSprite(Sprite sprite){
        _curTile.Find("Tile").GetComponent<Image>().sprite = sprite;
    }

    public void ToggleCurSpecialTile(Tile tile){
        if (tile != null) {
            _curSpecialTile.gameObject.SetActive(true);
            _lineSpecialTile.gameObject.SetActive(true);
            _curSpecialTile.Find("Sprite").GetComponent<SpriteRenderer>().sprite = tile.GetHost().baseSprite;
            _lineSpecialTile.SetPosition(0, tile.transform.position);
            _lineSpecialTile.SetPosition(1, Camera.main.ScreenToWorldPoint(_curTile.transform.position));
            _curSpecialTile.position = tile.transform.position; 
        }
        else { 
            _lineSpecialTile.gameObject.SetActive(false);
            _curSpecialTile.gameObject.SetActive(false); 
        }
    }

    public void UpdateTurn(bool init = false){
        bool temp;

        if (init) { temp = init; }
        else { temp = _enemyAI.IsExists()? !_playerTurn : true; }

        if (_playerTurn != temp || init) {
            _playerTurn = temp;
            _curTurn.Find("LArrow").gameObject.SetActive(_playerTurn);
            _curTurn.Find("RArrow").gameObject.SetActive(!_playerTurn);
        }

        _turnTimeLimit = init ? 1800 : 6;
        _turnTimeLimitText.color = _playerTurn ? Color.blue : Color.red;

        if (!_playerTurn) { _enemyAI.TriggerAction(); }
    }

    public void UpdateHealth(float num = 0){
        Transform image = _healthBar.Find("BlueBar");
        Vector2 scale = image.localScale;
        Text numLabel = _healthBar.Find("Num").GetComponent<Text>();
        bool playerWin;

        _blueHealth = Mathf.Max(Mathf.Min(_blueHealth + num, _maxHealth), 0);
        _redHealth = Mathf.Max(_maxHealth - _blueHealth, 0);
        scale.x = _blueHealth / _maxHealth;
        image.localScale = scale;
        numLabel.text = _blueHealth.ToString("0") + " : " + _redHealth.ToString("0");
        playerWin = _blueHealth >= _maxHealth;

        if (_blueHealth <= 0 || playerWin) { TogglePrevScene(playerWin); }
    }

    public void ResetHealth(float num = 0){
        if (num > 0) { _blueHealth = Mathf.Max(Mathf.Min(_blueHealth, _maxHealth), 0); }
        else { _blueHealth = _maxHealth / 2;}
        
        UpdateHealth();
    }
    
    public bool OnMouseHold() { return _onMouseHold; }

    public void ToggleResetBtn(bool status) {
        _resetPathBtn.interactable = status;
    }

    public void SetCharActive(string side, bool status){
        Transform obj;

        if (side.Equals("start")) { obj = _blueChar; }
        else { obj = _redChar; }

        obj.GetComponent<Animator>().SetBool("active", status);
    }

    public void SetChar(string side, CharObj charObj){
        Transform obj;

        if (side.Equals("start")) { obj = _blueChar; }
        else { obj = _redChar; }

        if (charObj != null) {
            Image image = obj.Find("Object/Image").GetComponent<Image>();

            obj.Find("Label").gameObject.SetActive(true);
            obj.Find("Object").GetComponent<Animator>().Play("active", 0);
            obj.Find("Label/Text").GetComponent<Text>().text = charObj.charName;

            image.sprite = charObj.sprite;
            image.transform.rotation = Quaternion.Euler(0, charObj.spriteDir >= 0 ? 0 : 180, 0);

            // temp fix for bug character face not showing
            if (charObj.name.Equals("bug")) {
                Vector2 post = obj.Find("Object").transform.position;

                post.y += 130;
                obj.Find("Object").transform.position = post;
            }
        }
        else { obj.Find("Label").gameObject.SetActive(false); }
    }

    public void TogglePrevScene(bool status){
        // if win, unlock fragment of the door code
        if (status) {
            int codeIndex = PlayerPrefs.GetInt("room_code_index");

            codeIndex = Mathf.Min(codeIndex + 1, 5);
            PlayerPrefs.SetInt("room_code_index", codeIndex);
        }
        
        PlayerPrefs.SetInt("prev_combat_result", status ? 1 : 0);
        PlayerPrefs.SetInt("reset_player_post", 1);
        _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "enter_combat");
        _sceneLoaderScript.LoadScene(PlayerPrefs.GetString("prev_scene"));
    }
}