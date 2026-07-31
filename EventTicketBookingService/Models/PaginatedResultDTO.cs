namespace EventTicketBookingService.Models
{
    public class PaginatedResultDTO<T>
    {
        public int TotalCount { get; set; }         // Общее количество событий
        public IEnumerable<T>? Events { get; set; } // Массив (коллекция) событий
        public int PageIndex { get; set; }          // Номер текущей страницы
        public int PageSizeByIndex {  get; set; }   // Количество элементов на текущей странице
    }
}
