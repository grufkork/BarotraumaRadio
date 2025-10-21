using Barotrauma;
using LibVLCSharp.Shared;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BarotraumaRadio
{
    public class BufferPlayer : IDisposable
    {
        private readonly LibVLC         _libVLC;
        private readonly MediaPlayer    _mediaPlayer;
        private readonly static int     _bufferSize = 88200 * 4;
        private readonly CircularBuffer _pcmBuffer = new(_bufferSize);
        private bool _disposed;

        public BufferPlayer()
        {
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string vlcPath = Path.Combine(modPath, "../../../Content/libvlc/win-x64");

            if (!Directory.Exists(vlcPath))
            {
                LuaCsSetup.PrintCsError($"Failed to find path for VLC player at: {vlcPath}");
            }
            else
            {
                LuaCsSetup.PrintCsMessage($"Found VLC at path: {vlcPath}");

                LuaCsSetup.PrintCsMessage($"libvlc.dll exists: {File.Exists(Path.Combine(vlcPath, "libvlc.dll"))}");
                LuaCsSetup.PrintCsMessage($"Plugins exists: {Directory.Exists(Path.Combine(vlcPath, "plugins"))}");

                Core.Initialize(vlcPath);
            }
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.SetAudioFormat("S16N", 44100, 1);
            _mediaPlayer.SetAudioCallbacks(PlayCallback, null, null, null, null);
        }

        private void PlayCallback(IntPtr opaque, IntPtr samples, uint count, long pts)
        {
            int bytes = (int)count * 2;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(samples, buffer, 0, bytes);
            _pcmBuffer.Write(buffer);
        }

        public int ReadPcmData(short[] outputBuffer, int maxSamples)
        {
            int bytesRequested = maxSamples * 2; 
            byte[] byteBuffer = new byte[bytesRequested];
            int bytesRead = _pcmBuffer.Read(byteBuffer, 0, bytesRequested);

            Buffer.BlockCopy(byteBuffer, 0, outputBuffer, 0, bytesRead);
            return bytesRead / 2; 
        }

        public void Play(string url)
        {
            using Media media = new(_libVLC, url, FromType.FromLocation);

            media.AddOption(":network-caching=500"); 
            media.AddOption(":live-caching=500");
            _mediaPlayer.Volume = 125;
            _mediaPlayer.Play(media);
        }

        public void Stop()
        {
            if (_mediaPlayer == null)
            {
                return;
            }
            _mediaPlayer.Stop();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
            _disposed = true;
        }
    }
}
