using System.Text.Json;
using System.Text;
using ReservationService.DTOs;

namespace ReservationService.Services
{

    // Service for communicating with the external Room API via HTTP

    public class RoomService : IRoomService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RoomService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RoomService(HttpClient httpClient, IConfiguration configuration, ILogger<RoomService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Retrieves a specific room by ID from the Room API
        public async Task<RoomDto?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug("Fetching room with ID: {RoomId}", id);

                var response = await _httpClient.GetAsync($"api/rooms/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Room with ID {RoomId} not found", id);
                    return CreateFallbackRoom(id);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve room {RoomId}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        id, response.StatusCode, response.ReasonPhrase);
                    return CreateFallbackRoom(id);
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Empty response received for room {RoomId}", id);
                    return CreateFallbackRoom(id);
                }

                var room = JsonSerializer.Deserialize<RoomDto>(json, _jsonOptions);
                _logger.LogDebug("Successfully retrieved room {RoomId}: {RoomName}", id, room?.Name);

                return room ?? CreateFallbackRoom(id);
            }
            catch (HttpRequestException ex) when (ex.InnerException is System.Security.Authentication.AuthenticationException)
            {
                _logger.LogError(ex, "SSL authentication failed for room {RoomId}. Using fallback room.", id);
                return CreateFallbackRoom(id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while retrieving room {RoomId}. Using fallback room.", id);
                return CreateFallbackRoom(id);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while retrieving room {RoomId}. Using fallback room.", id);
                return CreateFallbackRoom(id);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while retrieving room {RoomId}. Using fallback room.", id);
                return CreateFallbackRoom(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving room {RoomId}. Using fallback room.", id);
                return CreateFallbackRoom(id);
            }
        }

        // Get all available rooms from the Room API
        public async Task<IEnumerable<RoomDto>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all rooms");

                var response = await _httpClient.GetAsync("rooms");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to retrieve rooms. Status: {StatusCode}, Reason: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return Enumerable.Empty<RoomDto>();
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Empty response received for all rooms");
                    return Enumerable.Empty<RoomDto>();
                }

                var rooms = JsonSerializer.Deserialize<IEnumerable<RoomDto>>(json, _jsonOptions);
                var roomList = rooms?.ToList() ?? new List<RoomDto>();

                _logger.LogDebug("Successfully retrieved {Count} rooms", roomList.Count);
                return roomList;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while retrieving all rooms");
                return Enumerable.Empty<RoomDto>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while retrieving all rooms");
                return Enumerable.Empty<RoomDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while retrieving all rooms");
                return Enumerable.Empty<RoomDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving all rooms");
                return Enumerable.Empty<RoomDto>();
            }
        }

        // Check if a specific room type exists
        public async Task<bool> ExistsAsync(Guid roomTypeId)
        {
            try
            {
                _logger.LogDebug("Checking if room {RoomId} exists", roomTypeId);

                var response = await _httpClient.GetAsync($"rooms/{roomTypeId}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Room {RoomId} does not exist", roomTypeId);
                    return false;
                }

                var exists = response.IsSuccessStatusCode;
                _logger.LogDebug("Room {RoomId} exists: {Exists}", roomTypeId, exists);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room {RoomId} exists", roomTypeId);
                return false;
            }
        }

        // Get multiple rooms by their IDs
        public async Task<IEnumerable<RoomDto>> GetByIdsAsync(IEnumerable<Guid> roomIds)
        {
            if (roomIds == null || !roomIds.Any())
            {
                _logger.LogDebug("No room IDs provided");
                return Enumerable.Empty<RoomDto>();
            }

            var rooms = new List<RoomDto>();
            var roomIdList = roomIds.ToList();

            _logger.LogDebug("Fetching {Count} rooms by IDs", roomIdList.Count);
            var tasks = roomIdList.Select(async id =>
            {
                var room = await GetByIdAsync(id);
                return room;
            });

            var results = await Task.WhenAll(tasks);
            rooms.AddRange(results.Where(r => r != null)!);

            _logger.LogDebug("Successfully retrieved {Count} out of {RequestedCount} rooms",
                rooms.Count, roomIdList.Count);

            return rooms;
        }
        
        // Creates a fallback room object when room data cannot be retrieved
        private RoomDto CreateFallbackRoom(Guid roomId)
        {
            return new RoomDto
            {
                Id = roomId,
                Name = "Room Service Unavailable",
            };
        }
    }

}