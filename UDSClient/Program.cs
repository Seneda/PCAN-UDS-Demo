using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using PCANUDS;
using Peak.Can.Uds;


using TPCANHandle = System.UInt16;
using TPCANTimestampFD = System.UInt64;
using TPUDSCANHandle = System.UInt16;

namespace PCUClient
{
    public class Program
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace);

        // a global counter to keep track of the number of failed tests (see displayMessage function)
        static int g_nbErr = 0;

        // CAN address information for this example
        private static readonly byte N_SA = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_TEST_EQUIPMENT);
        private static readonly byte N_TA_PHYS = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_ECU_1);
        private static readonly byte N_TA_FUNC = ((byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL);
        private static readonly byte N_RA = ((byte)0x00);

        // console handling
        private static readonly bool USE_GETCH = false;

        // A simple function that waits for user input
        static void waitGetch(string pMsg = null)
        {
            if (USE_GETCH)
            {
                if (pMsg != null)
                    log.Debug($" {pMsg} Press <Enter> to continue...");
                else
                    log.Debug(" Press <Enter> to continue...");
                Console.ReadKey(true);

                //if (USE_GETCH) Console.Clear();
            }
        }

        // A function that displays UDS Request and Response messages (and count error if no response)
        static void displayMessage2(TPUDSMsg Request, TPUDSMsg Response, bool noResponseExpected = false)
        {
            if (!Request.Equals(default(TPUDSMsg)))
            {
                string result = Request.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK!";
                log.Debug($"UDS REQUEST  from 0x{Request.NETADDRINFO.SA:x2} (to 0x{Request.NETADDRINFO.TA:x2}, with RA 0x{Request.NETADDRINFO.RA:x2}) - result: {Request.RESULT} - {result}");
                // display data
                string s = $"\t\\-> Length: {Request.LEN}, Data= ";
                for (int i = 0; i < Request.LEN; i++)
                {
                    s += $"{Request.DATA[i]:x2} ";
                }
                log.Debug(s);

            }
            if (!Response.Equals(default(TPUDSMsg)))
            {
                string result = Response.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK!";
                log.Debug($"UDS RESPONSE from 0x{Response.NETADDRINFO.SA:x2} (to 0x{Response.NETADDRINFO.TA:x2}, with RA 0x{Response.NETADDRINFO.RA:x2}) - result: {Response.RESULT} - {result}");
                // display data
                string s = $"\t\\-> Length: {Response.LEN}, Data= ";
                for (int i = 0; i < Response.LEN; i++)
                {
                    s += $"{Response.DATA[i]:x2} ";
                }
                log.Debug(s);
            }
            else if (!noResponseExpected)
            {
                log.Debug(" /!\\ ERROR : NO UDS RESPONSE !!");
                g_nbErr++;
            }
        }

        static void displayMessage(TPUDSMsg Request, TPUDSMsg Response, bool noResponseExpected = false)
        {
            if (!Request.Equals(default(TPUDSMsg)))
            {
                string result = Request.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK!";
                log.Info($"UDS REQUEST   {Request.ToStringEx()}");
            }
            if (!Response.Equals(default(TPUDSMsg)))
            {
                log.Info($"UDS RESPONSE  {Response.ToStringEx()}");
            }
            else if (!noResponseExpected)
            {
                log.Warn(" /!\\ ERROR : NO UDS RESPONSE !!");
                g_nbErr++;
            }
        }

        // Inverts the bytes of a 32 bits numeric value
        //
        static uint Reverse32(uint v)
        {
            byte[] array = BitConverter.GetBytes(v);
            Array.Reverse(array);

            return BitConverter.ToUInt32(array, 0);
            //return ((v & 0x000000FF) << 24) | ((v & 0x0000FF00) << 8) | ((v & 0x00FF0000) >> 8) | ((v & 0xFF000000) >> 24);
        }

        // UDS Service DiagnosticSessionControl
        static void testDiagnosticSessionControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSSessionInfo lSessionInfo = new TPUDSSessionInfo();
            TPUDSMsg Message         = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq      = new TPUDSMsg();

            // initialization
            Message.NETADDRINFO = N_AI;
            lSessionInfo.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: DiagnosticSessionControl ***");

            //  Read default session information 
            //  Server is not yet known (Status will be PUDS_ERROR_NOT_INITIALIZED)
            //	yet the API will still set lSessionInfo to default values
            IntPtr lSessionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, lSessionInfoPtr, false);

            Status = UDSApi.GetValue(
                Channel, 
                TPUDSParameter.PUDS_PARAM_SESSION_INFO,
                lSessionInfoPtr, 
                (uint)Marshal.SizeOf(lSessionInfo)
                );


            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(lSessionInfoPtr, typeof(TPUDSSessionInfo));
            log.Debug($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX:x4}; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX:x4}]");
            waitGetch();

            // Set Diagnostic session to DEFAULT (to get session information)
            log.Info("Setting a DEFAULT Diagnostic Session :");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_DS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDiagnosticSessionControl: {Status}");

            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());


            // Read current session information
            lSessionInfo = new TPUDSSessionInfo();
            lSessionInfo.NETADDRINFO = N_AI;
            lSessionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, lSessionInfoPtr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, lSessionInfoPtr, (uint)Marshal.SizeOf(lSessionInfo));
            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(lSessionInfoPtr, typeof(TPUDSSessionInfo));
            log.Debug($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX}ms; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX}ms]");
            waitGetch();


            // Set Diagnostic session to PROGRAMMING
            log.Info("Setting a ECUPS Diagnostic Session :");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_ECUPS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDiagnosticSessionControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());


            // Read current session information
            lSessionInfo = new TPUDSSessionInfo();
            lSessionInfo.NETADDRINFO = N_AI;
            lSessionInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lSessionInfo));
            Marshal.StructureToPtr(lSessionInfo, lSessionInfoPtr, false);
            Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SESSION_INFO, lSessionInfoPtr, (uint)Marshal.SizeOf(lSessionInfo));
            lSessionInfo = (TPUDSSessionInfo)Marshal.PtrToStructure(lSessionInfoPtr, typeof(TPUDSSessionInfo));
            log.Debug($"  Diagnostic Session Information: {Status}, 0x{lSessionInfo.NETADDRINFO.TA:x2} => {lSessionInfo.SESSION_TYPE} = [{lSessionInfo.TIMEOUT_P2CAN_SERVER_MAX:x4}; {lSessionInfo.TIMEOUT_ENHANCED_P2CAN_SERVER_MAX:x4}]");
            log.Debug(" Assert that Auto TesterPresent Frame is sent...");
            Thread.Sleep(2000);
            log.Debug("  Should transmit an Auto TesterPresent Frame");
            Thread.Sleep(2000);
            log.Debug("  Should transmit an Auto TesterPresent Frame");

            waitGetch();


            // Set Diagnostic session back to DEFAULT
            log.Info("Setting a DEFAULT Diagnostic Session :");
            Status = UDSApi.SvcDiagnosticSessionControl(Channel, ref Message, UDSApi.TPUDSSvcParamDSC.PUDS_SVC_PARAM_DSC_DS);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDiagnosticSessionControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            log.Debug(" Assert that NO Auto TesterPresent Frame is sent...");
            Thread.Sleep(2000);
            log.Debug("  Should NOT transmit an Auto TesterPresent Frame");
            Thread.Sleep(2000);
            log.Debug("  Should NOT transmit an Auto TesterPresent Frame");
            waitGetch();
        }

        // UDS Service ECUReset
        static void testECUReset(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message         = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq      = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ECUReset ***");

            // Sends a Physical ECUReset Message
            log.Debug("Sends a Physical ECUReset Message: ");
            Status = UDSApi.SvcECUReset(Channel, ref Message, UDSApi.TPUDSSvcParamER.PUDS_SVC_PARAM_ER_SR);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcECUReset: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service SecurityAccess
        static void testSecurityAccess(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: SecurityAccess ***");

            // Sends a Physical SecurityAccess Message
            log.Debug("Sends a Physical SecurityAccess Message: ");
            uint valueLittleEndian = 0xF0A1B2C3;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)

            Status = UDSApi.SvcSecurityAccess(Channel, ref Message, UDSApi.PUDS_SVC_PARAM_SA_RSD_1, BitConverter.GetBytes(dwBuffer), (ushort)Marshal.SizeOf(dwBuffer));

            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcSecurityAccess: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service CommunicationControl
        static void testCommunicationControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: CommunicationControl ***");

            // Sends a Physical CommunicationControl Message
            log.Debug("Sends a Physical CommunicationControl Message: ");
            Status = UDSApi.SvcCommunicationControl(Channel, ref Message, UDSApi.TPUDSSvcParamCC.PUDS_SVC_PARAM_CC_ERXTX,
                UDSApi.PUDS_SVC_PARAM_CC_FLAG_APPL | UDSApi.PUDS_SVC_PARAM_CC_FLAG_NWM | UDSApi.PUDS_SVC_PARAM_CC_FLAG_DENWRIRO);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcCommunicationControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service TesterPresent
        static void testTesterPresent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: TesterPresent ***");

            // Sends a Physical TesterPresent Message
            log.Debug("Sends a Physical TesterPresent Message: ");
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcTesterPresent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical TesterPresent Message with no positive response
            log.Debug("Sends a Physical TesterPresent Message with no positive response :");
            Message.NO_POSITIVE_RESPONSE_MSG = UDSApi.PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcTesterPresent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg(), true);
            waitGetch();

            // Sends a Functional TesterPresent Message
            log.Debug("Sends a Functional TesterPresent Message: ");
            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;
            Message.NO_POSITIVE_RESPONSE_MSG = 0;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcTesterPresent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Functional TesterPresent Message with no positive response
            log.Debug("Sends a Functional TesterPresent Message with no positive response :");
            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;
            Message.NO_POSITIVE_RESPONSE_MSG = UDSApi.PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT;
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcTesterPresent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg(), true);
            waitGetch();
        }

        // UDS Service SecuredDataTransmission
        static void testSecuredDataTransmission(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: SecuredDataTransmission ***");

            // Sends a Physical SecuredDataTransmission Message
            log.Debug("Sends a Physical SecuredDataTransmission Message: ");
            uint valueLittleEndian = 0xF0A1B2C3;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
            Status = UDSApi.SvcSecuredDataTransmission(Channel, ref Message, BitConverter.GetBytes(dwBuffer), (ushort)Marshal.SizeOf(dwBuffer));
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcSecuredDataTransmission: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ControlDTCSetting
        static void testControlDTCSetting(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            uint dwBuffer;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ControlDTCSetting ***");

            // Sends a Physical ControlDTCSetting Message
            log.Debug("Sends a Physical ControlDTCSetting Message: ");
            uint valueLittleEndian = 0xF1A1B2EE;
            dwBuffer = Reverse32(valueLittleEndian);   // use helper function to set MSB as 1st byte in the buffer (Win32 uses little endian format)
            Status = UDSApi.SvcControlDTCSetting(Channel, ref Message, UDSApi.TPUDSSvcParamCDTCS.PUDS_SVC_PARAM_CDTCS_OFF, BitConverter.GetBytes(dwBuffer), 3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcControlDTCSetting: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ResponseOnEvent
        static void testResponseOnEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte[] lBuffer2 = new byte[50];
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ResponseOnEvent ***");

            // Sends a Physical ResponseOnEvent Message
            log.Debug("Sends a Physical ResponseOnEvent Message: ");
            lBuffer[0] = 0x08;
            lBuffer2[0] = (byte)TPUDSService.PUDS_SI_ReadDTCInformation;
            lBuffer2[1] = (byte)UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSM;
            lBuffer2[2] = 0x01;
            Status = UDSApi.SvcResponseOnEvent(Channel, ref Message, UDSApi.TPUDSSvcParamROE.PUDS_SVC_PARAM_ROE_ONDTCS,
                false, 0x08, lBuffer, 1, lBuffer2, 3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcResponseOnEvent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service LinkControl
        static void testLinkControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: LinkControl ***");

            // Sends a Physical LinkControl Message
            log.Debug("Sends a Physical LinkControl Message (Verify Fixed Baudrate): ");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_VBTWFBR, (byte)UDSApi.TPUDSSvcParamLCBaudrateIdentifier.PUDS_SVC_PARAM_LC_BAUDRATE_CAN_250K, 0);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcLinkControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            // Sends a Physical LinkControl Message
            log.Debug("Sends a Physical LinkControl Message (Verify Specific Baudrate): ");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_VBTWSBR, 0, 500000);   // 500K = 0x0007a120
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcLinkControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            // Sends a Physical LinkControl Message
            log.Debug("Sends a Physical LinkControl Message (Transition): ");
            Status = UDSApi.SvcLinkControl(Channel, ref Message, UDSApi.TPUDSSvcParamLC.PUDS_SVC_PARAM_LC_TB, 0, 0);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcLinkControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());

            waitGetch();
        }

        // UDS Service ReadDataByIdentifier
        static void testReadDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ReadDataByIdentifier ***");

            // Sends a Physical ReadDataByIdentifier Message
            log.Debug("Sends a Physical ReadDataByIdentifier Message: ");
            ushort[] buffer = new ushort[2] { (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ADSDID, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ECUMDDID };
            Status = UDSApi.SvcReadDataByIdentifier(Channel, ref Message, buffer, 2);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDataByIdentifier: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadMemoryByAddress
        static void testReadMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferAddr = new byte[20];
            byte[] lBufferSize = new byte[20];
            byte buffAddrLen = 10;
            byte buffSizeLen = 3;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ReadMemoryByAddress ***");

            // Sends a Physical ReadMemoryByAddress Message
            log.Debug("Sends a Physical ReadMemoryByAddress Message: ");
            for (int i = 0; i < buffAddrLen; i++)
            {
                lBufferAddr[i] = (byte)('A' + i);
                lBufferSize[i] = (byte)('1' + i);
            }
            Status = UDSApi.SvcReadMemoryByAddress(Channel, ref Message, lBufferAddr, buffAddrLen, lBufferSize, buffSizeLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadMemoryByAddress: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadScalingDataByIdentifier
        static void testReadScalingDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ReadScalingDataByIdentifier ***");

            // Sends a Physical ReadScalingDataByIdentifier Message
            log.Debug("Sends a Physical ReadScalingDataByIdentifier Message: ");
            Status = UDSApi.SvcReadScalingDataByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_BSFPDID);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadScalingDataByIdentifier: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadDataByPeriodicIdentifier
        static void testReadDataByPeriodicIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ReadDataByPeriodicIdentifier ***");

            // Sends a Physical ReadScalingDataByIdentifier Message
            log.Debug("Sends a Physical ReadDataByPeriodicIdentifier Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcReadDataByPeriodicIdentifier(Channel, ref Message, UDSApi.TPUDSSvcParamRDBPI.PUDS_SVC_PARAM_RDBPI_SAMR, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDataByPeriodicIdentifier: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service DynamicallyDefineDataIdentifier
        static void testDynamicallyDefineDataIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            ushort[] lBufferSourceDI = new ushort[20];
            byte[] lBufferMemSize = new byte[20];
            byte[] lBufferPosInSrc = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: DynamicallyDefineDataIdentifier ***");

            // Sends a Physical DynamicallyDefineDataIdentifierDBID Message
            log.Debug("Sends a Physical DynamicallyDefineDataIdentifierDBID Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBufferSourceDI[i] = (ushort)(((0xF0 + i) << 8) + ('A' + i));
                lBufferMemSize[i] = (byte)(i + 1);
                lBufferPosInSrc[i] = (byte)(100 + i);
            }
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierDBID(Channel, ref Message,
                (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CDDID, lBufferSourceDI, lBufferMemSize, lBufferPosInSrc, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDynamicallyDefineDataIdentifierDBID: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message
            log.Debug("Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierDBMA Message: ");
            buffLen = 3;
            byte[] lBuffsAddr = new byte[15];
            byte[] lBuffsSize = new byte[9];
            byte buffAddrLen = 5;
            byte buffSizeLen = 3;
            for (int j = 0; j < buffLen; j++)
            {
                for (int i = 0; i < buffAddrLen; i++)
                {
                    lBuffsAddr[buffAddrLen * j + i] = (byte)((10 * j) + i + 1);
                }
                for (int i = 0; i < buffSizeLen; i++)
                {
                    lBuffsSize[buffSizeLen * j + i] = (byte)(100 + (10 * j) + i + 1);
                }
            }
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierDBMA(Channel, ref Message,
                (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CESWNDID, buffAddrLen, buffSizeLen, lBuffsAddr, lBuffsSize, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDynamicallyDefineDataIdentifierDBMA: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message
            log.Debug("Sends a Physical UDS_SvcDynamicallyDefineDataIdentifierCDDDI Message: ");
            Status = UDSApi.SvcDynamicallyDefineDataIdentifierCDDDI(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_CESWNDID);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcDynamicallyDefineDataIdentifierCDDDI: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service WriteDataByIdentifier
        static void testWriteDataByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort buffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: WriteDataByIdentifier ***");

            // Sends a Physical WriteDataByIdentifier Message
            log.Debug("Sends a Physical WriteDataByIdentifier Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcWriteDataByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_ASFPDID, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcWriteDataByIdentifier: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service WriteMemoryByIdentifier
        static void testWriteMemoryByAddress(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[50];
            byte[] lBufferMemAddr = new byte[50];
            byte[] lBufferMemSize = new byte[50];
            ushort buffLen = 50;
            byte buffAddrLen = 5;
            byte buffSizeLen = 3;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: WriteMemoryByAddress ***");

            // Sends a Physical WriteMemoryByAddress Message
            log.Debug("Sends a Physical WriteMemoryByAddress Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)(i + 1);
                lBufferMemAddr[i] = (byte)('A' + i);
                lBufferMemSize[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcWriteMemoryByAddress(Channel, ref Message, lBufferMemAddr, buffAddrLen,
                lBufferMemSize, buffSizeLen, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcWriteMemoryByAddress: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ClearDiagnosticInformation
        static void testClearDiagnosticInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ClearDiagnosticInformation ***");

            // Sends a Physical ClearDiagnosticInformation Message
            log.Debug("Sends a Physical ClearDiagnosticInformation Message: ");
            Status = UDSApi.SvcClearDiagnosticInformation(Channel, ref Message, 0xF1A2B3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcClearDiagnosticInformation: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service ReadDTCInformation
        static void testReadDTCInformation(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: ReadDTCInformation ***");

            // Sends a Physical ReadDTCInformation Message
            log.Debug("Sends a Physical ReadDTCInformation Message: ");
            Status = UDSApi.SvcReadDTCInformation(Channel, ref Message, UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSM, 0xF1);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformation: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRDTCSSBDTC Message
            log.Debug("Sends a Physical ReadDTCInformationRDTCSSBDTC Message: ");
            Status = UDSApi.SvcReadDTCInformationRDTCSSBDTC(Channel, ref Message, 0x00A1B2B3, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  ReadDTCInformationRDTCSSBDTC: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRDTCSSBRN Message
            log.Debug("Sends a Physical ReadDTCInformationRDTCSSBRN Message: ");
            Status = UDSApi.SvcReadDTCInformationRDTCSSBRN(Channel, ref Message, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformationRDTCSSBRN: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationReportExtended Message
            log.Debug("Sends a Physical ReadDTCInformationReportExtended Message: ");
            Status = UDSApi.SvcReadDTCInformationReportExtended(Channel, ref Message,
                UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RDTCEDRBDN, 0x00A1B2B3, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformationReportExtended: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationReportSeverity Message
            log.Debug("Sends a Physical ReadDTCInformationReportSeverity Message: ");
            Status = UDSApi.SvcReadDTCInformationReportSeverity(Channel, ref Message,
                UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RNODTCBSMR, 0xF1, 0x12);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformationReportSeverity: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationRSIODTC Message
            log.Debug("Sends a Physical ReadDTCInformationRSIODTC Message: ");
            Status = UDSApi.SvcReadDTCInformationRSIODTC(Channel, ref Message, 0xF1A1B2B3);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformationRSIODTC: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();

            // Sends a Physical ReadDTCInformationNoParam Message
            log.Debug("Sends a Physical ReadDTCInformationNoParam Message: ");
            Status = UDSApi.SvcReadDTCInformationNoParam(Channel, ref Message, UDSApi.TPUDSSvcParamRDTCI.PUDS_SVC_PARAM_RDTCI_RSUPDTC);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcReadDTCInformationNoParam: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service InputOutputControlByIdentifier
        static void testInputOutputControlByIdentifier(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferOption = new byte[20];
            byte[] lBufferEnableMask = new byte[20];
            ushort lBuffOptionLen = 10;
            ushort lBuffMaskLen = 5;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: InputOutputControlByIdentifier ***");

            // Sends a Physical InputOutputControlByIdentifier Message
            log.Debug("Sends a Physical InputOutputControlByIdentifier Message: ");
            for (int i = 0; i < lBuffOptionLen; i++)
            {
                lBufferOption[i] = (byte)('A' + i);
                lBufferEnableMask[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcInputOutputControlByIdentifier(Channel, ref Message, (ushort)UDSApi.TPUDSSvcParamDI.PUDS_SVC_PARAM_DI_SSECUSWVNDID,
                lBufferOption, lBuffOptionLen, lBufferEnableMask, lBuffMaskLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcInputOutputControlByIdentifier: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RoutineControl
        static void testRoutineControl(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[20];
            ushort lBuffLen = 10;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: RoutineControl ***");

            // Sends a Physical RoutineControl Message
            log.Debug("Sends a Physical RoutineControl Message: ");
            for (int i = 0; i < lBuffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcRoutineControl(Channel, ref Message, UDSApi.TPUDSSvcParamRC.PUDS_SVC_PARAM_RC_RRR,
                0xF1A2, lBuffer, lBuffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcRoutineControl: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestDownload
        static void testRequestDownload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferMemAddr = BitConverter.GetBytes(0x00010020).Reverse().ToArray();
            byte[] lBufferMemSize = BitConverter.GetBytes(32).Reverse().ToArray();
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: RequestDownload ***");

            // Sends a Physical RequestDownload Message
            log.Debug("Sends a Physical RequestDownload Message: ");



            byte compressionMethod = 0x00; // No Compression
            byte encryptionMethod = 0x00; // No Compression
            Status = UDSApi.SvcRequestDownload(Channel, ref Message, compressionMethod, encryptionMethod,
                lBufferMemAddr, (byte)lBufferMemAddr.Length, lBufferMemSize, (byte)lBufferMemSize.Length);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcRequestDownload: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK) {
                displayMessage(Message, MessageResponse);

                int maxBlockSizeLength = MessageResponse.DATA[1] >> 4;
                long maxBlockSize = 0;
                for (int i = 0; i < maxBlockSizeLength; i++)
                {
                    int shift_offset = (8 * (maxBlockSizeLength - 1 - i));
                    maxBlockSize += MessageResponse.DATA[2 + i] << shift_offset;
                }
                log.Info($" Max Block Length: {maxBlockSize}");
            }
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestUpload
        static void testRequestUpload(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBufferMemAddr = new byte[50];
            byte[] lBufferMemSize = new byte[50];
            byte buffAddrLen = 21;
            byte buffSizeLen = 32;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: RequestUpload ***");

            // Sends a Physical RequestUpload Message
            log.Debug("Sends a Physical RequestUpload Message: ");
            for (int i = 0; i < buffSizeLen; i++)
            {
                lBufferMemAddr[i] = (byte)('A' + i);
                lBufferMemSize[i] = (byte)(10 + i);
            }
            Status = UDSApi.SvcRequestUpload(Channel, ref Message, 0x01, 0x02,
                lBufferMemAddr, buffAddrLen, lBufferMemSize, buffSizeLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcRequestUpload: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service TransferData
        static void testTransferData(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            ushort buffLen = 32;
            byte[] lBuffer = new byte[buffLen];
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: TransferData ***");

            // Sends a Physical TransferData Message
            log.Debug("Sends a Physical TransferData Message: ");
            for (int i = 0; i < lBuffer.Length; i++)
            {
                lBuffer[i] = (byte)(i);
            }
            byte blockCounter = 0x01;
            Status = UDSApi.SvcTransferData(Channel, ref Message, blockCounter, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
            {
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            }
            log.Debug($"  UDS_SvcTransferData: {Status}");
            byte checksum = 0;
            log.Debug("Calculating a checksum for the transmitted data");
            for (int i = 0; i < lBuffer.Length; i++) 
            {
                checksum += lBuffer[i];
                //log.Debug($"Calculating checksum byte{i}={lBuffer[i]}, checksum={checksum}");
            }
            if (checksum == MessageResponse.DATA[2])
            {
                log.Info($"Checksum matches 0x{checksum:X2}==0x{MessageResponse.DATA[2]:X2}");
            }
            else
            {
                log.Warn($"Checksum does not match 0x{checksum:X2}!=0x{MessageResponse.DATA[2]:X2}");
            }
            if (blockCounter == MessageResponse.DATA[1])
            {
                log.Info($"blockCounter matches 0x{blockCounter:X2}==0x{MessageResponse.DATA[1]:X2}");
            }
            else
            {
                log.Warn($"blockCounter does not match 0x{blockCounter:X2}!=0x{MessageResponse.DATA[1]:X2}");
            }
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestTransferExit
        static void testRequestTransferExit(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte buffLen = 0;
            byte[] lBuffer = new byte[buffLen];
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: RequestTransferExit ***");

            // Sends a Physical RequestTransferExit Message
            log.Debug("Sends a Physical RequestTransferExit Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcRequestTransferExit(Channel, ref Message, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcRequestTransferExit: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }


        // UDS Service TransferData with MAX_DATA length
        static void testTransferDataBigMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg MessageReq = new TPUDSMsg();
            byte[] lBuffer = new byte[4095];
            ushort buffLen = 4093;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: TransferData with MAX_DATA ***");

            // Sends a Physical TransferData Message with the maximum data available.
            // The goal is to show that UDS_WaitForService doesn't return a TIMEOUT error
            // although the transmit and receive time of all the data will be longer 
            // than the default time to get a response.
            log.Debug($"Sends a Physical TransferData Message (LEN={buffLen}): ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcTransferData(Channel, ref Message, 0x01, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForService(Channel, out MessageResponse, ref Message, out MessageReq);
            log.Debug($"  UDS_SvcTransferData: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                displayMessage(Message, MessageResponse);
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }

        // UDS Service RequestTransferExit
        static void testTransferDataMultipleFunctionalMessage(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            TPUDSMsg[] MessageBuffer = new TPUDSMsg[5] { new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg(), new TPUDSMsg() };
            uint msgBufLen = 5;
            uint msgCount = 0;
            byte[] lBuffer = new byte[10];
            ushort buffLen = 5;
            // initialization
            Message.NETADDRINFO = N_AI;

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service: TransferData with Functional Message***");

            Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL;

            // Sends a Functional TransferData Message.
            // The goal is to show that UDS_WaitForServiceFunctional waits long enough
            // to fetch all possible ECU responses.
            log.Debug("Sends a Functional TransferData Message: ");
            for (int i = 0; i < buffLen; i++)
            {
                lBuffer[i] = (byte)('A' + i);
            }
            Status = UDSApi.SvcTransferData(Channel, ref Message, 0x01, lBuffer, buffLen);
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                Status = UDSApi.WaitForServiceFunctional(Channel, MessageBuffer, msgBufLen, out msgCount, true, ref Message, out Message);
            log.Debug($"  UDS_SvcTransferData: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
            {
                displayMessage(Message, new TPUDSMsg(), true);
                log.Debug($" Received {msgCount} UDS responses:");
                for (uint i = 0; i < msgCount && i < msgBufLen; i++)
                    displayMessage(new TPUDSMsg(), MessageBuffer[i]);
            }
            else
                displayMessage(Message, new TPUDSMsg());
            waitGetch();
        }


        static void testDownloadFile(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI, int startAddress, byte[] data)
        {
            int status = 0;
            int udsBlockSize = 0;
            status = SendServiceRequestDownload(Channel, N_AI, startAddress, data.Length, out udsBlockSize);
            if (status != 0)
            {
                log.Warn("SendServiceRequestDownload Failed");
                return;
            }
            int dataBlockSize = udsBlockSize - 2;
            byte[] dataBlock;
            for (int i = 0; i < data.Length; i += dataBlockSize)
            {
                dataBlock = new byte[Math.Min(dataBlockSize, data.Length - i)];
                Array.Copy(data, i, dataBlock, 0, dataBlock.Length);
                byte blockIndex = (byte)(((i / dataBlockSize) + 1) % 256);
                status = SendServiceTransferData(Channel, N_AI, ref dataBlock, blockIndex, true);

                if (status != 0)
                {
                    log.Warn("SendServiceTransferData Failed");
                    return;
                }
            }
            

            status = SendServiceRequestTransferExit(Channel, N_AI);
            if (status != 0)
            {
                log.Warn("SendServiceRequestTransferExit Failed");
                return;
            }

        }

        private static int SendServiceTransferData(ushort Channel, TPUDSNetAddrInfo N_AI, ref byte[] data, byte blockIndex, bool download)
        {
            TPUDSStatus status;
            TPUDSMsg requestMsg = new TPUDSMsg();
            TPUDSMsg responseMsg = new TPUDSMsg();
            TPUDSMsg requestConfirmationMsg = new TPUDSMsg();

            requestMsg.NETADDRINFO = N_AI;

            if (download)
            {

                status = UDSApi.SvcTransferData(
                    Channel, 
                    ref requestMsg,
                    blockIndex, 
                    data, 
                    (ushort)data.Length
                    );
                log.Debug($"SvcTransferData: {status}");

                if (status != TPUDSStatus.PUDS_ERROR_OK)
                {
                    log.Warn($"Service : TransferData was unsuccessful");
                    return 1;
                }

                UDSApi.WaitForService(
                    Channel,
                    out responseMsg,
                    ref requestMsg,
                    out requestConfirmationMsg
                    );
                log.Debug($"WaitForService: {status}");

                if (status != TPUDSStatus.PUDS_ERROR_OK)
                {
                    log.Warn($"Service : TransferData did not get a positive UDS Response");
                    return 1;
                }
                
                byte checksum = 0;
                log.Debug("Calculating a checksum for the transmitted data");
                for (int i = 0; i < data.Length; i++)
                {
                    checksum += data[i];
                    //log.Debug($"Calculating checksum byte{i}={lBuffer[i]}, checksum={checksum}");
                }

                if (checksum == responseMsg.DATA[2])
                {
                    log.Info($"Checksum matches 0x{checksum:X2}==0x{responseMsg.DATA[2]:X2}");
                }
                else
                {
                    log.Warn($"Checksum does not match 0x{checksum:X2}!=0x{responseMsg.DATA[2]:X2}");
                    return 1;
                }

                if (blockIndex == responseMsg.DATA[1])
                {
                    log.Info($"blockIndex matches 0x{blockIndex:X2}==0x{responseMsg.DATA[1]:X2}");
                }
                else
                {
                    log.Warn($"blockIndex does not match 0x{blockIndex:X2}!=0x{responseMsg.DATA[1]:X2}");
                    return 1;
                }
                
                return 0;
            }
            else
            {
                log.Warn("Upload Transfer data not yet implemented");
                return 1;
            }


        }

        private static int SendServiceRequestTransferExit(ushort Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus status;
            TPUDSMsg requestMsg = new TPUDSMsg();
            TPUDSMsg responseMsg = new TPUDSMsg();
            TPUDSMsg requestConfirmationMsg = new TPUDSMsg();
            byte buffLen = 0;
            byte[] buffer = new byte[buffLen];

            requestMsg.NETADDRINFO = N_AI;

            log.Info("*** UDS Service: RequestTransferExit ***");


            status = UDSApi.SvcRequestTransferExit(
                Channel,
                ref requestMsg,
                buffer,
                buffLen
                );
            log.Debug($"SvcRequestTransferExit: {status}");

            if (status != TPUDSStatus.PUDS_ERROR_OK)
            {
                log.Warn($"Service : RequestTransferExit was unsuccessful");
                return 1;
            }

            UDSApi.WaitForService(
                Channel,
                out responseMsg,
                ref requestMsg,
                out requestConfirmationMsg
                );
            log.Debug($"WaitForService: {status}");

            if (status != TPUDSStatus.PUDS_ERROR_OK)
            {
                log.Warn($"Service : RequestTransferExit did not get a positive UDS Response");
                return 1;
            }

            displayMessage(requestMsg, responseMsg);

            return 0;
        }

        private static int SendServiceRequestDownload(ushort Channel, TPUDSNetAddrInfo N_AI, int startAddress, int dataLength, out int maxBlockSize)
        {
            TPUDSStatus status;
            TPUDSMsg requestMsg = new TPUDSMsg();
            TPUDSMsg responseMsg = new TPUDSMsg();
            TPUDSMsg requestConfirmationMsg = new TPUDSMsg();
            byte[] lBufferMemAddr = BitConverter.GetBytes(startAddress).Reverse().ToArray();
            byte[] lBufferMemSize = BitConverter.GetBytes(dataLength).Reverse().ToArray();
            maxBlockSize = 0;

            requestMsg.NETADDRINFO = N_AI;

            log.Info("*** UDS Service: RequestDownload ***");

            byte compressionMethod = 0x00; // No Compression
            byte encryptionMethod = 0x00; // No Encryption

            status = UDSApi.SvcRequestDownload(
                Channel,
                ref requestMsg,
                compressionMethod,
                encryptionMethod,
                lBufferMemAddr,
                (byte)lBufferMemAddr.Length,
                lBufferMemSize,
                (byte)lBufferMemSize.Length
                );
            log.Debug($"SvcRequestDownload: {status}");


            if (status != TPUDSStatus.PUDS_ERROR_OK)
            {
                log.Warn($"Service : RequestDownload was unsuccessful");
                return 1;
            }
            status = UDSApi.WaitForService(
                Channel,
                out responseMsg,
                ref requestMsg,
                out requestConfirmationMsg
                );
            log.Debug($"WaitForService: {status}");

            if (status != TPUDSStatus.PUDS_ERROR_OK)
            {
                log.Warn($"Service : RequestDownload did not get a positive UDS Response");
                return 1;
            }

            displayMessage(requestMsg, responseMsg);

            int maxBlockSizeLength = responseMsg.DATA[1] >> 4;

            for (int i = 0; i < maxBlockSizeLength; i++)
            {
                int shift_offset = (8 * (maxBlockSizeLength - 1 - i));
                maxBlockSize += responseMsg.DATA[2 + i] << shift_offset;
            }
            log.Info($"Max Block Size: {maxBlockSize}");
            return 0;
        }

        // Sample to use event
        static void testUsingEvent(TPUDSCANHandle Channel, TPUDSNetAddrInfo N_AI)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            TPUDSMsg MessageResponse = new TPUDSMsg();
            ManualResetEvent hEvent;
            uint myEventAsInt;
            bool res;
            // initialization
            Message.NETADDRINFO = N_AI;
            // set event handler
            hEvent = new ManualResetEvent(false);
            myEventAsInt = (uint)hEvent.SafeWaitHandle.DangerousGetHandle().ToInt32();
            Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_RECEIVE_EVENT, ref myEventAsInt, (uint)Marshal.SizeOf(myEventAsInt));
            if (Status != TPUDSStatus.PUDS_ERROR_OK)
            {
                log.Debug("Failed to set event, aborting...");
                waitGetch();
                return;
            }

            if (USE_GETCH) Console.Clear();

            log.Info("*** UDS Service with Event: TesterPresent ***");

            // Sends a Physical TesterPresent Message
            log.Debug("Sends a Physical TesterPresent Message: ");
            Status = UDSApi.SvcTesterPresent(Channel, ref Message);
            log.Debug($"  UDS_SvcTesterPresent: {Status}");
            if (Status == TPUDSStatus.PUDS_ERROR_OK)
            {
                // instead of calling WaitForService function,
                // this sample demonstrates how event can be used.
                //	But note that the use of a thread listening to notifications
                //	and making the read operations is preferred.
                bool bStop = false;
                // wait until we receive expected response
                do
                {
                    res = hEvent.WaitOne();
                    if (res)
                    {
                        // read all messages
                        do
                        {
                            Status = UDSApi.Read(Channel, out MessageResponse);
                            if (Status == TPUDSStatus.PUDS_ERROR_OK)
                            {
                                // this is a simple message check (type and sender/receiver address):
                                // to filter UDS request confirmation and get first message from target,
                                // but real use-case should check that the UDS service ID matches the request
                                if (MessageResponse.MSGTYPE == TPUDSMessageType.PUDS_MESSAGE_TYPE_CONFIRM &&
                                    MessageResponse.NETADDRINFO.SA == N_AI.TA &&
                                    MessageResponse.NETADDRINFO.TA == N_AI.SA)
                                {
                                    bStop = true;
                                    displayMessage(Message, MessageResponse);
                                }
                            }
                        } while (Status != TPUDSStatus.PUDS_ERROR_NO_MESSAGE);
                    }
                } while (!bStop);
            }
            waitGetch();

            // uninitialize event
            myEventAsInt = 0;
            UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_RECEIVE_EVENT, ref myEventAsInt, (uint)Marshal.SizeOf(myEventAsInt));
        }

        public static void Main(string[] args)
        {

            try
            {
                log.Info("Starting UDS Client Test");
                TPUDSCANHandle Channel;
                TPUDSStatus Status;
                TPUDSNetAddrInfo N_AI;
                uint iBuffer;
                uint ulBuffer;
                int nbErr = 0;

                // Set the PCAN-Channel to use (PCAN-USB Channel 1)
                Channel = UDSApi.PUDS_USBBUS1;
                // Initializing of the UDS Communication session 
                Status = UDSApi.Initialize(Channel, TPUDSBaudrate.PUDS_BAUD_250K, 0, 0, 0);
                log.Debug($"Initialize UDS: {Status} (chan. 0x{Channel:x2})");

                // Define Address
                iBuffer = UDSApi.PUDS_SERVER_ADDR_TEST_EQUIPMENT;
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_SERVER_ADDRESS, ref iBuffer, 1);
                log.Debug($"  Set ServerAddress: {Status} (0x{iBuffer:x2})");
                // Define TimeOuts
                ulBuffer = 2000;
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_TIMEOUT_REQUEST, ref ulBuffer, (uint)Marshal.SizeOf(ulBuffer));
                log.Debug($"  Set TIMEOUT_REQUEST: {Status} ({ulBuffer})");
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_TIMEOUT_RESPONSE, ref ulBuffer, (uint)Marshal.SizeOf(ulBuffer));
                log.Debug($"  Set TIMEOUT_REQUEST: {Status} ({ulBuffer})");
                waitGetch();

                // Define Network Address Information used for all the tests
                N_AI.SA = UDSApi.PUDS_SERVER_ADDR_TEST_EQUIPMENT;
                N_AI.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_ECU_1;
                N_AI.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_PHYSICAL;
                N_AI.RA = 0x00;
                N_AI.PROTOCOL = TPUDSProtocol.PUDS_PROTOCOL_ISO_15765_2_11B;

                //
                // The following functions call UDS Services 
                // with the following workflow :
                // -----------------------------------------
                //	// Transmits a UDS Request
                //	Status = UDS_Svc[SERVICE_NAME](Channel, MessageRequest, ...);
                //	// Verify Status
                //	if (Status == PUDS_ERROR_OK) {
                //		// Waits for the service response
                //		Status = UDS_WaitForService(Channel, &MessageResponse, &MessageRequest);
                //	}
                // -----------------------------------------
                //

                Console.WriteLine("Press <Enter> to start...");
                Console.ReadKey(true);

                byte[] data = new byte[128];
                for (byte i = 0; i <  128; i++)
                {
                    data[i] = i;
                }


                testDownloadFile(Channel, N_AI, 0x00010020, data);

                //testDiagnosticSessionControl(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testECUReset(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testSecurityAccess(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testCommunicationControl(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testTesterPresent(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testSecuredDataTransmission(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testControlDTCSetting(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testResponseOnEvent(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testLinkControl(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testReadDataByIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testReadMemoryByAddress(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testReadScalingDataByIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testReadDataByPeriodicIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testDynamicallyDefineDataIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testWriteDataByIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testWriteMemoryByAddress(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testClearDiagnosticInformation(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testReadDTCInformation(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testInputOutputControlByIdentifier(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testRoutineControl(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testRequestDownload(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testRequestUpload(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testTransferData(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testRequestTransferExit(Channel, N_AI);
                //log.Info("--------------------------------------");

                // Miscellaneous examples
                //testTransferDataBigMessage(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testTransferDataMultipleFunctionalMessage(Channel, N_AI);
                //log.Info("--------------------------------------");
                //testUsingEvent(Channel, N_AI);
                //log.Info("--------------------------------------");

                // Display a small report
                if (g_nbErr > 0)
                {
                    log.Debug($"ERROR : {g_nbErr} errors occured.");
                }
                else
                {
                    log.Debug("ALL Transmissions succeeded !");
                }
                log.Debug("Press <Enter> to quit...");
                Console.ReadKey(true);

                UDSApi.Uninitialize(Channel);
            }
            catch (Exception e)
            {
                log.Error("Unhandled Exception", e);
                log.Debug("Press <Enter> to quit...");
                Console.ReadKey(true);
                throw;
            }
        }
    }
}