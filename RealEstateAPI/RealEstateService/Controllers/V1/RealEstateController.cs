using Elsa.Http;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Services;
using Microsoft.AspNetCore.Mvc;
using RealEstateApplication.Services.Helpers;
using RealEstateApplication.Services.V1;
using RealEstateApplication.ViewModels;
using RealEstateCore.Models;
using RealEstateService.ElsaWorkflow;

namespace RealEstateService.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RealEstateController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealEstateController"/> class.
        /// </summary>
        /// <param name="realEstateService">The service for managing real estate operations.</param>
        /// <param name="luceneEngine"></param>
        public RealEstateController(RealEstatesService realEstateService,
                                    ILuceneEngine<RealEstate> luceneEngine,
                                    //IWorkflowService workflowService,
                                    IWorkflowRunner workflowRunner)
        {
            _realEstateService = realEstateService ?? throw new ArgumentNullException(nameof(realEstateService));
            _luceneEngine = luceneEngine ?? throw new ArgumentNullException(nameof(luceneEngine));
            //_workflowService = workflowService;
            _workflowRunner = workflowRunner;
        }

        [HttpGet("similar-function")]
        public async Task<ActionResult<IEnumerable<RealEstate>>> GetSimilarTitlesFunction([FromQuery] string input)
        {
            ResponseModel<IEnumerable<RealEstate>> response = await _realEstateService.GetSimilarTitlesWithFunctionAsync(input);

            return Ok(response);
        }

        [HttpGet("similar-freetext")]
        public async Task<ActionResult<IEnumerable<RealEstate>>> GetSimilarTitlesFreetext([FromQuery] string input)
        {
            var response = await _realEstateService.GetSimilarTitlesWithFreeText(input);

            return Ok(response);
        }

        [HttpGet("similar-lucine")]
        public IActionResult SearchFuzzy(string field, string term, int pageNumber = 1, int pageSize = 10, double similarity = 0.5)
        {
            var response = _luceneEngine.SearchWithSimilarity(term, field, pageNumber, pageSize, similarity);

            return Ok(response);
        }

        /// <summary>
        /// Adds a new real estate property.
        /// </summary>
        /// <param name="realEstateViewModel">The view model containing the details of the real estate property.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> containing the ID of the newly added real estate property.</returns>
        [HttpPost("add")]
        public async Task<ResponseModel<int>> AddRealEstate([FromBody] RealEstateViewModel realEstateViewModel)
        {
            return await _realEstateService.AddRealEstateAsync(realEstateViewModel);
        }

        /// <summary>
        /// Archives an existing real estate property by its ID.
        /// </summary>
        /// <param name="id">The ID of the real estate property to archive.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> indicating the status of the operation.</returns>
        [HttpPut("archive/{id}")]
        public async Task<ResponseModel<int>> ArchiveRealEstate(int id)
        {
            return await _realEstateService.ArchiveRealEstateAsync(id);
        }

        /// <summary>
        /// Updates the timestamp of an existing real estate property.
        /// </summary>
        /// <param name="id">The ID of the real estate property to update.</param>
        /// <returns>A <see cref="ResponseModel{T}"/> containing the ID of the updated real estate property.</returns>
        [HttpPut("update-time/{id}")]
        public async Task<ResponseModel<int>> UpdateRealEstateTime(int id)
        {
            return await _realEstateService.UpdateRealEstateTimeAsync(id);
        }

        /// <summary>
        /// Retrieves a list of all real estate properties.
        /// </summary>
        /// <returns>A <see cref="ResponseModel{T}"/> containing a list of all real estate properties.</returns>
        [HttpGet("list")]
        public async Task<ResponseModel<IEnumerable<RealEstate>>> GetAllRealEstates()
        {
            return await _realEstateService.GetAllRealEstatesAsync();
        }

        //[HttpPost("start-workflow")]
        //public async Task<IActionResult> StartWorkflow()
        //{
        //    await _workflowService.StartWorkflowAsync();
        //    return Ok("Workflow started");
        //}

        [HttpGet("run-workflow")]
        public async Task Get()
        {
            await _workflowRunner.RunAsync(new WriteHttpResponse
            {
                Content = new("Hello ASP.NET world!")
            });
        }

        private readonly RealEstatesService _realEstateService;
        private readonly ILuceneEngine<RealEstate> _luceneEngine;
        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowRunner _workflowRunner;
    }
}
