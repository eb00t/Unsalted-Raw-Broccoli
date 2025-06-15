using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BlackoutManager : MonoBehaviour
{
    public static BlackoutManager Instance { get; private set; }
    
    public bool blackoutComplete;
    private float _lerpTime;
    private bool _fadedOut;
    private float _failSafeTimer = 15;
    private bool _loading = true;
    private float _timer = 2;

    [SerializeField] private CanvasGroup canvasGroup;
    private Tween _fadeTween;

    private void Start()
    {
        if (LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.FinalBoss)
        {
            _failSafeTimer = 6;
        }
        else
        {
            _failSafeTimer = 10;
        }
    }

    void Awake()
    {
        gameObject.SetActive(true);

        if (Instance != null)
        {
            Debug.LogError("More than one Blackout Manager script in the scene.");
        }

        Instance = this;
    }

    public void LowerOpacity()
    {
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();
        _loading = false;
        canvasGroup.gameObject.SetActive(true);
        _fadeTween = canvasGroup.DOFade(0f, 2f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
            blackoutComplete = true;
        });
    }

    public void RaiseOpacity()
    {
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();
        blackoutComplete = false;
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);
        _fadeTween = canvasGroup.DOFade(1f, 2f);
    }

    private void Update()
    {
        if (_loading)
        {
            _failSafeTimer -= Time.deltaTime;
            if (_failSafeTimer <= 0)
            {
                Debug.LogError("Failsafe timer has expired, reloading scene to fix errors.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }

        if (!_fadedOut)
        {
            if (LevelBuilder.Instance.bossRoomGeneratingFinished &&
                LevelBuilder.Instance.currentFloor is not (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen or LevelBuilder.LevelMode.EndScreen))
            {
                _fadedOut = true;
                ResizeGraph(FindRoomBounds());
                AstarPath.active.Scan();
                LowerOpacity();
            }
            else if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen or LevelBuilder.LevelMode.EndScreen))
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _fadedOut = true;
                    LowerOpacity();
                }
            }
        }
    }
    
    public void ResizeGraph(Bounds bounds)
    {
        var gg = AstarPath.active.data.gridGraph;
        gg.center = bounds.center;
        gg.SetDimensions(Mathf.CeilToInt(bounds.size.x / gg.nodeSize), Mathf.CeilToInt(bounds.size.y / gg.nodeSize), gg.nodeSize);
        AstarPath.active.Scan();
    }
    
    private Bounds FindRoomBounds()
    {
        var allRooms = FindObjectsOfType<GraphUpdateScene>();
    
        if (allRooms.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one);
        }

        var totalBounds = new Bounds(allRooms[0].transform.position, Vector3.zero);

        foreach (var room in allRooms)
        {
            var colliders = room.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                {
                    totalBounds.Encapsulate(col.bounds);
                }
            }
            else
            {
                var renderers = room.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    totalBounds.Encapsulate(r.bounds);
                }
            }
        }

        return totalBounds;
    }
}
    


