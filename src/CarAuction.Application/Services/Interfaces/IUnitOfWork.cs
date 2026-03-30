using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Services.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
