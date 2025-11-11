using Barotrauma;

namespace BarotraumaRadio
{
    public class RadioDataStruct : INetSerializableStruct
    {
        [NetworkSerialize]
        public int? RadioID;
        [NetworkSerialize]
        public string? ParamValue;

        public RadioDataStruct(int radioID, string paramValue)
        {
            RadioID = radioID;
            ParamValue = paramValue;
        }

        public RadioDataStruct()
        {
        }
    }

    public class PlayDataStruct : INetSerializableStruct
    {
        [NetworkSerialize]
        public int? RadioID;
        [NetworkSerialize]
        public bool Playing;

        public PlayDataStruct(int radioID, bool playing)
        {
            RadioID = radioID;
            Playing = playing;
        }

        public PlayDataStruct()
        {
            Playing = false;
        }
    }
}
