# BACnet Server Example Proprietary Property C#

A basic BACnet IP server example written in C# using the [CAS BACnet Stack](https://store.chipkin.com/services/stacks/bacnet-stack). We show how to add a proprietary property to a device object in this example. 

## Releases

Build versions of this example can be downloaded from the [Releases](https://github.com/chipkin/BACnetServerExampleProprietaryPropertyCSharp/releases) page.

## Installation

Download the latest release zip file on the [Releases](https://github.com/chipkin/BACnetServerExampleProprietaryPropertyCSharp/releases) page.
Copy CASBACnetStack_x64_Release.dll from the CAS BACnet Stack into the release folder. Please contact Chipkin Automation Systems for access to the CAS BACnet Stack. Launch client by using the following command:
```
dotnet BACnetServerExample.dll
```
Requires [.NET Core 3.0+](https://dotnet.microsoft.com/download)

## Usage
Pre-configured with the following example BACnet device and properties
- device: 389001  (Device name Rainbow)
  - Property 513 = "Proprietary property 513 readonly" (CharString)
  - Property 514 = "Proprietary property 514 read/write" (CharString)

## Build

A [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) project is included with this project. This project also auto built using [Gitlab CI](https://docs.gitlab.com/ee/ci/) on every commit.

1. Copy *CASBACnetStack_x64_Debug.dll*, *CASBACnetStack_x64_Debug.lib*, *CASBACnetStack_x64_Release.dll*, and *CASBACnetStack_x64_Release.lib* from the [CAS BACnet Stack](https://store.chipkin.com/services/stacks/bacnet-stack) project into the /bin/netcoreapp2.1/ folder.
2. Use [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) to build the project. The solution can be found in the */BACnetServerExampleProprietaryPropertyCSharp/BACnetServerExample/* folder.


## Example output
```txt
Starting Windows-BACnetServerExampleProprietaryPropertyCSharp version0.0.1.0
https://github.com/chipkin/BACnetServerExampleProprietaryPropertyCSharp
FYI: BACnet Stack version: 3.8.3.0
FYI: CAS BACnet Stack Setup, successfuly
FYI: Starting main loop
FYI: Recving 12 bytes from 192.168.1.26:47808
FYI: Sending 25 bytes to 192.168.1.26:47808
FYI: Recving 19 bytes from 192.168.1.26:47808
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=12, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=44, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=70, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=77, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=121, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=513, propertyArrayIndex=0
FYI: Request for CallbackGetPropertyCharString. objectType=8, objectInstance=389001, propertyIdentifier=514, propertyArrayIndex=0
FYI: Sending 340 bytes to 192.168.1.26:47808
FYI: Recving 19 bytes from 192.168.1.26:47808
FYI: Sending 25 bytes to 192.168.1.26:47808
```