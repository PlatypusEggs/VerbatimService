using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace VerbatimService
{
    [DataContract]
    public class SpawnedDeck
    {
        [DataMember]
        public List<Card> Cards { get; set; }

        [DataMember]
        public string ImageFile { get; set; }
        [DataMember]
        public List<string> Timevals{ get; set; }
    }
}