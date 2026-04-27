using System;

public class DywaPitchTrack
{
    private double _prevPitch = -1.0;
    private int _pitchConfidence = 0;

    public double ComputePitch(float[] samples, int start, int count, double sampleRate)
    {
        float rawHz = (float)dywa_compute_pitch(samples, start, count);
        
        // 這是 DYWA 核心的動態過濾邏輯，能讓音高更穩定
        if (rawHz > 0)
        {
            if (_prevPitch == -1.0 || Math.Abs(rawHz - _prevPitch) < 100.0)
            {
                _prevPitch = rawHz;
                _pitchConfidence = 20;
            }
            else if (_pitchConfidence > 0)
            {
                _pitchConfidence--;
                return _prevPitch;
            }
            else
            {
                _prevPitch = rawHz;
            }
        }
        return rawHz;
    }

    private double dywa_compute_pitch(float[] samples, int start, int count)
    {
        // 1. 設定合理的頻率範圍 (人聲通常在 50Hz - 1200Hz)
        float sampleRate = UnityEngine.AudioSettings.outputSampleRate;
        int minPeriod = (int)(sampleRate / 1200); // 高音邊界
        int maxPeriod = (int)(sampleRate / 50);   // 低音邊界
        
        // 確保不會超出陣列邊界
        maxPeriod = Math.Min(maxPeriod, count / 2);

        double[] amdf = new double[maxPeriod + 1];
        double minVal = double.MaxValue;
        double maxVal = double.MinValue;

        // 2. 第一階段：計算 AMDF (加入能量正規化)
        for (int period = minPeriod; period <= maxPeriod; period++)
        {
            double diff = 0;
            for (int i = 0; i < count - period; i++)
            {
                diff += Math.Abs(samples[start + i] - samples[start + i + period]);
            }
            amdf[period] = diff;
            
            if (diff < minVal) minVal = diff;
            if (diff > maxVal) maxVal = diff;
        }

        // 3. 第二階段：尋找「真正的」第一個顯著波谷 (避免倍頻)
        // 我們設定一個門檻，尋找第一個低於 平均值*0.4 的局部極小值
        double threshold = minVal + (maxVal - minVal) * 0.2;
        int bestPeriod = -1;

        for (int period = minPeriod + 1; period < maxPeriod - 1; period++)
        {
            // 尋找局部極小值 (V-shape)
            if (amdf[period] < amdf[period - 1] && amdf[period] < amdf[period + 1])
            {
                if (amdf[period] < threshold)
                {
                    bestPeriod = period;
                    break; // 找到第一個符合門檻的波谷就停止，這能有效抑制「高八度」錯誤
                }
            }
        }

        // 如果沒找到符合門檻的，退而求其次找全局最小
        if (bestPeriod == -1)
        {
            double absoluteMin = double.MaxValue;
            for (int period = minPeriod; period <= maxPeriod; period++)
            {
                if (amdf[period] < absoluteMin)
                {
                    absoluteMin = amdf[period];
                    bestPeriod = period;
                }
            }
        }

        // 4. 第三階段：拋物線插值 (提高頻率精度，解決離散跳動)
        double finalPeriod = bestPeriod;
        if (bestPeriod > minPeriod && bestPeriod < maxPeriod)
        {
            double l = amdf[bestPeriod - 1];
            double c = amdf[bestPeriod];
            double r = amdf[bestPeriod + 1];
            double delta = (r - l) / (2 * (2 * c - l - r + 0.00001));
            finalPeriod += delta;
        }

        return sampleRate / finalPeriod;
    }
}