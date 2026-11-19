using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.Security;
using MotoShopWeb.ViewModels;
namespace MotoShopWeb.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class CustomerController(ApplicationDbContext db):Controller {
 [HttpGet,AllowAnonymous] public IActionResult Login(string? returnUrl)=>View(new CustomerLogin{ReturnUrl=returnUrl});
 [HttpPost,AllowAnonymous,EnableRateLimiting("staff-login")]
 public async Task<IActionResult> Login(CustomerLogin form) {
  if(!ModelState.IsValid)return View(form);
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var input=form.Username.Trim();
  var account=await db.TaiKhoanKhachHangs.Include(x=>x.MaKhNavigation).SingleOrDefaultAsync(x=>x.TenDangNhap==input || x.MaKhNavigation.Email==input);
  var result=PasswordVerificationResult.Failed;
  if(account is not null && account.TrangThai) {
   try { result=new PasswordHasher<TaiKhoanKhachHang>().VerifyHashedPassword(account,account.MatKhauHash,form.Password); }
   catch(FormatException) { }
   catch(ArgumentException) { }
  }
  if(result==PasswordVerificationResult.Failed){ModelState.AddModelError("","Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");return View(form);}
  if(result==PasswordVerificationResult.SuccessRehashNeeded){account!.MatKhauHash=new PasswordHasher<TaiKhoanKhachHang>().HashPassword(account,form.Password);await db.SaveChangesAsync();}
  await SignIn(account!);
  return Url.IsLocalUrl(form.ReturnUrl)?LocalRedirect(form.ReturnUrl!):RedirectToAction("Index","Home");
 }
 [HttpGet,AllowAnonymous] public IActionResult Register()=>View(new CustomerRegister());
 [HttpPost,AllowAnonymous,EnableRateLimiting("staff-login")]
 public async Task<IActionResult> Register(CustomerRegister form) {
  if(!ModelState.IsValid)return View(form);
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  if(await db.TaiKhoanKhachHangs.AnyAsync(x=>x.TenDangNhap==form.Username.Trim())) {
   ModelState.AddModelError(nameof(form.Username),"Tên đăng nhập này đã được sử dụng.");
  }
  if(await db.KhachHangs.AnyAsync(x=>x.Email==form.Email.Trim())) {
   ModelState.AddModelError(nameof(form.Email),"Địa chỉ email này đã được đăng ký.");
  }
  if(!ModelState.IsValid)return View(form);
  var account=new TaiKhoanKhachHang{TenDangNhap=form.Username.Trim(),TrangThai=true,NgayTao=DateTime.Now,
   MaKhNavigation=new KhachHang{HoTen=form.Name.Trim(),Email=form.Email.Trim(),SoDienThoai=string.IsNullOrWhiteSpace(form.Phone)?null:form.Phone.Trim(),NgayDangKy=DateTime.Now}};
  account.MatKhauHash=new PasswordHasher<TaiKhoanKhachHang>().HashPassword(account,form.Password);
  db.TaiKhoanKhachHangs.Add(account);
  try {await db.SaveChangesAsync();}
  catch(DbUpdateException ex) when(ex.InnerException is SqlException {Number:2601 or 2627}) {
   ModelState.AddModelError("","Tên đăng nhập hoặc email đã được sử dụng.");return View(form);
  }
  await SignIn(account);TempData["Success"]="Tài khoản đã sẵn sàng. Cùng tìm chiếc xe cho hành trình mới.";
  return RedirectToAction("Index","Home");
 }
 private Task SignIn(TaiKhoanKhachHang account)=>HttpContext.SignInAsync("Customer",new ClaimsPrincipal(new ClaimsIdentity([
  new Claim(ClaimTypes.NameIdentifier,account.MaKh.ToString()),new Claim(ClaimTypes.Name,account.MaKhNavigation.HoTen),
  new Claim("stamp",CustomerCookieEvents.Stamp(account.MatKhauHash))],"Customer")));
 [HttpGet,Authorize(AuthenticationSchemes="Customer")]
 public async Task<IActionResult> Profile() {
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  var account=await db.TaiKhoanKhachHangs.Include(x=>x.MaKhNavigation).SingleOrDefaultAsync(x=>x.MaKh==id);
  if(account is null)return RedirectToAction(nameof(Login));
  var kh=account.MaKhNavigation;
  return View(new CustomerProfileVm{Username=account.TenDangNhap,Name=kh.HoTen,Email=kh.Email,Phone=kh.SoDienThoai,Address=kh.DiaChi,BirthDate=kh.NgaySinh});
 }
 [HttpPost,Authorize(AuthenticationSchemes="Customer")]
 public async Task<IActionResult> Profile(CustomerProfileVm form) {
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  var account=await db.TaiKhoanKhachHangs.Include(x=>x.MaKhNavigation).SingleOrDefaultAsync(x=>x.MaKh==id);
  if(account is null)return RedirectToAction(nameof(Login));
  form.Username=account.TenDangNhap;
  if(await db.KhachHangs.AnyAsync(x=>x.Email==form.Email.Trim() && x.MaKh!=id))
   ModelState.AddModelError(nameof(form.Email),"Địa chỉ email này đã được tài khoản khác sử dụng.");
  if(!string.IsNullOrEmpty(form.NewPassword)) {
   if(string.IsNullOrEmpty(form.CurrentPassword))
    ModelState.AddModelError(nameof(form.CurrentPassword),"Nhập mật khẩu hiện tại để đổi mật khẩu mới.");
   else {
    var verify=new PasswordHasher<TaiKhoanKhachHang>().VerifyHashedPassword(account,account.MatKhauHash,form.CurrentPassword);
    if(verify==PasswordVerificationResult.Failed)
     ModelState.AddModelError(nameof(form.CurrentPassword),"Mật khẩu hiện tại chưa đúng.");
   }
  }
  if(!ModelState.IsValid)return View(form);
  var kh=account.MaKhNavigation;
  kh.HoTen=form.Name.Trim();
  kh.Email=form.Email.Trim();
  kh.SoDienThoai=string.IsNullOrWhiteSpace(form.Phone)?null:form.Phone.Trim();
  kh.DiaChi=string.IsNullOrWhiteSpace(form.Address)?null:form.Address.Trim();
  kh.NgaySinh=form.BirthDate;
  if(!string.IsNullOrEmpty(form.NewPassword))
   account.MatKhauHash=new PasswordHasher<TaiKhoanKhachHang>().HashPassword(account,form.NewPassword);
  await db.SaveChangesAsync();
  await SignIn(account);
  TempData["Success"]="Đã cập nhật hồ sơ cá nhân thành công.";
  return RedirectToAction(nameof(Profile));
 }
 [HttpGet,Authorize(AuthenticationSchemes="Customer")] public IActionResult Logout()=>View();
 [HttpPost,ActionName("Logout"),Authorize(AuthenticationSchemes="Customer")]
 public async Task<IActionResult> LogoutConfirmed(){await HttpContext.SignOutAsync("Customer");HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
}
