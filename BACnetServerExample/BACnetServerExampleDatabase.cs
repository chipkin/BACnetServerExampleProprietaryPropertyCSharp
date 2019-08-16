using CASBACnetStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACnetServerExample
{
    class ExampleDatabase
    {
        public class ExampleDatabaseBase
        {
            public String name;
        }
        public class ExampleDatabaseDevice : ExampleDatabaseBase
        {
            public String description;
            public UInt32 instance;
            public String modelName;
            public UInt32 vendorIdentifiier;
            public String vendorName;
            public String proprietaryProperty513;
            public String proprietaryProperty514;
        }

        public ExampleDatabaseDevice Device;

        public void Setup()
        {
            this.Device = new ExampleDatabaseDevice();

            // Default Values 
            this.Device.name = "Device name Rainbow";
            this.Device.instance = 389001;
            this.Device.description = "This is the example description";
            this.Device.vendorIdentifiier = 389; // 389 is Chipkin's vendorIdentifiier
            this.Device.vendorName = "Chipkin Automation Systems";
            this.Device.modelName = "Windows-BACnetServerExampleCSharp";
            this.Device.proprietaryProperty513 = "Proprietary property 513 readonly";
            this.Device.proprietaryProperty514 = "Proprietary property 514 read/write";

        }

        public void Loop()
        {

        }
    }
}
