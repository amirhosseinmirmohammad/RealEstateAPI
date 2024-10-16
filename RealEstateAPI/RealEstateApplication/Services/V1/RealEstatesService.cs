using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealEstateApplication.ViewModels;
using RealEstateCore.Enums;
using RealEstateCore.Interfaces.V1;
using RealEstateCore.Models;
using System.Data;
using RealEstateApplication.Services.Helpers;
namespace RealEstateApplication.Services.V1
{
    public class RealEstatesService
    {
        public RealEstatesService(IRealEstateRepository repository,
                                  ILogger<RealEstatesService> logger,
                                  IOptions<DatabaseSettings> dbSettings,
                                  ILuceneEngine<RealEstate> luceneEngine) 
        {
            _repository = repository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = dbSettings.Value.RealEstateConnection;
            _luceneEngine = luceneEngine ?? throw new ArgumentNullException(nameof(luceneEngine));
        }

        public async Task<ResponseModel<RealEstate>> GetRealEstateByIdAsync(int id)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(id);
                if (realEstate != null)
                {
                    _logger.LogInformation("RealEstate with ID {RealEstateId}", id);

                    return new ResponseModel<RealEstate>
                    {
                        StatusCode = (int)ResponseStatus.Success,
                        Data = realEstate,
                        Message = "Real estate retrieved successfully"
                    };
                }

                return new ResponseModel<RealEstate>
                {
                    StatusCode = (int)ResponseStatus.NotFound,
                    Data = null,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real estate");
                return new ResponseModel<RealEstate>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = null,
                    Message = "Error retrieving real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetSimilarTitlesWithFunctionAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Message = "Input cannot be empty.",
                };
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = @"
                                WITH Similarity AS (
                                    SELECT Title, 
                                           100 * (1 - CAST(dbo.LevenshteinPersian(Title, @Input) AS FLOAT) / 
                                               CASE 
                                                   WHEN LEN(Title) > LEN(@Input) THEN LEN(Title)
                                                   ELSE LEN(@Input)
                                               END) AS SimilarityPercentage
                                    FROM [dbo].[RealEstates]
                                )
                                SELECT Title
                                FROM Similarity
                                WHERE SimilarityPercentage >= 50;";

                var result = await db.QueryAsync<RealEstate>(query, new { Input = input });

                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = (int)ResponseStatus.Success,
                    Message = "Data fetched successfully.",
                    Data = result
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetSimilarTitlesWithFreeText(string input)
        {
            var response = new ResponseModel<IEnumerable<RealEstate>>();

            if (string.IsNullOrWhiteSpace(input))
            {
                response.StatusCode = (int)ResponseStatus.NotFound;
                response.Message = "Input search term cannot be empty.";
                return response;
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                                SELECT * 
                                FROM RealEstates 
                                WHERE FREETEXT(Title, @Input) 
                                OR Title LIKE '%' + @Input + '%'";

                    var result = await connection.QueryAsync<RealEstate>(query, new { Input = input });

                    response.StatusCode = (int)ResponseStatus.Success;
                    response.Data = result;
                    response.Message = "Data fetched successfully.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)ResponseStatus.ServerError;
                response.Message = "An error occurred while searching for similar titles.";
                response.Exception = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<int>> AddRealEstateAsync(RealEstateViewModel viewModel)
        {
            try
            {
                var realEstate = new RealEstate
                {
                    Title = viewModel.Title,
                    Status = viewModel.Status,
                    Price = viewModel.Price,
                    Floor = viewModel.Floor,
                    UserId = "UserId", // Set to the actual user ID
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Use the generic type directly
                var id = await _repository.AddRealEstateAsync(realEstate);

                // Add index using the generic type
                _luceneEngine.AddIndex(realEstate); 

                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.Success,
                    Data = id,
                    Message = "Real estate added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding real estate");
                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = 0,
                    Message = "Error adding real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> ArchiveRealEstateAsync(int id)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(id);
                if (realEstate != null)
                {
                    realEstate.Status = RealEstateStatus.Archived;
                    realEstate.UpdatedAt = DateTime.UtcNow;

                    realEstate.ChangeLogs.Add(new ChangeLog
                    {
                        ChangeDate = DateTime.UtcNow,
                        Description = "Property archived"
                    });

                    await _repository.UpdateRealEstateAsync(realEstate);

                    _logger.LogInformation("Archived RealEstate with ID {RealEstateId}", id);

                    return new ResponseModel<int>
                    {
                        StatusCode = (int)ResponseStatus.Success,
                        Data = id,
                        Message = "Real estate archived successfully"
                    };
                }

                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.NotFound,
                    Data = 0,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving real estate");
                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = 0,
                    Message = "Error archiving real estate",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<int>> UpdateRealEstateTimeAsync(int id)
        {
            try
            {
                var realEstate = await _repository.GetRealEstateByIdAsync(id);
                if (realEstate != null)
                {
                    realEstate.UpdatedAt = DateTime.UtcNow;

                    realEstate.ChangeLogs.Add(new ChangeLog
                    {
                        ChangeDate = DateTime.UtcNow,
                        Description = "Updated property time"
                    });

                    await _repository.UpdateRealEstateAsync(realEstate);

                    _luceneEngine.UpdateIndex(realEstate, "title"); // Update Lucene index

                    _logger.LogInformation("Updated time for RealEstate with ID {RealEstateId}", id);

                    return new ResponseModel<int>
                    {
                        StatusCode = (int)ResponseStatus.Success,
                        Data = id,
                        Message = "Real estate time updated successfully"
                    };
                }

                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.NotFound,
                    Data = 0,
                    Message = "Real estate not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating real estate time");
                return new ResponseModel<int>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = 0,
                    Message = "Error updating real estate time",
                    Exception = ex.Message
                };
            }
        }

        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetAllRealEstatesAsync()
        {
            try
            {
                var allRealEstates = await _repository.GetAllRealEstatesAsync();
                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = (int)ResponseStatus.Success,
                    Data = allRealEstates.OrderByDescending(r => r.UpdatedAt),
                    Message = "Real estate list retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real estate list");
                return new ResponseModel<IEnumerable<RealEstate>>
                {
                    StatusCode = (int)ResponseStatus.ServerError,
                    Data = null,
                    Message = "Error retrieving real estate list",
                    Exception = ex.Message
                };
            }
        }

        private readonly string _connectionString;
        private readonly IRealEstateRepository _repository;
        private readonly ILogger<RealEstatesService> _logger;
        private readonly ILuceneEngine<RealEstate> _luceneEngine; 
    }
}
