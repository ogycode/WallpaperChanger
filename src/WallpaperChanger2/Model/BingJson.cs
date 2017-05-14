using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WallpaperChanger2.Model
{
    [DataContract(Name = "BingJson")]
    public class BingJson
    {
        [DataMember(Name = "images")]
        public List<BingImage> Images { get; set; }
        [DataMember(Name = "tooltips")]
        public object Tooltips { get; set; }
    }
}
