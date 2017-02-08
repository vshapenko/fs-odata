using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.OData;
using ODataClient;

namespace Interop
{
    public class MetadataType
    {
        
    }

    public class Input
    {
        
    }

    public class Output
    {

    }
    class Program
    {
        static void Main(string[] args)
        {
            var parserSettings = new Facade.ParserSettings<Output, Input, MetadataType>(stream => new Output[0], exception => Console.WriteLine("Web:{0}", exception.Message), exception => Console.WriteLine("Fatal:{0}", exception.Message), o => new ODataEntry(),model =>new MetadataType[0] );
        }
    }
}
