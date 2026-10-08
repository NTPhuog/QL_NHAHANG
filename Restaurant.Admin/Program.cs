using RestaurantManagement.Hosting;
var app = RestaurantHost.Create(args, "Admin", 5099);
if (await RestaurantHost.InitializeAsync(app, args, "Admin")) return;
await app.RunAsync();
