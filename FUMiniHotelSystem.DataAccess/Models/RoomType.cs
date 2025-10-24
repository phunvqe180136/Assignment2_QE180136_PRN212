namespace FUMiniHotelSystem.DataAccess.Models
{
    public class RoomType
    {
        public int RoomTypeID { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public string TypeNote { get; set; } = string.Empty;
    }
}
