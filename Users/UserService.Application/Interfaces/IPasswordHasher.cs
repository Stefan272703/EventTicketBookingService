namespace UserService.Application.Interfaces
{
    public interface IPasswordHasher
    {
        // Хеширование пароля
        public string Hash(string password);

        // Проверка на соответствие пароля хешу
        public bool Verify(string password, string passwordHash);
    }
}
