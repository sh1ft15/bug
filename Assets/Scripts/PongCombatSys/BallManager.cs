using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    [SerializeField] Ball _ballPrefab;
    List<Ball> _balls;
    float _spawnBallDelay = 0;
    UIManager _uiManager;
    GridManager _gridManager;

    void Start() {
        _gridManager = GameObject.Find("/GridManager").GetComponentInParent<GridManager>();
        _uiManager = GameObject.Find("/UIManager").GetComponent<UIManager>();
        _balls = new List<Ball>();
    }

    void Update(){
        if (_spawnBallDelay > 0) { _spawnBallDelay -= Time.deltaTime; }

        if (_balls.Count > 0) {
            for(int i = 0; i < _balls.Count; i++){
                Ball ball = _balls[i];

                if (ball != null && !ball._reachedEndPoint) {
                    List<Tile> path = ball._tiles;
                    Vector2 curPost = ball.transform.position;

                    if (path != null && path.Count > 0) {
                        Vector2 nextPost = path[ball._curIndex].transform.position,
                                endPost = path[path.Count - 1].transform.position;

                        if (Vector2.Distance(curPost, endPost) <= .1f) {
                            if (!ball._reachedEndPoint) { ball._reachedEndPoint = true; }
                        }
                        else if (Vector2.Distance(curPost, nextPost) <= .1f) {
                            ball._curIndex += 1;
                        }
                        else {
                            Vector2 post = Vector2.MoveTowards(curPost, nextPost, Time.deltaTime * ball._speed);

                            ball.transform.position = post;
                        }
                    }
                }
            }
        }
    }

    public void SpawnBall(List<Tile> tiles){
        if (_spawnBallDelay <= 0) {
            Vector2 startPost = tiles[0].transform.position;
            HostObj host = tiles[0].GetHost();
            Ball ball = null;
            int ballIndex = -1;

            // ensure it's a copy and not the original instance
            tiles = new List<Tile>(tiles);

            if (_balls.Count > 0) {
                ballIndex = _balls.FindIndex(b => !b.gameObject.activeSelf);

                if (ballIndex != -1) { 
                    ball = _balls[ballIndex]; 
                    ball.transform.position = startPost;
                    ball.gameObject.SetActive(true);     
                }
            }

            if (ball == null) { 
                ball = Instantiate(_ballPrefab, startPost, Quaternion.identity);
                ball.transform.parent = this.transform; 
            }

            ball.name = $"Ball {startPost.x} {startPost.y}";
            ball._startTile = tiles[0];
            ball._endTile = tiles[tiles.Count - 1];
            ball._tiles = tiles;
            ball._curIndex = 0;
            ball._speed = 2;
            ball._reachedEndPoint = false;
            ball._hostName = host.name;
            ball._isGhost = false;
            ball.SetSprite(host.ballSprite);
            ball._curEndTileIndex = _gridManager.GetNextEndTileIndex(tiles[0]);
        
            if (ballIndex != -1) { _balls[ballIndex] = ball; }
            else { _balls.Add(ball); }

            _spawnBallDelay = 1;
        }
    }

    public IEnumerator SpawnBallWithDelay(List<Tile> tiles, float delay) {
        yield return new WaitForSeconds(delay);
        SpawnBall(tiles);
    }

    public void RedirectBall(Ball ball, List<Tile> tiles, int offset = 0) {
        int ballIndex = _balls.FindIndex(b => b == ball),
            endPointIndex = tiles.FindIndex(t => t.IsEndPoint());

        // ensure it's a copy and not the original instance
        tiles = new List<Tile>(tiles);
        
        if (ballIndex != -1 && endPointIndex != -1) {
            Vector2 startPost = tiles[offset].transform.position;
            HostObj host = tiles[0].GetHost();

            ball.name = $"Ball {startPost.x} {startPost.y}";
            ball._startTile = tiles[0];
            ball._endTile = tiles[tiles.Count - 1];
            ball._tiles = tiles;
            ball._curIndex = offset;
            ball.transform.position = startPost;
            ball._reachedEndPoint = false;
            
            _balls[ballIndex] = ball;
        }
    }

    public void CounterBall(Ball ball){
        List<Tile> tiles = new List<Tile>(ball._tiles);
        Tile endPoint = tiles[tiles.Count - 1];
        HostObj host = endPoint.GetHost();
        string ballHostName = ball._hostName;

        if (!ball._isGhost) {
            switch(host.name) {
                case "Start": 
                    _uiManager.UpdateHealth(ballHostName.Equals(host.name) ? 10 : -10);
                break;
                case "End": 
                    _uiManager.UpdateHealth(ballHostName.Equals(host.name) ? -10 : 10);
                break;
            }
        }

        tiles.Reverse();
        ball.SetSprite(host.ballSprite);
        ball._hostName = host.name;
        ball._isGhost = false;
        ball._curEndTileIndex = _gridManager.GetNextEndTileIndex(endPoint);
        RedirectBall(ball, tiles);
    }

    public void RemoveAllBall(){
        foreach(Ball ball in _balls) { ball.gameObject.SetActive(false); }
    }

    public bool HasBall(){
        return _balls.FindIndex(b => b.gameObject.activeSelf) != -1;
    }

    public Ball GetActiveBall() { return _balls.Find(b => b.gameObject.activeSelf); }
}
