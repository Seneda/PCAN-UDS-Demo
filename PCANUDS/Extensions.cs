using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peak.Can.Uds;

namespace PCANUDS
{
    public static class extensions
    {
        public static string ToStringEx(this TPUDSMsg msg)
        {
            string result = msg.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK !";
            byte[] data = new byte[msg.LEN];
            Array.Copy(msg.DATA, 0, data, 0, data.Length);
            string serviceID;
            if (Enum.IsDefined(typeof(TPUDSService), msg.ServiceID))
            {
                serviceID = ((TPUDSService)msg.ServiceID).ToString();
            }
            else if (Enum.IsDefined(typeof(TPUDSService), (byte)(msg.ServiceID - 0x40)))
            {
                serviceID = "RESPONSE_" + ((TPUDSService)((byte)(msg.ServiceID - 0x40))).ToString();
            }
            else
            {
                serviceID = $"UNKNOWN SI {msg.ServiceID:X2}";
            }
                
            string s = $"TPUDSMsg : SI:{serviceID}, SA:{msg.NETADDRINFO.SA:X2}, TA:{msg.NETADDRINFO.TA:X2}, RA:0x{msg.NETADDRINFO.RA:X2}, Result:{msg.RESULT} - {result}\n\tLEN: {msg.LEN}, DATA: [{BitConverter.ToString(data)}]";
            return s;
        }
    }
}
