using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IRoomRepository
    {
        List<Room> GetAll();
        Room? GetById(int id);
        List<Room> Search(Expression<Func<Room, bool>> predicate);
        void Add(Room room);
        void Update(Room room);
        void Delete(int id);
        bool Exists(int id);
    }
}
