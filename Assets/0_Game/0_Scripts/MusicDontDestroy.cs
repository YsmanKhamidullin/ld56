using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicDontDestroy : MonoBehaviour
{
    public static MusicDontDestroy Instance => _instance;
    private static MusicDontDestroy _instance;
    [SerializeField]
    private List<AudioSource> _bgMusics;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ChangeTrack(int track)
    {
        foreach (var audioSource in _bgMusics)
        {
            audioSource.Stop();
        }

        _bgMusics[track].Play();
    }
}