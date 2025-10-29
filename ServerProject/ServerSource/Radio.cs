using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using System.Text.Json;

namespace BarotraumaRadio
{
    public partial class Radio : ItemComponent
    {
        public string ServerUrl
        {
            get => currentStationUrl;
            set
            {
                currentStationUrl = value;
                UpdateServerLastPlayed();
                SendUrlToClient();
            }
        }

        private void UpdateServerLastPlayed()
        {
            ServerRadioConfig config = new(currentStationUrl);
            string serializedConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(serverConfigPath, serializedConfig);
        }

        public void SendUrlToClient()
        {
            IWriteMessage message = GameMain.LuaCs.Networking.Start("ChangeStationFromServer");

            if (string.IsNullOrEmpty(ServerUrl))
            {
                return;
            }

            INetSerializableStruct dataStruct = new RadioDataStruct(item.ID, ServerUrl);

            dataStruct.Write(message);
            GameMain.LuaCs.Networking.Send(message);
        }
    }
}
