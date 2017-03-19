namespace Warhammer.Mvc.Models
{
    public class SuspendPlayerItemViewModel
    {
        public int SessionId { get; set; }
        public int PlayerId { get; set; }
        public bool PlayerSuspended { get; set; }
        public string PlayerName { get; set; }
    }
}