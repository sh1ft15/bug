using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.Experimental.Rendering.Universal;

public class Tile : MonoBehaviour {
    [SerializeField] Color _baseColor, _offsetColor;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] GameObject _highlight;
    [SerializeField] Text _numText;
    HostObj _hostObj;
    GridManager _gridManager;
    BallManager _ballManager;
    UIManager _uiManager;
    Ball _ballEntered;
    bool _isBranchTile, _isPath, _ballTriggered;

    void Start() { 
        _gridManager = transform.GetComponentInParent<GridManager>();
        _ballManager = GameObject.Find("/BallManager").GetComponent<BallManager>();
        _uiManager = GameObject.Find("/UIManager").GetComponent<UIManager>();
    }

    public void Init(bool isOffset) {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
        _isPath = isOffset;

        // disable num label if not a path
        if (!_isPath && _numText.gameObject.activeSelf) { _numText.gameObject.SetActive(false); }
    }

    void OnMouseEnter() {
        if (!_uiManager.OnMouseHold()) {
            if (!_isPath) { _renderer.color = _offsetColor; }

            _renderer.transform.localScale = Vector2.one * 1.2f;
            _highlight.SetActive(true);
        }
    }

    void OnMouseOver() {
        if (_uiManager.OnMouseHold()) { OnMouseExit(); }
        else { OnMouseEnter(); }
    }

    void OnMouseExit() {
        if (!_isPath) { _renderer.color = _baseColor; }

        if (_renderer.transform.localScale.x != 1) { 
            _renderer.transform.localScale = Vector2.one;
        }
        
        if (_highlight.activeSelf) { _highlight.SetActive(false); }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!_ballEntered && other.tag.Equals("Ball")) { _ballEntered = other.GetComponent<Ball>(); }
    }

    void OnTriggerStay2D(Collider2D other) {
        float dist = Vector2.Distance(other.transform.position, transform.position);

        if (!_highlight.activeSelf) { _renderer.sprite = _hostObj.offsetSprite; }

        if (!_ballTriggered && other.tag.Equals("Ball") && dist <= .3f) {
            Ball ball = other.GetComponent<Ball>();
            bool reachedEndPoint = ball._reachedEndPoint;

            if (_gridManager.IsSpecialTile(this)) {
                _gridManager.PerformSpecialAction(this, ball);
            }
            else if (_isBranchTile) {
                int ballEndTileIndex = ball._curEndTileIndex;

                // transition from branch to main
                if (reachedEndPoint) {
                    List<Tile> mainTiles = new List<Tile>(_gridManager.GetMainTiles());
                    int branchIndex = mainTiles.FindIndex(t => t == this);

                    if (branchIndex != -1) {
                        if (_gridManager.GetEndTileIndex(mainTiles[0]) == ballEndTileIndex) { mainTiles.Reverse(); }
                        
                        branchIndex = mainTiles.FindIndex(t => t == this);

                        _ballManager.RedirectBall(ball, mainTiles, branchIndex);
                    }
                }
                // transition from main to branch
                else { 
                    List<Tile> tiles = _gridManager.GetBranchTiles(this);

                    if (_gridManager.GetEndTileIndex(tiles[tiles.Count - 1]) == ballEndTileIndex) { 
                        _ballManager.RedirectBall(ball, tiles); 
                    }
                }

                _ballTriggered = true;
            }
            else if (reachedEndPoint) {
                if (IsEndPoint()) { 
                    _ballManager.CounterBall(ball); 
                    _ballTriggered = true;
                }
                else { ball.gameObject.SetActive(false); }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        _renderer.sprite = _hostObj.baseSprite;
        _ballTriggered = false;

        if (_ballEntered && other.tag.Equals("Ball")) { _ballEntered = null; }
    }

    public void SetBranchTile(bool status) {
        _isBranchTile = status;
    }

    public void SetHost(HostObj hostObj){
        _hostObj = hostObj;
        _renderer.sprite = _hostObj.baseSprite;
    }

    public HostObj GetHost() { return _hostObj; }

    public bool IsEndPoint(){
        return _gridManager.GetEndTileIndex(this) != -1;
    }

    public bool IsPath() { return _isPath; }

    public void SetEndTileIndex(int num){
        if (!_numText.gameObject.activeSelf) { _numText.gameObject.SetActive(true); }

        _numText.text = num.ToString("0");
    }

    public Ball GetBallEntered() { return _ballEntered; }
}