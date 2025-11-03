using Library.Domain.Abstractions.Services;
using Library.Domain.Models;
using Library.SharedKernel.Dto;
using Library.SharedKernel.Enums;
using Library.SharedKernel.Utils;

namespace Library.Domain.Services;

/// <inheritdoc cref="IBookService"/>
public sealed class BookService : IBookService
{
    private readonly Dictionary<Guid, Book> _books;

    public BookService()
    {
        _books = [];
    }
    
    /// <inheritdoc cref="IBookService.CreateBook"/>
    public async Task<Result<Guid>> CreateBook(CreateBookDto createBook)
    {
        await Task.Delay(100);

        try
        {
            var bookId = Guid.NewGuid();
            
            _books.Add(bookId, new Book()
            {
                Title = createBook.Title,
                Authors = createBook.Authors,
                Category = createBook.Category,
                Description = createBook.Description,
                Year = createBook.Year
            });
            
            return Result<Guid>.Success(bookId);
        }
        catch (Exception exception)
        {
            return Result<Guid>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.GetBookById"/>
    public async Task<Result<GetBookDto>> GetBookById(Guid bookId)
    {
        try
        {
            await Task.Delay(100);

            if (!_books.TryGetValue(bookId, out var book))
            {
                return Result<GetBookDto>.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }
            
            return Result<GetBookDto>.Success(new GetBookDto
            {
                Id = bookId,
                Title = book.Title,
                Authors = book.Authors,
                Category = book.Category,
                Description = book.Description,
                Year = book.Year
            });
        }
        catch (Exception exception)
        {
            return Result<GetBookDto>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.GetBooks"/>
    public async Task<Result<ICollection<GetBookDto>>> GetBooks()
    {
        try
        {
            await Task.Delay(100);

            var bookDtos = _books.Keys.Select(bookId => new GetBookDto
                {
                    Id = bookId,
                    Title = _books[bookId].Title,
                    Authors = _books[bookId].Authors,
                    Category = _books[bookId].Category,
                    Description = _books[bookId].Description,
                    Year = _books[bookId].Year
                })
                .ToList();

            return Result<ICollection<GetBookDto>>.Success(bookDtos);
        }
        catch (Exception exception)
        {
            return Result<ICollection<GetBookDto>>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.UpdateBook"/>
    public async Task<Result> UpdateBook(Guid id, UpdateBookDto updateBook)
    {
        try
        {
            await Task.Delay(100);
            
            if (!_books.TryGetValue(id, out var book))
            {
                return Result.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }

            book.Title = updateBook.Title;
            book.Authors = updateBook.Authors;
            book.Description = updateBook.Description;
            book.Year = updateBook.Year;

            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.DeleteBook"/>
    public async Task<Result> DeleteBook(Guid id)
    {
        try
        {
            var getResult = await GetBookById(id);

            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            _books.Remove(id);
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }
}