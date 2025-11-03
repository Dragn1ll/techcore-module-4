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
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var authors = new List<AuthorEntity>();
            foreach (var fullName in createBook.Authors)
            {
                var author = await _context.Authors.FirstOrDefaultAsync(a => a.FullName == fullName);
                if (author == null)
                {
                    author = new AuthorEntity { FullName = fullName };
                    _context.Authors.Add(author);
                }
                authors.Add(author);
            }

            var book = new BookEntity
            {
                Title = createBook.Title,
                Category = createBook.Category,
                Authors = authors,
                Description = createBook.Description,
                Year = createBook.Year
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Result<Guid>.Success(book.Id);
        }
        catch (Exception exception)
        {
            // Откатываем транзакцию и пробрасываем ошибку
            await transaction.RollbackAsync();
            return Result<Guid>.Failure(new Error(ErrorType.ServerError, exception.Message));
        }
    }

    /// <inheritdoc cref="IBookService.GetByIdAsync"/>
    public async Task<Result<GetBookDto>> GetByIdAsync(Guid bookId)
    {
        try
        {
            var entity = await _context.Books.Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (entity == null)
            {
                return Result<GetBookDto>.Failure(new Error(ErrorType.NotFound, "Книга не найдена"));
            }
            
            return Result<GetBookDto>.Success(new GetBookDto
            {
                Id = bookId,
                Title = entity.Title,
                Category = entity.Category,
                Authors = entity.Authors.Select(a => a.FullName).ToList(),
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
                .Include(e => e.Authors)
                .Select(e => new GetBookDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Category = e.Category,
                    Authors = e.Authors.Select(a => a.FullName).ToList(),
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