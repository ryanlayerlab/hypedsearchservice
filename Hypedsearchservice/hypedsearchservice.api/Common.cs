using System;
using System.Collections.Generic;

namespace hypedsearchservice.api
{ 
    public class ProteinMatch
    {
        public string ProteinName { get; set; }
        public double Weight { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
