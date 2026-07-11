
using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.TestUser;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories
{

    public interface ITestUserRepository : IBaseRepository<TestUser>
    {
        Task<PagedResult<TestUser>> GetAll(TestUserFilterParams filterParams);
    }
}