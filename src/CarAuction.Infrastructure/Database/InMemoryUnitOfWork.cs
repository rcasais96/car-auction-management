using CarAuction.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Database
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }
}
