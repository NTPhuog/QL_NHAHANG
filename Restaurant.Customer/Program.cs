using RestaurantManagement.Hosting;
var app = RestaurantHost.Create(args, "KhachHang", 5097);
if (await RestaurantHost.InitializeAsync(app, args, "KhachHang")) return;
await app.RunAsync();
