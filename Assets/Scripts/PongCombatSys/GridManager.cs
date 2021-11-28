using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour {
    [SerializeField] int _width, _height;
    [SerializeField] Tile _tilePrefab;
    [SerializeField] Transform _cam;
    [SerializeField] List<HostObj> _hosts;
    [SerializeField] List<CharObj> _characters;
    Dictionary<Vector2, Tile> _tiles;
    List<Tile> _mainTiles, _endTiles, _specialTiles;
    List<List<Tile>> _subTiles;
    List<string> _specials;
    Tile[] _specialTilesOnPath;
    Coroutine _specialTileCoroutine;
    int _maxBranch = 1, _maxSpecialTiles = 5, _maxSpecialTilesOnPath = 3, _curSpecialTileOnPathIndex = 0;
    float _branchRate = 0f, _vertRate = 0f;
    BallManager _ballManager;
    UIManager _uiManager;
    AudioScript _audioScript;

    void Start() {
        _audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();
        _ballManager = GameObject.Find("/BallManager").GetComponent<BallManager>();
        _uiManager = GameObject.Find("/UIManager").GetComponent<UIManager>();
        _specialTilesOnPath = new Tile[_maxSpecialTilesOnPath];
        _specials = new List<string>{"swap", "destroy", "reverse", "speedup", "slowdown", "ghost"};

        SetupChars();
        GenerateGrid();
        StartCoroutine(GenerateMainTrail());
    }

    void GenerateGrid() {
        _tiles = new Dictionary<Vector2, Tile>();

        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var tile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                
                tile.name = $"Tile {x} {y}";
                tile.transform.parent = this.transform;
                tile.Init(false);
                tile.SetHost(_hosts[0]);
                _tiles[new Vector2(x, y)] = tile;
            }
        }

        _cam.transform.position = new Vector3((float) _width / 2 -.5f, (float)_height / 2 - 1.5f, -10);
    }

    IEnumerator GenerateMainTrail(){
        int prevHeightY = 0,
            quaterX = Mathf.RoundToInt(_width / 3),
            centerY = Mathf.RoundToInt(_height / 2);
        bool prevHasSub = false;
        List<Tile> subInitTiles = new List<Tile>();

        _mainTiles = new List<Tile>();
        _subTiles = new List<List<Tile>>();

        for(int x = 0; x < _width; x++){
            int dirY = 0, heightY = 0, initY;
            Tile prevTile = null;
            bool canHasVert = !prevHasSub && prevHeightY == 0; // can has vertical path / branch

            // later column follow previous vertical axis
            if (x > 0) { 
                prevTile = _mainTiles[_mainTiles.Count - 1];
                initY = (int) prevTile.transform.position.y; 
            }
            // first column use center vertical axis
            else { initY = centerY; }

            if (canHasVert && Random.value <= _vertRate && x > 0 && x < (_width - 1)) {
                if (initY > 0 && initY < _height) { 
                    int minY = (int) initY,
                        maxY = _height - minY;
                    
                    dirY = Random.value <= .5f ? -1 : 1; 
                    heightY = Random.Range(2, dirY < 0 ? minY : maxY);
                }
                else if (initY == _height) { 
                    dirY = -1; 
                    heightY = Random.Range(2, _height);
                }
                else if (initY == 0) { 
                    dirY = 1; 
                    heightY = Random.Range(2, _height);
                }
            }      

            // spawn vertical path
            if (heightY > 0 && dirY != 0) {
                heightY = Mathf.Min(heightY, 3); // cap vertical axis height 

                for (int y = 0; y < heightY; y++){
                    Vector2 tilePost = new Vector2(x, initY + (y * dirY));

                    yield return StartCoroutine(CreatePath(_mainTiles, tilePost, .05f));
                }

                prevHeightY = heightY;
            }
            else {
                yield return StartCoroutine(CreatePath(_mainTiles, new Vector2(x, initY), .05f));

                prevHeightY = 0;
            }

            // spawn branch tiles on center columns
            if (canHasVert && heightY <= 0 && Random.value <= _branchRate && x > 2 && x < (_width - 1)) {
                subInitTiles.Add(_mainTiles[_mainTiles.Count - 1]);
                prevHasSub = true;
            }
            else { prevHasSub = false; }
        }

        if (_mainTiles.Count > 0) {
            Tile firstTile = _mainTiles[0],
                 lastTile = _mainTiles[_mainTiles.Count - 1];

            _endTiles = new List<Tile>();
            firstTile.SetHost(_hosts.Find(h => h.name == "Start"));
            _endTiles.Add(firstTile);
            firstTile.SetEndTileIndex(_endTiles.IndexOf(firstTile));

            foreach(Tile sub in subInitTiles){
                yield return StartCoroutine(GenerateSubTrail(sub));
            }

            lastTile.SetHost(_hosts.Find(h => h.name == "End"));
            _endTiles.Add(lastTile);
            lastTile.SetEndTileIndex(_endTiles.IndexOf(lastTile));

            _specialTiles = new List<Tile>();
            _specialTileCoroutine = StartCoroutine(GenerateSpecialTiles(true));
        }
    }

    // todo: animate path spawning
    IEnumerator CreatePath(List<Tile> tiles, Vector2 post, float delay) {
        Tile tile = this.GetTileAtPosition(post);

        if (tile != null) { 
            tile.Init(true); 
            tiles.Add(tile);

            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator GenerateSubTrail(Tile initTile){
        Vector2 initPost = initTile.transform.position;
        int initY = (int) initPost.y,
            dirY = 0,
            heightY = 0; 
        List<Tile> tempTiles = new List<Tile>();

        if (initY > 0 && initY < _height) { 
            int minY = initY + 1,
                maxY = _height - initY;
        
            if (minY > maxY) { 
                dirY = -1;
                heightY = minY;
            }
            else if (minY < maxY) { 
                dirY = 1;
                heightY = maxY; 
            }
            else { 
                dirY = Random.value <= .5f ? -1 : 1; 
                heightY = dirY < 0 ? minY : maxY;
            }
        }
        else if (initY == _height) { 
            dirY = -1; 
            heightY = _height;
        }
        else if (initY == 0) { 
            dirY = 1; 
            heightY = _height;
        }

        if (_subTiles.Count < _maxBranch) {
            for (int y = 0; y < heightY; y++){
                Vector2 tilePost = new Vector2(initPost.x, initY + (y * dirY));

                yield return StartCoroutine(CreatePath(tempTiles, tilePost, .05f));
            }
            
            if (tempTiles.Count > 0) {
                Tile lastTile = tempTiles[tempTiles.Count - 1];

                tempTiles[0].SetBranchTile(true);
                lastTile.SetHost(_hosts.Find(h => h.name == "End"));
                _subTiles.Add(tempTiles);
                _endTiles.Add(lastTile);
                lastTile.SetEndTileIndex(_endTiles.IndexOf(lastTile));
            }
        }
    }

    IEnumerator GenerateSpecialTiles(bool init = false){
        yield return new WaitForSeconds(init ? 0 : Random.Range(0, 1));

        if (_specialTiles.Count < _maxSpecialTiles) {
            List<Tile> idleTiles = new List<Tile>(_tiles.Values);
            List<HostObj> specialHosts = _hosts.FindAll(h => _specials.IndexOf(h.name.ToLower()) != -1);

            idleTiles = idleTiles.FindAll(t => !t.IsPath() && _specials.IndexOf(t.GetHost().name.ToLower()) == -1);
            
            if (idleTiles.Count > 0) {
                Tile specialTile = idleTiles[Random.Range(0, idleTiles.Count)];

                specialTile.SetHost(specialHosts[Random.Range(0, specialHosts.Count)]);
                AddSpecialTile(specialTile);
            }
        }
        
        _specialTileCoroutine = StartCoroutine(GenerateSpecialTiles());
    }

    public void ResetPath(){
        if (_specialTileCoroutine != null) { StopCoroutine(_specialTileCoroutine); }

        _ballManager.RemoveAllBall();
        _uiManager.ResetHealth();

        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var tile = GetTileAtPosition(new Vector2(x, y));

                if (tile != null) { 
                    tile.Init(false);
                    tile.SetHost(_hosts[0]); 
                }
            }
        }

        StartCoroutine(GenerateMainTrail());
    }

    public Tile GetTileAtPosition(Vector2 pos) {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    public bool IsFirstTile(Tile tile) {
        return _mainTiles.IndexOf(tile) == 0;
    }

    public void AddSpecialTile(Tile tile) {
        if (_specialTiles.IndexOf(tile) == -1) { 
            if (tile.IsPath()) { 
                int index = _curSpecialTileOnPathIndex;
                Tile oldTile = _specialTilesOnPath[index];
                HostObj host = tile.GetHost();

                // remove old special tile on this index
                if (oldTile != null && _specialTiles.IndexOf(oldTile) != -1) { 
                    oldTile.SetHost(_hosts[0]);
                    _specialTiles.Remove(oldTile);
                }

                _specialTiles.Add(tile);
                _specialTilesOnPath[index] = tile;
                _uiManager.SetHost(index, host); 
                _uiManager.SetCurrentTileSprite(host.baseSprite);
                _uiManager.CheckCurSpecialTile();
            }
            else { _specialTiles.Add(tile);  }
        }
    }

    public void RemoveSpecialTile(Tile tile) {
        if (_specialTiles.IndexOf(tile) != -1) { 
            int index = Array.FindIndex(_specialTilesOnPath, s => s == tile);

            _specialTiles.Remove(tile); 

            if (tile.IsPath() && index != -1) { 
                HostObj host = _hosts[0];

                _uiManager.SetHost(index, host);
                _specialTilesOnPath[index] = null;

                if (index == _curSpecialTileOnPathIndex) { 
                    _uiManager.SetCurrentTileSprite(host.baseSprite);
                }

                _uiManager.CheckCurSpecialTile();
            }
        }
        
        tile.SetHost(_hosts[0]);
    }

    public void MoveCurSpecialTileOnPath(){
        int index = _curSpecialTileOnPathIndex;

        index = (index + 1) == _maxSpecialTilesOnPath ? 0 : index + 1;

        _curSpecialTileOnPathIndex = index;
        _uiManager.TriggerCurrentTile(index); 
    }

    public bool CanAddSpecialTileOnPath() {
        Tile[] temp = Array.FindAll(_specialTilesOnPath, s => s != null && _specials.IndexOf(s.GetHost().name.ToLower()) != -1);

        return temp.Length < _maxSpecialTilesOnPath;
    }

    public Tile GetCurSpecialTileOnPath(){
        return _specialTilesOnPath[_curSpecialTileOnPathIndex] ?? null;
    }

    public bool IsSpecialTile(Tile tile) {
        return _specialTiles.FindIndex(s => s == tile) != -1;
    }

    public bool IsSpecialTileOnPath(Tile tile) {
        return Array.FindIndex(_specialTilesOnPath, s => s == tile) != -1;
    }

    public List<Tile> GetBranchTiles(Tile tile){
        List<Tile> branchTiles = new List<Tile>();

        foreach(List<Tile> tiles in _subTiles) {
            int index = tiles.FindIndex(t => t == tile);

            if (index != -1) {
                branchTiles = new List<Tile>(tiles);
                break;
            }
        }

        return branchTiles;
    }

    public List<Tile> GetMainTiles() { return new List<Tile>(_mainTiles); }

    public List<Tile> GeSpecialTiles() { return new List<Tile>(_specialTiles); }

    public int GetNextEndTileIndex(Tile tile) {
        int index = _endTiles.IndexOf(tile);

        if ((index + 1) == _endTiles.Count) { return 0; }
        else { return index + 1; }
    }

    public int GetEndTileIndex(Tile tile) { return _endTiles.IndexOf(tile); }

    public void SwapBallHost(Ball ball){
        HostObj host;

        if (ball._hostName.Equals("Start")) { host = _hosts.Find(h => h.name.Equals("End")); }
        else if (ball._hostName.Equals("End")) { host = _hosts.Find(h => h.name.Equals("Start")); }
        else { host = null; }

        if (host != null) {
            ball.SetSprite(host.ballSprite);
            ball._hostName = host.name;
        }
    }

    public void GhostBall(Ball ball) {
        ball._isGhost = true;
        ball.SetSprite(_hosts[0].ballSprite);
    }

    public void PerformSpecialAction(Tile tile, Ball ball) {
        if (tile != null && ball != null && !ball._isGhost) {
            HostObj host = tile.GetHost();
            List<Tile> tiles;
            int index;

            _audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "ball_effected");

            switch(host.name.ToLower()){
                case "swap":
                    SwapBallHost(ball);
                    RemoveSpecialTile(tile);
                break;
                case "reverse":
                    tiles = new List<Tile>(ball._tiles);
                    
                    tiles.Reverse();
                    index = tiles.FindIndex(t => t == tile);
                    _ballManager.RedirectBall(ball, tiles, index);
                    RemoveSpecialTile(tile);
                break;
                case "speedup": 
                    ball._speed = 4;
                    RemoveSpecialTile(tile); 
                break;
                case "slowdown": 
                    ball._speed = 2; 
                    RemoveSpecialTile(tile);
                break;
                case "ghost": 
                    GhostBall(ball);
                    RemoveSpecialTile(tile); 
                break;
                case "destroy":
                    RemoveSpecialTile(tile);
                    ball.gameObject.SetActive(false);
                    _uiManager.UpdateTurn();
                break;
            }
        }
    }

    void SetupChars(){
        List<string> specials = new List<string>();
        int gridWidth = 0, gridHeight = 0;

        foreach(string side in new string[]{"start", "end"}) {
            string charObjName = PlayerPrefs.GetString(side + "_character");
            CharObj charObj = _characters.Find(c => c.name.Equals(charObjName));

            if (charObj != null) {
                _uiManager.SetChar(side, charObj);
                gridHeight += charObj.gridHeight;
                gridWidth += charObj.gridWidth;



                foreach(HostObj host in charObj.hosts) {
                    if (specials.IndexOf(host.name.ToLower()) == -1) {
                        specials.Add(host.name.ToLower());
                    }
                }
            }
        }

        _width = gridWidth;
        _height = gridHeight;
        _specials = specials;
    }
}