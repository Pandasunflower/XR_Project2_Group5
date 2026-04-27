using UnityEngine;
using System.Collections.Generic;
using System.Collections; // 為了使用 Coroutine

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour {
    private string _device;
    private AudioClip _clip;
    private int _sampleRate = 48000;
    private DywaPitchTrack _tracker = new DywaPitchTrack();
    
    // 雖然叫 spectrum，但 DYWA 用的是時域波形數據 (Samples)
    private float[] _samples = new float[2048]; 
    private AudioSource _audioSource;

    [Header("Testing")]
    public bool useSimulatedVocal = false;
    public AudioSource testVocalSource;

    [Header("Filtering")]
    public int filterWindowSize = 5;
    private List<float> _pitchHistory = new List<float>();

    void Start() {
        foreach (var device in Microphone.devices) {
            Debug.Log("可用麥克風: " + device);
        }
        _audioSource = GetComponent<AudioSource>();
        
        // 取得系統實際 SampleRate，避免 48000 vs 44100 誤差
        _sampleRate = AudioSettings.outputSampleRate;

        if (useSimulatedVocal) {
            if (testVocalSource != null) {
                testVocalSource.Play();
            } else {
                Debug.LogError("未拖入測試用的 Vocal Source！");
            }
        } else {
            StartCoroutine(InitMicrophone());
        }
    }

    // 使用 Coroutine 初始化麥克風，避免 while 迴圈卡死 Unity 畫面
    IEnumerator InitMicrophone() {
        if (Microphone.devices.Length > 0) {
            _device = Microphone.devices[0];
            _clip = Microphone.Start(_device, true, 10, _sampleRate); // 10秒循環 Buffer
            
            _audioSource.clip = _clip;
            _audioSource.loop = true;
            _audioSource.volume = 0; // 靜音避免回授

            // 等待麥克風採樣填入 Buffer
            while (!(Microphone.GetPosition(_device) > 0)) {
                yield return null; 
            }

            _audioSource.Play();
            Debug.Log($"<color=green>麥克風已啟動: {_device}, SampleRate: {_sampleRate}</color>");
        } else {
            Debug.LogError("找不到麥克風設備！");
        }
    }

    public float GetCurrentPitch() {
        AudioSource activeSource = useSimulatedVocal ? testVocalSource : _audioSource;
        
        // 基礎安全檢查
        if (activeSource == null || activeSource.clip == null) return 0;
        if (!activeSource.isPlaying) return 0;

        if (useSimulatedVocal) {
            // 模擬模式：GetOutputData 運作通常沒問題
            activeSource.GetOutputData(_samples, 0); 
        } else {
            // --- 麥克風模式關鍵修正：直接從 Clip Buffer 抓取 ---
            int micPos = Microphone.GetPosition(_device);
            int clipSamples = activeSource.clip.samples;
            
            // 計算環狀 Buffer 的讀取起始點 (當前錄音位置往回推 2048 點)
            // 加 clipSamples 再取餘數是為了處理 micPos < 2048 的邊界情況
            int readPos = (micPos - _samples.Length + clipSamples) % clipSamples;
            
            activeSource.clip.GetData(_samples, readPos);
        }

        // 計算振幅峰值 (用於 Debug 與門檻過濾)
        float peak = 0;
        for (int i = 0; i < _samples.Length; i++) {
            float absVal = Mathf.Abs(_samples[i]);
            if (absVal > peak) peak = absVal;
        }

        // 印出峰值，如果對著麥克風吹氣還是 0，請檢查 Windows 權限
        // UnityEngine.Debug.Log($"振幅峰值: {peak:F6}");

        // 如果環境太安靜或是沒聲音，直接回傳 0，不要跑後續運算
        if (peak < 0.0005f) return 0;

        // 執行 DYWA 計算
        double hz = _tracker.ComputePitch(_samples, 0, _samples.Length, _sampleRate);

        // 排除不合理的頻率與 NaN
        if (double.IsNaN(hz) || hz < 50 || hz > 2000) return 0;

        // MIDI 轉換
        return 69f + 12f * Mathf.Log((float)hz / 440f, 2f);
    }

    public float GetCurrentPitchFiltered() {
        float rawPitch = GetCurrentPitch(); 

        if (rawPitch > 0) {
            _pitchHistory.Add(rawPitch);
            if (_pitchHistory.Count > filterWindowSize) {
                _pitchHistory.RemoveAt(0);
            }
        } else {
            // 當沒聲音時，逐漸清空歷史，而不是直接設為0 (增加一點穩定度)
            if (_pitchHistory.Count > 0) _pitchHistory.RemoveAt(0);
            if (_pitchHistory.Count == 0) return 0;
        }

        List<float> sorted = new List<float>(_pitchHistory);
        sorted.Sort();
        return sorted[sorted.Count / 2];
    }
}