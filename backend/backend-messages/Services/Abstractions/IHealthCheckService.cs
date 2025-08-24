namespace BackendMessages.Services.Abstractions
{
    /// <summary>
    /// Interface for health check operations.
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Checks the basic health status of the service.
        /// </summary>
        /// <returns>Basic health status information</returns>
        Task<object> GetBasicHealthAsync();

        /// <summary>
        /// Checks the database health status.
        /// </summary>
        /// <returns>Database health status information</returns>
        Task<object> GetDatabaseHealthAsync();
    }
}
