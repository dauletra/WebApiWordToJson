var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Добавьте сервисы аутентификации и авторизации
builder.Services.AddAuthentication(); // Можно добавить конкретную схему аутентификации при необходимости
builder.Services.AddAuthorization();  // Регистрация сервиса авторизации
builder.Services.AddControllers();

var app = builder.Build();
// Используем политику CORS перед другими middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Добавьте вызов UseAuthentication перед UseAuthorization, если используется аутентификация
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
