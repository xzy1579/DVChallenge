using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvMod.Challenges
{
    public class Signal
    {
        public static string[] StationIds =
        {
            "HB",
            "SM",
            "CSW",
            "MF",
            "FF",
            "GF",
            "SW",
            "FRC",
            "FRS",
            "OWC",
            "OWN",
            "IME",
            "IMW",
            "CM",
            "FM"
        };

        public string stationId = "";
        public string status = "";
        public string message = "";
        public string jobs="";
    }
}
