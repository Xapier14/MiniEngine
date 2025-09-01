using System;
using System.Collections.Generic;
using MiniEngine.Utility;
using static SDL2.SDL_mixer;

namespace MiniEngine
{
    public class AudioService(TaskScheduler taskScheduler)
    {
        private readonly Dictionary<MemoryResource, IntPtr> _effectCache = [];
        private readonly Dictionary<MemoryResource, IntPtr> _musicCache = [];

        private IntPtr GetChunk(MemoryResource audioResource)
        {
            if (!_effectCache.TryGetValue(audioResource, out var chunk))
            {
                chunk = Mix_LoadWAV_RW(audioResource.RwHandle, 0);
                _effectCache.Add(audioResource, chunk);
            }

            if (chunk == IntPtr.Zero)
                LoggingService.Error($"Error loading audio chunk for \"{audioResource}\"");

            return chunk;
        }

        private IntPtr GetMusic(MemoryResource audioResource)
        {
            if (!_musicCache.TryGetValue(audioResource, out var music))
            {
                music = Mix_LoadMUS_RW(audioResource.RwHandle, 0);
                _musicCache.Add(audioResource, music);
            }

            if (music == IntPtr.Zero)
                LoggingService.Error($"Error loading music for \"{audioResource}\"");

            return music;
        }

        internal bool InitializeDevice()
        {
            LoggingService.Debug("Opening audio device...");
            return Mix_OpenAudio(MIX_DEFAULT_FREQUENCY, MIX_DEFAULT_FORMAT, MIX_DEFAULT_CHANNELS, 4096) == -1;
        }

        internal bool CleanupCache()
        {
            foreach (var (_, chunk) in _effectCache)
            {
                Mix_FreeChunk(chunk);
            }
            foreach (var (_, music) in _musicCache)
            {
                Mix_FreeMusic(music);
            }

            return false;
        }

        public bool IsMusicPaused => Mix_PausedMusic() != 0;
        public bool IsMusicPlaying => Mix_PlayingMusic() != 0;

        public void PlayEffect(MemoryResource? audioResource, int channel = -1, int loops = 0)
        {
            if (audioResource == null)
                return;
            var chunk = GetChunk(audioResource);
            Mix_PlayChannel(channel, chunk, loops);
        }

        public void PlayEffectDelayed(MemoryResource? audioResource, int delay, int channel = -1, int loops = 0)
        {
            taskScheduler.ScheduleIn(delay,() =>
            {
                PlayEffect(audioResource, channel, loops);
            });
        }

        public void PlayMusic(MemoryResource? audioResource, int loops = 0)
        {
            if (audioResource == null)
                return;
            var music = GetMusic(audioResource);
            _ = Mix_PlayMusic(music, loops);
        }

        public void PauseMusic()
        {
            Mix_PauseMusic();
        }

        public void ResumeMusic()
        {
            Mix_ResumeMusic();
        }

        public void StopMusic()
        {
            _ = Mix_HaltMusic();
        }
    }
}
