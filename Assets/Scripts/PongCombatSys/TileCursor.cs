using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCursor : MonoBehaviour
{
    [SerializeField] Transform _targetTileCursor;
    [SerializeField] SpriteRenderer _renderer;
    GridManager _gridManager;
    Tile _targetTile, _originTile;
    bool _onTarget;

    void Start(){
        _gridManager = GameObject.Find("/GridManager").GetComponent<GridManager>();
    }

    void Update(){
        if (!_onTarget && _targetTileCursor.gameObject.activeSelf) {
            _targetTileCursor.transform.position = transform.position;
        }
    }

    void OnTriggerStay2D(Collider2D other){
        if (!_onTarget) {
            switch(other.transform.tag) {
                case "Tile":
                    Tile tile = other.GetComponent<Tile>();

                    if (tile != null && tile.IsPath() && !tile.IsEndPoint()) {
                        SetTargetTile(other.transform);
                    }
                break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (_onTarget) { RemoveTargetTile(); }
    }

    public void SetOriginTile(Tile tile) {
        HostObj host = tile.GetHost();

        _originTile = tile;
        _renderer.sprite = host.baseSprite;
        _targetTileCursor.Find("Sprite").GetComponent<SpriteRenderer>().sprite = host.baseSprite;
    }

    public void SetTargetTile(Transform target) {
        _onTarget = true;
        _targetTile = target.GetComponent<Tile>();
        _targetTileCursor.transform.position = target.position;
    }

    public void RemoveTargetTile() { _onTarget = false; }

    public Tile GetTargetTile() { return _targetTile; }

    public Tile GetOriginTile() { return _originTile; }

    public void ToggleCursor(bool status) {
        gameObject.SetActive(status);
        _targetTileCursor.gameObject.SetActive(status);

        if (status == false) { _targetTile = _originTile = null; }
    }

    public bool OnTarget() { return _onTarget; }
}
