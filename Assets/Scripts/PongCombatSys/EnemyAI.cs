using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    BallManager _ballManager;
    GridManager _gridManager;
    UIManager _uiManager;
    Coroutine _simulateActionCoroutine;
    bool _exists = false;

    void Start() {
        _ballManager = GameObject.Find("/BallManager").GetComponent<BallManager>();
        _gridManager = GameObject.Find("/GridManager").GetComponent<GridManager>();
        _uiManager = GameObject.Find("/UIManager").GetComponent<UIManager>();
    }

    public void SetExists(bool status) {
        _exists = status;
    }

    public bool IsExists() { return _exists; }

    public void TriggerAction() {
        if (_simulateActionCoroutine == null) {
            _simulateActionCoroutine = StartCoroutine(SimulateAction());
        }
        else { _uiManager.UpdateTurn(); }
    }

    IEnumerator SimulateAction(){
        yield return new WaitForSeconds(Random.Range(1, 4));
        
        List<Tile> specialTiles = _gridManager.GeSpecialTiles(),
                   originTiles,
                   tiles;
        Ball ball = _ballManager.GetActiveBall();

        if (ball == null) {
            tiles = _gridManager.GetMainTiles();
            tiles.Reverse();
            _ballManager.SpawnBall(tiles);
        }
        else { 
            List<string> specials = new List<string>{"swap", "destroy", "reverse", "speedup", "slowdown", "ghost"};
            bool curEndIsMe = ball._endTile.Equals("End"),
                 curEndIsPlayer = ball._endTile.Equals("Start");
            Tile targetTile, originTile;
            
            tiles = ball._tiles;

            // ball host is blue - will hurt enemy n heal player - get rid of it!
            if (ball._hostName.Equals("Start") && (curEndIsMe || curEndIsPlayer)) {
                specials = new List<string>{"swap", "destroy", "ghost"};
            }
            // ball host is red - heal enemy n hurt player - do nothing / quicken or reverse the trip
            else if (ball._hostName.Equals("End")) {
                specials = new List<string>{"speedup", "reverse"};
            }
            
            // origin tile must not be from the path
            // target tile is a path and not a special tile
            originTiles = specialTiles.FindAll(s => specials.IndexOf(s.GetHost().name.ToLower()) != -1 && !_gridManager.IsSpecialTileOnPath(s) );
            tiles = tiles.FindAll(t => !_gridManager.IsSpecialTile(t) && !t.IsEndPoint() && tiles.IndexOf(t) > ball._curIndex);

            if (tiles.Count > 0 && originTiles.Count > 0) {
                originTile = originTiles[Random.Range(0, originTiles.Count)];
                targetTile = tiles[Random.Range(0, tiles.Count)];

                targetTile.SetHost(originTile.GetHost());
                _gridManager.AddSpecialTile(targetTile);
                _gridManager.RemoveSpecialTile(originTile);
                _gridManager.MoveCurSpecialTileOnPath();
            }
        }

        _uiManager.UpdateTurn();
        _simulateActionCoroutine = null;
    }
}