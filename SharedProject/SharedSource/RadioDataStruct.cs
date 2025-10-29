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
    };
}
