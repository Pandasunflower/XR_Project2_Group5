using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class LyricLine
{
    public float time;
    public string text;
}

public class LyricSystem : MonoBehaviour
{
    [Header("Song Settings")]
    public string songEventName;

    [Header("Lyrics lrc txt")]
    public TextAsset lrcFile;

    [Header("Lyrics UI")]
    public TextMeshProUGUI lyricUI;
    public TextMeshPro lyricUI2;

    private List<LyricLine> lyrics = new List<LyricLine>();
    private int index = 0;
    private uint playingId;
    private bool isPlaying = false;
    private bool syncReady = false;
    private float playStartTime;

    void Start()
    {
        lyrics = ParseLRC(lrcFile.text);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Play();
        }

        if (!isPlaying || !syncReady) return;

        float time = GetTime();

        if (index < lyrics.Count - 1)
        {
            if (time >= lyrics[index].time && time < lyrics[index + 1].time)
            {
                if (!lyricUI) lyricUI2.text = lyrics[index].text;
                else lyricUI.text = lyrics[index].text;
            }
            else if (time >= lyrics[index + 1].time)
            {
                index++;
                if (!lyricUI) lyricUI2.text = lyrics[index].text;
                else lyricUI.text = lyrics[index].text;
            }
        }
    }

    public void Play()
    {
        // playingId = AkUnitySoundEngine.PostEvent(songEventName, gameObject);

        index = 0;
        // if (!lyricUI) lyricUI2.text = lyrics.Count > 0 ? lyrics[0].text : "";
        // else lyricUI.text = lyrics.Count > 0 ? lyrics[0].text : "";

        playStartTime = Time.realtimeSinceStartup;
        isPlaying = true;
        syncReady = false;

        Invoke(nameof(EnableSync), 0.1f);
    }

    void EnableSync()
    {
        syncReady = true;
    }

    float GetTime()
    {
        return Time.realtimeSinceStartup - playStartTime;
    }

    List<LyricLine> ParseLRC(string lrc)
    {
        var list = new List<LyricLine>();

        var lines = lrc.Split('\n');

        foreach (var line in lines)
        {
            if (!line.Contains("[")) continue;

            int s = line.IndexOf("[") + 1;
            int e = line.IndexOf("]");

            if (s < 0 || e < 0) continue;

            string timeStr = line.Substring(s, e - s);
            string text = line.Substring(e + 1).Trim();

            list.Add(new LyricLine
            {
                time = ParseTime(timeStr),
                text = text
            });
        }

        return list;
    }

    float ParseTime(string t)
    {
        var sp = t.Split(':');

        float min = float.Parse(sp[0]);
        float sec = float.Parse(sp[1]);
        return min * 60f + sec;
    }
}