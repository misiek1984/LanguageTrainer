using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MK.Utilities
{
    public enum SerializationFormat 
    {
        ObjectDumper,
        XmlSerializer,
        BinaryFormatter,
        DataContractSerializer,
        SoapFormatter
    }
}
