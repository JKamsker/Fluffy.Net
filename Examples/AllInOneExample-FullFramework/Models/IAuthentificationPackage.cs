namespace AllInOneExample_FullFramework.Models
{
    public interface IAuthenticationPacket
    {
    }

    public class NamePasswordAuthentication : IAuthenticationPacket
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}