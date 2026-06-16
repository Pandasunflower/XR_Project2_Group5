using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class LyricSegment
{
    public float startTime;
    public float endTime;
    public string text;
}

public class LyricsGenerator : MonoBehaviour
{
    [Header("Song Settings")]
    public string songName;
    public string songEventName;

    [Header("Debug UI")]
    // public TextMeshProUGUI lyricDisplay;
    public TextMeshProUGUI debugUI;

    [Header("Lyrics txt")]
    public TextAsset lyricsFile;

    private uint playingId;
    private List<string> lyrics = new List<string>();
    private List<LyricSegment> segments = new List<LyricSegment>();
    private int currentIndex = -1;
    private bool isPlaying = false;
    private float playStartTime;

    void Start()
    {
        LoadLyrics();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Play();
        }

        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextLyric();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndSong();
        }
    }

    void LoadLyrics()
    {
        lyrics.Clear();

        string[] lines = lyricsFile.text.Split('\n');

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                lyrics.Add(line.Trim());
        }

        debugUI.text = $"Loaded {lyrics.Count} lines";
    }

    void Play()
    {
        playingId = AkUnitySoundEngine.PostEvent(songEventName, gameObject);

        segments.Clear();
        currentIndex = -1;

        isPlaying = true;
        playStartTime = Time.realtimeSinceStartup;

        debugUI.text = "START\n";
    }

    void NextLyric()
    {
        float now = GetWwiseTime();

        if (segments.Count > 0)
        {
            segments[segments.Count - 1].endTime = now;
        }

        currentIndex++;

        if (currentIndex >= lyrics.Count)
        {
            EndSong();
            return;
        }

        string text = lyrics[currentIndex];

        segments.Add(new LyricSegment
        {
            startTime = now,
            text = text
        });

        debugUI.text += $"[{now:F2}] {text}\n";
    }

    void EndSong()
    {
        float now = GetWwiseTime();

        if (segments.Count > 0)
        {
            segments[segments.Count - 1].endTime = now;
        }

        isPlaying = false;

        ExportLRC();
    }

    float GetWwiseTime()
    {
        return Time.realtimeSinceStartup - playStartTime;
    }

    void ExportLRC()
    {
        string output = "";

        foreach (var s in segments)
        {
            int min = (int)(s.startTime / 60);
            float sec = s.startTime % 60;

            output += $"[{min:00}:{sec:00.00}] {s.text}\n";
        }

        string folder = Application.dataPath + "/Lyrics/";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = folder + songName + "_lyrics.txt";

        File.WriteAllText(path, output);

        Debug.Log("Exported LRC: " + path);
    }
}