var builder = WebApplication.CreateBuilder(args);

// ��������� ������� CORS
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

// �������� ������� �������������� � �����������
builder.Services.AddAuthentication(); // ����� �������� ���������� ����� �������������� ��� �������������
builder.Services.AddAuthorization();  // ����������� ������� �����������
builder.Services.AddControllers();

var app = builder.Build();
// ���������� �������� CORS ����� ������� middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// �������� ����� UseAuthentication ����� UseAuthorization, ���� ������������ ��������������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
