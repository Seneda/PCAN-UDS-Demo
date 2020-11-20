using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peak.Can.Uds;

namespace UDSServer
{
    public static class extensions
    {
        public static string ToStringEx(this TPUDSMsg msg)
        {
            string result = msg.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK !";
            string s = $"TPUDSMsg : SA:{msg.NETADDRINFO.SA:X2}, TA:{msg.NETADDRINFO.TA:X2}, RA:0x{msg.NETADDRINFO.RA:X2}, Result:{msg.RESULT} - {result}";
            return s;
        }
    }
}
