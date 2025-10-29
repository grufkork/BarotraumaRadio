using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
<<<<<<< Updated upstream
using System.Text.Json;
=======
>>>>>>> Stashed changes

namespace BarotraumaRadio // Server
{
<<<<<<< Updated upstream
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
=======
    public partial class Radio : CustomInterface, IServerSerializable
    {


        public override void Update(float deltaTime, Camera cam)
        {
            if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
            {
                if (doSync)
                {
                    doSync = false;
                    item.CreateServerEvent(this);
                }
            }
        }

        public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
        {
            msg.WriteBoolean(radioEnabled);
            msg.WriteByte((byte)currentStationIndex);
            msg.WriteRangedSingle(volume, 0f, 1f, 8);
>>>>>>> Stashed changes
        }
    }
}
