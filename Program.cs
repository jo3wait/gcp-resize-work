using Google.Cloud.Storage.V1;
using ResizeWork.Services;
using ResizeWorker.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- DI ----------
if (builder.Environment.IsDevelopment())
{
    // �����G���ξ���
    builder.Services.AddSingleton<IStorage, FakeStorage>();
}
else
{
    // Cloud Run�G�ϥ� ADC
    builder.Services.AddSingleton<IStorage>(sp =>
        new GcsStorage(StorageClient.Create()));
}

// �̻ݨD���U�A��
//builder.Services.AddSingleton(StorageClient.Create());
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<SqlService>();

// MVC
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();          // �L MapCloudEvents ��i���`�� POST JSON
app.Run();
