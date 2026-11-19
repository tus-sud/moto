using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
namespace MotoShopWeb.Security;
public class CustomerCookieEvents(ApplicationDbContext db):CookieAuthenticationEvents {
 public static string Stamp(string hash)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
 public override async Task ValidatePrincipal(CookieValidatePrincipalContext context) {
  var id=context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
  var account=int.TryParse(id,out var key)?await db.TaiKhoanKhachHangs.AsNoTracking().SingleOrDefaultAsync(x=>x.MaKh==key):null;
  if(account is null || !account.TrangThai || context.Principal?.FindFirstValue("stamp")!=Stamp(account.MatKhauHash)) {
   context.RejectPrincipal(); await context.HttpContext.SignOutAsync("Customer");
  }
 }
}
