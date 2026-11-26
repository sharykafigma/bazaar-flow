using UnityEditor;
using UnityEngine;

namespace BazaarFlow.EditorTools
{
    /// <summary>
    /// Applies mobile-friendly import settings to everything in Assets/BazaarFlow/Audio:
    /// SFX = mono, decompress on load, ADPCM (tiny CPU cost, instant playback).
    /// Music = mono, streamed, Vorbis ~q0.5 (low memory).
    /// </summary>
    public class BazaarFlowAudioImporter : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.Replace('\\', '/').Contains("/BazaarFlow/Audio/")) return;
            var importer = (AudioImporter)assetImporter;
            importer.forceToMono = true;
            importer.loadInBackground = false;

            var s = importer.defaultSampleSettings;
            bool isMusic = System.IO.Path.GetFileName(assetPath).StartsWith("music_");
            if (isMusic)
            {
                s.loadType = AudioClipLoadType.Streaming;
                s.compressionFormat = AudioCompressionFormat.Vorbis;
                s.quality = 0.5f;
                importer.loadInBackground = true;
            }
            else
            {
                s.loadType = AudioClipLoadType.DecompressOnLoad;
                s.compressionFormat = AudioCompressionFormat.ADPCM;
            }
            s.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            importer.defaultSampleSettings = s;
        }
    }
}
