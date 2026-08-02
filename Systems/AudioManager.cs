using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace EscapeSpaceStation.Systems
{
    /// <summary>
    /// Central hub for playing background music (looped) and one-shot sound
    /// effects, respecting the user's volume settings. Uses SoundEffectInstance
    /// for music so we can loop raw audio files without needing the content
    /// pipeline's Song type (which requires MGCB-processed .xnb files).
    /// </summary>
    public class AudioManager
    {
        private readonly AssetManager _assets;
        private readonly SettingsManager _settings;
        private SoundEffectInstance _currentMusic;
        private string _currentMusicName;

        public AudioManager(AssetManager assets, SettingsManager settings)
        {
            _assets = assets;
            _settings = settings;
        }

        public void PlayMusic(string fileName, bool loop = true)
        {
            if (_currentMusicName == fileName && _currentMusic != null && _currentMusic.State != SoundState.Stopped)
                return; // already playing this track

            StopMusic();

            var sfx = _assets.GetSoundEffect("music", fileName);
            if (sfx == null) return; // missing file - silently skip, game keeps running

            _currentMusic = sfx.CreateInstance();
            _currentMusic.IsLooped = loop;
            _currentMusic.Volume = _settings.Current.MusicVolume;
            _currentMusic.Play();
            _currentMusicName = fileName;
        }

        public void StopMusic()
        {
            if (_currentMusic != null)
            {
                _currentMusic.Stop();
                _currentMusic.Dispose();
                _currentMusic = null;
                _currentMusicName = null;
            }
        }

        public void UpdateMusicVolume()
        {
            if (_currentMusic != null)
                _currentMusic.Volume = _settings.Current.MusicVolume;
        }

        public void PlaySfx(string fileName)
        {
            var sfx = _assets.GetSoundEffect("sfx", fileName);
            sfx?.Play(_settings.Current.SfxVolume, 0f, 0f);
        }
    }
}
