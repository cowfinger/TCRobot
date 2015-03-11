namespace TC
{
    public class TCRequestArgument
    {
        public TCRequestArgument(TCElement name)
        {
            this.Name = name;
            this.Value = "";
        }

        public TCRequestArgument(TCElement name, int value)
        {
            this.Name = name;
            this.Value = value.ToString();
        }

        public TCRequestArgument(TCElement name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public TCRequestArgument(TCElement name, object value)
        {
            this.Name = name;
            this.Value = value.ToString();
        }

        public TCElement Name { get; set; }

        public string Value { get; set; }
    };
}