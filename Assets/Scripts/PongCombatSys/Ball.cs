using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] SpriteRenderer _renderer;
    public Tile _startTile, _endTile;
    public List<Tile> _tiles;
    public int _curIndex, _curEndTileIndex;
    public float _speed;
    public bool _reachedEndPoint, _isGhost;
    public string _hostName;

    public void SetSprite(Sprite sprite) {
        _renderer.sprite = sprite;
    }

    public Sprite GetSprite() { return _renderer.sprite; }
}
