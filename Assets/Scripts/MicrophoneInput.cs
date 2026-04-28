using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour {
    private string _device;
    private AudioClip _clip;
    private int _sampleRate = 48000;
    private DywaPitchTrack _tracker = new DywaPitchTrack();
    
    private float[] _samples = new float[2048]; 
    private AudioSource _audioSource;

    [Header("Testing")]
    public bool useSimulatedVocal = false;
    public AK.Wwise.Event vocalEvent;  

    [Header("Filtering")]
    public int filterWindowSize = 5;
    private List<float> _pitchHistory = new List<float>();

    void Start() {
        _audioSource = GetComponent<AudioSource>();
        
        // 1. 修正：Wwise 2024 註冊方式與安全檢查
        AkUnitySoundEngine.RegisterGameObj(gameObject);
        
        // 2. 修正：防止 Audio System 被禁用時報錯
        // 如果 Project Settings 關閉了 Audio，這行會噴錯，我們加一個保險
        try {
            _sampleRate = AudioSettings.outputSampleRate;
            if (_sampleRate <= 0) _sampleRate = 48000;
        } catch {
            _sampleRate = 48000;
            Debug.LogWarning("Unity Audio 系統未啟用，預設使用 48000Hz");
        }

        if (useSimulatedVocal) {
            if (vocalEvent != null && vocalEvent.IsValid()) {
                // 注意：Unity 的 AudioSource 無法直接聽到 Wwise 的聲音
                // 除非你在 Wwise 做了 Output 錄製，這裡暫時維持邏輯，
                // 但建議模擬人聲直接用原本的 AudioSource.Play(vocalClip) 最準確。
                vocalEvent.Post(gameObject);
                Debug.Log("<color=cyan>[Wwise] 語音事件已啟動</color>");
            } else {
                Debug.LogError("未拖入語音 Wwise Event 或 Event 無效！");
            }
        } else {
            StartCoroutine(InitMicrophone());
        }
    }

    IEnumerator InitMicrophone() {
        // 增加等待，確保音訊系統準備好
        yield return new WaitForSeconds(0.1f);

        if (Microphone.devices.Length > 0) {
            _device = Microphone.devices[0];
            _clip = Microphone.Start(_device, true, 10, _sampleRate);
            
            _audioSource.clip = _clip;
            _audioSource.loop = true;
            _audioSource.volume = 0; 

            while (!(Microphone.GetPosition(_device) > 0)) {
                yield return null; 
            }

            _audioSource.Play();
            Debug.Log($"<color=green>麥克風已啟動: {_device}, SampleRate: {_sampleRate}</color>");
        } else {
            Debug.LogError("找不到麥克風設備！請檢查硬體連線或 Windows 隱私設定。");
        }
    }

    public float GetCurrentPitch() {
        if (_audioSource == null || _audioSource.clip == null) return 0;

        if (useSimulatedVocal) {
            // 重要提示：Wwise 的聲音不會出現在 AudioSource.GetOutputData 中
            // 如果要偵測 Wwise 的音高，必須使用 Wwise 的分析插件。
            // 這裡維持原樣，但若無聲，建議改用 Unity 原生播模擬人聲。
            _audioSource.GetOutputData(_samples, 0); 
        } else {
            if (!Microphone.IsRecording(_device)) return 0;

            int micPos = Microphone.GetPosition(_device);
            int clipSamples = _audioSource.clip.samples;
            int readPos = (micPos - _samples.Length + clipSamples) % clipSamples;
            
            _audioSource.clip.GetData(_samples, readPos);
        }

        float peak = 0;
        for (int i = 0; i < _samples.Length; i++) {
            float absVal = Mathf.Abs(_samples[i]);
            if (absVal > peak) peak = absVal;
        }

        // 門檻值過低會抓到雜訊，過高會抓不到聲音
        if (peak < 0.001f) return 0;

        double hz = _tracker.ComputePitch(_samples, 0, _samples.Length, _sampleRate);

        if (double.IsNaN(hz) || hz < 50 || hz > 2000) return 0;

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
            if (_pitchHistory.Count > 0) _pitchHistory.RemoveAt(0);
            if (_pitchHistory.Count == 0) return 0;
        }

        if (_pitchHistory.Count == 0) return 0;
        List<float> sorted = new List<float>(_pitchHistory);
        sorted.Sort();
        return sorted[sorted.Count / 2];
    }

    void OnDestroy() {
        // 修正：Wwise 2024 注銷方式
        AkUnitySoundEngine.UnregisterGameObj(gameObject);
    }
}