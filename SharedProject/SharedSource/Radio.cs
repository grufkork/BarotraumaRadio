using Barotrauma;
using Barotrauma.Items.Components;
using System.Reflection;
using System.Text.Json;

namespace BarotraumaRadio // Shared
{
    public partial class Radio : ItemComponent
    {
        private readonly ContentXElement contentXElement;

        private int    currentStationIndex = 0;
        public  string currentStationUrl;
        private float  volume = 0.85f;

        private bool   radioEnabled   = false;
        private bool   lastLeverValue = false;
        private bool   serverSync     = true;

        private string stationsPath   = "";
        private string clientConfigPath = "";
        private string serverConfigPath = "";

        private RadioItem[] radiostations =
        [
            new("3gg", "http://3gg.se:8001/stream")
        ];

<<<<<<< Updated upstream:SharedProject/SharedSource/Radio.cs
=======
        private int currentStationIndex = 0;

        private float volume = 0.85f;

        private bool radioEnabled = false;
        private bool doSync = false;
        private bool lastLeverValue = false;

>>>>>>> Stashed changes:SharedProject/SharedProject/Radio.cs
        private Powered? powered;

        public Radio(Item item, ContentXElement element) : base(item, element)
        {
            contentXElement = element;
            LoadFromFile();
            TryGetPoweredComponent(out Powered? component);
            powered = component;
            if (string.IsNullOrEmpty(currentStationUrl))
            {
                currentStationUrl = radiostations[currentStationIndex].Url;
            }
        }

        public bool TryGetPoweredComponent(out Powered? component)
        {
            component = item.GetComponent<Powered>();
            return component != null;
        }
<<<<<<< Updated upstream:SharedProject/SharedSource/Radio.cs

        private void LoadFromFile()
        {
            string contentDirectory = FindContentDirectory();
            if (string.IsNullOrEmpty(contentDirectory))
            {
                LuaCsSetup.PrintCsError("Could not find content directory");
                return;
            }

            stationsPath = Path.Combine(contentDirectory, "radiostations.json");
            clientConfigPath = Path.Combine(contentDirectory, "clientConfig.json");
            serverConfigPath = Path.Combine(contentDirectory, "serverConfig.json");

            try
            {
                if (File.Exists(stationsPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found stations file");
                    string serializedStations = File.ReadAllText(stationsPath);
                    radiostations = JsonSerializer.Deserialize<RadioItem[]>(serializedStations)!;
                }
                else
                {
                    string serializedStations = JsonSerializer.Serialize(radiostations);
                    File.WriteAllText(stationsPath, serializedStations);
                }
#if CLIENT
                if (File.Exists(clientConfigPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found client config file");
                    string serializedClientConfig = File.ReadAllText(clientConfigPath);
                    ClientRadioConfig clientConfig = JsonSerializer.Deserialize<ClientRadioConfig>(serializedClientConfig)!;
                    currentStationIndex = clientConfig.LastPlayedIndex;
                    volume = clientConfig.Volume;
                    if (GameMain.Client is not null)
                    {
                        serverSync = clientConfig.ServerSync;
                    }
                    else
                    {
                        serverSync = false;
                    }
                }
#endif
#if SERVER
                if (File.Exists(serverConfigPath))
                {
                    string serializedServerConfig = File.ReadAllText(serverConfigPath);
                    ServerRadioConfig clientConfig = JsonSerializer.Deserialize<ServerRadioConfig>(serializedServerConfig)!;
                    currentStationUrl = clientConfig.LastPlayedUrl;
                }
                else
                {
                    currentStationUrl = radiostations[0].Url;
                }
#endif
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

                if (Directory.Exists(contentPath))
                {
                    return Path.GetFullPath(contentPath);
                }
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            try
            {
                string contentPath = Path.Combine("RadioMod", "Content");

                if (Directory.Exists(contentPath))
                {
                    return contentPath;
                }
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            return "";
        }
=======
>>>>>>> Stashed changes:SharedProject/SharedProject/Radio.cs
    }
}
