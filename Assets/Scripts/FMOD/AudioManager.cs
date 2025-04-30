using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    //Ideally there should be one of these for each mixer, but there is only one right now.
    [Header("Volume")] 
    [Range(0, 1)] 
    public float masterVolume = 1;
    [Range(0, 1)] 
    public float ambienceVolume = 1;
    [Range(0, 1)] 
    public float musicVolume = 1;
    [Range(0, 1)] 
    public float sfxVolume = 1;
    [Range(0, 1)] 
    public float uiSfxVolume = 1;
    [Range(0, 1)]
    public int pause;
    [Range(0, 1)] 
    public int playerIsDead;

    private Bus _masterBus; //There should be one of these for each mixer.
    private Bus _ambienceBus;
    private Bus _musicBus;
    private Bus _sfxBus;
    private Bus _uiSfxBus;
    
    private List<EventInstance> _eventInstances;
    private List<StudioEventEmitter> _eventEmitters;
    private EventInstance _ambienceEventInstance;
    public EventInstance loadingEventInstance;
    public EventInstance MusicEventInstance;
    public int nowPlaying = 0;
    public bool playLoadingSounds = true;
    public bool playAmbienceSounds = true;
    //Add any more sort of background-ish sound effects here if necessary.

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one AudioManager in the scene.");
        }
        Instance = this;

        _eventInstances = new List<EventInstance>();
        _eventEmitters = new List<StudioEventEmitter>();

        _masterBus = RuntimeManager.GetBus("bus:/");
        _ambienceBus = RuntimeManager.GetBus("bus:/Not Music or UI/Ambience");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _sfxBus = RuntimeManager.GetBus("bus:/Not Music or UI/SFX");
        _uiSfxBus = RuntimeManager.GetBus("bus:/UI SFX");

        playerIsDead = 0;
    }

    private void Start()
    {
       loadingEventInstance = CreateEventInstance(FMODEvents.Instance.Loading);
        if (playLoadingSounds)
        {
            loadingEventInstance.start();
        }

        if (playAmbienceSounds)
        {
            InitialiseAmbience(FMODEvents.Instance.Ambience);
        }

        InitialiseMusic(FMODEvents.Instance.Music);
        SetGlobalEventParameter("NoMusicUIVolume", 1);
    }

    private void Update()
    {
        _masterBus.setVolume(masterVolume);
        _ambienceBus.setVolume(ambienceVolume);
        _musicBus.setVolume(musicVolume);
        _sfxBus.setVolume(sfxVolume);
        _uiSfxBus.setVolume(uiSfxVolume);
        // RuntimeManager.StudioSystem.setParameterByName("PauseFilter", pause);
       // RuntimeManager.StudioSystem.setParameterByName("PlayerIsDead", playerIsDead);
    }

    //GENERAL FUNCTIONS
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        //This is for playing any sound once, worldPos will usually be transform.position.
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        //This is used in the event a sound needs to be stopped or started at anytime other than the beginning.
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public void AttachInstanceToGameObject(EventInstance eventInstance, GameObject gameObject)
    {
        RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject);
    }

    public StudioEventEmitter InitialiseEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        //This is for starting and stopping sounds that play constantly, such as humming around a pickup.
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
       /* emitter.OverrideAttenuation = overrideAttenuation;
        if (overrideAttenuation)
        {
            emitter.OverrideMaxDistance = maxDist;
        }*/
        _eventEmitters.Add(emitter);
        return emitter;
    }

    public void SetEmitterParameter(StudioEventEmitter studioEventEmitter, string name, float value)
    {
        studioEventEmitter.SetParameter(name, value);
    }
    
    public void SetEventParameter(EventInstance eventInstance, string parameterName, float parameterValue)
    {
        eventInstance.setParameterByName(parameterName, parameterValue);
    }
    
    public void SetGlobalEventParameter(string parameterName, float parameterValue)
    {
        RuntimeManager.StudioSystem.setParameterByName(parameterName, parameterValue);
    }
    
    //AMBIENCE FUNCTIONS
    private void InitialiseAmbience(EventReference ambienceEventReference)
    {
        _ambienceEventInstance = CreateEventInstance(ambienceEventReference);
        _ambienceEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        /*This is used to change anything related to ambience, e.g. changing any effects,
        or changing the volume or frequency of certain sounds in the ambient track.
        These are set up inside FMOD.*/
        _ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }
    
    public void SetAmbienceArea(AmbienceArea area)
    {
        //Used to change the ambience based on the player's location. Use a trigger or something to call it.
        _ambienceEventInstance.setParameterByName("area", (float)area);
    }
    
    //MUSIC FUNCTIONS
    public void InitialiseMusic(EventReference musicEventReference)
    {
        MusicEventInstance = CreateEventInstance(musicEventReference);
        if (LevelBuilder.Instance != null)
        {
            switch (LevelBuilder.Instance.currentFloor)
            {
                case LevelBuilder.LevelMode.Intermission or LevelBuilder.LevelMode.Tutorial:
                    SetGlobalEventParameter("Music Track", 5);
                    break;
                default:
                    SetGlobalEventParameter("Music Track", 0);
                    break;
            }
        }
        else if (LevelBuilder.Instance == null && SceneManager.GetActiveScene().name == "StartScreen")
        {
            SetGlobalEventParameter("Music Track", 8);
        }
        else
        {
            SetGlobalEventParameter("Music Track", 6);
        }
        MusicEventInstance.start();
    }
    public void SetMusicParameter(string parameterName, float parameterValue)
    {
        /*This is used to change anything related to ambience, e.g. changing any effects,
        or changing the volume or frequency of certain sounds in the ambient track.
        These are set up inside FMOD.*/
        MusicEventInstance.setParameterByName(parameterName, parameterValue);
        nowPlaying = (int)parameterValue;
       //Debug.Log("Now playing: " + parameterName + " " + parameterValue);
    }
    public void SetMusicArea(MusicArea area)
    {
       //Same story for changing music as changing ambience.
        MusicEventInstance.setParameterByName("Track", (float)area);
        nowPlaying = (int)area;
    }

    public void Pause() //Two functions to prevent any issues
    {
        pause = 1;
    } 
    public void Unpause()
    {
        pause = 0;
    }
    
    public void PlayerDead() //Two functions to prevent any issues
    {
        playerIsDead = 1;
    } 
    public void PlayerNotDead()
    {
        playerIsDead = 0;
    }
    
    //DESTROY FUNCTIONS
    private void CleanUp()
    {
        foreach (EventInstance eI in _eventInstances)
        {
            eI.stop(STOP_MODE.IMMEDIATE);
            eI.release();
        }

        foreach (StudioEventEmitter sEE in _eventEmitters)
        {
            sEE.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
