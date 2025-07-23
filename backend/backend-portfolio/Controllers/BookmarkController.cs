using backend_portfolio.DTO.Request;
using backend_portfolio.DTO.Response;
using backend_portfolio.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookmarkController : ControllerBase
    {
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly ILogger<BookmarkController> _logger;

        public BookmarkController(
            IBookmarkRepository bookmarkRepository,
            ILogger<BookmarkController> logger)
        {
            _bookmarkRepository = bookmarkRepository;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookmarks(Guid userId)
        {
            try
            {
                var bookmarks = await _bookmarkRepository.GetBookmarksByUserIdAsync(userId);
                var response = bookmarks.Select(b => new BookmarkResponse
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PortfolioId = b.PortfolioId,
                    CollectionName = b.CollectionName,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookmarks for user: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleBookmark([FromBody] BookmarkToggleRequest request)
        {
            try
            {
                var bookmarkExists = await _bookmarkRepository.BookmarkExistsAsync(request.UserId, request.PortfolioId);

                if (bookmarkExists)
                {
                    var result = await _bookmarkRepository.DeleteBookmarkByUserAndPortfolioAsync(request.UserId, request.PortfolioId);
                    if (!result)
                        return BadRequest("Failed to remove bookmark");

                    return Ok(new { isBookmarked = false, message = "Bookmark removed successfully" });
                }
                else
                {
                    var createRequest = new BookmarkCreateRequest
                    {
                        UserId = request.UserId,
                        PortfolioId = request.PortfolioId,
                        CollectionName = request.CollectionName,
                        Notes = request.Notes
                    };

                    var bookmark = await _bookmarkRepository.CreateBookmarkAsync(createRequest);
                    var response = new BookmarkResponse
                    {
                        Id = bookmark.Id,
                        UserId = bookmark.UserId,
                        PortfolioId = bookmark.PortfolioId,
                        CollectionName = bookmark.CollectionName,
                        Notes = bookmark.Notes,
                        CreatedAt = bookmark.CreatedAt
                    };

                    return Ok(new { isBookmarked = true, bookmark = response, message = "Bookmark added successfully" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling bookmark for user: {UserId}, portfolio: {PortfolioId}", 
                    request.UserId, request.PortfolioId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check/{userId}/{portfolioId}")]
        public async Task<IActionResult> CheckBookmark(Guid userId, Guid portfolioId)
        {
            try
            {
                var exists = await _bookmarkRepository.BookmarkExistsAsync(userId, portfolioId);
                return Ok(new { isBookmarked = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking bookmark for user: {UserId}, portfolio: {PortfolioId}", 
                    userId, portfolioId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookmark(Guid id)
        {
            try
            {
                var result = await _bookmarkRepository.DeleteBookmarkAsync(id);
                if (!result)
                    return NotFound($"Bookmark with ID {id} not found.");

                return Ok(new { message = "Bookmark deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting bookmark: {BookmarkId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 