namespace AliceInCradle.Network
{
    public class StrengthConfig
    {
        public int? Set { get; set; }
        public int? Add { get; set; }
        public int? Sub { get; set; }

        public StrengthConfig()
        {
            Set = null;
            Add = null;
            Sub = null;
        }
    }
}
