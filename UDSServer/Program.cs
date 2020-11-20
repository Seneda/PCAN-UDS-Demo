using System;
using System.Runtime.InteropServices;
using System.Threading;
using Peak.Can.Uds;

using TPCANHandle = System.UInt16;
using TPUDSCANHandle = System.UInt16;
using System.Text;
using PCANUDS;

namespace PCUServer
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace);


        // Inverts the bytes of a 16 bits numeric value
        static ushort Reverse16(ushort v)
        {
            byte[] array = BitConverter.GetBytes(v);
            Array.Reverse(array);

            return BitConverter.ToUInt16(array, 0);
        }

        // A function that displays UDS messages
        static void displayMessage2(ref TPUDSMsg Message, bool isRx)
        {
            if (Message.MSGTYPE != TPUDSMessageType.PUDS_MESSAGE_TYPE_INDICATION)
            {
                string RxTx = isRx ? "Received" : "Transmitted";
                string result = Message.RESULT != TPUDSResult.PUDS_RESULT_N_OK ? "ERROR !!!" : "OK !";
                log.Info($"{RxTx} UDS message from 0x{Message.NETADDRINFO.SA:x2} (to 0x{Message.NETADDRINFO.TA:x2}, with RA 0x{Message.NETADDRINFO.RA:x2}) - result: {Message.RESULT} - {result}");
                // display data
                string s = $"\t\\-> Length: {Message.LEN}, Data= ";
                for (int i = 0; i < Message.LEN; i++)
                {
                    s += $"{Message.DATA[i]:x2} ";
                }
                log.Debug(s);
            }
            else
            {
                log.Debug($"PENDING UDS message from 0x{Message.NETADDRINFO.SA:x2} (to 0x{Message.NETADDRINFO.TA:x2}, with RA 0x{Message.NETADDRINFO.RA:x2}) -> LEN={Message.LEN} ...");
            }
        }

        static void displayMessage(ref TPUDSMsg Message, bool isRx)
        {
            if (Message.MSGTYPE != TPUDSMessageType.PUDS_MESSAGE_TYPE_INDICATION)
            {
                if (isRx)
                {
                    log.Info($"UDS REQUEST   {Message.ToStringEx()}");
                }
                else
                {
                    log.Info($"UDS RESPONSE  {Message.ToStringEx()}");
                }

            }
            else
            {
                log.Debug($"PENDING UDS message from 0x{Message.NETADDRINFO.SA:x2} (to 0x{Message.NETADDRINFO.TA:x2}, with RA 0x{Message.NETADDRINFO.RA:x2}) -> LEN={Message.LEN} ...");
            }
        }

        // This function generates and transmits UDS responses
        static void transmitResponse(TPUDSCANHandle Channel, byte serverAddr, ref TPUDSMsg UDSRequest)
        {
            TPUDSStatus Status;
            TPUDSMsg Message = new TPUDSMsg();
            byte SI = UDSRequest.ServiceID;
            ushort lDataIdentifier;
            ushort lCount;
            ushort lMemorySizeLength;
            ushort lMemoryAddressLength;
            Random rnd = new Random();

            // copy N_AI
            Message.NETADDRINFO = UDSRequest.NETADDRINFO;
            if (UDSRequest.NETADDRINFO.TA_TYPE == TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL)
                // response to functional addressing is set to TEST_EQUIPMENT
                Message.NETADDRINFO.TA = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_TEST_EQUIPMENT;
            else
                Message.NETADDRINFO.TA = UDSRequest.NETADDRINFO.SA;
            Message.NETADDRINFO.SA = serverAddr;
            Message.NETADDRINFO.TA_TYPE = TPUDSAddressingType.PUDS_ADDRESSING_PHYSICAL;
            // all responses are positive
            Message.DATA = new byte[UDSApi.PUDS_MAX_DATA];
            Message.DATA[0] = (byte)(UDSRequest.ServiceID + UDSApi.PUDS_SI_POSITIVE_RESPONSE);

            // customize message response based on the UDS Service ID (see ISO 14229-1)
            switch ((TPUDSService)SI)
            {
                case TPUDSService.PUDS_SI_DiagnosticSessionControl:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.DATA[2] = 0x00;  // P2Can_Server_Max = 0x0010
                    Message.DATA[3] = 0x10;
                    Message.DATA[4] = 0x03;  // P2*Can_Server_Max = 0x03E8
                    Message.DATA[5] = 0xE8;
                    Message.LEN = 6;
                    break;
                case TPUDSService.PUDS_SI_ECUReset:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    if (UDSRequest.DATA[1] == (byte)UDSApi.TPUDSSvcParamER.PUDS_SVC_PARAM_ER_ERPSD)
                    {
                        Message.DATA[2] = 0x66;  // power down time
                        Message.LEN = 3;
                    }
                    break;
                case TPUDSService.PUDS_SI_SecurityAccess:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    if (UDSRequest.DATA[1] >= UDSApi.PUDS_SVC_PARAM_SA_RSD_MIN
                        && UDSRequest.DATA[1] <= UDSApi.PUDS_SVC_PARAM_SA_RSD_MAX
                        && UDSRequest.DATA[1] % 2 == 1)
                    {   // Request security seed are Even values
                        // fill with dummy data
                        Message.LEN = (ushort)((1 + SI) > UDSApi.PUDS_MAX_DATA ? UDSApi.PUDS_MAX_DATA : (1 + SI));
                        for (int i = 1; i < Message.LEN - 1; i++)   // (LEN - 1) as POSITIVE.SI uses 1 byte
                            Message.DATA[i + 1] = (byte)(i + 1);
                    }
                    break;
                case TPUDSService.PUDS_SI_CommunicationControl:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    break;
                case TPUDSService.PUDS_SI_TesterPresent:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    break;
                case TPUDSService.PUDS_SI_SecuredDataTransmission:
                    // fill with dummy data (check Security-SubLayer record defined in ISO-15764)
                    Message.LEN = (ushort)(1 + rnd.Next(0, 32767) % 50);
                    for (int i = 0; i < Message.LEN - 1; i++)   // (LEN - 1) as POSITIVE.SI uses 1 byte
                        Message.DATA[i + 1] = (byte)(i + 1);
                    break;
                case TPUDSService.PUDS_SI_ControlDTCSetting:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    break;
                case TPUDSService.PUDS_SI_ResponseOnEvent:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    if (UDSRequest.DATA[1] == (byte)UDSApi.TPUDSSvcParamROE.PUDS_SVC_PARAM_ROE_RAE)
                    {
                        Message.DATA[2] = 0; // # of activated events
                        Message.LEN = 3;
                        // EventType and ServiceToRespondTo Records not implemented
                    }
                    else
                    {
                        Message.DATA[2] = 0; // # of identified events
                        Message.DATA[3] = UDSRequest.DATA[2]; // # event window time
                        Message.LEN = 4;
                        // EventType and ServiceToRespondTo Records not implemented
                    }
                    break;
                case TPUDSService.PUDS_SI_LinkControl:
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.LEN = 2;
                    break;
                case TPUDSService.PUDS_SI_ReadDataByIdentifier:
                    lCount = 0;
                    for (int i = 0; i < UDSRequest.LEN - 1; i += 2)
                    {
                        // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                        lDataIdentifier = Reverse16(UDSRequest.DATA[i + 1]);
                        // copy DataIdentifier
                        Message.DATA[lCount++ + 1] = UDSRequest.DATA[i + 1];
                        Message.DATA[lCount++ + 1] = UDSRequest.DATA[i + 2];
                        // DataRecord : fill with dummy data
                        for (int j = 0; j < 5; j++)
                        {
                            Message.DATA[lCount++ + 1] = (byte)(j + 'A');
                        }
                    }
                    Message.LEN = (ushort)(lCount + 1);
                    break;
                case TPUDSService.PUDS_SI_ReadMemoryByAddress:
                    // read memorySize = bits [7..4]
                    Message.LEN = (ushort)(1 + ((UDSRequest.DATA[2] >> 4) & 0xF));
                    // fill with dummy data
                    for (int i = 0; i < Message.LEN - 1; i++)   // (LEN - 1) as POSITIVE.SI uses 1 byte
                        Message.DATA[i + 1] = (byte)(i + 1);
                    break;
                case TPUDSService.PUDS_SI_ReadScalingDataByIdentifier:
                    // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                    lDataIdentifier = Reverse16(UDSRequest.DATA[1]);
                    // copy DataIdentifier
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.DATA[2] = UDSRequest.DATA[2];
                    // create a formula Vehicule Speed = (0.75*x+30) km/h
                    Message.DATA[3] = (0x0 << 4) | (0x1);    // unSignedNumeric of 1 Bytes)
                    Message.DATA[4] = 0x90;  // formula, 0 data bytes
                    Message.DATA[5] = 0x00;  // formulaIdentifier = C0 * x + C1
                    Message.DATA[6] = 0xE0;  // C0 high byte
                    Message.DATA[7] = 0x4B;  // C0 low byte
                    Message.DATA[8] = 0x00;  // C1 high byte
                    Message.DATA[9] = 0x1E;  // C1 low byte
                    Message.DATA[10] = 0xA0;  // unit/format, 0 data bytes
                    Message.DATA[11] = 0x30; // unit ID, km/h
                    Message.LEN = 11;
                    break;
                case TPUDSService.PUDS_SI_ReadDataByPeriodicIdentifier:
                    Message.LEN = 1;
                    break;
                case TPUDSService.PUDS_SI_DynamicallyDefineDataIdentifier:
                    Message.LEN = 4;
                    Message.DATA[1] = UDSRequest.DATA[1];
                    // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                    lDataIdentifier = Reverse16(UDSRequest.DATA[1]);
                    // copy DataIdentifier
                    Message.DATA[2] = UDSRequest.DATA[2];
                    Message.DATA[3] = UDSRequest.DATA[3];
                    break;
                case TPUDSService.PUDS_SI_WriteDataByIdentifier:
                    Message.LEN = 3;
                    // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                    lDataIdentifier = Reverse16(UDSRequest.DATA[1]);
                    // copy DataIdentifier
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.DATA[2] = UDSRequest.DATA[2];
                    break;
                case TPUDSService.PUDS_SI_WriteMemoryByAddress:
                    // read MemorySizeLength & MemoryAddressLength
                    lMemorySizeLength = (ushort)((UDSRequest.DATA[1] >> 4) & 0xF);
                    lMemoryAddressLength = (ushort)(UDSRequest.DATA[1] & 0xF);
                    Message.LEN = (ushort)(2 + lMemorySizeLength + lMemoryAddressLength);
                    // copy Address and Memory parameters
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Array.Copy(UDSRequest.DATA, 2, Message.DATA, 2, lMemoryAddressLength);
                    Array.Copy(UDSRequest.DATA, 2 + lMemoryAddressLength, Message.DATA, 2 + lMemoryAddressLength, lMemorySizeLength);
                    break;
                case TPUDSService.PUDS_SI_ClearDiagnosticInformation:
                    Message.LEN = 1;
                    break;
                case TPUDSService.PUDS_SI_InputOutputControlByIdentifier:
                    Message.LEN = 3;
                    // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                    lDataIdentifier = Reverse16(UDSRequest.DATA[1]);
                    // copy DataIdentifier
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.DATA[2] = UDSRequest.DATA[2];
                    // ControlStatus Record not implemented
                    break;
                case TPUDSService.PUDS_SI_RoutineControl:
                    Message.LEN = 4;
                    Message.DATA[1] = UDSRequest.DATA[1];
                    // Use helper function to read network data (in big endian format) to WORD value (in windows i.e. little endian format)
                    lDataIdentifier = Reverse16(UDSRequest.DATA[1]);
                    // copy DataIdentifier
                    Message.DATA[2] = UDSRequest.DATA[2];
                    Message.DATA[3] = UDSRequest.DATA[3];
                    // RoutineStatus Record not implemented
                    break;
                case TPUDSService.PUDS_SI_RequestDownload:
                case TPUDSService.PUDS_SI_RequestUpload:
                    Message.LEN = 4;
                    Message.DATA[1] = 0x20;  // max number of block length = 0xF
                    Message.DATA[2] = 0x00;
                    Message.DATA[3] = 32 + 2; // Only allow data to be sent in blocks of 32 bytes ( which ends up as 5 can frames 4+7+7+7+7 = 32) Plus two bytes for the Service ID and the block counter
                    break;
                case TPUDSService.PUDS_SI_TransferData:
                    Message.LEN = (ushort)((2 + UDSRequest.LEN) > UDSApi.PUDS_MAX_DATA ? UDSApi.PUDS_MAX_DATA : 2 + UDSRequest.LEN);
                    // custom response to PCUClient example:
                    //  a. service is requested functionally,
                    //  b. 1st response is NRC ResponsePending
                    //  c. wait
                    //  d. send correct response
                    if (UDSRequest.NETADDRINFO.TA_TYPE == TPUDSAddressingType.PUDS_ADDRESSING_FUNCTIONAL)
                    {

                        // Transmit a NRC ResponsePending response
                        Message.LEN = 3;
                        Message.DATA[0] = (byte)TPUDSService.PUDS_NR_SI;
                        Message.DATA[1] = SI;
                        Message.DATA[2] = UDSApi.PUDS_NRC_EXTENDED_TIMING;
                        Status = UDSApi.Write(Channel, ref Message);
                        log.Debug($"   ...Transmitting a NRC Response Pending message: {Status}");
                        log.Debug($"   ...simulating computation... (waiting ~{UDSApi.PUDS_P2CAN_ENHANCED_SERVER_MAX}ms)");
                        Thread.Sleep(UDSApi.PUDS_P2CAN_ENHANCED_SERVER_MAX - 100);
                        // initialize real service response
                        Message.LEN = UDSApi.PUDS_MAX_DATA;
                        Message.DATA[0] = (byte)(UDSRequest.DATA[0] + UDSApi.PUDS_SI_POSITIVE_RESPONSE);
                    }
                    Message.DATA[1] = UDSRequest.DATA[1];
                    // fill with dummy data
                    byte checksum = 0;
                    log.Debug("Calculating a checksum for the received data");
                    for (int i = 2; i < UDSRequest.LEN; i++)  // Start from 1 as byte 0 is the block sequence counter
                    {
                        checksum += UDSRequest.DATA[i];
                        //log.Debug($"Calculating checksum byte{i}={UDSRequest.DATA[i]}, checksum={checksum}");
                    }
                    log.Debug($"Checksum = 0x{checksum:X2}");
                    Message.LEN = 3;
                    Message.DATA[1] = UDSRequest.DATA[1];
                    Message.DATA[2] = checksum;
                    break;
                case TPUDSService.PUDS_SI_RequestTransferExit:
                    Message.LEN = UDSRequest.LEN;
                    // fill with dummy data
                    for (int i = 1; i < Message.LEN; i++)   // (LEN - 1) as POSITIVE.SI uses 1 byte
                        Message.DATA[i] = UDSRequest.DATA[i];
                    break;
                default:
                    // fill with dummy data
                    Message.LEN = (ushort)((1 + SI) > UDSApi.PUDS_MAX_DATA ? UDSApi.PUDS_MAX_DATA : (1 + SI));
                    Message.DATA[1] = SI;
                    for (int i = 1; i < Message.LEN - 1; i++)   // (LEN - 1) as POSITIVE.SI uses 1 byte
                        Message.DATA[i + 1] = (byte)(i + 1);
                    break;
            }

            // Transmit UDS response
            Status = UDSApi.Write(Channel, ref Message);
            log.Debug($"\n   ...Transmitting response: {Status}\n");
            UDSRequest = Message;
        }

        public static void Main(string[] args)
        {
            try
            {
                log.Info("Starting UDS Server Test");
                TPUDSStatus Status;
                TPUDSCANHandle Channel;
                int nbErr = 0;
                uint param = 0x0;
                byte serverAddr = (byte)TPUDSAddress.PUDS_ISO_15765_4_ADDR_ECU_1;
                StringBuilder buff = new StringBuilder();

                // Show version information
                UDSApi.GetValue(0, TPUDSParameter.PUDS_PARAM_API_VERSION, buff, 50);
                log.Debug($"PCAN-UDS API Version : {buff}");

                // Sets the default PCAN-Channel to use (PCAN-USB Channel 2)
                //
                Channel = UDSApi.PUDS_USBBUS2;

                // Sets server address and channel from application arguments if specified
                if (args.Length == 3)
                {
                    Channel = (TPCANHandle)(UDSApi.PUDS_USBBUS1 - 1 + TPCANHandle.Parse(args[1]));
                    serverAddr = byte.Parse(args[2]);
                }


                // Initializing of the UDS Communication session
                //
                Status = UDSApi.Initialize(Channel, TPUDSBaudrate.PUDS_BAUD_250K, 0, 0, 0);
                log.Info($"Initialize UDS: {Status} (chan. 0x{Channel:x2})");

                // Setting Debug trace for CAN channel
                //
                param = (uint)UDSApi.PUDS_DEBUG_CAN;
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_DEBUG, ref param, (uint)Marshal.SizeOf(param));
                log.Info($"Enable Debug Mode: {Status}");

                // Define server address and filtered address
                //
                param = serverAddr;
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_SERVER_ADDRESS, ref param, (uint)Marshal.SizeOf(param));
                log.Debug($"Set ServerAddress: {Status}");
                Status = UDSApi.GetValue(Channel, TPUDSParameter.PUDS_PARAM_SERVER_ADDRESS, out param, (uint)Marshal.SizeOf(param));
                log.Debug($"ServerAddress = 0x{param}");
                // listen to the standard OBD functional address
                param = (uint)TPUDSAddress.PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL | UDSApi.PUDS_SERVER_FILTER_LISTEN;
                Status = UDSApi.SetValue(Channel, TPUDSParameter.PUDS_PARAM_SERVER_FILTER, ref param, (uint)Marshal.SizeOf(param));
                log.Debug($"Set Filtered Address: {Status}");

                Console.WriteLine("Note: press 'c' or 'C' to clear screen,");
                Console.WriteLine("Note: press 'q', 'Q' or '<Escape>' to quit...");

                // Message Polling
                //
                TPUDSMsg Message;
                bool bStop = false;
                while (!bStop)
                {
                    // check message
                    Status = UDSApi.Read(Channel, out Message);
                    if (Status == TPUDSStatus.PUDS_ERROR_OK)
                    {
                        // display message
                        displayMessage(ref Message, Message.NETADDRINFO.SA != serverAddr);
                        // check if an automatic reply should be sent
                        if (Message.NETADDRINFO.SA != serverAddr    // do not reply to Tx Confirmation from this server
                            && Message.RESULT == TPUDSResult.PUDS_RESULT_N_OK)  // and reply if there is no Network error
                        {
                            // do not reply if it was requested not to
                            if (Message.NO_POSITIVE_RESPONSE_MSG == UDSApi.PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT)
                            {
                                log.Debug("   ...Skipping response...");
                            }
                            // do not reply to incoming message notification
                            else if (Message.MSGTYPE != TPUDSMessageType.PUDS_MESSAGE_TYPE_INDICATION)
                            {
                                transmitResponse(Channel, serverAddr, ref Message);
                            }
                        }
                    }
                    Thread.Sleep(1);
                    // check exit request
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey().KeyChar)
                        {
                            case 'q':
                            case 'Q':
                            case (char)27:    //Escape
                                bStop = true;
                                break;
                            case 'c':
                            case 'C':
                                Console.Clear();
                                Console.WriteLine($"ServerAddress = 0x{serverAddr:x2}");
                                Console.WriteLine("Note: press 'c' or 'C' to clear screen,");
                                Console.WriteLine("Note: press 'q', 'Q' or '<Escape>' to quit...");
                                break;
                        }
                    }
                }

                // Display a small report
                if (nbErr > 0)
                {
                    log.Warn($"ERROR : {nbErr} errors occured.");
                    Console.WriteLine("Press <Enter> to quit...");
                    Console.ReadKey(true);
                }

                // Release channel
                UDSApi.Uninitialize(Channel);
            }
            catch (Exception e)
            {
                log.Error("Unhandled Exception", e);
                log.Debug("Press <Enter> to quit...");
                Console.ReadKey(true);
            }
        }
    }
}