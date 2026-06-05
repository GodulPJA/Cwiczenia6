using Cwiczenia6.Data;
using Cwiczenia6.DTOs;
using Cwiczenia6.Models;
using Microsoft.EntityFrameworkCore;

namespace Cwiczenia6.Services
{
    public class DbService : IDbService
    {
        private readonly _2019sbdContext _context;

        public DbService(_2019sbdContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientDto>> GetPatientsAsync(string? search)
        {
            var query = _context.Patients
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Ward)
                .Include(p => p.BedAssignments)
                    .ThenInclude(ba => ba.Bed)
                        .ThenInclude(b => b.BedType)
                .Include(p => p.BedAssignments)
                    .ThenInclude(ba => ba.Bed)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Ward)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = $"%{search}%";
                query = query.Where(p => 
                    EF.Functions.Like(p.FirstName, searchTerm) || 
                    EF.Functions.Like(p.LastName, searchTerm));
            }

            return await query.Select(p => new PatientDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female", 
                Admissions = p.Admissions.Select(a => new AdmissionDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),
                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedDto
                    {
                        Id = ba.Bed.Id,
                        BedType = new BedTypeDto
                        {
                            Id = ba.Bed.BedType.Id,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new RoomDto
                        {
                            Id = ba.Bed.Room.Id,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new WardDto
                            {
                                Id = ba.Bed.Room.Ward.Id,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            }).ToListAsync();
        }

        public async Task<bool> DoesPatientExistAsync(string pesel)
        {
            return await _context.Patients.AnyAsync(p => p.Pesel == pesel);
        }

        public async Task<bool> IsBedAvailableAsync(string bedType, string ward, DateTime from, DateTime? to)
        {
            var maxTo = to ?? DateTime.MaxValue;
            
            return await _context.Beds
                .AnyAsync(b => 
                    b.BedType.Name == bedType && 
                    b.Room.Ward.Name == ward &&
                    !b.BedAssignments.Any(ba => ba.From < maxTo && (ba.To ?? DateTime.MaxValue) > from)
                );
        }

        public async Task AssignBedAsync(string pesel, CreateBedAssignmentDto dto)
        {
            var maxTo = dto.To ?? DateTime.MaxValue;
            
            var freeBed = await _context.Beds
                .Where(b => b.BedType.Name == dto.BedType && b.Room.Ward.Name == dto.Ward)
                .Where(b => !b.BedAssignments.Any(ba => ba.From < maxTo && (ba.To ?? DateTime.MaxValue) > dto.From))
                .FirstOrDefaultAsync();

            if (freeBed != null)
            {
                var assignment = new BedAssignment
                {
                    PatientPesel = pesel,
                    BedId = freeBed.Id,
                    From = dto.From,
                    To = dto.To
                };
                
                _context.BedAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }
        }
    }
}