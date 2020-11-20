//  PCAN-UDS.cs
//
//  ~~~~~~~~~~~~
//
//  PCAN-UDS API
//
//  ~~~~~~~~~~~~
//
//  ------------------------------------------------------------------
//  Author : Fabrice Vergnaud
//	Last changed by:	$Author: Fabrice $
//  Last changed date:	$Date: 2019-10-11 14:05:33 +0200 (Fri, 11 Oct 2019) $
//
//  Language: C#
//  ------------------------------------------------------------------
//
//  Copyright (C) 2015  PEAK-System Technik GmbH, Darmstadt
//  more Info at http://www.peak-system.com 
//
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Peak.Can.Uds
{
    // Aliases definition
    //
    using TPUDSCANHandle = System.UInt16;     // Represents a CAN hardware channel of the underlying CAN system

    #region Enumerations
    /// <summary>
    /// Represents a PCAN Baud rate register value
    /// </summary>
    public enum TPUDSBaudrate : ushort
    {
        /// <summary>
        /// 1 MBit/s
        /// </summary>
        PUDS_BAUD_1M = 0x0014,
        /// <summary>
        /// 800 kBit/s
        /// </summary>
        PUDS_BAUD_800K = 0x0016,
        /// <summary>
        /// 500 kBit/s
        /// </summary>
        PUDS_BAUD_500K = 0x001C,
        /// <summary>
        /// 250 kBit/s
        /// </summary>
        PUDS_BAUD_250K = 0x011C,
        /// <summary>
        /// 125 kBit/s
        /// </summary>
        PUDS_BAUD_125K = 0x031C,
        /// <summary>
        /// 100 kBit/s
        /// </summary>
        PUDS_BAUD_100K = 0x432F,
        /// <summary>
        /// 95,238 kBit/s
        /// </summary>
        PUDS_BAUD_95K = 0xC34E,
        /// <summary>
        /// 83,333 kBit/s
        /// </summary>
        PUDS_BAUD_83K = 0x852B,
        /// <summary>
        /// 50 kBit/s
        /// </summary>
        PUDS_BAUD_50K = 0x472F,
        /// <summary>
        /// 47,619 kBit/s
        /// </summary>
        PUDS_BAUD_47K = 0x1414,
        /// <summary>
        /// 33,333 kBit/s
        /// </summary>
        PUDS_BAUD_33K = 0x8B2F,
        /// <summary>
        /// 20 kBit/s
        /// </summary>
        PUDS_BAUD_20K = 0x532F,
        /// <summary>
        /// 10 kBit/s
        /// </summary>
        PUDS_BAUD_10K = 0x672F,
        /// <summary>
        /// 5 kBit/s
        /// </summary>
        PUDS_BAUD_5K = 0x7F7F,
    }
    
    /// <summary>
    /// Represents the different Not Plug-And-Play PCAN Hardware types
    /// </summary>
    public enum TPUDSHWType : byte
    {
        /// <summary>
        /// PCAN-ISA 82C200
        /// </summary>
        PUDS_TYPE_ISA = 0x01,
        /// <summary>
        /// PCAN-ISA SJA1000
        /// </summary>
        PUDS_TYPE_ISA_SJA = 0x09,
        /// <summary>
        /// PHYTEC ISA 
        /// </summary>
        PUDS_TYPE_ISA_PHYTEC = 0x04,
        /// <summary>
        /// PCAN-Dongle 82C200
        /// </summary>
        PUDS_TYPE_DNG = 0x02,
        /// <summary>
        /// PCAN-Dongle EPP 82C200
        /// </summary>
        PUDS_TYPE_DNG_EPP = 0x03,
        /// <summary>
        /// PCAN-Dongle SJA1000
        /// </summary>
        PUDS_TYPE_DNG_SJA = 0x05,
        /// <summary>
        /// PCAN-Dongle EPP SJA1000
        /// </summary>
        PUDS_TYPE_DNG_SJA_EPP = 0x06,
    }
    
    /// <summary>
    /// Represent the PUDS error and status codes 
    /// </summary>
    public enum TPUDSStatus : uint
    {
        /// <summary>
        /// No error 
        /// </summary>
        PUDS_ERROR_OK = 0x00000,
        /// <summary>
        /// Not Initialized
        /// </summary>
        PUDS_ERROR_NOT_INITIALIZED = 0x00001,
        /// <summary>
        /// Already Initialized
        /// </summary>
        PUDS_ERROR_ALREADY_INITIALIZED = 0x00002,
        /// <summary>
        /// Could not obtain memory
        /// </summary>
        PUDS_ERROR_NO_MEMORY = 0x00003,
        /// <summary>
        /// Input buffer overflow
        /// </summary>
        PUDS_ERROR_OVERFLOW = 0x00004,
        /// <summary>
        /// Timeout while accessing the PCANTP mutex
        /// </summary>
        PUDS_ERROR_TIMEOUT = 0x00006,
        /// <summary>
        /// No Message available
        /// </summary>
        PUDS_ERROR_NO_MESSAGE = 0x00007,
        /// <summary>
        /// Wrong message parameters
        /// </summary>
        PUDS_ERROR_WRONG_PARAM = 0x00008,
        /// <summary>
        /// PCANTP Channel is in BUS-LIGHT error state
        /// </summary>
        PUDS_ERROR_BUSLIGHT = 0x00009,
        /// <summary>
        /// PCANTP Channel is in BUS-HEAVY error state
        /// </summary>
        PUDS_ERROR_BUSHEAVY = 0x0000A,
        /// <summary>
        /// PCANTP Channel is in BUS-OFF error state
        /// </summary>
        PUDS_ERROR_BUSOFF = 0x0000B,
        /// <summary>
        /// Global CAN error, status code for composition of PCANBasic Errors.
        ///	Remove this value to get a PCAN-Basic TPCANStatus error code.
        /// </summary>
        PUDS_ERROR_CAN_ERROR = 0x80000000,
    }

    /// <summary>
    /// Represents network result values as defined in ISO 15765-2
    /// </summary>
    public enum TPUDSResult : byte
    {
        /// <summary>
        /// No network error
        /// </summary>	
        PUDS_RESULT_N_OK = 0x00,
        /// <summary>
        /// timeout occured between 2 frames transmission (sender and receiver side)
        /// </summary>
        PUDS_RESULT_N_TIMEOUT_A = 0x01,
        /// <summary>
        /// sender side timeout while waiting for flow control frame
        /// </summary>
        PUDS_RESULT_N_TIMEOUT_BS = 0x02,
        /// <summary>
        /// receiver side timeout while waiting for consecutive frame
        /// </summary>
        PUDS_RESULT_N_TIMEOUT_CR = 0x03,
        /// <summary>
        /// unexpected sequence number
        /// </summary>
        PUDS_RESULT_N_WRONG_SN = 0x04,
        /// <summary>
        /// invalid or unknown FlowStatus
        /// </summary>
        PUDS_RESULT_N_INVALID_FS = 0x05,
        /// <summary>
        /// unexpected protocol data unit
        /// </summary>
        PUDS_RESULT_N_UNEXP_PDU = 0x06,
        /// <summary>
        /// reception of flow control WAIT frame that exceeds the maximum counter defined by PUDS_PARAM_WFT_MAX
        /// </summary>
        PUDS_RESULT_N_WFT_OVRN = 0x07,
        /// <summary>
        /// buffer on the receiver side cannot store the data length (server side only)
        /// </summary>
        PUDS_RESULT_N_BUFFER_OVFLW = 0x08,
        /// <summary>
        /// general error
        /// </summary>
        PUDS_RESULT_N_ERROR = 0x09,
    }

    /// <summary>
    /// PCANTP parameters
    /// </summary>
    public enum TPUDSParameter : byte
    {
        /// <summary>
        /// 2 BYTE data describing the physical address of the equipment
        /// </summary>
        PUDS_PARAM_SERVER_ADDRESS = 0xC1,
        /// <summary>
        /// 2 BYTE data (2 BYTE functional address and MSB for status)
        /// describing a functional address to ignore or listen to
        /// </summary>
        PUDS_PARAM_SERVER_FILTER = 0xC2,
        /// <summary>
        /// 4 BYTE data describing the maximum time allowed by the client to transmit a request 
        /// See ISO-15765-3 §6.3.2 : /\P2Can_Req
        /// </summary>
        PUDS_PARAM_TIMEOUT_REQUEST = 0xC3,
        /// <summary>
        /// 4 BYTE data describing the maximum time allowed by the client to receive a response
        /// See ISO-15765-3 §6.3.2 : /\P2Can_Rsp
        /// </summary>
        PUDS_PARAM_TIMEOUT_RESPONSE = 0xC4,
        /// <summary>
        /// Require a pointer to a TPUDSSessionInfo structure
        /// </summary>
        PUDS_PARAM_SESSION_INFO = 0xC5,
        /// <summary>
        /// API version parameter
        /// </summary>
        PUDS_PARAM_API_VERSION = 0xC6,
        /// <summary>
        /// Define UDS receive-event handler, require a pointer to an event HANDLE. 
        /// </summary>
        PUDS_PARAM_RECEIVE_EVENT = 0xC7,
		/// <summary>
		/// Define a new ISO-TP mapping, requires a pointer to TPUDSMsg containing 
		/// the mapped CAN ID and CAN ID response in the DATA.RAW field.
		/// </summary>
		PUDS_PARAM_MAPPING_ADD = 0xC8,
		/// <summary>
		/// Remove an ISO-TP mapping, requires a pointer to TPUDSMsg containing the mapped CAN ID to remove. 
		/// </summary>
		PUDS_PARAM_MAPPING_REMOVE = 0xC9,

        /// <summary>
        /// 1 BYTE data describing the block size parameter (BS)
        /// </summary>
        PUDS_PARAM_BLOCK_SIZE = 0xE1,
        /// <summary>
        /// 1 BYTE data describing the seperation time parameter (STmin)
        /// </summary>
        PUDS_PARAM_SEPERATION_TIME = 0xE2,
        /// <summary>
        /// 1 BYTE data describing the debug mode 
        /// </summary>
        PUDS_PARAM_DEBUG = 0xE3,
        /// <summary>
        /// 1 Byte data describing the condition of a channel
        /// </summary>
        PUDS_PARAM_CHANNEL_CONDITION = 0xE4,
        /// <summary>
        /// Integer data describing the Wait Frame Transmissions parameter. 
        /// </summary>
        PUDS_PARAM_WFT_MAX = 0xE5,
        /// <summary>
        /// 1 BYTE data stating if CAN frame DLC uses padding or not
        /// </summary>
        PUDS_PARAM_CAN_DATA_PADDING = 0xE8,
        /// <summary>
        /// 1 BYTE data stating the value used for CAN data padding
        /// </summary>
        PUDS_PARAM_PADDING_VALUE = 0xED,
    }

    /// <summary>
    /// PUDS Service IDs defined in ISO 14229-1
    /// </summary>
    public enum TPUDSService : byte
    {
        PUDS_SI_DiagnosticSessionControl = 0x10,
        PUDS_SI_ECUReset = 0x11,
        PUDS_SI_SecurityAccess = 0x27,
        PUDS_SI_CommunicationControl = 0x28,
        PUDS_SI_TesterPresent = 0x3E,
        PUDS_SI_AccessTimingParameter = 0x83,
        PUDS_SI_SecuredDataTransmission = 0x84,
        PUDS_SI_ControlDTCSetting = 0x85,
        PUDS_SI_ResponseOnEvent = 0x86,
        PUDS_SI_LinkControl = 0x87,
        PUDS_SI_ReadDataByIdentifier = 0x22,
        PUDS_SI_ReadMemoryByAddress = 0x23,
        PUDS_SI_ReadScalingDataByIdentifier = 0x24,
        PUDS_SI_ReadDataByPeriodicIdentifier = 0x2A,
        PUDS_SI_DynamicallyDefineDataIdentifier = 0x2C,
        PUDS_SI_WriteDataByIdentifier = 0x2E,
        PUDS_SI_WriteMemoryByAddress = 0x3D,
        PUDS_SI_ClearDiagnosticInformation = 0x14,
        PUDS_SI_ReadDTCInformation = 0x19,
        PUDS_SI_InputOutputControlByIdentifier = 0x2F,
        PUDS_SI_RoutineControl = 0x31,
        PUDS_SI_RequestDownload = 0x34,
        PUDS_SI_RequestUpload = 0x35,
        PUDS_SI_TransferData = 0x36,
        PUDS_SI_RequestTransferExit = 0x37,
        /// <summary>
        /// Negative response code
        /// </summary>
        PUDS_NR_SI = 0x7f,
    }

    /// <summary>
    /// PUDS ISO_15765_4 address definitions
    /// </summary>
    public enum TPUDSAddress : byte
    {
        /// <summary>
        /// External test equipment
        /// </summary>
        PUDS_ISO_15765_4_ADDR_TEST_EQUIPMENT = 0xF1,
        /// <summary>
        /// OBD funtional system
        /// </summary>
        PUDS_ISO_15765_4_ADDR_OBD_FUNCTIONAL = 0x33,
        /// <summary>
        /// ECU 1
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_1 = 0x01,
        /// <summary>
        /// ECU 2
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_2 = 0x02,
        /// <summary>
        /// ECU 3
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_3 = 0x03,
        /// <summary>
        /// ECU 4
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_4 = 0x04,
        /// <summary>
        /// ECU 5
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_5 = 0x05,
        /// <summary>
        /// ECU 6
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_6 = 0x06,
        /// <summary>
        /// ECU 7
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_7 = 0x07,
        /// <summary>
        /// ECU 8
        /// </summary>
        PUDS_ISO_15765_4_ADDR_ECU_8 = 0x08,
    }
    
    /// <summary>
    /// PUDS ISO_15765_4 11 bit CAN Identifier
    /// </summary>
    public enum TPUDSCanId : uint
    {
        /// <summary>
        /// CAN ID for functionally addressed request messages sent by external test equipment
        /// </summary>        
        PUDS_ISO_15765_4_CAN_ID_FUNCTIONAL_REQUEST = 0x7DF,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #1
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_1 = 0x7E0,
        /// <summary>
        /// physical response CAN ID from ECU #1 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_1 = 0x7E8,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #2
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_2 = 0x7E1,
        /// <summary>
        /// physical response CAN ID from ECU #2 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_2 = 0x7E9,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #3
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_3 = 0x7E2,
        /// <summary>
        /// physical response CAN ID from ECU #3 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_3 = 0x7EA,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #4
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_4 = 0x7E3,
        /// <summary>
        /// physical response CAN ID from ECU #4 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_4 = 0x7EB,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #5
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_5 = 0x7E4,
        /// <summary>
        /// physical response CAN ID from ECU #5 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_5 = 0x7EC,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #6
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_6 = 0x7E5,
        /// <summary>
        /// physical response CAN ID from ECU #6 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_6 = 0x7ED,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #7
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_7 = 0x7E6,
        /// <summary>
        /// physical response CAN ID from ECU #7 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_7 = 0x7EE,
        /// <summary>
        /// physical request CAN ID from external test equipment to ECU #8
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_REQUEST_8 = 0x7E7,
        /// <summary>
        /// physical response CAN ID from ECU #8 to external test equipment
        /// </summary>
        PUDS_ISO_15765_4_CAN_ID_PHYSICAL_RESPONSE_8 = 0x7EF,
    }
        
    /// <summary>
    /// PUDS Protocol ISO-15765 definitions
    /// </summary>
    public enum TPUDSProtocol : byte
    {
        /// <summary>
        /// non ISO-TP frame (Unacknowledge Unsegmented Data Transfer)
        /// </summary>
	    PUDS_PROTOCOL_NONE = 0x00,
        /// <summary>
        /// using PCAN-ISO-TP with 11 BIT CAN ID, NORMAL addressing and diagnostic message
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_11B = 0x01,
        /// <summary>
        /// using PCAN-ISO-TP with 11 BIT CAN ID, MIXED addressing and remote diagnostic message
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_11B_REMOTE = 0x02,
        /// <summary>
        /// using PCAN-ISO-TP with 29 BIT CAN ID, FIXED NORMAL addressing and diagnostic message
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_29B = 0x03,
        /// <summary>
        /// using PCAN-ISO-TP with 29 BIT CAN ID, MIXED addressing and remote diagnostic message
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_29B_REMOTE = 0x04,
        /// <summary>
        /// using PCAN-ISO-TP with Enhanced diagnostics 29 bit CAN Identifiers
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_3_29B = 0x05,
        /// <summary>
        /// using PCAN-ISO-TP with 29 BIT CAN ID, NORMAL addressing and diagnostic message
        /// Note: this protocol requires extra mapping definitions via PCAN-ISO-TP API
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_29B_NORMAL = 0x06,
        /// <summary>
        /// using PCAN-ISO-TP with 11 BIT CAN ID, EXTENDED addressing and diagnostic message
        /// Note: this protocol requires extra mapping definitions via PCAN-ISO-TP API
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_11B_EXTENDED = 0x07,
        /// <summary>
        /// using PCAN-ISO-TP with 29 BIT CAN ID, EXTENDED addressing and diagnostic message
        /// Note: this protocol requires extra mapping definitions via PCAN-ISO-TP API
        /// </summary>
        PUDS_PROTOCOL_ISO_15765_2_29B_EXTENDED = 0x08,
    }

    /// <summary>
    /// PUDS addressing type
    /// </summary>
    public enum TPUDSAddressingType : byte
    {
        /// <summary>
        /// Physical addressing
        /// </summary>
        PUDS_ADDRESSING_PHYSICAL = 0x01,
        /// <summary>
        /// Functional addressing
        /// </summary>
        PUDS_ADDRESSING_FUNCTIONAL = 0x02,
    }
        
    /// <summary>
    /// PCANTP message types
    /// </summary>
    public enum TPUDSMessageType : byte
    {
        /// <summary>
        /// UDS Request Message
        /// </summary>
        PUDS_MESSAGE_TYPE_REQUEST = 0x00,
        /// <summary>
        /// UDS Request/Response confirmation  Message
        /// </summary>
        PUDS_MESSAGE_TYPE_CONFIRM = 0x01,
        /// <summary>
        /// Incoming UDS Message
        /// </summary>
        PUDS_MESSAGE_TYPE_INDICATION = 0x02,
        /// <summary>
        /// UDS Message transmission started
        /// </summary>
        PUDS_MESSAGE_TYPE_INDICATION_TX = 0x03,
		/// <summary>
		/// Unacknowledge Unsegmented Data Transfert
		/// </summary>
		PUDS_MESSAGE_TYPE_CONFIRM_UUDT = 0x04,
    }

    /// <summary>
    /// PUDS Service Result
    /// </summary>
    public enum TPUDSServiceResult : byte
    {
        /// <summary>
        /// Response is valid and matches the requested Service ID.
        /// </summary>
        Confirmed = 0x00,
        /// <summary>
        /// Response is valid but an Negative Response Code was replied.
        /// </summary>
        NRC = 0x01,
        /// <summary>
        /// A network error occured in the ISO-TP layer.
        /// </summary>
        NetworkError = 0x02,
        /// <summary>
        /// Response does not match the requested UDS Service.
        /// </summary>
        ServiceMismatch = 0x03,
        /// <summary>
        /// Generic error, the message is not a valid response.
        /// </summary>
        GenericError = 0x04,
    }
    #endregion

    #region Structures
    /// <summary>
    /// PCAN-UDS Network Addressing Information
    /// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct TPUDSNetAddrInfo
    {   
        /// <summary>
        /// Represents the origin of this message (address from 
        /// where this message was or will be sent)
        /// </summary>
        public byte SA;
        /// <summary>
        /// Represents the destination of this message (address to 
        /// where this message was or will be sent)
        /// </summary>
        public byte TA;
        /// <summary>
        /// Represents the kind of addressing being used for communication
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public TPUDSAddressingType TA_TYPE;
        /// <summary>
        /// Represents the destination of this message in a remote network 
        /// </summary>
        public byte RA;
        /// <summary>
        /// Represents the protocol being used for communication
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public TPUDSProtocol PROTOCOL;
    }

    /// <summary>
    /// PCAN-UDS Diagnostic Session Information of a server
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct TPUDSSessionInfo
    {          
        /// <summary>
        /// Network address information
        /// </summary>
	    public TPUDSNetAddrInfo NETADDRINFO;
        /// <summary>
        /// Activated Diagnostic Session (see PUDS_SVC_PARAM_DSC_xxx values)
        /// </summary>
	    public byte SESSION_TYPE;
        /// <summary>
        /// Default P2Can_Server_Max timing for the activated session
        /// </summary>
	    public ushort TIMEOUT_P2CAN_SERVER_MAX;
        /// <summary>
        /// Enhanced P2Can_Server_Max timing for the activated session
        /// </summary>
	    public ushort TIMEOUT_ENHANCED_P2CAN_SERVER_MAX;
    }
    
	/// <summary>
	/// PCAN-UDS Message
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct TPUDSMsg
    {   
        /// <summary>
        /// Network Addressing Information
        /// </summary>
	    public TPUDSNetAddrInfo NETADDRINFO;
        /// <summary>
        /// Result status of the network communication
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
	    public TPUDSResult RESULT;
        /// <summary>
        /// States wether Positive Response Message should be suppressed.
        /// See constants PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT & PUDS_KEEP_POS_RSP_MSG_INDICATION_BIT
        /// </summary>
	    public byte NO_POSITIVE_RESPONSE_MSG;
        /// <summary>
        /// Data Length of the message
        /// </summary>
	    public ushort LEN;        
        /// <summary>
        /// Type of UDS Message
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
	    public TPUDSMessageType MSGTYPE;
        /// <summary>
        /// Represents the buffer containing the data of this message
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4095)]
        public byte[] DATA;


        /// <summary>
        /// Indicates if this message represents a valid Response
        /// </summary>
        [Obsolete("IsPositiveResponse is deprecated, please use IsValidResponse instead.")]
        public bool IsPositiveResponse
        {
            get
            {  
                return IsValidResponse;
            }
        }
        /// <summary>
        /// Indicates if this message represents a valid Response
        /// </summary>
        public bool IsValidResponse
        {
            get
            {
                if (DATA != null)
                    return (DATA[0] & 0x40) == 0x40;
                return false;
            }
        }

        /// <summary>
        /// Indicates if this message represents a Negative-Response
        /// </summary>
        public bool IsNegativeResponse
        {
            get 
            {
                if (DATA != null)
                    return DATA[0] == 0x7F;
                return false;
            }
        }

        /// <summary>
        /// Shows the data-byte representing the Service-ID of this message
        /// </summary>
        public byte ServiceID
        {
            get 
            {
                if (DATA != null)
                    return IsNegativeResponse ? DATA[1] : DATA[0];
                return 0;
            }
        }

        /// <summary>
        /// Checks if a UDS message is a valid response to a request with the specified Service ID.
        /// </summary>
        /// <param name="serviceId">Service ID of the request.</param>
        /// <param name="nrc">If the response is valid but indicates a UDS error, this parameter will hold the UDS Negative Response Code.</param>
        /// <returns>Status indicating if the message is confirmed as a positive response.</returns>
        public TPUDSServiceResult CheckResponse(TPUDSService serviceId, out byte nrc)
        {
            nrc = 0;
            if (RESULT != TPUDSResult.PUDS_RESULT_N_OK)
                return TPUDSServiceResult.NetworkError;
            if (IsNegativeResponse)
            {
                if (DATA[1] != (byte)serviceId)
                    return TPUDSServiceResult.ServiceMismatch;
                nrc = DATA[2];
                return TPUDSServiceResult.NRC;
            }
            else if (IsPositiveResponse)
            {
                if (DATA[0] != ((byte)serviceId | 0x40))
                    return TPUDSServiceResult.ServiceMismatch;
                return TPUDSServiceResult.Confirmed;
            }
            return TPUDSServiceResult.GenericError;
        }
    }
    #endregion

    #region PCAN UDS Api
    public static class UDSApi
    {
        #region PCAN-BUS Handles Definition
        /// <summary>
        /// Undefined/default value for a PCAN bus
        /// </summary>
        public const TPUDSCANHandle PUDS_NONEBUS = 0x00;

        /// <summary>
        /// PCAN-ISA interface, channel 1
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS1 = 0x21;
        /// <summary>
        /// PCAN-ISA interface, channel 2
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS2 = 0x22;
        /// <summary>
        /// PCAN-ISA interface, channel 3
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS3 = 0x23;
        /// <summary>
        /// PCAN-ISA interface, channel 4
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS4 = 0x24;
        /// <summary>
        /// PCAN-ISA interface, channel 5
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS5 = 0x25;
        /// <summary>
        /// PCAN-ISA interface, channel 6
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS6 = 0x26;
        /// <summary>
        /// PCAN-ISA interface, channel 7
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS7 = 0x27;
        /// <summary>
        /// PCAN-ISA interface, channel 8
        /// </summary>
        public const TPUDSCANHandle PUDS_ISABUS8 = 0x28;

        /// <summary>
        /// PPCAN-Dongle/LPT interface, channel 1 
        /// </summary>
        public const TPUDSCANHandle PUDS_DNGBUS1 = 0x31;

        /// <summary>
        /// PCAN-PCI interface, channel 1
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS1 = 0x41;
        /// <summary>
        /// PCAN-PCI interface, channel 2
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS2 = 0x42;
        /// <summary>
        /// PCAN-PCI interface, channel 3
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS3 = 0x43;
        /// <summary>
        /// PCAN-PCI interface, channel 4
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS4 = 0x44;
        /// <summary>
        /// PCAN-PCI interface, channel 5
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS5 = 0x45;
        /// <summary>
        /// PCAN-PCI interface, channel 6
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS6 = 0x46;
        /// <summary>
        /// PCAN-PCI interface, channel 7
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS7 = 0x47;
        /// <summary>
        /// PCAN-PCI interface, channel 8
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS8 = 0x48;
        /// <summary>
        /// PCAN-PCI interface, channel 9
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS9 = 0x409;
        /// <summary>
        /// PCAN-PCI interface, channel 10
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS10 = 0x40A;
        /// <summary>
        /// PCAN-PCI interface, channel 11
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS11 = 0x40B;
        /// <summary>
        /// PCAN-PCI interface, channel 12
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS12 = 0x40C;
        /// <summary>
        /// PCAN-PCI interface, channel 13
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS13 = 0x40D;
        /// <summary>
        /// PCAN-PCI interface, channel 14
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS14 = 0x40E;
        /// <summary>
        /// PCAN-PCI interface, channel 15
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS15 = 0x40F;
        /// <summary>
        /// PCAN-PCI interface, channel 16
        /// </summary>
        public const TPUDSCANHandle PUDS_PCIBUS16 = 0x410;

        /// <summary>
        /// PCAN-USB interface, channel 1
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS1 = 0x51;
        /// <summary>
        /// PCAN-USB interface, channel 2
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS2 = 0x52;
        /// <summary>
        /// PCAN-USB interface, channel 3
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS3 = 0x53;
        /// <summary>
        /// PCAN-USB interface, channel 4
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS4 = 0x54;
        /// <summary>
        /// PCAN-USB interface, channel 5
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS5 = 0x55;
        /// <summary>
        /// PCAN-USB interface, channel 6
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS6 = 0x56;
        /// <summary>
        /// PCAN-USB interface, channel 7
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS7 = 0x57;
        /// <summary>
        /// PCAN-USB interface, channel 8
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS8 = 0x58;
        /// <summary>
        /// PCAN-USB interface, channel 9
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS9 = 0x509;
        /// <summary>
        /// PCAN-USB interface, channel 10
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS10 = 0x50A;
        /// <summary>
        /// PCAN-USB interface, channel 11
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS11 = 0x50B;
        /// <summary>
        /// PCAN-USB interface, channel 12
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS12 = 0x50C;
        /// <summary>
        /// PCAN-USB interface, channel 13
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS13 = 0x50D;
        /// <summary>
        /// PCAN-USB interface, channel 14
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS14 = 0x50E;
        /// <summary>
        /// PCAN-USB interface, channel 15
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS15 = 0x50F;
        /// <summary>
        /// PCAN-USB interface, channel 16
        /// </summary>
        public const TPUDSCANHandle PUDS_USBBUS16 = 0x510;

        /// <summary>
        /// PCAN-PC Card interface, channel 1
        /// </summary>
        public const TPUDSCANHandle PUDS_PCCBUS1 = 0x61;
        /// <summary>
        /// PCAN-PC Card interface, channel 2
        /// </summary>
        public const TPUDSCANHandle PUDS_PCCBUS2 = 0x62;

        /// <summary>
        /// PCAN-LAN interface, channel 1
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS1 = 0x801;
        /// <summary>
        /// PCAN-LAN interface, channel 2
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS2 = 0x802;
        /// <summary>
        /// PCAN-LAN interface, channel 3
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS3 = 0x803;
        /// <summary>
        /// PCAN-LAN interface, channel 4
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS4 = 0x804;
        /// <summary>
        /// PCAN-LAN interface, channel 5
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS5 = 0x805;
        /// <summary>
        /// PCAN-LAN interface, channel 6
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS6 = 0x806;
        /// <summary>
        /// PCAN-LAN interface, channel 7
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS7 = 0x807;
        /// <summary>
        /// PCAN-LAN interface, channel 8
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS8 = 0x808;
        /// <summary>
        /// PCAN-LAN interface, channel 9
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS9 = 0x809;
        /// <summary>
        /// PCAN-LAN interface, channel 10
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS10 = 0x80A;
        /// <summary>
        /// PCAN-LAN interface, channel 11
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS11 = 0x80B;
        /// <summary>
        /// PCAN-LAN interface, channel 12
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS12 = 0x80C;
        /// <summary>
        /// PCAN-LAN interface, channel 13
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS13 = 0x80D;
        /// <summary>
        /// PCAN-LAN interface, channel 14
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS14 = 0x80E;
        /// <summary>
        /// PCAN-LAN interface, channel 15
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS15 = 0x80F;
        /// <summary>
        /// PCAN-LAN interface, channel 16
        /// </summary>
        public const TPUDSCANHandle PUDS_LANBUS16 = 0x810;
        #endregion

        #region Parameter values definition
        /// <summary>
        /// No debug messages
        /// </summary>
        public const byte PUDS_DEBUG_NONE = 0;
        /// <summary>
        /// Puts CAN debug messages to stdout
        /// </summary>
        public const byte PUDS_DEBUG_CAN = 1;
        /// <summary>
        /// The Channel is illegal or not available
        /// </summary>
        public const byte PUDS_CHANNEL_UNAVAILABLE = 0;
        /// <summary>
        /// The Channel is available
        /// </summary>
        public const byte PUDS_CHANNEL_AVAILABLE = 1;
        /// <summary>
        /// The Channel is valid, and is being used
        /// </summary>
        public const byte PUDS_CHANNEL_OCCUPIED = 2;

        /// <summary>
        /// Physical address for external test equipment
        /// </summary>
        public const byte PUDS_SERVER_ADDR_TEST_EQUIPMENT = 0xF1;
        /// <summary>
        /// Functional request address for Legislated OBD system
        /// </summary>
        public const byte PUDS_SERVER_ADDR_REQUEST_OBD_SYSTEM = 0x33;
        /// <summary>
        /// Flag stating that the address is defined as a ISO-15765-3 address
        /// </summary>
        public const ushort PUDS_SERVER_ADDR_FLAG_ENHANCED_ISO_15765_3 = 0x1000;
        /// <summary>
        /// Mask used for the ISO-15765-3 enhanced addresses
        /// </summary>
        public const ushort PUDS_SERVER_ADDR_MASK_ENHANCED_ISO_15765_3 = 0x07FF;
        /// <summary>
        /// Filter status : ignore (used to remove previously set filter)
        /// </summary>
        public const ushort PUDS_SERVER_FILTER_IGNORE = 0x0000;
        /// <summary>
        /// Filter status : listen to (must be OR'ed with the 2 BYTE functional address)
        /// </summary>
        public const ushort PUDS_SERVER_FILTER_LISTEN = 0x8000;
        /// <summary>
        /// Default maximum timeout for UDS transmit confirmation
        /// </summary>
        public const UInt32 PUDS_TIMEOUT_REQUEST = 10000;
        /// <summary>
        /// Default maximum timeout for UDS response reception
        /// </summary>
        public const UInt32 PUDS_TIMEOUT_RESPONSE = 10000;
        /// <summary>
        /// Default server performance requirement (See ISO-15765-3 §6.3.2)
        /// </summary>
        public const ushort PUDS_P2CAN_DEFAULT_SERVER_MAX = 50;
        /// <summary>
        /// Enhanced server performance requirement (See ISO-15765-3 §6.3.2)
        /// </summary>
        public const ushort PUDS_P2CAN_ENHANCED_SERVER_MAX = 5000;        
		/// <summary>
		/// Uses CAN frame data optimization
		/// </summary>
        public const ushort PUDS_CAN_DATA_PADDING_NONE = 0x00;
        /// <summary>
        /// Uses CAN frame data padding (default, i.e. CAN DLC = 8)
        /// </summary>
        public const ushort PUDS_CAN_DATA_PADDING_ON = 0x01;
        /// <summary>
        /// Default value used if CAN data padding is enabled
        /// </summary>
        public const ushort PUDS_CAN_DATA_PADDING_VALUE = 0x55;
        #endregion

        #region Values definition related to UDS Message
        /// <summary>
        /// Maximum data length  of UDS messages
        /// </summary>
        public const ushort PUDS_MAX_DATA = 4095;
        /// <summary>
        /// Value (for member NO_POSITIVE_RESPONSE_MSG) stating to suppress positive response messages
        /// </summary>
        public const byte PUDS_SUPPR_POS_RSP_MSG_INDICATION_BIT = 0x80;
        /// <summary>
        /// Default Value (for member NO_POSITIVE_RESPONSE_MSG) stating to keep positive response messages
        /// </summary>
        public const byte PUDS_KEEP_POS_RSP_MSG_INDICATION_BIT = 0x00;
        /// <summary>
        /// Negative response code: Server wants more time
        /// </summary>
        public const byte PUDS_NRC_EXTENDED_TIMING = 0x78;
        /// <summary>
        /// Positive response offset
        /// </summary>
        public const byte PUDS_SI_POSITIVE_RESPONSE = 0x40;
        #endregion
                
        #region PCAN UDS API Implementation
        /// <summary>
        /// Initializes a PUDS-Client based on a PUDS-Channel
        /// </summary>
        /// <remarks>Only one UDS-Client can be initialized per CAN-Channel</remarks>
        /// <param name="CanChannel">The PCAN-Basic channel to be used as UDS client</param>
        /// <param name="Baudrate">The CAN Hardware speed</param>
        /// <param name="HwType">NON PLUG&PLAY: The type of hardware and operation mode</param>
        /// <param name="IOPort">NON PLUG&PLAY: The I/O address for the parallel port</param>
        /// <param name="Interrupt">NON PLUG&PLAY: Interrupt number of the parallel port</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_Initialize")]
        public static extern TPUDSStatus Initialize(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U2)]
            TPUDSBaudrate Baudrate,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSHWType HwType,
            UInt32 IOPort,
            UInt16 Interrupt);

        /// <summary>
        /// Initializes a PUDS-Client based on a PUDS-Channel
        /// </summary>
        /// <remarks>Only one UDS-Client can be initialized per CAN-Channel</remarks>
        /// <param name="CanChannel">The PCAN-Basic channel to be used as UDS client</param>
        /// <param name="Baudrate">The CAN Hardware speed</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        public static TPUDSStatus Initialize(
            TPUDSCANHandle CanChannel,
            TPUDSBaudrate Baudrate)
        {
            return Initialize(CanChannel, Baudrate, (TPUDSHWType)0, 0, 0);
        }

        /// <summary>
        /// Uninitializes a PUDS-Client initialized before
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_Uninitialize")]
        public static extern TPUDSStatus Uninitialize(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel);

        /// <summary>
        /// Resets the receive and transmit queues of a PUDS-Client 
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_Reset")]
        public static extern TPUDSStatus Reset(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel);

        /// <summary>
        /// Gets information about the internal BUS status of a PUDS CAN-Channel.
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_GetStatus")]
        public static extern TPUDSStatus GetStatus(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel);

        /// <summary>
        /// Reads a PUDS message from the receive queue of a PUDS-Client
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="MessageBuffer">A TPUDSMsg structure buffer to store the PUDS message</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_Read")]
        public static extern TPUDSStatus Read(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            out TPUDSMsg MessageBuffer);

        /// <summary>
        /// Transmits a PUDS message
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="MessageBuffer">A TPUDSMsg buffer with the message to be sent</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_Write")]
        public static extern TPUDSStatus Write(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer);


        /// <summary>
        /// Retrieves a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to get</param>
        /// <param name="StringBuffer">Buffer for the parameter value</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_GetValue")]
        public static extern TPUDSStatus GetValue(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSParameter Parameter,
            StringBuilder StringBuffer,
            UInt32 BufferLength);
        /// <summary>
        /// Retrieves a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to get</param>
        /// <param name="NumericBuffer">Buffer for the parameter value</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_GetValue")]
        public static extern TPUDSStatus GetValue(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSParameter Parameter,
            out UInt32 NumericBuffer,
            UInt32 BufferLength);
        /// <summary>
        /// Retrieves a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to get</param>
        /// <param name="Buffer">Buffer for the parameter value</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_GetValue")]
        public static extern TPUDSStatus GetValue(
            [MarshalAs(UnmanagedType.U2)]
			TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
			TPUDSParameter Parameter,
            [MarshalAs(UnmanagedType.LPArray)]
			[Out] Byte[] Buffer,
            UInt32 BufferLength);
        /// <summary>
        /// Retrieves a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to get</param>
        /// <param name="Buffer">Buffer for the parameter value</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_GetValue")]
        public static extern TPUDSStatus GetValue(
            [MarshalAs(UnmanagedType.U2)]
			TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
			TPUDSParameter Parameter,
            IntPtr Buffer,
            UInt32 BufferLength);
        
        /// <summary>
        /// Configures or sets a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to set</param>
        /// <param name="NumericBuffer">Buffer with the value to be set</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SetValue")]
        public static extern TPUDSStatus SetValue(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSParameter Parameter,
            ref UInt32 NumericBuffer,
            UInt32 BufferLength);
        /// <summary>
        /// Configures or sets a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to set</param>
        /// <param name="StringBuffer">Buffer with the value to be set</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SetValue")]
        public static extern TPUDSStatus SetValue(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSParameter Parameter,
            [MarshalAs(UnmanagedType.LPStr, SizeParamIndex = 3)]
            string StringBuffer,
            UInt32 BufferLength);
        /// <summary>
        /// Configures or sets a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to set</param>
        /// <param name="Buffer">Buffer with the value to be set</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SetValue")]
        public static extern TPUDSStatus SetValue(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
            TPUDSParameter Parameter,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
			Byte[] Buffer,
            UInt32 BufferLength);
        /// <summary>
        /// Configures or sets a PUDS-Client value
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel Handle representing a PUDS-Client</param>
        /// <param name="Parameter">The TPUDSParameter parameter to set</param>
        /// <param name="Buffer">Buffer with the value to be set</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SetValue")]
        public static extern TPUDSStatus SetValue(
            [MarshalAs(UnmanagedType.U2)]
			TPUDSCANHandle CanChannel,
            [MarshalAs(UnmanagedType.U1)]
			TPUDSParameter Parameter,
            IntPtr Buffer,
            UInt32 BufferLength);
        #endregion

        #region PCAN UDS API Implementation : Service handlers
        /// <summary>
        /// Waits for a message (a response or a transmit confirmation) based on a UDS Message Request.
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="MessageBuffer">A TPUDSMsg structure buffer to store the PUDS response</param>
        /// <param name="MessageRequest">A sent TPUDSMsg message</param>
        /// <param name="IsWaitForTransmit">The message to wait for is a Transmit Confirmation or not</param>
        /// <param name="TimeInterval">Time interval to check for new message</param>
        /// <param name="Timeout">Maximum time to wait for the message</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_WaitForSingleMessage")]
        public static extern TPUDSStatus WaitForSingleMessage(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            out TPUDSMsg MessageBuffer,
            ref TPUDSMsg MessageRequest,
            bool IsWaitForTransmit,
            UInt32 TimeInterval,
            UInt32 Timeout);

        /// <summary>
        /// Waits for multiple messages (multiple responses from a functional request for instance) based on a UDS Message Request.
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="Buffer">Buffer must be an array of 'MaxCount' entries (must have at least 
        /// a size of iMaxCount * sizeof(TPUDSMsg) bytes</param>
        /// <param name="MaxCount">Size of the Buffer array (max. messages that can be received)</param>
        /// <param name="pCount">Buffer for the real number of messages read</param>
        /// <param name="MessageRequest">A sent TPUDSMsg message</param>
        /// <param name="TimeInterval">Time interval to check for new message</param>
        /// <param name="Timeout">Maximum time to wait for the message</param>
        /// <param name="TimeoutEnhanced">Maximum time to wait for the message in UDS Enhanced mode</param>
        /// <param name="WaitUntilTimeout">if <code>FALSE</code> the function is interrupted if pCount reaches MaxCount.</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success, 
        ///	PUDS_ERROR_OVERFLOW indicates success but Buffer was too small to hold all responses.</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_WaitForMultipleMessage")]
        public static extern TPUDSStatus WaitForMultipleMessage(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [In, Out] 
            TPUDSMsg[] Buffer,
            UInt32 MaxCount,
            out UInt32 pCount,
            ref TPUDSMsg MessageRequest,
            UInt32 TimeInterval,
            UInt32 Timeout,
            UInt32 TimeoutEnhanced,
            bool WaitUntilTimeout);

        /// <summary>
        /// Handles the communication workflow for a UDS service expecting a single response.
        /// </summary>
        ///	<remark>
        ///	The function waits for a transmit confirmation then for a message response.
        ///	Even if the SuppressPositiveResponseMessage flag is set, the function will still wait 
        /// for an eventual Negative Response.
        ///	</remark>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="MessageBuffer">A TPUDSMsg structure buffer to store the PUDS response</param>
        /// <param name="MessageRequest">A sent TPUDSMsg message</param>
        /// <param name="MessageReqBuffer">A TPUDSMsg structure buffer to store the PUDS request confirmation 
        ///	(if <code>NULL</code>, the result confirmation will be set in MessageRequest parameter)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_WaitForService")]
        public static extern TPUDSStatus WaitForService(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            out TPUDSMsg MessageBuffer,
            ref TPUDSMsg MessageRequest,
            out TPUDSMsg MessageReqBuffer);

        /// <summary>
        /// Handles the communication workflow for a UDS service expecting multiple responses.
        /// </summary>
        ///	<remark>
        ///	The function waits for a transmit confirmation then for N message responses.
        ///	Even if the SuppressPositiveResponseMessage flag is set, the function will still wait 
        /// for eventual Negative Responses.
        ///	</remark>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="Buffer">Buffer must be an array of 'MaxCount' entries (must have at least 
        /// a size of iMaxCount * sizeof(TPUDSMsg) bytes</param>
        /// <param name="MaxCount">Size of the Buffer array (max. messages that can be received)</param>
        /// <param name="pCount">Buffer for the real number of messages read</param>
        /// <param name="WaitUntilTimeout">if <code>FALSE</code> the function is interrupted if pCount reaches MaxCount.</param>
        /// <param name="MessageRequest">A sent TPUDSMsg message</param>
        /// <param name="MessageReqBuffer">A TPUDSMsg structure buffer to store the PUDS request confirmation 
        ///	(if <code>NULL</code>, the result confirmation will be set in MessageRequest parameter)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success, 
        ///	PUDS_ERROR_OVERFLOW indicates success but Buffer was too small to hold all responses.</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_WaitForServiceFunctional")]
        public static extern TPUDSStatus WaitForServiceFunctional(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            [In, Out] 
            TPUDSMsg[] Buffer,
            UInt32 MaxCount,
            out UInt32 pCount,
            bool WaitUntilTimeout,
            ref TPUDSMsg MessageRequest, 		
            out TPUDSMsg MessageReqBuffer);

        /// <summary>
        /// Process a UDS response message to manage ISO-14229/15765 features (like session handling).
        /// </summary>
        /// <param name="CanChannel">A PUDS CAN-Channel representing a PUDS-Client</param>
        /// <param name="MessageBuffer">A TPUDSMsg structure buffer representing the PUDS Response Message</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_ProcessResponse")]
        public static extern TPUDSStatus ProcessResponse(
            [MarshalAs(UnmanagedType.U2)]
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer);
        #endregion

        #region PCAN UDS API Implementation : Services

        #region UDS Service: DiagnosticSessionControl
        // ISO-15765-3:2004 §9.2.1 p.42 & ISO-14229-1:2006 §9.2 p.36

        /// <summary>
        /// Subfunction parameter for UDS service DiagnosticSessionControl
        /// </summary>
        public enum TPUDSSvcParamDSC : byte
        {
            /// <summary>
            /// Default Session
            /// </summary>
            PUDS_SVC_PARAM_DSC_DS = 0x01,
            /// <summary>
            /// ECU Programming Session
            /// </summary>
            PUDS_SVC_PARAM_DSC_ECUPS = 0x02,
            /// <summary>
            /// ECU Extended Diagnostic Session
            /// </summary>
            PUDS_SVC_PARAM_DSC_ECUEDS = 0x03,
            /// <summary>
            /// Safety System Diagnostic Session
            /// </summary>
            PUDS_SVC_PARAM_DSC_SSDS = 0x04
        }        

        /// <summary>
        /// The DiagnosticSessionControl service is used to enable different diagnostic sessions in the server.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="SessionType">Subfunction parameter: type of the session (see PUDS_SVC_PARAM_DSC_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcDiagnosticSessionControl")]
        public static extern TPUDSStatus SvcDiagnosticSessionControl(
	        TPUDSCANHandle CanChannel,
	        ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamDSC SessionType);

        #endregion

        #region UDS Service: ECUReset
        // ISO-15765-3:2004 §9.2.2 p.42 && ISO-14229-1:2006 §9.3 p.42

        /// <summary>
        /// Subfunction parameter for UDS service ECURest
        /// </summary>
        public enum TPUDSSvcParamER : byte
        {
            /// <summary>
            /// Hard Reset
            /// </summary>
            PUDS_SVC_PARAM_ER_HR = 0x01,
            /// <summary>
            /// Key Off on Reset
            /// </summary>
            PUDS_SVC_PARAM_ER_KOFFONR = 0x02,
            /// <summary>
            /// Soft Reset
            /// </summary>
            PUDS_SVC_PARAM_ER_SR = 0x03,
            /// <summary>
            /// Enable Rapid Power Shutdown
            /// </summary>
            PUDS_SVC_PARAM_ER_ERPSD = 0x04,
            /// <summary>
            /// Disable Rapid Power Shutdown
            /// </summary>
            PUDS_SVC_PARAM_ER_DRPSD = 0x05,
        }
        /// <summary>
        /// The ECUReset service is used by the client to request a server reset.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="ResetType">Subfunction parameter: type of Reset (see PUDS_SVC_PARAM_ER_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcECUReset")]
        public static extern TPUDSStatus SvcECUReset(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamER ResetType);
        #endregion

        #region UDS Service: SecurityAccess
        // ISO-15765-3:2004 §9.2.3 p.43 && ISO-14229-1:2006 §9.4 p.45
        
        /// <summary>
        /// SecurityAccessType : Request Seed and Send Key values
        /// </summary>        
        public const byte PUDS_SVC_PARAM_SA_RSD_1 = 0x01;	// Request Seed
        public const byte PUDS_SVC_PARAM_SA_RSD_3 = 0x03;	// Request Seed
        public const byte PUDS_SVC_PARAM_SA_RSD_5 = 0x05;	// Request Seed
        public const byte PUDS_SVC_PARAM_SA_RSD_MIN = 0x07;	// Request Seed (odd numbers)
        public const byte PUDS_SVC_PARAM_SA_RSD_MAX = 0x5F;	// Request Seed (odd numbers)
        public const byte PUDS_SVC_PARAM_SA_SK_2 = 0x02;	// Send Key
        public const byte PUDS_SVC_PARAM_SA_SK_4 = 0x04;	// Send Key
        public const byte PUDS_SVC_PARAM_SA_SK_6 = 0x06;	// Send Key
        public const byte PUDS_SVC_PARAM_SA_SK_MIN = 0x08;	// Send Key (even numbers)
        public const byte PUDS_SVC_PARAM_SA_SK_MAX = 0x60;	// Send Key (even numbers)

        /// <summary>
        /// SecurityAccess service provides a means to access data and/or diagnostic services which have
        ///	restricted access for security, emissions or safety reasons.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="SecurityAccessType">Subfunction parameter: type of SecurityAccess (see PUDS_SVC_PARAM_SA_xxx)</param>
        /// <param name="Buffer">If Requesting Seed, buffer is the optional data to transmit to a server (like identification).
        ///	If Sending Key, data holds the value generated by the security algorithm corresponding to a specific “seed” value</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcSecurityAccess")]
        public static extern TPUDSStatus SvcSecurityAccess(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte SecurityAccessType,
            byte[] Buffer,
            ushort BufferLength);
        #endregion
                
        #region UDS Service: CommunicationControl
        // ISO-15765-3:2004 §9.2.4 p.43 && ISO-14229-1:2006 §9.5 p.52

        /// <summary>
        /// ControlType: Subfunction parameter for UDS service CommunicationControl 
        /// </summary>
        public enum TPUDSSvcParamCC : byte
        {
            /// <summary>
            /// Enable Rx and Tx
            /// </summary>
            PUDS_SVC_PARAM_CC_ERXTX = 0x00,
            /// <summary>
            /// Enable Rx and Disable Tx
            /// </summary>
            PUDS_SVC_PARAM_CC_ERXDTX = 0x01,
            /// <summary>
            /// Disable Rx and Enable Tx
            /// </summary>
            PUDS_SVC_PARAM_CC_DRXETX = 0x02,
            /// <summary>
            /// Disable Rx and Tx
            /// </summary>
            PUDS_SVC_PARAM_CC_DRXTX = 0x03,            
        }

        /// <summary>
        /// CommunicationType Flag: Application (01b)
        /// </summary>  
        public const byte PUDS_SVC_PARAM_CC_FLAG_APPL = 0x01;
        /// <summary>
        /// CommunicationType Flag: NetworkManagement (10b)
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_NWM = 0x02;
        /// <summary>
        /// CommunicationType Flag: Disable/Enable specified communicationType (see Flags APPL/NMW)
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_DESCTIRNCN = 0x00;
        // in the receiving node and all connected networks
        /// <summary>
        /// CommunicationType Flag: Disable/Enable network which request is received on
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_DENWRIRO = 0xF0;
        /// <summary>
        /// CommunicationType Flag: Disable/Enable specific network identified by network number (minimum value)
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_DESNIBNN_MIN = 0x10;
        /// <summary>
        /// CommunicationType Flag: Disable/Enable specific network identified by network number (maximum value)
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_DESNIBNN_MAX = 0xE0;
        /// <summary>
        /// CommunicationType Flag: Mask for DESNIBNN bits
        /// </summary>
        public const byte PUDS_SVC_PARAM_CC_FLAG_DESNIBNN_MASK = 0xF0;

        /// <summary>
        ///	CommunicationControl service's purpose is to switch on/off the transmission 
        ///	and/or the reception of certain messages of (a) server(s).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="ControlType">Subfunction parameter: type of CommunicationControl (see PUDS_SVC_PARAM_CC_xxx)</param>
        /// <param name="CommunicationType">a bit-code value to reference the kind of communication to be controlled,
        ///	See PUDS_SVC_PARAM_CC_FLAG_xxx flags and ISO_14229-2006 §B.1 for bit-encoding</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcCommunicationControl")]
        public static extern TPUDSStatus SvcCommunicationControl(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamCC ControlType,	
	        byte CommunicationType);
        #endregion

        #region UDS Service: TesterPresent
        // ISO-15765-3:2004 §9.2.5 p.43 && ISO-14229-1:2006 §9.6 p.55
        
        /// <summary>
        /// TesterPresentType: Subfunction parameter for UDS service TesterPresent
        /// </summary>
        public enum TPUDSSvcParamTP : byte
        {
            /// <summary>
            /// Zero SubFunction
            /// </summary>
            PUDS_SVC_PARAM_TP_ZSUBF = 0x00,
        }

        /// <summary>
        ///	TesterPresent service indicates to a server (or servers) that a client is still connected
        ///	to the vehicle and that certain diagnostic services and/or communications 
        ///	that have been previously activated are to remain active.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="TesterPresentType">No Subfunction parameter by default (PUDS_SVC_PARAM_TP_ZSUBF)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcTesterPresent")]
        public static extern TPUDSStatus SvcTesterPresent(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamTP TesterPresentType);

        /// <summary>
        ///	TesterPresent service indicates to a server (or servers) that a client is still connected
        ///	to the vehicle and that certain diagnostic services and/or communications 
        ///	that have been previously activated are to remain active.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        public static TPUDSStatus SvcTesterPresent(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer)
        {
            return SvcTesterPresent(CanChannel, ref MessageBuffer, TPUDSSvcParamTP.PUDS_SVC_PARAM_TP_ZSUBF);
        }
        #endregion
        
        #region UDS Service: SecuredDataTransmission
        // ISO-15765-3:2004 §9.2.6 p.44 && ISO-14229-1:2006 §9.8 p.63
        
        /// <summary>
        ///	SecuredDataTransmission service's purpose is to transmit data that is protected 
        ///	against attacks from third parties, which could endanger data security.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="Buffer">buffer containing the data as processed by the Security Sub-Layer (See ISO-15764)</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcSecuredDataTransmission")]
        public static extern TPUDSStatus SvcSecuredDataTransmission(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: ControlDTCSetting
        // ISO-15765-3:2004 §9.2.7 p.44 && ISO-14229-1:2006 §9.9 p.69

        /// <summary>
        /// DTCSettingType: Subfunction parameter for UDS service ControlDTCSetting
        /// ISO
        /// </summary>
        public enum TPUDSSvcParamCDTCS : byte
        {
            /// <summary>
            /// The server(s) shall resume the setting of diagnostic trouble codes
            /// </summary>
            PUDS_SVC_PARAM_CDTCS_ON = 0x01,
            /// <summary>
            /// The server(s) shall stop the setting of diagnostic trouble codes
            /// </summary>
            PUDS_SVC_PARAM_CDTCS_OFF = 0x02,
        }

        /// <summary>
        ///	ControlDTCSetting service shall be used by a client to stop or resume the setting of 
        ///	diagnostic trouble codes (DTCs) in the server(s).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DTCSettingType">Subfunction parameter (see PUDS_SVC_PARAM_CDTCS_xxx)</param>
        /// <param name="Buffer">This parameter record is user-optional and transmits data to a server when controlling the DTC setting. 
        ///	It can contain a list of DTCs to be turned on or off.</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcControlDTCSetting")]
        public static extern TPUDSStatus SvcControlDTCSetting(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamCDTCS DTCSettingType,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: ResponseOnEvent
        // ISO-15765-3:2004 §9.2.8 p.44 && ISO-14229-1:2006 §9.10 p.73

        /// <summary>
        /// EventType: Subfunction parameter for UDS service ResponseOnEvent
        /// </summary>
        public enum TPUDSSvcParamROE : byte
        {
            /// <summary>
            /// Stop Response On Event
            /// </summary>
            PUDS_SVC_PARAM_ROE_STPROE = 0x00,
            /// <summary>
            /// On DTC Status Change
            /// </summary>
            PUDS_SVC_PARAM_ROE_ONDTCS = 0x01,
            /// <summary>
            /// On Timer Interrupt
            /// </summary>
            PUDS_SVC_PARAM_ROE_OTI = 0x02,
            /// <summary>
            /// On Change Of Data Identifier
            /// </summary>
            PUDS_SVC_PARAM_ROE_OCODID = 0x03,
            /// <summary>
            /// Report Activated Events
            /// </summary>
            PUDS_SVC_PARAM_ROE_RAE = 0x04,
            /// <summary>
            /// Start Response On Event
            /// </summary>
            PUDS_SVC_PARAM_ROE_STRTROE = 0x05,
            /// <summary>
            /// Clear Response On Event
            /// </summary>
            PUDS_SVC_PARAM_ROE_CLRROE = 0x06,
            /// <summary>
            /// On Comparison Of Values
            /// </summary>
            PUDS_SVC_PARAM_ROE_OCOV = 0x07,
        }
        
        /// <summary>
        /// RoE Recommended service (first byte of ServiceToRespondTo Record)
        /// </summary>
        public enum TPUDSSvcParamROERecommendedServiceID : byte
        {
            PUDS_SVC_PARAM_ROE_STRT_SI_RDBI = TPUDSService.PUDS_SI_ReadDataByIdentifier,
            PUDS_SVC_PARAM_ROE_STRT_SI_RDTCI = TPUDSService.PUDS_SI_ReadDTCInformation,
            PUDS_SVC_PARAM_ROE_STRT_SI_RC = TPUDSService.PUDS_SI_RoutineControl,
            PUDS_SVC_PARAM_ROE_STRT_SI_IOCBI = TPUDSService.PUDS_SI_InputOutputControlByIdentifier,
        }
           
        /// <summary>
        /// expected size of EventTypeRecord for ROE_STPROE
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_STPROE_LEN = 0;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_ONDTCS
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_ONDTCS_LEN = 1;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_OTI
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_OTI_LEN = 1;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_OCODID
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_OCODID_LEN = 2;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_RAE
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_RAE_LEN = 0;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_STRTROE
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_STRTROE_LEN = 0;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_CLRROE
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_CLRROE_LEN = 0;
        /// <summary>
        /// expected size of EventTypeRecord for ROE_OCOV
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_OCOV_LEN = 10;
        /// <summary>
        /// Infinite Time To Response (eventWindowTime parameter)
        /// </summary>
        public const byte PUDS_SVC_PARAM_ROE_EWT_ITTR = 0x02;
                     
        /// <summary>
        ///	The ResponseOnEvent service requests a server to 
        ///	start or stop transmission of responses on a specified event.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="EventType">Subfunction parameter: event type (see PUDS_SVC_PARAM_ROE_xxx)</param>
        /// <param name="StoreEvent">Storage State (TRUE = Store Event, FALSE = Do Not Store Event)</param>
        /// <param name="EventWindowTime">Specify a window for the event logic to be active in the server (see PUDS_SVC_PARAM_ROE_EWT_ITTR)</param>
        /// <param name="EventTypeRecord">Additional parameters for the specified eventType</param>
        /// <param name="EventTypeRecordLength">Size in bytes of the EventType Record (see PUDS_SVC_PARAM_ROE_xxx_LEN)</param>
        /// <param name="ServiceToRespondToRecord">Service parameters, with first byte as service Id (see PUDS_SVC_PARAM_ROE_STRT_SI_xxx)</param>
        /// <param name="ServiceToRespondToRecordLength">Size in bytes of the ServiceToRespondTo Record</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcResponseOnEvent")]
        public static extern TPUDSStatus SvcResponseOnEvent(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamROE EventType,
	        bool StoreEvent,
	        byte EventWindowTime,
	        byte[] EventTypeRecord,
	        ushort EventTypeRecordLength,
	        byte[] ServiceToRespondToRecord,
	        ushort ServiceToRespondToRecordLength);
        #endregion

        #region UDS Service: LinkControl
        // ISO-15765-3:2004 §9.2.9 p.47 && ISO-14229-1:2006 §9.11 p.91

        /// <summary>
        /// LinkControlType: Subfunction parameter for UDS service LinkControl
        /// </summary>
        public enum TPUDSSvcParamLC : byte
        {
            /// <summary>
            /// Verify Baudrate Transition With Fixed Baudrate
            /// </summary>
            PUDS_SVC_PARAM_LC_VBTWFBR = 0x01,
            /// <summary>
            /// Verify Baudrate Transition With Specific Baudrate
            /// </summary>
            PUDS_SVC_PARAM_LC_VBTWSBR = 0x02,
            /// <summary>
            /// Transition Baudrate
            /// </summary>
            PUDS_SVC_PARAM_LC_TB = 0x03,
        }
        
        /// <summary>
        /// BaudrateIdentifier: standard Baudrate Identifiers
        /// </summary>
        public enum TPUDSSvcParamLCBaudrateIdentifier : byte
        {
            /// <summary>
            /// standard PC baud rate of 9.6 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_PC_9600	=	0x01,
            /// <summary>
            /// standard PC baud rate of 19.2 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_PC_19200	=	0x02,
            /// <summary>
            /// standard PC baud rate of 38.4 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_PC_38400	=	0x03,	
            /// <summary>
            /// standard PC baud rate of 57.6 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_PC_57600	=	0x04,
            /// <summary>
            /// standard PC baud rate of 115.2 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_PC_115200 =	0x05,
            /// <summary>
            /// standard CAN baud rate of 125 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_CAN_125K	=	0x10,
            /// <summary>
            /// standard CAN baud rate of 250 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_CAN_250K	=	0x11,
            /// <summary>
            /// standard CAN baud rate of 500 KBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_CAN_500K	=	0x12,
            /// <summary>
            /// standard CAN baud rate of 1 MBaud
            /// </summary>
            PUDS_SVC_PARAM_LC_BAUDRATE_CAN_1M	=	0x13,
        }
        
        /// <summary>
        ///	The LinkControl service is used to control the communication link baud rate
        ///	between the client and the server(s) for the exchange of diagnostic data.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="LinkControlType">Subfunction parameter: Link Control Type (see PUDS_SVC_PARAM_LC_xxx)</param>
        /// <param name="BaudrateIdentifier">defined baud rate identifier (see PUDS_SVC_PARAM_LC_BAUDRATE_xxx)</param>
        /// <param name="LinkBaudrate">used only with PUDS_SVC_PARAM_LC_VBTWSBR parameter: 
        ///	a three-byte value baud rate (baudrate High, Middle and Low Bytes).
        ///	</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success </returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcLinkControl")]
        public static extern TPUDSStatus SvcLinkControl(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamLC LinkControlType,
            byte BaudrateIdentifier,
            UInt32 LinkBaudrate);

        /// <summary>
        ///	The LinkControl service is used to control the communication link baud rate
        ///	between the client and the server(s) for the exchange of diagnostic data.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="LinkControlType">Subfunction parameter: Link Control Type (see PUDS_SVC_PARAM_LC_xxx)</param>
        /// <param name="BaudrateIdentifier">defined baud rate identifier (see PUDS_SVC_PARAM_LC_BAUDRATE_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success </returns>
        public static TPUDSStatus SvcLinkControl(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamLC LinkControlType,
            byte BaudrateIdentifier)
        {
            return SvcLinkControl(CanChannel, ref MessageBuffer, LinkControlType, BaudrateIdentifier, 0);
        }
        #endregion

        #region UDS Service: ReadDataByIdentifier
        // ISO-15765-3:2004 §9.3.1 p.47 && ISO-14229-1:2006 §10.2 p.97

        /// <summary>
        /// Data Identifiers ISO-14229-1:2006 §C.1 p.259
        /// </summary>
        public enum TPUDSSvcParamDI : ushort
        {
            /// <summary>
            /// bootSoftwareIdentificationDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_BSIDID = 0xF180,
            /// <summary>
            /// applicationSoftwareIdentificationDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ASIDID = 0xF181,
            /// <summary>
            /// applicationDataIdentificationDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ADIDID = 0xF182,
            /// <summary>
            /// bootSoftwareIdentificationDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_BSFPDID = 0xF183,
            /// <summary>
            /// applicationSoftwareFingerprintDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ASFPDID = 0xF184,
            /// <summary>
            /// applicationDataFingerprintDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ADFPDID = 0xF185,
            /// <summary>
            /// activeDiagnosticSessionDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ADSDID = 0xF186,
            /// <summary>
            /// vehicleManufacturerSparePartNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VMSPNDID = 0xF187,
            /// <summary>
            /// vehicleManufacturerECUSoftwareNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VMECUSNDID = 0xF188,
            /// <summary>
            /// vehicleManufacturerECUSoftwareVersionNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VMECUSVNDID = 0xF189,
            /// <summary>
            /// systemSupplierIdentifierDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SSIDDID = 0xF18A,
            /// <summary>
            /// ECUManufacturingDateDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ECUMDDID = 0xF18B,
            /// <summary>
            /// ECUSerialNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ECUSNDID = 0xF18C,
            /// <summary>
            /// supportedFunctionalUnitsDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SFUDID = 0xF18D,
            /// <summary>
            /// vehicleManufacturerKitAssemblyPartNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VMKAPNDID = 0xF18E,
            /// <summary>
            /// VINDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VINDID = 0xF190,
            /// <summary>
            /// vehicleManufacturerECUHardwareNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_VMECUHNDID = 0xF191,
            /// <summary>
            /// systemSupplierECUHardwareNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SSECUHWNDID = 0xF192,
            /// <summary>
            /// systemSupplierECUHardwareVersionNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SSECUHWVNDID = 0xF193,
            /// <summary>
            /// systemSupplierECUSoftwareNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SSECUSWNDID = 0xF194,
            /// <summary>
            /// systemSupplierECUSoftwareVersionNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SSECUSWVNDID = 0xF195,
            /// <summary>
            /// exhaustRegulationOrTypeApprovalNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_EROTANDID = 0xF196,
            /// <summary>
            /// systemNameOrEngineTypeDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_SNOETDID = 0xF197,
            /// <summary>
            /// repairShopCodeOrTesterSerialNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_RSCOTSNDID = 0xF198,
            /// <summary>
            /// programmingDateDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_PDDID = 0xF199,
            /// <summary>
            /// calibrationRepairShopCodeOrCalibrationEquipmentSerialNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_CRSCOCESNDID = 0xF19A,
            /// <summary>
            /// calibrationDateDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_CDDID = 0xF19B,
            /// <summary>
            /// calibrationEquipmentSoftwareNumberDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_CESWNDID = 0xF19C,
            /// <summary>
            /// ECUInstallationDateDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_EIDDID = 0xF19D,
            /// <summary>
            /// ODXFileDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_ODXFDID = 0xF19E,
            /// <summary>
            /// entityDataIdentifier
            /// </summary>
            PUDS_SVC_PARAM_DI_EDID = 0xF19F,
        }

        /// <summary>
        ///	The ReadDataByIdentifier service allows the client to request data record values 
        ///	from the server identified by one or more dataIdentifiers.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="Buffer">buffer containing a list of two-byte Data Identifiers (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <param name="BufferLength">Number of elements in the buffer (size in WORD of the buffer)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDataByIdentifier")]
        public static extern TPUDSStatus SvcReadDataByIdentifier(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: ReadMemoryByAddress
        // ISO-15765-3:2004 §9.3.2 p.47 && ISO-14229-1:2006 §10.3 p.102

        /// <summary>
        ///	The ReadMemoryByAddress service allows the client to request memory data from the server 
        ///	via a provided starting address and to specify the size of memory to be read.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="MemoryAddress">starting address of server memory from which data is to be retrieved</param>
        /// <param name="MemoryAddressLength">Size in bytes of the MemoryAddress buffer (max.: 0xF)</param>
        /// <param name="MemorySize">number of bytes to be read starting at the address specified by memoryAddress</param>
        /// <param name="MemorySizeLength">Size in bytes of the MemorySize buffer (max.: 0xF)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadMemoryByAddress")]
        public static extern TPUDSStatus SvcReadMemoryByAddress(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte[] MemoryAddress,
            byte MemoryAddressLength,
            byte[] MemorySize,
            byte MemorySizeLength);
        #endregion
        
        #region UDS Service: ReadScalingDataByIdentifier
        // ISO-15765-3:2004 §9.3.3 p.48 && ISO-14229-1:2006 §10.4 p.106

        /// <summary>
        ///	The ReadScalingDataByIdentifier service allows the client to request 
        ///	scaling data record information from the server identified by a dataIdentifier.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="DataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadScalingDataByIdentifier")]
        public static extern TPUDSStatus SvcReadScalingDataByIdentifier(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort DataIdentifier);
        #endregion

        #region UDS Service: ReadDataByPeriodicIdentifier
        // ISO-15765-3:2004 §9.3.4 p.48 && ISO-14229-1:2006 §10.5 p.112

        /// <summary>
        /// TransmissionMode: Subfunction parameter for UDS service ReadDataByPeriodicIdentifier
        /// </summary>
        public enum TPUDSSvcParamRDBPI : byte
        {
            /// <summary>
            /// Send At Slow Rate
            /// </summary>
            PUDS_SVC_PARAM_RDBPI_SASR = 0x01,
            /// <summary>
            /// Send At Medium Rate
            /// </summary>
            PUDS_SVC_PARAM_RDBPI_SAMR = 0x02,
            /// <summary>
            /// Send At Fast Rate
            /// </summary>
            PUDS_SVC_PARAM_RDBPI_SAFR = 0x03,
            /// <summary>
            /// Stop Sending
            /// </summary>
            PUDS_SVC_PARAM_RDBPI_SS = 0x04,
        }

        /// <summary>
        ///	The ReadDataByPeriodicIdentifier service allows the client to request the periodic transmission 
        ///	of data record values from the server identified by one or more periodicDataIdentifiers.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="TransmissionMode">transmission rate code (see PUDS_SVC_PARAM_RDBPI_xxx)</param>
        /// <param name="Buffer">buffer containing a list of Periodic Data Identifiers</param>
        /// <param name="BufferLength">Number of elements in the buffer (size in WORD of the buffer)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDataByPeriodicIdentifier")]
        public static extern TPUDSStatus SvcReadDataByPeriodicIdentifier(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
	        TPUDSSvcParamRDBPI TransmissionMode,
	        byte[] Buffer,
	        ushort BufferLength);
        #endregion

        #region UDS Service: DynamicallyDefineDataIdentifier
        // ISO-15765-3:2004 §9.3.5 p.54 && ISO-14229-1:2006 §10.6 p.123

        /// <summary>
        /// DynamicallyDefineDataIdentifier Type: Subfunction parameter for UDS service DynamicallyDefineDataIdentifier
        /// </summary>
        public enum TPUDSSvcParamDDDI : byte
        {
            /// <summary>
            /// Define By Identifier
            /// </summary>
            PUDS_SVC_PARAM_DDDI_DBID = 0x01,
            /// <summary>
            /// Define By Memory Address
            /// </summary>
            PUDS_SVC_PARAM_DDDI_DBMA = 0x02,
            /// <summary>
            /// Clear Dynamically Defined Data Identifier
            /// </summary>
            PUDS_SVC_PARAM_DDDI_CDDDI = 0x03,
        }

        /// <summary>
        ///	The DynamicallyDefineDataIdentifier service allows the client to dynamically define 
        ///	in a server a data identifier that can be read via the ReadDataByIdentifier service at a later time.
        ///	The Define By Identifier subfunction specifies that definition of the dynamic data
        ///	identifier shall occur via a data identifier reference.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DynamicallyDefinedDataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <param name="SourceDataIdentifier">buffer containing the sources of information to be included into the dynamic data record</param>
        /// <param name="MemorySize">buffer containing the total numbers of bytes from the source data record address</param>
        /// <param name="PositionInSourceDataRecord">buffer containing the starting byte positions of the excerpt of the source data record</param>
        /// <param name="BuffersLength">Number of elements in the buffers (SourceDataIdentifier, MemoryAddress and PositionInSourceDataRecord)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcDynamicallyDefineDataIdentifierDBID")]
        public static extern TPUDSStatus SvcDynamicallyDefineDataIdentifierDBID(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
	        ushort DynamicallyDefinedDataIdentifier,
	        ushort[] SourceDataIdentifier,
            byte[] MemorySize,
            byte[] PositionInSourceDataRecord,
	        ushort BuffersLength);

        /// <summary>
        ///	The DynamicallyDefineDataIdentifier service allows the client to dynamically define 
        ///	in a server a data identifier that can be read via the ReadDataByIdentifier service at a later time.
        ///	The Define By Memory Address subfunction specifies that definition of the dynamic data
        ///	identifier shall occur via an address reference.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DynamicallyDefinedDataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <param name="MemoryAddressLength">Size in bytes of the MemoryAddress items in the MemoryAddressBuffer buffer (max.: 0xF)</param>
        /// <param name="MemorySizeLength">Size in bytes of the MemorySize items in the MemorySizeBuffer buffer (max.: 0xF)</param>
        /// <param name="MemoryAddressBuffer">buffer containing the MemoryAddress buffers,
        ///	must be an array of 'BuffersLength' entries which contains 'MemoryAddressLength' bytes
        ///	(size is 'BuffersLength * MemoryAddressLength' bytes)</param>
        /// <param name="MemorySizeBuffer">buffer containing the MemorySize buffers,
        ///	must be an array of 'BuffersLength' entries which contains 'MemorySizeLength' bytes
        ///	(size is 'BuffersLength * MemorySizeLength' bytes)</param>
        /// <param name="BuffersLength">Size in bytes of the MemoryAddressBuffer and MemorySizeBuffer buffers</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcDynamicallyDefineDataIdentifierDBMA")]
        public static extern TPUDSStatus SvcDynamicallyDefineDataIdentifierDBMA(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort DynamicallyDefinedDataIdentifier,
            byte MemoryAddressLength,
            byte MemorySizeLength,
            byte[] MemoryAddressBuffer,
            byte[] MemorySizeBuffer,
            ushort BuffersLength);

        /// <summary>
        ///	The Clear Dynamically Defined Data Identifier subfunction shall be used to clear 
        ///	the specified dynamic data identifier.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DynamicallyDefinedDataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcDynamicallyDefineDataIdentifierCDDDI")]
        public static extern TPUDSStatus SvcDynamicallyDefineDataIdentifierCDDDI(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort DynamicallyDefinedDataIdentifier);
        #endregion

        #region UDS Service: WriteDataByIdentifier
        // ISO-15765-3:2004 §9.3.6 p.54 && ISO-14229-1:2006 §10.7 p.143

        /// <summary>
        ///	The WriteDataByIdentifier service allows the client to write information into the server at an internal location
        ///	specified by the provided data identifier.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="DataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <param name="Buffer">buffer containing the data to write</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcWriteDataByIdentifier")]
        public static extern TPUDSStatus SvcWriteDataByIdentifier(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort DataIdentifier,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: WriteMemoryByAddress
        // ISO-15765-3:2004 §9.3.7 p.54 && ISO-14229-1:2006 §10.8 p.146

        /// <summary>
        ///	The WriteMemoryByAddress service allows the client to write 
        ///	information into the server at one or more contiguous memory locations.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="MemoryAddress">starting address of server memory to which data is to be written</param>
        /// <param name="MemoryAddressLength">Size in bytes of the MemoryAddress buffer (max.: 0xF)</param>
        /// <param name="MemorySize">number of bytes to be written starting at the address specified by memoryAddress</param>
        /// <param name="MemorySizeLength">Size in bytes of the MemorySize buffer (max.: 0xF)</param>
        /// <param name="Buffer">buffer containing the data to write</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcWriteMemoryByAddress")]
        public static extern TPUDSStatus SvcWriteMemoryByAddress(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte[] MemoryAddress,
	        byte MemoryAddressLength,
            byte[] MemorySize,
	        byte MemorySizeLength,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: ClearDiagnosticInformation
        // ISO-15765-3:2004 §9.4.2 p.56 && ISO-14229-1:2006 §11.2 p.152

        /// <summary>
        /// groupOfDTC : Emissions-related systems group of DTCs
        /// </summary>  
        public const UInt32 PUDS_SVC_PARAM_CDI_ERS = 0x000000;
        /// <summary>
        /// groupOfDTC : All Groups of DTCs
        /// </summary>  
        public const UInt32 PUDS_SVC_PARAM_CDI_AGDTC = 0xFFFFFF;

        /// <summary>
        ///	The ClearDiagnosticInformation service is used by the client to clear diagnostic information 
        ///	in one server's or multiple servers’ memory.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="groupOfDTC">a three-byte value indicating the group of DTCs (e.g. powertrain, body, chassis) 
        /// or the particular DTC to be cleared (see PUDS_SVC_PARAM_CDI_xxx)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcClearDiagnosticInformation")]
        public static extern TPUDSStatus SvcClearDiagnosticInformation(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            UInt32 groupOfDTC);
        #endregion

        #region UDS Service: ReadDTCInformation
        // ISO-15765-3:2004 §9.4.1 p.54 && ISO-14229-1:2006 §11.3 p.154

        /// <summary>
        /// RDTCIType: Subfunction parameter for UDS service ReadDTCInformation
        /// ISO-15765-3:2004 §9.4.1 p.54 && ISO-14229-1:2006 §11.3 p.154
        /// </summary>
        public enum TPUDSSvcParamRDTCI : byte
        {
            /// <summary>
            /// report Number Of DTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RNODTCBSM = 0x01,
            /// <summary>
            /// report DTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCBSM = 0x02,
            /// <summary>
            /// report DTC Snapshot Identification
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCSSI = 0x03,
            /// <summary>
            /// report DTC Snapshot Record By DTC Number
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCSSBDTC = 0x04,
            /// <summary>
            /// report DTC Snapshot Record By Record Number
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCSSBRN = 0x05,
            /// <summary>
            /// report DTC Extended Data Record By DTC Number
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCEDRBDN = 0x06,
            /// <summary>
            /// report Number Of DTC By Severity Mask Record
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RNODTCBSMR = 0x07,
            /// <summary>
            /// report DTC By Severity Mask Record
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCBSMR = 0x08,
            /// <summary>
            /// report Severity Information Of DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RSIODTC = 0x09,
            /// <summary>
            /// report Supported DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RSUPDTC = 0x0A,
            /// <summary>
            /// report First Test Failed DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RFTFDTC = 0x0B,
            /// <summary>
            /// report First Confirmed DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RFCDTC = 0x0C,
            /// <summary>
            /// report Most Recent Test Failed DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RMRTFDTC = 0x0D,
            /// <summary>
            /// report Most Recent Confirmed DTC
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RMRCDTC = 0x0E,
            /// <summary>
            /// report Mirror Memory DTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RMMDTCBSM = 0x0F,
            /// <summary>
            /// report Mirror Memory DTC Extended Data Record By DTC Number
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RMMDEDRBDN = 0x10,
            /// <summary>
            /// report Number Of Mirror MemoryDTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RNOMMDTCBSM = 0x11,
            /// <summary>
            /// report Number Of Emissions Related OBD DTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RNOOBDDTCBSM = 0x12,
            /// <summary>
            /// report Emissions Related OBD DTC By Status Mask
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_ROBDDTCBSM = 0x13,
            /// <summary>
            /// report DTC Fault Detection Counter 
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCFDC = 0x14,
            /// <summary>
            /// report DTC With Permanent Status
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_RDTCWPS = 0x15,
        }

        /// <summary>
        /// DTCSeverityMask (DTCSVM) : ISO-14229-1:2006 §D.3 p.285
        /// </summary>
        [Flags]
        public enum TPUDSSvcParamRDTCI_DTCSVM : byte
        {
            /// <summary>
            /// DTC severity bit definitions : no SeverityAvailable
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_DTCSVM_NSA = 0x00,
            /// <summary>
            /// DTC severity bit definitions : maintenance Only
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_DTCSVM_MO = 0x20,
            /// <summary>
            /// DTC severity bit definitions : check At Next Halt
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_DTCSVM_CHKANH = 0x40,
            /// <summary>
            /// DTC severity bit definitions : check Immediately
            /// </summary>
            PUDS_SVC_PARAM_RDTCI_DTCSVM_CHKI = 0x80,
        }

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        /// Only reportNumberOfDTCByStatusMask, reportDTCByStatusMask, reportMirrorMemoryDTCByStatusMask,
        ///	reportNumberOfMirrorMemoryDTCByStatusMask, reportNumberOfEmissionsRelatedOBDDTCByStatusMask, 
        ///	reportEmissionsRelatedOBDDTCByStatusMask Sub-functions are allowed.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="RDTCIType">Subfunction parameter: ReadDTCInformation type, use one of the following:
        ///	PUDS_SVC_PARAM_RDTCI_RNODTCBSM, PUDS_SVC_PARAM_RDTCI_RDTCBSM,
        ///	PUDS_SVC_PARAM_RDTCI_RMMDTCBSM, PUDS_SVC_PARAM_RDTCI_RNOMMDTCBSM,
        ///	PUDS_SVC_PARAM_RDTCI_RNOOBDDTCBSM, PUDS_SVC_PARAM_RDTCI_ROBDDTCBSM</param>
        /// <param name="DTCStatusMask">Contains eight DTC status bit.</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformation")]
        public static extern TPUDSStatus SvcReadDTCInformation(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamRDTCI RDTCIType,
	        byte DTCStatusMask);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        ///	The sub-function reportDTCSnapshotRecordByDTCNumber (PUDS_SVC_PARAM_RDTCI_RDTCSSBDTC) is implicit.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DTCMask">a unique identification number (three byte value) for a specific diagnostic trouble code</param>
        /// <param name="DTCSnapshotRecordNumber">the number of the specific DTCSnapshot data records</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationRDTCSSBDTC")]
        public static extern TPUDSStatus SvcReadDTCInformationRDTCSSBDTC(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            UInt32 DTCMask,
            byte DTCSnapshotRecordNumber);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        ///	The sub-function reportDTCSnapshotByRecordNumber (PUDS_SVC_PARAM_RDTCI_RDTCSSBRN) is implicit.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DTCSnapshotRecordNumber">the number of the specific DTCSnapshot data records</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationRDTCSSBRN")]
        public static extern TPUDSStatus SvcReadDTCInformationRDTCSSBRN(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte DTCSnapshotRecordNumber);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        /// Only reportDTCExtendedDataRecordByDTCNumber and reportMirrorMemoryDTCExtendedDataRecordByDTCNumber Sub-functions are allowed.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="RDTCIType">Subfunction parameter: ReadDTCInformation type, use one of the following:
        ///	PUDS_SVC_PARAM_RDTCI_RDTCEDRBDN, PUDS_SVC_PARAM_RDTCI_RMMDEDRBDN</param>
        /// <param name="DTCMask">a unique identification number (three byte value) for a specific diagnostic trouble code</param>
        /// <param name="DTCExtendedDataRecordNumber">the number of the specific DTCExtendedData record requested.</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationReportExtended")]
        public static extern TPUDSStatus SvcReadDTCInformationReportExtended(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamRDTCI RDTCIType,
            UInt32 DTCMask,
            byte DTCExtendedDataRecordNumber);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        /// Only reportNumberOfDTCBySeverityMaskRecord and reportDTCSeverityInformation Sub-functions are allowed.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="RDTCIType">Subfunction parameter: ReadDTCInformation type, use one of the following:
        ///	PUDS_SVC_PARAM_RDTCI_RNODTCBSMR, PUDS_SVC_PARAM_RDTCI_RDTCBSMR</param>
        /// <param name="DTCSeverityMask">a mask of eight (8) DTC severity bits (see PUDS_SVC_PARAM_RDTCI_DTCSVM_xxx)</param>
        /// <param name="DTCStatusMask">a mask of eight (8) DTC status bits</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationReportSeverity")]
        public static extern TPUDSStatus SvcReadDTCInformationReportSeverity(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamRDTCI RDTCIType,
            byte DTCSeverityMask,
            byte DTCStatusMask);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        ///	The sub-function reportSeverityInformationOfDTC (PUDS_SVC_PARAM_RDTCI_RSIODTC) is implicit.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="DTCMask">a unique identification number for a specific diagnostic trouble code</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationRSIODTC")]
        public static extern TPUDSStatus SvcReadDTCInformationRSIODTC(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            UInt32 DTCMask);

        /// <summary>
        ///	This service allows a client to read the status of server-resident Diagnostic Trouble Code (DTC) information.
        /// Only reportSupportedDTC, reportFirstTestFailedDTC, reportFirstConfirmedDTC, reportMostRecentTestFailedDTC,
        ///	reportMostRecentConfirmedDTC, reportDTCFaultDetectionCounter, reportDTCWithPermanentStatus, 
        /// and reportDTCSnapshotIdentification Sub-functions are allowed.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="RDTCIType">Subfunction parameter: ReadDTCInformation type, use one of the following:
        ///	PUDS_SVC_PARAM_RDTCI_RFTFDTC, PUDS_SVC_PARAM_RDTCI_RFCDTC, 
        ///	PUDS_SVC_PARAM_RDTCI_RMRTFDTC, PUDS_SVC_PARAM_RDTCI_RMRCDTC, 
        ///	PUDS_SVC_PARAM_RDTCI_RSUPDTC, PUDS_SVC_PARAM_RDTCI_RDTCWPS,
        ///	PUDS_SVC_PARAM_RDTCI_RDTCSSI</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcReadDTCInformationNoParam")]
        public static extern TPUDSStatus SvcReadDTCInformationNoParam(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamRDTCI RDTCIType);
        #endregion
        
        #region UDS Service: InputOutputControlByIdentifier
        // ISO-15765-3:2004 §9.5.1 p.56 && ISO-14229-1:2006 §12.2 p.209, 

        /// <summary>
        /// inputOutputControlParameter: ISO-14229-1:2006  §E.1 p.289
        /// </summary>
        public enum TPUDSSvcParamIOCBI : byte
        {
            /// <summary>
            /// returnControlToECU (0 controlState bytes in request)
            /// </summary>
            PUDS_SVC_PARAM_IOCBI_RCTECU = 0x00,
            /// <summary>
            /// resetToDefault (0 controlState bytes in request)
            /// </summary>
            PUDS_SVC_PARAM_IOCBI_RTD = 0x01,
            /// <summary>
            /// freezeCurrentState (0 controlState bytes in request)
            /// </summary>
            PUDS_SVC_PARAM_IOCBI_FCS = 0x02,
            /// <summary>
            /// shortTermAdjustment
            /// </summary>
            PUDS_SVC_PARAM_IOCBI_STA = 0x03,
        }

        /// <summary>
        ///	The InputOutputControlByIdentifier service is used by the client to substitute a value for an input signal,
        ///	internal server function and/or control an output (actuator) of an electronic system.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="DataIdentifier">a two-byte Data Identifier (see PUDS_SVC_PARAM_DI_xxx)</param>
        /// <param name="ControlOptionRecord">First byte can be used as either an InputOutputControlParameter 
        ///	that describes how the server shall control its inputs or outputs (see PUDS_SVC_PARAM_IOCBI_xxx),
        ///	or as an additional controlState byte</param>
        /// <param name="ControlOptionRecordLength">Size in bytes of the ControlOptionRecord buffer</param>
        /// <param name="ControlEnableMaskRecord">The ControlEnableMask shall only be supported when 
        ///	the inputOutputControlParameter is used and the dataIdentifier to be controlled consists 
        ///	of more than one parameter (i.e. the dataIdentifier is bit-mapped or packeted by definition). 
        ///	There shall be one bit in the ControlEnableMask corresponding to each individual parameter 
        ///	defined within the dataIdentifier.</param>
        /// <param name="ControlEnableMaskRecordLength">Size in bytes of the controlEnableMaskRecord buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcInputOutputControlByIdentifier")]
        public static extern TPUDSStatus SvcInputOutputControlByIdentifier(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            ushort DataIdentifier,
            byte[] ControlOptionRecord,
            ushort ControlOptionRecordLength,
            byte[] ControlEnableMaskRecord,
            ushort ControlEnableMaskRecordLength);
        #endregion

        #region UDS Service: RoutineControl
        // ISO-15765-3:2004 §9.6.1 p.56 && ISO-14229-1:2006 §13.2 p.225

        /// <summary>
        /// RoutineControlType: Subfunction parameter for UDS service RoutineControl
        /// </summary>
        public enum TPUDSSvcParamRC : byte
        {
            /// <summary>
            /// Start Routine
            /// </summary>
            PUDS_SVC_PARAM_RC_STR = 0x01,
            /// <summary>
            /// Stop Routine
            /// </summary>
            PUDS_SVC_PARAM_RC_STPR = 0x02,
            /// <summary>
            /// Request Routine Results
            /// </summary>
            PUDS_SVC_PARAM_RC_RRR = 0x03,
        }

        /// <summary>
        /// Routine Identifier: ISO-14229-1:2006 §F.1 p.290
        /// </summary>
        public enum TPUDSSvcParamRC_RID : ushort
        {
            /// <summary>
            /// Deploy Loop Routine ID
            /// </summary>
            PUDS_SVC_PARAM_RC_RID_DLRI_ = 0xE200,
            /// <summary>
            /// erase Memory
            /// </summary>
            PUDS_SVC_PARAM_RC_RID_EM_ = 0xFF00,
            /// <summary>
            /// check Programming Dependencies
            /// </summary>
            PUDS_SVC_PARAM_RC_RID_CPD_ = 0xFF01,
            /// <summary>
            /// erase Mirror Memory DTCs
            /// </summary>
            PUDS_SVC_PARAM_RC_RID_EMMDTC_ = 0xFF02,
        }
        
        /// <summary>
        ///	The RoutineControl service is used by the client to start/stop a routine,
        ///	and request routine results.
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message</param>
        /// <param name="RoutineControlType">Subfunction parameter: RoutineControl type (see PUDS_SVC_PARAM_RC_xxx)</param>
        /// <param name="RoutineIdentifier">Server Local Routine Identifier (see PUDS_SVC_PARAM_RC_RID_xxx)</param>
        /// <param name="Buffer">buffer containing the Routine Control Options (only with start and stop routine sub-functions)</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcRoutineControl")]
        public static extern TPUDSStatus SvcRoutineControl(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            TPUDSSvcParamRC RoutineControlType, 
	        ushort RoutineIdentifier, 
	        byte[] Buffer,
	        ushort BufferLength);
        #endregion

        #region UDS Service: requestDownload
        // ISO-15765-3:2004 §9.7.1 p.57 && ISO-14229-1:2006 §14.2 p.231

        /// <summary>
        ///	The requestDownload service is used by the client to initiate a data transfer 
        ///	from the client to the server (download).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="CompressionMethod">A nibble-value that specifies the "compressionMethod",	
        ///	The value 0x0 specifies that no compressionMethod is used.</param>
        /// <param name="EncryptingMethod">A nibble-value that specifies the "encryptingMethod",
        ///	The value 0x0 specifies that no encryptingMethod is used.</param>
        /// <param name="MemoryAddress">starting address of server memory to which data is to be written</param>
        /// <param name="MemoryAddressLength">Size in bytes of the MemoryAddress buffer (max.: 0xF)</param>
        /// <param name="MemorySize">used by the server to compare the uncompressed memory size with 
        ///	the total amount of data transferred during the TransferData service</param>
        /// <param name="MemorySizeLength">Size in bytes of the MemorySize buffer (max.: 0xF)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcRequestDownload")]
        public static extern TPUDSStatus SvcRequestDownload(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
	        byte CompressionMethod,
	        byte EncryptingMethod,
	        byte[] MemoryAddress,
	        byte MemoryAddressLength,
	        byte[] MemorySize,		
	        byte MemorySizeLength);
        #endregion

        #region UDS Service: requestUpload
        // ISO-15765-3:2004 §9.7.1 p.57 && ISO-14229-1:2006 §14.3 p.234

        /// <summary>
        ///	The requestUpload service is used by the client to initiate a data transfer 
        ///	from the server to the client (upload).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="CompressionMethod">A nibble-value that specifies the "compressionMethod",	
        ///	The value 0x0 specifies that no compressionMethod is used.</param>
        /// <param name="EncryptingMethod">A nibble-value that specifies the "encryptingMethod",
        ///	The value 0x0 specifies that no encryptingMethod is used.</param>
        /// <param name="MemoryAddress">starting address of server memory from which data is to be retrieved</param>
        /// <param name="MemoryAddressLength">Size in bytes of the MemoryAddress buffer (max.: 0xF)</param>
        /// <param name="MemorySize">used by the server to compare the uncompressed memory size with 
        ///	the total amount of data transferred during the TransferData service</param>
        /// <param name="MemorySizeLength">Size in bytes of the MemorySize buffer (max.: 0xF)</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcRequestUpload")]
        public static extern TPUDSStatus SvcRequestUpload(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte CompressionMethod,
            byte EncryptingMethod,
            byte[] MemoryAddress,
            byte MemoryAddressLength,
            byte[] MemorySize,
            byte MemorySizeLength);
        #endregion

        #region UDS Service: TransferData
        // ISO-15765-3:2004 §9.7.1 p.57 && ISO-14229-1:2006 §14.4 p.237

        /// <summary>
        ///	The TransferData service is used by the client to transfer data either from the client 
        ///	to the server (download) or from the server to the client (upload).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="BlockSequenceCounter">The blockSequenceCounter parameter value starts at 01 hex
        ///	with the first TransferData request that follows the RequestDownload (34 hex) 
        ///	or RequestUpload (35 hex) service. Its value is incremented by 1 for each subsequent
        ///	TransferData request. At the value of FF hex, the blockSequenceCounter rolls over 
        ///	and starts at 00 hex with the next TransferData request message.</param>
        /// <param name="Buffer">buffer containing the required transfer parameters</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcTransferData")]
        public static extern TPUDSStatus SvcTransferData(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte BlockSequenceCounter,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #region UDS Service: RequestTransferExit
        // ISO-15765-3:2004 §9.7.1 p.57 && ISO-14229-1:2006 §14.5 p.242

        /// <summary>
        ///	The RequestTransferExit service is used by the client to terminate a data 
        ///	transfer between client and server (upload or download).
        /// </summary>
        /// <param name="CanChannel">A PUDS Channel Handle representing a PUDS-Client</param>
        /// <param name="MessageBuffer">The PUDS message (NO_POSITIVE_RESPONSE_MSG is ignored)</param>
        /// <param name="Buffer">buffer containing the required transfer parameters</param>
        /// <param name="BufferLength">Size in bytes of the buffer</param>
        /// <returns>A TPUDSStatus code. PUDS_ERROR_OK is returned on success</returns>
        [DllImport("PCAN-UDS.dll", EntryPoint = "UDS_SvcRequestTransferExit")]
        public static extern TPUDSStatus SvcRequestTransferExit(
            TPUDSCANHandle CanChannel,
            ref TPUDSMsg MessageBuffer,
            byte[] Buffer,
            ushort BufferLength);
        #endregion

        #endregion
    }
    #endregion
}