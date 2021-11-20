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
    EnemyAI _enemyAI;
    Collider2D _objOnCursor;
    Coroutine _iterateCurrentTileCoroutine;
    bool _onMouseHold, _isPaused, _playerTurn;
    float _turnTimeLimit, _timeLimit;

    void Start() {
        _sceneLoaderScript = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _gridManager = GameObject.Find("/GridManager").GetComponent<GridManager>();
        _ballManager = GameObject.Find("/BallManager").GetComponent<BallManager>();
        _enemyAI = GameObject.Find("/EnemyAI").GetComponent<EnemyAI>();
        _playerTurn = true;
        _turnTimeLimit = 30;
        _timeLimitText.enabled = false;
        _enemyAI.SetExists(true);

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
                                        UpdateTurn();
                                    }
                                    // add warning here
                                    else { }
                                }
                                else if (_gridManager.IsFirstTile(tile) && !_ballManager.HasBall()) { 
                                    _ballManager.SpawnBall(_gridManager.GetMainTiles()); 
                                    UpdateTurn();
                                }
                            }
                        break;
                    }
                }

                _tileCursor.ToggleCursor(false);
            }
        }
        else { if (_tileCursor.gameObject.activeSelf) { _tileCursor.ToggleCursor(false); } }
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

        _turnTimeLimit = init ? 30 : 6;
        _turnTimeLimitText.color = _playerTurn ? Color.blue : Color.red;

        if (!_playerTurn) { _enemyAI.TriggerAction(); }
    }

    public void UpdateHealth(float num = 0){
        Transform image = _healthBar.Find("BlueBar");
        Vector2 scale = image.localScale;
        Text numLabel = _healthBar.Find("Num").GetComponent<Text>();

        _blueHealth = Mathf.Max(Mathf.Min(_blueHealth + num, _maxHealth), 0);
        _redHealth = Mathf.Max(_maxHealth - _blueHealth, 0);
        scale.x = _blueHealth / _maxHealth;
        image.localScale = scale;
        numLabel.text = _blueHealth.ToString("0") + " : " + _redHealth.ToString("0");

        if (_blueHealth <= 0 || _blueHealth >= _maxHealth) {
            TogglePrevScene();
        }
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
        }
        else { obj.Find("Label").gameObject.SetActive(false); }
    }

    void TogglePrevScene(){
        PlayerPrefs.SetInt("reset_player_post", 1);
        _sceneLoaderScript.LoadScene(PlayerPrefs.GetString("prev_scene"));
    }
}

// public void IterateGoal(Goal goal, bool? status = null) {
//     int goalIndex = _goals.IndexOf(goal),
//         nextIndex = goalIndex + 1;

//     if (status == null) {
//         float rate = goal.GetHost().Equals("start") ? _blueHealth : _redHealth;
        
//         status = Mathf.Abs(goal.GetRate() - rate) <= 0.1f;
//     }

//     goal.SetRateColor((bool) status ? Color.green : Color.red);
//     goal.ToggleActive(false);
    
//     if (nextIndex < _goals.Count && _goals[nextIndex].gameObject.activeSelf) {
//         _activeGoal = _goals[nextIndex];
//         _goals[nextIndex].ToggleActive(true);
//         ResetHealth();
//     }
//     else { _ballManager.RemoveAllBall(); }
// }

// public void CheckActiveGoal(){
//     if (_activeGoal != null) {
//         float rate = _activeGoal.GetHost().Equals("start") ? _blueHealth : _redHealth;
//         int goalIndex = _goals.IndexOf(_activeGoal);

//         if (Mathf.Abs(_activeGoal.GetRate() - rate) <= 0.1f) { IterateGoal(_activeGoal); }
//     }
// }

// public void SetupGoals(){
//     _activeGoal = _goals[0];

//     for(int i = 0; i < _goals.Count; i++){
//         Goal goal = _goals[i];

//         if (i < _goalMax) {
//             if (!goal.gameObject.activeSelf) { goal.gameObject.SetActive(false); }
//             float randRate = Random.value <= .5f ? Random.Range(1, 4) : Random.Range(6, 9);

//             goal.SetHost(Random.value <= .5f ? "start" : "end");
//             goal.SetRate(Mathf.RoundToInt(randRate) * 10);
//             goal.SetRateColor(Color.white);
//             goal.SetTimeLimit(60);
//             goal.ToggleActive(i == 0);
//         }
//         else { goal.gameObject.SetActive(false); }
//     }
// }