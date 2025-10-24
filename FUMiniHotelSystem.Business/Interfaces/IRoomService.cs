using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Interfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<List<Room>> SearchRoomsAsync(Expression<Func<Room, bool>> predicate);
        Task<bool> AddRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(int id);
        Task<bool> RoomExistsAsync(int id);
        Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
        Task<List<Room>> GetRoomsByTypeAsync(int roomTypeId);
        Task<List<RoomType>> GetAllRoomTypesAsync();
        Task<RoomType?> GetRoomTypeByIdAsync(int id);
    }
}
