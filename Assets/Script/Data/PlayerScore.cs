namespace Script.Data
{
    public struct PlayerScore
    {
        public uint NetId;
        public int Count;

        public PlayerScore(uint netId, int count)
        {
            NetId = netId;
            Count = count;
        }
    }
}