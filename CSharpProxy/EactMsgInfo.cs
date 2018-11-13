using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpProxy
{
    public class EactMsgInfo
    {
        public string Msg { get; set; }
        public int Type { get; set; }

        public string SerializeObject()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static EactMsgInfo DeserializeObject(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<EactMsgInfo>(str)??new EactMsgInfo();
        }
    }
}
