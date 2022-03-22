using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Stages {
    static class StageImporter {
        public static Map deserialize_map(string filename) {
            Map m;
            FileStream map_stream = new FileStream(filename, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            m = (Map)bf.Deserialize(map_stream);
            return m;
        }
    }
}
