using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using System.Globalization;

namespace BarotraumaRadio
{
    public partial class Radio : ItemComponent, IServerSerializable, IClientSerializable
    {
        public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
        {
        }

        public void ServerEventRead(IReadMessage msg, Client c)
        {
            throw new NotImplementedException();
        }
    }
}
