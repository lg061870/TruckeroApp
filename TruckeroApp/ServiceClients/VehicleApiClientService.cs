using System.Net.Http.Json;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients
{
    public class VehicleApiClientService : IVehicleService
    {
        private readonly HttpClient _http;

        public VehicleApiClientService(HttpClient http)
        {
            _http = http;
        }

        public async Task AddAsync(Truck vehicle)
        {
            var response = await _http.PostAsJsonAsync("/api/vehicle", vehicle);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<Truck>> GetVehiclesForDriverAsync(Guid driverProfileId)
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<Truck>>($"/api/vehicle/driver/{driverProfileId}");
            return result ?? Enumerable.Empty<Truck>();
        }

        public async Task DeleteAsync(Guid vehicleId)
        {
            var response = await _http.DeleteAsync($"/api/vehicle/{vehicleId}");
            response.EnsureSuccessStatusCode();
        }

        // Optional: Update method if needed
        public async Task UpdateAsync(Truck vehicle)
        {
            var response = await _http.PutAsJsonAsync($"/api/vehicle/{vehicle.Id}", vehicle);
            response.EnsureSuccessStatusCode();
        }
    }
}