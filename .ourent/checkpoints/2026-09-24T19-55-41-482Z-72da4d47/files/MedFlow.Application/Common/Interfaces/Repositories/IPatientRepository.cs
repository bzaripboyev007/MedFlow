using MedFlow.Domain.Entities;

namespace MedFlow.Application.Common.Interfaces.Repositories;

public interface IPatientRepository : IBaseRepository<Patient, long>
{
}
