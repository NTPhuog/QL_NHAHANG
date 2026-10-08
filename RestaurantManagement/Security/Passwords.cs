namespace RestaurantManagement.Security;
public static class Passwords
{
 private static readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> Hasher=new();
 public static string Hash(string value)=>Hasher.HashPassword(new object(),value);
 public static bool Verify(string stored,string value) {
  if(string.IsNullOrEmpty(stored)||string.IsNullOrEmpty(value))return false;
  try { if(Hasher.VerifyHashedPassword(new object(),stored,value)!=Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)return true; } catch(FormatException){} catch(ArgumentException){}
  // Compatibility with existing records; upgraded immediately after successful login.
  return !stored.StartsWith("AQAAAA") && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(stored),System.Text.Encoding.UTF8.GetBytes(value));
 }
}

