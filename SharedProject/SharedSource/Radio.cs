using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using static Barotrauma.Voting;

namespace BarotraumaRadio
{
    public partial class Radio : CustomInterface
    {
        private readonly ContentXElement contentXElement;

        private string stationsPath = "";
        private string lastPlayedPath = "";

        private RadioItem[] radiostations =
        [
            new("AniSonFM", "https://pool.anison.fm/AniSonFM(320)"),
            new("truckers.fm", "http://radio.truckers.fm"),
            new("radioparadise.com", "http://stream.radioparadise.com/flacm"),
            new("somafm", "http://ice1.somafm.com/groovesalad-256-mp3"),
            new("kexp", "http://kexp-mp3-128.streamguys1.com/kexp128.mp3"),
            new("srg-ssr", "http://stream.srg-ssr.ch/m/rsj/mp3_128"),
            new("ClassicFM", "http://media-ssl.musicradio.com/ClassicFM"),
            new("radiocaroline", "http://sc5.radiocaroline.net:8040/stream"),
        ];

        private int currentStationIndex = 0;

        private float volume = 0.85f;

        private bool radioEnabled = false;
        private bool lastLeverValue = false;

        private Powered? powered;

        public Radio(Item item, ContentXElement element) : base(item, element)
        {
            contentXElement = element;
        }

        public bool TryGetPoweredComponent(out Powered? component)
        {
            component = item.GetComponent<Powered>();
            return component != null;
        }

        private void LoadFromFile()
        {
            string contentDirectory = FindContentDirectory();
            if (string.IsNullOrEmpty(contentDirectory))
            {
                LuaCsSetup.PrintCsError("Could not find content directory");
                return;
            }

            stationsPath = Path.Combine(contentDirectory, "radiostations.json");
            lastPlayedPath = Path.Combine(contentDirectory, "lastPlayed.txt");

            try
            {
                if (File.Exists(stationsPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found stations file");
                    string stationsText = File.ReadAllText(stationsPath);
                    radiostations = JsonSerializer.Deserialize<RadioItem[]>(stationsText)!;
                }

                if (File.Exists(lastPlayedPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found last played file");
                    int index = int.Parse(File.ReadAllText(lastPlayedPath));
                    currentStationIndex = Math.Min(index, radiostations.Length - 1);
                }
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }
        }

        private string FindContentDirectory()
        {
            try
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string modDirectory = Path.GetDirectoryName(assemblyLocation)!;
                string contentPath = Path.Combine(modDirectory, "..", "..", "..", "Content");

                LuaCsSetup.PrintCsMessage("Going to check contentPath - " + contentPath);

                if (Directory.Exists(contentPath))
                {
                    return Path.GetFullPath(contentPath);
                }

                LuaCsSetup.PrintCsMessage("Failed to find content directory at" + contentPath);
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            try
            {
                string contentPath = Path.Combine("RadioMod", "Content");

                LuaCsSetup.PrintCsMessage("Going to check contentPath - " + contentPath);

                if (Directory.Exists(contentPath))
                {
                    return contentPath;
                }

                LuaCsSetup.PrintCsError("Failed to find content directory at" + contentPath);
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            return "";
        }
    }
}
