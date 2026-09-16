using System;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
