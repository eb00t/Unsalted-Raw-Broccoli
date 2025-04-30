using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BlackoutManager : MonoBehaviour
{
    public static BlackoutManager Instance { get; private set; }

    public Image blackoutImage;
    public Image vignetteImage;
    public Image loadBck;
    public Color blackoutColor;
    public Color vignetteColor;
    public Color transparentColor;
    public bool blackoutComplete;
    private float _lerpTime;
    private bool _fadedOut;
    private float _failSafeTimer = 15;
    private bool _loading = true;
    private float _timer = 2;

    private void Start()
    {
        if (LevelBuilder.Instance.currentFloor == LevelBuilder.LevelMode.FinalBoss)
        {
            _failSafeTimer = 6;
        }
        else
        {
            _failSafeTimer = ;
        }
    }

    private enum LerpDirection
    {
        Neither,
        FadeOut,
        FadeIn,
    }

    private LerpDirection _lerpDirection;

    void Awake()
    {
        _lerpDirection = LerpDirection.Neither;
        transparentColor.a = 0;
        if (blackoutImage == null)
        {
            blackoutImage = transform.Find("Blackout").gameObject.GetComponent<Image>();
        } 
        if (vignetteImage == null)
        {
            vignetteImage = transform.Find("vignette").gameObject.GetComponent<Image>();
        }

        gameObject.SetActive(true);

        if (Instance != null)
        {
            Debug.LogError("More than one Blackout Manager script in the scene.");
        }

        Instance = this;
    }

    public void LowerOpacity()
    {
        _loading = false;
        blackoutImage.gameObject.SetActive(true);
        vignetteImage.gameObject.SetActive(true);
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeOut;
        _timer = 2;
    }

    public void RaiseOpacity()
    {
        blackoutComplete = false;
        blackoutImage.gameObject.SetActive(true);
        vignetteImage.gameObject.SetActive(true);
        _lerpTime = 0;
        _lerpDirection = LerpDirection.FadeIn;
    }

    private void Update()
    {
        if (LevelBuilder.Instance.bossRoomGeneratingFinished && _fadedOut == false && LevelBuilder.Instance.currentFloor is not (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen))
        {
            _fadedOut = true;
            ResizeGraph(FindRoomBounds());
            AstarPath.active.Scan();
            LowerOpacity();
        } 
        else if (LevelBuilder.Instance.currentFloor is (LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial or LevelBuilder.LevelMode.TitleScreen) && _fadedOut == false)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _fadedOut = true;
                LowerOpacity();
            }
        }

        switch (_lerpDirection)
        {
            case LerpDirection.Neither:
                blackoutImage.color = blackoutColor;
                break;
            case LerpDirection.FadeOut:
                blackoutImage.color = Color.Lerp(blackoutColor, transparentColor, _lerpTime);
                vignetteImage.color = Color.Lerp(vignetteColor, transparentColor, _lerpTime);
                loadBck.gameObject.SetActive(false);
                
                if (blackoutImage.color.a <= 0)
                {
                    blackoutImage.gameObject.SetActive(false);
                }
                if (vignetteImage.color.a <= 0)
                {
                    vignetteImage.gameObject.SetActive(false);
                }

                break;
            case LerpDirection.FadeIn:
                blackoutImage.color = Color.Lerp(transparentColor, blackoutColor, _lerpTime);
                vignetteImage.color = Color.Lerp(transparentColor, vignetteColor, _lerpTime);
                break;
        }

        _lerpTime += .01f;
        
        if (_loading)
        {
            //Debug.Log(_failSafeTimer);
            _failSafeTimer -= Time.deltaTime;
            if (_failSafeTimer <= 0)
            {
                Debug.LogError("Failsafe timer has expired, reloading scene to fix errors.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
        if (blackoutImage.color.a <= 0)
        {
            blackoutComplete = true;
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
    


