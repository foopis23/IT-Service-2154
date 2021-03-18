using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class SimpleSfxController : MonoBehaviour, ISfxController
{
    [Serializable]
    public struct SourceSettings
    {
        public AudioMixerGroup output;
        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        public bool playOnAwake;
        public bool loop;
        [Range(0.0f, 256.0f)]
        public int priority;
        [Range(0, 1)]
        public float volume;
        [Range(-3, 3)]
        public float pitch;
        [Range(-1, 1)]
        public float stereoPan;
        [Range(0, 1.0f)]
        public float spatialBlend;
        [Range(0, 1.1f)]
        public float reverbZoneMix;
    }
    
    [Serializable]
    public struct SoundEffect
    { 
        public string name;
        public AudioClip clip;
        [HideInInspector] public AudioSource source;
        [FormerlySerializedAs("SourceSettings")] public SourceSettings sourceSettings;
    }

    public SoundEffect[] soundEffects;

    private void Start()
    {
        // Create Audio Sources for each Effect
        for (var i=0; i<soundEffects.Length; i++)
        {
            
            soundEffects[i].source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;

            soundEffects[i].source.clip = soundEffects[i].clip;
            soundEffects[i].source.mute = soundEffects[i].sourceSettings.mute;
            soundEffects[i].source.bypassEffects = soundEffects[i].sourceSettings.bypassEffects;
            soundEffects[i].source.bypassReverbZones = soundEffects[i].sourceSettings.bypassReverbZones;
            soundEffects[i].source.bypassListenerEffects = soundEffects[i].sourceSettings.bypassListenerEffects;
            soundEffects[i].source.playOnAwake = soundEffects[i].sourceSettings.playOnAwake;
            soundEffects[i].source.loop = soundEffects[i].sourceSettings.loop;
            soundEffects[i].source.priority = soundEffects[i].sourceSettings.priority;
            soundEffects[i].source.volume = soundEffects[i].sourceSettings.volume;
            soundEffects[i].source.pitch = soundEffects[i].sourceSettings.pitch;
            soundEffects[i].source.panStereo = soundEffects[i].sourceSettings.stereoPan;
            soundEffects[i].source.spatialBlend = soundEffects[i].sourceSettings.spatialBlend;
            soundEffects[i].source.reverbZoneMix = soundEffects[i].sourceSettings.reverbZoneMix;
        }
    }

    public void PlayEffect(string effectName)
    {
        foreach (var sound in soundEffects)
        {
            if (!sound.name.Equals(effectName)) continue;
            sound.source.Play();
            return;
        }
        
        //no sound effect found
        throw new Exception($"Invalid Effect Name: {effectName}");
    }
}