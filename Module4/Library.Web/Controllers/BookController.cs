using Library.Contracts.Books.Request;
using Library.Contracts.Books.Response;
using Library.Domain.Abstractions.Services;
using Library.Web.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BookController (IBookService bookService) : Controller
{
    [HttpPost]
    public async Task<ActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        var result = await bookService.CreateAsync(request.ToCreateBookDto());
        
        return result.IsSuccess 
            ? Ok(new CreateBookResponse(result.Value)) 
            : Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
    }

    [HttpGet]
    public async Task<ActionResult> GetBooks()
    {
        var result = await bookService.GetAllAsync();
            
        return result.IsSuccess 
            ? Ok(result.Value!.Select(gbd => gbd.ToGetBookResponse()).ToList()) 
            : Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetBook([FromRoute] Guid id)
    {
        try
        {
            var result = await bookService.GetByIdAsync(id);
        
            return result.IsSuccess 
                ? Ok(result.Value!.ToGetBookResponse())
                : Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateBook([FromRoute] Guid id, [FromBody] UpdateBookRequest request)
    {
        var result = await bookService.UpdateAsync(id, request.ToUpdateBookDto());
            
        return result.IsSuccess 
            ? Ok()
            : Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteBook([FromRoute] Guid id)
    {
        var result = await bookService.DeleteBook(id);
        
        return result.IsSuccess 
            ? Ok() 
            : Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
    }
}