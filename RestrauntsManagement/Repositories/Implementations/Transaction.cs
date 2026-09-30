using System;
using System.Data.Entity;
using DotNetRestaurantManagement.Repositories.Interfaces;

namespace DotNetRestaurantManagement.Repositories
{
    public class Transaction : ITransaction
    {
        private readonly DbContextTransaction _transaction;

        /// <summary>
        /// Initializes the transaction with the given database transaction
        /// </summary>
        public Transaction(DbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        public void Commit()
        {
            _transaction.Commit();
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        public void Rollback()
        {
            _transaction.Rollback();
        }

        /// <summary>
        /// Releases the resources used by the transaction
        /// </summary>
        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}
