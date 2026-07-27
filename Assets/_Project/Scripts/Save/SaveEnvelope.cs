using System;

namespace TW08.Save
{
    [Serializable]
    public sealed class SaveEnvelope
    {
        public int formatVersion = 1;
        public string payload;
        public string checksum;
    }
}
