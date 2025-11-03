namespace Library.Data.PostgreSql.Entities;

/// <summary>
/// Сущность автора
/// </summary>
public class AuthorEntity
{
    /// <summary>Идентификатор автора</summary>
    public Guid Id { get; set; }

    /// <summary>ФИО автора</summary>
    public Guid FullName { get; set; }
}