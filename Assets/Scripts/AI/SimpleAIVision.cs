using System;
using UnityEngine;

public class SimpleAIVision : MonoBehaviour, IVision
{
    public float viewDistance; //view distance for spotting player
    public float viewConeAngle; //view cone angle for spotting player
    public string playerTag;

    public float visionCheckDelta = 0.25f;
    private float _lastVisionCheck;

    private bool _canSeePlayer;
    
    private Player[] _players;
    private GameObject[] _visiblePlayers;

    private void Start()
    {
        _lastVisionCheck = 0;
        // TODO: Make player manager so that can hold an updated list of players
        var playersObjects = GameObject.FindGameObjectsWithTag(playerTag);
        
        _players = new Player[playersObjects.Length];

        var i = 0;
        foreach (var obj in playersObjects)
        {
            var player = obj.GetComponent<Player>();

            if (player == null) continue;
            _players[i] = player;
            i++;
        }
        
        Array.Resize(ref _players, i+1);

        _visiblePlayers = null;
    }

    private void Update()
    {
        if (Time.time - _lastVisionCheck < visionCheckDelta) return;

        _lastVisionCheck = Time.time;
        
        if (_players.Length < 1) 
        {
            _canSeePlayer = false;
            _visiblePlayers = null;
            return;
        }
        
        // Filter Invisible Players
        var findablePlayers = Array.FindAll(_players, player => player != null && !player.Invisible);

        if (findablePlayers.Length < 1)
        {
            _canSeePlayer = false;
            _visiblePlayers = null;
            return;
        }
        
        // Check if Players are in View Distance
        var playersInViewDistance = Array.FindAll(findablePlayers,
            p => Vector3.Distance(transform.position, p.transform.position) <= viewDistance);
        
        // If there are no players in View Distance, can't see anyone
        if (playersInViewDistance.Length < 1)
        {
            _canSeePlayer = false;
            _visiblePlayers = null;
            return;
        }

        // Check if Players are in View Angle
        var playersInSightLine = Array.FindAll(playersInViewDistance, player =>
        {
            var aiTransform = transform;
            var directionToPlayer = (player.transform.position - aiTransform.position).normalized;
            var angleBetween = Mathf.Abs(Vector3.Angle(directionToPlayer, aiTransform.forward));
            return angleBetween <= viewConeAngle / 2;
        });

        // If no player in sight line, can't see anyone
        if (playersInSightLine.Length < 1)
        {
            _canSeePlayer = false;
            _visiblePlayers = null;
            return;
        }

        // Check if player is not behind wall
        var visiblePlayers = Array.FindAll(playersInSightLine, player =>
        {
            var aiPosition = transform.position;
            var directionToPlayer = (player.transform.position - aiPosition).normalized;

            return Physics.Raycast(aiPosition, directionToPlayer, out var hit) &&
                   hit.collider.gameObject.CompareTag(playerTag);
        });

        if (visiblePlayers.Length < 1)
        {
            _canSeePlayer = false;
            _visiblePlayers = null;
        }
        else
        {
            _visiblePlayers = new GameObject[visiblePlayers.Length];
            for (var i = 0; i < visiblePlayers.Length; i++)
            {
                _visiblePlayers[i] = visiblePlayers[i].gameObject;
            }

            // visible players by distance
            Array.Sort(_visiblePlayers, (a, b) =>
            {
                var position = transform.position;
                return (int) (Vector3.Distance(position, b.transform.position) -
                              Vector3.Distance(position, a.transform.position));
            });
            
            _canSeePlayer = true;
        }
    }

    public bool CanSeeObject()
    {
        return _canSeePlayer;
    }

    public GameObject[] GetVisibleObjects()
    {
        return _visiblePlayers;
    }
}