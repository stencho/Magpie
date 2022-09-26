using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Stages {
    public static class StageExporter {
        //I should have known it would not be this simple, this needs to be replaced with a more reasonable system
        public static void serialize_map(Map map, string filename) {
            FileStream map_stream = new FileStream(filename, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(map_stream, map);
            map_stream.Close();
        }
    }
}
