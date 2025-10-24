using System.Data.SqlClient;
using System.Data;
using FUMiniHotelSystem.DataAccess.Models;
using System.Text.Json;

namespace FUMiniHotelSystem.DataAccess.Services
{
    public class SqlDataService
    {
        private readonly string _connectionString;

        public SqlDataService()
        {
            _connectionString = GetConnectionString();
        }

        private string GetConnectionString()
        {
            try
            {
                // Tìm file appsettings.json trong thư mục gốc của solution
                var currentDir = Directory.GetCurrentDirectory();
                var solutionDir = currentDir;
                
                // Tìm thư mục chứa file .sln
                while (solutionDir != null && !Directory.GetFiles(solutionDir, "*.sln").Any())
                {
                    solutionDir = Path.GetDirectoryName(solutionDir);
                }
                
                if (solutionDir == null)
                {
                    solutionDir = currentDir; // Fallback to current directory
                }
                
                var appSettingsPath = Path.Combine(solutionDir, "StudentNameWPF", "appsettings.json");
                
                if (File.Exists(appSettingsPath))
                {
                    var json = File.ReadAllText(appSettingsPath);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (config.TryGetProperty("ConnectionStrings", out var connectionStrings))
                    {
                        if (connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
                        {
                            return defaultConnection.GetString() ?? GetDefaultConnectionString();
                        }
                    }
                }
                
                return GetDefaultConnectionString();
            }
            catch
            {
                return GetDefaultConnectionString();
            }
        }

        private string GetDefaultConnectionString()
        {
            // Fallback connection strings
            return "Server=(localdb)\\mssqllocaldb;Database=FUMiniHotelManagement;Trusted_Connection=true;MultipleActiveResultSets=true;";
        }

        public async Task<List<Customer>> LoadCustomersAsync()
        {
            var customers = new List<Customer>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT CustomerID, CustomerFullName, Telephone, EmailAddress, CustomerBirthday, CustomerStatus, Password FROM Customers WHERE CustomerStatus = 1", connection);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        customers.Add(new Customer
                        {
                            CustomerID = reader.GetInt32("CustomerID"),
                            CustomerFullName = reader.GetString("CustomerFullName"),
                            Telephone = reader.GetString("Telephone"),
                            EmailAddress = reader.GetString("EmailAddress"),
                            CustomerBirthday = reader.GetDateTime("CustomerBirthday"),
                            CustomerStatus = reader.GetInt32("CustomerStatus"),
                            Password = reader.GetString("Password"),
                            IsAdmin = reader.GetString("EmailAddress") == "admin@FUMiniHotelSystem.com"
                        });
                    }
                }
            }
            
            return customers;
        }

        public async Task<bool> SaveCustomerAsync(Customer customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                if (customer.CustomerID == 0) // Insert
                {
                    var command = new SqlCommand(@"
                        INSERT INTO Customers (CustomerFullName, Telephone, EmailAddress, CustomerBirthday, CustomerStatus, Password)
                        VALUES (@FullName, @Telephone, @Email, @Birthday, @Status, @Password);
                        SELECT SCOPE_IDENTITY();", connection);
                    
                    command.Parameters.AddWithValue("@FullName", customer.CustomerFullName);
                    command.Parameters.AddWithValue("@Telephone", customer.Telephone);
                    command.Parameters.AddWithValue("@Email", customer.EmailAddress);
                    command.Parameters.AddWithValue("@Birthday", customer.CustomerBirthday);
                    command.Parameters.AddWithValue("@Status", customer.CustomerStatus);
                    command.Parameters.AddWithValue("@Password", customer.Password);
                    
                    var result = await command.ExecuteScalarAsync();
                    customer.CustomerID = Convert.ToInt32(result);
                }
                else // Update
                {
                    var command = new SqlCommand(@"
                        UPDATE Customers 
                        SET CustomerFullName = @FullName, Telephone = @Telephone, EmailAddress = @Email, 
                            CustomerBirthday = @Birthday, CustomerStatus = @Status, Password = @Password
                        WHERE CustomerID = @ID", connection);
                    
                    command.Parameters.AddWithValue("@ID", customer.CustomerID);
                    command.Parameters.AddWithValue("@FullName", customer.CustomerFullName);
                    command.Parameters.AddWithValue("@Telephone", customer.Telephone);
                    command.Parameters.AddWithValue("@Email", customer.EmailAddress);
                    command.Parameters.AddWithValue("@Birthday", customer.CustomerBirthday);
                    command.Parameters.AddWithValue("@Status", customer.CustomerStatus);
                    command.Parameters.AddWithValue("@Password", customer.Password);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                return true;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("UPDATE Customers SET CustomerStatus = 2 WHERE CustomerID = @ID", connection);
                command.Parameters.AddWithValue("@ID", customerId);
                
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<List<RoomType>> LoadRoomTypesAsync()
        {
            var roomTypes = new List<RoomType>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT RoomTypeID, RoomTypeName, TypeDescription, TypeNote FROM RoomTypes", connection);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roomTypes.Add(new RoomType
                        {
                            RoomTypeID = reader.GetInt32("RoomTypeID"),
                            RoomTypeName = reader.GetString("RoomTypeName"),
                            TypeDescription = reader.IsDBNull("TypeDescription") ? "" : reader.GetString("TypeDescription"),
                            TypeNote = reader.IsDBNull("TypeNote") ? "" : reader.GetString("TypeNote")
                        });
                    }
                }
            }
            
            return roomTypes;
        }

        public async Task<List<Room>> LoadRoomsAsync()
        {
            var rooms = new List<Room>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"
                    SELECT r.RoomID, r.RoomNumber, r.RoomDescription, r.RoomMaxCapacity, 
                           r.RoomStatus, r.RoomPricePerDate, r.RoomTypeID,
                           rt.RoomTypeName, rt.TypeDescription, rt.TypeNote
                    FROM RoomInformation r
                    LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                    WHERE r.RoomStatus = 1", connection);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rooms.Add(new Room
                        {
                            RoomID = reader.GetInt32("RoomID"),
                            RoomNumber = reader.GetString("RoomNumber"),
                            RoomDescription = reader.IsDBNull("RoomDescription") ? "" : reader.GetString("RoomDescription"),
                            RoomMaxCapacity = reader.GetInt32("RoomMaxCapacity"),
                            RoomStatus = reader.GetInt32("RoomStatus"),
                            RoomPricePerDate = reader.GetDecimal("RoomPricePerDate"),
                            RoomTypeID = reader.GetInt32("RoomTypeID"),
                            RoomType = new RoomType
                            {
                                RoomTypeID = reader.GetInt32("RoomTypeID"),
                                RoomTypeName = reader.GetString("RoomTypeName"),
                                TypeDescription = reader.IsDBNull("TypeDescription") ? "" : reader.GetString("TypeDescription"),
                                TypeNote = reader.IsDBNull("TypeNote") ? "" : reader.GetString("TypeNote")
                            }
                        });
                    }
                }
            }
            
            return rooms;
        }

        public List<Room> LoadRooms()
        {
            var rooms = new List<Room>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT r.RoomID, r.RoomNumber, r.RoomDescription, r.RoomMaxCapacity, 
                           r.RoomStatus, r.RoomPricePerDate, r.RoomTypeID,
                           rt.RoomTypeName, rt.TypeDescription, rt.TypeNote
                    FROM RoomInformation r
                    LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                    WHERE r.RoomStatus = 1", connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new Room
                        {
                            RoomID = reader.GetInt32("RoomID"),
                            RoomNumber = reader.GetString("RoomNumber"),
                            RoomDescription = reader.IsDBNull("RoomDescription") ? "" : reader.GetString("RoomDescription"),
                            RoomMaxCapacity = reader.GetInt32("RoomMaxCapacity"),
                            RoomStatus = reader.GetInt32("RoomStatus"),
                            RoomPricePerDate = reader.GetDecimal("RoomPricePerDate"),
                            RoomTypeID = reader.GetInt32("RoomTypeID"),
                            RoomType = new RoomType
                            {
                                RoomTypeID = reader.GetInt32("RoomTypeID"),
                                RoomTypeName = reader.GetString("RoomTypeName"),
                                TypeDescription = reader.IsDBNull("TypeDescription") ? "" : reader.GetString("TypeDescription"),
                                TypeNote = reader.IsDBNull("TypeNote") ? "" : reader.GetString("TypeNote")
                            }
                        });
                    }
                }
            }
            
            return rooms;
        }

        public async Task<bool> SaveRoomAsync(Room room)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                if (room.RoomID == 0) // Insert
                {
                    var command = new SqlCommand(@"
                        INSERT INTO RoomInformation (RoomNumber, RoomDescription, RoomMaxCapacity, RoomStatus, RoomPricePerDate, RoomTypeID)
                        VALUES (@Number, @Description, @Capacity, @Status, @Price, @TypeID);
                        SELECT SCOPE_IDENTITY();", connection);
                    
                    command.Parameters.AddWithValue("@Number", room.RoomNumber);
                    command.Parameters.AddWithValue("@Description", room.RoomDescription);
                    command.Parameters.AddWithValue("@Capacity", room.RoomMaxCapacity);
                    command.Parameters.AddWithValue("@Status", room.RoomStatus);
                    command.Parameters.AddWithValue("@Price", room.RoomPricePerDate);
                    command.Parameters.AddWithValue("@TypeID", room.RoomTypeID);
                    
                    var result = await command.ExecuteScalarAsync();
                    room.RoomID = Convert.ToInt32(result);
                }
                else // Update
                {
                    var command = new SqlCommand(@"
                        UPDATE RoomInformation 
                        SET RoomNumber = @Number, RoomDescription = @Description, RoomMaxCapacity = @Capacity,
                            RoomStatus = @Status, RoomPricePerDate = @Price, RoomTypeID = @TypeID
                        WHERE RoomID = @ID", connection);
                    
                    command.Parameters.AddWithValue("@ID", room.RoomID);
                    command.Parameters.AddWithValue("@Number", room.RoomNumber);
                    command.Parameters.AddWithValue("@Description", room.RoomDescription);
                    command.Parameters.AddWithValue("@Capacity", room.RoomMaxCapacity);
                    command.Parameters.AddWithValue("@Status", room.RoomStatus);
                    command.Parameters.AddWithValue("@Price", room.RoomPricePerDate);
                    command.Parameters.AddWithValue("@TypeID", room.RoomTypeID);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                return true;
            }
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("UPDATE RoomInformation SET RoomStatus = 2 WHERE RoomID = @ID", connection);
                command.Parameters.AddWithValue("@ID", roomId);
                
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<List<Booking>> LoadBookingsAsync()
        {
            var bookings = new List<Booking>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand(@"
                    SELECT b.BookingID, b.CustomerID, b.RoomID, b.CheckInDate, b.CheckOutDate, 
                           b.TotalAmount, b.BookingStatus, b.CreatedDate, b.Notes,
                           c.CustomerFullName, c.EmailAddress, c.Telephone,
                           r.RoomNumber, r.RoomDescription, r.RoomPricePerDate,
                           rt.RoomTypeName
                    FROM Bookings b
                    LEFT JOIN Customers c ON b.CustomerID = c.CustomerID
                    LEFT JOIN RoomInformation r ON b.RoomID = r.RoomID
                    LEFT JOIN RoomTypes rt ON r.RoomTypeID = rt.RoomTypeID
                    ORDER BY b.CreatedDate DESC", connection);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bookings.Add(new Booking
                        {
                            BookingID = reader.GetInt32("BookingID"),
                            CustomerID = reader.GetInt32("CustomerID"),
                            RoomID = reader.GetInt32("RoomID"),
                            CheckInDate = reader.GetDateTime("CheckInDate"),
                            CheckOutDate = reader.GetDateTime("CheckOutDate"),
                            TotalAmount = reader.GetDecimal("TotalAmount"),
                            BookingStatus = reader.GetInt32("BookingStatus"),
                            CreatedDate = reader.GetDateTime("CreatedDate"),
                            Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                            Customer = new Customer
                            {
                                CustomerID = reader.GetInt32("CustomerID"),
                                CustomerFullName = reader.GetString("CustomerFullName"),
                                EmailAddress = reader.GetString("EmailAddress"),
                                Telephone = reader.GetString("Telephone")
                            },
                            Room = new Room
                            {
                                RoomID = reader.GetInt32("RoomID"),
                                RoomNumber = reader.GetString("RoomNumber"),
                                RoomDescription = reader.IsDBNull("RoomDescription") ? "" : reader.GetString("RoomDescription"),
                                RoomPricePerDate = reader.GetDecimal("RoomPricePerDate"),
                                RoomType = new RoomType
                                {
                                    RoomTypeName = reader.GetString("RoomTypeName")
                                }
                            }
                        });
                    }
                }
            }
            
            return bookings;
        }

        public async Task<bool> SaveBookingAsync(Booking booking)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                if (booking.BookingID == 0) // Insert
                {
                    var command = new SqlCommand(@"
                        INSERT INTO Bookings (CustomerID, RoomID, CheckInDate, CheckOutDate, TotalAmount, BookingStatus, CreatedDate, Notes)
                        VALUES (@CustomerID, @RoomID, @CheckInDate, @CheckOutDate, @TotalAmount, @BookingStatus, @CreatedDate, @Notes);
                        SELECT SCOPE_IDENTITY();", connection);
                    
                    command.Parameters.AddWithValue("@CustomerID", booking.CustomerID);
                    command.Parameters.AddWithValue("@RoomID", booking.RoomID);
                    command.Parameters.AddWithValue("@CheckInDate", booking.CheckInDate);
                    command.Parameters.AddWithValue("@CheckOutDate", booking.CheckOutDate);
                    command.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
                    command.Parameters.AddWithValue("@BookingStatus", booking.BookingStatus);
                    command.Parameters.AddWithValue("@CreatedDate", booking.CreatedDate);
                    command.Parameters.AddWithValue("@Notes", booking.Notes ?? (object)DBNull.Value);
                    
                    var result = await command.ExecuteScalarAsync();
                    booking.BookingID = Convert.ToInt32(result);
                }
                else // Update
                {
                    var command = new SqlCommand(@"
                        UPDATE Bookings 
                        SET CustomerID = @CustomerID, RoomID = @RoomID, CheckInDate = @CheckInDate, 
                            CheckOutDate = @CheckOutDate, TotalAmount = @TotalAmount, BookingStatus = @BookingStatus, 
                            CreatedDate = @CreatedDate, Notes = @Notes
                        WHERE BookingID = @ID", connection);
                    
                    command.Parameters.AddWithValue("@ID", booking.BookingID);
                    command.Parameters.AddWithValue("@CustomerID", booking.CustomerID);
                    command.Parameters.AddWithValue("@RoomID", booking.RoomID);
                    command.Parameters.AddWithValue("@CheckInDate", booking.CheckInDate);
                    command.Parameters.AddWithValue("@CheckOutDate", booking.CheckOutDate);
                    command.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
                    command.Parameters.AddWithValue("@BookingStatus", booking.BookingStatus);
                    command.Parameters.AddWithValue("@CreatedDate", booking.CreatedDate);
                    command.Parameters.AddWithValue("@Notes", booking.Notes ?? (object)DBNull.Value);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                return true;
            }
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("UPDATE Bookings SET BookingStatus = 0 WHERE BookingID = @ID", connection);
                command.Parameters.AddWithValue("@ID", bookingId);
                
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
