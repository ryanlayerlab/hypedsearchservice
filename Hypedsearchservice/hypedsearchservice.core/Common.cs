using System;
using System.Collections.Generic;

namespace hypedsearchservice
{ 
    public class ProteinMatch
    {
        public string ProteinName { get; set; }
        public double Weight { get; set; }
        public int KMerLength { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string KMers { get; set; }
    }

    public class Input
    {
        public string IonCharge { get; set; }
        public double PPMTolerance { get; set; }
        public List<double> Weights { get; set; }
    }
}
