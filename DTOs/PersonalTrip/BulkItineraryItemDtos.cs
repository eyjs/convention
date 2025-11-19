namespace LocalRAG.DTOs.PersonalTrip
{
    public class BulkDeleteItineraryItemsDto
    {
        public List<int> ItemIds { get; set; } = new List<int>();
    }

    public class UpdateItemDayDto
    {
        public int ItemId { get; set; }
        public int NewDayNumber { get; set; }
    }

    public class BulkUpdateItineraryItemsDayDto
    {
        public List<UpdateItemDayDto> Updates { get; set; } = new List<UpdateItemDayDto>();
    }
}
