using Cwiczenia6.DTOs;

namespace Cwiczenia6.Services
{
    public interface IDbService
    {
        Task<IEnumerable<PatientDto>> GetPatientsAsync(string? search);
        Task<bool> DoesPatientExistAsync(string pesel);
        Task<bool> IsBedAvailableAsync(string bedType, string ward, DateTime from, DateTime? to);
        Task AssignBedAsync(string pesel, CreateBedAssignmentDto dto);
        
    }
}