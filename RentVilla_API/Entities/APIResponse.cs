using RentVilla_API.Helpers;
using System.Net;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace RentVilla_API.Entities
{
    public class APIResponse
    {

        public APIResponse() 
        {
            ErrorMessages = new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool ResponseIsSuccessfull { get; set; } = true;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> ErrorMessages { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Result { get; set; }

        public bool ShouldSerializeErrorMessages() => JsonSerialization.ShouldSerializeProperty(this, nameof(ErrorMessages));
        public bool ShouldSerializeResult() => JsonSerialization.ShouldSerializeProperty(this, nameof(Result));

    }
}
