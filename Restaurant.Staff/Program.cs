using RestaurantManagement.Hosting;
var app = RestaurantHost.Create(args, "NhanVien", 5098);
if (await RestaurantHost.InitializeAsync(app, args, "NhanVien")) return;
await app.RunAsync();
