using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watcher : MonoBehaviour
{
    private GameObject _eyeBall, _player;
    public int speed = 1;

    void Start()
    {
        _eyeBall = gameObject;
        _player = GameObject.FindWithTag("Player");
        
    }
    
    void FixedUpdate()
    {
        Vector3 direction = _eyeBall.transform.position - new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z - 4.5f);
        Quaternion ToRotation = Quaternion.LookRotation(direction * -1, Vector3.up);
        _eyeBall.transform.rotation = Quaternion.Lerp(_eyeBall.transform.rotation, ToRotation, speed * Time.deltaTime);

    }
}
