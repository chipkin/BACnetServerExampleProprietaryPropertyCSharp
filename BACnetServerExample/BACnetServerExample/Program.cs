﻿/*
 * Windows BACnet Server Example Proprietary Property Sharp
 * ----------------------------------------------------------------------------
 * Program.cs
 * 
 * A BACnet server example that shows how to add proprietary property to an object 
 *
 * More information https://github.com/chipkin/BACnetServerExampleProprietaryPropertyCSharp
 * 
 * Created by: Steven Smethurst 
 * Created on: June 24, 2019 
 * Last updated: June 24, 2019 
*/


using BACnetStackDLLServerCSharpExample;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace BACnetServerExample
{
    class Program
    {
        // Main function
        static void Main(string[] args)
        {
            BACnetServer bacnetServer = new BACnetServer();
            bacnetServer.Run();
        }

        // BACnet Server Object
        unsafe class BACnetServer
        {
            // UDP 
            UdpClient udpServer;
            IPEndPoint RemoteIpEndPoint;

            // Set up the BACnet port 
            const UInt16 SETTING_BACNET_PORT = 47808;

            // A Database to hold the current state of the 
            private ExampleDatabase database = new ExampleDatabase();

            // Version 
            const string APPLICATION_VERSION = "0.0.1";

            // Server setup and main loop
            public void Run()
            {
                Console.WriteLine("Starting Windows-BACnetServerExampleProprietaryPropertyCSharp version {0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("https://github.com/chipkin/BACnetServerExampleProprietaryPropertyCSharp");
                Console.WriteLine("FYI: BACnet Stack version: {0}.{1}.{2}.{3}",
                    CASBACnetStackAdapter.GetAPIMajorVersion(),
                    CASBACnetStackAdapter.GetAPIMinorVersion(),
                    CASBACnetStackAdapter.GetAPIPatchVersion(),
                    CASBACnetStackAdapter.GetAPIBuildVersion());

                // 1. Setup the callbacks
                // ---------------------------------------------------------------------------

                // Send/Recv callbacks. 
                CASBACnetStackAdapter.RegisterCallbackSendMessage(SendMessage);
                CASBACnetStackAdapter.RegisterCallbackReceiveMessage(RecvMessage);
                CASBACnetStackAdapter.RegisterCallbackGetSystemTime(CallbackGetSystemTime);

                // Get Datatype Callbacks 
                CASBACnetStackAdapter.RegisterCallbackGetPropertyCharacterString(CallbackGetPropertyCharString);

                // Set Datatype Callbacks 
                CASBACnetStackAdapter.RegisterCallbackSetPropertyCharacterString(CallbackSetPropertyCharacterString);

                // 2. Setup the BACnet device
                // ---------------------------------------------------------------------------

                // Initialize database
                this.database.Setup();

                // Add the device
                CASBACnetStackAdapter.AddDevice(this.database.Device.instance);
                CASBACnetStackAdapter.SetProprietaryProperty(this.database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_DEVICE, this.database.Device.instance, 512 + 1, false, false, CASBACnetStackAdapter.DATA_TYPE_CHARACTER_STRING, false, false, false);
                CASBACnetStackAdapter.SetProprietaryProperty(this.database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_DEVICE, this.database.Device.instance, 512 + 2, true, true, CASBACnetStackAdapter.DATA_TYPE_CHARACTER_STRING, false, false, false);

                // Enable optional services 
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_READ_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY_MULTIPLE, true);

                // All done with the BACnet setup
                Console.WriteLine("FYI: CAS BACnet Stack Setup, successfuly");

                // 3. Open the BACnet port to receive messages
                // ---------------------------------------------------------------------------
                this.udpServer = new UdpClient(SETTING_BACNET_PORT);
                this.RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // 4. Main loop
                // ---------------------------------------------------------------------------
                Console.WriteLine("FYI: Starting main loop");
                for (; ; )
                {
                    CASBACnetStackAdapter.Loop(); // CAS BACnet stack loop

                    database.Loop(); // Update database values
                }
            }

            // Not used
            private void DoUserInput()
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.F1:
                            Console.WriteLine("FYI: BACnet Stack version: {0}.{1}.{2}.{3}",
                                CASBACnetStackAdapter.GetAPIMajorVersion(),
                                CASBACnetStackAdapter.GetAPIMinorVersion(),
                                CASBACnetStackAdapter.GetAPIPatchVersion(),
                                CASBACnetStackAdapter.GetAPIBuildVersion());
                            break;
                        default:
                            break;
                    }
                }
            }

            // Callback used by the BACnet Stack to get the current time
            public ulong CallbackGetSystemTime()
            {
                // https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
                return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            // Callback used by the BACnet Stack to send a BACnet message
            public UInt16 SendMessage(System.Byte* message, UInt16 messageLength, System.Byte* connectionString, System.Byte connectionStringLength, System.Byte networkType, Boolean broadcast)
            {
                if (connectionStringLength < 6 || messageLength <= 0)
                {
                    return 0;
                }
                // Extract the connection string into a IP address and port. 
                IPAddress ipAddress = new IPAddress(new byte[] { connectionString[0], connectionString[1], connectionString[2], connectionString[3] });
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, (connectionString[4] + connectionString[5] * 256));

                // Debug 
                Console.WriteLine("FYI: Sending {0} bytes to {1}", messageLength, ipEndPoint.ToString());

                // Copy from the unsafe pointer to a Byte array. 
                byte[] sendBytes = new byte[messageLength];
                Marshal.Copy((IntPtr)message, sendBytes, 0, messageLength);

                try
                {
                    this.udpServer.Send(sendBytes, sendBytes.Length, ipEndPoint);
                    return (UInt16)sendBytes.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                return 0;
            }

            // Callback used by the BACnet Stack to check if there is a message to process
            public UInt16 RecvMessage(System.Byte* message, UInt16 maxMessageLength, System.Byte* receivedConnectionString, System.Byte maxConnectionStringLength, System.Byte* receivedConnectionStringLength, System.Byte* networkType)
            {
                try
                {
                    if (this.udpServer.Available > 0)
                    {
                        // Data buffer for incoming data.  
                        byte[] receiveBytes = this.udpServer.Receive(ref this.RemoteIpEndPoint);
                        byte[] ipAddress = RemoteIpEndPoint.Address.GetAddressBytes();
                        byte[] port = BitConverter.GetBytes(UInt16.Parse(RemoteIpEndPoint.Port.ToString()));

                        // Copy from the unsafe pointer to a Byte array. 
                        Marshal.Copy(receiveBytes, 0, (IntPtr)message, receiveBytes.Length);

                        // Copy the Connection string 
                        Marshal.Copy(ipAddress, 0, (IntPtr)receivedConnectionString, 4);
                        Marshal.Copy(port, 0, (IntPtr)receivedConnectionString + 4, 2);
                        *receivedConnectionStringLength = 6;

                        // Debug 
                        Console.WriteLine("FYI: Recving {0} bytes from {1}", receiveBytes.Length, RemoteIpEndPoint.ToString());

                        // Return length. 
                        return (ushort)receiveBytes.Length;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                return 0;
            }

            // Update a string and return its size
            private UInt32 UpdateStringAndReturnSize(System.Char* value, UInt32 maxElementCount, string stringAsValue)
            {
                byte[] nameAsBuffer = ASCIIEncoding.ASCII.GetBytes(stringAsValue);
                UInt32 valueElementCount = maxElementCount;
                if (nameAsBuffer.Length < valueElementCount)
                {
                    valueElementCount = Convert.ToUInt32(nameAsBuffer.Length);
                }
                Marshal.Copy(nameAsBuffer, 0, (IntPtr)value, Convert.ToInt32(valueElementCount));
                return valueElementCount;
            }

            // Read string from char pointer
            private string GetStringFromCharPointer(System.Char* value, UInt32 maxElementCount, Byte encodingType)
            {
                byte[] nameAsBuffer = new Byte[maxElementCount];
                Marshal.Copy((IntPtr)value, nameAsBuffer, 0, Convert.ToInt32(maxElementCount));
                return ASCIIEncoding.ASCII.GetString(nameAsBuffer);
            }

            // Callback used by the BACnet Stack to set Charstring property values to the user
            public bool CallbackGetPropertyCharString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, System.Char* value, UInt32* valueElementCount, UInt32 maxElementCount, System.Byte encodingType, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyCharString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_MODEL_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.modelName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_VENDOR_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.vendorName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_APPLICATIONSOFTWAREVERSION)
                            {
                                string version = APPLICATION_VERSION + "." + CIBuildVersion.CIBUILDNUMBER;
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, version);
                                return true;
                            }
                            else if (propertyIdentifier == 512 + 1)
                            {
                                string version = database.Device.proprietaryProperty513;
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, version);
                                return true;
                            }
                            else if (propertyIdentifier == 512 + 2)
                            {
                                string version = database.Device.proprietaryProperty514;
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, version);
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; // Could not handle this request. 
            }

            // Callback used by the BACnet Stack to set Charstring property values to the user
            bool CallbackSetPropertyCharacterString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, char* value, UInt32 length, Byte encodingType, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyCharacterString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == 512 + 2)
                            {
                                database.Device.proprietaryProperty514 = GetStringFromCharPointer(value, length, encodingType);
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }
                
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
        }
    }
}
