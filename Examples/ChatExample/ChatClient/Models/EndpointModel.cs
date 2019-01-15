using System.Net;

namespace ChatClient.Models
{
    public class EndpointModel
    {
        public string Description { get; set; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
    }
}