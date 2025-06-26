using DG.Tweening;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackoutManager : MonoBehaviour
{
    public static BlackoutManager Instance { get; private set; }
    
    public bool blackoutComplete;
    private float _lerpTime;
    private bool _fadedOut;
    public float failSafeTimer;
    private bool _loading = true;
    private float _timer = 2;

    [SerializeField] private CanvasGroup canvasGroup, loadingGroup;
    private Tween _fadeTween;
    private CanvasGroup _hudCanvasGroup;

    private void Start()
    {
        failSafeTimer = 5;
        _hudCanvasGroup = MenuHandler.Instance.GetComponentInParent<CanvasGroup>();
    }

    private void Awake()
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
        loadingGroup.gameObject.SetActive(false);
        
        _fadeTween = canvasGroup.DOFade(0f, 2f).SetUpdate(true).OnComplete(() =>
        {
            _hudCanvasGroup.DOFade(1f, 0.1f).SetUpdate(true);
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
        _hudCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        _fadeTween = canvasGroup.DOFade(1f, 2f).SetUpdate(true);
    }

    private void Update()
    {
        if (_loading)
        {
            failSafeTimer -= Time.deltaTime;
            if (failSafeTimer <= 0)
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
            
            Debug.Log("Time until reset: " + failSafeTimer);
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
    


