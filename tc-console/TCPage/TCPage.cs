namespace TC.TCPage
{
    internal class TCPage
    {
        protected TCPage(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }
    }
}