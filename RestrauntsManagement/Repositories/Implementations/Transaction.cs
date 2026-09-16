using System;
using System.Data.Entity;
using DotNetRestaurantManagement.Repositories.Interfaces;

namespace DotNetRestaurantManagement.Repositories
{
    public class Transaction : ITransaction
    {
        private readonly DbContextTransaction _transaction;
        public Transaction(DbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}
