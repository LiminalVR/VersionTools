using System;

namespace BuildTools
{
    [Serializable]
    public class VersionModel
    {
        public int Major, Minor, Revision;

        public override string ToString()
        {
            return $"v{Major}.{Minor}.{Revision}";
        }
    }
}