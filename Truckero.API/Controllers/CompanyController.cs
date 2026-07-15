using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Constants;
using Truckero.Core.DTOs;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CompanyController : ControllerBase {
    private readonly ICompanyRepository _companyRepository;

    public CompanyController(ICompanyRepository companyRepository) {
        _companyRepository = companyRepository;
    }

    // GET: /api/Company/metadata
    [HttpGet("metadata")]
    public async Task<ActionResult<CompanyResponse>> GetMetadata() {
        try {
            var businessTypes = await _companyRepository.GetBusinessTypesAsync();
            var industries = await _companyRepository.GetIndustriesAsync();

            var response = new CompanyResponse {
                Success = true,
                BusinessTypes = businessTypes.Select(bt => new BusinessTypeRequest {
                    Id = bt.Id,
                    Name = bt.Name,
                    Description = bt.Description,
                    IsActive = bt.IsActive
                }).ToList(),
                IndustryTypes = industries.Select(i => new IndustryTypeRequest {
                    Id = i.Id,
                    Name = i.Name
                }).ToList()
            };

            return Ok(response);
        } catch (Exception ex) {
            Console.Write("");
        }
        return null;
    }

    // GET: /api/Company/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id) {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null) {
            return NotFound(new CompanyResponse {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.NotFound,
                Message = $"Company with id {id} not found"
            });
        }

        var dto = MapToRequest(company);

        return Ok(new CompanyResponse {
            Success = true,
            CompanyRequests = new List<CompanyRequest> { dto }
        });
    }

    // POST: /api/Company
    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CompanyRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new CompanyResponse {
                Success = false,
                Message = "Invalid company payload."
            });
        }

        var company = new Company {
            Id = Guid.NewGuid(),
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            BusinessTypeId = request.BusinessTypeId,
            Industries = request.Industries
        };

        await _companyRepository.AddAsync(company);
        await _companyRepository.SaveChangesAsync();

        var dto = MapToRequest(company);

        var response = new CompanyResponse {
            Success = true,
            CompanyRequests = new List<CompanyRequest> { dto }
        };

        return CreatedAtAction(nameof(GetById), new { id = company.Id }, response);
    }

    // PUT: /api/Company/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> Update(Guid id, [FromBody] CompanyRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new CompanyResponse {
                Success = false,
                Message = "Invalid company payload."
            });
        }

        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null) {
            return NotFound(new CompanyResponse {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.NotFound,
                Message = $"Company with id {id} not found"
            });
        }

        company.Name = request.Name;
        company.LogoUrl = request.LogoUrl;
        company.BusinessTypeId = request.BusinessTypeId;
        company.Industries = request.Industries;

        await _companyRepository.UpdateAsync(company);
        await _companyRepository.SaveChangesAsync();

        var dto = MapToRequest(company);

        return Ok(new CompanyResponse {
            Success = true,
            CompanyRequests = new List<CompanyRequest> { dto }
        });
    }

    private static CompanyRequest MapToRequest(Company company) => new() {
        Id = company.Id,
        Name = company.Name,
        LogoUrl = company.LogoUrl,
        BusinessTypeId = company.BusinessTypeId,
        Industries = company.Industries
    };

    public async Task<CompanyResponse> LoadMetadataAsync()
    {
        // TEMP: bypass API call to isolate client-side behavior
        await Task.CompletedTask;

        return new CompanyResponse
        {
            Success = true,
            BusinessTypes =
            [
                new BusinessTypeRequest
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000001001"),
                    Name = "Appliance Store",
                    Description = "Large & small appliance retailers",
                    IsActive = true
                }
            ],
            IndustryTypes =
            [
                new IndustryTypeRequest
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000002001"),
                    Name = "Furniture",
                }
            ]
        };
    }
}
