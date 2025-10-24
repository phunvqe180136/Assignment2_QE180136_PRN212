using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IRoomTypeRepository
    {
        List<RoomType> GetAll();
        RoomType? GetById(int id);
        List<RoomType> Search(Expression<Func<RoomType, bool>> predicate);
        void Add(RoomType roomType);
        void Update(RoomType roomType);
        void Delete(int id);
        bool Exists(int id);
    }
}
