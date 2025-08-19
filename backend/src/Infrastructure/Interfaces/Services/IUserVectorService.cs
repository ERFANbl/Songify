using Domain.DTOs.UserVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Services
{
    public interface IUserVectorService
    {
        public Task UpdateUserVectorAsync(int userId, UserInteractionsDTO userInteractions);
    }
}
