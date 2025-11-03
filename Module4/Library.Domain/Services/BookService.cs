using Library.Data.PostgreSql;
using Library.Data.PostgreSql.Entities;
using Library.Domain.Abstractions.Services;
using Library.Domain.Models;
using Library.SharedKernel.Dto;
using Library.SharedKernel.Enums;
using Library.SharedKernel.Utils;
using Microsoft.EntityFrameworkCore;

namespace Library.Domain.Services;

/// <inheritdoc cref="IBookService"/>
public sealed class BookService : IBookService
{
    private readonly BookContext _context;

    public BookService(BookContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc cref="IBookService.CreateAsync"/>
    public async Task<Result<Guid>> CreateAsync(CreateBookDto createBook)
    {
        try
        {
            var entity = new BookEntity
            {
                Title = createBook.Title,
                Category = createBook.Category,
                Description = createBook.Description,
                Year = createBook.Year
            };
            
            _context.Books.Add(entity);
            await _context.SaveChangesAsync();
            
            return Result<Guid>.Success(entity.Id);
        }
        catch (Exception exception)
        {
            return Result<Guid>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.GetByIdAsync"/>
    public async Task<Result<GetBookDto>> GetByIdAsync(Guid bookId)
    {
        try
        {
            var entity = await _context.Books.FindAsync(bookId);

            if (entity == null)
            {
                return Result<GetBookDto>.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }
            
            return Result<GetBookDto>.Success(new GetBookDto
            {
                Id = bookId,
                Title = entity.Title,
                Category = entity.Category,
                Description = entity.Description,
                Year = entity.Year
            });
        }
        catch (Exception exception)
        {
            return Result<GetBookDto>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.GetAllAsync"/>
    public async Task<Result<ICollection<GetBookDto>>> GetAllAsync()
    {
        try
        {
            var bookDtos = await _context.Books.AsNoTracking()
                .Select(e => new GetBookDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Category = e.Category,
                    Description = e.Description,
                    Year = e.Year
                })
                .ToListAsync();

            return Result<ICollection<GetBookDto>>.Success(bookDtos);
        }
        catch (Exception exception)
        {
            return Result<ICollection<GetBookDto>>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.UpdateAsync"/>
    public async Task<Result> UpdateAsync(Guid id, UpdateBookDto updateBook)
    {
        try
        {
            var entity = await _context.Books.FindAsync(id);
            
            if (entity == null)
            {
                return Result.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }

            entity.Title = updateBook.Title;
            entity.Description = updateBook.Description;
            entity.Year = updateBook.Year;
            
            _context.Books.Update(entity);
            await _context.SaveChangesAsync();

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
            var entity = await _context.Books.FindAsync(id);

            if (entity == null)
            {
                return Result.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }

            _context.Books.Remove(entity);
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }
}