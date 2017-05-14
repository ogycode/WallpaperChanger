using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WallpaperChanger2.Model
{
    [DataContract(Name = "Image")]
    public class BingImage
    {
        [DataMember(Name = "startdate")]
        public string StartDate { get; set; }
        [DataMember(Name = "fullstartdate")]
        public string FullStartDate { get; set; }
        [DataMember(Name = "enddate")]
        public string EndDate { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }
        [DataMember(Name = "urlbase")]
        public string UrlBase { get; set; }
        [DataMember(Name = "copyright")]
        public string Copyright { get; set; }
        [DataMember(Name = "copyrightlink")]
        public string copyrightlink { get; set; }
        [DataMember(Name = "quiz")]
        public string quiz { get; set; }
        [DataMember(Name = "wp")]
        public bool wp { get; set; }
        [DataMember(Name = "hsh")]
        public string hsh { get; set; }
        [DataMember(Name = "drk")]
        public int drk { get; set; }
        [DataMember(Name = "top")]
        public int top { get; set; }
        [DataMember(Name = "bot")]
        public int bot { get; set; }
        [DataMember(Name = "hs")]
        public List<object> hs { get; set; }
    }
}
