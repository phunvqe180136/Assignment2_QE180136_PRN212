using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Services;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Repositories
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private static RoomTypeRepository? _instance;
        private static readonly object _lock = new object();
        private List<RoomType> _roomTypes;
        private readonly SqlDataService _sqlDataService;

        private RoomTypeRepository()
        {
            _sqlDataService = new SqlDataService();
            _roomTypes = new List<RoomType>();
            LoadRoomTypesAsync();
        }

        public static RoomTypeRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RoomTypeRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        private async void LoadRoomTypesAsync()
        {
            _roomTypes = await _sqlDataService.LoadRoomTypesAsync();
        }

        public List<RoomType> GetAll()
        {
            return _roomTypes.ToList();
        }

        public RoomType? GetById(int id)
        {
            return _roomTypes.FirstOrDefault(rt => rt.RoomTypeID == id);
        }

        public List<RoomType> Search(Expression<Func<RoomType, bool>> predicate)
        {
            return _roomTypes.AsQueryable().Where(predicate).ToList();
        }

        public async void Add(RoomType roomType)
        {
            // RoomTypes are managed by database schema, not through application
            throw new NotImplementedException("RoomTypes are managed by database schema");
        }

        public async void Update(RoomType roomType)
        {
            // RoomTypes are managed by database schema, not through application
            throw new NotImplementedException("RoomTypes are managed by database schema");
        }

        public async void Delete(int id)
        {
            // RoomTypes are managed by database schema, not through application
            throw new NotImplementedException("RoomTypes are managed by database schema");
        }

        public bool Exists(int id)
        {
            return _roomTypes.Any(rt => rt.RoomTypeID == id);
        }
    }
}
