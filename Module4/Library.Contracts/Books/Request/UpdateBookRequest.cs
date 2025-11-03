namespace Library.Contracts.Books.Request;

/// <summary>
/// Запрос на обновление книги
/// </summary>
/// <param name="Title">Название книги</param>
/// <param name="Authors">Авторы книги</param>
/// <param name="Description">Описание книги</param>
/// <param name="Year">Год издания</param>
public sealed record UpdateBookRequest(string Title, IReadOnlyList<string> Authors, string Description, int Year);