using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using System.Text.Json;

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void InitServer()
        {
            SetServerSyncCallbacks();
        }

        public void SetServerSyncCallbacks()
        {
            GameMain.LuaCs.Networking.Receive("ChangeStationFromClient", (object[] args) =>
            {
                IReadMessage message = (IReadMessage)args[0];
                RadioDataStruct dataStruct = INetSerializableStruct.Read<RadioDataStruct>(message);
                Item? item = Item.ItemList.FirstOrDefault(serverItem => serverItem.ID == dataStruct.RadioID);
                if (item == null)
                    return;
                ItemComponent? component = item.Components.FirstOrDefault(c => c is Radio);
                if (component != null && component is Radio radioComponent)
                {
                    SendStringToClient("6");
                    radioComponent.ServerUrl = dataStruct.ParamValue!;
                }
            });

            GameMain.LuaCs.Networking.Receive("SetStateFromClient", (object[] args) =>
            {
                GameMain.LuaCs.PrintMessage("Received set state from client");
                IReadMessage message = (IReadMessage)args[0];
                INetSerializableStruct dataStruct = INetSerializableStruct.Read<PlayDataStruct>(message);
                IWriteMessage msg = GameMain.LuaCs.Networking.Start("SetStateFromServer");
                dataStruct.Write(msg);
                GameMain.LuaCs.Networking.Send(msg);
                GameMain.LuaCs.PrintMessage("Sent set state to clients");
            });

            GameMain.LuaCs.Networking.Receive("RequestStationFromClient", (object[] args) =>
            {
                
                IReadMessage message = (IReadMessage)args[0];

                RadioDataStruct dataStruct = INetSerializableStruct.Read<RadioDataStruct>(message);

                Item? item = Item.ItemList.FirstOrDefault(serverItem => serverItem.ID == dataStruct.RadioID);

                if (item == null)
                    return;

                ItemComponent? component = item.Components.FirstOrDefault(c => c is Radio);

                if (component != null && component is Radio radioComponent)
                {
                    radioComponent.SendUrlToClient();
                }
            });
        }

        public void SendStringToClient(string data)
        {
            IWriteMessage message = GameMain.LuaCs.Networking.Start("SendStringFromServer");
            message.WriteString(data);
            GameMain.LuaCs.Networking.Send(message);
        }
    }
}
