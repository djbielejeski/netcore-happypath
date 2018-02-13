using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data.Models
{
    public class ListValidationResult
    {
        public ListValidationResult()
        {
            ValidationResultMessages = new List<string>();
        }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<string> ValidationResultMessages { get; set; }
    }
}
