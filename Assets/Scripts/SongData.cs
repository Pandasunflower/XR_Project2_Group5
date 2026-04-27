using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PitchFrame {
    public float t; // Time (秒)
    public float f; // Frequency (Hz)
    public float m; // MIDI Note
}

[System.Serializable]
public class SongPitchData {
    public string songName;
    public List<PitchFrame> frames;
}