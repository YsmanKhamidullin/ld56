using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicDontDestroy : MonoBehaviour
{
    public static MusicDontDestroy Instance => _instance;
    private static MusicDontDestroy _instance;

    [SerializeField]
    private List<AudioSource> _bgMusics;

    private int _curPlayingNumber;

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
        if (track == _curPlayingNumber)
        {
            return;
        }

        _curPlayingNumber = track;
        foreach (var audioSource in _bgMusics)
        {
            audioSource.Stop();
        }

        track--;
        if (track == -1)
        {
            return;
        }

        _bgMusics[track].Play();
    }
}