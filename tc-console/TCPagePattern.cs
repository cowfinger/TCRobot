using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC
{
    class TCPagePattern
    {
        public string Name
        {
            get;
            set;
        }

        public string BeginPattern
        {
            get;
            set;
        }

        public string EndPattern
        {
            get;
            set;
        }

        public string EnumPattern
        {
            get;
            set;
        }

        public string SinglePattern
        {
            get;
            set;
        }

        public IList<TCPagePattern> SubPatterns
        {
            get;
            private set;
        }

        public TCPagePattern(string name)
        {
            this.Name = name;
            this.SubPatterns = new List<TCPagePattern>();
        }

        public void ParsePage(string page)
        {
            if (!string.IsNullOrEmpty(this.SinglePattern) && !string.IsNullOrEmpty(this.EndPattern))
            {
                throw new InvalidOperationException("Pattern can be either Single or Enum but not both.");
            }

            int ParseBegin = 0;
            if (!string.IsNullOrEmpty(this.BeginPattern))
            {
                var matchBegin = Regex.Match(page, this.BeginPattern);
                if (matchBegin.Success)
                {
                    ParseBegin = matchBegin.Index + matchBegin.Length;
                }
            }

            int ParseEnd = page.Length;
            if (!string.IsNullOrEmpty(this.EndPattern))
            {
                var regex = new Regex(this.EndPattern);
                var matchEnd = regex.Match(page, ParseBegin);
                if (matchEnd.Success)
                {
                    ParseEnd = matchEnd.Index;
                }
            }

            if (!string.IsNullOrEmpty(this.SinglePattern))
            {
                var regex = new Regex(this.SinglePattern);
                var singleMatch = regex.Match(page, ParseBegin, ParseEnd - ParseBegin);
                if (singleMatch.Success)
                {
                }
            }
        }
    }
}
