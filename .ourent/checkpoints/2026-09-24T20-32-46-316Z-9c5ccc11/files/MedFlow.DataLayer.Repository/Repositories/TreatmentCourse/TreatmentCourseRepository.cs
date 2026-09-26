using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.TreatmentCourse;

public class TreatmentCourseRepository : BaseEntityRepository<Domain.Entities.TreatmentCourse, long>, ITreatmentCourseRepository
{
    public TreatmentCourseRepository(MedFlowDbContext context) : base(context)
    {
    }
}
