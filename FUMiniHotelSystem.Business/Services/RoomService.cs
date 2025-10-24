using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Repositories;
using FUMiniHotelSystem.Business.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Services
{
    public class RoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;

        public RoomService()
        {
            _roomRepository = RoomRepository.Instance;
            _roomTypeRepository = RoomTypeRepository.Instance;
        }

        public List<Room> GetAllRooms()
        {
            var rooms = _roomRepository.GetAll();
            var roomTypes = _roomTypeRepository.GetAll();
            
            // Populate RoomType navigation property
            foreach (var room in rooms)
            {
                room.RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
            }
            
            return rooms;
        }

        public Room? GetRoomById(int id)
        {
            var room = _roomRepository.GetById(id);
            if (room != null)
            {
                var roomTypes = _roomTypeRepository.GetAll();
                room.RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
            }
            return room;
        }

        public List<Room> SearchRooms(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllRooms();

            var rooms = _roomRepository.Search(r => 
                r.RoomNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.RoomDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            var roomTypes = _roomTypeRepository.GetAll();
            
            // Populate RoomType navigation property
            foreach (var room in rooms)
            {
                room.RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
            }
            
            return rooms;
        }

        public List<Room> GetRoomsByType(int roomTypeId)
        {
            var rooms = _roomRepository.Search(r => r.RoomTypeID == roomTypeId);
            var roomTypes = _roomTypeRepository.GetAll();
            
            // Populate RoomType navigation property
            foreach (var room in rooms)
            {
                room.RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
            }
            
            return rooms;
        }

        public List<RoomType> GetAllRoomTypes()
        {
            return _roomTypeRepository.GetAll();
        }

        public ServiceResult AddRoom(Room room)
        {
            try
            {
                // Validation
                var validationResult = ValidateRoom(room);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if room number already exists
                var existingRoom = _roomRepository.Search(r => r.RoomNumber == room.RoomNumber).FirstOrDefault();
                if (existingRoom != null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "A room with this number already exists."
                    };
                }

                _roomRepository.Add(room);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Room added successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error adding room: {ex.Message}"
                };
            }
        }

        public ServiceResult UpdateRoom(Room room)
        {
            try
            {
                // Validation
                var validationResult = ValidateRoom(room);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if room exists
                if (!_roomRepository.Exists(room.RoomID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Room not found."
                    };
                }

                // Check if room number is taken by another room
                var existingRoom = _roomRepository.Search(r => r.RoomNumber == room.RoomNumber).FirstOrDefault();
                if (existingRoom != null && existingRoom.RoomID != room.RoomID)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "A room with this number already exists."
                    };
                }

                _roomRepository.Update(room);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Room updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error updating room: {ex.Message}"
                };
            }
        }

        public ServiceResult DeleteRoom(int id)
        {
            try
            {
                if (!_roomRepository.Exists(id))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Room not found."
                    };
                }

                _roomRepository.Delete(id);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Room deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error deleting room: {ex.Message}"
                };
            }
        }

        private ServiceResult ValidateRoom(Room room)
        {
            if (string.IsNullOrWhiteSpace(room.RoomNumber))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room number is required."
                };
            }

            if (room.RoomNumber.Length > 50)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room number cannot exceed 50 characters."
                };
            }

            if (string.IsNullOrWhiteSpace(room.RoomDescription))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room description is required."
                };
            }

            if (room.RoomDescription.Length > 220)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room description cannot exceed 220 characters."
                };
            }

            if (room.RoomMaxCapacity <= 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room max capacity must be greater than 0."
                };
            }

            if (room.RoomPricePerDate < 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room price cannot be negative."
                };
            }

            if (room.RoomTypeID <= 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room type is required."
                };
            }

            return new ServiceResult { IsSuccess = true };
        }
    }
}
