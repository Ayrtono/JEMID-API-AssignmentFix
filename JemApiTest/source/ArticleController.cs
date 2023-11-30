using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

[ApiController]
[Route("api/articles")]
public class ArticleController : ControllerBase {
    private readonly MyDbContext _dbContext;

    public ArticleController(MyDbContext dbContext) {
        _dbContext = dbContext;
    }

    // GET api/articles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Article>>> Get() {
        var articles = await _dbContext.Articles.ToListAsync();
        return Ok(articles);
    }

    // GET api/articles/{code}
    [HttpGet("{code}")]
    public async Task<ActionResult<Article>> Get(string code) {
        var article = await _dbContext.Articles.FirstOrDefaultAsync(a => a.Code == code);
        if (article == null) {
            return NotFound();
        }

        return Ok(article);
    }

    // POST api/articles
    [HttpPost]
    public async Task<ActionResult<Article>> Post([FromBody] Article article) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        // Check if an article with the same code already exists
        if (await _dbContext.Articles.AnyAsync(a => a.Code == article.Code)) {
            return Conflict("An article with the same code already exists.");
        }

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();

        return CreatedAtRoute(nameof(Get), new { code = article.Code }, article);
    }

    // PUT api/articles/{code}
    [HttpPut("{code}")]
    public async Task<ActionResult<Article>> Put(string code, [FromBody] Article updatedArticle) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var existingArticle = await _dbContext.Articles.FirstOrDefaultAsync(a => a.Code == code);
        if (existingArticle == null) {
            return NotFound();
        }

        // Update properties
        existingArticle.Name = updatedArticle.Name;
        existingArticle.PotSize = updatedArticle.PotSize;
        existingArticle.PlantHeight = updatedArticle.PlantHeight;
        existingArticle.ProductGroup = updatedArticle.ProductGroup;
        existingArticle.Colour = updatedArticle.Colour;

        await _dbContext.SaveChangesAsync();

        return Ok(existingArticle);
    }

    // DELETE api/articles/{code}
    [HttpDelete("{code}")]
    public async Task<ActionResult> Delete(string code) {
        var articleToRemove = await _dbContext.Articles.FirstOrDefaultAsync(a => a.Code == code);
        if (articleToRemove == null) {
            return NotFound();
        }

        _dbContext.Articles.Remove(articleToRemove);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    // GET api/articles/search?name={name}&minPotSize={minPotSize}&maxPotSize={maxPotSize}&colour={colour}&productGroup={productGroup}
    [HttpGet("search")]
    public ActionResult<IEnumerable<Article>> Search(
        [FromQuery] string name,
        [FromQuery] int? minPotSize,
        [FromQuery] int? maxPotSize,
        [FromQuery] string colour,
        [FromQuery] string productGroup,
        [FromQuery] string sortField = "Name",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10) {

            // Maximum page size
            const int maxPageSize = 100;

            // Check for pageNumber against abuse and for efficiency
            if (pageNumber < 1) {
                return BadRequest("Invalid page number. Must be greater than or equal to 1.");
            }
            
            // Check pageSize against abuse and for efficiency
            if (pageSize < 1 || pageSize > maxPageSize) {
                return BadRequest($"Invalid page size. Must be between 1 and {maxPageSize}.");
            }

            // Ensure that the minimum pot size is not negative, if the field has any value within the request.
            if (minPotSize.HasValue && minPotSize < 0) {
                return BadRequest("Invalid minimum pot size. Must be greater than or equal to 0.");
            }

            // Ensure that the maximum pot size is not negative, if the field has any value within the request.
            if (maxPotSize.HasValue && maxPotSize < 0) {
                return BadRequest("Invalid maximum pot size. Must be greater than or equal to 0.");
            }
            
            // Ensure that the maximum pot size is not smaller or equal to the minimum pot size if both have any value within the request.
            if (minPotSize.HasValue && maxPotSize.HasValue && minPotSize > maxPotSize) {
                return BadRequest("Invalid combination of minimum and maximum pot size.");
            }

            var query = _dbContext.Articles.AsQueryable();
            
            // Apply filters based on the provided parameters
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(a => a.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }
                
            if (minPotSize.HasValue) {
                query = query.Where(a => a.PotSize >= minPotSize.Value);
            }
                
            if (maxPotSize.HasValue) {
                query = query.Where(a => a.PotSize <= maxPotSize.Value);
            }

            if (!string.IsNullOrEmpty(colour)) {
                query = query.Where(a => a.Colour == colour);
            }

            if (!string.IsNullOrEmpty(productGroup)) {
                query = query.Where(a => a.ProductGroup == productGroup);
            }
            
            var result = query.ToList();
            return Ok(result);
        }

    private IQueryable<Article> ApplySorting(IQueryable<Article> query, string sortField, string sortOrder) {
        // Validate sortField to prevent SQL injection
        var validSortFields = new[] { "Name", "PotSize", "PlantHeight", "Colour", "ProductGroup" };
        if (!validSortFields.Contains(sortField)) {
            sortField = "Name"; // Default to Name if an invalid field is provided
        }

        // Apply sorting based on sortOrder
        query = sortOrder.ToLower() == "desc" ? query.OrderByDescendingDynamic(sortField) : query.OrderByDynamic(sortField);
        return query;
    }
}
