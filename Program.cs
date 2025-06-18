using Google.Cloud.Storage.V1;
using ResizeWork.Services;
using ResizeWorker.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- DI ----------
if (builder.Environment.IsDevelopment())
{
    // 本機：不用憑證
    builder.Services.AddSingleton<IStorage, FakeStorage>();
}
else
{
    // Cloud Run：使用 ADC
    builder.Services.AddSingleton<IStorage>(sp =>
        new GcsStorage(StorageClient.Create()));
}

// 依需求註冊服務
//builder.Services.AddSingleton(StorageClient.Create());
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<SqlService>();

// MVC
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();          // 無 MapCloudEvents 亦可正常收 POST JSON
app.Run();
