using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPeer : MonoBehaviour
{
    public AudioSource _audioSource;
    private float[] _samplesLeft = new float[512];
    private float[] _samplesRight = new float[512];

    // Audio 64
    private float[] _freqBand = new float[64];
    private float[] _bandBuffer = new float[64];
    private float[] _bufferDecrease = new float[64];
    private float[] _freqBandHighest = new float[64];

    public float[] _audioBand, _audioBandBuffer;

    public float _amplitude, _ampliduteBuffer;
    private float _amplitudeHighest;
    public float _audioProfile;

    public enum Channel { Stereo, Left, Right }
    public Channel _channel = new Channel();

    private void Start()
    {
        AudioProfile(_audioProfile);
        
        _audioBand = new float[64];
        _audioBandBuffer = new float[64];
    }

    private void FixedUpdate()
    {
        // We update in the fixed update so that we aren't dependant on framerate
        GetSpectrumData();
        MakeFrequencyBands64();
        BandBuffer64();
        CreateAudioBands64();

        for (int i = 0; i < 10; i++)
        {
            GetAmplitude();
        }
        
    }

    void AudioProfile(float audioProfile)
    {
        for (int i = 0; i < 64; i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }

    void GetSpectrumData()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0;

        for (int i = 0; i < 64; i++)
        {
            currentAmplitude += _audioBand[i];
            currentAmplitudeBuffer += _audioBandBuffer[i];
        }

        if(currentAmplitude > _amplitudeHighest)
        {
            _amplitudeHighest = currentAmplitude;
        }

        _amplitude = currentAmplitude / _amplitudeHighest;
        _ampliduteBuffer = currentAmplitude / _amplitudeHighest;
    }

    void CreateAudioBands64()
    {
        for (int i = 0; i < 64; i++)
        {
            if(_freqBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _freqBand[i];
            }

            _audioBand[i] = (_freqBand[i] / _freqBand[i]);
            _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }

    void MakeFrequencyBands64()
    {
        var count = 0;
        var sampleCount = 1;
        var power = 0;

        for (int i = 0; i < 64; i++)
        {
            var average = 0f;

            if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);

                if(power == 3)
                {
                    sampleCount -= 2;
                }
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if(_channel == Channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (_channel == Channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (_channel == Channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }
                    
                count++;
            }

            average /= count;
            _freqBand[i] = average * 8;
        }
    }

    void BandBuffer64()
    {
        for (int g = 0; g < 64; g++)
        {
            if(_freqBand[g] > _bandBuffer[g])
            {
                _bandBuffer[g] = _freqBand[g];
                _bufferDecrease[g] = 0.005f;
            }

            if(_freqBand[g] < _bandBuffer[g])
            {
                _bandBuffer[g] -= _bufferDecrease[g];
                _bufferDecrease[g] *= 1.35f;
            }
        }
    }
}
