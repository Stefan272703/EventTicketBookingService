using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Models
{
    public class Event
    {
        [Required(ErrorMessage = "ID обязательное для заполнения")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Title обязательное для заполнения")]
        public string Title {  get; set; }
        public string Description { get; set; } = string.Empty;
        [Required(ErrorMessage = "StartAt обязательное для заполнения")]
        public DateTime StartAt { get;set;  }
        [Required(ErrorMessage = "EndAt обязательное для заполнения")]
        public DateTime EndAt { get;set;  }
    }
}
