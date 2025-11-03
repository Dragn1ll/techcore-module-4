using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql;

public class BookContext(DbContextOptions<BookContext> options) : DbContext(options)
{
    
}